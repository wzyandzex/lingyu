using System;
using System.Collections.Generic;

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
                Codex = new Dictionary<string, int>(StringComparer.Ordinal)
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
        public const int CurrentVersion = 0;

        public int Version { get; set; } = CurrentVersion;
        public string SavedAtUtc { get; set; } = string.Empty;
        public string AreaId { get; set; } = "R01";
        public PlayerSnapshot Player { get; set; } = new PlayerSnapshot();
        public Dictionary<string, bool> Flags { get; set; } = new Dictionary<string, bool>();
        public string[] Party { get; set; } = Array.Empty<string>();
        public Dictionary<string, int> Codex { get; set; } = new Dictionary<string, int>();
    }
}
