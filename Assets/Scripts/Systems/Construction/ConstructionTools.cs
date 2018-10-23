using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zoo.UI;
using Zoo.Systems;
using Zoo.Systems.World;
using Zoo.Utilities;

namespace Zoo.Systems.Construction
{
    public class ConstructionTools : MonoBehaviour
    {

        // Set component references
        public World.World world;
        public MainGameUI mainGameUI;
        public TileMapMouseInterface mouseInterface;

        public Material ghostMaterial;
        public Material blockedghostMaterial;

        /// <summary>
        /// The object currently selected to be built.
        /// </summary>
        public string objectToBuildID;

        /// <summary>
        /// When enabled, construction functions will work. Should only be enabled when construction window is open.
        /// </summary>
        public bool constructionEnabled = false;

        /// <summary>
        /// When enabled, building construction functions will work.
        /// </summary>
        public bool buildingToolsEnabled = false;

        /// <summary>
        /// When enabled, path construction functions will work.
        /// </summary>
        public bool pathToolsEnabled = false;

        /// <summary>
        /// When enabled, scenery construction functions will work.
        /// </summary>
        public bool sceneryToolsEnabled = false;

        /// <summary>
        /// Enabled when construction begins.
        /// </summary>
        public bool isBuilding = false;

        /// <summary>
        /// Enabled while rotating an object.
        /// </summary>
        public bool isRotating = false;

        /// <summary>
        /// Enabled when the player has modified building height. Keeps height at player-chosen level until reset.
        /// </summary>
        public bool heightHasBeenModified = false;

        /// <summary>
        /// When enabled, the current object can be built as desired.
        /// </summary>
        public bool clearToBuild = false;

        /// <summary>
        /// When enabled, clearance checks are in effect.
        /// </summary>
        public bool clearancesEnabled = true;

        // Building parameters
        /// <summary>
        /// Object type of object being built.
        /// </summary>
        public string currentBuildObjectType;

        /// <summary>
        /// Reference to the ghost object being displayed.
        /// </summary>
        public BuildableObject referenceGhostObject;

        /// <summary>
        /// Reference to the ghost objects being displayed. Used to attempt construction of objects.
        /// </summary>
        public List<BuildableObject> ghostObjects;

        /// <summary>
        /// List of auto-deleteable objects to be deleted when building.
        /// </summary>
        public List<BuildableObject> autoDeleteableObjectsToDelete;

        /// <summary>
        /// Current height at which we are building. This is reset to ground level at mouse position when the construction window is toggled.
        /// </summary>
        public float currentBuildHeight;

        /// <summary>
        /// Height at which to build current object.
        /// </summary>
        public float heightToBuild;

        /// <summary>
        /// Current rotation with which we are building. This will not be reset in the current instance.
        /// </summary>
        public float currentBuildRotation = 0;

        private void Update()
        {
            if (isBuilding)
            {
                if (referenceGhostObject.isFixedRotateable && Input.GetKeyDown(KeyCode.Z) && !isRotating)
                {
                    StartCoroutine(RotateObject());
                }
            }
        }

        #region UI Toggle Functions
        public void ToggleConstructionTools()
        {
            if (!constructionEnabled)
            {
                constructionEnabled = true;
                heightHasBeenModified = false;
                ResetCurrentBuildHeight();

            }
            else
            {
                heightHasBeenModified = false;
                constructionEnabled = false;
            }
        }

        public void ToggleBuildingTools()
        {
            if (!buildingToolsEnabled)
            {
                buildingToolsEnabled = true;
                constructionEnabled = true;
                heightHasBeenModified = false;

                ResetCurrentBuildHeight();
            }
            else
            {
                heightHasBeenModified = false;
                buildingToolsEnabled = false;
                constructionEnabled = false;
            }
        }

        public void TogglePathTools()
        {
            if (!pathToolsEnabled)
            {
                pathToolsEnabled = true;
                constructionEnabled = true;
                heightHasBeenModified = false;

                ResetCurrentBuildHeight();
            }
            else
            {
                heightHasBeenModified = false;
                pathToolsEnabled = false;
                constructionEnabled = false;
            }
        }

        public void ToggleSceneryTools()
        {
            if (!sceneryToolsEnabled)
            {
                sceneryToolsEnabled = true;
                constructionEnabled = true;
                heightHasBeenModified = false;

                ResetCurrentBuildHeight();

            }
            else
            {
                heightHasBeenModified = false;
                sceneryToolsEnabled = false;
                constructionEnabled = false;
            }
        }
        #endregion

        /// <summary>
        /// Start construction of the provided objectID.
        /// </summary>
        public void StartConstruction(string objectIDToBuild)
        {
            // Stop any previous construction before changing object type.
            StopBuilding();
            objectToBuildID = objectIDToBuild;
            if (!isBuilding)
            {
                isBuilding = true;
                StartCoroutine(DisplayGhostObjects());
            }
        }

