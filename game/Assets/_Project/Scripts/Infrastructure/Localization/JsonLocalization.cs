using System;
using System.Collections.Generic;
using System.IO;
using Aetherion.Application.Ports;
using UnityEngine;

namespace Aetherion.Infrastructure.Localization
{
    [Serializable]
    internal sealed class L10nFileDto
    {
        public string locale;
        public L10nEntry[] entries;
    }

    [Serializable]
    internal sealed class L10nEntry
    {
        public string key;
        public string value;
    }

    /// <summary>
    /// Loads localization from a simplified array form OR from key/value object JSON.
    /// For VS0 we also accept Unity-unfriendly object maps by line-scanning name_key style files
    /// via a companion simple format under StreamingAssets.
    /// </summary>
    public sealed class JsonLocalization : ILocalization
    {
        private readonly Dictionary<string, string> _map =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public void LoadFromSimpleJsonObject(string path)
        {
            _map.Clear();
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[L10n] Missing: {path}");
                return;
            }

            // VS0: parse flat "key": "value" pairs without System.Text.Json dependency issues.
            var text = File.ReadAllText(path);
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!line.StartsWith("\"", StringComparison.Ordinal))
                        continue;
                    var colon = line.IndexOf(':');
                    if (colon <= 1) continue;
                    var keyPart = line.Substring(0, colon).Trim();
                    var valPart = line.Substring(colon + 1).Trim().TrimEnd(',');
                    var key = Unquote(keyPart);
                    var val = Unquote(valPart);
                    if (!string.IsNullOrEmpty(key) && val != null && key != "locale" && key != "entries")
                        _map[key] = val;
                }
            }

            Debug.Log($"[L10n] Loaded {_map.Count} string(s) from {path}");
        }

        public string Get(string key, string fallback = null)
        {
            if (string.IsNullOrEmpty(key))
                return fallback ?? string.Empty;
            return _map.TryGetValue(key, out var value) ? value : (fallback ?? key);
        }

        private static string Unquote(string s)
        {
            s = s.Trim();
            if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"')
                return s.Substring(1, s.Length - 2).Replace("\\\"", "\"");
            return s;
        }
    }
}
