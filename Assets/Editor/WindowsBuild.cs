using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WindowsBuild
{
    private const string MainMenu = "Assets/Scenes/MainMenu.unity";

    [Serializable]
    private sealed class AssemblyManifest
    {
        public string[] names = Array.Empty<string>();
    }

    [MenuItem("Milthrill/Gerar build Windows (64 bits)")]
    public static void Build()
    {
        string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        if (projectPath.Contains(","))
        {
            throw new BuildFailedException(
                "O UnityLinker desta versão falha quando o caminho do projeto contém vírgula. " +
                "Feche o projeto e reabra uma cópia em um caminho sem vírgula, como C:/Projetos/Milthrill.");
        }

        string[] scenes = Array.ConvertAll(
            Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled),
            scene => scene.path);

        if (scenes.Length == 0 || scenes[0] != MainMenu)
            throw new BuildFailedException("Coloque MainMenu como primeira cena habilitada na lista de build.");

        // Uma pasta nova evita misturar arquivos de builds diferentes.
        string directory = Path.GetFullPath(Path.Combine(
            projectPath, "Builds", "Windows", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")));
        Directory.CreateDirectory(directory);

        string executable = Path.Combine(directory, "Milthrill.exe");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = executable,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.CleanBuildCache
        });

        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException($"Build Windows: {report.summary.result}. Consulte o Console.");

        RequireFile(executable);
        RequireFile(Path.Combine(directory, "UnityPlayer.dll"));
        string data = Path.Combine(directory, "Milthrill_Data");
        RequireFile(Path.Combine(data, "globalgamemanagers"));
        RequireFile(Path.Combine(data, "level0"));

        if (!File.Exists(Path.Combine(directory, "GameAssembly.dll")))
        {
            RequireFile(Path.Combine(directory, "MonoBleedingEdge", "EmbedRuntime", "mono-2.0-bdwgc.dll"));
            string manifestPath = Path.Combine(data, "ScriptingAssemblies.json");
            RequireFile(manifestPath);
            var manifest = JsonUtility.FromJson<AssemblyManifest>(File.ReadAllText(manifestPath));
            if (manifest?.names == null || manifest.names.Length == 0)
                throw new BuildFailedException("O manifesto de DLLs da build está vazio.");

            RequireFile(Path.Combine(data, "Managed", "mscorlib.dll"));
            RequireFile(Path.Combine(data, "Managed", "Assembly-CSharp.dll"));
            foreach (string assembly in manifest.names)
                RequireFile(Path.Combine(data, "Managed", assembly));
        }

        Debug.Log($"Build Windows publicada em {directory}");
        if (!Application.isBatchMode)
            EditorUtility.RevealInFinder(executable);
    }

    private static void RequireFile(string path)
    {
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
            throw new BuildFailedException($"Arquivo ausente ou vazio na build: {path}");
    }
}
