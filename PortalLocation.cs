namespace CustomPortalLocations
{
    internal class PortalLocation
    {
        public string locationName;
        public int xCoord;
        public int yCoord;
        public bool exists;

        public PortalLocation(string locationName = "", int x = -1, int y = -1, bool exists = false)
        {
            this.locationName = locationName;
            this.xCoord = x;
            this.yCoord = y;
            this.exists = exists;
        }

        public static bool operator== (PortalLocation location1, PortalLocation location2)
        {
            return (location1.locationName == location2.locationName
                        && location1.xCoord == location2.xCoord
                        && location1.yCoord == location2.yCoord
                        && location1.exists == location2.exists);
        }

        public static bool operator != (PortalLocation location1, PortalLocation location2)
        {
            return !(location1.locationName == location2.locationName
                        && location1.xCoord == location2.xCoord
                        && location1.yCoord == location2.yCoord
                        && location1.exists == location2.exists);
        }
    }
}