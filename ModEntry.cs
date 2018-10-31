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

        public CustomObjectData portalGun;

        public int BlueAnimationFrame = -1;
        public int OrangeAnimationFrame = 4;

        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += this.AfterLoad;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;


            this.config = helper.ReadConfig<ModConfig>();
            Directory.CreateDirectory(
                $"{this.Helper.DirectoryPath}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}");


            //GameEvents.EighthUpdateTick += this.InterceptWarps;
            ControlEvents.KeyPressed += this.KeyPressed;
            GameEvents.SecondUpdateTick += GameEvents_SecondUpdateTick;


        }

        /**
         * Reads/Creates a warp location save data file after the game loads.
         **/
        private void AfterLoad(object sender, EventArgs e)
        {
            Texture2D portalGunTexture = this.Helper.Content.Load<Texture2D>("PortalGun1.png");

            CustomObjectData portalGun = CustomObjectData.newObject("PortalGunId", portalGunTexture, Color.White, "Portal Gun",
                "Property of Aperture Science Inc.", 0, "", "Basic", 1, -300, "", craftingData: new CraftingData("Portal Gun", "388 1"));

            LocationSaveFileName =
                $"{this.Helper.DirectoryPath}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}{Constants.SaveFolderName}.json";

            if (File.Exists(LocationSaveFileName))
            {
                PortalLocations = this.Helper.ReadJsonFile<NewPortalLocations>(LocationSaveFileName);
                //this.ValidatePortalLocations(PortalLocations);
            }
            else
            {
                PortalLocations = new NewPortalLocations();
            }

            this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations);

            string tileSheetPath = this.Helper.Content.GetActualAssetKey("PortalsAnimated.png", ContentSource.ModFolder);

            // Get an instance of the in-game location you want to patch. For the farm, use Game1.getFarm() instead.
            //GameLocation location = Game1.getLocationFromName("Town");

            foreach (GameLocation location in GetLocations())
            {
                // Add the tilesheet.
                TileSheet tileSheet = new TileSheet(
                   id: "z_portal-spritesheet", // a unique ID for the tilesheet
                   map: location.map,
                   imageSource: tileSheetPath,
                   sheetSize: new xTile.Dimensions.Size(160, 16), // the pixel size of your tilesheet image.
                   tileSize: new xTile.Dimensions.Size(16, 16) // should always be 16x16 for maps
                );
                location.map.AddTileSheet(tileSheet);
                location.map.LoadTileSheets(Game1.mapDisplayDevice);

            }

            PortalLocations.portalLocations[0] = new PortalLocation();
            PortalLocations.portalLocations[1] = new PortalLocation();


        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            // skip if save not loaded yet
            if (!Context.IsWorldReady)
                return;


        }

        private void GameEvents_SecondUpdateTick(object sender, EventArgs e)
        {
            if (BlueAnimationFrame > -1)
            {
                // remove old tile
                Game1.getLocationFromName(PortalLocations.portalLocations[0].locationName).removeTile(PortalLocations.portalLocations[0].xCoord,
                    PortalLocations.portalLocations[0].yCoord, "Buildings");

                Layer layer = Game1.getLocationFromName(PortalLocations.portalLocations[0].locationName).map.GetLayer("Buildings");
                TileSheet tileSheet = Game1.getLocationFromName(PortalLocations.portalLocations[0].locationName).map.GetTileSheet("z_portal-spritesheet");
                layer.Tiles[PortalLocations.portalLocations[0].xCoord, PortalLocations.portalLocations[0].yCoord] = new StaticTile(layer, tileSheet, BlendMode.Alpha, BlueAnimationFrame);

                if (BlueAnimationFrame != 4)
                {
                    ++BlueAnimationFrame;
                }
            }
            if (OrangeAnimationFrame > 4)
            {
                // remove old tile
                Game1.getLocationFromName(PortalLocations.portalLocations[1].locationName).removeTile(PortalLocations.portalLocations[1].xCoord,
                    PortalLocations.portalLocations[1].yCoord, "Buildings");

                Layer layer = Game1.getLocationFromName(PortalLocations.portalLocations[1].locationName).map.GetLayer("Buildings");
                TileSheet tileSheet = Game1.getLocationFromName(PortalLocations.portalLocations[1].locationName).map.GetTileSheet("z_portal-spritesheet");
                layer.Tiles[PortalLocations.portalLocations[1].xCoord, PortalLocations.portalLocations[1].yCoord] = new StaticTile(layer, tileSheet, BlendMode.Alpha, OrangeAnimationFrame);

                if (OrangeAnimationFrame != 9)
                {
                    ++OrangeAnimationFrame;
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

            if (e.KeyPressed.ToString().ToLower() != this.config.BluePortalSpawnKey.ToLower()
                && e.KeyPressed.ToString().ToLower() != this.config.OrangePortalSpawnKey.ToLower())
                return;

            /* if (!AllowedPortalLocations.Contains(Game1.currentLocation.Name))
            {
                Game1.showGlobalMessage("Your portal gun doesn't seem to work in this location");
                return;
            } */

            if (Game1.player.ActiveObject.DisplayName != "Portal Gun")
            {
                return;
            }

            var location = this.GetPortalLocation();

            if (location == PortalLocations.portalLocations[0] || location == PortalLocations.portalLocations[1])
            {
                return;
            }

            /* if (Game1.getLocationFromName(location.locationName).waterTiles[location.xCoord + 1, location.yCoord])
            {
                return;
            } */

            // Game1.player


            if (e.KeyPressed.ToString().ToLower() == this.config.BluePortalSpawnKey.ToLower())
            {
                this.SetPortalLocation(0, 1, location);
            }

            else if (e.KeyPressed.ToString().ToLower() == this.config.OrangePortalSpawnKey.ToLower())
            {
                this.SetPortalLocation(1, 0, location);
            }
            //this.Helper.WriteJsonFile(LocationSaveFileName, PortalLocations); 
            //}

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
                layerOld.Tiles[PortalLocations.portalLocations[newIndex].xCoord, PortalLocations.portalLocations[newIndex].yCoord] = PortalLocations.OldTiles[newIndex];
            }

            PortalLocations.portalLocations[newIndex] = newLocation;

            // save old tile
            Layer layer = Game1.getLocationFromName(PortalLocations.portalLocations[newIndex].locationName).map.GetLayer("Buildings");
            PortalLocations.OldTiles[newIndex] = layer.Tiles[PortalLocations.portalLocations[newIndex].xCoord, PortalLocations.portalLocations[newIndex].yCoord];

            if (newIndex == 0)
            {
                BlueAnimationFrame = 0;
            }

            else if (newIndex == 1)
            {
                OrangeAnimationFrame = 5;
            }

            Game1.showGlobalMessage("New Blue Portal Location Saved");

            if (PortalLocations.portalLocations[targetIndex].exists)
            {
                if (PortalLocations.portalWarpLocations[newIndex] != PortalLocations.portalWarpLocations[2])
                {
                    Game1.getLocationFromName(PortalLocations.portalWarpLocations[targetIndex].TargetName).warps.Remove(PortalLocations.portalWarpLocations[newIndex]);
                }

                PortalLocations.portalWarpLocations[newIndex] = new Warp(PortalLocations.portalLocations[newIndex].xCoord,
                    PortalLocations.portalLocations[newIndex].yCoord, PortalLocations.portalLocations[targetIndex].locationName,
                    PortalLocations.portalLocations[targetIndex].xCoord + 1, PortalLocations.portalLocations[targetIndex].yCoord, false);

                Game1.getLocationFromName(PortalLocations.portalLocations[newIndex].locationName).warps.Add(PortalLocations.portalWarpLocations[newIndex]);

                if (PortalLocations.portalWarpLocations[targetIndex] != PortalLocations.portalWarpLocations[2])
                {
                    Game1.getLocationFromName(PortalLocations.portalWarpLocations[newIndex].TargetName).warps.Remove(PortalLocations.portalWarpLocations[targetIndex]);
                }

                PortalLocations.portalWarpLocations[targetIndex] = new Warp(PortalLocations.portalLocations[targetIndex].xCoord,
                    PortalLocations.portalLocations[targetIndex].yCoord, PortalLocations.portalLocations[newIndex].locationName,
                    PortalLocations.portalLocations[newIndex].xCoord + 1, PortalLocations.portalLocations[newIndex].yCoord, false);

                Game1.getLocationFromName(PortalLocations.portalLocations[targetIndex].locationName).warps.Add(PortalLocations.portalWarpLocations[targetIndex]);
            }
            Game1.currentLocation.playSound("debuffSpell");
        }


        private PortalLocation GetPortalLocation()
        {
            return new PortalLocation(Game1.currentLocation.Name, (int)Game1.currentCursorTile.X,
               (int)Game1.currentCursorTile.Y, true);
        }
    }

}