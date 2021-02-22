using MelonLoader;

[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonInfo(typeof(SafetyPresets.SafetyPresetsMod), "Safety-Presets", "0.0.3", "死神#6938", "https://github.com/Kiokuu/SafetyPresets")]

namespace SafetyPresets
{
    public class SafetyPresetsMod : MelonMod
    {
        public override void OnApplicationStart()
        {
            HarmonyPatches.Patch(Harmony);

            SafetySaveManager.Initialize();
            SafetyPreferences.SetupPrefs();
        }
    }
}
