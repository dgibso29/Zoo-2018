using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zoo.IO;

namespace Zoo.Utilities
{
    public static class GameStartupHelper
    {

        /// <summary>
        /// Runs necessary game functions, such as file system initialisation.
        /// </summary>
        public static IEnumerator RunStartupProcedures()
        {
            SaveHelper.CheckFileStructure();
            SettingsHelper.InitializeSettings();
            AssetManager.InitializeAssets();
            yield break;
        }
    }
}
