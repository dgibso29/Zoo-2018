using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Zoo.Utilities;

namespace Zoo.Systems.World
{
    public class World : MonoBehaviour
    {

        // Reference TileDataTile for list of terrain types

        int _sizeX;
        int _sizeY;

        //tweakables
        public int minWorldSize = 10;
        public int maxWorldSize = 500;
        public int worldSize = 250; //tiles per side of the world -- MUST BE 1X1, 2X2, 3X3, 4X4, 5X5, ETC.
        int oldWorldSize = 250;
        public int chunkSize = 40;  //Number of tiles per side of each chunk
        public float tileSize = .25f; //meters per side of each tiles
                                      /// <summary>
                                      /// Increment between each height tile height level.
                                      /// </summary>
        public float tileHeightStep = 0.5f;

        public bool gridEnabled = true;

        /// <summary>
        /// Maximum height of world.
        /// </summary>
        public float maxHeight = 25f;
        /// <summary>
        /// Bottom of world heightmap.
        /// </summary>
        public float bottomOfMapHeight = -6f;

        /// <summary>
        /// World height if world is flat.
        /// </summary>
        public float baseElevation = 0f;


        // Random generation parameter limits
        public float minSeed = .02f;
        public float maxSeed = .06f;

        public float minFlatness = 0f;
        public float maxFlatness = 2f;

        public float minFreq = 0.25f;
        public float maxFreq = .75f;

        public float minAmp = 0.5f;
        public float maxAmp = 1.5f;

        public float minPersistence = 0.25f;
        public float maxPersistence = 0.5f;

        public int minOctaves = 4;
        public int maxOctaves = 8;


        // Map Generation Variables
        /// <summary>
        /// Minimum height of generated terrain. Must be clamped 1 above bottom of map height.
        /// </summary>
        public float minGenerationHeight = -2f;
        /// <summary>
        /// Maximum height of generated terrain. Must be clamped 1 below maximum height.
        /// </summary>
        public float maxGenerationHeight = 10f;

        /// <summary>
        /// Determines the water level in random map generation.
        /// </summary>
        public float waterLevel = 0f;
        /// <summary>
        /// Minimum elevation for snow to appear.
        /// </summary>
        public float snowLine = 8f;


        public float elevationSeed;
        public float moistureSeed;

        /// <summary>
        /// Determines flatness of random map generation.
        /// </summary>
        public float flatness = 1f;

        /// <summary>
        /// Starting frequency for elevation. Frequency of subsequent octaves will be based upon this.
        /// </summary>
        public float baseElevationFrequency = .5f;
        /// <summary>
        /// Starting frequency for moisture. Frequency of subsequent octaves will be based upon this.
        /// </summary>
        public float baseMoistureFrequency = .5f;
        /// <summary>
        /// Starting amplitude for elevation. Amplitude of subsequent octaves will be based upon this.
        /// </summary>
        public float baseElevationAmplitude = 1;
        /// <summary>
        /// Starting amplitude for moisture. Amplitude of subsequent octaves will be based upon this.
        /// </summary>
        public float baseMoistureAmplitude = 1;

        public float elevationAmplitudePersistence = .5f;
        public float moistureAmplitudePersistence = .5f;

        public int numberOfElevationOctaves = 4;
        public int numberOfMoistureOctaves = 4;

        float[] elevationOctaveAmplitude;
        float[] elevationOctaveFrequency;
        float[] moistureOctaveAmplitude;
        float[] moistureOctaveFrequency;

        public int defaultTerrainType = 0; // Grass Terrain
        public int defaultCliffType = 1; // Dirt Cliff
        public int defaultOverlayTileType = 0;
        Material[] terrainTextureMaterials;
        public UnityEngine.GameObject selectionBoxMap;
        //public GameObject chunk;  // used to create all chunks -- chunk at world pos (5,0,5)

        private int numberOfChunks; // Calculate the number of chunks based on World size
        private static QuarterTile[,] mapData;    //set of all the tiles that make up the world
        public Chunk[,] chunks; //set of all the chunks we're going to use to draw the world. Chunks will be arranged in coordinates much like tiles -- (0,0), (0,1), (1,0), (1,1), etc
        public Chunk[,] overlayChunks; //set of all the selection box chunks
        private GameObject[,] chunkObjects; // Set all chunk gameobjects to this array
        private GameObject[,] chunkObjectsOverlay; // Set all chunk gameobjects for selection box to this array

        // Reference components


        // Use this for initialization
        void Start()
        {
            // Generate the world!
            GenerateWorld();
            ToggleOverlayGrid();

            // TODO: Move to Game Manager/Startup Utility
            StartCoroutine(GameStartupHelper.RunStartupProcedures());

        }

