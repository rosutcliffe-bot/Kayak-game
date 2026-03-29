# Kayak Simulator

An **ultra-realistic kayak simulator** with Gerstner wave water physics, a 3D
kayak with paddle animation, and both Arcade and Simulation control modes.

**▶ Play instantly in your browser — no installs required.**

Deploy the included `index.html` to any static host (GitHub Pages, Netlify,
itch.io, or any web server) and share the URL. Anyone with a modern browser
can open it and start paddling immediately.

---

## 🌊 Play It Now

### Quickest way (GitHub Pages — free)

1. **Fork** this repository.
2. Go to **Settings → Pages → Source** → select **Deploy from a branch** →
   choose `main` / `/ (root)` → click **Save**.
3. After a minute your game is live at
   `https://<your-username>.github.io/Kayak-game/`.
4. Share the URL — anyone can open it and play.

### Other one-click hosts

| Host | How |
|---|---|
| [**Netlify**](https://app.netlify.com/drop) | Drag-and-drop the repo folder (or just `index.html`) |
| [**itch.io**](https://itch.io) | Create an HTML project → upload a zip of the repo → set viewport to 960 × 600 |
| **Any web server** | Copy `index.html` to your web root — it is 100 % self-contained |

### Run locally (zero setup)

```bash
git clone https://github.com/rosutcliffe-bot/Kayak-game.git
cd Kayak-game
python3 -m http.server 8000          # or:  npx serve .
# Open http://localhost:8000 in your browser
```

Or simply double-click `index.html` in most browsers (Chrome may require the
local server; Firefox works directly).

---

## ✨ Features

| Category | Details |
|---|---|
| **Water** | 3D Gerstner wave ocean with foam, Fresnel reflections, specular highlights, and depth-based colouring — all in a custom GLSL shader |
| **Kayak** | Tapered hull, cockpit, deck, and animated double-blade paddle — built from procedural geometry |
| **Physics** | Momentum, water drag, angular drag, wave-surface tracking, tilt with wave normals, and lean |
| **Modes** | **Arcade** (intuitive: A = left turn, D = right turn) and **Simulation** (realistic: left stroke turns right) |
| **Camera** | Third-person follow, first-person cockpit, orbiting cinematic — press `C` to cycle |
| **HUD** | Live speed (km/h), compass heading, left/right stroke power bars, distance meter |
| **Audio** | Procedural ambient ocean sound via Web Audio API — no audio files needed |
| **Mobile** | Touch paddle zones appear automatically on phones and tablets |
| **Toolbar** | Fullscreen toggle (⛶), mute (🔊), pause (⏸) |
| **Buoys** | 12 coloured buoys bobbing on the waves — paddle around them |
| **Self-contained** | Single HTML file + Three.js from CDN — nothing else to install or build |

---

## 🎮 Controls

| Action | Keyboard | Mobile |
|---|---|---|
| Left paddle | `A` | Left touch zone |
| Right paddle | `D` | Right touch zone |
| Steer | `←` / `→` | — |
| Lean | `Q` / `E` | — |
| Pause | `Escape` | ⏸ button |
| Switch camera | `C` | — |
| Fullscreen | — | ⛶ button |
| Mute | — | 🔊 button |

---

## 📁 Project Structure

```
index.html                       ← THE GAME — open this in a browser
server.py                        – Optional local HTTP server for testing

Assets/                          – Unity project source (for advanced development)
├── Scripts/
│   ├── Core/                    – GameManager, InputManager
│   ├── Physics/                 – Gerstner waves, buoyancy, kayak physics
│   ├── Paddle/                  – Blade forces, paddle animation
│   ├── Controls/                – Camera controller
│   ├── Water/                   – Procedural mesh, VFX pooling
│   ├── UI/                      – Menu, HUD, settings, pause
│   ├── WebGL/                   – Browser bridge (C#)
│   └── Utilities/               – Object pool, smooth follow
├── Editor/                      – One-click WebGL build script
├── Plugins/WebGL/               – Browser bridge (JavaScript)
├── WebGLTemplates/KayakSimulator/ – Custom WebGL HTML template
└── Shaders/Water/               – URP HLSL ocean shader

Documentation/
├── SceneSetupGuide.md           – Unity scene assembly instructions
└── WebGLDeploymentGuide.md      – WebGL build & hosting guide
```

---

## 🔧 How It Works

The standalone web game (`index.html`) uses **Three.js** loaded from a CDN to
render a 3D ocean scene entirely in the browser:

- **Water** — a high-resolution plane mesh with a custom `ShaderMaterial`.
  The vertex shader displaces vertices using four summed Gerstner waves
  (matching the Unity project's `GerstnerWaveSystem`). The fragment shader
  blends shallow/deep colours, adds foam at wave peaks, computes Fresnel
  reflections, and applies specular highlights from a directional sun.

- **Kayak** — procedural geometry (tapered hull, deck, cockpit rim, seat)
  grouped with an animated paddle pivot. Each frame the kayak's Y position
  and tilt are set from the analytical wave height and normal.

- **Physics** — a simple momentum model with thrust from paddle strokes,
  directional drag, angular drag, and speed clamping. Arcade mode maps
  A/D intuitively to left/right turning; Simulation mode uses realistic
  opposing torque.

- **Audio** — a looping noise buffer filtered through a low-pass creates
  ambient ocean sound via the Web Audio API. No sound files are needed.

The Unity project under `Assets/` contains the full high-fidelity C# source
for anyone who wants to develop the game further in the Unity Editor. See
[`Documentation/SceneSetupGuide.md`](Documentation/SceneSetupGuide.md) and
[`Documentation/WebGLDeploymentGuide.md`](Documentation/WebGLDeploymentGuide.md)
for Unity-specific instructions.

