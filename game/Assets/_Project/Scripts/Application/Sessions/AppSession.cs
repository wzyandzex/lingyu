using Aetherion.Application.Ports;

namespace Aetherion.Application.Sessions
{
    /// <summary>Process-wide services. Does not own per-save progress.</summary>
    public sealed class AppSession
    {
        public IDataCatalog DataCatalog { get; }
        public ISaveService SaveService { get; }
        public ILocalization Localization { get; }
        public GameModeRouter ModeRouter { get; }

        public WorldSession World { get; private set; }

        public AppSession(
            IDataCatalog dataCatalog,
            ISaveService saveService,
            ILocalization localization,
            GameModeRouter modeRouter)
        {
            DataCatalog = dataCatalog;
            SaveService = saveService;
            Localization = localization;
            ModeRouter = modeRouter;
        }

        public void StartNewGame(string areaId = "R01")
        {
            World = new WorldSession { AreaId = areaId };
            World.Player.X = 0f;
            World.Player.Y = 1f;
            World.Player.Z = 0f;
            World.Player.Yaw = 0f;
        }

        public bool TryContinue(int slot = 0)
        {
            if (!SaveService.TryLoad(slot, out var snapshot))
                return false;

            if (snapshot.Version > WorldSessionSnapshot.CurrentVersion)
                return false;

            World = new WorldSession();
            World.ApplySnapshot(snapshot);
            return true;
        }

        public void Save(int slot = 0)
        {
            if (World == null) return;
            SaveService.Save(slot, World.ToSnapshot());
        }
    }
}