        public void GenerateWorld()
        {
            //Pathfinding.world = GetComponent<World>();
            if (worldSize % chunkSize > 0)
            {
                numberOfChunks = worldSize / chunkSize + 1;                
            }
            else
            {
                numberOfChunks = worldSize / chunkSize;
            }

            //initiate things
            if (mapData == null)
            {
                mapData = new QuarterTile[worldSize + 1, worldSize + 1];
                Debug.Log("Created mapData array");
                GenerateFlatTerrain();
            }

            //generate the terrain!
            //GenerateRandomTerrain();
            //RandomizeTileHeight(MapData[0, 0], .8931389f, 0f);
            //RandomizeTileHeight(MapData[1, 0], .8931389f, 0f);
            //RandomizeTileHeight(MapData[0, 1], .8931389f, 0f);
            //RandomizeTileHeight(MapData[1, 1], .8931389f, 0f);


            int chunkArraySize;
            //Debug.Log(numberOfChunks + " " + numberOfChunks % 5);
            chunkArraySize = numberOfChunks;
            //Instantiate the chunk & chunkobject arrays
            chunkObjects = new GameObject[chunkArraySize, chunkArraySize];
            chunks = new Chunk[chunkArraySize, chunkArraySize];

            //Create each chunk
            for (int x = 0; x < chunkArraySize; x++)
            {
                for (int z = 0; z < chunkArraySize; z++)
                {
                    Vector3 newChunkPos = new Vector3(x * chunkSize, 0f, z * chunkSize);
                    chunkObjects[x, z] = new GameObject("Chunk (" + x + "," + z + ")");
                    chunkObjects[x, z].layer = 8;
                    chunkObjects[x, z].transform.parent = gameObject.transform;
                    chunkObjects[x, z].transform.position = newChunkPos;
                    chunkObjects[x, z].AddComponent<Chunk>();
                    chunks[x, z] = chunkObjects[x, z].GetComponent<Chunk>();
                }
            }

            //Debug.Log(chunks.GetLength(0));

            //Tell each chunk to draw their share of the mesh
            for (int x = 0; x < chunkArraySize; x++)
            {
                for (int z = 0; z < chunkArraySize; z++)
                {
                    Chunk newChunk = chunks[x, z];
                    newChunk.ChunkPosition = new Vector2(x, z);
                    newChunk.ChunkWorldPosition = new Vector3(x * chunkSize, 0f, z * chunkSize);
                    newChunk.tileSize = tileSize;
                    newChunk.chunkSize = chunkSize;
                    //newChunk.TerrainTextureMaterials = terrainTextureMaterials;
                    newChunk.DrawTiles(mapData, false);
                }
            }

            //Instantiate the chunk & chunkobject arrays for selectionBoxMap
            chunkObjectsOverlay = new GameObject[chunkArraySize, chunkArraySize];
            overlayChunks = new Chunk[chunkArraySize, chunkArraySize];

            //Create each selection box chunk
            for (int x = 0; x < chunkArraySize; x++)
            {
                for (int z = 0; z < chunkArraySize; z++)
                {
                    Vector3 newChunkPos = new Vector3(x * chunkSize, 0f, z * chunkSize);
                    chunkObjectsOverlay[x, z] = new GameObject("SelectionBoxChunk (" + x + "," + z + ")");
                    chunkObjectsOverlay[x, z].layer = 9;
                    chunkObjectsOverlay[x, z].transform.parent = selectionBoxMap.transform;
                    chunkObjectsOverlay[x, z].transform.position = newChunkPos;
                    chunkObjectsOverlay[x, z].AddComponent<Chunk>();
                    overlayChunks[x, z] = chunkObjectsOverlay[x, z].GetComponent<Chunk>();
                }
            }

            //Debug.Log(chunks.GetLength(0));

            //Tell each selection box chunk to draw their share of the mesh
            for (int x = 0; x < chunkArraySize; x++)
            {
                for (int z = 0; z < chunkArraySize; z++)
                {
                    Chunk newChunk = overlayChunks[x, z];
                    newChunk.ChunkPosition = new Vector2(x, z);
                    newChunk.ChunkWorldPosition = new Vector3(x * chunkSize, 0f, z * chunkSize);
                    newChunk.tileSize = tileSize;
                    newChunk.chunkSize = chunkSize;
                    newChunk.DrawTiles(mapData, true);
                }
            }

            Debug.Log("Map generation complete");

        }

