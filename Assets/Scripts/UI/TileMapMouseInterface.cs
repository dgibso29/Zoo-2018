using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using Zoo.Systems.World;
using Zoo.Systems.Landscaping;
using Zoo.Systems.Construction;

namespace Zoo.UI
{
    public class TileMapMouseInterface : MonoBehaviour
    {
        /// <summary>
        /// Vertice coordinates currently under the mouse cursor, map coordinates. 
        /// Must be compared against current Quarter Tile for conversion to referenced vertice for use.
        /// </summary>
        public Vector3 currentVerticeCoordinates;
        /// <summary>
        /// Name (LowerLeft/UpperLeft/UpperRight/LowerRight) of the currently selected vertice, based on the
        /// current vertice coordinates.
        /// </summary>
        public string currentVerticeName;
        /// <summary>
        /// Name (North/South/East/West) of the currently selected tile edge.
        /// </summary>
        public string currentTileEdgeName;
        /// <summary>
        /// Quarter tile currently under the mouse cursor.
        /// </summary>
        public QuarterTile currentQuarterTile;
        /// <summary>
        /// Full tile currently under the mouse cursor.
        /// </summary>
        public FullTile currentFullTile;
        /// <summary>
        /// Tiles currently selected by selection box.
        /// </summary>
        public List<QuarterTile> currentlySelectedTiles;

        /// <summary>
        /// BuildableObject currently under the mouse cursor.
        /// </summary>
        public BuildableObject currentlySelectedBuildableObject;

        /// <summary>
        /// Current coordinates of the mouse cursor on the tilemap.
        /// </summary>
        public Vector3 currentMouseCoordinatesOnTileMap;

        /// <summary>
        /// Current mouse coordinates in world space.
        /// </summary>
        public Vector3 currentMouseWorldPosition;

        /// <summary>
        /// The current height we are working with.
        /// </summary>
        public float currentHeight;
        /// <summary>
        /// The new height we are changing to.
        /// </summary>
        public float newHeight;

        /// <summary>
        /// Current Y coordinate of the mouse cursor.
        /// </summary>
        float currentCursorPosY;

        /// <summary>
        /// If the current mouse Y is this much greater or lesser than the starting Y, 
        /// the newHeight will increase/decrease by one tileStepIncrement (.25).
        /// </summary>
        public int heightChangeInterval = 33;

        /// <summary>
        /// The tile selected when the player left clicked. Used to determine if the player is attempting to drag or not.
        /// </summary>
        public QuarterTile tileWhenLeftClicked;

        /// <summary>
        /// The full tile selected when the player left clicked.
        /// </summary>
        public FullTile fullTileWhenLeftClicked;

        /// <summary>
        /// The selected vertice when the player left clicked.
        /// </summary>
        public Vector3 verticeCoordinatesWhenLeftClicked;

        /// <summary>
        /// Name of the selected vertice when the player left clicked.
        /// </summary>
        public string verticeNameWhenLeftClicked;

        /// <summary>
        /// Name (North/South/East/West) of the selected tile edge when the player left clicked.
        /// </summary>
        public string tileEdgeNameWhenLeftClicked;

        /// <summary>
        /// Tile map coordinates of the mouse cursor when the player left clicked.
        /// </summary>
        public Vector3 mouseCoordinatesOnTileMapWhenLeftClicked;

        /// <summary>
        ///  Mouse position in world space when player left clicked.
        /// </summary>
        public Vector3 mousePositionWhenLeftClicked;

        /// <summary>
        /// If true, the player was modifying a vertice when they left clicked.
        /// </summary>
        bool modifyingVerticeWhenLeftClicked = false;

        public float timeBeforeDragStarts = .5f;

        /// <summary>
        /// Size of square selection brush. If below 2x2 (value of 2), revert to single tile selection.
        /// </summary>
        private int selectionBrushSize = 1;

        /// <summary>
        /// Maximum size of selection brush. The primary factor here is performance.
        /// </summary>
        public int maxSelectionBrushSize = 20;

        /// <summary>
        /// Enabled when mouse cursor is over an UI element.
        /// </summary>
        public bool isOverUI = false;

        /// <summary>
        /// Enabled when player is selecting something.
        /// </summary>
        public bool isSelecting = false;

        /// <summary>
        /// Enabled when the player begins dragging a selection box, or dragging while constructing.
        /// </summary>
        public bool isDragging = false;

        /// <summary>
        /// Enabled when the player is holding down the modifier key (default to shift).
        /// </summary>
        public bool modifierKeyPressed = false;

        public bool isModifyingHeight = false;

        /// <summary>
        /// Enabled when the mouse cursor is closer to the corner of a tile than its center.
        /// </summary>
        bool isOverVertice = false;

        public bool singleTileOverlayActive = false;
        public bool selectionBoxOverlayActive = false;

        // Set component references
        public World world;
        public LandscapingTools landscapingTools;
        public MainGameUI mainGameUI;
        public ConstructionTools constructionTools;
        public CameraController cameraController;

        // Declare the structs to hold the tiles at the edge of a selection.
        /// <summary>
        /// Finds and holds the tiles at the edge of a given rectangular selection.
        /// </summary>
        public struct RectangleEdgeTiles
        {
            public QuarterTile SouthwestTile;
            public QuarterTile NorthwestTile;
            public QuarterTile NortheastTile;
            public QuarterTile SoutheastTile;
            public List<QuarterTile> SouthernEdgeTiles;
            public List<QuarterTile> WesternEdgeTiles;
            public List<QuarterTile> NorthernEdgeTiles;
            public List<QuarterTile> EasternEdgeTiles;

