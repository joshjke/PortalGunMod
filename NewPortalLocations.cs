using StardewValley;
using xTile.Tiles;

namespace CustomPortalLocations
{
    internal class NewPortalLocations
    {
        public PortalLocation[] portalLocations { get; set; } = new PortalLocation[2];

        public Warp[] portalWarpLocations = new Warp[3] { null, null, null};

        public Tile[] OldTiles = new Tile[3] { null, null, null };
    }
}