using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

using xTile;
using xTile.ObjectModel;
using xTile.Tiles;
using xTile.Layers;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools;

using System.Linq;
using StardewValley.Locations;
using Microsoft.Xna.Framework.Graphics;

using PyTK.Extensions;
using PyTK.Types;
using PyTK.CustomElementHandler;
using PyTK.CustomTV;

namespace CustomPortalLocations
{
    public class ModEntry : Mod
    {
        // For loading portals, warps, and animations
        private const int NUM_OF_PORTALS = 10;

        // For portal retract key
        private ModConfig config;

        // For saving and loading JSON data
        private static string LocationSaveFileName;

        // Class to save and load from
        internal static NewPortalLocations PortalLocations;

        // array to hold animation states for each portal
        private int[] PortalAnimationFrame = new int[NUM_OF_PORTALS] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

        // Array to hold warps for every portal
        private Warp[] PortalWarpLocations { get; set; } = new Warp[NUM_OF_PORTALS] { null, null, null, null, null, null, null, null, null, null };

        // Array to hold tiles that are replaced by the Portal Tiles
        private Tile[] OldTiles { get; set; } = new Tile[NUM_OF_PORTALS] { null, null, null, null, null, null, null, null, null, null };

        /// <summary>Get all game locations.</summary>
        public static IEnumerable<GameLocation> GetLocations()
        {
            return Game1.locations
                .Concat(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );
        }
        
        // PortalGun Objects
        private CustomObjectData portalGun1;
        private CustomObjectData portalGun2;
        private CustomObjectData portalGun3;
        private CustomObjectData portalGun4;
        private CustomObjectData portalGun1Potato;

        
        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += this.AfterLoad;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;

            this.config = helper.ReadConfig<ModConfig>();

            ControlEvents.KeyPressed += this.KeyPressed;

            Directory.CreateDirectory(
                $"{this.Helper.DirectoryPath}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}");

            //ControlEvents.KeyPressed += this.KeyPressed;
            InputEvents.ButtonPressed += this.ButtonPressed;

            GameEvents.SecondUpdateTick += UpdatePortalAnimationFrames;
            MineEvents.MineLevelChanged += LoadMinePortals;
        }



        // Loads portGun Objects, portal animation tiles, and saves/loads locations from JSON
        private void AfterLoad(object sender, EventArgs e)
        {
            LoadPortalSaves();
            LoadPortalGunObjects();
            LoadPortalTextures();
            LoadMinePortals(sender, e);
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            // skip if save not loaded yet
            if (!Context.IsWorldReady)
                return;
        }

        private void LoadPortalGunObjects()
        {
            // create portalGun objects
            Texture2D portalGunTexture = this.Helper.Content.Load<Texture2D>("PortalGun1.png");
            portalGun1 = CustomObjectData.newObject("PortalGun1Id", portalGunTexture, Color.White, "Portal Gun",
                "Property of Aperture Science Inc.", 0, "", "Basic", 1, -300, "", craftingData: new CraftingData("Portal Gun", "335 5 768 5 769 5"));

            Texture2D portalGunPotatoTexture = this.Helper.Content.Load<Texture2D>("PortalGun1Potato.png");
            portalGun1Potato = CustomObjectData.newObject("PortalGun1PotatoId", portalGunPotatoTexture, Color.White, "Portal Gun Potato",
                "Oh no, it's you", 0, "", "Basic", 1, -300, "", craftingData: new CraftingData("Portal Gun Potato", "335 5 768 5 769 5 192 1"));

            Texture2D bluePortalGunTexture = this.Helper.Content.Load<Texture2D>("PortalGun2.png");
            portalGun2 = CustomObjectData.newObject("PortalGun2Id", bluePortalGunTexture, Color.White, "Blue Portal Gun",
                "Property of Aperture Science Inc.", 0, "", "Basic", 1, -300, "", craftingData: new CraftingData("Blue Portal Gun", "335 5 768 5 769 5"));

            Texture2D greenPortalGunTexture = this.Helper.Content.Load<Texture2D>("PortalGun3.png");
            portalGun3 = CustomObjectData.newObject("PortalGun3Id", greenPortalGunTexture, Color.White, "Green Portal Gun",
                "Property of Aperture Science Inc.", 0, "", "Basic", 1, -300, "", craftingData: new CraftingData("Green Portal Gun", "335 5 768 5 769 5"));

            Texture2D orangePortalGunTexture = this.Helper.Content.Load<Texture2D>("PortalGun4.png");
            portalGun4 = CustomObjectData.newObject("PortalGunId4", orangePortalGunTexture, Color.White, "Orange Portal Gun",
                "Property of Aperture Science Inc.", 0, "", "Basic", 1, -300, "", craftingData: new CraftingData("Orange Portal Gun", "335 5 768 5 769 5"));
        }

