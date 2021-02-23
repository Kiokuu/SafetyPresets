using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UIExpansionKit.API;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using static UnityEngine.UI.InputField;
using static VRC.Core.ApiWorldInstance;

namespace SafetyPresets
{
    class SafetySaveManager
    {
        private static List<SavedSafetySetting> SavedSafetySettings = new List<SavedSafetySetting>();

        public static List<(string, string)> SelectableSafetySettings => SavedSafetySettings.Select(sss => (sss.SaveIndex + "", sss.SettingName)).ToList();

        private static List<int> OurCachedSafetyValues = null;

        private static MethodInfo ShowPopup;
        private static MethodInfo ClosePopup;
        private static MethodInfo CloseMenu;

        private static VRCUiPageSafety SafetyPage;
        private static PropertyInfo SafetyFeatureValue;
        private static MethodInfo FeatureSetEnabled;
        private static MethodInfo OnTrustSettingsChanged;

        public static void Initialize()
        {
            if (Directory.Exists("UserData"))
            {
                if (File.Exists("UserData/SafetyPresets.json"))
                {
                    try
                    {
                        SavedSafetySettings = JsonConvert.DeserializeObject<List<SavedSafetySetting>>(File.ReadAllText("UserData/SafetyPresets.json"));
                    } catch (JsonSerializationException)
                    {
                        // Fuck this behaviour
                        SavedSafetySettings.Add(JsonConvert.DeserializeObject<SavedSafetySetting>(File.ReadAllText("UserData/SafetyPresets.json")));
                    }
                }
            }
            else
            {
                Directory.CreateDirectory("UserData");
            }

            ShowPopup = typeof(VRCUiPopupManager).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") 
                    && CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2")).First();
            ClosePopup = typeof(VRCUiPopupManager).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_String_") && mb.Name.Length <= 28 && !mb.Name.Contains("PDM") && CheckUsed(mb, "Close")).First();
            CloseMenu = typeof(VRCUiManager).GetMethods()
                     .Where(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_") && CheckUsed(mb, "ShowAddMessagePopup")).First();

            SafetyFeatureValue = typeof(UiSafetyFeatureToggle).GetProperties().Where(pi => pi.PropertyType.IsEnum).First();
            FeatureSetEnabled = typeof(FeaturePermissionSet).GetMethods().Where(
                m => m.ReturnType == typeof(bool) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.IsEnum && !m.Name.Contains("_PDM_")).Single();
            OnTrustSettingsChanged = typeof(FeaturePermissionManager).GetMethods().Where(
                m => m.ReturnType == typeof(void) && CheckMethod(m, "Avatar Performance Rating Minimum To Display changed to: ")).Single();

            MelonLogger.Msg($"Show PopupV2: {ShowPopup.Name}");
            MelonLogger.Msg($"Close Popup: {ClosePopup.Name}");
            MelonLogger.Msg($"Close Menu: {CloseMenu.Name}");
            MelonLogger.Msg($"Safety Feature Property: {SafetyFeatureValue.Name}");
            MelonLogger.Msg($"OnTrustSettingsChanged: {OnTrustSettingsChanged.Name}");
            MelonLogger.Msg($"Feature Set Enabled: {FeatureSetEnabled.Name}");

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SafetyMenu).AddSimpleButton($"Delete Preset", OpenDeleteMenu);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SafetyMenu).AddSimpleButton($"Load Preset", OpenLoadMenu);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SafetyMenu).AddSimpleButton($"Save Preset", OpenSaveMenu);
        }

        private static bool CheckMethod(MethodBase methodBase, string match)
        {
            try
            {
                return UnhollowerRuntimeLib.XrefScans.XrefScanner.XrefScan(methodBase)
                    .Where(instance => instance.Type == UnhollowerRuntimeLib.XrefScans.XrefType.Global && instance.ReadAsObject().ToString().Contains(match)).Any();
            }
            catch { }
            return false;
        }

        private static bool CheckUsed(MethodBase methodBase, string methodName)
        {
            try
            {
                return UnhollowerRuntimeLib.XrefScans.XrefScanner.UsedBy(methodBase)
                    .Where(instance => instance.TryResolve() != null && instance.TryResolve().Name.Contains(methodName)).Any();
            }
            catch { }
            return false;
        }

        public static int GetNextIndex()
        {
            if (SavedSafetySettings == null
                || SavedSafetySettings.Count == 0)
                return 1;

            return SavedSafetySettings.OrderByDescending(sss => sss.SaveIndex).First().SaveIndex + 1;
        }

        private static void OpenLoadMenu()
        {
            var LoadMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.QuickMenu3Columns);

            SavedSafetySettings.OrderBy(sss => sss.SaveIndex).ToList().ForEach(sss => LoadMenu.AddSimpleButton($"Load {sss.SettingName}", () =>
            {
                LoadSavedSetting(sss.SaveIndex);
                LoadMenu.Hide();
                CloseMenu.Invoke(VRCUiManager.prop_VRCUiManager_0, new object[] { false, false });
            }));

            LoadMenu.AddSimpleButton("Close", () => LoadMenu.Hide());
            LoadMenu.Show();
        }

        private static void OpenSaveMenu()
        {
            var SaveMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.QuickMenu3Columns);

            SavedSafetySettings.OrderBy(sss => sss.SaveIndex).ToList().ForEach(sss => SaveMenu.AddSimpleButton($"Save {sss.SettingName}", () =>
            {
                SaveSetting(sss.SaveIndex);
                SaveMenu.Hide();
            }));

            SaveMenu.AddSimpleButton("Save New", () =>
            {
                AskForName();
                SaveMenu.Hide();
            });

            SaveMenu.AddSimpleButton("Close", () => SaveMenu.Hide());
            SaveMenu.Show();
        }

        private static void OpenDeleteMenu()
        {
            var DeleteMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.QuickMenu3Columns);

            SavedSafetySettings.OrderBy(sss => sss.SaveIndex).ToList().ForEach(sss => DeleteMenu.AddSimpleButton($"Delete {sss.SettingName}", () =>
            {
                SaveSetting(sss.SaveIndex, "", true);
                DeleteMenu.Hide();
            }));

            DeleteMenu.AddSimpleButton("Close", () => DeleteMenu.Hide());
            DeleteMenu.Show();
        }

        public static void OnInstanceChange(AccessType? OurAccessType)
        {
            if (!OurAccessType.HasValue)
                return;

            switch (OurAccessType)
            {
                case AccessType.Public:
                    if (SafetyPreferences.DoChangeInPublics)
                    {
                        MelonLogger.Msg($"Public Instance -> Loading IDX: {SafetyPreferences.DoChangeInPublicsPreset()}");

                        bool DidLoad = LoadSavedSetting(SafetyPreferences.DoChangeInPublicsPreset());

                        if (!DidLoad)
                        {
                            MelonLogger.Msg($"Failed To Load IDX: {SafetyPreferences.DoChangeInPublicsPreset()}");
                        }
                    }

                    return;
                case AccessType.FriendsOnly:
                    if (SafetyPreferences.DoChangeInFriends)
                    {
                        MelonLogger.Msg($"Friends Instance -> Loading IDX: {SafetyPreferences.DoChangeInFriendsPreset()}");

                        bool DidLoad = LoadSavedSetting(SafetyPreferences.DoChangeInFriendsPreset());

                        if (!DidLoad)
                        {
                            MelonLogger.Msg($"Failed To Load IDX: {SafetyPreferences.DoChangeInFriendsPreset()}");
                        }
                    }

                    return;
                case AccessType.InviteOnly:
                    if (SafetyPreferences.DoChangeInPrivates)
                    {
                        MelonLogger.Msg($"Private Instance -> Loading IDX: {SafetyPreferences.DoChangeInPrivatesPreset()}");

                        bool DidLoad = LoadSavedSetting(SafetyPreferences.DoChangeInPrivatesPreset());

                        if (!DidLoad)
                        {
                            MelonLogger.Msg($"Failed To Load IDX: {SafetyPreferences.DoChangeInPrivatesPreset()}");
                        }
                    }

                    return;
                default:
                    return;
            }
        }

        private static bool LoadSavedSetting(int SaveIndex)
        {
            Cache();

            SavedSafetySetting SavedSafetySetting = SavedSafetySettings.Find(sss => sss.SaveIndex == SaveIndex);

            if (SavedSafetySetting == null)
                return false;

            GameObject SafetyMatrixToggles = GameObject.Find("UserInterface/MenuContent/Screens/Settings_Safety/_SafetyMatrix/_Toggles");
            GameObject UserLevelSelects = GameObject.Find("UserInterface/MenuContent/Screens/Settings_Safety/_Buttons_UserLevel");

            Il2CppArrayBase<UiSafetyRankToggle> LevelSelects = UserLevelSelects.GetComponentsInChildren<UiSafetyRankToggle>();
            Il2CppArrayBase<UiSafetyFeatureToggle> SafetyToggles = SafetyMatrixToggles.GetComponentsInChildren<UiSafetyFeatureToggle>();

            foreach (UserSocialClass SocialClass in Enum.GetValues(typeof(UserSocialClass)))
            {
                // Dirty workaround for DBS (Patch was inconsistent, and this is literally just the selected social class)
                foreach (UiSafetyRankToggle SafetyRankToggle in LevelSelects)
                {
                    if (SafetyRankToggle.field_Public_UserSocialClass_0 == SocialClass)
                        SafetyRankToggle.GetComponent<Toggle>().isOn = true;
                }

                SafetyPage.field_Private_UserSocialClass_0 = SocialClass;

                foreach (UiSafetyFeatureToggle SafetyFeatureToggle in SafetyToggles)
                {
                    SafetySetting? SafetySetting = SavedSafetySetting.GetValue(SocialClass, (int)SafetyFeatureValue.GetValue(SafetyFeatureToggle));

                    if (SafetySetting.HasValue)
                        SafetyFeatureToggle.GetComponent<Toggle>().isOn = SafetySetting.Value.Value;
                }
            }

            OnTrustSettingsChanged.Invoke(FeaturePermissionManager.prop_FeaturePermissionManager_0, new object[] { });

            return true;
        }

        private static void AskForName()
        {
            BuiltinUiUtils.ShowInputPopup("Enter a new preset name", "", InputType.Standard, false, "Save", (NewName, unused_, unused__) =>
            {
                if (NewName.Length == 0)
                {
                    MelonCoroutines.Start(ShowError());
                    return;
                }

                SaveSetting(GetNextIndex(), NewName);
            });
        }

        // Hack to make popup work
        private static IEnumerator ShowError()
        {
            yield return new WaitForSeconds(0.1f);

            ShowPopup.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[] {
                        "Oops!",
                        "Please enter a valid name for your preset.",
                        "Ok",
                        (Il2CppSystem.Action)new Action(() => ClosePopup.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[] { "POPUP" })),
                        null
                    });
        }

        private static void SaveSetting(int SaveIndex, string NewName = "", bool remove = false)
        {
            Cache();

            if (NewName == null || NewName.Equals(""))
                NewName = SavedSafetySettings.Find(sss => sss.SaveIndex == SaveIndex).SettingName;

            SavedSafetySettings.RemoveAll(sss => sss.SaveIndex == SaveIndex);

            if (!remove)
            {
                SavedSafetySetting SavedSafetySetting = new SavedSafetySetting(SaveIndex, NewName);

                foreach (UserSocialClass SocialClass in Enum.GetValues(typeof(UserSocialClass)))
                {
                    // Dirty workaround for DBS (Patch was inconsistent, and this is literally just the selected social class)
                    SafetyPage.field_Private_UserSocialClass_0 = SocialClass;
                    FeaturePermissionSet FeaturePermissionSet = FeaturePermissionManager.prop_FeaturePermissionManager_0.Method_Private_FeaturePermissionSet_UserSocialClass_0(SocialClass);

                    foreach (int FeatureValue in OurCachedSafetyValues)
                    {
                        bool IsEnabled = (bool)FeatureSetEnabled.Invoke(FeaturePermissionSet, new object[] { FeatureValue });

                        SavedSafetySetting.AddValue(SocialClass, new SafetySetting()
                        {
                            SafetyType = FeatureValue,
                            Value = IsEnabled
                        });
                    }
                }

                SavedSafetySettings.Add(SavedSafetySetting);
            }

            SafetyPreferences.ReloadUIXOptions();

            File.WriteAllText("UserData/SafetyPresets.json", JsonConvert.SerializeObject(SavedSafetySettings, Formatting.Indented));
        }

        private static void Cache()
        {
            if (OurCachedSafetyValues == null)
            {
                SafetyPage = GameObject.Find("UserInterface/MenuContent/Screens/Settings_Safety").GetComponent<VRCUiPageSafety>();

                OurCachedSafetyValues = new List<int>();

                GameObject SafetyMatrixToggles = GameObject.Find("UserInterface/MenuContent/Screens/Settings_Safety/_SafetyMatrix/_Toggles");

                Il2CppArrayBase<UiSafetyFeatureToggle> SafetyToggles = SafetyMatrixToggles.GetComponentsInChildren<UiSafetyFeatureToggle>();

                foreach (UiSafetyFeatureToggle SafetyFeatureToggle in SafetyToggles)
                {
                    OurCachedSafetyValues.Add((int)SafetyFeatureValue.GetValue(SafetyFeatureToggle));
                }
            }
        }
    }
}
