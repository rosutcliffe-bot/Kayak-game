# Kayak Simulator

An **ultra-realistic kayak simulator** built in Unity (URP), featuring physically
accurate water physics, immersive visuals, and both Arcade and Simulation control modes.

---

## ✨ Features

| Category | Details |
|---|---|
| **Water Physics** | Gerstner wave simulation, Archimedes buoyancy, hydrodynamic drag, water current & Perlin turbulence |
| **Paddle System** | Per-blade hydrodynamic lift/drag, splash & wake VFX, feathered double-blade animation |
| **Controls** | Keyboard / gamepad support via Unity Input System; Arcade mode (direct steering) & Simulation mode (realistic strokes) |
| **Camera** | Third-person chase, first-person cockpit, and orbiting cinematic views — switchable with `C` |
| **Visuals** | Custom HLSL ocean shader (depth fade, Gerstner displacement, foam, refraction, Fresnel reflections, animated normal maps) |
| **UI** | Main menu, pause menu, settings (audio/graphics/controls), in-game HUD (speed, heading, stroke indicators) |
| **Architecture** | Modular C# namespace structure, singleton managers, object pooling for VFX |

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs          – Game state & simulation mode singleton
│   │   └── InputManager.cs         – Unified keyboard / gamepad input
│   ├── Physics/
│   │   ├── GerstnerWaveSystem.cs   – CPU/GPU Gerstner wave model
│   │   ├── BuoyancySystem.cs       – Multi-point Archimedes buoyancy
│   │   ├── KayakPhysicsController.cs – Propulsion, steering, righting torque
│   │   └── WaterPhysics.cs         – High-level water API (height, normal, velocity)
│   ├── Paddle/
│   │   ├── PaddleBlade.cs          – Hydrodynamic blade forces + VFX
│   │   └── PaddleController.cs     – Paddle mesh animation & stroke timing
│   ├── Controls/
│   │   └── CameraController.cs     – Third-person / FP / orbital camera
│   ├── Water/
│   │   ├── WaterSurface.cs         – Procedural mesh + CPU vertex update
│   │   └── WaterEffects.cs         – Pooled splash & ripple particle system
│   ├── UI/
│   │   ├── MainMenuManager.cs
│   │   ├── PauseMenuManager.cs
│   │   ├── SettingsManager.cs      – Audio, graphics, sensitivity (PlayerPrefs)
│   │   └── HUDManager.cs           – Speed, heading, stroke power bars
│   └── Utilities/
│       ├── SmoothFollow.cs         – Smooth transform follower
│       └── ObjectPool.cs           – Generic component object pool
│
├── Shaders/
│   └── Water/
│       └── OceanSurface.shader     – URP HLSL shader with foam, refraction, Fresnel
│
├── Materials/           – (create OceanSurface.mat, assign shader)
├── Prefabs/             – (kayak, paddle, splash/ripple particles)
├── Scenes/              – (MainMenu.unity, GameScene.unity)
├── Textures/            – (normal maps, foam texture)
└── Audio/               – (paddle splash, ambient water/wind)

Packages/
└── manifest.json        – URP 14, Input System 1.7, TMPro 3.0, Cinemachine 2.9

ProjectSettings/
└── ProjectVersion.txt   – Unity 2022.3 LTS

Documentation/
└── SceneSetupGuide.md   – Full step-by-step scene assembly instructions
```

---

## 🚀 Getting Started

### Requirements
- **Unity 2022.3 LTS** (or newer)
- **Universal Render Pipeline (URP)** — included via `Packages/manifest.json`
- **New Input System** package

### Setup
1. Clone this repo and open in Unity Hub → **Open project from disk**.
2. Unity will prompt to install missing packages from `manifest.json`; accept.
3. Configure URP: **Edit → Project Settings → Graphics** → assign a URP asset.
4. Enable New Input System: **Edit → Project Settings → Player → Other Settings →
   Active Input Handling → Both**.
5. Follow [`Documentation/SceneSetupGuide.md`](Documentation/SceneSetupGuide.md)
   to assemble the two scenes.

---

## 🎮 Controls

| Action | Keyboard | Gamepad |
|---|---|---|
| Left paddle stroke | `A` | Left Trigger |
| Right paddle stroke | `D` | Right Trigger |
| Steer | `←` / `→` arrow keys | Left Stick X |
| Lean (Simulation mode) | `Q` / `E` | Right Stick X |
| Pause | `Escape` | Start |
| Switch camera | `C` | — |

---

## 🔧 Architecture Notes

- All systems communicate through **singletons** (`GameManager`, `InputManager`,
  `WaterPhysics`, `WaterEffects`) to keep components loosely coupled.
- **GerstnerWaveSystem** runs the same wave math on both the CPU (for physics
  accuracy) and the GPU (via shader parameters synced each frame).
- Replacing Gerstner with an FFT ocean requires only changing `GerstnerWaveSystem`;
  all callers use the `WaterPhysics` façade.
- Object pooling (`ObjectPool<T>` and `WaterEffects`) avoids runtime GC allocations
  from particle spawning.

---

## 📖 Documentation

See [`Documentation/SceneSetupGuide.md`](Documentation/SceneSetupGuide.md) for:
- Detailed hierarchy layouts for both scenes
- Material property recommendations
- Rigidbody configuration values
- Extension points (multiplayer, FFT ocean, new water bodies)

