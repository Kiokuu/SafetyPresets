using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

using UnityEngine;
using MelonLoader;
using VRC;
using VRC.Core;


namespace SafetyPresets
{
    public static class Helpers
    {
        public static void SaveSafetyJSON() => File.WriteAllText($"UserData\\{Prefs.UserDataFileName}", JsonConvert.SerializeObject(Settings.availablePresets, Formatting.Indented));

        public static string GetPresetName(int presetnum)
        {
            try
            {
                return Settings.availablePresets.ActualPresets[GetActualPresetFromPresetNum(presetnum)].settingsPresetName;
            }
            catch
            {
                return "New Preset";
            }
        }

        public static int GetActualPresetFromPresetNum(int presetnum)
        {
            return Settings.availablePresets.ActualPresets.FindIndex(sss => sss.settingsPresetNum == presetnum);
        }

        public static bool IsPresetValid(int presetnum) => (Settings.availablePresets.ActualPresets[GetActualPresetFromPresetNum(presetnum)].settingRanks != null && Settings.availablePresets.ActualPresets[GetActualPresetFromPresetNum(presetnum)].settingRanks.Count > 0);

        public static int NextAvailablePresetNum() {
            try
            {
                return Settings.availablePresets.ActualPresets[Settings.availablePresets.ActualPresets.Count - 1].settingsPresetNum + 1;
            }
            catch // probably have 0 presets
            {
                return 1;
            }
        }
        public static List<Classes.SettingsPreset> ValidPresetList()
        {
            return Settings.availablePresets.ActualPresets.Where(p => IsPresetValid(p.settingsPresetNum)).ToList();
        }

        public static bool AreWeUsingCustomSafetySettings()
        {
            var safetyLevelValue = VRCInputManager.field_Private_Static_Dictionary_2_String_Object_0["setting_SafetyLevel"].Unbox<int>();
            if(safetyLevelValue!=5) MelonLogger.Msg("Not touching safety settings as they are not custom!");
            return safetyLevelValue == 5;
        }
        
        private static MethodBase applySafety;
        public static void DoXrefMagic()
        {
            MelonLogger.Msg("Beginning Xref scanning");
            applySafety = typeof(FeaturePermissionManager).GetMethods()
                .First(methodBase => methodBase.Name.StartsWith("Method_Public_Void_")
                                     && !methodBase.Name.Contains("PDM") 
                                     && SafetyXref.CheckMethod(methodBase, "Safety Settings Changed to: "));
            
            MelonLogger.Msg($"OnTrustSettingsChanged determined to be: {applySafety.Name}");
        }

        public static IList<(string presetNumber,string friendlyName)> ValidPresetIList(){
            var tempReturnList = new List<(string, string)>();
            try
            {
                tempReturnList.AddRange(ValidPresetList().Select(preset => (preset.settingsPresetNum.ToString(), preset.settingsPresetName)));
            }
            catch
            {
                MelonLogger.Msg("[NotAnError] -> No presets to populate dropdowns in the mod settings.");
            }
            return tempReturnList;
        }

