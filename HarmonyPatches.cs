using Harmony;
using MelonLoader;
using System.Linq;
using System.Reflection;
using VRC.Core;

namespace SafetyPresets
{
    class HarmonyPatches
    {
        public static void Patch(HarmonyInstance Harmony)
        {
            typeof(RoomManager).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name.StartsWith("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_"))
                .ToList().ForEach(m => Harmony.Patch(m, prefix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(HarmonyPatches.EnterWorldPrefix), BindingFlags.NonPublic | BindingFlags.Static))));
        }

        private static void EnterWorldPrefix(ApiWorldInstance __1)
        {
            if (__1 == null)
                return;

            SafetySaveManager.OnInstanceChange(__1.GetAccessType());
        }
    }
}