            public RectangleEdgeTiles(List<QuarterTile> rectangularSelection)
            {
                //Debug.Log($"Setting up rectangle edge tiles with selection with a count of {rectangularSelection.Count}");
                int minX = rectangularSelection.Find(t => t != null).TileCoordX;
                int maxX = rectangularSelection.Find(t => t != null).TileCoordX;
                int minZ = rectangularSelection.Find(t => t != null).TileCoordZ;
                int maxZ = rectangularSelection.Find(t => t != null).TileCoordZ;

                // Find the min and max X & Z coordinates
                foreach (QuarterTile t in rectangularSelection)
                {
                    if (t != null)
                    {
                        // If X is greater than the current max
                        if (t.TileCoordX > maxX)
                        {
                            maxX = t.TileCoordX;
                        }
                        // If X is less than the current min
                        if (t.TileCoordX < minX)
                        {
                            minX = t.TileCoordX;
                        }
                        // If Z is greater than the current max
                        if (t.TileCoordZ > maxZ)
                        {
                            maxZ = t.TileCoordZ;
                        }
                        // If Z is less than the current min{
                        if (t.TileCoordZ < minZ)
                        {
                            minZ = t.TileCoordZ;
                        }
                    }
                }

                //Debug.Log($" Min X/Z: {minX},{minZ}; Max X/Z {maxX},{maxZ}");
                // Find each corner based on min and max X & Z coordinates
                SouthwestTile = rectangularSelection.Find(t => t.TileCoordX == minX && t.TileCoordZ == minZ);
                NorthwestTile = rectangularSelection.Find(t => t.TileCoordX == minX && t.TileCoordZ == maxZ);
                NortheastTile = rectangularSelection.Find(t => t.TileCoordX == maxX && t.TileCoordZ == maxZ);
                SoutheastTile = rectangularSelection.Find(t => t.TileCoordX == maxX && t.TileCoordZ == minZ);
                //if(SouthwestTile == null)
                //{
                //    Debug.Log("Fuck me SW");
                //}
                //if (SoutheastTile == null)
                //{
                //    Debug.Log("Fuck me SE");
                //}
                //if (NorthwestTile == null)
                //{
                //    Debug.Log("Fuck me NW");
                //}
                //if (NortheastTile == null)
                //{
                //    Debug.Log("Fuck me NE");
                //}
                // Find the edge tiles based on the min and max coordinates, taking care to exlude the corner tiles.
                SouthernEdgeTiles = rectangularSelection.FindAll(t => t.TileCoordZ == minZ && t.TileCoordX != minX && t.TileCoordX != maxX);
                WesternEdgeTiles = rectangularSelection.FindAll(t => t.TileCoordX == minX && t.TileCoordZ != minZ && t.TileCoordZ != maxZ);
                NorthernEdgeTiles = rectangularSelection.FindAll(t => t.TileCoordZ == maxZ && t.TileCoordX != minX && t.TileCoordX != maxX);
                EasternEdgeTiles = rectangularSelection.FindAll(t => t.TileCoordX == maxX && t.TileCoordZ != minZ && t.TileCoordZ != maxZ);

            }


        }

        /// <summary>
        /// Holds the tiles at the edge of a polygonal selection.
        /// </summary>
        public struct PolygonEdgeTiles
        {

        }


        void Update()
        {
            GetSelectedTile();
            GetCurrentMouseY();
            CheckIfOverUI();
            GetBuildableObjectAtCursor();

            currentMouseWorldPosition = Input.mousePosition;

            if (isSelecting && !isDragging)
            {
                // If we should be selecting a single tile
                if (selectionBrushSize == 1)
                {
                    // If the single tile selection coroutine is not already running, start it.
                    if (!singleTileOverlayActive)
                    {
                        //Debug.Log("Starting single tile selection");
                        StartCoroutine(SelectedTileSelectionOverlay());
                    }
                }
                // If we should be selecting a grid/brush of tiles
                else if (selectionBrushSize > 1)
                {
                    if (!selectionBoxOverlayActive)
                    {
                        //Debug.Log("Starting Selection brush");
                        StartCoroutine(SelectionGrid());
                    }
                }
            }
            if (!isOverUI)
            {
                // All left-mouse button click functions.
                if (Input.GetMouseButtonDown(0))
                {
                    //Debug.Log(currentQuarterTile.gridType);
                    // Save the tile selected when the left button is clicked.
                    tileWhenLeftClicked = currentQuarterTile;
                    fullTileWhenLeftClicked = currentFullTile;
                    verticeCoordinatesWhenLeftClicked = currentVerticeCoordinates;
                    verticeNameWhenLeftClicked = currentVerticeName;
                    tileEdgeNameWhenLeftClicked = currentTileEdgeName;
                    modifyingVerticeWhenLeftClicked = isOverVertice;
                    mouseCoordinatesOnTileMapWhenLeftClicked = currentMouseCoordinatesOnTileMap;
                    mousePositionWhenLeftClicked = Input.mousePosition;

                    // Check if dragging while constructing, if the current object can be dragged.
                    if (constructionTools.isBuilding &&constructionTools.referenceGhostObject.isDraggable)
                    {
                        // Stop any pre-existing check that may be left over due to rapid-fire clicking, etc.
                        StopCoroutine(CheckIfDraggingConstruction());
                        // Start checking.
                        StartCoroutine(CheckIfDraggingConstruction());
                    }

                }
                if (Input.GetMouseButton(0))
                {
                    // If the modifier key is not pressed AND we are not dragging AND
                    // the current tile is not the tile that was initally clicked, start dragging a selection box.
                    if (isSelecting && !modifierKeyPressed && !isDragging && tileWhenLeftClicked != currentQuarterTile)
                    {
                        StartCoroutine(DragSelectionBox());
                    }
                }
                if (Input.GetMouseButtonUp(0))
                {
                    if (constructionTools.isBuilding)
                    {
                        constructionTools.AttemptToBuildGhostObjects();
                    }
                }

                
                // All right-mouse button click functions.
                if (Input.GetMouseButtonDown(1))
                {
                    if (constructionTools.isBuilding)
                    {
                        constructionTools.isBuilding = false;
                    }
                    else if(constructionTools.constructionEnabled || landscapingTools.landscapingEnabled)
                    {
                        constructionTools.DeleteObjectAtCursor();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                constructionTools.heightHasBeenModified = false;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                StartCoroutine(EnableModifierKeyFunctions());
            }

            // Decrease brush size
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                //SelectionBrushSize -= 1;
                StartCoroutine(DecreaseSelectionBrushSize());

            }
            // Increase brush size
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                //SelectionBrushSize += 1;
                StartCoroutine(IncreaseSelectionBrushSize());

            }

        }


        private void FixedUpdate()
        {
        }

        #region Selection UI Functions

        /// <summary>
        /// Used to turn selection on/off. Should be called when relevant windows are opened and closed.
        /// </summary>
        public void ToggleSelectionTools()
        {
            if (!isSelecting)
            {
                isSelecting = true;
            }
            else
            {
                isSelecting = false;
            }
        }

        IEnumerator EnableModifierKeyFunctions()
        {
            while (Input.GetKey(KeyCode.LeftShift))
            {
                modifierKeyPressed = true;
                yield return new WaitForEndOfFrame();
            }
            modifierKeyPressed = false;
        }


