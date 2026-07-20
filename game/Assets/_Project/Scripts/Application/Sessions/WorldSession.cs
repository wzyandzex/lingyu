using System;
using System.Collections.Generic;
using Aetherion.Domain.Codex;

namespace Aetherion.Application.Sessions
{
    public sealed class PlayerState
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Yaw { get; set; }
    }

    public sealed class WorldSession
    {
        public string AreaId { get; set; } = "R01";
        public PlayerState Player { get; } = new PlayerState();
        public Dictionary<string, bool> Flags { get; } = new Dictionary<string, bool>(StringComparer.Ordinal);

        /// <summary>Single source of truth for codex unlocks (VS1+).</summary>
        public CodexProgress Codex { get; } = new CodexProgress();

        public WorldSessionSnapshot ToSnapshot()
        {
            return new WorldSessionSnapshot
            {
                Version = WorldSessionSnapshot.CurrentVersion,
                SavedAtUtc = DateTime.UtcNow.ToString("o"),
                AreaId = AreaId,
                Player = new PlayerSnapshot
                {
                    X = Player.X,
                    Y = Player.Y,
                    Z = Player.Z,
                    Yaw = Player.Yaw
                },
                Flags = new Dictionary<string, bool>(Flags),
                Party = Array.Empty<string>(),
                CodexEntries = Codex.ToEntries()
            };
        }

        public void ApplySnapshot(WorldSessionSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            AreaId = snapshot.AreaId ?? "R01";
            Player.X = snapshot.Player?.X ?? 0f;
            Player.Y = snapshot.Player?.Y ?? 0f;
            Player.Z = snapshot.Player?.Z ?? 0f;
            Player.Yaw = snapshot.Player?.Yaw ?? 0f;
            Flags.Clear();
            if (snapshot.Flags != null)
            {
                foreach (var kv in snapshot.Flags)
                    Flags[kv.Key] = kv.Value;
            }

            Codex.Clear();
            if (snapshot.CodexEntries != null)
                Codex.LoadFrom(snapshot.CodexEntries);
        }
    }

    public sealed class PlayerSnapshot
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Yaw { get; set; }
    }

    public sealed class WorldSessionSnapshot
    {
        public const int CurrentVersion = 1;

        public int Version { get; set; } = CurrentVersion;
        public string SavedAtUtc { get; set; } = string.Empty;
        public string AreaId { get; set; } = "R01";
        public PlayerSnapshot Player { get; set; } = new PlayerSnapshot();
        public Dictionary<string, bool> Flags { get; set; } = new Dictionary<string, bool>();
        public string[] Party { get; set; } = Array.Empty<string>();
        public List<CodexEntryState> CodexEntries { get; set; } = new List<CodexEntryState>();
    }
}
