using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zoo;
using Zoo.Systems.World;
using Zoo.UI;

namespace Zoo.Systems.Landscaping
{
    public class LandscapingTools : MonoBehaviour
    {


        // TO DO:
        /* Raise/Lower/Level terrain based on selection (click and drag for level, brush for others
         * -- allow quarter tile increments?)  ---------COMPLETE OTHER THAN SELECTING IN INTERFACE
         * Raise/Lower individual vertices based on selection (oh god) ------COMPLETE OTHER THAN SELECTING
         * Re-draw chunks as needed for dirty chunks only -------COMPLETE
         * Paint terrain based on click and drag selection ----------COMPLETE OTHER THAN SELECTING
         * Mountain tool (fuck) FUUUUUUUUUUUCK */


        // Set component references
        public World.World world;
        public MainGameUI mainGameUI;

        /// <summary>
        /// When enabled, landscaping functions will work. Should not be enabled unless Landscaping Window is open!
        /// </summary>
        public bool landscapingEnabled = false;

        /// <summary>
        /// When enabled, all terrain modification functions will smooth terrain as they go.
        /// </summary>
        public bool mountainToolEnabled = false;

        /// <summary>
        /// When enabled, terrain modification functions will work.
        /// </summary>
        public bool terrainModificationEnabled = true;

        /// <summary>
        /// When enabled, terrain texture painting will work.
        /// </summary>
        public bool terrainTexturePaintingEnabled = false;

        /// <summary>
        /// When enabled, terrain cliff painting will work.
        /// </summary>
        public bool terrainCliffPaintingEnabled = false;

        /// <summary>
        /// Currently selected terrain texture. Used for terrain texture painting.
        /// </summary>
        public int currentTerrainTexture = 0;
        /// <summary>
        /// Currently selected cliff texture. Used for terrain cliff painting.
        /// </summary>
        public int currentCliffTexture = 1;


        List<Chunk> currentlyDirtyChunks = new List<Chunk>();
        List<Chunk> currentlyDirtyOverlayChunks = new List<Chunk>();

        /// <summary>
        /// Limit on the number of smoothing passes when mountain tool is enabled.
        /// </summary>
        public int mountainToolSmoothingPassLimit = 30;

        /// <summary>
        /// Current mountain tool pass iteration. Should always be set to zero before use, and be checked against the limit.
        /// </summary>
        int mountainToolSmoothingPassIteration = 0;


        public void ToggleLandscapingTools()
        {
            if (!landscapingEnabled)
            {
                landscapingEnabled = true;
            }
            else
            {
                landscapingEnabled = false;
            }
        }

        #region Landscaping Tool Functions

        /// <summary>
        /// Paint provided tiles textures and cliffs based enabling of each type of painting.
        /// This function does not update the tiles, as it can operate separately of terrain modification.
        /// Calling function should only update tiles once all terrain modification & painting is complete.
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="terrainTexture">Index of desired terrain texture.</param>
        /// <param name="cliffTexture">Index of desired cliff texture.</param>
        public void PaintTextures(List<QuarterTile> tiles, int terrainTexture, int cliffTexture)
        {
            // If terrain texture painting is enabled, do so
            if (terrainTexturePaintingEnabled)
            {
                PaintTerrainTextures(tiles, terrainTexture);
            }
            // If terrain cliff painting is enabled, do so
            if (terrainCliffPaintingEnabled)
            {
                PaintCliffTextures(tiles, cliffTexture);
            }
        }

        private void PaintTerrainTextures(List<QuarterTile> tiles, int terrainTexture)
        {

        }

        private void PaintCliffTextures(List<QuarterTile> tiles, int cliffTexture)
        {

        }
        /// <summary>
        /// Levels provided tiles to provided new height. Does not draw tiles. Will smooth tiles if mountain tool is enabled.
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="newHeight"></param>
        public void LevelTerrain(List<QuarterTile> tiles, float newHeight)
        {
            ChangeQuarterTileHeights(tiles, newHeight);
            if (mountainToolEnabled)
            {
                if (tiles.Count > 1)
                {
                    //Debug.Log("Mountain tool enabled with more than 1 tile and we should be smoothing tiles");
                    // Find tiles at edge of selection and smooth them 
                    mountainToolSmoothingPassIteration = 0;
                    SmoothTiles(tiles);
                    DrawCurrentlyDirtyChunks();
                }
                if (tiles.Count == 1)
                {
                    //Debug.Log("Mountain tool enabled with 1 tile and we should be smoothing tiles");
                    // Smooth the tiles around the single tile
                    mountainToolSmoothingPassIteration = 0;
                    SmoothSingleTile(tiles[0]);
                    DrawCurrentlyDirtyChunks();
                }
            }
        }