        public static IEnumerator ChangeOnInstance()
        {
            while(RoomManager.field_Internal_Static_ApiWorldInstance_0?.type == null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            var instanceType = RoomManager.field_Internal_Static_ApiWorldInstance_0?.type;
            yield return new WaitForSeconds(0.5f);

            MelonLogger.Msg($"Current Instance Type -> {instanceType.Value}");
            
            if(instanceType == InstanceAccessType.Public && Prefs.DoChangeInPublics)
            {
                LoadSafetySettings(Prefs.DoChangeInPublicsPreset());
                MelonLogger.Msg("Public instance -> changing safety preset.");
            }
            else if(instanceType==InstanceAccessType.FriendsOfGuests && Prefs.DoChangeInFriends)
            {
                LoadSafetySettings(Prefs.DoChangeInFriendsPreset());
                MelonLogger.Msg("Friends instance -> changing safety preset.");
            }
            else if(Prefs.DoChangeInPrivates)
            {
                LoadSafetySettings(Prefs.DoChangeInPrivatesPreset());
                MelonLogger.Msg("Private instance -> changing safety preset.");
            }
        }

        public static void SaveSafetySettings(int presetNum,string name)
        {
            if (!AreWeUsingCustomSafetySettings()) return;
            try
            {
                
                var fManager = GameObject.Find("_Application").GetComponent<FeaturePermissionManager>();
                applySafety.Invoke(FeaturePermissionManager.prop_FeaturePermissionManager_0, new object[] { } );
                var rankSettingList = new List<Classes.RankSetting>{};
                foreach(var rankPerm in fManager.field_Private_Dictionary_2_UserSocialClass_ObjectPublicStBoBoStBoBoBoBoBoBoUnique_0) // .field_Private_Dictionary_2_UserSocialClass_FeaturePermissionSet_0
                {

                    Dictionary<string,bool> tempSettings = new Dictionary<string,bool>
                    {
                        {"SpeakingPermission",rankPerm.Value.field_Private_Boolean_0},
                        {"AvatarPermission",rankPerm.Value.field_Private_Boolean_1},
                        {"UserIconPermission",rankPerm.Value.field_Private_Boolean_2},
                        {"AudioPermission",rankPerm.Value.field_Private_Boolean_3},
                        {"ParticlesLightsPermission",rankPerm.Value.field_Private_Boolean_4},
                        {"ShaderPermission",rankPerm.Value.field_Private_Boolean_5},
                        {"AnimationPermission",rankPerm.Value.field_Private_Boolean_6}
                    };

                    var tempRankSetting = new Classes.RankSetting(rankPerm.Key,tempSettings);
                    rankSettingList.Add(tempRankSetting);
                }

                var presetTest = new Classes.SettingsPreset(presetNum, name) {settingRanks = rankSettingList};
                try
                {
                    Settings.availablePresets.ActualPresets[GetActualPresetFromPresetNum(presetNum)] = presetTest; 
                }
                catch
                {
                    Settings.availablePresets.ActualPresets.Add(presetTest);
                }

                Helpers.SaveSafetyJSON();

                MelonLogger.Msg($"Saved safety preset -> \"{name}\" ({presetNum})");

                Settings.UpdateSelectablePresets();
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error(e.Message);
            }
        }

        public static void LoadSafetySettings(int presetNum)
        {
            if (!AreWeUsingCustomSafetySettings()) return;
            Classes.SettingsPreset toLoadPreset;
            try
            {
                toLoadPreset = Settings.availablePresets.ActualPresets[GetActualPresetFromPresetNum(presetNum)];
            }
            catch
            {
                MelonLogger.Msg("No saved safety settings could be loaded.");
                return;
            }

            if(!IsPresetValid(toLoadPreset.settingsPresetNum))
            {
                MelonLogger.Msg("This preset is an empty default. Nothing to load here.");
                return;
            }

            try
            {
                FeaturePermissionManager fManager = GameObject.Find("_Application").GetComponent<FeaturePermissionManager>();
                foreach(Il2CppSystem.Collections.Generic.KeyValuePair<UserSocialClass, ObjectPublicStBoBoStBoBoBoBoBoBoUnique> rankPerm in fManager.field_Private_Dictionary_2_UserSocialClass_ObjectPublicStBoBoStBoBoBoBoBoBoUnique_0)
                {
                    ObjectPublicStBoBoStBoBoBoBoBoBoUnique test = rankPerm.Value;

                    foreach (var rSetting in toLoadPreset.settingRanks.Where(rSetting => rSetting.UserRank == rankPerm.Key))
                    {
                        test.field_Private_Boolean_0 = rSetting.UserSettings["SpeakingPermission"]; // Voice
                        test.field_Private_Boolean_1 = rSetting.UserSettings["AvatarPermission"]; // Avatar
                        test.field_Private_Boolean_2 = rSetting.UserSettings["UserIconPermission"]; // UserIcons
                        test.field_Private_Boolean_3 = rSetting.UserSettings["AudioPermission"]; // Audio
                        test.field_Private_Boolean_4 = rSetting.UserSettings["ParticlesLightsPermission"]; // Light&Particles
                        test.field_Private_Boolean_5 = rSetting.UserSettings["ShaderPermission"]; // Shaders
                        test.field_Private_Boolean_6 = rSetting.UserSettings["AnimationPermission"]; // CustomAnimations
                    }
                }
                MelonLogger.Msg($"Loaded safety preset -> \"{toLoadPreset.settingsPresetName}\" ({toLoadPreset.settingsPresetNum})");
                applySafety.Invoke(FeaturePermissionManager.prop_FeaturePermissionManager_0, new object[] { });
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error(e.StackTrace);
            }
        }

        public static void DeleteSafetySettings(int presetNum)
        {
            Settings.availablePresets.ActualPresets.Remove(Settings.availablePresets.ActualPresets[GetActualPresetFromPresetNum(presetNum)]);
            Helpers.SaveSafetyJSON();
            Settings.UpdateSelectablePresets();
        }
    }
}