        public void ResizeWorld(int newSize)
        {
            oldWorldSize = worldSize;
            worldSize = newSize;
            // Grab old world size
            //oldWorldSize = mapData.GetLength(0) - 1;
            //Debug.Log(oldWorldSize);
            //Debug.Log(mapData.GetLength(0) + "," + mapData.GetLength(1));
            // If the new size is smaller than the old, delete the extra tiles and rebuild the mapData array.
            if (oldWorldSize > worldSize)
            {
                for (int x = worldSize; x < oldWorldSize - 1; x++)
                {
                    for (int z = worldSize; z < oldWorldSize - 1; z++)
                    {
                        //Debug.Log(x + " " + z);
                        mapData[x, z] = null;

                    }
                }
                //Debug.Log("Attempting rebuild");
                //Debug.Log(mapData.GetLength(1));

                // Rebuild mapdata
                QuarterTile[,] tempMapData = new QuarterTile[worldSize + 1, worldSize + 1];
                //Debug.Log(tempMapData.GetLength(0) + "," + tempMapData.GetLength(1));
                //Debug.Log(worldSize - 2);
                //Debug.Log(mapData.GetLength(0) + "," + mapData.GetLength(1));
                for (int x = 0; x < worldSize; x++)
                {
                    for (int z = 0; z < worldSize; z++)
                    {
                        //Debug.Log(x + "," + z);
                        tempMapData[x, z] = mapData[x, z];
                    }
                }
                mapData = tempMapData;
            }
            // If the new size is larger than the old, rebuild the mapData array and add the additional tiles.
            else if (oldWorldSize < worldSize)
            {
                // Rebuild mapdata
                QuarterTile[,] tempMapData = new QuarterTile[worldSize + 1, worldSize + 1];
                //Debug.Log(mapData.GetLength(1));
                //Debug.Log($"Rebuilding mapData, where x length = {MapData.GetLength(0)} and z length = {MapData.GetLength(1)}.");
                for (int x = 0; x < mapData.GetLength(0); x++)
                {
                    for (int z = 0; z < mapData.GetLength(1); z++)
                    {
                        //Debug.Log(x + "," + z);
                        if (x > 200)
                        {
                            //Debug.Log($"Added tile {x},{z} to tempMapData.");
                        }
                        tempMapData[x, z] = mapData[x, z];
                    }
                }
                mapData = tempMapData;
            }
            //Debug.Log($"Adding to mapData, where x length = {worldSize - 1} and z length = {worldSize - 1}. The starting parameters are x = {oldWorldSize} and y = {oldWorldSize}.");

            for (int x = oldWorldSize; x < worldSize; x++)
            {
                for (int z = 0; z < worldSize; z++)
                {
                    if (mapData[x, z] == null)
                    {
                        mapData[x, z] = new QuarterTile(x, z, 0f, tileSize, defaultTerrainType, defaultOverlayTileType, bottomOfMapHeight, defaultCliffType);
                        AssignTileGridOverlay(MapData[x, z]);
                    }

                }
            }
            for (int x = 0; x < worldSize; x++)
            {
                for (int z = oldWorldSize; z < worldSize; z++)
                {
                    if (mapData[x, z] == null)
                    {
                        mapData[x, z] = new QuarterTile(x, z, 0f, tileSize, defaultTerrainType, defaultOverlayTileType, bottomOfMapHeight, defaultCliffType);
                        AssignTileGridOverlay(MapData[x, z]);
                    }
                }
            }


            if (worldSize % chunkSize > 0)
            {
                numberOfChunks = worldSize / chunkSize + 1;
            }
            else
            {
                numberOfChunks = worldSize / chunkSize;
            }
            // Check if chunks need to be added or deleted
            int chunkArraySize;
            //Debug.Log("Number of chunks = " + numberOfChunks);
            chunkArraySize = numberOfChunks;
            //Debug.Log("Chunk array size = " + chunkArraySize);
            int oldChunkLength = chunks.GetLength(0);
            //Debug.Log("Old Chunk length = " + oldChunkLength);

            // If the number of chunks has changed
            if (oldChunkLength != chunkArraySize)
            {
                //Debug.Log("Number of chunks has changed!");
                // If there are now less chunks needed, find and delete the unneeded ones.
                if (chunkArraySize < oldChunkLength)
                {
                    //Debug.Log("Destroying chunks!");
                    //Delete each unneeded chunk
                    for (int x = oldChunkLength - 1; x >= chunkArraySize; x--)
                    {
                        for (int z = 0; z < oldChunkLength; z++)
                        {
                            //Destroy(chunks[x, z].GetComponent<Chunk>());
                            Destroy(chunks[x, z]);
                            Destroy(chunkObjects[x, z]);
                            chunks[x, z] = null;
                            chunkObjects[x, z] = null;
                            //Debug.Log("Destroyed Chunk " + x + "," + z);
                        }
                    }
                    for (int x = 0; x < oldChunkLength; x++)
                    {
                        for (int z = oldChunkLength - 1; z >= chunkArraySize; z--)
                        {
                            //Destroy(chunks[x, z].GetComponent<Chunk>());
                            Destroy(chunks[x, z]);
                            Destroy(chunkObjects[x, z]);
                            chunks[x, z] = null;
                            chunkObjects[x, z] = null;
                            //Debug.Log("Destroyed Chunk " + x + "," + z);
                        }
                    }
                    // Delete each unneeded selection box chunk
                    for (int x = oldChunkLength - 1; x >= chunkArraySize; x--)
                    {
                        for (int z = 0; z < oldChunkLength; z++)
                        {
                            //Destroy(selectionChunks[x, z].GetComponent<Chunk>());
                            Destroy(overlayChunks[x, z]);
                            Destroy(chunkObjectsOverlay[x, z]);
                            overlayChunks[x, z] = null;
                            chunkObjectsOverlay[x, z] = null;
                            //Debug.Log("Destroyed Selection Chunk " + x + "," + z);
                        }
                    }
                    for (int x = 0; x < oldChunkLength; x++)
                    {
                        for (int z = oldChunkLength - 1; z >= chunkArraySize; z--)
                        {
                            //Destroy(selectionChunks[x, z].GetComponent<Chunk>());
                            Destroy(overlayChunks[x, z]);
                            Destroy(chunkObjectsOverlay[x, z]);
                            overlayChunks[x, z] = null;
                            chunkObjectsOverlay[x, z] = null;
                            //Debug.Log("Destroyed Selection Chunk " + x + "," + z);
                        }
                    }
                }


                // Resize chunk arrays if removing chunks
                if (chunkObjects.GetLength(0) > chunkArraySize)
                {

                    GameObject[,] tempChunkObjectsArray = new GameObject[chunkArraySize, chunkArraySize];
                    for (int x = 0; x <= chunkArraySize - 1; x++)
                    {
                        for (int z = 0; z <= chunkArraySize - 1; z++)
                        {

                            //Debug.Log("chunk array resize x and z = " + x + "," + z);
                            //Debug.Log("temp array size = " + tempChunkObjectsArray.GetLength(0));
                            //Debug.Log("chunkObjs array size = " + chunkObjects.GetLength(0));
                            tempChunkObjectsArray[x, z] = chunkObjects[x, z];

                        }
                    }
                    Chunk[,] tempChunksArray = new Chunk[chunkArraySize, chunkArraySize];
                    for (int x = 0; x <= chunkArraySize - 1; x++)
                    {
                        for (int z = 0; z <= chunkArraySize - 1; z++)
                        {
                            //Debug.Log("Adding Chunk " + x + "," + z + "to tempChunks array");
                            tempChunksArray[x, z] = chunks[x, z];

                        }
                    }
                    chunkObjects = tempChunkObjectsArray;
                    chunks = tempChunksArray;

                    // Resize selection chunk arrays
                    GameObject[,] tempChunkObjectsSelectionArray = new GameObject[chunkArraySize, chunkArraySize];
                    for (int x = 0; x <= chunkArraySize - 1; x++)
                    {
                        for (int z = 0; z <= chunkArraySize - 1; z++)
                        {
                            tempChunkObjectsSelectionArray[x, z] = chunkObjectsOverlay[x, z];

                        }
                    }
                    Chunk[,] tempSelectionChunksArray = new Chunk[chunkArraySize, chunkArraySize];
                    for (int x = 0; x <= chunkArraySize - 1; x++)
                    {
                        for (int z = 0; z <= chunkArraySize - 1; z++)
                        {
                            tempSelectionChunksArray[x, z] = overlayChunks[x, z];

                        }
                    }

                    chunkObjectsOverlay = tempChunkObjectsSelectionArray;
                    overlayChunks = tempSelectionChunksArray;
                }
                // Resize arrays if adding chunks
                if (chunkObjects.GetLength(0) < chunkArraySize)
                {
                    GameObject[,] tempChunkObjectsArray = new GameObject[chunkArraySize, chunkArraySize];
                    for (int x = 0; x <= chunkObjects.GetLength(0) - 1; x++)
                    {
                        for (int z = 0; z <= chunkObjects.GetLength(0) - 1; z++)
                        {

                            //Debug.Log("chunk array resize x and z = " + x + "," + z);
                            //Debug.Log("temp array size = " + tempChunkObjectsArray.GetLength(0));
                            //Debug.Log("chunkObjs array size = " + chunkObjects.GetLength(0));
                            tempChunkObjectsArray[x, z] = chunkObjects[x, z];

                        }
                    }
                    Chunk[,] tempChunksArray = new Chunk[chunkArraySize, chunkArraySize];
                    for (int x = 0; x <= chunks.GetLength(0) - 1; x++)
                    {
                        for (int z = 0; z <= chunks.GetLength(0) - 1; z++)
                        {
                            //Debug.Log("Adding Chunk " + x + "," + z + "to tempChunks array");
                            tempChunksArray[x, z] = chunks[x, z];

                        }
                    }
                    chunkObjects = tempChunkObjectsArray;
                    chunks = tempChunksArray;

                    // Resize selection chunk arrays
                    GameObject[,] tempChunkObjectsSelectionArray = new GameObject[chunkArraySize, chunkArraySize];
                    for (int x = 0; x <= chunkObjectsOverlay.GetLength(0) - 1; x++)
                    {
                        for (int z = 0; z <= chunkObjectsOverlay.GetLength(0) - 1; z++)
                        {
                            tempChunkObjectsSelectionArray[x, z] = chunkObjectsOverlay[x, z];

                        }
                    }
                    Chunk[,] tempSelectionChunksArray = new Chunk[chunkArraySize, chunkArraySize];
                    for (int x = 0; x <= overlayChunks.GetLength(0) - 1; x++)
                    {
                        for (int z = 0; z <= overlayChunks.GetLength(0) - 1; z++)
                        {
                            tempSelectionChunksArray[x, z] = overlayChunks[x, z];

                        }
                    }

                    chunkObjectsOverlay = tempChunkObjectsSelectionArray;
                    overlayChunks = tempSelectionChunksArray;
                }

                // If there are now more chunks needed, add them.
                if (chunkArraySize > oldChunkLength)
                {
                    //Debug.Log("Creating new chunks!");
                    //Create each new chunk
                    for (int x = 0; x < chunkArraySize; x++)
                    {
                        for (int z = oldChunkLength; z < chunkArraySize; z++)
                        {
                            //Debug.Log("Trying to create chunk " + x + "," + z);
                            Vector3 newChunkPos = new Vector3(x * chunkSize, 0f, z * chunkSize);
                            chunkObjects[x, z] = new GameObject("Chunk (" + x + "," + z + ")");
                            chunkObjects[x, z].layer = 8;
                            chunkObjects[x, z].transform.parent = gameObject.transform;
                            chunkObjects[x, z].transform.position = newChunkPos;
                            chunkObjects[x, z].AddComponent<Chunk>();
                            chunks[x, z] = chunkObjects[x, z].GetComponent<Chunk>();
                            //Debug.Log("Created Chunk " + x + "," + z);
                        }
                    }
                    for (int x = oldChunkLength; x < chunkArraySize; x++)
                    {
                        for (int z = 0; z < oldChunkLength; z++)
                        {
                            Vector3 newChunkPos = new Vector3(x * chunkSize, 0f, z * chunkSize);
                            chunkObjects[x, z] = new GameObject("Chunk (" + x + "," + z + ")");
                            chunkObjects[x, z].layer = 8;
                            chunkObjects[x, z].transform.parent = gameObject.transform;
                            chunkObjects[x, z].transform.position = newChunkPos;
                            chunkObjects[x, z].AddComponent<Chunk>();
                            chunks[x, z] = chunkObjects[x, z].GetComponent<Chunk>();
                            //Debug.Log("Created Chunk " + x + "," + z);
                        }
                    }
                    //Create each new selection box chunk
                    for (int x = 0; x < chunkArraySize; x++)
                    {
                        for (int z = oldChunkLength; z < chunkArraySize; z++)
                        {
                            Vector3 newChunkPos = new Vector3(x * chunkSize, 0f, z * chunkSize);
                            chunkObjectsOverlay[x, z] = new GameObject("SelectionBoxChunk (" + x + "," + z + ")");
                            chunkObjectsOverlay[x, z].layer = 9;
                            chunkObjectsOverlay[x, z].transform.parent = selectionBoxMap.transform;
                            chunkObjectsOverlay[x, z].transform.position = newChunkPos;
                            chunkObjectsOverlay[x, z].AddComponent<Chunk>();
                            overlayChunks[x, z] = chunkObjectsOverlay[x, z].GetComponent<Chunk>();
                        }
                    }
                    for (int x = oldChunkLength; x < chunkArraySize; x++)
                    {
                        for (int z = 0; z < oldChunkLength; z++)
                        {
                            Vector3 newChunkPos = new Vector3(x * chunkSize, 0f, z * chunkSize);
                            chunkObjectsOverlay[x, z] = new GameObject("SelectionBoxChunk (" + x + "," + z + ")");
                            chunkObjectsOverlay[x, z].layer = 9;
                            chunkObjectsOverlay[x, z].transform.parent = selectionBoxMap.transform;
                            chunkObjectsOverlay[x, z].transform.position = newChunkPos;
                            chunkObjectsOverlay[x, z].AddComponent<Chunk>();
                            overlayChunks[x, z] = chunkObjectsOverlay[x, z].GetComponent<Chunk>();
                        }
                    }
                }

            }

            // Update chunks
            for (int x = 0; x < chunkArraySize; x++)
            {
                for (int z = 0; z < chunkArraySize; z++)
                {
                    Chunk newChunk = chunks[x, z];
                    //Debug.Log("Updating Chunk " + x + "," + z);
                    newChunk.ChunkPosition = new Vector2(x, z);
                    newChunk.ChunkWorldPosition = new Vector3(x * chunkSize, 0f, z * chunkSize);
                    newChunk.tileSize = tileSize;
                    newChunk.chunkSize = chunkSize;
                    newChunk.DrawTiles(mapData, false);
                }
            }

            //Tell each selection box chunk to draw their share of the mesh
            for (int x = 0; x < chunkArraySize; x++)
            {
                for (int z = 0; z < chunkArraySize; z++)
                {
                    Chunk newChunk = overlayChunks[x, z];
                    //Debug.Log("Updating Selection Chunk " + x + "," + z);
                    newChunk.ChunkPosition = new Vector2(x, z);
                    newChunk.ChunkWorldPosition = new Vector3(x * chunkSize, 0f, z * chunkSize);
                    newChunk.tileSize = tileSize;
                    newChunk.chunkSize = chunkSize;
                    newChunk.DrawTiles(mapData, true);
                }
            }
            ToggleOverlayGrid();
            //Debug.Log("Map resizing complete");
            //Debug.Log(mapData.GetLength(0) + "," + mapData.GetLength(1));

        }

