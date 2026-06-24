# Building AntennaHelperNext

This document covers how to (re)build the point-cloud **shader bundles** that ship
in `GameData/AntennaHelperNext/Shaders/15/` as `_osx`, `_linux`, and `_windows`.
These are Unity AssetBundles built from `AHNextUI/Assets/Shaders/point_particle.shader`
(the `Custom/PointParticle` geometry shader the mod loads at runtime).

> The C# plugin (`AntennaHelperNext/`) is a separate, normal Unity/KSP assembly build
> and is not covered here — this doc is specifically about the shader bundles, which
> have a platform-specific gotcha (see *Why the graphics APIs are forced*).

## Prerequisite: Unity 2019.4.18f1 (exact version)

The bundles **must** be built in **Unity 2019.4.18f1** — the engine version KSP
1.12.5 ships with. AssetBundles are **not** forward-compatible across major Unity
versions: a bundle built in any other major version (e.g. 6000.x) will silently
fail to load in KSP, leaving the point cloud broken with no obvious error.

Install it via Unity Hub → *Installs* → *Add* → *Archive* (or the 2019.4 LTS
download page).

## Building

The Unity project lives in `AHNextUI/`. Open it in Unity 2019.4.18f1 and let it
import once before building.

### Option A — Unity Editor menu (one platform at a time)

1. **File ▸ Build Settings** → select **PC, Mac & Linux Standalone**, set
   **Target Platform** to the platform you want, and click **Switch Platform**.
2. Run **KSP ▸ Build Shader Bundle (current platform)**.

The console logs the output path and the graphics APIs used, e.g.:

```
[AHNext] Shader bundle for StandaloneOSX written to: …/Shaders/15/_osx  (graphics APIs: OpenGLCore, Metal)
```

The bundle is built and **auto-copied** into `GameData/AntennaHelperNext/Shaders/15/`
with the correct `_<platform>` suffix — no manual rename needed.

### Option B — headless / command line (all platforms in one run)

`BatchBuildShaderBundles` exposes entry points for batch-mode builds, so the whole
set can be produced from one command (CI-friendly, no GUI clicking):

```sh
"/Applications/Unity/Hub/Editor/2019.4.18f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -quit \
  -projectPath "AHNextUI" \
  -executeMethod BatchBuildShaderBundles.All \
  -logFile build.log
```

Available `-executeMethod` targets:

| Method | Builds |
|---|---|
| `BatchBuildShaderBundles.OSX`     | `_osx` (OpenGLCore, Metal)   |
| `BatchBuildShaderBundles.Linux`   | `_linux` (OpenGLCore, Vulkan) |
| `BatchBuildShaderBundles.Windows` | `_windows` (Direct3D11)      |
| `BatchBuildShaderBundles.All`     | all three                    |

Each method switches the active build target and then calls the same logic as the
menu item, so CLI and Editor builds are identical. On Windows/Linux, adjust the
path to the `Unity` executable accordingly.

## Why the graphics APIs are forced

`Custom/PointParticle` is a **geometry shader**. On a default macOS standalone
build, Unity uses **Metal only** (graphics API = *Automatic*), and **Metal cannot
run geometry shaders** — so a naive Mac build ships no usable variant and the point
cloud collapses to a line. KSP on macOS actually runs on **OpenGL 4.1**
(GL-over-Metal), so the build script forces **OpenGLCore** into the macOS graphics
API list (with Metal kept as a fallback). Linux is pinned to OpenGLCore+Vulkan and
Windows to Direct3D11 for the same "compile the variant the platform actually uses"
reason.

These graphics-API settings are committed in
`AHNextUI/ProjectSettings/ProjectSettings.asset` (per-platform, *Auto Graphics API*
off), so a fresh clone starts from the correct configuration; the build script also
re-applies them on every run as a safeguard.

## Verifying a bundle

You can confirm a bundle contains the right shader variant by inspecting it with
[UnityPy](https://pypi.org/project/UnityPy/): load the bundle, find the
`Custom/PointParticle` shader object, and scan its raw blob for marker tokens. A
correct macOS bundle contains a **geometry shader** (look for `EmitVertex`) plus a
**GLCore** variant (look for `#version`) — matching `_linux`. If `_osx` instead
shows a PSIZE / point-sprite shader (`PSIZE`, `gl_PointSize`), or only a Metal
variant, the OpenGLCore setting didn't take and the bundle needs rebuilding.

## History — why this build setup exists (issue #5)

Originally the bundles were built with a single menu command that ran
`BuildPipeline.BuildAssetBundles` for the *current* active build target only, relying
on Inspector "AssetBundle" name tags stored in `.meta` files. Because `.gitignore`
excludes `*.meta`, those tags were local-only state, and producing all three
platforms meant manually switching the active target, rebuilding, and renaming/copying
the output by hand — once per platform.

In commit `d2600b5` ("update shader to fix linux issue") the geometry-shader fix was
rebuilt for Linux and Windows but the **macOS pass was skipped**, so `_osx` kept
shipping the old point-sprite shader. macOS users saw the point cloud collapse to a
line ([issue #5](https://github.com/Richi0D/AHNext/issues/5)).

The current scripts remove every manual step: the bundle is built from an explicit
`AssetBundleBuild` (no `.meta` reliance), the graphics APIs are forced per platform,
the output is auto-copied with the right suffix, and `BatchBuildShaderBundles.All`
builds all three platforms in one command — so a platform can't be silently missed
again.