        private void LoadPortalTextures()
        {
            // get animated portal tilesheet file
            string tileSheetPath = this.Helper.Content.GetActualAssetKey("PortalsAnimated3.png", ContentSource.ModFolder);
            foreach (GameLocation location in GetLocations())
            {
                // Add the tilesheet.
                TileSheet tileSheet = new TileSheet(
                   id: "z_portal-spritesheet", // a unique ID for the tilesheet
                   map: location.map,
                   imageSource: tileSheetPath,
                   sheetSize: new xTile.Dimensions.Size(800, 16), // the pixel size of your tilesheet image.
                   tileSize: new xTile.Dimensions.Size(16, 16) // should always be 16x16 for maps
                );
                location.map.AddTileSheet(tileSheet);
                location.map.LoadTileSheets(Game1.mapDisplayDevice);
            }
        }

        private void LoadMinePortals(object sender, EventArgs e)
        {
            string tileSheetPath = this.Helper.Content.GetActualAssetKey("PortalsAnimated3.png", ContentSource.ModFolder);
            int mineLevel = Game1.player.deepestMineLevel;
            for (int i = 1; i <= mineLevel; i++)
            {
                GameLocation location = Game1.getLocationFromName($"UndergroundMine{i}");

                // Add the tilesheet.
                TileSheet tileSheet = new TileSheet(
                   id: "z_portal-spritesheet", // a unique ID for the tilesheet
                   map: location.map,
                   imageSource: tileSheetPath,
                   sheetSize: new xTile.Dimensions.Size(800, 16), // the pixel size of your tilesheet image.
                   tileSize: new xTile.Dimensions.Size(16, 16) // should always be 16x16 for maps
                );
                location.map.AddTileSheet(tileSheet);
                location.map.LoadTileSheets(Game1.mapDisplayDevice);
            }

            // Reload Portal Locations
            for (int i = 0; i < NUM_OF_PORTALS; i++)
            {
                if (PortalLocations.portalLocations[i].exists)
                {
                    // even number targets are i + 1
                    if (i % 2 == 0)
                    {
                        this.SetPortalLocation(i, i + 1, PortalLocations.portalLocations[i]);
                    }
                    // odd number targets are i - 1
                    else
                    {
                        this.SetPortalLocation(i, i - 1, PortalLocations.portalLocations[i]);
                    }
                }
            }
        }