        public void FlattenWorld()
        {
            GenerateFlatTerrain();
            ToggleOverlayGrid();
            DrawAllChunks();
            ToggleOverlayGrid();
        }

        public void UpdateChunks()
        {
            // Update chunks
            for (int x = 0; x < chunks.GetLength(0); x++)
            {
                for (int z = 0; z < chunks.GetLength(1); z++)
                {
                    Chunk newChunk = chunks[x, z];
                    //Debug.Log("Updating Chunk " + x + "," + z);
                    newChunk.ChunkPosition = new Vector2(x, z);
                    newChunk.ChunkWorldPosition = new Vector3(x * chunkSize, 0f, z * chunkSize);
                    newChunk.tileSize = tileSize;
                    newChunk.chunkSize = chunkSize;
                    newChunk.DrawTiles(mapData, false);
                }
            }

            //Tell each selection box chunk to draw their share of the mesh
            for (int x = 0; x < overlayChunks.GetLength(0); x++)
            {
                for (int z = 0; z < overlayChunks.GetLength(1); z++)
                {
                    Chunk newChunk = overlayChunks[x, z];
                    //Debug.Log("Updating Selection Chunk " + x + "," + z);
                    newChunk.ChunkPosition = new Vector2(x, z);
                    newChunk.ChunkWorldPosition = new Vector3(x * chunkSize, 0f, z * chunkSize);
                    newChunk.tileSize = tileSize;
                    newChunk.chunkSize = chunkSize;
                    newChunk.DrawTiles(mapData, true);
                }
            }
        }

