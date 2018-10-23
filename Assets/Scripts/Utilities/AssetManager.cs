using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zoo.Systems.Construction;

namespace Zoo.Utilities
{
    public static class AssetManager
    {

        /// <summary>
        /// Containts all BuildableObjects, including custom content. Searched by objectID, ex. b_test.
        /// </summary>
        public static Dictionary<string, BuildableObject> BuildableObjectsDictionary = new Dictionary<string, BuildableObject>();




        /// <summary>
        /// Load all assets. Call on start of program.
        /// </summary>
        public static void InitializeAssets()
        {
            LoadBuildableObjects();
        }

        static void LoadBuildableObjects()
        {
            BuildableObject[] buildableObjects = Resources.LoadAll<BuildableObject>("BuildableObjects/");
            // Add each object to the dictionary with its key.
            foreach(BuildableObject obj in buildableObjects)
            {
                BuildableObjectsDictionary.Add(obj.objectID, obj);
            }
        }

    }
}
