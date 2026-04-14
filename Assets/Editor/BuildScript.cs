using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
 
namespace UnityBuilderAction
{
    public static class BuildScript
    {
        private static readonly string Eol = Environment.NewLine;
 
        private static readonly string[] Secrets =
            { "androidKeystorePass", "androidKeyaliasName", "androidKeyaliasPass" };
 
        public static void Build()
        {
            Dictionary<string, string> options = GetValidatedOptions();
 
            // Set version
            if (options.TryGetValue("buildVersion", out string version))
            {
                PlayerSettings.bundleVersion = version;
            }

            // Set Android version code
            if (options.TryGetValue("androidVersionCode", out string androidVersionCode))
            {
                if (int.TryParse(androidVersionCode, out int code))
                {
                    PlayerSettings.Android.bundleVersionCode = code;
                }
            }

            // Emit generated version constants so runtime GameVersion can report them
            string buildNumber = options.TryGetValue("buildNumber", out string bn) && !string.IsNullOrEmpty(bn)
                ? bn
                : "dev";
            string buildTimestamp = options.TryGetValue("buildTimestamp", out string bt) && !string.IsNullOrEmpty(bt)
                ? bt
                : DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            WriteGeneratedVersion(buildNumber, buildTimestamp);

            // Perform build
            BuildReport buildReport = BuildPipeline.BuildPlayer(GetBuildPlayerOptions(options));
            BuildSummary summary = buildReport.summary;
 
            // Report results
            ReportSummary(summary);
 
            // Exit with appropriate code
            ExitWithResult(summary.result);
        }
 
        private static BuildPlayerOptions GetBuildPlayerOptions(Dictionary<string, string> options)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(s => s.path)
                .ToArray();
 
            BuildTarget buildTarget =
                (BuildTarget)Enum.Parse(typeof(BuildTarget), options["buildTarget"]);
 
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = options["customBuildPath"],
                target = buildTarget,
                options = BuildOptions.None,
            };
 
            return buildOptions;
        }
 
        private static Dictionary<string, string> GetValidatedOptions()
        {
            ParseCommandLineArguments(out Dictionary<string, string> validatedOptions);
 
            if (!validatedOptions.TryGetValue("projectPath", out _))
            {
                Debug.LogError("Missing argument -projectPath");
                EditorApplication.Exit(110);
            }
 
            if (!validatedOptions.TryGetValue("buildTarget", out string buildTarget))
            {
                Debug.LogError("Missing argument -buildTarget");
                EditorApplication.Exit(120);
            }
 
            if (!Enum.IsDefined(typeof(BuildTarget), buildTarget ?? string.Empty))
            {
                Debug.LogError($"{buildTarget} is not a defined {nameof(BuildTarget)}");
                EditorApplication.Exit(121);
            }
 
            if (!validatedOptions.TryGetValue("customBuildPath", out _))
            {
                Debug.LogError("Missing argument -customBuildPath");
                EditorApplication.Exit(130);
            }
 
            const string defaultVersion = "1.0.0";
            if (!validatedOptions.TryGetValue("buildVersion", out string ver) ||
                string.IsNullOrEmpty(ver))
            {
                validatedOptions["buildVersion"] = defaultVersion;
            }
 
            return validatedOptions;
        }
 
        private static void ParseCommandLineArguments(
            out Dictionary<string, string> providedArguments)
        {
            providedArguments = new Dictionary<string, string>();
            string[] args = Environment.GetCommandLineArgs();
 
            Debug.Log(
                $"{Eol}" +
                $"###############################{Eol}" +
                $"#   Parsing build arguments   #{Eol}" +
                $"###############################{Eol}" +
                $"{Eol}"
            );
 
            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {
                bool isFlag = args[current].StartsWith("-");
                if (!isFlag) continue;
 
                string flag = args[current].TrimStart('-');
 
                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";
                bool secret = Secrets.Contains(flag);
                string displayValue = secret ? "*****" : $"\"{value}\"";
 
                Debug.Log($"Found flag \"{flag}\" with value {displayValue}.");
                providedArguments.Add(flag, value);
            }
        }
 
        private static void WriteGeneratedVersion(string buildNumber, string buildTimestamp)
        {
            const string path = "Assets/_Project/Code/Game.Utilities/GameVersion.Generated.cs";
            var escapedNumber = buildNumber.Replace("\"", "\\\"");
            var escapedTimestamp = buildTimestamp.Replace("\"", "\\\"");
            var contents =
                "namespace Game.Utilities" + Eol +
                "{" + Eol +
                "    internal static class GameVersionGenerated" + Eol +
                "    {" + Eol +
                $"        public const string BuildNumber = \"{escapedNumber}\";" + Eol +
                $"        public const string BuildTimestamp = \"{escapedTimestamp}\";" + Eol +
                "    }" + Eol +
                "}" + Eol;
            File.WriteAllText(path, contents);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            Debug.Log($"Wrote {path} (buildNumber={buildNumber}, buildTimestamp={buildTimestamp})");
        }

        private static void ReportSummary(BuildSummary summary)
        {
            Debug.Log(
                $"{Eol}" +
                $"###############################{Eol}" +
                $"#       Build results         #{Eol}" +
                $"###############################{Eol}" +
                $"{Eol}" +
                $"Duration: {summary.totalTime}{Eol}" +
                $"Warnings: {summary.totalWarnings}{Eol}" +
                $"Errors:   {summary.totalErrors}{Eol}" +
                $"Size:     {summary.totalSize} bytes{Eol}" +
                $"Result:   {summary.result}{Eol}" +
                $"{Eol}"
            );
        }
 
        private static void ExitWithResult(BuildResult result)
        {
            switch (result)
            {
                case BuildResult.Succeeded:
                    Debug.Log("Build succeeded!");
                    EditorApplication.Exit(0);
                    break;
                case BuildResult.Failed:
                    Debug.LogError("Build failed!");
                    EditorApplication.Exit(101);
                    break;
                case BuildResult.Cancelled:
                    Debug.LogWarning("Build cancelled.");
                    EditorApplication.Exit(102);
                    break;
                case BuildResult.Unknown:
                default:
                    Debug.LogError("Build result is unknown.");
                    EditorApplication.Exit(103);
                    break;
            }
        }
    }
}