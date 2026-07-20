using System.IO;
using UnityEngine;

namespace Aetherion.Infrastructure.Bootstrap
{
    /// <summary>Resolves data paths for Editor (repo data/) vs Player (StreamingAssets).</summary>
    public static class DataPathResolver
    {
        public static string GetCreaturesDirectory()
        {
#if UNITY_EDITOR
            var repo = FindRepoRoot();
            if (repo != null)
            {
                var p = Path.Combine(repo, "data", "creatures");
                if (Directory.Exists(p))
                    return p;
            }
#endif
            return Path.Combine(Application.streamingAssetsPath, "data", "creatures");
        }

        public static string GetL10nFilePath(string localeFileName = "zh-Hans.json")
        {
#if UNITY_EDITOR
            var repo = FindRepoRoot();
            if (repo != null)
            {
                var p = Path.Combine(repo, "data", "l10n", localeFileName);
                if (File.Exists(p))
                    return p;
            }
#endif
            return Path.Combine(Application.streamingAssetsPath, "data", "l10n", localeFileName);
        }

        public static string GetSaveDirectory()
        {
            return Path.Combine(Application.persistentDataPath, "saves");
        }

#if UNITY_EDITOR
        private static string FindRepoRoot()
        {
            // game/ is Unity project root; repo root is parent of game/
            var dataPath = Application.dataPath; // .../game/Assets
            var gameRoot = Directory.GetParent(dataPath)?.FullName;
            var repoRoot = Directory.GetParent(gameRoot ?? string.Empty)?.FullName;
            if (repoRoot != null && Directory.Exists(Path.Combine(repoRoot, "data")))
                return repoRoot;
            return null;
        }
#endif
    }
}
