using Characters;
using Tiles;

namespace Interfaces
{
    public interface ITileInterface
    {
        void OnInteract(Character character);
        void SetOwningTile(TileScript tile);
        TileScript GetOwningTile();
    }
}
