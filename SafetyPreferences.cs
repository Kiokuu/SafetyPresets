using MelonLoader;
using System;
using UIExpansionKit.API;

namespace SafetyPresets
{
    class SafetyPreferences
    {
        private const string SettingCategory = "SafetyPresetsMod";
        private const string PublicPreset = "PublicPreset";
        private const string FriendsPreset = "FriendsPreset";
        private const string PrivatePreset = "PrivatePreset";
        private const string ChangeInPublic = "ChangeInPublic";
        private const string ChangeInFriends = "ChangeInFriends";
        private const string ChangeInPrivate = "ChangeInPrivate";

        private static MelonPreferences_Category OurCategory;

        public static void SetupPrefs()
        {
            OurCategory = MelonPreferences.CreateCategory(SettingCategory, "Safety Presets Mod");

            OurCategory.CreateEntry(PublicPreset, "No Preset Selected", "Public Preset");
            OurCategory.CreateEntry(FriendsPreset, "No Preset Selected", "Friends Preset");
            OurCategory.CreateEntry(PrivatePreset, "No Preset Selected", "Private Preset");

            ExpansionKitApi.RegisterSettingAsStringEnum(SettingCategory, PublicPreset, SafetySaveManager.SelectableSafetySettings);
            ExpansionKitApi.RegisterSettingAsStringEnum(SettingCategory, FriendsPreset, SafetySaveManager.SelectableSafetySettings);
            ExpansionKitApi.RegisterSettingAsStringEnum(SettingCategory, PrivatePreset, SafetySaveManager.SelectableSafetySettings);

            OurCategory.CreateEntry(ChangeInPublic, false, "Change safety setting preset in public?");
            OurCategory.CreateEntry(ChangeInFriends, false, "Change safety setting preset in friends?");
            OurCategory.CreateEntry(ChangeInPrivate, false, "Change safety setting preset in privates?");
        }

        public static bool DoChangeInPublics => OurCategory.GetEntry<bool>(ChangeInPublic).Value;
        public static bool DoChangeInFriends => OurCategory.GetEntry<bool>(ChangeInFriends).Value;
        public static bool DoChangeInPrivates => OurCategory.GetEntry<bool>(ChangeInPrivate).Value;

        public static int DoChangeInPublicsPreset()
        {
            int PresetInt = -1;
            Int32.TryParse(OurCategory.GetEntry<string>(PublicPreset).Value, out PresetInt);
            return PresetInt;
        }
        public static int DoChangeInFriendsPreset()
        {
            int PresetInt = -1;
            Int32.TryParse(OurCategory.GetEntry<string>(FriendsPreset).Value, out PresetInt);
            return PresetInt;
        }

        public static int DoChangeInPrivatesPreset()
        {
            int PresetInt = -1;
            Int32.TryParse(OurCategory.GetEntry<string>(PrivatePreset).Value, out PresetInt);
            return PresetInt;
        }
    }
}
