# WebGL Deployment Guide – Kayak Simulator

This guide walks you through **building**, **testing locally**, and **deploying**
the Kayak Simulator as a browser game.

---

## Prerequisites

| Requirement | Details |
|---|---|
| Unity 2022.3 LTS+ | With **WebGL Build Support** module installed via Unity Hub |
| Scenes configured | `MainMenu` (index 0) and `GameScene` (index 1) in Build Settings |
| URP asset assigned | **Edit → Project Settings → Graphics** |
| Modern browser | Chrome, Firefox, or Edge (Safari has limited WebGL 2 support) |

> **Tip:** In Unity Hub, click the gear icon next to your Unity version →
> **Add Modules** → check **WebGL Build Support** if it is not already installed.

---

## 1 — Build for WebGL

### Option A: One-click menu build

1. Open the project in Unity.
2. Click **Kayak Simulator → Build WebGL** in the top menu bar.
3. The build output is placed in a `WebGLBuild/` folder at the project root.

### Option B: Command-line / CI build

```bash
# From the repository root:
Unity -batchmode -projectPath . \
      -executeMethod KayakSimulator.Editor.WebGLBuilder.Build \
      -quit -logFile build.log
```

Check `build.log` if anything goes wrong.

### Option C: Manual build via Build Settings

1. **File → Build Settings** → select **WebGL** → click **Switch Platform**.
2. **Player Settings → Resolution and Presentation → WebGL Template** →
   select **KayakSimulator**.
3. Click **Build** → choose an output folder.

After any method the output folder will look like this:

```
WebGLBuild/
├── Build/
│   ├── WebGLBuild.data.gz
│   ├── WebGLBuild.framework.js.gz
│   ├── WebGLBuild.loader.js
│   └── WebGLBuild.wasm.gz
├── TemplateData/   (if any)
└── index.html      ← the custom Kayak Simulator web page
```

---

## 2 — Test Locally

WebGL builds **must** be served over HTTP — you cannot just open `index.html`
as a file. Use any of these quick methods:

### Python (pre-installed on macOS / Linux)

```bash
cd WebGLBuild
python3 -m http.server 8000
```

### Included helper script

```bash
python3 server.py            # from the repository root
# or
python3 server.py 3000       # use a custom port
```

### Node.js

```bash
npx serve WebGLBuild
```

Then open **http://localhost:8000** (or whichever port) in your browser.

---

## 3 — Deploy to the Web

Upload the **entire contents** of `WebGLBuild/` to a static web host.

### GitHub Pages (free)

1. Create a branch (e.g. `gh-pages`) or use `/docs` on `main`.
2. Copy the build output into that location.
3. Push and enable Pages in **Settings → Pages**.
4. Your game is live at `https://<user>.github.io/<repo>/`.

### Netlify (free tier)

1. Go to [app.netlify.com](https://app.netlify.com) and create a new site.
2. Drag-and-drop the `WebGLBuild/` folder onto the deploy area.
3. Done — Netlify gives you a URL instantly.

### itch.io (free, game-focused)

1. Create an account at [itch.io](https://itch.io).
2. Create a new project → set **Kind of project** to **HTML**.
3. Zip the contents of `WebGLBuild/` and upload the zip.
4. Check **This file will be played in the browser**.
5. Set the viewport size to **960 × 600** (or larger).

### Self-hosted (Nginx / Apache / S3)

Upload the folder to your web root. Make sure your server sends the correct
MIME types and supports gzip-compressed `.gz` files:

**Nginx example:**

```nginx
location /kayak/ {
    # Serve pre-compressed gzip files
    gzip_static on;

    # MIME types for Unity WebGL
    types {
        application/javascript  js;
        application/wasm        wasm;
        application/octet-stream data;
    }
}
```

---

## 4 — Web Interface Features

The custom WebGL template (`Assets/WebGLTemplates/KayakSimulator/index.html`)
provides the following out of the box:

| Feature | Description |
|---|---|
| **Loading screen** | Animated progress bar with percentage while the game downloads |
| **Responsive canvas** | Game fills the full browser window and resizes automatically |
| **Fullscreen button** | ⛶ toggle in the top-right toolbar |
| **Mute button** | 🔊 toggle in the toolbar — calls `AudioListener.volume` via the bridge |
| **Mobile touch controls** | Automatically shown on phones/tablets — left paddle, right paddle, and steer zones |
| **WebGL check** | Shows a friendly error if the browser lacks WebGL support |

---

## 5 — Troubleshooting

| Problem | Solution |
|---|---|
| **Blank / black screen** | Open the browser console (F12) and look for errors. Most common: wrong MIME types on the server. |
| **"Unable to parse Build/…"** | The `.gz` files are not being served correctly. Ensure `Content-Encoding: gzip` is set, or re-build with compression disabled (**Player Settings → Publishing Settings → Compression Format → Disabled**). |
| **Game loads but input doesn't work** | Click on the game canvas to give it focus. On mobile, touch controls appear automatically. |
| **Audio doesn't play** | Browsers require a user gesture before playing audio. Click the canvas or press a key first. |
| **Low FPS on mobile** | Reduce quality in the in-game Settings panel, or build with lower texture resolution. |
| **"Out of memory"** | Increase the WebGL memory size in **Player Settings → Publishing Settings → Memory Size** (default 256 MB). |

---

## 6 — Scene Setup Reminder

If you have not created the Unity scenes yet, follow
[`SceneSetupGuide.md`](SceneSetupGuide.md) first. The WebGL build requires at
least the two scenes (`MainMenu`, `GameScene`) to be present in Build Settings.

When setting up the **MainMenu** scene for WebGL, add a `WebGLInitializer`
component to the `Managers` GameObject alongside `GameManager` and
`InputManager`. This bootstraps the browser bridge automatically.
