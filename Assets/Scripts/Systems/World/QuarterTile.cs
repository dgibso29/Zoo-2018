using System.Collections.Generic;
using UnityEngine;

namespace Zoo.Systems.World
{
    [System.Serializable]
    public class QuarterTile
    {

        public Vector3 upperLeft;           // NW Corner
        public Vector3 upperRight;          // NE Corner
        public Vector3 lowerRight;          // SE Corner
        public Vector3 lowerLeft;           // SW Corner    
        public Vector3 bottomUpperLeft;     // NW Bottom corner
        public Vector3 bottomUpperRight;    // NE Bottom corner
        public Vector3 bottomLowerRight;    // SE Bottom corner
        public Vector3 bottomLowerLeft;     // SW Bottom corner

        /// <summary>
        /// Tracks what type of grid texture the tile uses, where 7 = SW, 8 = NW, 9 = NE, 10 = SE.
        /// </summary>
        public int gridType;
        public float bottomHeight;
        public float height;
        public bool isSlope = false;
        /// <summary>
        /// ID of enclosure tile is in, if any.
        /// </summary>
        public int enclosureID = 0;

        [SerializeField]
        private float tileSize = .25f; //tile size in meters
        [SerializeField]
        private bool isOwnedByZoo = false;
        [SerializeField]
        private bool hasBeenUpdated = false;
        [SerializeField]
        private float centerX;
        [SerializeField]
        private float centerZ;
        [SerializeField]
        private int terrainType;
        [SerializeField]
        private int cliffType;
        [SerializeField]
        private int overlayType; // array index for selectionbox tiles

        private float heightIncrement = 0.25f;

        //private List<BuildableObject> tileObjects = new List<BuildableObject>(); // Array of objects on the tile -- used to track object location, save/load, everything

        // Tile Terrain and Cliff textures are always ordered such that Grass = 0, Grass Cliff = 1, Sand = 2, Sand Cliff = 3, so on.

        public QuarterTile(float centerX, float centerZ, float height, float size, int terrainType, int overlayType, float bottomHeight, int cliffType)
        {
            this.height = height;
            this.bottomHeight = bottomHeight;
            this.centerX = centerX;
            this.centerZ = centerZ;
            this.terrainType = terrainType;
            this.cliffType = cliffType;
            this.overlayType = overlayType;
            //make sure we're set to the right size
            tileSize = size;
            //float halfSize = size / 2f;

            //setup the vectors! 
            upperLeft = new Vector3(centerX, height, centerZ + 1);                  // NW corner
            upperRight = new Vector3(centerX + 1, height, centerZ + 1);             // NE corner
            lowerRight = new Vector3(centerX + 1, height, centerZ);                 // SE corner
            lowerLeft = new Vector3(centerX, height, centerZ);                      // SW corner
                                                                                    // Set up the bottom vectors
            bottomUpperLeft = new Vector3(centerX, bottomHeight, centerZ + 1);      // NW bottom corner
            bottomUpperRight = new Vector3(centerX + 1, bottomHeight, centerZ + 1);  // NE bottom corner
            bottomLowerRight = new Vector3(centerX + 1, bottomHeight, centerZ);     // SE bottom corner
            bottomLowerLeft = new Vector3(centerX, bottomHeight, centerZ);         // SW bottom corner


        }

        public int TileCoordX
        {
            get { return (int)centerX; }
        }

        public int TileCoordZ
        {
            get { return (int)centerZ; }
        }

        public float TileHeight
        {
            get { return height; }
            set { height = value; }
        }

        public int TerrainType
        {
            get { return terrainType; }
            set { terrainType = value; }
        }

        public int CliffType
        {
            get { return cliffType; }
            set { cliffType = value; }
        }
        public int OverlayTileType
        {
            get { return overlayType; }
            set { overlayType = value; }
        }
        public bool HasBeenUpdated
        {
            get { return hasBeenUpdated; }
            set { hasBeenUpdated = value; }
        }

        //public List<BuildableObject> Objects
        //{
        //    get { return tileObjects; }
        //    set { tileObjects = value; }
        //}

        //public void AddObjectToTile(BuildableObject newObject)
        //{
        //    tileObjects.Add(newObject);
        //}

        public bool IsOwnedByZoo
        {
            get { return isOwnedByZoo; }
            set { isOwnedByZoo = value; }
        }

