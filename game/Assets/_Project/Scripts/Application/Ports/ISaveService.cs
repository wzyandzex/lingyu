using Aetherion.Application.Sessions;

namespace Aetherion.Application.Ports
{
    public interface ISaveService
    {
        void Save(int slot, WorldSessionSnapshot snapshot);
        bool TryLoad(int slot, out WorldSessionSnapshot snapshot);
        bool Exists(int slot);
    }
}
