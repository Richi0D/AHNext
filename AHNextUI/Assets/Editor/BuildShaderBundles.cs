using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

// Builds the AntennaHelperNext point-cloud shader bundle for the active build
// target and copies it straight into GameData with the correct platform suffix
// (_osx / _linux / _windows).
//
// Why this is explicit instead of relying on Inspector "AssetBundle" tags:
//   * The repo .gitignore excludes *.meta, so the assetBundleName tag is NOT in
//     version control and is lost on a fresh clone. Defining the AssetBundleBuild
//     here makes the build reproducible from a clean checkout.
//   * The previous process was "switch target, rebuild, manually rename" -- which
//     is how the _osx bundle was missed in d2600b5 (issue #5). Auto-copying with
//     the right name removes that foot-gun.
//   * macOS standalone defaults to Metal-only graphics APIs, and Metal cannot run
//     geometry shaders. We force OpenGLCore into the API list so the bundle ships
//     a GLCore variant of the geometry-shader point_particle (KSP on macOS runs
//     OpenGL 4.1), matching how the working Linux bundle is produced.
public static class BuildShaderBundles
{
    // The shader asset(s) that go into the bundle. point_particle.shader is the
    // only shader the mod loads at runtime (Lib.GetShader("PointParticle")).
    const string ShaderAsset = "Assets/Shaders/point_particle.shader";

    // Temp bundle name; the produced file is copied/renamed to _<platform>.
    const string BundleName = "ahshaders";

    [MenuItem("KSP/Build Shader Bundle (current platform)")]
    public static void Build()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

        string suffix = PlatformSuffix(target);
        if (suffix == null)
        {
            Debug.LogError("[AHNext] Active build target " + target +
                           " is not OSX/Linux/Windows standalone. Switch target and retry.");
            return;
        }

        // --- force the graphics APIs so the right shader variants are compiled ---
        ApplyGraphicsAPIs(target);

        // --- build the bundle from an explicit definition (no .meta reliance) ---
        string outputDir = "ShaderBundles";
        Directory.CreateDirectory(outputDir);

        var build = new AssetBundleBuild
        {
            assetBundleName = BundleName,
            assetNames = new[] { ShaderAsset },
        };

        BuildPipeline.BuildAssetBundles(
            outputDir,
            new[] { build },
            BuildAssetBundleOptions.None,
            target);

        // --- copy into GameData as _<platform> ---
        string built = Path.Combine(outputDir, BundleName);
        if (!File.Exists(built))
        {
            Debug.LogError("[AHNext] Expected bundle not found at " + built +
                           " -- build may have failed (is " + ShaderAsset + " present?).");
            return;
        }

        // AHNextUI/Assets -> repo root -> GameData/.../Shaders/15
        string repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
        string destDir = Path.Combine(repoRoot, "GameData", "AntennaHelperNext", "Shaders", "15");
        Directory.CreateDirectory(destDir);
        string dest = Path.Combine(destDir, "_" + suffix);

        File.Copy(built, dest, overwrite: true);
        Debug.Log("[AHNext] Shader bundle for " + target + " written to: " + dest +
                  "  (graphics APIs: " + string.Join(", ", System.Array.ConvertAll(
                      PlayerSettings.GetGraphicsAPIs(target), a => a.ToString())) + ")");
        AssetDatabase.Refresh();
    }

    static string PlatformSuffix(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneOSX:        return "osx";
            case BuildTarget.StandaloneLinux64:    return "linux";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:  return "windows";
            default:                               return null;
        }
    }

    // Pin the graphics-API order so geometry-shader variants are produced for the
    // API KSP actually uses on each platform.
    static void ApplyGraphicsAPIs(BuildTarget target)
    {
        GraphicsDeviceType[] apis;
        switch (target)
        {
            // macOS: KSP runs OpenGL 4.1 -> GLCore variant is the one used. Metal
            // kept second so a Metal-forced launch at least loads (geometry stage
            // is unsupported there -- that is the future Option B's job to fix).
            case BuildTarget.StandaloneOSX:
                apis = new[] { GraphicsDeviceType.OpenGLCore, GraphicsDeviceType.Metal };
                break;
            case BuildTarget.StandaloneLinux64:
                apis = new[] { GraphicsDeviceType.OpenGLCore, GraphicsDeviceType.Vulkan };
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                apis = new[] { GraphicsDeviceType.Direct3D11 };
                break;
            default:
                return;
        }

        PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);
        PlayerSettings.SetGraphicsAPIs(target, apis);
    }
}
