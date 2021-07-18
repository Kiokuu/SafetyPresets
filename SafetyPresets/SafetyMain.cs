using MelonLoader;

namespace SafetyPresets
{
    internal class Main : MelonMod
    {
        public override void OnApplicationStart()
        {
            Settings.SetupDefaults();
            Prefs.SetupPrefs();
            Helpers.DoXrefMagic();
            CompatChecks.CheckEnvironment();
            UI.SetupUIX();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex==-1)
            {
                MelonCoroutines.Start(Helpers.ChangeOnInstance());
            }
        }
    }
}