        public void DrawAllChunks()
        {
            // Update chunks
            for (int x = 0; x < chunks.GetLength(0); x++)
            {
                for (int z = 0; z < chunks.GetLength(1); z++)
                {
                    chunks[x, z].DrawTiles(mapData, false);
                }
            }
        }
        public void DrawAllOverlayChunks()
        {
            //Tell each selection box chunk to draw their share of the mesh
            for (int x = 0; x < overlayChunks.GetLength(0); x++)
            {
                for (int z = 0; z < overlayChunks.GetLength(1); z++)
                {
                    overlayChunks[x, z].DrawTiles(mapData, true);
                }
            }
        }

        public void DrawChunks(List<Chunk> chunksToDraw)
        {
            foreach (Chunk chunk in chunksToDraw)
            {
                chunk.DrawTiles(MapData, false);
            }
        }
        public void DrawChunks(Chunk chunkToDraw)
        {
            chunkToDraw.DrawTiles(MapData, false);
        }
        public void DrawOverlayChunks(List<Chunk> overlayChunksToDraw)
        {
            foreach (Chunk overlayChunk in overlayChunksToDraw)
            {
                overlayChunk.DrawTiles(MapData, true);
            }
        }
        public void DrawOverlayChunks(Chunk overlayChunkToDraw)
        {
            overlayChunkToDraw.DrawTiles(MapData, true);
        }

        public void GenerateFlatTerrain()
        {
            for (int x = 0; x < worldSize; x++)
            {
                for (int z = 0; z < worldSize; z++)
                {
                    mapData[x, z] = new QuarterTile(x, z, baseElevation, tileSize, defaultTerrainType, defaultOverlayTileType, bottomOfMapHeight, defaultCliffType);
                    AssignTileGridOverlay(MapData[x, z]);
                }
            }
        }



        /// <summary>
        /// Randomly generate all terrain generation parameters.
        /// </summary>
        public void GenerateRandomGenerationParameters()
        {
            GenerateNumberOfOctaves();
            GenerateBaseFrequencies();
            GenerateBaseAmplitudes();
            GeneratePersistence();
        }

        /// <summary>
        /// If a random seed is desired, run this function prior to generating terrain.
        /// </summary>
        public void GenerateRandomGenerationSeed(bool randomElevationSeed, bool randomMoistureSeed)
        {
            if (randomElevationSeed)
            {
                elevationSeed = UnityEngine.Random.Range(minSeed, maxSeed);
                //Debug.Log($"Elevation seed = {elevationSeed}");
            }
            if (randomMoistureSeed)
            {
                moistureSeed = UnityEngine.Random.Range(minSeed, maxSeed);
                //Debug.Log($"Moisture seed = {moistureSeed}");
            }
        }
        
        void GenerateNumberOfOctaves()
        {
            numberOfElevationOctaves = UnityEngine.Random.Range(minOctaves, maxOctaves);
            //Debug.Log($"Number of octaves = {numberOfElevationOctaves}");
            numberOfMoistureOctaves = UnityEngine.Random.Range(minOctaves, maxOctaves);
        }
        void GenerateBaseFrequencies()
        {
            baseElevationFrequency = UnityEngine.Random.Range(minFreq, maxFreq);
            //Debug.Log($"Base elevation frequency = {baseElevationFrequency}");
            baseMoistureFrequency = UnityEngine.Random.Range(minFreq, maxFreq);
        }
        void GenerateBaseAmplitudes()
        {
            baseElevationAmplitude = UnityEngine.Random.Range(minAmp, maxAmp);
            //Debug.Log($"Base amplitude = {baseElevationAmplitude}");
            baseMoistureAmplitude = UnityEngine.Random.Range(minAmp, maxAmp);
        }
        void GeneratePersistence()
        {
            elevationAmplitudePersistence = UnityEngine.Random.Range(minPersistence, maxPersistence);
            //Debug.Log($"Persistence = {elevationAmplitudePersistence}");
            moistureAmplitudePersistence = UnityEngine.Random.Range(minPersistence, maxPersistence);
        }
        void PopulateFrequencyArrays()
        {
            elevationOctaveFrequency = new float[numberOfElevationOctaves];
            moistureOctaveFrequency = new float[numberOfMoistureOctaves];

            elevationOctaveFrequency[0] = baseElevationFrequency;
            for (int x = 1; x < numberOfElevationOctaves; x++)
            {
                elevationOctaveFrequency[x] = elevationOctaveFrequency[x - 1] * 2;
                //Debug.Log($"Frequency {x + 1} = {elevationOctaveFrequency[x]}");
            }
            moistureOctaveFrequency[0] = baseMoistureFrequency;
            for (int x = 1; x < numberOfMoistureOctaves; x++)
            {
                moistureOctaveFrequency[x] = moistureOctaveFrequency[x - 1] * 2;
            }
        }
        void PopulateAmplitudeArrays()
        {
            elevationOctaveAmplitude = new float[numberOfElevationOctaves];
            moistureOctaveAmplitude = new float[numberOfMoistureOctaves];

            float amplitude = baseElevationAmplitude;
            for (int x = 0; x < numberOfElevationOctaves; x++)
            {
                elevationOctaveAmplitude[x] = amplitude;
                //Debug.Log($"Amplitude {x} before persistence = {elevationOctaveAmplitude[x]}");
                amplitude *= elevationAmplitudePersistence;
                //Debug.Log($"Amplitude {x} after persistence = {elevationOctaveAmplitude[x]}");
                //amplitude *= elevationSeed;
            }
            amplitude = baseMoistureAmplitude;
            for (int x = 0; x < numberOfMoistureOctaves; x++)
            {
                moistureOctaveAmplitude[x] = amplitude;
                amplitude *= moistureAmplitudePersistence;
                //amplitude *= moistureSeed;
            }
        }
        void GenerateFlatness()
        {
            flatness = UnityEngine.Random.Range(minFlatness, maxFlatness);
        }

