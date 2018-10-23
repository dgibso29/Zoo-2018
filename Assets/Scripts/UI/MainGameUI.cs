using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Zoo.Systems.World;
using Zoo.Systems.Landscaping;

namespace Zoo.UI
{
    public class MainGameUI : MonoBehaviour
    {
        // Set component references
        public World world;
        public TileMapMouseInterface mouseInterface;
        public LandscapingTools landscapingTools;

        // Set UI references
        #region MapGenUIRef
        #region MapInfoUIRef
        public Slider mapSizeSlider;
        public InputField mapSizeInputField;

        public Slider waterLevelSlider;
        public InputField waterLevelInputField;

        public Slider snowLineSlider;
        public InputField snowLineInputField;

        public Slider baseHeightSlider;
        public InputField baseHeightInputField;
        #endregion
        #region RandomGenerationUIRef
        public Toggle RandomParametersToggle;
        bool randomParametersEnabled = true;

        public Toggle RandomSeedsToggle;

        public InputField elevationSeedInputField;
        bool randomElevationSeedEnabled = true;

        public InputField moistureSeedInputField;
        bool randomMoistureSeedEnabled = true;

        public Slider minimumHeightSlider;
        public InputField minimumHeightInputField;

        public Slider maximumHeightSlider;
        public InputField maximumHeightInputField;

        public Slider flatnessSlider;
        public InputField flatnessInputField;

        public Slider elevationFreqSlider;
        public InputField elevationFreqInputField;

        public Slider moistureFreqSlider;
        public InputField moistureFreqInputField;

        public Slider elevationAmpSlider;
        public InputField elevationAmpInputField;

        public Slider moistureAmpSlider;
        public InputField moistureAmpInputField;

        public Slider elevationPersistenceSlider;
        public InputField elevationPersistenceInputField;

        public Slider moisturePersistenceSlider;
        public InputField moisturePersistenceInputField;

        public Slider elevationOctavesSlider;
        public InputField elevationOctavesInputField;

        public Slider moistureOctavesSlider;
        public InputField moistureOctavesInputField;
        #endregion
        #endregion
        #region LandscapingUIRef
        public Slider brushSizeSlider;
        public InputField brushSizeInputField;

        public Toggle advancedConstructionToggle;
        public Toggle mountainToolToggle;
        #endregion


        public void Start()
        {
            InitializeUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (!advancedConstructionToggle.isOn)
                {
                    advancedConstructionToggle.isOn = true;                    
                }
                else
                {
                    advancedConstructionToggle.isOn = false;
                }
            }
        }

        void InitializeUI()
        {
            InitializeMapGenerationWindow();
            InitializeLandscapingWindow();
        }

        #region Map Generation Window
        void InitializeMapGenerationWindow()
        {
            InitializeMapInfoPanel();
            InitializeRandomGenerationPanel();
        }
        #region Map Size Panel
        void InitializeMapInfoPanel()
        {
            // Map size
            mapSizeInputField.text = $"{world.worldSize}";
            mapSizeSlider.value = world.worldSize;
            mapSizeSlider.minValue = world.minWorldSize;
            mapSizeSlider.maxValue = world.maxWorldSize;
            // Water level
            waterLevelInputField.text = $"{world.waterLevel}";
            waterLevelSlider.value = world.waterLevel;
            waterLevelSlider.minValue = world.bottomOfMapHeight + world.tileHeightStep;
            waterLevelSlider.maxValue = world.maxHeight;
            // Snow Line
            snowLineInputField.text = $"{world.snowLine}";
            snowLineSlider.value = world.snowLine;
            snowLineSlider.minValue = world.bottomOfMapHeight + world.tileHeightStep;
            snowLineSlider.maxValue = world.maxHeight;
            // Base Height
            baseHeightInputField.text = $"{world.baseElevation}";
            baseHeightSlider.value = world.baseElevation;
            baseHeightSlider.minValue = world.bottomOfMapHeight + world.tileHeightStep;
            baseHeightSlider.maxValue = world.maxHeight;
        }

