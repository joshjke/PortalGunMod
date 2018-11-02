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
        private static string LocationSaveFileName;

        internal static NewPortalLocations PortalLocations;

        public Warp[] portalWarpLocations { get; set; } = new Warp[10] { null, null, null, null, null, null, null, null, null, null };

        public Tile[] OldTiles { get; set; } = new Tile[10] { null, null, null, null, null, null, null, null, null, null };

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

        private ModConfig config;

        public CustomObjectData portalGun1;
        public CustomObjectData portalGun2;
        public CustomObjectData portalGun3;
        public CustomObjectData portalGun4;
        public CustomObjectData portalGun1Potato;

        

        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += this.AfterLoad;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;


            this.config = helper.ReadConfig<ModConfig>();
            Directory.CreateDirectory(
                $"{this.Helper.DirectoryPath}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}");

            ControlEvents.KeyPressed += this.KeyPressed;
            GameEvents.SecondUpdateTick += GameEvents_SecondUpdateTick;
            MineEvents.MineLevelChanged += ReloadMinePortals;
        }

        
        private void AfterLoad(object sender, EventArgs e)
        {
            // make portalGun objects
            Texture2D portalGun1Texture = this.Helper.Content.Load<Texture2D>("PortalGun1.png");
            portalGun1 = CustomObjectData.newObject("PortalGun1Id", portalGun1Texture, Color.White, "Portal Gun1",
                "Property of Aperture Science Inc.", 0, "", "Basic", 1, -300, "", craftingData: new CraftingData("Portal Gun", "388 1"));

            /* Texture2D portalGun2Texture = this.Helper.Content.Load<Texture2D>("PortalGun2.png");
            portalGun2 = CustomObjectData.newObject("PortalGun2Id", portalGun2Texture, Color.White, "Portal Gun2",
                "Property of Aperture Science Inc.", 0, "", "Basic", 1, -300, "", craftingData: new CraftingData("Portal Gun2", "388 1")); */

            Texture2D portalGun3Texture = this.Helper.Content.Load<Texture2D>("PortalGun3.png");
            portalGun3 = CustomObjectData.newObject("PortalGun3Id", portalGun3Texture, Color.White, "Portal Gun3",
                "Property of Aperture Science Inc.", 0, "", "Basic", 1, -300, "", craftingData: new CraftingData("Portal Gun3", "388 1"));

            Texture2D portalGun4Texture = this.Helper.Content.Load<Texture2D>("PortalGun4.png");
            portalGun4 = CustomObjectData.newObject("PortalGunId4", portalGun4Texture, Color.White, "Portal Gun4",
                "Property of Aperture Science Inc.", 0, "", "Basic", 1, -300, "", craftingData: new CraftingData("Portal Gun4", "388 1"));

            Texture2D portalGun1PotatoTexture = this.Helper.Content.Load<Texture2D>("PortalGun1Potato.png");
            portalGun1Potato = CustomObjectData.newObject("PortalGun1PotatoId", portalGun1PotatoTexture, Color.White, "Portal Gun1 Potato",
                "Oh no, it's you", 0, "", "Basic", 1, -300, "", craftingData: new CraftingData("Portal Gun1 Potato", "388 1"));

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

            // Reads or Creates a Portal Gun save data file
            LocationSaveFileName =
                $"{this.Helper.DirectoryPath}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}{Constants.SaveFolderName}.json";

            if (File.Exists(LocationSaveFileName))
            {
                PortalLocations = this.Helper.ReadJsonFile<NewPortalLocations>(LocationSaveFileName);
                for (int i = 0; i < 10; i++)
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
                // initialize portalLocation array with default items
                for (int i = 0; i < 10; i++)
                {
                    PortalLocations.portalLocations[i] = new PortalLocation();
                }
                this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);
            }
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            // skip if save not loaded yet
            if (!Context.IsWorldReady)
                return;
        }

        //public int BlueAnimationFrame = -1;
        //public int OrangeAnimationFrame = 4;
        public int[] portalAnimationFrame = new int[10] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1};


        private void GameEvents_SecondUpdateTick(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                if (portalAnimationFrame[i] > -1)
                {
                    if (portalAnimationFrame[i] < 4)
                    {
                        ++portalAnimationFrame[i];
                        // remove old tile
                        Game1.getLocationFromName(PortalLocations.portalLocations[i].locationName).removeTile(PortalLocations.portalLocations[i].xCoord,
                            PortalLocations.portalLocations[i].yCoord, "Buildings");

                        Layer layer = Game1.getLocationFromName(PortalLocations.portalLocations[i].locationName).map.GetLayer("Buildings");
                        TileSheet tileSheet = Game1.getLocationFromName(PortalLocations.portalLocations[i].locationName).map.GetTileSheet("z_portal-spritesheet");
                        layer.Tiles[PortalLocations.portalLocations[i].xCoord, PortalLocations.portalLocations[i].yCoord] = new StaticTile(layer, tileSheet, BlendMode.Alpha, portalAnimationFrame[i] + i * 5);
                    }
                    else
                    {
                        portalAnimationFrame[i] = -1;
                    }
                }
            }
        }

        private void ReloadMinePortals(object sender, EventArgs e)
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
            for (int i = 0; i < 10; i++)
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

        /**
         * Handles pressed keys in order to save new warp locations.
         **/

        private void KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.KeyPressed.ToString().ToLower() == this.config.SpawnPortalGun.ToLower())
            {

                return;
            }

            /* if (e.KeyPressed.ToString().ToLower() != this.config.BluePortalSpawnKey.ToLower()
                && e.KeyPressed.ToString().ToLower() != this.config.OrangePortalSpawnKey.ToLower())
                return; */
            string itemName = Game1.player.CurrentItem.DisplayName;
            if (itemName != "Portal Gun1" && itemName != "Portal Gun2" && itemName != "Portal Gun3" && itemName != "Portal Gun4" && itemName != "Portal Gun1 Potato" )
            {
                return;
            }

            var location = this.GetPortalLocation();

            if (!Game1.getLocationFromName(location.locationName).isTileLocationTotallyClearAndPlaceable(location.xCoord, location.yCoord))
            {
                //Game1.showGlobalMessage("Tile location is not totally clear and placeable");
                return;
            }
            Game1.showGlobalMessage($"{location.locationName}");


            for (int i = 0; i < 10; i++)
            {
                if (location == PortalLocations.portalLocations[i])
                {
                    return;
                }
            }

            /* if (Game1.getLocationFromName(location.locationName).waterTiles[location.xCoord + 1, location.yCoord])
            {
                return;
            } */

            // Game1.player

            if (Game1.player.CurrentItem.Name == "Portal Gun1")
            {
                if (e.KeyPressed.ToString().ToLower() == this.config.Portal1SpawnKey.ToLower())
                {
                    this.SetPortalLocation(0, 1, location);
                    Game1.currentLocation.playSound("debuffSpell");
                    this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);

                }

                else if (e.KeyPressed.ToString().ToLower() == this.config.Portal2SpawnKey.ToLower())
                {
                    this.SetPortalLocation(1, 0, location);
                    Game1.currentLocation.playSound("debuffSpell");
                    this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);
                }

            }
            else if (Game1.player.CurrentItem.Name == "Portal Gun1 Potato")
            {
                if (e.KeyPressed.ToString().ToLower() == this.config.Portal1SpawnKey.ToLower())
                {
                    this.SetPortalLocation(2, 3, location);
                    Game1.currentLocation.playSound("debuffSpell");
                    this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);

                }

                else if (e.KeyPressed.ToString().ToLower() == this.config.Portal2SpawnKey.ToLower())
                {
                    this.SetPortalLocation(3, 2, location);
                    Game1.currentLocation.playSound("debuffSpell");
                    this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);
                }
            }
            else if (Game1.player.CurrentItem.Name == "Portal Gun3")
            {
                if (e.KeyPressed.ToString().ToLower() == this.config.Portal1SpawnKey.ToLower())
                {
                    this.SetPortalLocation(4, 5, location);
                    Game1.currentLocation.playSound("debuffSpell");
                    this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);

                }

                else if (e.KeyPressed.ToString().ToLower() == this.config.Portal2SpawnKey.ToLower())
                {
                    this.SetPortalLocation(5, 4, location);
                    Game1.currentLocation.playSound("debuffSpell");
                    this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);
                }
            }
            else if (Game1.player.CurrentItem.Name == "Portal Gun4")
            {
                if (e.KeyPressed.ToString().ToLower() == this.config.Portal1SpawnKey.ToLower())
                {
                    this.SetPortalLocation(6, 7, location);
                    Game1.currentLocation.playSound("debuffSpell");
                    this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);

                }

                else if (e.KeyPressed.ToString().ToLower() == this.config.Portal2SpawnKey.ToLower())
                {
                    this.SetPortalLocation(7, 6, location);
                    Game1.currentLocation.playSound("debuffSpell");
                    this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);
                }
            }


        }

        private void SetPortalLocation(int newIndex, int targetIndex, PortalLocation newLocation)
        {
            if (PortalLocations.portalLocations[newIndex].exists)
            {
                // remove old tile
                Game1.getLocationFromName(PortalLocations.portalLocations[newIndex].locationName).removeTile(PortalLocations.portalLocations[newIndex].xCoord,
                    PortalLocations.portalLocations[newIndex].yCoord, "Buildings");

                // replace old tile
                Layer layerOld = Game1.getLocationFromName(PortalLocations.portalLocations[newIndex].locationName).map.GetLayer("Buildings");
                layerOld.Tiles[PortalLocations.portalLocations[newIndex].xCoord, PortalLocations.portalLocations[newIndex].yCoord] = OldTiles[newIndex];
            }

            PortalLocations.portalLocations[newIndex] = newLocation;

            // save old tile
            Layer layer = Game1.getLocationFromName(PortalLocations.portalLocations[newIndex].locationName).map.GetLayer("Buildings");
           OldTiles[newIndex] = layer.Tiles[PortalLocations.portalLocations[newIndex].xCoord, PortalLocations.portalLocations[newIndex].yCoord];

            portalAnimationFrame[newIndex] = 0;

            // if the other portal exists, set / replace warps
            if (PortalLocations.portalLocations[targetIndex].exists)
            {
                if (portalWarpLocations[newIndex] != null)
                {
                    Game1.getLocationFromName(portalWarpLocations[targetIndex].TargetName).warps.Remove(portalWarpLocations[newIndex]);
                }

                portalWarpLocations[newIndex] = new Warp(PortalLocations.portalLocations[newIndex].xCoord,
                    PortalLocations.portalLocations[newIndex].yCoord, PortalLocations.portalLocations[targetIndex].locationName,
                    PortalLocations.portalLocations[targetIndex].xCoord + 1, PortalLocations.portalLocations[targetIndex].yCoord, false);

                Game1.getLocationFromName(PortalLocations.portalLocations[newIndex].locationName).warps.Add(portalWarpLocations[newIndex]);

                if (portalWarpLocations[targetIndex] != null)
                {
                    Game1.getLocationFromName(portalWarpLocations[newIndex].TargetName).warps.Remove(portalWarpLocations[targetIndex]);
                }

                portalWarpLocations[targetIndex] = new Warp(PortalLocations.portalLocations[targetIndex].xCoord,
                    PortalLocations.portalLocations[targetIndex].yCoord, PortalLocations.portalLocations[newIndex].locationName,
                    PortalLocations.portalLocations[newIndex].xCoord + 1, PortalLocations.portalLocations[newIndex].yCoord, false);

                Game1.getLocationFromName(PortalLocations.portalLocations[targetIndex].locationName).warps.Add(portalWarpLocations[targetIndex]);
                
            }
        }


        private PortalLocation GetPortalLocation()
        {
            return new PortalLocation(Game1.currentLocation.Name, (int)Game1.currentCursorTile.X,
               (int)Game1.currentCursorTile.Y, true);
        }
    }

}