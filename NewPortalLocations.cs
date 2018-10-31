using StardewValley;
using xTile.Tiles;

namespace CustomPortalLocations
{
    internal class NewPortalLocations
    {
        public PortalLocation[] portalLocations { get; set; } = new PortalLocation[2];

        public Warp[] portalWarpLocations { get; set; } = new Warp[3] { null, null, null};

        public Tile[] OldTiles { get; set; } = new Tile[3] { null, null, null };
    }
}