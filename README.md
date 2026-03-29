# River Run — Whitewater Kayak

A **realistic whitewater river kayaking game** with procedurally generated
canyon terrain, flowing rapids, eddies, stoppers, and proper river physics.

**▶ Play instantly in your browser — no installs required.**

Deploy the included `index.html` to any static host (GitHub Pages, Netlify,
itch.io, or any web server) and share the URL. Anyone with a modern browser
can open it and start paddling immediately.

---

## 🚀 How to Play (Quick Start)

1. **Open the game** — go to your deployed URL or run locally (see below)
2. **Click "Launch Downstream"** on the menu screen
3. **Paddle!** The river current carries you automatically:
   - Press **W** to paddle forward
   - Press **A** to paddle/steer left, **D** to paddle/steer right
   - Press **S** to back-paddle/brake
   - Use **←** / **→** arrow keys for fine steering
4. **Dodge rocks** — grey boulders in the river will slow you down on impact
5. **Survive rapids** — when you see ⚠ RAPIDS, paddle hard and steer carefully
6. **Use eddies** — calm water behind large rocks lets you rest
7. **Escape stoppers** — recirculating water at drops can trap you; paddle forward (**W**) to break free

> 💡 **Tip:** Click **"How to Play"** on the menu for full controls reference, or check the table below.

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
| [**Netlify**](https://app.netlify.com/drop) | Drag-and-drop the repo folder (or just `index.html` + `three.min.js`) |
| [**itch.io**](https://itch.io) | Create an HTML project → upload a zip of the repo → set viewport to 960 × 600 |
| **Any web server** | Copy `index.html` and `three.min.js` to your web root |

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
| **River** | Procedurally generated winding river with varying width, gradient, rapids sections, and calm pools |
| **Canyon** | Continuous rocky cliff walls on both sides with moss, overhangs, and outcrop boulders |
| **Water** | Custom GLSL shader with flow-aligned ripples, depth-based colouring, Fresnel reflections, foam, and white-water in rapids |
| **Rocks** | Dozens of boulders in the river to navigate around — some above water, some submerged |
| **Eddies** | Circular currents behind large rocks where you can rest (just like real whitewater) |
| **Stoppers** | Recirculating hydraulics at steep drops that trap the kayak — paddle hard to escape |
| **Rapids** | Steeper river sections with standing waves, turbulent foam, and an on-screen ⚠ RAPIDS warning |
| **Current** | Realistic river current that pushes the kayak downstream, stronger in the centre, with auto-alignment |
| **Kayak** | Tapered hull, cockpit, deck, and animated double-blade paddle — built from procedural geometry |
| **Physics** | Momentum, water drag, bank/rock collisions with bounce, eddy trapping, stopper effects |
| **Modes** | **Arcade** (intuitive: A = left turn, D = right turn) and **Simulation** (realistic: left stroke turns right) |
| **Camera** | Third-person follow, first-person cockpit, orbiting cinematic — press `C` to cycle |
| **HUD** | Live speed (km/h), compass heading, left/right stroke power bars, distance downstream |
| **Particles** | Foam/spray particles near rapids and turbulent sections |
| **Trees** | Procedural pine trees and bushes along the canyon ridgeline |
| **Audio** | Three-layer procedural river sound (rumble + splash + detail) via Web Audio API |
| **Sky** | Gradient sky dome with warm horizon glow and atmospheric fog |
| **Mobile** | Touch paddle zones appear automatically on phones and tablets |
| **Toolbar** | Fullscreen toggle (⛶), mute (🔊), pause (⏸) |
| **Self-contained** | Single HTML file + Three.js — nothing else to install or build |

---

## 🎮 Controls

| Action | Keyboard | Mobile |
|---|---|---|
| Forward paddle | `W` | — |
| Left paddle | `A` | Left touch zone |
| Right paddle | `D` | Right touch zone |
| Back-paddle / brake | `S` | — |
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
three.min.js                     ← Three.js library (local copy)
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

The standalone web game (`index.html`) uses **Three.js** to render a 3D river
canyon scene entirely in the browser:

- **River Path** — a deterministic combination of sine waves defines the river
  centre, width, gradient, and tangent at any Z coordinate. The river winds
  through a canyon that gets steeper and narrower in rapids sections.

- **Water** — a large plane mesh with a custom `ShaderMaterial`. The vertex
  shader displaces vertices to follow the river channel (bank masking pushes
  vertices outside the river below the terrain). The fragment shader blends
  depth-based colours, flow-aligned foam, standing-wave crests, Fresnel
  reflections, and specular highlights.

- **Canyon Walls** — continuous `BufferGeometry` walls generated per chunk with
  vertex colours for wet rock, dry rock, and moss. Rocky outcrops and boulders
  add detail.

- **Current System** — river current follows the path tangent, is strongest in
  the centre, and reverses in stopper zones. Eddies behind rocks create
  circular currents. The kayak auto-aligns with the current for intuitive
  control.

- **Chunk System** — terrain (walls, rocks, trees, ground) is generated in
  60-unit chunks around the player. Old chunks are disposed as the player
  moves downstream, creating an infinite river.

- **Physics** — paddle thrust, differential turning, bank/rock collisions
  with elastic bounce, eddy trapping, stopper recirculation, and auto-
  alignment with river current.

- **Audio** — three layers of filtered noise (low rumble, mid splash, high
  detail) create a realistic river sound via the Web Audio API.

The Unity project under `Assets/` contains the original C# source for anyone
who wants to develop the game further in the Unity Editor. See
[`Documentation/SceneSetupGuide.md`](Documentation/SceneSetupGuide.md) and
[`Documentation/WebGLDeploymentGuide.md`](Documentation/WebGLDeploymentGuide.md)
for Unity-specific instructions.