        /// <summary>
        /// Resize world based on current map size value.
        /// </summary>
        public void ResizeWorld()
        {
            //Debug.Log($"Map slider value ={(int)mapSizeSlider.value}");
            world.ResizeWorld((int)mapSizeSlider.value);
        }

        /// <summary>
        /// Generate flat terrain. Can be influenced by changing relevant parameters.
        /// </summary>
        public void FlattenWorld()
        {
            world.FlattenWorld();
        }

        /// <summary>
        /// Generate terrain using current input values.
        /// </summary>
        public void GenerateTerrain()
        {

        }

        /// <summary>
        /// Update displayed map size based on slider value.
        /// </summary>
        public void UpdateMapSizeInputText()
        {
            string newText = mapSizeSlider.value.ToString();
            mapSizeInputField.text = $"{newText}";
        }
        /// <summary>
        /// Update map size slider based on input field value.
        /// </summary>
        public void UpdateMapSizeSlider()
        {
            mapSizeSlider.value = ValidateMapSizeInput(mapSizeInputField.text);
            UpdateMapSizeInputText();
        }
        /// <summary>
        /// Ensure new map size is within allowable parameters.
        /// </summary>
        /// <param name="mapSizeInput"></param>
        /// <returns></returns>
        int ValidateMapSizeInput(string mapSizeInput)
        {
            return Mathf.Clamp(Convert.ToInt32(mapSizeInput), world.minWorldSize, world.maxWorldSize);
        }

        public void UpdateWaterLevelInputText()
        {
            float newWaterLevel = World.RoundToNearestQuarter((waterLevelSlider.value));
            string newText = newWaterLevel.ToString();
            waterLevelInputField.text = $"{newText}";
            world.waterLevel = newWaterLevel;
        }

        public void UpdateWaterLevelSlider()
        {
            waterLevelSlider.value = ValidateWaterLevelInput(waterLevelInputField.text);
            UpdateWaterLevelInputText();
        }

        float ValidateWaterLevelInput(string waterLevelInput)
        {
            return World.RoundToNearestQuarter(Mathf.Clamp(Convert.ToSingle(waterLevelInput), world.bottomOfMapHeight + world.tileHeightStep, world.maxHeight));
        }

        public void UpdateSnowLineInputText()
        {
            float newSnowLine = World.RoundToNearestQuarter((snowLineSlider.value));
            string newText = newSnowLine.ToString();
            snowLineInputField.text = $"{newText}";
            world.snowLine = newSnowLine;
        }

        public void UpdateSnowLineSlider()
        {
            snowLineSlider.value = ValidateSnowLineInput(snowLineInputField.text);
            UpdateSnowLineInputText();
        }

        float ValidateSnowLineInput(string snowLineInput)
        {
            return World.RoundToNearestQuarter(Mathf.Clamp(Convert.ToSingle(snowLineInput), world.bottomOfMapHeight + world.tileHeightStep, world.maxHeight));
        }

        public void UpdateBaseHeightInputText()
        {
            float newBaseHeight = World.RoundToNearestQuarter((baseHeightSlider.value));
            string newText = newBaseHeight.ToString();
            baseHeightInputField.text = $"{newText}";
            world.baseElevation = newBaseHeight;
        }

        public void UpdateBaseHeightSlider()
        {
            baseHeightSlider.value = ValidateBaseHeightInput(baseHeightInputField.text);
            UpdateBaseHeightInputText();
        }

        float ValidateBaseHeightInput(string baseHeightInput)
        {
            return World.RoundToNearestQuarter(Mathf.Clamp(Convert.ToSingle(baseHeightInput), world.bottomOfMapHeight + world.tileHeightStep, world.maxHeight));
        }

        #endregion
        #region Random Generation Panel