        IEnumerator DecreaseSelectionBrushSize()
        {
            while (Input.GetKey(KeyCode.LeftBracket))
            {
                SelectionBrushSize -= 1;
                yield return new WaitForSecondsRealtime(.075f);
            }
        }
        IEnumerator IncreaseSelectionBrushSize()
        {
            while (Input.GetKey(KeyCode.RightBracket))
            {
                SelectionBrushSize += 1;
                yield return new WaitForSecondsRealtime(.075f);
            }
        }

        void CheckIfOverUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                isOverUI = true;
            }
            else
            {
                isOverUI = false;
            }
        }

        #endregion

        #region Selection Functions
        /// <summary>
        /// Show single tile selection overlay, alternating between quarter and full tile based on advanced mode toggle.
        /// In advanced mode, vertice selection is enabled,
        /// and occures when the mouse is closer to a corner than the center of a tile.
        /// </summary>
        /// <returns></returns>
        IEnumerator SelectedTileSelectionOverlay()
        {
            singleTileOverlayActive = true;
            string activeVerticeName = null;
            QuarterTile activeTile = null;
            FullTile activeFullTile = null;

            bool advModeStatus = SettingsHelper.AdvancedConstructionModeEnabled;
            while (isSelecting && !isDragging)
            {
                if (selectionBrushSize > 1 || isDragging)
                {
                    if (activeTile != null)
                        landscapingTools.ReturnOverlayTypeToDefault(activeTile);
                    if (activeFullTile != null)
                    {
                        landscapingTools.ReturnOverlayTypeToDefault(activeFullTile.tiles);
                    }
                    singleTileOverlayActive = false;
                    break;
                }
                if (activeTile != currentQuarterTile || advModeStatus != SettingsHelper.AdvancedConstructionModeEnabled)
                {
                    advModeStatus = SettingsHelper.AdvancedConstructionModeEnabled;
                    // Return previous tile to normal
                    if (activeTile != null)
                    {
                        landscapingTools.ReturnOverlayTypeToDefault(activeTile);
                        if (activeFullTile != null)
                        {
                            landscapingTools.ReturnOverlayTypeToDefault(activeFullTile.tiles);
                        }
                    }
                    // If currently dragging, stop displaying the single tile until dragging ends.
                    if (Input.GetMouseButton(0))
                    {
                        while (Input.GetMouseButton(0))
                        {
                            yield return null;
                        }
                        break;
                    }
                    // Update active tile
                    activeTile = currentQuarterTile;
                    if (!SettingsHelper.AdvancedConstructionModeEnabled)
                    {
                        activeFullTile = currentFullTile;
                    }
                }
                // Make sure the full tile is set back to default texture if it is turned off. 
                if (activeFullTile != null && SettingsHelper.AdvancedConstructionModeEnabled)
                {
                    landscapingTools.ReturnOverlayTypeToDefault(activeFullTile.tiles);
                }
                // Set overlay textures
                // If we are over a vertice && advanced construction mode is enabled, enable vertices.
                if (isOverVertice && SettingsHelper.AdvancedConstructionModeEnabled)
                {

                    activeVerticeName = currentVerticeName;
                    landscapingTools.ChangeTileOverlayTypes(new List<QuarterTile>() { activeTile },
                        landscapingTools.GetVerticeSelectionOverlayTexture(activeVerticeName));
                }
                // Otherwise, continue as normal.
                else
                {
                    // Quarter/Full Tile as needed
                    //Debug.Log("Active full tile exists!");
                    landscapingTools.ChangeTileOverlayTypes(new List<QuarterTile>() { activeTile }, 1);
                    if (!SettingsHelper.AdvancedConstructionModeEnabled && activeFullTile != null)
                    {
                        landscapingTools.ChangeTileOverlayTypes(activeFullTile.tiles, 1);
                    }
                }
                if (selectionBrushSize > 1 || isDragging)
                {
                    if (activeTile != null)
                        landscapingTools.ReturnOverlayTypeToDefault(activeTile);
                    if (activeFullTile != null)
                    {
                        landscapingTools.ReturnOverlayTypeToDefault(activeFullTile.tiles);
                    }
                    singleTileOverlayActive = false;
                    break;
                }
                // If the player clicks the left mouse button, and is still over the same tile, do things with the current selection.
                if (Input.GetMouseButton(0) && tileWhenLeftClicked == currentQuarterTile && !isOverUI && currentQuarterTile != null)
                {
                    // If landscaping is enabled
                    if (landscapingTools.landscapingEnabled)
                    {
                        // If the player is pressing the modifier key,
                        // and terrain modification is enabled, start raising/lowering the current selection.
                        if (modifierKeyPressed && landscapingTools.terrainModificationEnabled)
                        {
                            while (Input.GetMouseButton(0))
                            {
                                // Raise/Lower land here!
                                if (!isModifyingHeight)
                                {
                                    StartCoroutine(ChangeHeight());
                                }
                                yield return new WaitForEndOfFrame();
                            }
                            // Wait while land is being modified, then clear selection, turn dragging off, and break
                            if (activeTile != null)
                                landscapingTools.ReturnOverlayTypeToDefault(activeTile);
                            if (activeFullTile != null)
                            {
                                landscapingTools.ReturnOverlayTypeToDefault(activeFullTile.tiles);
                            }
                            singleTileOverlayActive = false;
                            yield break;
                        }
                    }
                }
                // If the player lets go of the left mouse button, and is still over the same tile, do things with the current selection.
                if (Input.GetMouseButtonUp(0) && tileWhenLeftClicked == currentQuarterTile && !isOverUI && currentQuarterTile != null)
                {
                    // If landscaping is enabled
                    if (landscapingTools.landscapingEnabled)
                    {
                        // If Terrain Modification is enabled and the modifier key is not pressed, level the currently selected tiles.
                        if (landscapingTools.terrainModificationEnabled && !modifierKeyPressed)
                        {

                            // If not over vertice, level to height of current tile.
                            if (SettingsHelper.AdvancedConstructionModeEnabled && !isOverVertice)
                            {
                                landscapingTools.LevelTerrain(new List<QuarterTile>(1) { activeTile }, activeTile.height);
                            }
                            // If over vertice, level to height of current vertice.
                            else if (SettingsHelper.AdvancedConstructionModeEnabled && isOverVertice)
                            {
                                landscapingTools.LevelTerrain(new List<QuarterTile>(1) { activeTile }, currentVerticeCoordinates.y);
                            }
                            else
                            {
                                landscapingTools.LevelTerrain(activeFullTile.tiles, activeFullTile.HeightOfHighestQuarterTile());
                            }
                        }
                        // If Terrain painting is enabled, paint the currently selected tiles.
                        if (landscapingTools.terrainTexturePaintingEnabled || landscapingTools.terrainCliffPaintingEnabled)
                        {
                            if (SettingsHelper.AdvancedConstructionModeEnabled)
                            {
                                landscapingTools.PaintTextures(new List<QuarterTile>(1) { activeTile },
                                    landscapingTools.currentTerrainTexture, landscapingTools.currentCliffTexture);
                            }
                            else
                            {
                                landscapingTools.PaintTextures(activeFullTile.tiles,
                                    landscapingTools.currentTerrainTexture, landscapingTools.currentCliffTexture);
                            }
                        }

                    }
                    // Draw affected tiles
                    if (SettingsHelper.AdvancedConstructionModeEnabled)
                    {
                        world.DrawChunks(landscapingTools.GetDirtyChunks(new List<QuarterTile>(1) { activeTile }));
                    }
                    else
                    {
                        world.DrawChunks(landscapingTools.GetDirtyChunks(activeFullTile.tiles));
                    }

                }
                yield return new WaitForFixedUpdate();
                // Return previous tile to normal
            }
            if (activeTile != null)
                landscapingTools.ReturnOverlayTypeToDefault(activeTile);
            if (activeFullTile != null)
            {
                landscapingTools.ReturnOverlayTypeToDefault(activeFullTile.tiles);
            }
            singleTileOverlayActive = false;
            yield break;
        }

        /// <summary>
        /// When the left mouse button is held down, a selection box is drawn between
        /// the starting quarter tile and the current quarter tile.
        /// </summary>
        /// <returns></returns>
        IEnumerator DragSelectionBox()
        {
            QuarterTile startingTile = tileWhenLeftClicked;
            List<QuarterTile> oldTileList = new List<QuarterTile>();
            QuarterTile oldEndTile = currentQuarterTile;
            isDragging = true;
            //currentlySelectedTiles.Clear();
            //Debug.Log("Started dragging.");
            // While the left mouse button is held down, update the list of selected tiles and update their overlay.
            while (Input.GetMouseButton(0))
            {
                while (selectionBoxOverlayActive)
                {
                    yield return new WaitUntil(() => !selectionBoxOverlayActive);
                }
                if (oldEndTile != currentQuarterTile)
                {
                    landscapingTools.ReturnOverlayTypeToDefault(oldTileList);
                    // Get the tiles in the selection
                    currentlySelectedTiles = world.GetQuarterTilesFromSelection(startingTile, currentQuarterTile);
                    // Update the tiles.
                    landscapingTools.ChangeTileOverlayTypes(currentlySelectedTiles, 1);

                    oldEndTile = currentQuarterTile;
                    oldTileList = currentlySelectedTiles;
                }
                // If the player presses the modifier key while dragging,
                // and landscaping is enabled, start raising/lowering the current selection.
                if (modifierKeyPressed && landscapingTools.landscapingEnabled && landscapingTools.terrainModificationEnabled)
                {
                    while (Input.GetMouseButton(0) && modifierKeyPressed)
                    {
                        // Raise/Lower land here!
                        if (!isModifyingHeight)
                        {
                            StartCoroutine(ChangeHeight());
                        }
                        yield return new WaitForEndOfFrame();
                    }
                    // Wait while land is being modified, then clear selection, turn dragging off, and break
                    landscapingTools.ReturnOverlayTypeToDefault(currentlySelectedTiles);
                    // Empty selected tiles list
                    currentlySelectedTiles.Clear();
                    isDragging = false;
                    //Debug.Log("Stopped dragging.");
                    yield break;
                }
                yield return new WaitForEndOfFrame();
            }
            // Once the player lets go of the mouse, do things.
            // If Landscaping
            if (landscapingTools.landscapingEnabled)
            {
                // If the modifier key is NOT pressed, level terrain.
                if (landscapingTools.terrainModificationEnabled && !modifierKeyPressed)
                {
                    //Debug.Log("Leveled selection");
                    landscapingTools.LevelTerrain(currentlySelectedTiles, startingTile.height);
                }
                // If painting terrain
                if (landscapingTools.terrainTexturePaintingEnabled || landscapingTools.terrainCliffPaintingEnabled)
                {
                    landscapingTools.PaintTextures(currentlySelectedTiles, landscapingTools.currentTerrainTexture, landscapingTools.currentCliffTexture);
                }
                // Once all operations are complete, re-draw the affected tiles.
                world.DrawChunks(landscapingTools.GetDirtyChunks(currentlySelectedTiles));
            }

            // Change all tiles back to default.
            landscapingTools.ReturnOverlayTypeToDefault(currentlySelectedTiles);
            // Empty selected tiles list
            currentlySelectedTiles.Clear();
            isDragging = false;
            //Debug.Log("Stopped dragging.");
            yield break;

        }

        /// <summary>
        /// When called, a selection overlay is drawn based on the provided grid size.
        /// If advanced mode is enabled, the grid size will change to reflect quarter or full tiles.
        /// Ex: Size of 2 = 2x2 quarter tiles OR 2x2 Full tiles (4x4 quarter tiles).
        /// </summary>
        /// <returns></returns>
        IEnumerator SelectionGrid()
        {
            // Need to handle multi-tile selection.
            selectionBoxOverlayActive = true;
            QuarterTile activeTile = null;
            List<QuarterTile> activeTiles = null;
            int oldBrushSize = 0;
            bool advModeStatus = SettingsHelper.AdvancedConstructionModeEnabled;
            //currentlySelectedTiles.Clear();
            while (isSelecting && !isDragging)
            {
                if (selectionBrushSize == 1 || isDragging)
                {
                    //Debug.Log("Stopping selection brush because we are dragging or brush size is 1");
                    landscapingTools.ReturnOverlayTypeToDefault(currentlySelectedTiles);
                    currentlySelectedTiles.Clear();
                    selectionBoxOverlayActive = false;
                    break;
                }
                // If the brush size, active tile, or construction mode have changed, update the brush.
                if (oldBrushSize != selectionBrushSize
                    || activeTile != currentQuarterTile
                    || advModeStatus != SettingsHelper.AdvancedConstructionModeEnabled)
                {
                    oldBrushSize = selectionBrushSize;
                    advModeStatus = SettingsHelper.AdvancedConstructionModeEnabled;
                    // Return previous tile to normal
                    if (activeTiles != null)
                    {
                        landscapingTools.ReturnOverlayTypeToDefault(activeTiles);
                    }
                    // If currently dragging, stop displaying the single tile until dragging ends.
                    if (Input.GetMouseButton(0))
                    {
                        while (Input.GetMouseButton(0))
                        {
                            yield return null;
                        }
                        landscapingTools.ReturnOverlayTypeToDefault(currentlySelectedTiles);
                    }
                    // Update active tiles
                    activeTile = currentQuarterTile;
                    activeTiles = new List<QuarterTile>();
                    int brushSizeModifier = 1;
                    if (!SettingsHelper.AdvancedConstructionModeEnabled)
                    {
                        brushSizeModifier = 2;
                    }
                    for (int x = activeTile.TileCoordX; x < activeTile.TileCoordX + selectionBrushSize * brushSizeModifier; x++)
                    {
                        for (int z = activeTile.TileCoordZ; z < activeTile.TileCoordZ + selectionBrushSize * brushSizeModifier; z++)
                        {
                            if (x > world.MapData.GetLength(0) - 1 || z > world.MapData.GetLength(1) - 1)
                                continue;
                            if (world.MapData[x, z] != null)
                                activeTiles.Add(world.MapData[x, z]);
                        }
                    }
                    // And paint them
                    landscapingTools.ChangeTileOverlayTypes(activeTiles, 1);
                    // And set the selection to the current selection tiles.
                    currentlySelectedTiles = activeTiles;
                }
                if (isDragging)
                {
                    //Debug.Log("Stopping selection brush because we are dragging");
                    landscapingTools.ReturnOverlayTypeToDefault(currentlySelectedTiles);
                    currentlySelectedTiles.Clear();
                    selectionBoxOverlayActive = false;
                    break;

                }
                // If the player clicks the left mouse button, and is still over the starting tile, do thing
                if (Input.GetMouseButton(0) && tileWhenLeftClicked == currentQuarterTile && !isOverUI && currentQuarterTile != null && !isDragging)
                {
                    // If Landscaping
                    if (landscapingTools.landscapingEnabled)
                    {
                        // If the player is pressing the modifier key,
                        // and terrain modification is enabled, start raising/lowering the current selection.
                        if (modifierKeyPressed && landscapingTools.terrainModificationEnabled)
                        {
                            while (Input.GetMouseButton(0) && modifierKeyPressed)
                            {
                                // Raise/Lower land here!
                                if (!isModifyingHeight)
                                {
                                    StartCoroutine(ChangeHeight());
                                }
                                yield return new WaitForEndOfFrame();
                            }
                            // Wait while land is being modified, then clear selection, turn dragging off, and break
                            landscapingTools.ReturnOverlayTypeToDefault(currentlySelectedTiles);
                            // Empty selected tiles list
                            currentlySelectedTiles.Clear();
                            selectionBoxOverlayActive = false;
                            yield break;
                        }
                    }
                }
                // If the player lets go of the left mouse button, and is still over the starting tile, do things
                if (Input.GetMouseButtonUp(0) && tileWhenLeftClicked == currentQuarterTile && !isOverUI && currentQuarterTile != null && !isDragging)
                {
                    // If Landscaping
                    if (landscapingTools.landscapingEnabled)
                    {

                        // If the modifier key is NOT pressed, level terrain.
                        if (landscapingTools.terrainModificationEnabled && !modifierKeyPressed)
                        {
                            landscapingTools.LevelTerrain(currentlySelectedTiles, currentQuarterTile.height);
                            //Debug.Log("Leveled brush");
                        }
                        // If painting terrain
                        if (landscapingTools.terrainTexturePaintingEnabled || landscapingTools.terrainCliffPaintingEnabled)
                        {
                            landscapingTools.PaintTextures(currentlySelectedTiles, landscapingTools.currentTerrainTexture, landscapingTools.currentCliffTexture);
                        }
                        // Once all operations are complete, re-draw the affected tiles.
                        world.DrawChunks(landscapingTools.GetDirtyChunks(currentlySelectedTiles));
                    }

                }
                yield return new WaitForFixedUpdate();
            }
            // Return previous tile to normal
            if (activeTiles != null)
            {
                landscapingTools.ReturnOverlayTypeToDefault(activeTiles);
            }
            // Empty selected tiles list
            currentlySelectedTiles.Clear();
            selectionBoxOverlayActive = false;
            //Debug.Log("Normal end of selection brush. Current tiles are cleared.");
            yield break;
        }

        /// <summary>
        /// When called, newHeight is updated based on the cursor's up/down movments in relation to the currentHeight,
        /// and performs relevant options based on the current context (landscaping, building, etc).
        /// </summary>
        /// <returns></returns>
        public IEnumerator ChangeHeight()
        {
            // TO DO: ONLY INCREASE/DECREASE THE LOWEST/HEIGHEST TILES IN A SELECTION
            // Assign currentHeight based on current context.
            // If landscaping
            if (landscapingTools.landscapingEnabled)
            {
                // If advanced mode is on AND we are selecting a vertice, set the current height to the height of the current vertice.
                if (SettingsHelper.AdvancedConstructionModeEnabled && singleTileOverlayActive && modifyingVerticeWhenLeftClicked)
                {
                    currentHeight = verticeCoordinatesWhenLeftClicked.y;
                }
                // Othwerise, set current height to the height of the current tile
                else
                {
                    currentHeight = tileWhenLeftClicked.height;
                }
            }
            // If building
            else if (constructionTools.constructionEnabled)
            {
                currentHeight = constructionTools.currentBuildHeight;
            }
            bool isRaisingLand = true;
            Vector3 startCursorPos = Input.mousePosition;
            float currentHeightChangeInterval;
            newHeight = currentHeight;
            isModifyingHeight = true;
            // If landscaping
            if (landscapingTools.landscapingEnabled)
            {
                currentHeightChangeInterval = heightChangeInterval;

                float startY = startCursorPos.y;
                while (modifierKeyPressed && Input.GetMouseButton(0))
                {
                    //Debug.Log($"Start Pos = {startCursorPosY}. Current Pos = {currentCursorPosY}");
                    // Only run this loop if needed.
                    // Has the mouse position changed enough to change the height?

                    //Debug.Log($"Current Mathf.Abs(startCursorPos.y - mousePos.y) = {Mathf.Abs(startY - mousePos.y)}, compared against {/*Mathf.Abs(startCursorPos.y - mousePos.y) +*/ currentHeightChangeInterval}");

                    //Debug.Log("Should be changing height.");
                    // Check if we need to change the newHeight by checking current mouse Y against start mouse Y
                    #region Update Height
                    if (Mathf.Abs(startY - currentMouseWorldPosition.y) >= currentHeightChangeInterval)
                    {
                        //Debug.Log($"Current Mathf.Abs(startY - currentMouseWorldPosition.y) = {Mathf.Abs(startY - currentMouseWorldPosition.y)}, compared against {/*Mathf.Abs(startCursorPos.y - mousePos.y) +*/ currentHeightChangeInterval}");
                        //Debug.Log($"StartPos = {startCursorPos}, Current Start Pos Y = {startY}, current pos = {currentMouseWorldPosition.y}");
                        
                        // If advanced construction is enabled
                        if (SettingsHelper.AdvancedConstructionModeEnabled)
                        {
                            // Round to nearest quarter height increment.
                            newHeight = World.RoundToNearestQuarter(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - Camera.main.ScreenToWorldPoint(startCursorPos).y + currentHeight);
                        }
                        // Otherwise
                        else
                        {
                            // Round to nearest half height increment.
                            newHeight = World.RoundToNearestHalf(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - Camera.main.ScreenToWorldPoint(startCursorPos).y + currentHeight);
                        }
                        // If the current Y is greater than the start Y
                        if (currentMouseWorldPosition.y > startY)
                        {
                            //Debug.Log("Raising");
                            //Debug.Log(newHeight);
                            isRaisingLand = true;
                            startY += heightChangeInterval;
                        }
                        // If the current Y is less than the start Y
                        else if (currentMouseWorldPosition.y < startY)
                        {
                            //Debug.Log("Lowering");
                            //Debug.Log(newHeight);
                            isRaisingLand = false;
                            startY -= heightChangeInterval;
                        }
                        // Update start cursor position
                        //startY = Input.mousePosition.y;

                        // Make sure height is not too high or too low.
                        if (newHeight < -5)
                        {
                            newHeight = -5;
                        }
                        if (newHeight > 100)
                        {
                            newHeight = 100;
                        }
                        #endregion
                        #region Do Things
                        // Now that we've updated the height, do things based on the current context.    

                        // Raise/lower height based on Advanced Mode status
                        // If Adv Mode is on AND we are only modifying a single tile/vertice
                        if (SettingsHelper.AdvancedConstructionModeEnabled && singleTileOverlayActive && !isDragging)
                        {
                            // If we are modifying a vertice
                            if (modifyingVerticeWhenLeftClicked)
                            {
                                // Modify the vertice
                                landscapingTools.ModifyTerrainHeight(tileWhenLeftClicked, verticeNameWhenLeftClicked, newHeight);
                                world.DrawChunks(landscapingTools.GetDirtyChunks(new List<QuarterTile>(1) { tileWhenLeftClicked }));
                            }
                            // Otherwise
                            else
                            {
                                // Modify the single tile
                                landscapingTools.ModifyTerrainHeight(tileWhenLeftClicked, newHeight);
                                world.DrawChunks(landscapingTools.GetDirtyChunks(new List<QuarterTile>(1) { tileWhenLeftClicked }));
                            }
                        }
                        // If Adv mode is on AND we are modifying multiple tiles
                        else if (SettingsHelper.AdvancedConstructionModeEnabled && !singleTileOverlayActive)
                        {
                            landscapingTools.ModifyTerrainHeight(currentlySelectedTiles, isRaisingLand, newHeight);
                            world.DrawChunks(landscapingTools.GetDirtyChunks(currentlySelectedTiles));
                        }
                        // If Adv Mode is off
                        else if (!SettingsHelper.AdvancedConstructionModeEnabled)
                        {
                            // Modify the selected tile/tiles
                            // If only one tile is selected
                            if (singleTileOverlayActive)
                            {
                                // Modify the full tile
                                landscapingTools.ModifyTerrainHeight(fullTileWhenLeftClicked.tiles, isRaisingLand, newHeight);
                                world.DrawChunks(landscapingTools.GetDirtyChunks(currentlySelectedTiles));
                            }
                            // If multiple tiles are selected
                            else
                            {
                                landscapingTools.ModifyTerrainHeight(currentlySelectedTiles, isRaisingLand, newHeight);
                                world.DrawChunks(landscapingTools.GetDirtyChunks(currentlySelectedTiles));
                            }

                        }
                        #endregion
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
            // If constructing
            else if (constructionTools.constructionEnabled)
            {
                while (modifierKeyPressed)
                {
                    // If the mouse cursor Y position has changed, update height.
                    if (currentCursorPosY != startCursorPos.y)
                    {
                        newHeight = Camera.main.ScreenToWorldPoint(Input.mousePosition).y - Camera.main.ScreenToWorldPoint(startCursorPos).y + currentHeight;
                    }
                    // If constructing
                    if (constructionTools.constructionEnabled)
                    {
                        if(newHeight < -5.5)
                        {
                            newHeight = -5.5f;
                        }
                        if(newHeight > 99.5)
                        {
                            newHeight = 99.5f;
                        }
                        constructionTools.currentBuildHeight = newHeight;
                    }                    
                    yield return new WaitForEndOfFrame();
                }
            }
            isModifyingHeight = false;

        }

        /// <summary>
        /// While running, checks if conditions have been met to begin dragging construction.
        /// </summary>
        /// <returns></returns>
        public IEnumerator CheckIfDraggingConstruction()
        {
            // Wait before checking
            yield return new WaitForSeconds(timeBeforeDragStarts);

            // While the left mouse button is held down
            while (Input.GetMouseButton(0))
            {
                string direction = GetDragDirection();
                Vector3 footprintToCheck = constructionTools.referenceGhostObject.objectFootprint;

                // Check if the mouse cursor's position is further away from its starting position than
                // half the footprint of the object being built.
                switch (direction)
                {
                    case "N":
                        {
                            if (Mathf.Abs(currentMouseCoordinatesOnTileMap.z - mouseCoordinatesOnTileMapWhenLeftClicked.z) > footprintToCheck.z / 2)
                            {
                                isDragging = true;
                            }
                            else
                            {
                                isDragging = false;
                            }
                            break;
                        }
                    case "S":
                        {
                            if (Mathf.Abs(currentMouseCoordinatesOnTileMap.z - mouseCoordinatesOnTileMapWhenLeftClicked.z) > footprintToCheck.z / 2)
                            {
                                isDragging = true;
                            }
                            else
                            {
                                isDragging = false;
                            }
                            break;
                        }
                    case "E":
                        {
                            if (Mathf.Abs(currentMouseCoordinatesOnTileMap.x - mouseCoordinatesOnTileMapWhenLeftClicked.x) > footprintToCheck.x / 2)
                            {
                                isDragging = true;
                            }
                            else
                            {
                                isDragging = false;
                            }
                            break;
                        }
                    case "W":
                        {
                            if (Mathf.Abs(currentMouseCoordinatesOnTileMap.x - mouseCoordinatesOnTileMapWhenLeftClicked.x) > footprintToCheck.x / 2)
                            {
                                isDragging = true;
                            }
                            else
                            {
                                isDragging = false;
                            }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                yield return new WaitForEndOfFrame();
            }
            isDragging = false;
        }

        /// <summary>
        /// Get the direction of the current drag as a string (N/S/E/W).
        /// </summary>
        /// <returns></returns>
        public string GetDragDirection()
        {
            string direction = "null";

            Vector3 startMousePos = mouseCoordinatesOnTileMapWhenLeftClicked;
            Vector3 currentMousePos = currentMouseCoordinatesOnTileMap;

            // Get drag direction based on starting and current mouse tile map coordinates.
            // E/W = X 1/0; N/S = Z 1/0
            // The higher value wins -- 5,4 is East of 3,3, while 4,5 is North of 3,3

            // If current Z is greater than start Z
            if (currentMousePos.z > startMousePos.z)
            {
                // And if current X is greater than start X
                if (currentMousePos.x > startMousePos.x)
                {
                    // If the difference between current and start Z is greater than the difference between current and start X
                    if (Mathf.Abs(currentMousePos.z - startMousePos.z) > Mathf.Abs(currentMousePos.x - startMousePos.x))
                    {
                        // We're North
                        direction = "N";
                    }
                    // If the difference between current and start X is greater than the difference between current and start Z
                    else if (Mathf.Abs(currentMousePos.x - startMousePos.x) > Mathf.Abs(currentMousePos.z - startMousePos.z))
                    {
                        // We're East
                        direction = "E";
                    }
                    // If Z & X are equal
                    else if (currentMousePos.x == currentMousePos.z)
                    {
                        direction = "NE";
                    }
                }
                // And if current X is less than start X
                else if(currentMousePos.x < startMousePos.x)
                {
                    // If the difference between current and start Z is greater than the difference between current and start X
                    if (Mathf.Abs(currentMousePos.z - startMousePos.z) > Mathf.Abs(currentMousePos.x - startMousePos.x))
                    {
                        // We're North
                        direction = "N";
                    }
                    // If the difference between current and start X is greater than the difference between current and start Z
                    else if (Mathf.Abs(currentMousePos.x - startMousePos.x) > Mathf.Abs(currentMousePos.z - startMousePos.z))
                    {
                        // We're West
                        direction = "W";
                    }
                    // And Z & X are equal
                    else if (currentMousePos.x == currentMousePos.z)
                    {
                        direction = "NW";
                    }
                }
            }
            // If current Z is less than start Z
            else if(currentMousePos.z < startMousePos.z)
            {
                // And if current X is greater than start X
                if (currentMousePos.x > startMousePos.x)
                {
                    // If the difference between current and start Z is greater than the difference between current and start X
                    if (Mathf.Abs(currentMousePos.z - startMousePos.z) > Mathf.Abs(currentMousePos.x - startMousePos.x))
                    {
                        // We're South
                        direction = "S";
                    }
                    // If the difference between current and start X is greater than the difference between current and start Z
                    else if (Mathf.Abs(currentMousePos.x - startMousePos.x) > Mathf.Abs(currentMousePos.z - startMousePos.z))
                    {
                        // We're East
                        direction = "E";
                    }
                    // If Z & X are equal
                    else if (currentMousePos.x == currentMousePos.z)
                    {
                        direction = "SE";
                    }
                }
                // And if current X is less than start X
                else if (currentMousePos.x < startMousePos.x)
                {
                    // If the difference between current and start Z is greater than the difference between current and start X
                    if (Mathf.Abs(currentMousePos.z - startMousePos.z) > Mathf.Abs(currentMousePos.x - startMousePos.x))
                    {
                        // We're South
                        direction = "S";
                    }
                    // If the difference between current and start X is greater than the difference between current and start Z
                    else if (Mathf.Abs(currentMousePos.x - startMousePos.x) > Mathf.Abs(currentMousePos.z - startMousePos.z))
                    {
                        // We're West
                        direction = "W";
                    }
                    // And Z & X are equal
                    else if (currentMousePos.x == currentMousePos.z)
                    {
                        direction = "SW";
                    }
                }
            }
            return direction;
        }

        void GetSelectedTile()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            LayerMask layerMask = 1 << 11;
            layerMask |= 1 << 12;
            layerMask = ~layerMask;
            // Grab the coordinates of the tile moused over, and adjust it to the current tile size (Likely always 1 for Zoo.)
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
            {
                int x = Mathf.FloorToInt(hitInfo.point.x);
                //int y = Mathf.FloorToInt(hitInfo.point.y);
                float y = World.RoundToNearestQuarter(hitInfo.point.y);
                int z = Mathf.FloorToInt(hitInfo.point.z);
                float vertX = Mathf.Round(hitInfo.point.x);
                float vertZ = Mathf.Round(hitInfo.point.z);
                //Debug.Log($"Current vertice = {vertX - x}, {vertZ - z}");
                //Debug.Log($"Current y coordinate = {y}");
                if (world.GetQuarterTile(x, z) != null)
                {
                    currentQuarterTile = world.GetQuarterTile(x, z);
                    currentFullTile = world.GetFullTile(currentQuarterTile);
                    currentVerticeCoordinates = currentQuarterTile.GetVertice(vertX, vertZ);
                    currentVerticeName = currentQuarterTile.GetVerticeName(vertX, vertZ);
                    CheckIfOverVertice(hitInfo.point.x - x, hitInfo.point.z - z);
                    GetNameOfCurrentTileEdge(hitInfo.point);
                    currentMouseCoordinatesOnTileMap = hitInfo.point;
                }     
                if (Input.GetMouseButtonDown(0) && currentQuarterTile != null)
                {
                    //Debug.Log("Selecting Tile (" + x + "," + currentTile.TileHeight + "," + z + ") of type " + currentTile.TerrainType);
                    //Debug.Log($"Tile cliff type is {selectedTile.CliffType}. Tile isSlope = {selectedTile.isSlope}.");
                    //Debug.Log($"Tile top vertices = {selectedTile.upperRight},{selectedTile.lowerRight},{selectedTile.lowerLeft},{selectedTile.upperLeft}.");
                    //Debug.Log($"Tile bottom vertices = {selectedTile.bottomUpperRight},{selectedTile.bottomLowerRight},{selectedTile.bottomLowerLeft},{selectedTile.bottomUpperLeft}.");

                    //for(int i = 0; i < world.GetAdjacentTiles(selectedTile).Length; i++)
                    //{
                    //    string[] direction = new string[8] { "South", "Southwest", "West", "Northwest", "North", "Northeast", "East", "Southeast" };
                    //    Debug.Log($"Attempting to select {direction[i]} tile to tile {x},{z}.");
                    //    if (world.GetAdjacentTiles(selectedTile)[i] != null)
                    //    {
                    //        Tile t = world.GetAdjacentTiles(selectedTile)[i];
                    //        Debug.Log("Selecting Tile (" + t.TileCoordX + "," + t.TileHeight + "," + t.TileCoordZ + ") of type " + t.TileType);
                    //        Debug.Log($"Tile cliff type is {t.CliffType}. Tile isSlope = {t.isSlope}.");
                    //        Debug.Log($"Tile top vertices = {t.upperRight},{t.lowerRight},{t.lowerLeft},{t.upperLeft}.");
                    //        Debug.Log($"Tile bottom vertices = {t.bottomUpperRight},{t.bottomLowerRight},{t.bottomLowerLeft},{t.bottomUpperLeft}.");
                    //    }
                    //    else
                    //    {
                    //        Debug.Log($"{direction[i]} tile does not exist!");
                    //    }
                    //}
                    //int pointsAtReference = 0;
                    //if (selectedTile.upperRight.y.Equals(selectedTile.height)) { pointsAtReference++; }
                    //if (selectedTile.upperLeft.y.Equals(selectedTile.height)) { pointsAtReference++; }
                    //if (selectedTile.lowerRight.y.Equals(selectedTile.height)) { pointsAtReference++; }
                    //if (selectedTile.lowerLeft.y.Equals(selectedTile.height)) { pointsAtReference++; }
                    //Debug.Log($"Points at reference: {pointsAtReference}.");
                }
            }
            //else
            //{
            //    //currentTile = null;
            //    //if (Input.GetMouseButtonDown(0))
            //    //{
            //    //    //Debug.Log("No tile detected!");
            //    //}
            //}
        }

        void GetBuildableObjectAtCursor()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            LayerMask layerMask = 1 << 10;
            layerMask = ~layerMask;
            // Grab the buildable object moused over, if not over UI.
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
            {
                if (!isOverUI)
                {
                    if(hitInfo.collider != null)
                    {
                        if(hitInfo.collider.gameObject.GetComponent<BuildableObject>() != null)
                        {
                            currentlySelectedBuildableObject = hitInfo.collider.gameObject.GetComponent<BuildableObject>();
                        }
                        else
                        {
                            currentlySelectedBuildableObject = null;
                        }
                    }
                    else
                    {
                        currentlySelectedBuildableObject = null;
                    }
                }
                else
                {
                    currentlySelectedBuildableObject = null;
                }
            }
        }

        void GetCurrentMouseY()
        {
            currentCursorPosY = Input.mousePosition.y;
        }

        /// <summary>
        /// Check if provided coordinates are closer to the center of a tile, or to its corners.
        /// Input parameters must be normalised between 0 and 1.
        /// </summary>
        /// <param name="inputX"></param>
        /// <param name="inputZ"></param>
        void CheckIfOverVertice(float inputX, float inputZ)
        {
            // If both coordinates are closer to the corners, enable vertice selection.
            if (inputX < .3f || inputX > .7f || inputZ < .3f || inputZ > .7f)
            {
                //Debug.Log($"Vert coords are {inputX}, {inputZ}. Over vertice!");
                isOverVertice = true;
            }
            else
            {
                //Debug.Log($"Vert coords are {inputX}, {inputZ}. Not over vertice.");
                isOverVertice = false;
            }
        }

        /// <summary>
        /// Get the name of the current tile edge based on current mouse position on tile map.
        /// </summary>
        void GetNameOfCurrentTileEdge(Vector3 input)
        {

            Vector3 tileCenter = new Vector3(currentQuarterTile.TileCoordX + .5f, 0, currentQuarterTile.TileCoordZ + .5f);
            Vector3 tileCoords = new Vector3(input.x, 0, input.z);
            //Debug.Log($"Tile center: {tileCenter}");
            //Debug.Log($"Tile coordinates: {tileCoords}");
            tileCoords -= tileCenter;
            if (Mathf.Abs(tileCoords.x) > Mathf.Abs(tileCoords.z))
            {
                tileCoords.z = 0;
            }
            else
            {
                tileCoords.x = 0;
            }   
            tileCoords.y = 0f;
            tileCoords.Normalize();
            //Debug.Log($"Tile coords after normalisation: {tileCoords}");
            // Set tile side
            // If North side
            if (tileCoords.z == 1)
            {
                currentTileEdgeName = "North";
            }
            // If East side
            else if (tileCoords.x == 1)
            {
                currentTileEdgeName = "East";
            }
            // If South side
            else if (tileCoords.z == -1)
            {
                currentTileEdgeName = "South";
            }
            // If West side
            else if (tileCoords.x == -1)
            {
                currentTileEdgeName = "West";
            }
            //if (inputZ > .5f && inputX < inputZ)
            //{
            //    currentTileEdgeName = "North";
            //}
            //else if(inputZ < .5f && inputX )
        }


        /// <summary>
        /// Size of square selection brush. If below 2x2 (value of 2), revert to single tile selection.
        /// </summary>
        public int SelectionBrushSize
        {
            get
            {
                return selectionBrushSize;
            }
            set
            {
                selectionBrushSize = Mathf.Clamp(value, 1, maxSelectionBrushSize);
                mainGameUI.UpdateBrushSizeUI();
                //Debug.Log($"Selection brush size = {SelectionBrushSize}");
            }
        }

        #endregion

    }
}
