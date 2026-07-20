using System;
using System.Collections.Generic;
using Aetherion.Domain.Bonding;
using Aetherion.Domain.Codex;
using Aetherion.Domain.Creatures;
using Aetherion.Domain.Party;

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
        public CodexProgress Codex { get; } = new CodexProgress();
        public PartyState Party { get; } = new PartyState();

        /// <summary>In-progress QuietFollow session; null when not bonding.</summary>
        public BondingSession ActiveBonding { get; set; }

        public WorldSessionSnapshot ToSnapshot()
        {
            var party = new List<PartyMemberSnapshot>();
            foreach (var m in Party.Members)
            {
                party.Add(new PartyMemberSnapshot
                {
                    InstanceId = m.InstanceId,
                    DefId = m.DefId.Value
                });
            }

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
                Party = party,
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

            Party.Clear();
            if (snapshot.Party != null)
            {
                var list = new List<CreatureInstance>();
                foreach (var p in snapshot.Party)
                {
                    if (p == null || string.IsNullOrEmpty(p.DefId)) continue;
                    list.Add(new CreatureInstance(CreatureDefId.Parse(p.DefId), p.InstanceId));
                }
                Party.ReplaceAll(list);
            }

            ActiveBonding = null;
        }
    }

    public sealed class PlayerSnapshot
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Yaw { get; set; }
    }

    public sealed class PartyMemberSnapshot
    {
        public string InstanceId { get; set; }
        public string DefId { get; set; }
    }

    public sealed class WorldSessionSnapshot
    {
        public const int CurrentVersion = 2;

        public int Version { get; set; } = CurrentVersion;
        public string SavedAtUtc { get; set; } = string.Empty;
        public string AreaId { get; set; } = "R01";
        public PlayerSnapshot Player { get; set; } = new PlayerSnapshot();
        public Dictionary<string, bool> Flags { get; set; } = new Dictionary<string, bool>();
        public List<PartyMemberSnapshot> Party { get; set; } = new List<PartyMemberSnapshot>();
        public List<CodexEntryState> CodexEntries { get; set; } = new List<CodexEntryState>();
    }
}
