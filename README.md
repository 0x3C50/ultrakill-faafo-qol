# ULTRAKILL "Fuck around and find out" QOL
Adds a variety of utilities for "fuck around and find out"-minded ultrakill users. Things like `teleport`, `movementSpeed`, etc.

## Downloads
[here](https://thunderstore.io/c/ultrakill/p/0x150_mods/FAAFO_QOL/)

## Building yourself
**FIRST OFF**: This repository does **NOT** have all required libraries bundled. I explicitly left out ULTRAKILL's Assembly-CSharp.dll, since it may infringe on hakita's copyright on it. If you want to build this mod, you will have to get the assembly yourself.

1. Get the Assembly-CSharp.dll file from `<ultrakill steam directory>/ULTRAKILL_Data/Managed/Assembly-CSharp.dll`
2. Copy it into `<project root>/libs/Assembly-CSharp.dll`, create the `libs` folder if it doesn't exist

After this, you can continue with the regular build process:
1. `dotnet build`
2. Output will be in `bin/Debug/netstandard2.1/FAAFO-QOL.dll`

## Features
### Cheats
#### Invincibility
Temporary grants full invincibility against all damage sources without having to enable major assists and remember to disable them

### Commands
#### `sv_cheats`
Quickly allows you to enable cheats without having to do the konami code and pressing "Yes"

#### `movementSpeed`
Allows you to modify certain settings in the movement controller, like movement speed, jump power, etc

#### `teleport`
Enables you to teleport yourself to other locations or other enemies to you.

Notes:
- Enemies within an encounter may be buggy to teleport, because some of them appear to have a set region where they may be positioned. They get reset once they leave that area
  - Enemies outside of encounters (or teleporting enemies within their encounter zone) work fine.

## Suggestions
I am looking to improve this mod as far as I can get it, since this is my first actual, "useful" mod for the game. If you find any bugs or have suggestions for more features in the future, please open an issue and tell me about it.