        private void LoadPortalSaves()
        {
            // Reads or Creates a Portal Gun save data file
            LocationSaveFileName =
                $"{this.Helper.DirectoryPath}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}{Constants.SaveFolderName}.json";

            if (File.Exists(LocationSaveFileName))
            {
                PortalLocations = this.Helper.ReadJsonFile<NewPortalLocations>(LocationSaveFileName);
                for (int i = 0; i < NUM_OF_PORTALS; i++)
                {
                    if (PortalLocations.portalLocations[i].exists)
                    {
                        if (i % 2 == 0)
                        {
                            this.SetPortalLocation(i, i + 1, PortalLocations.portalLocations[i]);
                        }
                        else
                        {
                            this.SetPortalLocation(i, i - 1, PortalLocations.portalLocations[i]);
                        }
                    }
                }
            }
            else
            {
                PortalLocations = new NewPortalLocations();
                // Initialize portalLocation array with default items
                for (int i = 0; i < NUM_OF_PORTALS; i++)
                {
                    PortalLocations.portalLocations[i] = new PortalLocation();
                }
                this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);
            }
        }

        private void KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            // Retract portals of current portal gun
            if (e.KeyPressed.ToString().ToLower() == this.config.RetractPortals.ToLower())
            {
                int portalIndex = GetPortalIndex();
                if (portalIndex == -1)
                {
                    return;
                }
                else
                {
                    // Remove the tiles
                    RemovePortalTile(portalIndex);
                    RemovePortalTile(portalIndex + 1);
                    // Remove the warps
                    RemoveCurrentPortalWarps(portalIndex, portalIndex + 1);
                    // Reset the portalLocations
                    PortalLocations.portalLocations[portalIndex] = new PortalLocation();
                    PortalLocations.portalLocations[portalIndex + 1] = new PortalLocation();
                    // Reset the warpLocations
                    PortalWarpLocations[portalIndex] = null;
                    PortalWarpLocations[portalIndex + 1] = null;
                    // Reset the oldTiles
                    OldTiles[portalIndex] = null;
                    OldTiles[portalIndex + 1] = null;
                    // Save portal status
                    this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);

                    // Animation for some indication other than sound
                    Game1.switchToolAnimation();

                    Game1.currentLocation.playSound("serpentDie");
                }
            }
        }

        // For placing a new portal
        private void ButtonPressed(object sender, EventArgsInput e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }
  
            if (Game1.menuUp)
            {
                //Game1.showGlobalMessage("menuUp");
                return;
            }

            if (Game1.activeClickableMenu != null)
            {
                //Game1.showGlobalMessage("active clickable menu");
                return;
            }

            if (e.IsUseToolButton)
            {
                PortalSpawner(sender, e);
                return;
            }
            else if (e.IsActionButton)
            {
                PortalSpawner(sender, e);
                return;
            }
        }

        private void PortalSpawner(object sender, EventArgsInput e)
        {
            if (Game1.isFestival())
            {
                return;
            }
            // Checks for a portal gun
            if (Game1.player.ActiveObject == null)
            {
                return;
            }
            string itemName = Game1.player.CurrentItem.DisplayName;
            
            if (itemName != "Portal Gun" && itemName != "Blue Portal Gun" && itemName != "Green Portal Gun" && itemName != "Orange Portal Gun" && itemName != "Portal Gun Potato")
            {
                return;
            }

            var location = this.GetPortalLocation();

            if (!Game1.getLocationFromName(location.locationName).isTileLocationTotallyClearAndPlaceable(location.xCoord, location.yCoord)
                || !Game1.getLocationFromName(location.locationName).isTileLocationTotallyClearAndPlaceable(location.xCoord + 1, location.yCoord))
            {
                //Game1.showGlobalMessage("Tile location is not totally clear and placeable");

                // Animation for some indication other than sound
                Game1.switchToolAnimation();

                Game1.currentLocation.playSound("serpentHit");
                return;
            }

            // if the new location is equal to any existing portalLocation
            for (int i = 0; i < NUM_OF_PORTALS; i++)
            {
                if (location == PortalLocations.portalLocations[i])
                {
                    // Animation for some indication other than sound
                    Game1.switchToolAnimation();

                    Game1.currentLocation.playSound("serpentHit");
                    return;
                }
            }
            //Game1.showGlobalMessage($"{Game1.currentLocation.Name}");

            int newPortalIndex = GetPortalIndex();
            if (newPortalIndex == -1)
            {
                return;
            }
            else
            {
                if (e.IsUseToolButton)
                {
                    
                    this.SetPortalLocation(newPortalIndex, newPortalIndex + 1, location);
                    this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);
                }
                else if (e.IsActionButton)
                {
                    this.SetPortalLocation(newPortalIndex + 1, newPortalIndex, location);
                    this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);
                }
                else
                {
                    return;
                }
                // Animation for some indication other than sound
                Game1.switchToolAnimation();

                // Play sounds only when a new portalLocation is set
                Game1.currentLocation.playSound("debuffSpell");
            }
        }

        // Returns the index of the portal based on keyPressed and itemBeingHeld
        private int GetPortalIndex()
        {
            // Checks which PortalGun is being used
            if (Game1.player.CurrentItem.Name == null)
            {
                return -1;
            }
            else if (Game1.player.CurrentItem.Name == "Portal Gun")
            {
                return 0;
            }
            else if (Game1.player.CurrentItem.Name == "Portal Gun Potato")
            {
                return 2;
            }
            else if (Game1.player.CurrentItem.Name == "Green Portal Gun")
            {
                return 4;
            }
            else if (Game1.player.CurrentItem.Name == "Orange Portal Gun")
            {
                return 6;
            }
            else
            {
                return -1;
            }
        }

        private void SetPortalLocation(int newIndex, int targetIndex, PortalLocation newLocation)
        {
            // portalLocations[newIndex] is not yet equal to newLocation
            RemovePortalTile(newIndex);

            // portalLocations[newIndex] is now equal to the newLocation
            PortalLocations.portalLocations[newIndex] = newLocation;

            // Save old tile
            Layer layer = Game1.getLocationFromName(PortalLocations.portalLocations[newIndex].locationName).map.GetLayer("Buildings");
            OldTiles[newIndex] = layer.Tiles[PortalLocations.portalLocations[newIndex].xCoord, PortalLocations.portalLocations[newIndex].yCoord];

            // Start the portalAnimation
            PortalAnimationFrame[newIndex] = 0;

            // If the corresponding portal exists, set and replace warps
            if (PortalLocations.portalLocations[targetIndex].exists)
            {
                RemoveCurrentPortalWarps(newIndex, targetIndex);
                CreatePortalWarp(newIndex, targetIndex);
                CreatePortalWarp(targetIndex, newIndex);
            }
        }

        private PortalLocation GetPortalLocation()
        {
            return new PortalLocation(Game1.currentLocation.Name, (int)Game1.currentCursorTile.X,
               (int)Game1.currentCursorTile.Y, true);
        }

        private void RemovePortalTile(int index)
        {
            if (PortalLocations.portalLocations[index].exists)
            {
                // remove portal tile
                Game1.getLocationFromName(PortalLocations.portalLocations[index].locationName).removeTile(PortalLocations.portalLocations[index].xCoord,
                    PortalLocations.portalLocations[index].yCoord, "Buildings");

                // replace old tile
                Layer layerOld = Game1.getLocationFromName(PortalLocations.portalLocations[index].locationName).map.GetLayer("Buildings");
                layerOld.Tiles[PortalLocations.portalLocations[index].xCoord, PortalLocations.portalLocations[index].yCoord] = OldTiles[index];
            }
        }

        private void UpdatePortalAnimationFrames(object sender, EventArgs e)
        {
            // For every portal
            for (int i = 0; i < NUM_OF_PORTALS; i++)
            {
                // If it  is not the value
                if (PortalAnimationFrame[i] > -1)
                {
                    // If it's not on the last animation frame
                    if (PortalAnimationFrame[i] < 4)
                    {
                        // Increase frame
                        ++PortalAnimationFrame[i];

                        // Remove old tile
                        Game1.getLocationFromName(PortalLocations.portalLocations[i].locationName).removeTile(PortalLocations.portalLocations[i].xCoord,
                            PortalLocations.portalLocations[i].yCoord, "Buildings");
                        
                        // Place new tile
                        Layer layer = Game1.getLocationFromName(PortalLocations.portalLocations[i].locationName).map.GetLayer("Buildings");
                        TileSheet tileSheet = Game1.getLocationFromName(PortalLocations.portalLocations[i].locationName).map.GetTileSheet("z_portal-spritesheet");
                        layer.Tiles[PortalLocations.portalLocations[i].xCoord, PortalLocations.portalLocations[i].yCoord] = new StaticTile(layer, tileSheet, BlendMode.Alpha, PortalAnimationFrame[i] + i * 5);
                    }
                    // If it is on the last animation frame
                    else
                    {
                        // Set frame value to default value
                        PortalAnimationFrame[i] = -1;
                    }
                }
            }
        }

        private void RemoveCurrentPortalWarps(int newIndex, int targetIndex)
        {
            // if one exists, they both exist
            if (PortalWarpLocations[newIndex] != null)
            {
                // Remove both corresponding portalWarps
                Game1.getLocationFromName(PortalWarpLocations[targetIndex].TargetName).warps.Remove(PortalWarpLocations[newIndex]);
                Game1.getLocationFromName(PortalWarpLocations[newIndex].TargetName).warps.Remove(PortalWarpLocations[targetIndex]);
            }
        }

        private void CreatePortalWarp(int newIndex, int targetIndex)
        {
            // Save new warp
            PortalWarpLocations[newIndex] = new Warp(PortalLocations.portalLocations[newIndex].xCoord,
                PortalLocations.portalLocations[newIndex].yCoord, PortalLocations.portalLocations[targetIndex].locationName,
                PortalLocations.portalLocations[targetIndex].xCoord + 1, PortalLocations.portalLocations[targetIndex].yCoord, false);

            // Add new Warp
            Game1.getLocationFromName(PortalLocations.portalLocations[newIndex].locationName).warps.Add(PortalWarpLocations[newIndex]);
        }
    }
}