        void InitializeRandomGenerationPanel()
        {
            // Minimum Height
            minimumHeightSlider.minValue = world.bottomOfMapHeight + world.tileHeightStep;
            minimumHeightSlider.maxValue = world.maxHeight - world.tileHeightStep;
            // Maximum Height
            maximumHeightSlider.minValue = world.bottomOfMapHeight + (world.tileHeightStep * 2);
            maximumHeightSlider.maxValue = world.maxHeight;
            // Flatness
            flatnessSlider.minValue = world.minFlatness;
            flatnessSlider.maxValue = world.maxFlatness;
            // Elevation Freq
            elevationFreqSlider.minValue = 0.01f;
            elevationFreqSlider.maxValue = world.maxFreq;
            // Moisture Freq
            moistureFreqSlider.minValue = 0.01f;
            moistureFreqSlider.maxValue = world.maxFreq;
            // Elevation Amp
            elevationAmpSlider.minValue = 0.01f;
            elevationAmpSlider.maxValue = world.maxAmp;
            // Moisture Amp
            moistureAmpSlider.minValue = 0.01f;
            moistureAmpSlider.maxValue = world.maxAmp;
            // Elevation Persistence
            elevationPersistenceSlider.minValue = 0.01f;
            elevationPersistenceSlider.maxValue = 1;
            // Moisture Persistence
            moisturePersistenceSlider.minValue = 0.01f;
            moisturePersistenceSlider.maxValue = 1;
            // Elevation Octaves
            elevationOctavesSlider.minValue = 1;
            elevationOctavesSlider.maxValue = 12;
            // Moisture Octaves
            moistureOctavesSlider.minValue = 1;
            moistureOctavesSlider.maxValue = 12;
            // Random Parameters Toggle
            ToggleRandomParameters();

            UpdateRandomParameterValues();
        }

        void UpdateRandomParameterValues()
        {
            // Minimum Height
            minimumHeightInputField.text = $"{world.minGenerationHeight}";
            minimumHeightSlider.value = world.minGenerationHeight;
            // Maximum Height
            maximumHeightInputField.text = $"{world.maxGenerationHeight}";
            maximumHeightSlider.value = world.maxGenerationHeight;
            // Flatness
            flatnessInputField.text = $"{world.flatness}";
            flatnessSlider.value = world.flatness;
            // Elevation Freq
            elevationFreqInputField.text = $"{world.baseElevationFrequency}";
            elevationFreqSlider.value = world.baseElevationFrequency;
            // Moisture Freq
            moistureFreqInputField.text = $"{world.baseMoistureFrequency}";
            moistureFreqSlider.value = world.baseMoistureFrequency;
            // Elevation Amp
            elevationAmpInputField.text = $"{world.baseElevationAmplitude}";
            elevationAmpSlider.value = world.baseElevationAmplitude;
            // Moisture Amp
            moistureAmpInputField.text = $"{world.baseMoistureAmplitude}";
            moistureAmpSlider.value = world.baseMoistureAmplitude;
            // Elevation Persistence
            elevationPersistenceInputField.text = $"{world.elevationAmplitudePersistence}";
            elevationPersistenceSlider.value = world.elevationAmplitudePersistence;
            // Moisture Persistence
            moisturePersistenceInputField.text = $"{world.moistureAmplitudePersistence}";
            moisturePersistenceSlider.value = world.moistureAmplitudePersistence;
            // Elevation Octaves
            elevationOctavesInputField.text = $"{world.numberOfElevationOctaves}";
            elevationOctavesSlider.value = world.numberOfElevationOctaves;
            // Moisture Octaves
            moistureOctavesInputField.text = $"{world.numberOfMoistureOctaves}";
            moistureOctavesSlider.value = world.numberOfMoistureOctaves;
        }

        #region Buttons
        /// <summary>
        /// Generate random terrain using provide input, unless random inputs are enabled.
        /// </summary>
        public void GenerateRandomTerrain()
        {
            // Generate random seeds if enabled
            if (randomElevationSeedEnabled || randomMoistureSeedEnabled)
            {
                world.GenerateRandomGenerationSeed(randomElevationSeedEnabled, randomMoistureSeedEnabled);
                if (randomElevationSeedEnabled)
                {
                    UpdateElevationSeed();
                }
                if (randomMoistureSeedEnabled)
                {
                    UpdateMoistureSeed();
                }
            }
            // Generate random parameters if enabled
            if (randomParametersEnabled)
            {
                world.GenerateRandomGenerationParameters();
            }
            // Update the UI values
            UpdateRandomParameterValues();
            // Generate the random terrain
            world.GenerateRandomTerrain();
        }
        #endregion