        /// <summary>
        /// Generates random terrain map and moisture map, and then assign biomes based on those maps.
        /// </summary>
        public void GenerateRandomTerrain()
        {
            // Populate Frequencies and Amplitudes using current parameters
            PopulateFrequencyArrays();
            PopulateAmplitudeArrays();

            // Create 2d array to store heightmap.
            float[,] heightmap = new float[worldSize + 1, worldSize + 1];

            // Generate heightmap by randomising each vertex using perlin noise.
            for (int x = 0; x < worldSize + 1; x++)
            {
                for (int z = 0; z < worldSize + 1; z++)
                {
                    // Generate height info
                    float newHeight = GenerateVertexHeight(new Vector3(x, 0f, z), numberOfElevationOctaves);
                    heightmap[x, z] = newHeight;
                    //Debug.Log($"Generating vertice {x},{z}");             
                                    
                }
            }
            // TODO: Generate moisture map
            // Update each tile height & moisture
            for (int x = 0; x < worldSize; x++)
            {
                for (int z = 0; z < worldSize; z++)
                {
                    QuarterTile active = MapData[x, z];
                    active.lowerLeft.y = heightmap[x, z];
                    active.upperLeft.y = heightmap[x, z + 1];
                    active.upperRight.y = heightmap[x + 1, z + 1];
                    active.lowerRight.y = heightmap[x + 1, z];
                    active.ReSetStats();

                    // TODO: Assign biomes based on elevation & moisture

                }
            }


            //UpdateAllTiles();
            for (int x = 0; x < worldSize; x++)
            {
                for (int z = 0; z < worldSize; z++)
                {
                    QuarterTile active = mapData[x, z];
                    active.TerrainType = TempTerrainAssignment(active.height);
                    active.CliffType = TempCliffAssignment(active.height);
                }
            }
            ToggleOverlayGrid();
            UpdateChunks();
        }

        float GenerateVertexHeight(Vector3 input, int numberOfElevationOctaves)
        {
            //Debug.Log($"Input vector3 = {input.x},{input.y},{input.z}.");
            float newHeight = 0;
            float amplitude = 0;
            // Divide by world size to allow frequency to work. wavelength = mapsize / frequency.
            float nX = input.x / worldSize;
            float nZ = input.z / worldSize;

            // Create elevation octaves
            for (int x = 0; x < numberOfElevationOctaves; x++)
            {
                newHeight += Octave(elevationOctaveAmplitude[x], elevationOctaveFrequency[x], nX, nZ, elevationSeed);
                //Debug.Log($"Octave {x + 1} = {Octave(elevationOctaveAmplitude[x], elevationOctaveFrequency[x], nX, nZ, elevationSeed)}");
                amplitude += elevationOctaveAmplitude[x];
            }
            //Debug.Log($"All Octaves added ={newHeight}.");
            //Debug.Log($"All amplitudes added = {amplitude}.");
            // Normalise the height between the provided height parameters.
            //newHeight = Mathf.InverseLerp(0, maxGenerationHeight - minGenerationHeight, newHeight);
            //newHeight /= (heightScale - bottomOfMapHeight - .5f);
            //newHeight /= amplitude;
            //newHeight /= maxGenerationHeight - minGenerationHeight;
            //Debug.Log($"Normalised height = {newHeight}.");

            // Convert into heightscale
            //newHeight = Mathf.Lerp(0, maxGenerationHeight - minGenerationHeight, newHeight);
            newHeight = Mathf.Lerp(minGenerationHeight, maxGenerationHeight, newHeight);
            //newHeight = Mathf.Lerp(bottomOfMapHeight + 1, maxHeight, newHeight);
            //Debug.Log($"Height after lerp = {newHeight}");

            // Adjust flatness of world using provided flatness parameter.
            //newHeight = Mathf.Pow(newHeight, flatness);
            //Debug.Log($"Height adjusted for flatness = {newHeight}.");

            //Convert the newHeight into the scale of the provided height parameters.
            //newHeight = Mathf.InverseLerp(0, maxGenerationHeight - minGenerationHeight, newHeight);
            //Debug.Log($"Height after InverseLerp ={newHeight}.");

            //newHeight = Mathf.LerpUnclamped(0, maxGenerationHeight - minGenerationHeight, newHeight);
            //Debug.Log($"Height after Lerp ={newHeight}.");
            //newHeight *= maxGenerationHeight - minGenerationHeight;
            //Debug.Log($"Height after converting = {newHeight}.");

            // Make sure the newHeight is rounded properly.
            newHeight = RoundToNearestQuarter(newHeight);

            //Debug.Log($"Height after rounding ={newHeight}.");
            //Make sure height is properly scaled if the min height is negative
            //if (minGenerationHeight < 0)
            //{
            //    newHeight += minGenerationHeight;
            //}
            // Ensure height is never too low.
            newHeight = Mathf.Clamp(newHeight, minGenerationHeight, maxGenerationHeight);
            newHeight = Mathf.Clamp(newHeight, bottomOfMapHeight + 1, maxHeight);
            //Debug.Log($"Final height after clamping to min/max height = {newHeight}.");

            return newHeight;
        }

        float Octave(float amplitude, float frequency, float nX, float nZ, float seed)
        {
            return amplitude * Perlin(frequency * nX, frequency * nZ, seed);
        }

        float Perlin(float x, float z, float seed)
        {
            float perlinX = x;
            float perlinZ = z;
            //Debug.Log($"perlinX = {perlinX}.");
            //Debug.Log($"perlinZ = {perlinZ}.");
            return ((Mathf.PerlinNoise(perlinX * seed, perlinZ * seed)) / 2);
        }

        /// <summary>
        /// Turns the overlay grid on or off.
        /// </summary>
        public void ToggleOverlayGrid()
        {
            if (gridEnabled)
            {
                foreach (QuarterTile t in MapData)
                {
                    if (t != null)
                    {
                        t.OverlayTileType = t.gridType;
                    }
                }
            }
            else
            {
                foreach (QuarterTile t in MapData)
                {
                    if (t != null)
                    {
                        t.OverlayTileType = 6;
                    }
                }
            }
            DrawAllOverlayChunks();
        }

