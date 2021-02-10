using MelonLoader;
using System.Linq;

namespace SafetyPresets
{
    public static class CompatChecks
    {
        public static void CheckEnvironment()
        {
            if(!MelonHandler.Mods.Any(mod=>mod.Info.Name=="UI Expansion Kit")){
                MelonLogger.Error("Failed to detect UIExpansionKit - This mod will lose its main functionality.");
            }
        }
    }
}
