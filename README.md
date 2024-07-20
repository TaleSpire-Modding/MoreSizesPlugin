# More Sizes Plugin
Giants aren't big enough? This mod allows for bigger minis.

## Install

Currently you need to either follow the build guide down below or use the R2ModMan. 

## Usage
The Radial menu to set size of a creature has now been expanded for more sizes.
If the mini has an aura attached, an intimidatory menu will allow you to select 
which specific aura to scale up/down.


## How to Compile / Modify

Open ```MoreSizesPlugin.sln``` in Visual Studio.

You will need to add references to:

```
* BepInEx.dll  (Download from the BepInEx project.)
* Bouncyrock.TaleSpire.Runtime (found in Steam\steamapps\common\TaleSpire\TaleSpire_Data\Managed)
* UnityEngine.dll
* UnityEngine.CoreModule.dll
* UnityEngine.InputLegacyModule.dll 
* UnityEngine.UI
* Unity.TextMeshPro
* RadialUI
* RPC Plugin
```

Build the project.

Browse to the newly created ```bin/Debug``` or ```bin/Release``` folders and copy the ```MoreSizesPlugin.dll``` to ```Steam\steamapps\common\TaleSpire\BepInEx\plugins```

## Changelog
- 2.3.1: Update/correct ReadMe
- 2.3.0: temp RPC Migration,
- 2.2.1: Fix dependencies
- 2.2.0: Upgrade to ADP
- 2.1.4: RadialUI deprecated feature update.
- 2.1.3: config update.
- 2.1.2: Util Update.
- 2.1.1: CyberPunk update release
- 2.1.0: HF Update 
- 2.0.0: Soft Dependency on EAR via stat messaging for Auras.
- 1.4.0: Can now set multiple custom sizes by updating config's array.
- 1.3.0: Update method to change size to now also include transformation and base.
- 1.2.0: Addressed issue from Radial UI Update.
- 1.1.0: Addressed fix for TS Build 7035408
- 1.0.1: added 0.75 for size.
- 1.0.0: Initial release
