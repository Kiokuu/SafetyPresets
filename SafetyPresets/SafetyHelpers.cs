using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using MelonLoader;
using VRC;
using VRC.Core;


namespace SafetyPresets
{
    public static class Helpers
    {
        public static void SaveSafetyJSON() => File.WriteAllText($"UserData//{Prefs.UserDataFileName}",JsonConvert.SerializeObject(Settings.availablePresets,Formatting.Indented));

        public static string GetPresetName(int presetnum) => Settings.availablePresets.ActualPresets[presetnum-1].settingsPresetName;

        public static bool IsPresetValid(int presetnum) => (Settings.availablePresets.ActualPresets[presetnum-1].settingRanks != null && Settings.availablePresets.ActualPresets[presetnum-1].settingRanks.Count>0);

        public static List<Classes.SettingsPreset> ValidPresetList()
        {
            return Settings.availablePresets.ActualPresets.Where(p => p.settingRanks.Count>0).ToList();
        } 

        public static IList<(string presetNumber,string friendlyName)> ValidPresetIList(){
            List<(string,string)> tempReturnList = new List<(string, string)>();
            try
            {
                foreach(Classes.SettingsPreset preset in ValidPresetList()){
                    tempReturnList.Add((preset.settingsPresetNum.ToString(),preset.settingsPresetName));
                }    
            }
            catch(Exception e)
            {
                MelonLogger.Msg("[NotAnError] -> No presets to populate dropdowns");
            }
            return tempReturnList;
        }

        public static IEnumerator ChangeOnInstance()
        {
            while(RoomManager.field_Internal_Static_ApiWorldInstance_0?.InstanceType==null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            var instanceType = RoomManager.field_Internal_Static_ApiWorldInstance_0?.InstanceType;
            yield return new WaitForSeconds(0.5f);

            MelonLogger.Msg("Current Instance Type - "+instanceType.Value);
            
            if(instanceType == ApiWorldInstance.AccessType.Public && Prefs.DoChangeInPublics)
            {
                LoadSafetySettings(Prefs.DoChangeInPublicsPreset());
                MelonLoader.MelonLogger.Msg("Public instance -> should be changing");
            }
            if((instanceType==ApiWorldInstance.AccessType.FriendsOnly || instanceType==ApiWorldInstance.AccessType.FriendsOfGuests) && Prefs.DoChangeInFriends)
            {
                LoadSafetySettings(Prefs.DoChangeInFriendsPreset());
                MelonLoader.MelonLogger.Msg("Friends instance -> should be changing");
            }
            if((instanceType==ApiWorldInstance.AccessType.InviteOnly || instanceType==ApiWorldInstance.AccessType.InvitePlus) && Prefs.DoChangeInPrivates)
            {
                LoadSafetySettings(Prefs.DoChangeInPrivatesPreset());
                MelonLoader.MelonLogger.Msg("Private instance -> should be changing");
            }
        }

        public static void SaveSafetySettings(int presetNum,string name)
        {
            try
            {
                FeaturePermissionManager fManager = GameObject.Find("_Application").GetComponent<FeaturePermissionManager>();
                fManager.Method_Public_Void_2(); // Apply current safety settings
                List<Classes.RankSetting> rankSettingList = new List<Classes.RankSetting>{};
                foreach(Il2CppSystem.Collections.Generic.KeyValuePair<UserSocialClass,FeaturePermissionSet> rankPerm in fManager.field_Private_Dictionary_2_UserSocialClass_FeaturePermissionSet_0)
                {

                    Dictionary<string,bool> tempSettings = new Dictionary<string,bool>
                    {
                        {"SpeakingPermission",rankPerm.Value.field_Public_Boolean_0},
                        {"AvatarPermission",rankPerm.Value.field_Public_Boolean_1},
                        {"UserIconPermission",rankPerm.Value.field_Public_Boolean_2},
                        {"AudioPermission",rankPerm.Value.field_Public_Boolean_3},
                        {"ParticlesLightsPermission",rankPerm.Value.field_Public_Boolean_4},
                        {"ShaderPermission",rankPerm.Value.field_Public_Boolean_5},
                        {"AnimationPermission",rankPerm.Value.field_Public_Boolean_6}
                    };

                    Classes.RankSetting tempRankSetting = new Classes.RankSetting(rankPerm.Key,tempSettings);
                    rankSettingList.Add(tempRankSetting);
                }

                Classes.SettingsPreset presetTest = new Classes.SettingsPreset(presetNum,name);
                presetTest.settingRanks = rankSettingList;

                Settings.availablePresets.ActualPresets[presetNum-1] = presetTest;

                Helpers.SaveSafetyJSON();
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error(e.Message);
            }
        }

        public static void LoadSafetySettings(int presetNum)
        {
            Classes.SettingsPreset toLoadPreset;
            try
            {
                toLoadPreset = Settings.availablePresets.ActualPresets.ElementAt(presetNum-1);
            }
            catch
            {
                MelonLoader.MelonLogger.Msg("No saved safety settings could be loaded");
                return;
            }

            if(!IsPresetValid(toLoadPreset.settingsPresetNum))
            {
                MelonLoader.MelonLogger.Msg("This preset is an empty default. Nothing to load here.");
                return;
            }

            try
            {
                FeaturePermissionManager fManager = GameObject.Find("_Application").GetComponent<FeaturePermissionManager>();
                foreach(Il2CppSystem.Collections.Generic.KeyValuePair<UserSocialClass,FeaturePermissionSet> rankPerm in fManager.field_Private_Dictionary_2_UserSocialClass_FeaturePermissionSet_0)
                {

                    FeaturePermissionSet test = rankPerm.Value;

                    foreach(Classes.RankSetting rSetting in toLoadPreset.settingRanks)
                    {
                        if(rSetting.UserRank == rankPerm.Key){
                            test.field_Public_Boolean_0 = rSetting.UserSettings["SpeakingPermission"]; // Voice
                            test.field_Public_Boolean_1 = rSetting.UserSettings["AvatarPermission"]; // Avatar
                            test.field_Public_Boolean_2 = rSetting.UserSettings["UserIconPermission"]; // UserIcons
                            test.field_Public_Boolean_3 = rSetting.UserSettings["AudioPermission"]; // Audio
                            test.field_Public_Boolean_4 = rSetting.UserSettings["ParticlesLightsPermission"]; // Light&Particles
                            test.field_Public_Boolean_5 = rSetting.UserSettings["ShaderPermission"]; // Shaders
                            test.field_Public_Boolean_6 = rSetting.UserSettings["AnimationPermission"]; // CustomAnimations
                        }
                    }
                }
                fManager.Method_Public_Void_2(); // Apply Safety Settings / Reload avatars
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Error(e.StackTrace);
            }
        }
    }
}