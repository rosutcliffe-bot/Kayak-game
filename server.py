#!/usr/bin/env python3
"""
Minimal HTTP server for testing Unity WebGL builds locally.

Usage:
    python3 server.py [port]

Defaults to port 8000.  Serves the WebGLBuild/ directory (or the current
directory if WebGLBuild/ does not exist).

Unity WebGL builds use gzip-compressed files (.gz).  This server adds the
required Content-Encoding header so browsers decompress them correctly.
"""

import http.server
import os
import sys

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 8000

# Serve from WebGLBuild/ if it exists next to this script, otherwise cwd
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
BUILD_DIR = os.path.join(SCRIPT_DIR, "WebGLBuild")
SERVE_DIR = BUILD_DIR if os.path.isdir(BUILD_DIR) else os.getcwd()


class GzipAwareHandler(http.server.SimpleHTTPRequestHandler):
    """Adds Content-Encoding: gzip for .gz files and sets correct MIME types."""

    EXTRA_TYPES = {
        ".js": "application/javascript",
        ".wasm": "application/wasm",
        ".data": "application/octet-stream",
        ".json": "application/json",
        ".unityweb": "application/octet-stream",
    }

    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=SERVE_DIR, **kwargs)

    def end_headers(self):
        if self.path.endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")

            # Guess the inner MIME type from the double extension
            base = self.path[: -len(".gz")]
            _, ext = os.path.splitext(base)
            mime = self.EXTRA_TYPES.get(ext)
            if mime:
                # Override the default Content-Type that was already set
                self.send_header("Content-Type", mime)

        # Required for Unity WebGL multi-threaded builds that use
        # SharedArrayBuffer; these headers enable cross-origin isolation.
        self.send_header("Cross-Origin-Opener-Policy", "same-origin")
        self.send_header("Cross-Origin-Embedder-Policy", "require-corp")
        super().end_headers()


if __name__ == "__main__":
    print(f"Serving {SERVE_DIR}")
    print(f"Open http://localhost:{PORT} in your browser")
    with http.server.HTTPServer(("", PORT), GzipAwareHandler) as httpd:
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nServer stopped.")
