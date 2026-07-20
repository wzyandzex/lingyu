using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aetherion.Application.Ports;
using Aetherion.Application.Sessions;
using Aetherion.Domain.Codex;
using UnityEngine;

namespace Aetherion.Infrastructure.Save
{
    [Serializable]
    internal sealed class SaveFileDto
    {
        public int version;
        public string savedAtUtc;
        public string areaId;
        public PlayerDto player;
        public FlagEntry[] flags;
        public string[] party;
        public CodexSaveEntry[] codex;
    }

    [Serializable]
    internal sealed class PlayerDto
    {
        public float x;
        public float y;
        public float z;
        public float yaw;
    }

    [Serializable]
    internal sealed class FlagEntry
    {
        public string key;
        public bool value;
    }

    [Serializable]
    internal sealed class CodexSaveEntry
    {
        public string defId;
        public string[] layers;
    }

    public sealed class FileSaveService : ISaveService
    {
        private readonly string _rootDirectory;

        public FileSaveService(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
            Directory.CreateDirectory(_rootDirectory);
        }

        public bool Exists(int slot) => File.Exists(SlotPath(slot));

        public void Save(int slot, WorldSessionSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            var dto = new SaveFileDto
            {
                version = snapshot.Version,
                savedAtUtc = snapshot.SavedAtUtc,
                areaId = snapshot.AreaId,
                player = new PlayerDto
                {
                    x = snapshot.Player?.X ?? 0f,
                    y = snapshot.Player?.Y ?? 0f,
                    z = snapshot.Player?.Z ?? 0f,
                    yaw = snapshot.Player?.Yaw ?? 0f
                },
                party = snapshot.Party ?? Array.Empty<string>(),
                flags = ToFlagArray(snapshot.Flags),
                codex = ToCodexArray(snapshot.CodexEntries)
            };

            var path = SlotPath(slot);
            var temp = path + ".tmp";
            var json = JsonUtility.ToJson(dto, true);
            File.WriteAllText(temp, json, Encoding.UTF8);
            if (File.Exists(path))
                File.Delete(path);
            File.Move(temp, path);
            Debug.Log($"[Save] Wrote slot {slot} v{dto.version} -> {path}");
        }

        public bool TryLoad(int slot, out WorldSessionSnapshot snapshot)
        {
            snapshot = null;
            var path = SlotPath(slot);
            if (!File.Exists(path))
                return false;

            try
            {
                var json = File.ReadAllText(path, Encoding.UTF8);
                var dto = JsonUtility.FromJson<SaveFileDto>(json);
                if (dto == null)
                    return false;

                if (dto.version > WorldSessionSnapshot.CurrentVersion)
                {
                    Debug.LogError($"[Save] Future version {dto.version} not supported");
                    return false;
                }

                snapshot = new WorldSessionSnapshot
                {
                    Version = dto.version,
                    SavedAtUtc = dto.savedAtUtc ?? string.Empty,
                    AreaId = string.IsNullOrEmpty(dto.areaId) ? "R01" : dto.areaId,
                    Player = new PlayerSnapshot
                    {
                        X = dto.player != null ? dto.player.x : 0f,
                        Y = dto.player != null ? dto.player.y : 0f,
                        Z = dto.player != null ? dto.player.z : 0f,
                        Yaw = dto.player != null ? dto.player.yaw : 0f
                    },
                    Party = dto.party ?? Array.Empty<string>(),
                    Flags = FromFlagArray(dto.flags),
                    CodexEntries = FromCodexArray(dto.codex)
                };
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Save] Load failed: {ex.Message}");
                return false;
            }
        }

        private string SlotPath(int slot) =>
            Path.Combine(_rootDirectory, $"slot{slot}.json");

        private static FlagEntry[] ToFlagArray(Dictionary<string, bool> flags)
        {
            if (flags == null || flags.Count == 0)
                return Array.Empty<FlagEntry>();
            var list = new List<FlagEntry>(flags.Count);
            foreach (var kv in flags)
                list.Add(new FlagEntry { key = kv.Key, value = kv.Value });
            return list.ToArray();
        }

        private static Dictionary<string, bool> FromFlagArray(FlagEntry[] entries)
        {
            var dict = new Dictionary<string, bool>(StringComparer.Ordinal);
            if (entries == null) return dict;
            foreach (var e in entries)
            {
                if (e != null && !string.IsNullOrEmpty(e.key))
                    dict[e.key] = e.value;
            }
            return dict;
        }

        private static CodexSaveEntry[] ToCodexArray(List<CodexEntryState> entries)
        {
            if (entries == null || entries.Count == 0)
                return Array.Empty<CodexSaveEntry>();
            var list = new List<CodexSaveEntry>(entries.Count);
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrEmpty(e.DefId)) continue;
                var layers = new List<string>();
                if (e.Layers != null)
                {
                    foreach (var layer in e.Layers)
                        layers.Add(layer.ToString());
                }
                list.Add(new CodexSaveEntry { defId = e.DefId, layers = layers.ToArray() });
            }
            return list.ToArray();
        }

        private static List<CodexEntryState> FromCodexArray(CodexSaveEntry[] entries)
        {
            var list = new List<CodexEntryState>();
            if (entries == null) return list;
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrEmpty(e.defId)) continue;
                var layers = new List<CodexLayer>();
                if (e.layers != null)
                {
                    foreach (var name in e.layers)
                    {
                        if (Enum.TryParse(name, true, out CodexLayer layer))
                            layers.Add(layer);
                    }
                }
                list.Add(new CodexEntryState { DefId = e.defId, Layers = layers });
            }
            return list;
        }
    }
}
