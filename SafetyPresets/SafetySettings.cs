using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SafetyPresets
{
    internal class Settings{
        public static Classes.Presets availablePresets;

        public static void SetupDefaults()
        {
            if (!Directory.Exists("UserData"))
            {
                Directory.CreateDirectory("UserData");
            }

            if (!File.Exists($"UserData/{Prefs.UserDataFileName}"))
            {
                File.Create("UserData//" + Prefs.UserDataFileName);
            }

            // Try loading available presets, if failed, generate blank presets
            try
            {
                availablePresets = JsonConvert.DeserializeObject<Classes.Presets>(File.ReadAllText($"UserData//{Prefs.UserDataFileName}"));
            }
            catch
            {
                MelonLoader.MelonLogger.Msg("No available presets have been saved - thats ok!");
                List<Classes.SettingsPreset> test = new List<Classes.SettingsPreset>()
                {
                    new Classes.SettingsPreset(1,"Preset1<Empty>"),
                    new Classes.SettingsPreset(2,"Preset2<Empty>"),
                    new Classes.SettingsPreset(3,"Preset3<Empty>") 
                }; 
                availablePresets = new Classes.Presets(test);
            }

        }
    }
}