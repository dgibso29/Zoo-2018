using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Zoo.IO
{
    public static class SaveHelper
    {

        static string myDocumentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        static string companyFolderPath = myDocumentsPath + "/Earring Pranks Studios";
        static string gameDataFolderPath = companyFolderPath + "/Zoo The Zoological Society";
        static string streamingAssetsPath = Application.dataPath + "/StreamingAssets";

        // Game Data Folder Paths

        public static string streamingAssetsBlueprintsFolderPath = streamingAssetsPath + "/Blueprints";
        public static string streamingAssetsSavesFolderPath = streamingAssetsPath + "/Saves";
        public static string streamingAssetsScenariosFolderPath = streamingAssetsPath + "/Scenarios";

        // User Data Folder Paths
        public static string userDataBlueprintsFolderPath = gameDataFolderPath + "/Blueprints";
        public static string userDataModsFolderPath = gameDataFolderPath + "/Mods";
        public static string userDataSavesFolderPath = gameDataFolderPath + "/Saves";
        public static string userDataScenariosFolderPath = gameDataFolderPath + "/Scenarios";
        public static string userDataSettingsFolderPath = gameDataFolderPath + "/Settings";


        static string[] fileStructureFolders = new string[]
        {
        // Streaming Assets Folders
        streamingAssetsBlueprintsFolderPath,
        streamingAssetsSavesFolderPath,
        streamingAssetsScenariosFolderPath,
        // User Data Folders
        userDataBlueprintsFolderPath,
        userDataModsFolderPath,
        userDataSavesFolderPath,
        userDataScenariosFolderPath,
        userDataSettingsFolderPath,

        };

        /// <summary>
        /// Check for missing folders in file structure, and generate any missing entries. Run on game start.
        /// </summary>
        public static void CheckFileStructure()
        {
            foreach (string folder in fileStructureFolders)
            {
                if (!Directory.Exists(folder))
                {
                    GenerateFileStructure();
                    break;
                }
            }
        }

        /// <summary>
        /// Generate any missing folders in file structure (Ex: Saves, Scenarios).
        /// </summary>
        public static void GenerateFileStructure()
        {
            foreach (string folder in fileStructureFolders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

        }

        #region SavedGameIO
        public static void SaveGameToDisk(SavedGame gameToSave, string saveName, string savePath)
        {
            gameToSave = new SavedGame();
            savePath += $"/{saveName}.zoo";
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(savePath);
            bf.Serialize(file, gameToSave);
            file.Close();
            Debug.Log($"Saved game to {savePath}.");

        }

        public static SavedGame LoadGameFromDisk(string savePath)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(savePath, FileMode.Open);
            SavedGame loadedGame = (SavedGame)bf.Deserialize(file);
            file.Close();
            return loadedGame;
        }
        #endregion

        #region ScenarioIO
        public static void SaveScenarioToDisk(Scenario scenarioToSave, string scenarioName, string savePath)
        {
            scenarioToSave = new Scenario();
            savePath += $"/{scenarioName}.scen";
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(savePath);
            bf.Serialize(file, scenarioToSave);
            file.Close();
            Debug.Log($"Saved scenario to {savePath}.");

        }

        public static Scenario LoadScenarioFromDisk(string savePath)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(savePath, FileMode.Open);
            Scenario loadedGame = (Scenario)bf.Deserialize(file);
            file.Close();
            return loadedGame;
        }
        #endregion
    }
}