        void ResetCurrentBuildHeight()
        {
            //if (SettingsHelper.AdvancedConstructionModeEnabled)
            //{
            //    currentBuildHeight = mouseInterface.currentMouseCoordinatesOnTileMap.y;
            //}
            //else
            //{
            //    currentBuildHeight = World.World.RoundToNearest(mouseInterface.currentMouseCoordinatesOnTileMap.y, (int)(1 / referenceGhostObject.objectFootprint.y));
            //}
            currentBuildHeight = mouseInterface.currentQuarterTile.TileHeight;
        }

        /// <summary>
        /// When called, stops building and destroys any ghost objects.
        /// </summary>
        public void StopBuilding()
        {
            isBuilding = false;
            foreach (BuildableObject ghost in ghostObjects)
            {
                Destroy(ghost.gameObject);
            }
            ghostObjects.Clear();
            StopCoroutine(DisplayGhostObjects());
        }

        /// <summary>
        /// When building, display the ghost images of the objects when valid.
        /// </summary>
        /// <returns></returns>
        public IEnumerator DisplayGhostObjects()
        {
            referenceGhostObject = AssetManager.BuildableObjectsDictionary[objectToBuildID];
            referenceGhostObject.GenerateObjectFootprint();
            int numberOfObjectsToBuild = 1;
            // Used to track draggable object construction.
            int objectsDisplayed = 0;

            Vector3 ghostObjPos = new Vector3();

            while (isBuilding)
            {
                // Reset objects displayed.
                objectsDisplayed = 0;

                // Reset height, if applicable
                if (!heightHasBeenModified)
                {
                    ResetCurrentBuildHeight();
                }
                // Display one or multiple objects.
                // If the mouse is not held down, we're working with a single object.
                #region Single Object
                if (!mouseInterface.isDragging)
                {
                    numberOfObjectsToBuild = 1;

                    // Display the ghost object for each object, if the number has changed.
                    if (numberOfObjectsToBuild != ghostObjects.Count)
                    {
                        foreach (BuildableObject ghost in ghostObjects)
                        {
                            Destroy(ghost.gameObject);
                        }
                        ghostObjects.Clear();
                        for (int i = 0; i < numberOfObjectsToBuild; i++)
                        {
                            ghostObjects.Add(Instantiate(referenceGhostObject));
                        }
                    }

                    // If the modifier key is pressed, change object height
                    if (mouseInterface.modifierKeyPressed)
                    {
                        while (mouseInterface.modifierKeyPressed)
                        {
                            if (!mouseInterface.isModifyingHeight)
                            {
                                mouseInterface.StartCoroutine(mouseInterface.ChangeHeight());
                            }
                            // If advanced construction is on, build freeform
                            if (SettingsHelper.AdvancedConstructionModeEnabled && referenceGhostObject.canHaveHeightModified && referenceGhostObject.canBeBuiltFreeform)
                            {
                                ghostObjPos.y = currentBuildHeight + referenceGhostObject.objectFootprint.y / 2;
                                heightToBuild = ghostObjPos.y;
                            }
                            // Otherwise, build on the grid.
                            else
                            {
                                ghostObjPos.y = RoundObjectHeightToGridBasedOnObjectType(ghostObjPos.y);
                                heightToBuild = ghostObjPos.y;
                            }
                            foreach (BuildableObject ghost in ghostObjects)
                            {
                                ghost.transform.position = ghostObjPos;
                                // Check clearances for each object, and change object ghost colour accordingly
                                UpdateGhostObjectMaterial(ghost.gameObject, CheckClearances(ghost, false));
                            }
                            yield return new WaitForEndOfFrame();
                        }
                        if (currentBuildHeight != mouseInterface.currentQuarterTile.TileHeight)
                        {
                            heightHasBeenModified = true;
                        }
                    }
                    foreach (BuildableObject ghost in ghostObjects)
                    {
                        switch (ghost.type)
                        {
                            case "Building":
                                {
                                    // If advanced construction is on, build freeform
                                    if (SettingsHelper.AdvancedConstructionModeEnabled && ghost.canBeBuiltFreeform)
                                    {
                                        ghostObjPos = GetObjectPositionFreeform();
                                    }
                                    // Otherwise, build on the grid.
                                    else
                                    {
                                        ghostObjPos = GetObjectPositionOnGrid();
                                    }
                                    ghost.transform.position = ghostObjPos;
                                    ghost.transform.rotation = GetRotationForObject(ghost);
                                    break;
                                }
                            case "Scenery":
                                {
                                    // If advanced construction is on, build freeform
                                    if (SettingsHelper.AdvancedConstructionModeEnabled && ghost.canBeBuiltFreeform)
                                    {
                                        ghostObjPos = GetObjectPositionFreeform();
                                    }
                                    // Otherwise, build on the grid.
                                    else
                                    {
                                        ghostObjPos = GetObjectPositionOnGrid();
                                    }
                                    ghost.transform.position = ghostObjPos;
                                    ghost.transform.rotation = GetRotationForObject(ghost);
                                    break;
                                }
                            case "Wall":
                                {
                                    // If advanced construction is on, build freeform
                                    if (SettingsHelper.AdvancedConstructionModeEnabled && ghost.canBeBuiltFreeform)
                                    {
                                        ghostObjPos = GetObjectPositionFreeform();
                                    }
                                    // Otherwise, build on the grid.
                                    else
                                    {
                                        ghostObjPos = GetObjectPositionOnGrid();
                                    }
                                    ghost.transform.position = ghostObjPos;
                                    ghost.transform.rotation = GetRotationForObject(ghost);
                                    break;
                                }
                            case "Path":
                                {
                                    ghostObjPos = GetObjectPositionOnGrid();

                                    ghost.transform.position = ghostObjPos;
                                    break;
                                }
                            case "PathObject":
                                {
                                    ghostObjPos = GetObjectPositionOnGrid();

                                    ghost.transform.position = ghostObjPos;
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        // Check clearances for each object, and change object ghost colour accordingly
                        UpdateGhostObjectMaterial(ghost.gameObject, CheckClearances(ghost, false));

                    }

                }
                #endregion
                // Otherwise, we're dragging, if the object can be dragged.
                // Determine how many ghost objects to create
                #region Multiple Objects
                else if (referenceGhostObject.isDraggable && mouseInterface.isDragging)
                {
                    // Get number of objects to build here
                    numberOfObjectsToBuild = GetNumberOfDraggedObjectsToBuild();
                    // Display the ghost object for each object, if the number has changed.
                    if (numberOfObjectsToBuild != ghostObjects.Count)
                    {
                        foreach (BuildableObject ghost in ghostObjects)
                        {
                            Destroy(ghost.gameObject);
                        }
                        ghostObjects.Clear();
                        for (int i = 0; i < numberOfObjectsToBuild; i++)
                        {
                            ghostObjects.Add(Instantiate(referenceGhostObject));
                        }
                    }

                    // If the modifier key is pressed, change object height
                    if (mouseInterface.modifierKeyPressed)
                    {
                        while (mouseInterface.modifierKeyPressed)
                        {
                            if (!mouseInterface.isModifyingHeight)
                            {
                                mouseInterface.StartCoroutine(mouseInterface.ChangeHeight());
                            }
                            // If advanced construction is on, build freeform
                            if (SettingsHelper.AdvancedConstructionModeEnabled && referenceGhostObject.canHaveHeightModified && referenceGhostObject.canBeBuiltFreeform)
                            {
                                ghostObjPos.y = currentBuildHeight + referenceGhostObject.objectFootprint.y / 2;
                                heightToBuild = ghostObjPos.y;
                            }
                            // Otherwise, build on the grid.
                            else
                            {
                                ghostObjPos.y = RoundObjectHeightToGridBasedOnObjectType(ghostObjPos.y);
                                heightToBuild = ghostObjPos.y;
                            }
                            foreach (BuildableObject ghost in ghostObjects)
                            {
                                ghost.transform.position = ghostObjPos;
                                // Check clearances for each object, and change object ghost colour accordingly
                                UpdateGhostObjectMaterial(ghost.gameObject, CheckClearances(ghost, false));
                            }
                            yield return new WaitForEndOfFrame();
                        }
                        if(currentBuildHeight != mouseInterface.currentQuarterTile.TileHeight)
                        {
                            heightHasBeenModified = true;
                        }
                        Debug.Log($"Height to build:{heightToBuild}");
                    }
                    foreach (BuildableObject ghost in ghostObjects)
                    {
                        switch (ghost.type)
                        {
                            case "Building":
                                {
                                    // If advanced construction is on, build freeform
                                    if (SettingsHelper.AdvancedConstructionModeEnabled && ghost.canBeBuiltFreeform)
                                    {
                                        ghostObjPos = GetObjectPositionFreeform();
                                    }
                                    // Otherwise, build on the grid.
                                    else
                                    {
                                        ghostObjPos = GetObjectPositionOnGrid();
                                    }
                                    ghost.transform.position = GetObjectPositionWhenDragging(ghostObjPos, objectsDisplayed);
                                    ghost.transform.rotation = GetRotationForObject(ghost);
                                    objectsDisplayed++;
                                    break;
                                }
                            case "Scenery":
                                {
                                    // If advanced construction is on, build freeform
                                    if (SettingsHelper.AdvancedConstructionModeEnabled && ghost.canBeBuiltFreeform)
                                    {
                                        ghostObjPos = GetObjectPositionFreeform();
                                    }
                                    // Otherwise, build on the grid.
                                    else
                                    {
                                        ghostObjPos = GetObjectPositionOnGrid();
                                    }
                                    ghost.transform.position = GetObjectPositionWhenDragging(ghostObjPos, objectsDisplayed);
                                    ghost.transform.rotation = GetRotationForObject(ghost);
                                    objectsDisplayed++;
                                    break;
                                }
                            case "Wall":
                                {
                                    // If advanced construction is on, build freeform
                                    if (SettingsHelper.AdvancedConstructionModeEnabled && ghost.canBeBuiltFreeform)
                                    {
                                        ghostObjPos = GetObjectPositionFreeform();
                                    }
                                    // Otherwise, build on the grid.
                                    else
                                    {
                                        ghostObjPos = GetObjectPositionOnGrid();
                                    }
                                    ghost.transform.position = GetObjectPositionWhenDragging(ghostObjPos, objectsDisplayed);
                                    ghost.transform.rotation = GetRotationForObject(ghost);
                                    objectsDisplayed++;
                                    break;
                                }
                            case "Path":
                                {
                                    // Paths will always be built on the grid.
                                    ghostObjPos = GetObjectPositionOnGrid();

                                    ghost.transform.position = GetObjectPositionWhenDragging(ghostObjPos, objectsDisplayed);
                                    objectsDisplayed++;
                                    break;
                                }
                            case "PathObject":
                                {
                                    // PathObjects will always be built on the grid/on paths (calling this method will handle these specifics)
                                    ghostObjPos = GetObjectPositionOnGrid();

                                    ghost.transform.position = GetObjectPositionWhenDragging(ghostObjPos, objectsDisplayed);
                                    objectsDisplayed++;
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        // Check clearances for each object, and change object ghost colour accordingly
                        UpdateGhostObjectMaterial(ghost.gameObject, CheckClearances(ghost, false));

                    }
                }
                #endregion               
                yield return new WaitForEndOfFrame();
            }
            // Destroy ghost objects and clear ghostObjects list before exiting coroutine.
            foreach (BuildableObject ghost in ghostObjects)
            {
                Destroy(ghost.gameObject);
            }
            ghostObjects.Clear();
            yield return new WaitForEndOfFrame();
        }

        /// <summary>
        /// Updates ghost objects to reflect if they can be built in their current location.
        /// </summary>
        void UpdateGhostObjectMaterial(GameObject ghost, bool isClearToBuild)
        {
            MeshRenderer ghostRenderer = ghost.GetComponent<MeshRenderer>();
            if (isClearToBuild)
            {
                ghostRenderer.sharedMaterial = ghostMaterial;
            }
            else
            {
                ghostRenderer.sharedMaterial = blockedghostMaterial;
            }
        }

        /// <summary>
        /// Round given height to grid based on type of object being built.
        /// </summary>
        /// <param name="heightToRound"></param>
        /// <returns></returns>
        float RoundObjectHeightToGridBasedOnObjectType(float heightToRound)
        {
            float roundedHeight = heightToRound;
            switch (referenceGhostObject.type)
            {
                case "Path":
                    {
                        roundedHeight = World.World.RoundToNearest((currentBuildHeight), 2) + (referenceGhostObject.objectFootprint.y / 2);
                        break;
                    }
                default:
                    {
                        roundedHeight = World.World.RoundToNearest((currentBuildHeight), 4) + (referenceGhostObject.objectFootprint.y / 2);
                        break;
                    }
            }
            return roundedHeight;
        }

        /// <summary>
        /// Returns number of draggable objects to build based on start/current mouse position, drag direction, and object size.
        /// </summary>
        /// <returns></returns>
        int GetNumberOfDraggedObjectsToBuild()
        {
            int numToBuild = 1;
            Vector3 startMousePos = mouseInterface.mouseCoordinatesOnTileMapWhenLeftClicked;
            Vector3 currentMousePos = mouseInterface.currentMouseCoordinatesOnTileMap;
            switch (mouseInterface.GetDragDirection())
            {
                case "N":
                    {
                        numToBuild = Mathf.FloorToInt(Mathf.Abs(currentMousePos.z - startMousePos.z) / (referenceGhostObject.objectFootprint.z)) + 1;
                        break;
                    }
                case "S":
                    {
                        numToBuild = Mathf.FloorToInt(Mathf.Abs(currentMousePos.z - startMousePos.z) / (referenceGhostObject.objectFootprint.z)) + 1;

                        break;
                    }
                case "E":
                    {                        
                        numToBuild = Mathf.FloorToInt(Mathf.Abs(currentMousePos.x - startMousePos.x) / (referenceGhostObject.objectFootprint.x)) + 1;
                        break;
                    }
                case "W":
                    {
                        numToBuild = Mathf.FloorToInt(Mathf.Abs(currentMousePos.x - startMousePos.x) / (referenceGhostObject.objectFootprint.x)) + 1;
                        break;
                    }
                default:
                    {
                        numToBuild = 1;
                        break;
                    }
            }
            // Make sure the number never goes below 1.
            if(numToBuild < 1)
            {
                numToBuild = 1;
            }
            return numToBuild;
        }

        /// <summary>
        /// Returns position to place object on the grid when placing multiple dragged objects.
        /// </summary>
        /// <returns></returns>
        Vector3 GetObjectPositionOnGrid()
        {
            Vector3 objectFootprint = referenceGhostObject.objectFootprint;
            Vector3 currentTileCoords = new Vector3(mouseInterface.currentQuarterTile.TileCoordX, 0, mouseInterface.currentQuarterTile.TileCoordZ);
            Vector3 currentMousePos = mouseInterface.currentMouseCoordinatesOnTileMap;
            string currentVerticeName = mouseInterface.currentVerticeName;
            string currentTileEdgeName = mouseInterface.currentTileEdgeName;
            // If dragging, lock all relevant input parameters.
            if (mouseInterface.isDragging)
            {
                currentTileCoords = new Vector3(mouseInterface.tileWhenLeftClicked.TileCoordX, 0, mouseInterface.tileWhenLeftClicked.TileCoordZ);
                currentVerticeName = mouseInterface.verticeNameWhenLeftClicked;
                currentTileEdgeName = mouseInterface.tileEdgeNameWhenLeftClicked;
            }
            float newX = 0;
            float newHeight;
            float newZ = 0;

            // Calculate object position on grid based on footprint type.
            switch (referenceGhostObject.objectFootprintType)
            {
                case FootprintType.Full:
                    {
                        newX = Mathf.Clamp((currentTileCoords.x + referenceGhostObject.objectFootprint.x / 2), 0, world.worldSize);
                        newZ = Mathf.Clamp((currentTileCoords.z + referenceGhostObject.objectFootprint.z / 2), 0, world.worldSize);
                        break;
                    }
                case FootprintType.EdgeOfTile:
                    {
                        // Adjust position, where footprint X is the length and footprint Z is the width of the wall object.
                        switch (currentTileEdgeName)
                        {
                            case "North":
                                {
                                    newZ = (currentTileCoords.z + 1) - (objectFootprint.z / 2);
                                    newX = currentTileCoords.x + .5f;
                                    break;
                                }
                            case "South":
                                {
                                    newZ = currentTileCoords.z + (objectFootprint.z / 2);
                                    newX = currentTileCoords.x + .5f;
                                    break;
                                }
                            case "East":
                                {
                                    newX = (currentTileCoords.x + 1) - (objectFootprint.z / 2);
                                    newZ = currentTileCoords.z + .5f;
                                    break;
                                }
                            case "West":
                                {
                                    newX = currentTileCoords.x + (objectFootprint.z / 2);
                                    newZ = currentTileCoords.z + .5f;
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                case FootprintType.CornerOfTile:
                    {
                        switch (currentVerticeName)
                        {
                            case "LowerLeft":
                                {
                                    newX = currentTileCoords.x + (objectFootprint.x / 2);
                                    newZ = currentTileCoords.z + (objectFootprint.z / 2);
                                    break;
                                }
                            case "UpperLeft":
                                {
                                    newX = currentTileCoords.x + (objectFootprint.x / 2);
                                    newZ = (currentTileCoords.z + 1) - (objectFootprint.z / 2);
                                    break;
                                }
                            case "UpperRight":
                                {
                                    newX = (currentTileCoords.x + 1) - (objectFootprint.x / 2);
                                    newZ = (currentTileCoords.z + 1) - (objectFootprint.z / 2);
                                    break;
                                }
                            case "LowerRight":
                                {
                                    newX = (currentTileCoords.x + 1) - (objectFootprint.x / 2);
                                    newZ = currentTileCoords.z + (objectFootprint.z / 2);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                case FootprintType.PartialTile:
                    {
                        newX = World.World.RoundToNearest(currentMousePos.x, (int)(1 / objectFootprint.x));
                        newZ = World.World.RoundToNearest(currentMousePos.z, (int)(1 / objectFootprint.z));

                        // Adjust for object size.
                        newX += (objectFootprint.x / 2);
                        newZ += (objectFootprint.z / 2);
                        break;
                    }
                case FootprintType.Path:
                    {
                        newX = Mathf.Clamp((currentTileCoords.x + referenceGhostObject.objectFootprint.x / 2), 0, world.worldSize);
                        newZ = Mathf.Clamp((currentTileCoords.z + referenceGhostObject.objectFootprint.z / 2), 0, world.worldSize);
                        break;
                    }
                case FootprintType.PathObject:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            #region CameraAdjustmentForObjPos
            //// Make object always appear 'above' mouse cursor based on camera rotation
            //float camRotation = mouseInterface.cameraController.currentCameraRotation;
            //if (camRotation >= 0 && camRotation <= 90)
            //{
            //    newX = Mathf.Clamp((currentTileCoords.x + referenceGhostObject.objectFootprint.x / 2), 0, world.worldSize);
            //    newZ = Mathf.Clamp((currentTileCoords.z + referenceGhostObject.objectFootprint.z / 2), 0, world.worldSize);
            //}
            //else if (camRotation > 90 && camRotation < 180)
            //{
            //    newX = Mathf.Clamp((currentTileCoords.x + referenceGhostObject.objectFootprint.x / 2), 0, world.worldSize);
            //    newZ = Mathf.Clamp((currentTileCoords.z), 0, world.worldSize);
            //}
            //else if (camRotation >= 180 && camRotation <= 270)
            //{
            //    newX = Mathf.Clamp((currentTileCoords.x), 0, world.worldSize);
            //    newZ = Mathf.Clamp((currentTileCoords.z), 0, world.worldSize);
            //}
            //else if (camRotation > 270 && camRotation < 360)
            //{
            //    newX = Mathf.Clamp((currentTileCoords.x), 0, world.worldSize);
            //    newZ = Mathf.Clamp((currentTileCoords.z + referenceGhostObject.objectFootprint.z / 2), 0, world.worldSize);
            //}
            #endregion

            //currentBuildHeight = World.World.RoundToNearestQuarter(currentBuildHeight);
            newHeight = heightToBuild + (referenceGhostObject.objectFootprint.y / 2);

            return new Vector3(newX, newHeight, newZ);
        }

        /// <summary>
        /// Returns position to place object based on mouse position when placing multiple dragged objects.
        /// </summary>
        /// <returns></returns>
        Vector3 GetObjectPositionFreeform()
        {
            float newX;
            float newHeight;
            float newZ;
            Vector3 mousePosition = mouseInterface.currentMouseCoordinatesOnTileMap;
            // If dragging, use mouse coordinates when clicked instead
            if (mouseInterface.isDragging)
            {
                mousePosition = mouseInterface.mouseCoordinatesOnTileMapWhenLeftClicked;
            }

            newX = Mathf.Clamp((mousePosition.x), .5f * referenceGhostObject.objectFootprint.x, world.worldSize - .5f * referenceGhostObject.objectFootprint.x);
            newHeight = heightToBuild + (referenceGhostObject.objectFootprint.y / 2);
            newZ = Mathf.Clamp((mousePosition.z), .5f * referenceGhostObject.objectFootprint.z, world.worldSize - .5f * referenceGhostObject.objectFootprint.z);

            return new Vector3(newX, newHeight, newZ);
        }

        /// <summary>
        /// Return position of object based on its index in the dragged objects being built and the footprint of the object.
        /// </summary>
        /// <param name="ghostObjPos"></param>
        /// <param name="objectsDisplayed"></param>
        /// <returns></returns>
        Vector3 GetObjectPositionWhenDragging(Vector3 ghostObjPos, int objectsDisplayed)
        {
            Vector3 newPos = ghostObjPos;
            string direction = mouseInterface.GetDragDirection();

            switch (direction)
            {
                case "N":
                    {
                        newPos.z += referenceGhostObject.objectFootprint.z * objectsDisplayed;
                        break;
                    }
                case "S":
                    {
                        newPos.z -= referenceGhostObject.objectFootprint.z * objectsDisplayed;
                        break;
                    }
                case "E":
                    {
                        newPos.x += referenceGhostObject.objectFootprint.x * objectsDisplayed;
                        break;
                    }
                case "W":
                    {
                        newPos.x -= referenceGhostObject.objectFootprint.x * objectsDisplayed;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return newPos;
        }

        /// <summary>
        /// Rotate current build object(s). Will rotate by 90 degrees,
        /// or freeform if rotate key is held down, advanced construction is enabled, and the object is allowed to be rotated freely.
        /// </summary>
        IEnumerator RotateObject()
        {
            isRotating = true;
            // If advanced construction is off, rotate by 90 degrees.
            if (!SettingsHelper.AdvancedConstructionModeEnabled)
            {
                RotateObjectBy90Degrees();
            }
            else
            {
                // If the object can be rotated freely
                if (referenceGhostObject.isFullyRotateable)
                {
                    // do freeform rotation here
                    while (Input.GetKeyDown(KeyCode.Z))
                    {

                        yield return new WaitForEndOfFrame();
                    }
                }
                // Otherwise
                else
                {
                    // Rotate by 90
                    RotateObjectBy90Degrees();
                }
            }
            isRotating = false;
        }

        /// <summary>
        /// Rotate current build object(s) by 90 degrees.
        /// </summary>
        void RotateObjectBy90Degrees()
        {
            if (currentBuildRotation >= 270)
            {
                currentBuildRotation = 0;
            }
            else
            {
                currentBuildRotation += 90;
            }
        }

        /// <summary>
        /// Return rotation for current build object based on object's footprint type. 
        /// </summary>
        /// <returns></returns>
        Quaternion GetRotationForObject(BuildableObject objectToRotate)
        {
            Quaternion rotation = new Quaternion();
            float newRotation = currentBuildRotation;

            // Declare input parameters.
            Vector3 objectFootprint = objectToRotate.objectFootprint;
            Vector3 currentTileCoords = new Vector3(mouseInterface.currentQuarterTile.TileCoordX, 0, mouseInterface.currentQuarterTile.TileCoordZ);
            string currentVerticeName = mouseInterface.currentVerticeName;
            string currentTileEdgeName = mouseInterface.currentTileEdgeName;

            // If dragging, lock all relevant input parameters.
            if (mouseInterface.isDragging)
            {
                currentTileCoords = new Vector3(mouseInterface.tileWhenLeftClicked.TileCoordX, 0, mouseInterface.tileWhenLeftClicked.TileCoordZ);
                currentVerticeName = mouseInterface.verticeNameWhenLeftClicked;
                currentTileEdgeName = mouseInterface.tileEdgeNameWhenLeftClicked;
            }

            // Calculate object position on grid based on footprint type.
            switch (objectToRotate.objectFootprintType)
            {
                case FootprintType.Full:
                    {
                        break;
                    }
                case FootprintType.EdgeOfTile:
                    {
                        if (SettingsHelper.AdvancedConstructionModeEnabled)
                        {
                            break;
                        }
                        else
                        {
                            switch (currentTileEdgeName)
                            {
                                case "North":
                                    {
                                        newRotation = 180;
                                        break;
                                    }
                                case "South":
                                    {
                                        newRotation = 0;
                                        break;
                                    }
                                case "East":
                                    {
                                        newRotation = 270;
                                        break;
                                    }
                                case "West":
                                    {
                                        newRotation = 90;
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case FootprintType.CornerOfTile:
                    {                      
                        break;
                    }
                case FootprintType.PartialTile:
                    {
                        break;
                    }
                case FootprintType.Path:
                    {
                        break;
                    }
                case FootprintType.PathObject:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            rotation = Quaternion.Euler(0, newRotation, 0);

            return rotation;
        }

        public void DeleteObjectAtCursor()
        {
            // If we are currently over an object.
            if(mouseInterface.currentlySelectedBuildableObject != null)
            {
                // Attempt to delete it.
                RemoveObject(mouseInterface.currentlySelectedBuildableObject);
            }
        }

        /// <summary>
        /// Attempt to build the current ghost objects.
        /// </summary>
        public void AttemptToBuildGhostObjects()
        {
            Debug.Log("Attempting to build objects");
            autoDeleteableObjectsToDelete.Clear();
            // Attempt to build each object in turn.
            foreach (BuildableObject objectToBuild in ghostObjects)
            {
                // Perform all necessary checks before building
                if (CheckIfCanAffordToBuild())
                {
                    // If there are no clearance issues, build the object.
                    if (CheckClearances(objectToBuild, true))
                    {
                        BuildObject(AssetManager.BuildableObjectsDictionary[objectToBuild.objectID], objectToBuild.transform.position, objectToBuild.transform.rotation);
                    }
                    // Otherwise, throw clearance error message.
                    else
                    {
                        ErrorHelper.OnErrorOccurred(this, new ErrorOccurredEventArgs(ErrorType.FailedClearanceCheck));
                    }
                }
                else if (!CheckIfCanAffordToBuild())
                {
                    // throw not enough money error message
                    ErrorHelper.OnErrorOccurred(this, new ErrorOccurredEventArgs(ErrorType.CantAffordSingle));
                }

            }
            foreach(BuildableObject toDelete in autoDeleteableObjectsToDelete)
            {
                RemoveObject(toDelete);
            }
            // If the object isn't multi-buildable, stop construction.
            if (!referenceGhostObject.multiBuildable)
            {
                isBuilding = false;
            }
        }

        /// <summary>
        /// Build the provided object with the provided parameters.
        /// </summary>
        /// <param name="objectToBuild"></param>
        public void BuildObject(BuildableObject objectToBuild, Vector3 objectPosition, Quaternion objectRotation = new Quaternion())
        {
            BuildableObject newObject = null;
            switch (objectToBuild.type)
            {
                case "Building":
                    {
                        newObject = Instantiate(objectToBuild);
                        break;
                    }
                case "Scenery":
                    {
                        newObject = Instantiate(objectToBuild);
                        break;
                    }
                case "Wall":
                    {
                        newObject = Instantiate(objectToBuild);
                        break;
                    }
                case "Path":
                    {
                        newObject = Instantiate(objectToBuild);
                        break;
                    }
                case "PathObject":
                    {
                        newObject = Instantiate(objectToBuild);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            newObject.transform.position = objectPosition;
            newObject.transform.rotation = objectRotation;

            newObject.InitializeObject();
            OnObjectBuilt(new ObjectEventArgs(newObject));
        }

        #region Events

        #region ObjectBuilt
        public delegate void ObjectBuiltEventHandler(object sender, ObjectEventArgs e);

        /// <summary>
        /// Raised when an object is built.
        /// </summary>
        public event ObjectBuiltEventHandler ObjectBuilt;

        public virtual void OnObjectBuilt(ObjectEventArgs e)
        {
            ObjectBuilt?.Invoke(this, e);
        }
        #endregion
        #region ObjectDeleted
        public delegate void ObjectDeletedEventHandler(object sender, ObjectEventArgs e);

        /// <summary>
        /// Raised when an object is deleted.
        /// </summary>
        public event ObjectDeletedEventHandler ObjectDeleted;

        public virtual void OnObjectDeleted(ObjectEventArgs e)
        {
            ObjectDeleted?.Invoke(this, e);
        }
        #endregion
        #endregion

        /// <summary>
        /// Attempt to remove/destroy the given object.
        /// </summary>
        public void RemoveObject(BuildableObject objectToDestroy)
        {
            // Check if it can be deleted            
            // Check if we can afford to delete it
            // Check for any other possible restrictions or warnings (ie don't delete the wall of an exhibit with animals in it!)
            if (objectToDestroy.canBeDeleted)
            {
                objectToDestroy.Delete();
                OnObjectDeleted(new ObjectEventArgs(objectToDestroy));
            }
        }

        /// <summary>
        /// Return true if the construction is valid.
        /// </summary>
        /// <returns></returns>
        bool CheckClearances(BuildableObject objectToCheck, bool isBuilding)
        {
            bool canBuild = true;

            List<BuildableObject> tempAutoDeleteableList = new List<BuildableObject>();

            if (clearancesEnabled && objectToCheck.hasClearances)
            {
                // Check clearances
                // autoDeletable objects
                // !hasClearances objects

                Collider col = objectToCheck.GetComponent<Collider>();
                // Get all colliders overlapping the current object
                Collider[] overlap = Physics.OverlapBox(col.bounds.center, col.bounds.extents * .999f);
                // Check each collider
                foreach (Collider other in overlap)
                {
                    //Debug.Log("Checking for collision with" + other.gameObject.name);
                    // If the colliders intersect
                    if (col.bounds.Intersects(other.bounds))
                    {
                        // If the other object is terrain.
                        if (other.gameObject.GetComponent<Chunk>())
                        {
                            //Debug.Log("Check 1");
                            canBuild = false;
                            continue;
                        }
                        BuildableObject otherObject = other.GetComponent<BuildableObject>();
                        if (otherObject == null || other == col)
                        {
                            //Debug.Log("Check 2");
                            continue;
                        }
                        // If the other object does not qualify for clearance checks, ignore it.
                        if (otherObject.hasClearances == false)
                        {
                            //Debug.Log("Check 3");

                            continue;
                        }
                        // If the ghostObject is not autoDeletable, ignore all autoDeletable objects.
                        if (objectToCheck.autoDeletable == false)
                        {
                            //Debug.Log("Check 4");
                            if (otherObject.autoDeletable)
                            {
                                //Debug.Log("Check 5");
                                tempAutoDeleteableList.Add(otherObject);
                                continue;
                            }
                        }
                        // At this point, only valid clearance conflicts should remain.
                        //Debug.Log("Unable to build; Conflict object name: " + other.name);
                        canBuild = false;
                        tempAutoDeleteableList.Clear();
                        break;
                    }
                }   

            }
            else
            {
                // If clearances are disabled, return true.
                canBuild = true;
            }

            // Temporarily bypass clearance checks.
            //canBuild = true;
            // If we can build, add to auto-deleteable list 
            if (canBuild && isBuilding)
            {
                autoDeleteableObjectsToDelete.AddRange(tempAutoDeleteableList);
            }
            
            return canBuild;
        }

        /// <summary>
        /// Return true if the construction can be paid for, or if in sandbox mode.
        /// </summary>
        /// <returns></returns>
        bool CheckIfCanAffordToBuild()
        {
            bool canAfford = true;

            return canAfford;
        }


    }
}