        public void ToggleRandomParameters()
        {
            randomParametersEnabled = RandomParametersToggle.isOn;
        }
        public void ToggleRandomSeed()
        {
            randomElevationSeedEnabled = RandomSeedsToggle.isOn;
            randomMoistureSeedEnabled = RandomSeedsToggle.isOn;
        }
        #region Random Seeds
        public void UpdateElevationSeed()
        {
            // If the field is blank, randomise the seed.
            if (elevationSeedInputField.text == "")
            {
                randomElevationSeedEnabled = true;
                elevationSeedInputField.placeholder.GetComponent<Text>().text = $"Current seed: {world.elevationSeed}";
            }
            else
            {
                randomElevationSeedEnabled = false;
                world.elevationSeed = Convert.ToSingle((elevationSeedInputField.text));
            }
        }
        public void UpdateMoistureSeed()
        {
            // If the field is blank, randomise the seed.
            if (moistureSeedInputField.text == "")
            {
                randomMoistureSeedEnabled = true;
                moistureSeedInputField.placeholder.GetComponent<Text>().text = $"Current seed: {world.moistureSeed}";
            }
            else
            {
                randomMoistureSeedEnabled = false;
                world.moistureSeed = Convert.ToSingle((moistureSeedInputField.text));
            }
        }
        #endregion
        #region Min/Max Height
        public void UpdateMinimumHeightInputText()
        {
            float newMinimumHeight = World.RoundToNearestQuarter((minimumHeightSlider.value));
            string newText = newMinimumHeight.ToString();
            minimumHeightInputField.text = $"{newText}";
            world.minGenerationHeight = newMinimumHeight;
        }

        public void UpdateMinimumHeightSlider()
        {
            minimumHeightSlider.value = ValidateMinimumHeightInput(minimumHeightInputField.text);
            UpdateMinimumHeightInputText();
        }

        float ValidateMinimumHeightInput(string minimumHeightText)
        {
            return World.RoundToNearestQuarter(Mathf.Clamp(Convert.ToSingle(minimumHeightText), world.bottomOfMapHeight + (world.tileHeightStep * 2), world.maxHeight - world.tileHeightStep));
        }

        public void UpdateMaximumHeightInputText()
        {
            float newMaximumHeight = World.RoundToNearestQuarter((maximumHeightSlider.value));
            string newText = newMaximumHeight.ToString();
            maximumHeightInputField.text = $"{newText}";
            world.maxGenerationHeight = newMaximumHeight;
        }

        public void UpdateMaximumHeightSlider()
        {
            maximumHeightSlider.value = ValidateMaximumHeightInput(maximumHeightInputField.text);
            UpdateMaximumHeightInputText();
        }

        float ValidateMaximumHeightInput(string maximumHeightText)
        {
            return World.RoundToNearestQuarter(Mathf.Clamp(Convert.ToSingle(maximumHeightText), world.bottomOfMapHeight + world.tileHeightStep, world.maxHeight));

        }
        #endregion
        #region Flatness
        public void UpdateFlatnessInputText()
        {
            float newFlatness = flatnessSlider.value;
            string newText = newFlatness.ToString();
            flatnessInputField.text = newText;
            world.flatness = newFlatness;
        }

        public void UpdateFlatnessSlider()
        {
            flatnessSlider.value = ValidateFlatnessInput(flatnessInputField.text);
            UpdateFlatnessInputText();
        }

        float ValidateFlatnessInput(string flatnessText)
        {
            return Mathf.Clamp(Convert.ToSingle(flatnessText), world.minFlatness, world.maxFlatness);
        }
        #endregion
        #region Frequencies
        public void UpdateElevationFreqInputText()
        {
            float newElevationFreq = elevationFreqSlider.value;
            string newText = newElevationFreq.ToString();
            elevationFreqInputField.text = newText;
            world.baseElevationFrequency = newElevationFreq;
        }

