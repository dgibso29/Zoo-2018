using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zoo.Systems.Construction;

namespace Zoo.Systems.Administration
{
    public class ZooManager : MonoBehaviour
    {
        // Component References
        public World.World world;



        public static ZooInfo ZooInfo = new ZooInfo();

        /// <summary>
        /// List of all objects currently in the zoo.
        /// </summary>
        public static List<BuildableObject> ObjectsInZoo = new List<BuildableObject>();

        /// <summary>
        /// 3-dimensional 'map' of all zoo paths, where the indices correspond to X/Y/Z map coordinates.
        /// Note: Y index follows the pattern Y Coord 0 = Y index 0, Y Coord 0.5 = Y index 1, Y coord 1 = Y index 2, so on.
        /// </summary>
        public static PathObject[,,] zooPathMatrix = new PathObject[150, 150, 150];

        private void Start()
        {
            ZooInfo.LastUniqueObjectID = 0;
        }

        public void InitializeZooManager()
        {

        }

        void InitializeZooPathMatrix()
        {
            
        }

    }
}