        /// <summary>
        /// Raises/Lowers provided Tiles to the provided new height, only changing the lowest/highest tiles each time.
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="raising">If true, will raise land. Otherwise, will lower.</param>
        /// <param name="newHeight"></param>
        public void ModifyTerrainHeight(List<QuarterTile> tiles, bool raising, float newHeight)
        {
            List<QuarterTile> tilesToModify;
            float minHeight = GetMinHeightOfTiles(tiles);
            float maxHeight = GetMaxHeightOfTiles(tiles);
            //Debug.Log("Modifying height.");
            //Debug.Log($"min {minHeight}, max {maxHeight}, new {newHeight}");
            // If current selection is not level
            if (minHeight != maxHeight)
            {
                if (raising)
                {
                    tilesToModify = GetLowestTiles(tiles, GetMinHeightOfTiles(tiles));
                }
                else
                {
                    tilesToModify = GetHighestTiles(tiles, GetMaxHeightOfTiles(tiles));
                }
            }
            else
            {
                //Debug.Log("level");
                tilesToModify = tiles;

            }

            if (raising)
            {
                newHeight = minHeight;
                if (SettingsHelper.AdvancedConstructionModeEnabled)
                {
                    newHeight += world.tileHeightStep;
                }
                else
                {
                    newHeight += world.tileHeightStep * 2;
                }
            }
            else
            {
                newHeight = maxHeight;
                if (SettingsHelper.AdvancedConstructionModeEnabled)
                {
                    newHeight -= world.tileHeightStep;
                }
                else
                {
                    newHeight -= world.tileHeightStep * 2;
                }
            }

            ChangeQuarterTileHeights(tilesToModify, newHeight);
            if (mountainToolEnabled)
            {
                if (tilesToModify.Count > 1)
                {
                    //Debug.Log("Mountain tool enabled with more than 1 tile and we should be smoothing tiles");
                    // Find tiles at edge of selection and smooth them 
                    mountainToolSmoothingPassIteration = 0;
                    SmoothTiles(tilesToModify);
                    DrawCurrentlyDirtyChunks();
                }
                if (tilesToModify.Count == 1)
                {
                    //Debug.Log("Mountain tool enabled with 1 tile and we should be smoothing tiles");
                    // Smooth the tiles around the single tile
                    mountainToolSmoothingPassIteration = 0;
                    SmoothSingleTile(tilesToModify[0]);
                    DrawCurrentlyDirtyChunks();
                }
            }
        }
        /// <summary>
        /// Raises/Lowers provided Tile to the provided new height.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="newHeight"></param>
        public void ModifyTerrainHeight(QuarterTile tile, float newHeight)
        {
            ChangeQuarterTileHeights(tile, newHeight);
            if (mountainToolEnabled)
            {
                //Debug.Log("Mountain tool enabled with 1 tile and we should be smoothing tiles");
                // Smooth the tiles around the single tile
                mountainToolSmoothingPassIteration = 0;
                SmoothSingleTile(tile);
                DrawCurrentlyDirtyChunks();
            }
        }
        /// <summary>
        /// Raises/Lowers provided Vertice to the provided new height.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="vertice"></param>
        /// <param name="newHeight"></param>
        public void ModifyTerrainHeight(QuarterTile tile, string vertice, float newHeight)
        {
            ChangeVerticeHeight(tile, vertice, newHeight);
            if (mountainToolEnabled)
            {
                //Debug.Log("Mountain tool enabled with 1 tile and we should be smoothing tiles");
                // Smooth the tiles around the single tile
                mountainToolSmoothingPassIteration = 0;
                SmoothSingleTile(tile);
                DrawCurrentlyDirtyChunks();
            }
        }

        #endregion

        #region Tile Modification Helper Functions
        /// <summary>
        /// [Deprecated] Used to change the height of a full tile's worth of quarter tiles. May not be utilised.
        /// </summary>
        /// <param name="fullTile"></param>
        /// <param name="newHeight"></param>
        private void ChangeFullTileHeight(FullTile fullTile, float newHeight)
        {
            ChangeQuarterTileHeight(fullTile.southWest, newHeight);
            ChangeQuarterTileHeight(fullTile.northWest, newHeight);
            ChangeQuarterTileHeight(fullTile.northEast, newHeight);
            ChangeQuarterTileHeight(fullTile.southEast, newHeight);
        }

        /// <summary>
        /// Change the height of the provided tiles to the provided height.
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="newHeight"></param>
        private void ChangeQuarterTileHeights(List<QuarterTile> tiles, float newHeight)
        {
            foreach (QuarterTile t in tiles)
            {
                ChangeQuarterTileHeight(t, newHeight);
            }
            world.DrawChunks(GetDirtyChunks(tiles));
            world.DrawOverlayChunks(GetDirtyOverlayChunks(tiles));
        }
        private void ChangeQuarterTileHeights(QuarterTile tile, float newHeight)
        {
            ChangeQuarterTileHeight(tile, newHeight);
            world.DrawChunks(world.GetChunkAtTile(tile));
            world.DrawOverlayChunks(world.GetOverlayChunkAtTile(tile));
        }

        /// <summary>
        /// Change the height of the provided tile to the provided height.
        /// </summary>
        /// <param name="quarterTile"></param>
        /// <param name="newHeight"></param>
        private void ChangeQuarterTileHeight(QuarterTile quarterTile, float newHeight)
        {
            quarterTile.height = newHeight;
            quarterTile.Recalculate();
            quarterTile.ReSetStats();
        }