        public void UpdateElevationFreqSlider()
        {
            elevationFreqSlider.value = ValidateElevationFreqInput(elevationFreqInputField.text);
            UpdateElevationFreqInputText();
        }

        float ValidateElevationFreqInput(string elevationFreqText)
        {
            return Mathf.Clamp(Convert.ToSingle(elevationFreqText), world.minFreq, world.maxFreq);
        }

        public void UpdateMoistureFreqInputText()
        {
            float newMoistureFreq = moistureFreqSlider.value;
            string newText = newMoistureFreq.ToString();
            moistureFreqInputField.text = newText;
            world.baseMoistureFrequency = newMoistureFreq;
        }

        public void UpdateMoistureFreqSlider()
        {
            moistureFreqSlider.value = ValidateMoistureFreqInput(moistureFreqInputField.text);
            UpdateMoistureFreqInputText();
        }

        float ValidateMoistureFreqInput(string moistureFreqText)
        {
            return Mathf.Clamp(Convert.ToSingle(moistureFreqText), world.minFreq, world.maxFreq);

        }
        #endregion
        #region Amplitudes
        public void UpdateElevationAmpInputText()
        {
            float newElevationAmp = elevationAmpSlider.value;
            string newText = newElevationAmp.ToString();
            elevationAmpInputField.text = newText;
            world.baseElevationAmplitude = newElevationAmp;
        }

        public void UpdateElevationAmpSlider()
        {
            elevationAmpSlider.value = ValidateElevationAmpInput(elevationAmpInputField.text);
            UpdateElevationAmpInputText();
        }

        float ValidateElevationAmpInput(string elevationAmpText)
        {
            return Mathf.Clamp(Convert.ToSingle(elevationAmpText), world.minAmp, world.maxAmp);
        }

        public void UpdateMoistureAmpInputText()
        {
            float newMoistureAmp = moistureAmpSlider.value;
            string newText = newMoistureAmp.ToString();
            moistureAmpInputField.text = newText;
            world.baseMoistureAmplitude = newMoistureAmp;
        }

        public void UpdateMoistureAmpSlider()
        {
            moistureAmpSlider.value = ValidateMoistureAmpInput(moistureAmpInputField.text);
            UpdateMoistureAmpInputText();
        }

        float ValidateMoistureAmpInput(string moistureAmpText)
        {
            return Mathf.Clamp(Convert.ToSingle(moistureAmpText), world.minAmp, world.maxAmp);
        }
        #endregion
        #region Persistence
        public void UpdateElevationPersistenceInputText()
        {
            float newElevationPersistence = elevationPersistenceSlider.value;
            string newText = newElevationPersistence.ToString();
            elevationPersistenceInputField.text = newText;
            world.elevationAmplitudePersistence = newElevationPersistence;
        }

        public void UpdateElevationPersistenceSlider()
        {
            elevationPersistenceSlider.value = ValidateElevationPersistenceInput(elevationPersistenceInputField.text);
            UpdateElevationPersistenceInputText();
        }

        float ValidateElevationPersistenceInput(string elevationPersistenceText)
        {
            return Mathf.Clamp(Convert.ToSingle(elevationPersistenceText), world.minPersistence, world.maxPersistence);
        }

        public void UpdateMoisturePersistenceInputText()
        {
            float newMoisturePersistence = moisturePersistenceSlider.value;
            string newText = newMoisturePersistence.ToString();
            moisturePersistenceInputField.text = newText;
            world.moistureAmplitudePersistence = newMoisturePersistence;
        }

        public void UpdateMoisturePersistenceSlider()
        {
            moisturePersistenceSlider.value = ValidateMoisturePersistenceInput(moisturePersistenceInputField.text);
            UpdateMoisturePersistenceInputText();
        }

