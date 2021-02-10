# SafetyPresets
## Purpose
The purpose of this [MelonLoader](https://github.com/LavaGang/MelonLoader) VRChat mod is to allow the user to save and load custom safety settings presets

## Requirements
* [UIExpansionKit](https://github.com/knah/VRCMods/) - Menus + Settings

## Features
* Custom Safety Settings Loading / Saving (Currently 3 presets)
* Instance Type Based Safety Settings (Public, Friends, Private)

## Known Issues
* The preset choices for instance type based loading will not update until game restart

## Building
To build this mod, reference the following libraries from MelonLoader/Managed after assembly generation
* Assembly-CSharp.dll
* Assembly-CSharp-firstpass.dll
* Il2Cppmscorlib.dll
* UnhollowerBaseLib.dll
* UnhollowerRuntimeLib.dll
* UnityEngine.dll
* UnityEngine.CoreModule.dll
* UnityEngine.UI.dll
* VRCCore-Editor.dll
* VRCCore-Standalone.dll
* VRCSDK3.dll
* VRCSDKBase.dll

Additionally, reference the following libraries
* MelonLoader.dll (from MelonLoader base directory)
* UIExpansionKit.dll (Built/Obtainable from [knah's VRCMods repository](https://github.com/knah/VRCMods/))

Install the following package
* Newtonsoft.Json

Finally, build in your favourite IDE.
