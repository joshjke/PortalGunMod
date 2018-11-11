using StardewValley;
using xTile.Tiles;

// Used to save Portal Locations into a JSON file
namespace CustomPortalLocations
{
    internal class NewPortalLocations
    {
        // default up to 100 diffeerent portals
        public PortalLocation[] portalLocations { get; set; } = new PortalLocation[100];   
    }
}