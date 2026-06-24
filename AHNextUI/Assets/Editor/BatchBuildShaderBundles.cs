using UnityEditor;
using UnityEngine;

// Headless entry points for building the point-cloud shader bundle(s) from the
// command line, e.g.:
//   Unity -batchmode -quit -projectPath AHNextUI \
//         -executeMethod BatchBuildShaderBundles.OSX
//
// These just switch the active build target and delegate to the menu logic in
// BuildShaderBundles.Build() so CLI builds and the "KSP ▸ Build Shader Bundle"
// menu item stay byte-for-byte identical.
public static class BatchBuildShaderBundles
{
    public static void OSX()     { BuildFor(BuildTarget.StandaloneOSX); }
    public static void Linux()   { BuildFor(BuildTarget.StandaloneLinux64); }
    public static void Windows() { BuildFor(BuildTarget.StandaloneWindows64); }

    public static void All()
    {
        BuildFor(BuildTarget.StandaloneOSX);
        BuildFor(BuildTarget.StandaloneLinux64);
        BuildFor(BuildTarget.StandaloneWindows64);
    }

    static void BuildFor(BuildTarget target)
    {
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, target))
            {
                Debug.LogError("[AHNext] Failed to switch active build target to " + target);
                return;
            }
        }
        BuildShaderBundles.Build();
    }
}
