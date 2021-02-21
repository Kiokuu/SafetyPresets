using MelonLoader;
using System;
using UIExpansionKit.API;
using System.Collections;

namespace SafetyPresets
{
    public static class Prefs
    {   
        public const string UserDataFileName = "SafetyPresets.json";
        private const string SettingCategory = "SafetyPresetsMod";
        private const string PublicPreset = "PublicPreset";
        private const string FriendsPreset = "FriendsPreset";
        private const string PrivatePreset = "PrivatePreset";
        private const string ChangeInPublic = "ChangeInPublic";
        private const string ChangeInFriends = "ChangeInFriends";
        private const string ChangeInPrivate = "ChangeInPrivate";
        public static void SetupPrefs()
        {
            MelonPreferences.CreateCategory(SettingCategory, "Safety Presets Mod");

            MelonPreferences.CreateEntry<string>(SettingCategory,PublicPreset,"No Preset Selected","Public Preset");
            MelonPreferences.CreateEntry<string>(SettingCategory,FriendsPreset,"No Preset Selected","Friends Preset");
            MelonPreferences.CreateEntry<string>(SettingCategory,PrivatePreset,"No Preset Selected","Private Preset");

            ExpansionKitApi.RegisterSettingAsStringEnum(SettingCategory,PublicPreset,Settings.selectablePresets);
            ExpansionKitApi.RegisterSettingAsStringEnum(SettingCategory,FriendsPreset,Settings.selectablePresets);
            ExpansionKitApi.RegisterSettingAsStringEnum(SettingCategory,PrivatePreset,Settings.selectablePresets);

            
            MelonPreferences.CreateEntry<bool>(SettingCategory, ChangeInPublic, false, "Change safety setting preset in public?");
            MelonPreferences.CreateEntry<bool>(SettingCategory, ChangeInFriends, false, "Change safety setting preset in friends?");
            MelonPreferences.CreateEntry<bool>(SettingCategory, ChangeInPrivate, false, "Change safety setting preset in privates?");
        }

        public static bool DoChangeInPublics => MelonPreferences.GetEntryValue<bool>(SettingCategory, ChangeInPublic );
        public static bool DoChangeInFriends => MelonPreferences.GetEntryValue<bool>(SettingCategory, ChangeInFriends );
        public static bool DoChangeInPrivates => MelonPreferences.GetEntryValue<bool>(SettingCategory, ChangeInPrivate );

        public static int DoChangeInPublicsPreset()
        {
            int PresetInt = 1;
            Int32.TryParse(MelonPreferences.GetEntryValue<string>(SettingCategory, PublicPreset),out PresetInt);
            return PresetInt;
        } 
        public static int DoChangeInFriendsPreset()
        {
            int PresetInt = 1;
            Int32.TryParse(MelonPreferences.GetEntryValue<string>(SettingCategory, FriendsPreset),out PresetInt);
            return PresetInt;
        }

        public static int DoChangeInPrivatesPreset(){
            int PresetInt = 1;
            Int32.TryParse(MelonPreferences.GetEntryValue<string>(SettingCategory, PrivatePreset),out PresetInt);
            return PresetInt;
        }
    }
}