        float ValidateMoisturePersistenceInput(string moisturePersistenceText)
        {
            return Mathf.Clamp(Convert.ToSingle(moisturePersistenceText), world.minPersistence, world.maxPersistence);
        }
        #endregion
        #region Octaves
        public void UpdateElevationOctavesInputText()
        {
            int newElevationOctaves = (int)elevationOctavesSlider.value;
            string newText = newElevationOctaves.ToString();
            elevationOctavesInputField.text = newText;
            world.numberOfElevationOctaves = newElevationOctaves;
        }

        public void UpdateElevationOctavesSlider()
        {
            elevationOctavesSlider.value = ValidateElevationOctavesInput(elevationOctavesInputField.text);
            UpdateElevationOctavesInputText();
        }

        float ValidateElevationOctavesInput(string elevationOctavesText)
        {
            return Mathf.Clamp(Convert.ToInt32(elevationOctavesText), world.minOctaves, world.maxOctaves);
        }

        public void UpdateMoistureOctavesInputText()
        {
            int newMoistureOctaves = (int)moistureOctavesSlider.value;
            string newText = newMoistureOctaves.ToString();
            moistureOctavesInputField.text = newText;
            world.numberOfMoistureOctaves = newMoistureOctaves;
        }

        public void UpdateMoistureOctavesSlider()
        {
            moistureOctavesSlider.value = ValidateMoistureOctavesInput(moistureOctavesInputField.text);
            UpdateMoistureOctavesInputText();
        }

        float ValidateMoistureOctavesInput(string moistureOctavesText)
        {
            return Mathf.Clamp(Convert.ToInt32(moistureOctavesText), world.minOctaves, world.maxOctaves);
        }
        #endregion
        #endregion
        #endregion
        #region Landscaping Window

        public void InitializeLandscapingWindow()
        {
            brushSizeInputField.text = mouseInterface.SelectionBrushSize.ToString();
            brushSizeSlider.value = mouseInterface.SelectionBrushSize;
            brushSizeSlider.minValue = 1;
            brushSizeSlider.maxValue = mouseInterface.maxSelectionBrushSize;
            UpdateAdvancedConstructionToggle();
            UpdateMountainToolToggle();
        }

        public void ToggleAdvancedConstructionMode()
        {
            if (!SettingsHelper.AdvancedConstructionModeEnabled)
            {
                SettingsHelper.AdvancedConstructionModeEnabled = true;
            }
            else
            {
                SettingsHelper.AdvancedConstructionModeEnabled = false;
            }
            Debug.Log("Toggling advanced construction!");

        }

        public void UpdateAdvancedConstructionToggle()
        {
            advancedConstructionToggle.isOn = SettingsHelper.AdvancedConstructionModeEnabled;
        }

        public void ToggleMountainTool()
        {
            if (!landscapingTools.mountainToolEnabled)
            {
                landscapingTools.mountainToolEnabled = true;
            }
            else
            {
                landscapingTools.mountainToolEnabled = false;
            }
            Debug.Log("Toggling mountain tool!");

        }

        public void UpdateMountainToolToggle()
        {
            mountainToolToggle.isOn = landscapingTools.mountainToolEnabled;
        }

        public void UpdateBrushSizeInputText()
        {
            string newText = brushSizeSlider.value.ToString();
            brushSizeInputField.text = $"{newText}";
        }

        public void UpdateBrushSizeUI()
        {
            brushSizeSlider.value = mouseInterface.SelectionBrushSize;
            UpdateBrushSizeInputText();
        }

        public void ChangeBrushSizeFromSlider()
        {
            mouseInterface.SelectionBrushSize = (int)brushSizeSlider.value;
            UpdateBrushSizeUI();
        }
        public void ChangeBrushSizeFromInputField()
        {
            mouseInterface.SelectionBrushSize = ValidateBrushSizeInput(brushSizeInputField.text);
            UpdateBrushSizeUI();
        }

        int ValidateBrushSizeInput(string brushSizeInput)
        {
            return Mathf.Clamp(Convert.ToInt32(brushSizeInput), (int)brushSizeSlider.minValue, mouseInterface.maxSelectionBrushSize);
        }

        #endregion
    }
}