        /// <summary>
        /// Check if this tile has a path at the height of the provided path, including sloped paths.
        /// </summary>
        /// <param name="pathToCheck"></param>
        /// <returns></returns>
        //public bool CheckForPath(Path pathToCheck, float heightScale)
        //{
        //    foreach (BuildableObject p in Objects)
        //    {
        //        if (p.GetComponent<Path>() != null)
        //            if (p.objectHeight == pathToCheck.objectHeight && !p.GetComponent<Path>().IsSlope)
        //            {
        //                return true;
        //            }
        //            else if (p.GetComponent<Path>().IsSlope && (p.objectHeight - (heightScale / 2) == pathToCheck.objectHeight || p.objectHeight + (heightScale / 2) == pathToCheck.objectHeight))
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //    }
        //    return false;
        //}
        ///// <summary>
        ///// Get the path object at the same height of the provided path, including if sloped.
        ///// </summary>
        ///// <param name="pathToCheck"></param>
        ///// <param name="heightScale"></param>
        ///// <returns></returns>
        //public Path GetPath(Path pathToCheck, float heightScale)
        //{
        //    foreach (BuildableObject p in Objects)
        //    {
        //        if (p.GetComponent<Path>() != null)
        //            if (p.objectHeight == pathToCheck.objectHeight && !p.GetComponent<Path>().IsSlope)
        //        {
        //            return p.GetComponent<Path>();
        //        }
        //        else if (p.GetComponent<Path>().IsSlope && (p.objectHeight - (heightScale / 2) == pathToCheck.objectHeight || p.objectHeight + (heightScale / 2) == pathToCheck.objectHeight))
        //        {
        //            return p.GetComponent<Path>();
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    return null;
        //}
        //public Path GetPathAtHeight(float height)
        //{
        //    foreach (BuildableObject p in Objects)
        //    {
        //        if (p.GetComponent<Path>() != null)
        //            if (p.objectHeight == height)
        //            {
        //                return p.GetComponent<Path>();
        //            }
        //    }
        //    return null;
        //}

        public void Recalculate()
        {
            //reset our vectors
            upperLeft = new Vector3(centerX, height, centerZ + 1);
            upperRight = new Vector3(centerX + 1, height, centerZ + 1);
            lowerRight = new Vector3(centerX + 1, height, centerZ);
            lowerLeft = new Vector3(centerX, height, centerZ);
        }

        public void ReSetStats()
        {
            height = Mathf.Max(upperRight.y, upperLeft.y, lowerLeft.y, lowerRight.y);

            if ((upperLeft.y + upperRight.y + lowerLeft.y + lowerRight.y) / 4f != height)
            {
                isSlope = true;
            }
            else
            {
                isSlope = false;
            }
        }

        /// <summary>
        /// Return vertice on tile from map coordinates.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetVertice(float inputX, float inputZ)
        {
            if (inputX == lowerLeft.x && inputZ == lowerLeft.z)
            {
                return lowerLeft;
            }
            else if (inputX == upperLeft.x && inputZ == upperLeft.z)
            {
                return upperLeft;
            }
            else if (inputX == upperRight.x && inputZ == upperRight.z)
            {
                return upperRight;
            }
            else if (inputX == lowerRight.x && inputZ == lowerRight.z)
            {
                return lowerRight;
            }
            else
            {
                Debug.Log("Vertice not found, breaking!");
                return new Vector3(-1, 0, -1);
            }
        }

        /// <summary>
        /// Return vertice name on tile from map coordinates.
        /// </summary>
        /// <returns></returns>
        public string GetVerticeName(float inputX, float inputZ)
        {
            if (inputX == lowerLeft.x && inputZ == lowerLeft.z)
            {
                return "LowerLeft";
            }
            else if (inputX == upperLeft.x && inputZ == upperLeft.z)
            {
                return "UpperLeft";
            }
            else if (inputX == upperRight.x && inputZ == upperRight.z)
            {
                return "UpperRight";
            }
            else if (inputX == lowerRight.x && inputZ == lowerRight.z)
            {
                return "LowerRight";
            }
            else
            {
                return "Not found";
            }
        }

        /// <summary>
        /// Update vertice based on vertice name.
        /// </summary>
        /// <param name="verticeName"></param>
        /// <param name="newVerticeInfo"></param>
        public void UpdateVerticeFromName(string verticeName, Vector3 newVerticeInfo)
        {
            switch (verticeName)
            {
                case "LowerLeft":
                    {
                        lowerLeft = newVerticeInfo;
                        break;
                    }
                case "UpperLeft":
                    {
                        upperLeft = newVerticeInfo;
                        break;
                    }
                case "UpperRight":
                    {
                        upperRight = newVerticeInfo;
                        break;
                    }
                case "LowerRight":
                    {
                        lowerRight = newVerticeInfo;
                        break;
                    }
                default:
                    {
                        break;
                    }

            }
        }