        /// <summary>
        /// Change the height of the given vertice, and change the height of the remaining 3 vertices if necessary.
        /// </summary>
        /// <param name="tile">The tile to which the vertice belongs.</param>
        /// <param name="vertice">Name of the vertice, ie. "LowerLeft"</param>
        /// <param name="newHeight"></param>
        private void ChangeVerticeHeight(QuarterTile tile, string vertice, float newHeight)
        {
            switch (vertice)
            {
                case "LowerLeft":
                    {
                        tile.lowerLeft.y = newHeight;
                        tile.ReSetStats();
                        tile.ValidateVertices(vertice);
                        break;
                    }
                case
                    "UpperLeft":
                    {
                        tile.upperLeft.y = newHeight;
                        tile.ReSetStats();
                        tile.ValidateVertices(vertice);
                        break;
                    }
                case
                    "UpperRight":
                    {
                        tile.upperRight.y = newHeight;
                        tile.ReSetStats();
                        tile.ValidateVertices(vertice);
                        break;
                    }
                case
                    "LowerRight":
                    {
                        tile.lowerRight.y = newHeight;
                        tile.ReSetStats();
                        tile.ValidateVertices(vertice);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            world.DrawChunks(world.GetChunkAtTile(tile));
            world.DrawOverlayChunks(world.GetOverlayChunkAtTile(tile));
        }
        

        public int GetVerticeSelectionOverlayTexture(string verticeName)
        {
            switch (verticeName)
            {
                case "LowerLeft":
                    {
                        return 2;
                    }
                case "UpperLeft":
                    {
                        return 3;
                    }
                case "UpperRight":
                    {
                        return 4;
                    }
                case "LowerRight":
                    {
                        return 5;
                    }
                default:
                    {
                        // Return a full tile overlay texture if we get here.
                        return 1;
                    }

            }
        }

        /// <summary>
        /// Change tile texture of provided tiles to the provided type. Does not update tiles once complete.
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="terrainType"></param>
        private void ChangeTileTerrainTypes(List<QuarterTile> tiles, int terrainType)
        {
            foreach (QuarterTile t in tiles)
            {
                ChangeTileTerrainType(t, terrainType);
            }
        }

        /// <summary>
        /// Change tile texture of provided tiles to the provided type. Does not update tiles once complete.
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="terrainType"></param>
        private void ChangeTileTerrainType(QuarterTile tile, int terrainType)
        {
            tile.TerrainType = terrainType;
        }

        public void ChangeTileOverlayTypes(List<QuarterTile> tiles, int overlayType)
        {
            foreach (QuarterTile t in tiles)
            {
                ChangeTileOverlayType(t, overlayType);
            }
            world.DrawOverlayChunks(GetDirtyOverlayChunks(tiles));
        }

        public void ReturnOverlayTypeToDefault(List<QuarterTile> tiles)
        {
            if (world.gridEnabled)
            {
                foreach (QuarterTile t in tiles)
                {
                    t.OverlayTileType = t.gridType;
                }
            }
            else
            {
                foreach (QuarterTile t in tiles)
                {
                    t.OverlayTileType = 6;
                }
            }
            world.DrawOverlayChunks(GetDirtyOverlayChunks(tiles));
        }

        public void ReturnOverlayTypeToDefault(QuarterTile tile)
        {
            if (world.gridEnabled)
            {
                tile.OverlayTileType = tile.gridType;
            }
            else
            {
                tile.OverlayTileType = 6;
            }
            world.DrawOverlayChunks(world.GetOverlayChunkAtTile(tile));
        }

        private void ChangeTileOverlayType(QuarterTile tile, int overlayType)
        {
            tile.OverlayTileType = overlayType;
        }

        /// <summary>
        /// Change tile cliff texture of provided tile to the provided type. Does not update tile once complete.
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="cliffType"></param>
        public void ChangeTileCliffType(List<QuarterTile> tiles, int cliffType)
        {
            foreach (QuarterTile t in tiles)
            {
                ChangeTileTerrainType(t, cliffType);
            }
            world.DrawChunks(GetDirtyChunks(tiles));
        }

        /// <summary>
        /// Change tile cliff texture of provided tile to the provided type. Does not update tile once complete.
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="cliffType"></param>
        private void ChangeTileCliffType(QuarterTile tile, int cliffType)
        {
            tile.CliffType = cliffType;
        }

        /// <summary>
        /// Return all chunks with tiles in the given list. Used to update only chunks that need updating.
        /// </summary>
        /// <param name="tilesToUpdate"></param>
        /// <returns></returns>
        public List<Chunk> GetDirtyChunks(List<QuarterTile> tilesToUpdate)
        {
            List<Chunk> dirtyChunks = new List<Chunk>();
            foreach (QuarterTile t in tilesToUpdate)
            {
                if (t != null)
                {
                    Chunk activeChunk = world.GetChunkAtTile(t);
                    if (!dirtyChunks.Contains(activeChunk))
                    {
                        dirtyChunks.Add(activeChunk);
                    }
                }
            }

            return dirtyChunks;
        }

        /// <summary>
        /// Return all overlay chunks with tiles in the given list. Used to update only overlay chunks that need updating.
        /// </summary>
        /// <param name="overlayTilesToUpdate"></param>
        /// <returns></returns>
        public List<Chunk> GetDirtyOverlayChunks(List<QuarterTile> overlayTilesToUpdate)
        {
            List<Chunk> dirtyChunks = new List<Chunk>();
            foreach (QuarterTile t in overlayTilesToUpdate)
            {
                if (t != null)
                {
                    Chunk activeChunk = world.GetOverlayChunkAtTile(t);
                    if (!dirtyChunks.Contains(activeChunk))
                    {
                        dirtyChunks.Add(activeChunk);
                    }
                }
            }

            return dirtyChunks;
        }

        private void DrawCurrentlyDirtyChunks()
        {
            if (currentlyDirtyChunks != null)
            {
                world.DrawChunks(currentlyDirtyChunks);
                world.DrawOverlayChunks(currentlyDirtyOverlayChunks);
                currentlyDirtyChunks = null;
                currentlyDirtyOverlayChunks = null;
            }
        }

        /// <summary>
        /// Smooth the provided tile around the provided tiles to match them.
        /// </summary>
        /// <param name="tiles"></param>
        private void SmoothTiles(List<QuarterTile> tiles)
        {
            //Debug.Log("Attempting to smooth tiles");
            // Grab the edge tiles only.
            TileMapMouseInterface.RectangleEdgeTiles edgeTiles = new TileMapMouseInterface.RectangleEdgeTiles(tiles);
            List<QuarterTile> nextEdgeTiles = new List<QuarterTile>(0);

            // Based on the orientation of the tiles, check if their neighbours require smoothing, smooth them if so, 
            // and add them to a new list of edgeTiles to be smoothed afterward.
            // Southwest Tile
            if (edgeTiles.SouthwestTile != null)
            {
                QuarterTile activeTile = edgeTiles.SouthwestTile;
                QuarterTile[] neighbors = world.GetAdjacentQuarterTiles(activeTile);
                if (neighbors[0] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[0], "S"))
                    {
                        SmoothTile(activeTile, neighbors[0], "S");
                        nextEdgeTiles.Add(neighbors[0]);
                    }

                }
                if (neighbors[1] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[1], "SW"))
                    {
                        SmoothTile(activeTile, neighbors[1], "SW");
                        nextEdgeTiles.Add(neighbors[1]);
                    }

                }
                if (neighbors[2] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[2], "W"))
                    {
                        SmoothTile(activeTile, neighbors[2], "W");
                        nextEdgeTiles.Add(neighbors[2]);
                    }

                }
            }
            // Western Tiles
            foreach (QuarterTile activeTile in edgeTiles.WesternEdgeTiles)
            {
                if (activeTile != null)
                {
                    QuarterTile westernTile = world.GetAdjacentQuarterTiles(activeTile)[2];

                    if (westernTile != null)
                    {
                        if (CheckIfTileNeedsSmoothing(activeTile, westernTile, "W"))
                        {
                            SmoothTile(activeTile, westernTile, "W");
                            nextEdgeTiles.Add(westernTile);
                        }

                    }
                }
            }
            // Northwest Tile
            if (edgeTiles.NorthwestTile != null)
            {
                QuarterTile activeTile = edgeTiles.NorthwestTile;
                QuarterTile[] neighbors = world.GetAdjacentQuarterTiles(activeTile);
                if (neighbors[2] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[2], "W"))
                    {
                        SmoothTile(activeTile, neighbors[2], "W");
                        nextEdgeTiles.Add(neighbors[2]);
                    }

                }
                if (neighbors[3] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[3], "NW"))
                    {
                        SmoothTile(activeTile, neighbors[3], "NW");
                        nextEdgeTiles.Add(neighbors[3]);
                    }

                }
                if (neighbors[4] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[4], "N"))
                    {
                        SmoothTile(activeTile, neighbors[4], "N");
                        nextEdgeTiles.Add(neighbors[4]);
                    }

                }
            }
            // Northern Tiles
            foreach (QuarterTile activeTile in edgeTiles.NorthernEdgeTiles)
            {
                if (activeTile != null)
                {
                    QuarterTile northernTile = world.GetAdjacentQuarterTiles(activeTile)[4];

                    if (northernTile != null)
                    {
                        if (CheckIfTileNeedsSmoothing(activeTile, northernTile, "N"))
                        {
                            SmoothTile(activeTile, northernTile, "N");
                            nextEdgeTiles.Add(northernTile);
                        }

                    }
                }
            }
            // Northeast Tile
            if (edgeTiles.NortheastTile != null)
            {
                QuarterTile activeTile = edgeTiles.NortheastTile;
                QuarterTile[] neighbors = world.GetAdjacentQuarterTiles(activeTile);
                if (neighbors[4] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[4], "N"))
                    {
                        SmoothTile(activeTile, neighbors[4], "N");
                        nextEdgeTiles.Add(neighbors[4]);
                    }

                }
                if (neighbors[5] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[5], "NE"))
                    {
                        SmoothTile(activeTile, neighbors[5], "NE");
                        nextEdgeTiles.Add(neighbors[5]);
                    }

                }
                if (neighbors[6] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[6], "E"))
                    {
                        SmoothTile(activeTile, neighbors[6], "E");
                        nextEdgeTiles.Add(neighbors[6]);
                    }

                }
            }
            // Eastern Tiles
            foreach (QuarterTile activeTile in edgeTiles.EasternEdgeTiles)
            {
                if (activeTile != null)
                {
                    QuarterTile easternTile = world.GetAdjacentQuarterTiles(activeTile)[6];

                    if (easternTile != null)
                    {
                        if (CheckIfTileNeedsSmoothing(activeTile, easternTile, "E"))
                        {
                            SmoothTile(activeTile, easternTile, "E");
                            nextEdgeTiles.Add(easternTile);
                        }

                    }
                }
            }
            // Southeast Tile
            if (edgeTiles.SoutheastTile != null)
            {
                QuarterTile activeTile = edgeTiles.SoutheastTile;
                QuarterTile[] neighbors = world.GetAdjacentQuarterTiles(activeTile);
                if (neighbors[6] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[6], "E"))
                    {
                        SmoothTile(activeTile, neighbors[6], "E");
                        nextEdgeTiles.Add(neighbors[6]);
                    }

                }
                if (neighbors[7] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[7], "SE"))
                    {
                        SmoothTile(activeTile, neighbors[7], "SE");
                        nextEdgeTiles.Add(neighbors[7]);
                    }

                }
                if (neighbors[0] != null)
                {
                    if (CheckIfTileNeedsSmoothing(activeTile, neighbors[0], "S"))
                    {
                        SmoothTile(activeTile, neighbors[0], "S");
                        nextEdgeTiles.Add(neighbors[0]);
                    }

                }
            }
            // Southern Tiles
            foreach (QuarterTile activeTile in edgeTiles.SouthernEdgeTiles)
            {
                if (activeTile != null)
                {
                    QuarterTile southernTile = world.GetAdjacentQuarterTiles(activeTile)[0];

                    if (southernTile != null)
                    {
                        if (CheckIfTileNeedsSmoothing(activeTile, southernTile, "S"))
                        {
                            SmoothTile(activeTile, southernTile, "S");
                            nextEdgeTiles.Add(southernTile);
                        }

                    }
                }
            }

            currentlyDirtyChunks = GetDirtyChunks(tiles);
            currentlyDirtyOverlayChunks = GetDirtyOverlayChunks(tiles);

            // If the nextEdgeTiles list is not null, smooth those tiles next, thus repeating the process until no tiles remain to be smoothed.
            if (mountainToolSmoothingPassIteration < mountainToolSmoothingPassLimit && nextEdgeTiles.Count > 0)
            {
                SmoothTiles(nextEdgeTiles);
                mountainToolSmoothingPassIteration++;
            }
        }
        ///// <summary>
        ///// Smooth the provided tile around the provided tiles to match them, using the provided rectangleEdgeTiles.
        ///// </summary>
        ///// <param name="rectangleEdgeTiles"></param>
        //void SmoothTiles(TileMapMouseInterface.RectangleEdgeTiles rectangleEdgeTiles)
        //{
        //    // Grab the edge tiles only.
        //    TileMapMouseInterface.RectangleEdgeTiles edgeTiles = rectangleEdgeTiles;
        //    TileMapMouseInterface.RectangleEdgeTiles nextEdgeTiles = new TileMapMouseInterface.RectangleEdgeTiles();
        //    bool ContinueToNextUpdate = false;
        //    // Based on the orientation of the tiles, check if their neighbours require smoothing, smooth them if so, 
        //    // and add them to a new list of edgeTiles to be smoothed afterward.
        //    // Southwest Tile
        //    if (edgeTiles.SouthwestTile != null)
        //    {
        //        QuarterTile activeTile = edgeTiles.SouthwestTile;
        //        QuarterTile[] neighbors = world.GetAdjacentQuarterTiles(activeTile);
        //        if (neighbors[0] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[0], "S"))
        //            {
        //                SmoothTile(neighbors[0]);
        //                nextEdgeTiles.SouthernEdgeTiles.Add(neighbors[0]);
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //        if (neighbors[1] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[1], "SW"))
        //            {
        //                SmoothTile(neighbors[1]);
        //                nextEdgeTiles.SouthwestTile = neighbors[1];
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //        if (neighbors[2] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[2], "W"))
        //            {
        //                SmoothTile(neighbors[2]);
        //                nextEdgeTiles.WesternEdgeTiles.Add(neighbors[2]);
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //    }
        //    // Western Tiles
        //    if (edgeTiles.WesternEdgeTiles != null)
        //    {
        //        foreach (QuarterTile activeTile in edgeTiles.WesternEdgeTiles)
        //        {
        //            if (activeTile != null)
        //            {
        //                QuarterTile westernTile = world.GetAdjacentQuarterTiles(activeTile)[2];

        //                if (westernTile != null)
        //                {
        //                    if (CheckIfTileNeedsSmoothing(activeTile, westernTile, "W"))
        //                    {
        //                        SmoothTile(westernTile);
        //                        nextEdgeTiles.WesternEdgeTiles.Add(westernTile);
        //                        ContinueToNextUpdate = true;
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    // Northwest Tile
        //    if (edgeTiles.NorthwestTile != null)
        //    {
        //        QuarterTile activeTile = edgeTiles.NorthwestTile;
        //        QuarterTile[] neighbors = world.GetAdjacentQuarterTiles(activeTile);
        //        if (neighbors[2] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[2], "W"))
        //            {
        //                SmoothTile(neighbors[2]);
        //                nextEdgeTiles.WesternEdgeTiles.Add(neighbors[2]);
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //        if (neighbors[3] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[3], "NW"))
        //            {
        //                SmoothTile(neighbors[3]);
        //                nextEdgeTiles.NorthwestTile = neighbors[3];
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //        if (neighbors[4] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[4], "N"))
        //            {
        //                SmoothTile(neighbors[4]);
        //                nextEdgeTiles.NorthernEdgeTiles.Add(neighbors[4]);
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //    }
        //    // Northern Tiles
        //    if (edgeTiles.NorthernEdgeTiles != null)
        //    {
        //        foreach (QuarterTile activeTile in edgeTiles.NorthernEdgeTiles)
        //        {
        //            if (activeTile != null)
        //            {
        //                QuarterTile northernTile = world.GetAdjacentQuarterTiles(activeTile)[4];

        //                if (northernTile != null)
        //                {
        //                    if (CheckIfTileNeedsSmoothing(activeTile, northernTile, "N"))
        //                    {
        //                        SmoothTile(northernTile);
        //                        nextEdgeTiles.NorthernEdgeTiles.Add(northernTile);
        //                        ContinueToNextUpdate = true;
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    // Northeast Tile
        //    if (edgeTiles.NortheastTile != null)
        //    {
        //        QuarterTile activeTile = edgeTiles.NortheastTile;
        //        QuarterTile[] neighbors = world.GetAdjacentQuarterTiles(activeTile);
        //        if (neighbors[4] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[4], "N"))
        //            {
        //                SmoothTile(neighbors[4]);
        //                nextEdgeTiles.NorthernEdgeTiles.Add(neighbors[4]);
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //        if (neighbors[5] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[5], "NE"))
        //            {
        //                SmoothTile(neighbors[5]);
        //                nextEdgeTiles.NortheastTile = neighbors[5];
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //        if (neighbors[6] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[6], "E"))
        //            {
        //                SmoothTile(neighbors[6]);
        //                nextEdgeTiles.EasternEdgeTiles.Add(neighbors[6]);
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //    }
        //    // Eastern Tiles
        //    if (edgeTiles.EasternEdgeTiles != null)
        //    {
        //        foreach (QuarterTile activeTile in edgeTiles.EasternEdgeTiles)
        //        {
        //            if (activeTile != null)
        //            {
        //                QuarterTile easternTile = world.GetAdjacentQuarterTiles(activeTile)[6];

        //                if (easternTile != null)
        //                {
        //                    if (CheckIfTileNeedsSmoothing(activeTile, easternTile, "E"))
        //                    {
        //                        SmoothTile(easternTile);
        //                        nextEdgeTiles.EasternEdgeTiles.Add(easternTile);
        //                        ContinueToNextUpdate = true;
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    // Southeast Tile
        //    if (edgeTiles.SoutheastTile != null)
        //    {
        //        QuarterTile activeTile = edgeTiles.SoutheastTile;
        //        QuarterTile[] neighbors = world.GetAdjacentQuarterTiles(activeTile);
        //        if (neighbors[6] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[6], "E"))
        //            {
        //                SmoothTile(neighbors[6]);
        //                nextEdgeTiles.EasternEdgeTiles.Add(neighbors[6]);
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //        if (neighbors[7] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[7], "SE"))
        //            {
        //                SmoothTile(neighbors[7]);
        //                nextEdgeTiles.SoutheastTile = neighbors[7];
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //        if (neighbors[0] != null)
        //        {
        //            if (CheckIfTileNeedsSmoothing(activeTile, neighbors[0], "S"))
        //            {
        //                SmoothTile(neighbors[0]);
        //                nextEdgeTiles.SouthernEdgeTiles.Add(neighbors[0]);
        //                ContinueToNextUpdate = true;
        //            }

        //        }
        //    }
        //    // Southern Tiles
        //    if (edgeTiles.SoutheastTile != null)
        //    {
        //        foreach (QuarterTile activeTile in edgeTiles.SouthernEdgeTiles)
        //        {
        //            if (activeTile != null)
        //            {
        //                QuarterTile southernTile = world.GetAdjacentQuarterTiles(activeTile)[0];

        //                if (southernTile != null)
        //                {
        //                    if (CheckIfTileNeedsSmoothing(activeTile, southernTile, "S"))
        //                    {
        //                        SmoothTile(southernTile);
        //                        nextEdgeTiles.SouthernEdgeTiles.Add(southernTile);
        //                        ContinueToNextUpdate = true;
        //                    }

        //                }
        //            }
        //        }
        //    }

        //    edgeTiles.RebuildAllEdgeTilesList();
        //    if (ContinueToNextUpdate)
        //    {  
        //        currentlyDirtyChunks.AddRange(GetDirtyChunks(edgeTiles.AllEdgeTiles));
        //        currentlyDirtyOverlayChunks.AddRange(GetDirtyOverlayChunks(edgeTiles.AllEdgeTiles));
        //    }
        //    nextEdgeTiles.RebuildAllEdgeTilesList();
        //    // If the nextEdgeTiles list is not null, smooth those tiles next, thus repeating the process until no tiles remain to be smoothed.
        //    if (ContinueToNextUpdate)
        //    {
        //        SmoothTiles(nextEdgeTiles);
        //    }

        //}

        /// <summary>
        /// Smooth the tiles around the provided tile to match it.
        /// </summary>
        /// <param name="tile"></param>
        private void SmoothSingleTile(QuarterTile tile)
        {
            QuarterTile[] neighbors = world.GetAdjacentQuarterTiles(tile);
            List<QuarterTile> tilesToSmooth = new List<QuarterTile>();

            // Iterate through the neighbors list and smooth the tiles as needed based on their directional relationship to the center tile.
            // Southern tile
            if (neighbors[0] != null)
            {
                if (CheckIfTileNeedsSmoothing(tile, neighbors[0], "S"))
                {
                    SmoothTile(tile, neighbors[0], "S");
                    tilesToSmooth.Add(neighbors[0]);
                }
            }
            // Southwestern tile
            if (neighbors[1] != null)
            {
                if (CheckIfTileNeedsSmoothing(tile, neighbors[1], "SW"))
                {
                    SmoothTile(tile, neighbors[1], "SW");
                    tilesToSmooth.Add(neighbors[1]);
                }
            }
            // Western tile
            if (neighbors[2] != null)
            {
                if (CheckIfTileNeedsSmoothing(tile, neighbors[2], "W"))
                {
                    SmoothTile(tile, neighbors[2], "W");
                    tilesToSmooth.Add(neighbors[2]);
                }
            }
            // Northwestern tile
            if (neighbors[3] != null)
            {
                if (CheckIfTileNeedsSmoothing(tile, neighbors[3], "NW"))
                {
                    SmoothTile(tile, neighbors[3], "NW");
                    tilesToSmooth.Add(neighbors[3]);
                }
            }
            // Northern tile
            if (neighbors[4] != null)
            {
                if (CheckIfTileNeedsSmoothing(tile, neighbors[4], "N"))
                {
                    SmoothTile(tile, neighbors[4], "N");
                    tilesToSmooth.Add(neighbors[4]);
                }
            }
            // Northeastern tile
            if (neighbors[5] != null)
            {
                if (CheckIfTileNeedsSmoothing(tile, neighbors[5], "NE"))
                {
                    SmoothTile(tile, neighbors[5], "NE");
                    tilesToSmooth.Add(neighbors[5]);
                }
            }
            // Eastern tile
            if (neighbors[6] != null)
            {
                if (CheckIfTileNeedsSmoothing(tile, neighbors[6], "E"))
                {
                    SmoothTile(tile, neighbors[6], "E");
                    tilesToSmooth.Add(neighbors[6]);
                }
            }
            // Southeastern tile
            if (neighbors[7] != null)
            {
                if (CheckIfTileNeedsSmoothing(tile, neighbors[7], "SE"))
                {
                    SmoothTile(tile, neighbors[7], "SE");
                    tilesToSmooth.Add(neighbors[7]);
                }
            }
            if (tilesToSmooth.Count > 0)
            {
                SmoothTiles(tilesToSmooth);
            }
        }

        /// <summary>
        /// Smooth the provided tile to match the tiles around it.
        /// </summary>
        void SmoothTile(QuarterTile masterTile, QuarterTile tileToSmooth, string relationToMaster)
        {
            // Check each tile's vertices against the master vertices, based on their directional relationship. 
            // If any vertice heights do not match, adjust them to match the master tile.
            switch (relationToMaster)
            {
                case "S":
                    {
                        if (masterTile.lowerLeft.y != tileToSmooth.upperLeft.y)
                        {
                            tileToSmooth.upperLeft.y = masterTile.lowerLeft.y;
                            tileToSmooth.ValidateVertices("UpperLeft");
                        }
                        if (masterTile.lowerRight.y != tileToSmooth.upperRight.y)
                        {
                            tileToSmooth.upperRight.y = masterTile.lowerRight.y;
                            tileToSmooth.ValidateVertices("UpperRight");
                        }

                        break;
                    }
                case "SW":
                    {
                        if (masterTile.lowerLeft.y != tileToSmooth.upperRight.y)
                        {
                            tileToSmooth.upperRight.y = masterTile.lowerLeft.y;
                            tileToSmooth.ValidateVertices("UpperRight");
                        }
                        break;
                    }
                case "W":
                    {
                        if (masterTile.lowerLeft.y != tileToSmooth.lowerRight.y)
                        {
                            tileToSmooth.lowerRight.y = masterTile.lowerLeft.y;
                            tileToSmooth.ValidateVertices("LowerRight");
                        }
                        if (masterTile.upperLeft.y != tileToSmooth.upperRight.y)
                        {
                            tileToSmooth.upperRight.y = masterTile.upperLeft.y;
                            tileToSmooth.ValidateVertices("UpperRight");
                        }
                        break;
                    }
                case "NW":
                    {
                        if (masterTile.upperLeft.y != tileToSmooth.lowerRight.y)
                        {
                            tileToSmooth.lowerRight.y = masterTile.upperLeft.y;
                            tileToSmooth.ValidateVertices("LowerRight");
                        }
                        break;
                    }
                case "N":
                    {
                        if (masterTile.upperLeft.y != tileToSmooth.lowerLeft.y)
                        {
                            tileToSmooth.lowerLeft.y = masterTile.upperLeft.y;
                            tileToSmooth.ValidateVertices("LowerLeft");
                        }
                        if (masterTile.upperRight.y != tileToSmooth.lowerRight.y)
                        {
                            tileToSmooth.lowerRight.y = masterTile.upperRight.y;
                            tileToSmooth.ValidateVertices("LowerRight");
                        }
                        break;
                    }
                case "NE":
                    {
                        if (masterTile.upperRight.y != tileToSmooth.lowerLeft.y)
                        {
                            tileToSmooth.lowerLeft.y = masterTile.upperRight.y;
                            tileToSmooth.ValidateVertices("LowerLeft");

                        }
                        break;
                    }
                case "E":
                    {
                        if (masterTile.upperRight.y != tileToSmooth.upperLeft.y)
                        {
                            tileToSmooth.upperLeft.y = masterTile.upperRight.y;
                            tileToSmooth.ValidateVertices("UpperLeft");
                        }
                        if (masterTile.lowerRight.y != tileToSmooth.lowerLeft.y)
                        {
                            tileToSmooth.lowerLeft.y = masterTile.lowerRight.y;
                            tileToSmooth.ValidateVertices("LowerLeft");
                        }
                        break;
                    }
                case "SE":
                    {
                        if (masterTile.lowerRight.y != tileToSmooth.upperLeft.y)
                        {
                            tileToSmooth.upperLeft.y = masterTile.lowerRight.y;
                            tileToSmooth.ValidateVertices("UpperLeft");
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Return true if the height of the masterTile does not match that of the tileToSmooth, based on the provided directional relationToMaster
        /// in terms of S, SW, W, etc.
        /// </summary>
        /// <param name="masterTile"></param>
        /// <param name="tileToSmooth"></param>
        /// <param name="relationToMaster"></param>
        /// <returns></returns>
        bool CheckIfTileNeedsSmoothing(QuarterTile masterTile, QuarterTile tileToSmooth, string relationToMaster)
        {
            // Check each tile's vertices against the master vertices, based on their directional relationship. 
            // If any vertice heights do not match, return true.
            switch (relationToMaster)
            {
                case "S":
                    {
                        if (masterTile.lowerLeft.y != tileToSmooth.upperLeft.y || masterTile.lowerRight.y != tileToSmooth.upperRight.y)
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                case "SW":
                    {
                        if (masterTile.lowerLeft.y != tileToSmooth.upperRight.y)
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                case "W":
                    {
                        if (masterTile.lowerLeft.y != tileToSmooth.lowerRight.y || masterTile.upperLeft.y != tileToSmooth.upperRight.y)
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                case "NW":
                    {
                        if (masterTile.upperLeft.y != tileToSmooth.lowerRight.y)
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                case "N":
                    {
                        if (masterTile.upperLeft.y != tileToSmooth.lowerLeft.y || masterTile.upperRight.y != tileToSmooth.lowerRight.y)
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                case "NE":
                    {
                        if (masterTile.upperRight.y != tileToSmooth.lowerLeft.y)
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                case "E":
                    {
                        if (masterTile.upperRight.y != tileToSmooth.upperLeft.y || masterTile.lowerRight.y != tileToSmooth.lowerLeft.y)
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                case "SE":
                    {
                        if (masterTile.lowerRight.y != tileToSmooth.upperLeft.y)
                        {
                            return true;
                        }
                        else
                            return false;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        /// <summary>
        /// Return the lowest tiles in a list.
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns></returns>
        List<QuarterTile> GetLowestTiles(List<QuarterTile> tiles, float minHeightOfTiles)
        {
            List<QuarterTile> lowTiles;
            lowTiles = tiles.FindAll(t => t.height == minHeightOfTiles || (t.isSlope && t.height == minHeightOfTiles + world.tileHeightStep));
            return lowTiles;
        }

        float GetMinHeightOfTiles(List<QuarterTile> tiles)
        {
            float minHeight = tiles[0].height;
            foreach (QuarterTile t in tiles)
            {
                if (t.height < minHeight)
                {
                    minHeight = t.height;
                }
            }
            return minHeight;
        }

        /// <summary>
        /// Return the highest tiles in a list.
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns></returns>
        List<QuarterTile> GetHighestTiles(List<QuarterTile> tiles, float maxHeightOfTiles)
        {
            List<QuarterTile> highTiles = new List<QuarterTile>();
            highTiles = tiles.FindAll(t => t.height == maxHeightOfTiles || (t.isSlope && t.height == maxHeightOfTiles - world.tileHeightStep));
            return highTiles;
        }

        float GetMaxHeightOfTiles(List<QuarterTile> tiles)
        {
            float maxHeight = tiles[0].height;
            foreach (QuarterTile t in tiles)
            {
                if (t.height > maxHeight)
                {
                    maxHeight = t.height;
                }
            }
            return maxHeight;
        }

        #endregion

    }
}
