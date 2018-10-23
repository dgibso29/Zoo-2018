using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoo
{
    /// <summary>
    /// Holds all game settings. Is saved/loaded/edited by SettingsManager.
    /// </summary>
    [System.Serializable]
    public class Settings
    {

        public GameplaySettings Gameplay { get; set; }
        public GraphicsSettings Graphics { get; set; }
        public ControlSettings Controls { get; set; }
        public AudioSettings Audio { get; set; }

        public Settings()
        {
            Gameplay = new GameplaySettings();
            Graphics = new GraphicsSettings();
            Controls = new ControlSettings();
            Audio = new AudioSettings();
            ResetToDefault();
        }

        /// <summary>
        /// Reset all settings to default.
        /// </summary>
        public void ResetToDefault()
        {
            Gameplay.ResetToDefault();
            Graphics.ResetToDefault();
            Controls.ResetToDefault();
            Audio.ResetToDefault();
        }
        [System.Serializable]
        public partial class GameplaySettings
        {
            /// <summary>
            /// Toggles between basic and advanced construction mode. 
            /// In advanced mode, quarter tile landscaping and construction tools are enabled.
            /// </summary>
            public bool advancedConstructionMode;

            /// <summary>
            /// Reset Gameplay Settings to default.
            /// </summary>
            public void ResetToDefault()
            {
                advancedConstructionMode = false;
            }
        }
        [System.Serializable]
        public partial class ControlSettings
        {

            /// <summary>
            /// Reset Control Settings to default.
            /// </summary>
            public void ResetToDefault()
            {

            }
        }
        [System.Serializable]
        public partial class GraphicsSettings
        {

            /// <summary>
            /// Reset Graphic Settings to default.
            /// </summary>
            public void ResetToDefault()
            {

            }
        }
        [System.Serializable]
        public partial class AudioSettings
        {

            /// <summary>
            /// Reset Audio Settings to default.
            /// </summary>
            public void ResetToDefault()
            {

            }
        }
    }
}