        /// <summary>
        /// Assign the tile the proper grid overlay at map generation.
        /// </summary>
        /// <param name="tile"></param>
        void AssignTileGridOverlay(QuarterTile tile)
        {
            // If the tile's X coordinate is even
            if (tile.TileCoordX % 2 == 0)
            {
                // And the Z coordinate is even
                if (tile.TileCoordZ % 2 == 0)
                {
                    // The tile is SW
                    tile.gridType = 7;
                }
                // And the Z coordinate is odd
                else if (tile.TileCoordZ % 2 != 0)
                {
                    // The tile is NW
                    tile.gridType = 8;
                }
            }
            // If the tile's X coordinate is odd
            else if (tile.TileCoordX % 2 != 0)
            {
                // And the Z coordinate is even
                if (tile.TileCoordZ % 2 == 0)
                {
                    // The tile is SE
                    tile.gridType = 10;
                }
                // And the Z coordinate is odd
                else if (tile.TileCoordZ % 2 != 0)
                {
                    // The tile is NE
                    tile.gridType = 9;
                }
            }

        }

        int TempTerrainAssignment(float tileHeight)
        {
            if (tileHeight <= waterLevel)
            {
                return 6;
            }
            if (tileHeight > waterLevel && tileHeight <= waterLevel + .25f)
            {
                return 2;
            }
            if (tileHeight <= maxHeight && tileHeight >= snowLine)
            {
                // Allow rock to generate more naturally.
                if (tileHeight == snowLine)
                {
                    float random = UnityEngine.Random.Range(0, 1);
                    if (random >= 50)
                    {
                        return 0;
                    }
                    else
                    {
                        return 4;
                    }
                }
                if (tileHeight == snowLine + .25f)
                {
                    float random = UnityEngine.Random.Range(0, 1);
                    if (random >= 65)
                    {
                        return 0;
                    }
                    else
                    {
                        return 4;
                    }
                }
                if (tileHeight == snowLine + .5f)
                {
                    float random = UnityEngine.Random.Range(0, 1);
                    if (random >= 80)
                    {
                        return 0;
                    }
                    else
                    {
                        return 4;
                    }
                }
                if (tileHeight == snowLine + .75f)
                {
                    float random = UnityEngine.Random.Range(0, 1);
                    if (random >= 90)
                    {
                        return 0;
                    }
                    else
                    {
                        return 4;
                    }
                }
                else
                {
                    return 5;
                }
            }
            else
            {
                return 0;
            }
        }
        int TempCliffAssignment(float tileHeight)
        {
            if (tileHeight <= waterLevel)
            {
                return 1;
            }
            if (tileHeight > waterLevel && tileHeight <= waterLevel + .5f)
            {
                return 3;
            }
            // Generate rock for mountains
            if (tileHeight <= maxHeight && tileHeight >= snowLine)
            {
                // Allow rock to generate more naturally.
                if (tileHeight == snowLine)
                {
                    float random = UnityEngine.Random.Range(0, 1);
                    if (random >= 50)
                    {
                        return 1;
                    }
                    else
                    {
                        return 5;
                    }
                }
                if (tileHeight == snowLine + .25f)
                {
                    float random = UnityEngine.Random.Range(0, 1);
                    if (random >= 65)
                    {
                        return 1;
                    }
                    else
                    {
                        return 5;
                    }
                }
                if (tileHeight == snowLine + .5f)
                {
                    float random = UnityEngine.Random.Range(0, 1);
                    if (random >= 80)
                    {
                        return 1;
                    }
                    else
                    {
                        return 5;
                    }
                }
                if (tileHeight == snowLine + .75f)
                {
                    float random = UnityEngine.Random.Range(0, 1);
                    if (random >= 90)
                    {
                        return 1;
                    }
                    else
                    {
                        return 5;
                    }
                }
                else
                {
                    return 5;
                }
            }
            else
            {
                return 1;
            }
        }
        
        public static float RoundToNearestQuarter(float a)
        {
            return a = Mathf.Floor(a * 4f) / 4;
        }
        public static float RoundToNearestHalf(float a)
        {
            return a = Mathf.Floor(a * 2f) / 2;
        }

        /// <summary>
        /// Round the provided input to the provided fraction.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="roundToThisFraction">2 = 1/2, 4 = 1/4, etc.</param>
        /// <returns></returns>
        public static float RoundToNearest(float a, int roundToThisFraction)
        {
            return a = Mathf.Floor(a * roundToThisFraction) / roundToThisFraction;
        }


        public QuarterTile[] GetAdjacentQuarterTiles(QuarterTile tile)
        {
            QuarterTile[] neighborTiles = new QuarterTile[8];
            int centerX = (int)tile.TileCoordX;
            int centerZ = (int)tile.TileCoordZ;
            // 0 = S tile
            if (centerZ > 0)
            {
                if (mapData[centerX, centerZ - 1] != null)
                    neighborTiles[0] = mapData[centerX, centerZ - 1];
            }
            else
            {
                neighborTiles[0] = null;
            }
            // If both tile coordinates are greater than 0, set up the SW tile
            if (centerZ > 0 && centerX > 0)
            {
                if (mapData[centerX - 1, centerZ - 1] != null)
                    neighborTiles[1] = mapData[centerX - 1, centerZ - 1];
            }
            else
            {
                neighborTiles[1] = null;
            }
            // If the tile X coordinate is greater than 0, set up the W tile
            if (centerX > 0)
            {
                if (mapData[centerX - 1, centerZ] != null)
                    neighborTiles[2] = mapData[centerX - 1, centerZ];
            }
            else
            {
                neighborTiles[2] = null;
            }
            // If the tile Z coordinate is less than world size and tile X is greater than 0, set up the NW tile
            if (centerZ < worldSize && centerX > 0)
            {
                if (mapData[centerX - 1, centerZ + 1] != null)
                    neighborTiles[3] = mapData[centerX - 1, centerZ + 1];
            }
            else
            {
                neighborTiles[3] = null;
            }
            // If the tile Z coordinate is less than world size, set up the N tile
            if (centerZ < worldSize)
            {
                if (mapData[centerX, centerZ + 1] != null)
                    neighborTiles[4] = mapData[centerX, centerZ + 1];
            }
            else
            {
                neighborTiles[4] = null;
            }
            // If both tiles are less than world size, set up the NE tile
            if (centerZ < worldSize && centerX < worldSize)
            {
                if (mapData[centerX + 1, centerZ + 1] != null)
                    neighborTiles[5] = mapData[centerX + 1, centerZ + 1];
            }
            else
            {
                neighborTiles[5] = null;
            }
            // If the tile X coordinate is less than world size, set up the E tile
            if (centerX < worldSize)
            {
                if (mapData[centerX + 1, centerZ] != null)
                    neighborTiles[6] = mapData[centerX + 1, centerZ];
            }
            else
            {
                neighborTiles[6] = null;
            }
            // If the tile X coordinate is less than world size and the tile Z coordinate is greater than 0, set up the SE tile
            if (centerZ > 0 && centerX < worldSize)
            {
                if (mapData[centerX + 1, centerZ - 1] != null)
                    neighborTiles[7] = mapData[centerX + 1, centerZ - 1];
            }
            else
            {
                neighborTiles[7] = null;
            }
            return neighborTiles;
        }


