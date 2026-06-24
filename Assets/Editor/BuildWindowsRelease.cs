using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildWindowsRelease
{
    private const string ProductName = "AdaptiveLearningGame";
    private const string BuildFolder = "Builds/Windows";

    [MenuItem("Build/Windows Thesis Release (v1.0)")]
    public static void BuildWindows()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string outputDir = Path.Combine(projectRoot, BuildFolder);
        Directory.CreateDirectory(outputDir);

        string exePath = Path.Combine(outputDir, ProductName + ".exe");

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetEnabledScenePaths(),
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("Build failed: " + report.summary.result);
            return;
        }

        Debug.Log("Build succeeded: " + exePath);
        EditorUtility.RevealInFinder(exePath);
    }

    private static string[] GetEnabledScenePaths()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        int count = 0;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].enabled)
            {
                count++;
            }
        }

        string[] paths = new string[count];
        int index = 0;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (!scenes[i].enabled)
            {
                continue;
            }

            paths[index++] = scenes[i].path;
        }

        return paths;
    }
}
