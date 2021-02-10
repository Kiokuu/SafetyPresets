using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
namespace SafetyPresets
{
    internal class UI
    {
        public static void SetupUIX()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SafetyMenu).AddSimpleButton($"Load Custom Safety Settings", OpenLoadMenu);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SafetyMenu).AddSimpleButton($"Save Custom Safety Settings", OpenSaveMenu);
        }

        public static void OpenLoadMenu()
        {

            var loadMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.QuickMenu3Columns);

            loadMenu.AddSimpleButton($"Load {Helpers.GetPresetName(1)}",()=>{Helpers.LoadSafetySettings(1);loadMenu.Hide();});
            loadMenu.AddSimpleButton($"Load {Helpers.GetPresetName(2)}",()=>{Helpers.LoadSafetySettings(2);loadMenu.Hide();});
            loadMenu.AddSimpleButton($"Load {Helpers.GetPresetName(3)}",()=>{Helpers.LoadSafetySettings(3);loadMenu.Hide();});

            loadMenu.AddSimpleButton("Close",()=>loadMenu.Hide());
            
            loadMenu.Show();
        }

        public static void OpenSaveMenu()
        {

            var saveMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.QuickMenu3Columns);

            saveMenu.AddSimpleButton($"Save {Helpers.GetPresetName(1)}",()=>OpenSavePresetMenu(1));
            saveMenu.AddSimpleButton($"Save {Helpers.GetPresetName(2)}",()=>OpenSavePresetMenu(2));
            saveMenu.AddSimpleButton($"Save {Helpers.GetPresetName(3)}",()=>OpenSavePresetMenu(3));

            saveMenu.AddSimpleButton("Close",()=>saveMenu.Hide());

            saveMenu.Show();
        }

        public static void OpenSavePresetMenu(int presetNumber)
        {
            var savePresetMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);
            string defaultText = Helpers.GetPresetName(presetNumber);
            savePresetMenu.AddSimpleButton("Save this preset",()=>{savePresetMenu.Hide(); UIExpansionKit.API.BuiltinUiUtils.ShowInputPopup("Enter a preset name",defaultText,InputField.InputType.Standard,false,"Save",(x, kc, txt)=>Helpers.SaveSafetySettings(presetNumber,x));});

            savePresetMenu.Show();
        }

    }

}
