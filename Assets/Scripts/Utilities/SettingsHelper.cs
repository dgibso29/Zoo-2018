using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Zoo.IO;

namespace Zoo
{
    public static class SettingsHelper
    {
        private static Settings settings;
        public static Settings.GameplaySettings GameplaySettings { get; set; }
        public static Settings.GraphicsSettings GraphicsSettings { get; set; }
        public static Settings.ControlSettings ControlSettings { get; set; }
        public static Settings.AudioSettings AudioSettings { get; set; }

        private static string settingsSavePath = SaveHelper.userDataSettingsFolderPath += $"/settings.ini";

        public static void InitializeSettings()
        {
            // Check if settings.ini exists, and load it if so
            if (File.Exists(settingsSavePath))
            {
                //Debug.Log("Settings.ini exists!");
                settings = LoadSettings();
                SetSettingReferences();
            }
            // Otherwise, create a new default settings file
            else
            {
                //Debug.Log("Settings.ini not found. Creating new settings file.");
                settings = new Settings();
                SetSettingReferences();
            }

        }

        static void SetSettingReferences()
        {
            GameplaySettings = settings.Gameplay;
            GraphicsSettings = settings.Graphics;
            ControlSettings = settings.Controls;
            AudioSettings = settings.Audio;
        }

        static Settings LoadSettings()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(settingsSavePath, FileMode.Open);
            Settings settings = (Settings)bf.Deserialize(file);
            file.Close();
            //Debug.Log("Loaded settings.ini");
            return settings;
        }

        public static void SaveSettings()
        {
            string savePath = settingsSavePath;
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(savePath);
            bf.Serialize(file, settings);
            file.Close();
            //Debug.Log($"Saved settings to {savePath}.");


        }

        public static bool AdvancedConstructionModeEnabled
        {
            get
            {
                return GameplaySettings.advancedConstructionMode;
            }
            set
            {
                GameplaySettings.advancedConstructionMode = value;
                SaveSettings();
            }
        }

    }
}