        /// <summary>
        /// Get tiles between the starting and ending tiles provided. Used for multi-tile selection.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<QuarterTile> GetQuarterTilesFromSelection(QuarterTile start, QuarterTile end)
        {
            List<QuarterTile> tiles = new List<QuarterTile>();
            // Determine where the end tile is in relation to the starting tile, and grab the relevant tiles.
            // If the start and end are the same
            if (end.TileCoordX == start.TileCoordX && end.TileCoordZ == start.TileCoordZ)
            {
                tiles.Add(mapData[start.TileCoordX, start.TileCoordZ]);
            }
            // If end is N
            else if (end.TileCoordX == start.TileCoordX && end.TileCoordZ > start.TileCoordZ)
            {
                for (int z = start.TileCoordZ; z <= end.TileCoordZ; z++)
                {
                    tiles.Add(MapData[start.TileCoordX, z]);
                }
            }
            // If end is NE
            else if (end.TileCoordX > start.TileCoordX && end.TileCoordZ > start.TileCoordZ)
            {
                for (int x = start.TileCoordX; x <= end.TileCoordX; x++)
                {
                    for (int z = start.TileCoordZ; z <= end.TileCoordZ; z++)
                    {
                        tiles.Add(mapData[x, z]);
                    }
                }
            }
            // If end is E
            else if (end.TileCoordX > start.TileCoordX && end.TileCoordZ == start.TileCoordZ)
            {
                for (int x = start.TileCoordX; x <= end.TileCoordX; x++)
                {
                    tiles.Add(MapData[x, start.TileCoordZ]);
                }
            }
            // If end is SE
            else if (end.TileCoordX > start.TileCoordX && end.TileCoordZ < start.TileCoordZ)
            {
                for (int x = start.TileCoordX; x <= end.TileCoordX; x++)
                {
                    for (int z = end.TileCoordZ; z <= start.TileCoordZ; z++)
                    {
                        tiles.Add(mapData[x, z]);
                    }
                }
            }
            // If end is S
            else if (end.TileCoordX == start.TileCoordX && end.TileCoordZ < start.TileCoordZ)
            {
                for (int z = end.TileCoordZ; z <= start.TileCoordZ; z++)
                {
                    tiles.Add(MapData[start.TileCoordX, z]);
                }
            }
            // If end is SW
            else if (end.TileCoordX < start.TileCoordX && end.TileCoordZ < start.TileCoordZ)
            {
                for (int x = end.TileCoordX; x <= start.TileCoordX; x++)
                {
                    for (int z = end.TileCoordZ; z <= start.TileCoordZ; z++)
                    {
                        tiles.Add(mapData[x, z]);
                    }
                }
            }
            // If end is W
            else if (end.TileCoordX < start.TileCoordX && end.TileCoordZ == start.TileCoordZ)
            {
                for (int x = end.TileCoordX; x <= start.TileCoordX; x++)
                {
                    tiles.Add(MapData[x, start.TileCoordZ]);
                }
            }
            // If end is NW
            else if (end.TileCoordX < start.TileCoordX && end.TileCoordZ > start.TileCoordZ)
            {
                for (int x = end.TileCoordX; x <= start.TileCoordX; x++)
                {
                    for (int z = start.TileCoordZ; z <= end.TileCoordZ; z++)
                    {
                        tiles.Add(mapData[x, z]);
                    }
                }
            }
            return tiles;
        }

        public QuarterTile GetQuarterTile(int x, int z)
        {
            if (mapData[x, z] != null)
            {
                return mapData[x, z];
            }
            else
            {
                return null;
            }
        }
        public QuarterTile GetQuarterTile(Vector2 tileCoords)
        {
            return mapData[(int)tileCoords.x, (int)tileCoords.y];
        }

        /// <summary>
        /// Form and retrieve a full tile, using the provided QuarterTile as the Southwestern portion.
        /// </summary>
        /// <param name="southwestX"></param>
        /// <param name="southwestZ"></param>
        /// <returns></returns>
        public FullTile GetFullTile(QuarterTile southWestTile)
        {
            FullTile newFull = new FullTile
            {
                southWest = southWestTile
            };
            if (MapData[southWestTile.TileCoordX, southWestTile.TileCoordZ + 1] != null)
                newFull.northWest = MapData[southWestTile.TileCoordX, southWestTile.TileCoordZ + 1];
            if (MapData[southWestTile.TileCoordX + 1, southWestTile.TileCoordZ + 1] != null)
            {
                newFull.northEast = MapData[southWestTile.TileCoordX + 1, southWestTile.TileCoordZ + 1];
            }
            if (MapData[southWestTile.TileCoordX + 1, southWestTile.TileCoordZ] != null)
            {
                newFull.southEast = MapData[southWestTile.TileCoordX + 1, southWestTile.TileCoordZ];
            }
            newFull.GenerateTileList();
            return newFull;
        }


        public Chunk GetChunkAtCoords(int x, int z)
        {
            return chunks[x, z];
        }

        public Chunk GetOverlayChunkAtCoords(int x, int z)
        {
            return overlayChunks[x, z];
        }


        public Chunk GetChunkAtTile(QuarterTile tile)
        {
            return chunks[tile.TileCoordX / chunkSize, tile.TileCoordZ / chunkSize];
        }

        public Chunk GetOverlayChunkAtTile(QuarterTile tile)
        {
            return overlayChunks[tile.TileCoordX / chunkSize, tile.TileCoordZ / chunkSize];
        }

        public QuarterTile[,] MapData
        {
            get { return mapData; }
            set { mapData = value; }
        }

        public static QuarterTile[,] GetMapDataStatic
        {
            get { return mapData; }
            set { mapData = value; }
        }

        public float GetMaxHeight
        {
            get { return maxHeight; }
        }
    }
}
