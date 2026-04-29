using UnityEditor;
using System.IO;

public class BuildShaderBundles
{
    [MenuItem("KSP/Build Shader Bundles")]
    public static void Build()
    {
        string outputDir = "ShaderBundles";
        Directory.CreateDirectory(outputDir);

        // Build for current active platform
        BuildPipeline.BuildAssetBundles(
            outputDir,
            BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget
        );

        UnityEngine.Debug.Log("Shader bundles built to: " + outputDir);
    }
}