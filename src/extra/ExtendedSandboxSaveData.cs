using UnityEngine;

namespace ExtraCommands.extra;

public class ExtendedSavedEnemy : SavedEnemy
{
    public int BloodColor;
    public int Bossbars;
}

public class ExtendedSandboxSaveData
{
    public SavedBlock[] Blocks;
    public ExtendedSavedEnemy[] Enemies;
    public string GameVersion = Application.version;
    public string MapIdentifier;
    public string MapName;
    public SavedProp[] Props;
    public int SaveVersion = 2;
}