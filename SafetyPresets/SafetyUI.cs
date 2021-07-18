using UIExpansionKit.API;
using UnityEngine.UI;
using System.Linq;

namespace SafetyPresets
{
    internal class UI
    {
        public static void SetupUIX()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SafetyMenu).AddSimpleButton($"Load Custom Safety Settings", OpenLoadMenu);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SafetyMenu).AddSimpleButton($"Save Custom Safety Settings", OpenSaveMenu);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SafetyMenu).AddSimpleButton($"Delete Custom Safety Settings", OpenDeleteMenu);
        }

        public static void OpenLoadMenu()
        {
            var loadMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.QuickMenu3Columns);

            Settings.availablePresets.ActualPresets.OrderBy(sss => sss.settingsPresetNum).ToList().ForEach(sss => loadMenu.AddSimpleButton($"Load {sss.settingsPresetName}", () =>
            {
                Helpers.LoadSafetySettings(sss.settingsPresetNum);
                loadMenu.Hide();
            }));

            loadMenu.AddSimpleButton("Close",()=>loadMenu.Hide());
            loadMenu.Show();
        }

        public static void OpenSaveMenu()
        {
            var saveMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.QuickMenu3Columns);
            Settings.availablePresets.ActualPresets.OrderBy(sss => sss.settingsPresetNum).ToList().ForEach(sss => saveMenu.AddSimpleButton($"Save {sss.settingsPresetName}", () =>
            {
                OpenSavePresetMenu(sss.settingsPresetNum);
            }));

            saveMenu.AddSimpleButton($"Save new",()=>OpenSavePresetMenu(Helpers.NextAvailablePresetNum()));
            saveMenu.AddSimpleButton("Close",()=>saveMenu.Hide());
            saveMenu.Show();
        }

        public static void OpenDeleteMenu()
        {
            var deleteMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.QuickMenu3Columns);
            Settings.availablePresets.ActualPresets.OrderBy(sss => sss.settingsPresetNum).ToList().ForEach(sss => deleteMenu.AddSimpleButton($"Delete {sss.settingsPresetName}", () =>
            {
                Helpers.DeleteSafetySettings(sss.settingsPresetNum);
                deleteMenu.Hide();
                OpenDeleteMenu();
            }));
            deleteMenu.AddSimpleButton("Close", () => deleteMenu.Hide());
            deleteMenu.Show();
        }
        public static void OpenSavePresetMenu(int presetNumber)
        {
            var savePresetMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);
            string defaultText = Helpers.GetPresetName(presetNumber);

            savePresetMenu.AddSimpleButton("Save this preset",()=>{savePresetMenu.Hide(); BuiltinUiUtils.ShowInputPopup("Enter a preset name",defaultText,InputField.InputType.Standard,false,"Save",(x, kc, txt)=>Helpers.SaveSafetySettings(presetNumber,x));});
            savePresetMenu.Show();
        }

    }

}
