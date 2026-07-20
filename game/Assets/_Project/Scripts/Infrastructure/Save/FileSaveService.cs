using System;
using System.Collections.Generic;
using System.IO;
using Aetherion.Application.Ports;
using Aetherion.Application.Sessions;
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
                flags = ToFlagArray(snapshot.Flags)
            };

            var path = SlotPath(slot);
            var temp = path + ".tmp";
            var json = JsonUtility.ToJson(dto, true);
            File.WriteAllText(temp, json);
            if (File.Exists(path))
                File.Delete(path);
            File.Move(temp, path);
            Debug.Log($"[Save] Wrote slot {slot} -> {path}");
        }

        public bool TryLoad(int slot, out WorldSessionSnapshot snapshot)
        {
            snapshot = null;
            var path = SlotPath(slot);
            if (!File.Exists(path))
                return false;

            try
            {
                var json = File.ReadAllText(path);
                var dto = JsonUtility.FromJson<SaveFileDto>(json);
                if (dto == null)
                    return false;

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
                    Codex = new Dictionary<string, int>(StringComparer.Ordinal)
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
    }
}
