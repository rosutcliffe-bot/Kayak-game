# Scene Setup Guide – Kayak Simulator

## Overview
This document describes how to configure the two Unity scenes required by the
Kayak Simulator:

1. **MainMenu** – title screen, mode selection, settings, quit
2. **GameScene** – the ocean environment with the playable kayak

---

## Prerequisites

| Tool | Version |
|---|---|
| Unity | 2022.3 LTS or later |
| Render Pipeline | Universal Render Pipeline (URP) 14+ |
| Input System | com.unity.inputsystem 1.7+ |
| TextMeshPro | com.unity.textmeshpro 3.0+ |

Install URP via **Window → Package Manager** and set the
*Graphics* settings asset to a URP asset.

---

## Project Settings

1. **Input System** → Edit → Project Settings → Player →
   *Active Input Handling* → **Both** (legacy + new).
2. Add scene names to the **Build Settings** in the following order:
   - Index 0: `Assets/Scenes/MainMenu.unity`
   - Index 1: `Assets/Scenes/GameScene.unity`

---

## MainMenu Scene

```
[Hierarchy]
 ├── Managers
 │    ├── GameManager          (GameManager.cs — DontDestroyOnLoad)
 │    └── InputManager         (InputManager.cs — DontDestroyOnLoad)
 ├── MainMenuCanvas (Canvas — Screen Space Overlay)
 │    ├── MainPanel
 │    │    ├── Title (TMP_Text)
 │    │    ├── StartButton
 │    │    ├── SettingsButton
 │    │    └── QuitButton
 │    └── SettingsPanel (SettingsManager.cs)
 │         ├── MasterVolumeSlider
 │         ├── MusicVolumeSlider
 │         ├── SFXVolumeSlider
 │         ├── QualityDropdown
 │         ├── FullscreenToggle
 │         └── BackButton → calls MainMenuManager.OnSettingsBackClicked()
 └── DirectionalLight
```

### Steps
1. Create a new scene named `MainMenu`.
2. Add an empty `Managers` GameObject and attach `GameManager.cs`, `InputManager.cs`,
   and `WebGLInitializer.cs` (from `Scripts/WebGL/`).
3. Create a **Canvas** (Screen Space – Overlay) and attach `MainMenuManager.cs`.
4. Wire up all Button/Toggle/Slider references in the inspector.
5. Set **EventSystem** (created automatically with Canvas).

---

## GameScene Scene

```
[Hierarchy]
 ├── Managers
 │    ├── GameManager          (already DontDestroyOnLoad, persists from MainMenu)
 │    ├── InputManager         (already DontDestroyOnLoad)
 │    ├── WaterPhysics         (WaterPhysics.cs)
 │    └── WaterEffects         (WaterEffects.cs) — assign particle prefabs
 │
 ├── Environment
 │    ├── WaterPlane           (WaterSurface.cs + MeshFilter + MeshRenderer)
 │    │    └── Material: Assets/Materials/OceanSurface.mat
 │    ├── GerstnerWaveSystem   (GerstnerWaveSystem.cs)
 │    │    └── Assign OceanSurface material to Water Material field
 │    ├── Directional Light    (shadows: Soft)
 │    ├── Skybox               (assign HDR panoramic skybox)
 │    └── ReflectionProbe      (Type: Realtime, Refresh: Every Frame)
 │
 ├── Kayak (Rigidbody — mass 18 kg, drag 0, angular drag 0)
 │    ├── KayakPhysicsController.cs
 │    ├── BuoyancySystem.cs   — configure 5 hull points
 │    ├── KayakMesh           (MeshRenderer — high-poly kayak model)
 │    └── Paddle
 │         ├── PaddleController.cs
 │         ├── PaddleMesh
 │         ├── LeftBlade       (PaddleBlade.cs)
 │         │    ├── SplashParticles
 │         │    └── WakeTrail
 │         └── RightBlade      (PaddleBlade.cs)
 │              ├── SplashParticles
 │              └── WakeTrail
 │
 ├── MainCamera               (CameraController.cs)
 │    └── Assign Kayak Transform reference
 │
 └── HUD_Canvas (Canvas — Screen Space – Overlay)
      ├── HUDManager.cs        — assign KayakPhysicsController reference
      ├── SpeedLabel (TMP_Text)
      ├── LeftStrokeBar (Image — Filled)
      ├── RightStrokeBar (Image — Filled)
      ├── HeadingLabel (TMP_Text)
      └── PauseMenu (PauseMenuManager.cs)
           ├── PausePanel
           │    ├── ResumeButton
           │    ├── SettingsButton
           │    └── MainMenuButton
           └── SettingsPanel (SettingsManager.cs)
```

### Steps

#### 1. Water Setup
1. Create `WaterPlane` → add `MeshFilter`, `MeshRenderer`, `WaterSurface.cs`.
2. Create `Assets/Materials/OceanSurface.mat` → assign
   `KayakSimulator/OceanSurface` shader.
3. Assign normal map textures (any tileable water normal map).
4. Create `GerstnerWaveSystem` and assign the material reference.

#### 2. Kayak Rigidbody
1. Add a `Rigidbody` → mass **18**, drag **0**, angular drag **0**
   (BuoyancySystem controls drag dynamically).
2. Add a box/capsule collider sized to the hull.
3. Attach `BuoyancySystem.cs` — adjust buoyancy points to match hull geometry.
4. Attach `KayakPhysicsController.cs`.

#### 3. Paddle
1. Create child `Paddle` under the Kayak root.
2. Create `LeftBlade` and `RightBlade` children positioned at the blade faces.
3. Attach `PaddleBlade.cs` to each blade.
4. Attach `PaddleController.cs` to the `Paddle` root and assign blade references.

#### 4. Camera
1. Select the `Main Camera`, attach `CameraController.cs`.
2. Assign the Kayak root transform.
3. Set the **Collision Mask** to include terrain/water layers.

#### 5. Lighting & Environment
- Directional Light: intensity 1.2, shadows Soft, colour warm white.
- Skybox: assign a HDR skybox in **Window → Rendering → Lighting → Environment**.
- Post-processing (URP Volume):
  - Bloom (threshold 0.9, intensity 0.4)
  - Color Grading (slight warm lift)
  - Depth of Field (optional, focus on kayak)

---

## Material: OceanSurface.mat

| Property | Recommended Value |
|---|---|
| Shallow Color | `(0.10, 0.60, 0.70, 0.85)` |
| Deep Color | `(0.02, 0.10, 0.30, 1.00)` |
| Depth Distance | `5` |
| Smoothness | `0.92` |
| Fresnel Power | `3` |
| Normal Strength | `1.2` |
| Normal Map 1/2 | tileable water normal, scale ~0.15 |
| Foam Threshold | `0.6` |

---

## Audio Suggestions (add AudioSource components)

| Sound | Trigger |
|---|---|
| Paddle splash (short) | `PaddleBlade.OnBladeEnterWater()` |
| Continuous water | Loop on kayak, volume ∝ speed |
| Ambient wind | Loop, constant |
| UI click | Button onClick |

---

## Extending the Project

- **New water body** – drop in a new `WaterSurface` + `GerstnerWaveSystem` with
  different wave parameters (river rapids, calm lake).
- **Multiplayer** – `KayakPhysicsController` is self-contained; wrap with
  Netcode for GameObjects' NetworkBehaviour and sync `_rb.position/rotation`.
- **FFT ocean** – replace `GerstnerWaveSystem` with an FFT implementation;
  `WaterPhysics` callers need no changes since they use `GetSurfaceHeight`.
