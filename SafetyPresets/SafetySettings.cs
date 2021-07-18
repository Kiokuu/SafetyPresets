using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SafetyPresets
{
    internal class Settings{
        public static Classes.Presets availablePresets;
        public static IList<(string,string)> selectablePresets = new List<(string,string)>{};

        public static void SetupDefaults()
        {
            if (!Directory.Exists("UserData"))
            {
                Directory.CreateDirectory("UserData");
            }

            try
            {
                availablePresets = JsonConvert.DeserializeObject<Classes.Presets>(File.ReadAllText($"UserData\\{Prefs.UserDataFileName}")); 
            }
            catch
            {
                MelonLoader.MelonLogger.Msg("No available presets have been saved - thats ok!");
                List<Classes.SettingsPreset> test = new List<Classes.SettingsPreset>(){}; 
                availablePresets = new Classes.Presets(test);
            }

            Helpers.SaveSafetyJSON();
            UpdateSelectablePresets();
        }

        public static void UpdateSelectablePresets(){
            // Update UIX String Enum properly (You can't just replace the list)
            selectablePresets.Clear();
            foreach(var presetInList in Helpers.ValidPresetIList()){
                selectablePresets.Add(presetInList);
            }
        }
    }
}