        /// <summary>
        /// Check that all 4 vertices are within the allowed height range from one another, and correct them if necessary.
        /// The provided string should correspond to the most recently updated vertice.
        /// </summary>
        public void ValidateVertices(string verticeUpdated)
        {
            switch (verticeUpdated)
            {
                case "LowerLeft":
                    {
                        // Check each vertice in turn
                        if (lowerLeft.y != upperLeft.y)
                        {
                            if (lowerLeft.y > upperLeft.y + heightIncrement)
                            {
                                upperLeft.y = lowerLeft.y - heightIncrement;
                            }
                            else if (lowerLeft.y < upperLeft.y - heightIncrement)
                            {
                                upperLeft.y = lowerLeft.y + heightIncrement;
                            }
                        }
                        if (lowerLeft.y != lowerRight.y)
                        {
                            if (lowerLeft.y > lowerRight.y + heightIncrement)
                            {
                                lowerRight.y = lowerLeft.y - heightIncrement;
                            }
                            else if (lowerLeft.y < lowerRight.y - heightIncrement)
                            {
                                lowerRight.y = lowerLeft.y + heightIncrement;
                            }
                        }
                        if (lowerLeft.y != upperRight.y)
                        {
                            if (lowerLeft.y > upperRight.y + (heightIncrement) * 2)
                            {
                                upperRight.y = lowerLeft.y - (heightIncrement) * 2;
                            }
                            else if (lowerLeft.y < upperRight.y - (heightIncrement) * 2)
                            {
                                upperRight.y = lowerLeft.y + (heightIncrement) * 2;
                            }
                        }
                        break;
                    }
                case
                    "UpperLeft":
                    {
                        //Debug.Log("Checking upper left vertice");
                        // Check each vertice in turn
                        if (upperLeft.y != upperRight.y)
                        {
                            if (upperLeft.y > upperRight.y + heightIncrement)
                            {
                                upperRight.y = upperLeft.y - heightIncrement;
                            }
                            else if (upperLeft.y < upperRight.y - heightIncrement)
                            {
                                upperRight.y = upperLeft.y + heightIncrement;
                            }
                        }
                        if (upperLeft.y != lowerLeft.y)
                        {
                            if (upperLeft.y > lowerLeft.y + heightIncrement)
                            {
                                lowerLeft.y = upperLeft.y - heightIncrement;
                            }
                            else if (upperLeft.y < lowerLeft.y - heightIncrement)
                            {
                                lowerLeft.y = upperLeft.y + heightIncrement;
                            }
                        }
                        if (upperLeft.y != lowerRight.y)
                        {
                            if (upperLeft.y > lowerRight.y + (heightIncrement) * 2)
                            {
                                lowerRight.y = upperLeft.y - (heightIncrement) * 2;
                            }
                            else if (upperLeft.y < lowerRight.y - (heightIncrement) * 2)
                            {
                                lowerRight.y = upperLeft.y + (heightIncrement) * 2;
                            }
                        }
                        break;
                    }
                case
                    "UpperRight":
                    {
                        // Check each vertice in turn
                        if (upperRight.y != lowerRight.y)
                        {
                            if (upperRight.y > lowerRight.y + heightIncrement)
                            {
                                lowerRight.y = upperRight.y - heightIncrement;
                            }
                            else if (upperRight.y < lowerRight.y - heightIncrement)
                            {
                                lowerRight.y = upperRight.y + heightIncrement;
                            }
                        }
                        if (upperRight.y != upperLeft.y)
                        {
                            if (upperRight.y > upperLeft.y + heightIncrement)
                            {
                                upperLeft.y = upperRight.y - heightIncrement;
                            }
                            else if (upperRight.y < upperLeft.y - heightIncrement)
                            {
                                upperLeft.y = upperRight.y + heightIncrement;
                            }
                        }
                        if (upperRight.y != lowerLeft.y)
                        {
                            if (upperRight.y > lowerLeft.y + (heightIncrement) * 2)
                            {
                                lowerLeft.y = upperRight.y - (heightIncrement) * 2;
                            }
                            else if (upperRight.y < lowerLeft.y - (heightIncrement) * 2)
                            {
                                lowerLeft.y = upperRight.y + (heightIncrement) * 2;
                            }
                        }
                        break;
                    }
                case
                    "LowerRight":
                    {
                        // Check each vertice in turn
                        if (lowerRight.y != upperRight.y)
                        {
                            if (lowerRight.y > upperRight.y + heightIncrement)
                            {
                                upperRight.y = lowerRight.y - heightIncrement;
                            }
                            else if (lowerRight.y < upperRight.y - heightIncrement)
                            {
                                upperRight.y = lowerRight.y + heightIncrement;
                            }
                        }
                        if (lowerRight.y != lowerLeft.y)
                        {
                            if (lowerRight.y > lowerLeft.y + heightIncrement)
                            {
                                lowerLeft.y = lowerRight.y - heightIncrement;
                            }
                            else if (lowerRight.y < lowerLeft.y - heightIncrement)
                            {
                                lowerLeft.y = lowerRight.y + heightIncrement;
                            }
                        }
                        if (lowerRight.y != upperLeft.y)
                        {
                            if (lowerRight.y > upperLeft.y + (heightIncrement) * 2)
                            {
                                upperLeft.y = lowerRight.y - (heightIncrement) * 2;
                            }
                            else if (lowerRight.y < upperLeft.y - (heightIncrement) * 2)
                            {
                                upperLeft.y = lowerRight.y + (heightIncrement) * 2;
                            }
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            ReSetStats();
        }
    }
}
