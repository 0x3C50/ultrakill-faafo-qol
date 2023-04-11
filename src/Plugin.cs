using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using ExtraCommands.cheats;
using ExtraCommands.commands;
using GameConsole;
using HarmonyLib;
using MonoMod.Utils;
using UnityEngine;

namespace ExtraCommands;

[BepInPlugin("faafoqol", "FAAFO QOL", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static bool TakeDamage = false;

    public static readonly ICommand[] Commands =
    {
        new TeleportCommand(),
        new CheatsCommand(),
        new MovementSpeed()
    };

    public static readonly ICheat[] Cheats =
    {
        new InvincibilityCheat()
    };

    public static readonly Dictionary<string, Sprite> AdditionalCheatIcons = new();
    private static readonly AssetBundle MainBundle = AssetBundle.LoadFromStream(Resources.LoadResource("mainbundle"));
    public static readonly GameObject ButtonTemplate = MainBundle.LoadAsset<GameObject>("ButtonTemplate");
    public static ManualLogSource BpxLogger;

    private void Awake()
    {
        FileLog.logPath = "/home/x150/Downloads/flog.txt";
        BpxLogger = Logger;
        Logger.LogInfo("Doing mischief...");
        AdditionalCheatIcons.Clear();
        try
        {
            AdditionalCheatIcons.Add("main", Resources.LoadSpriteFrom("inv_cheat_icon.png"));

            Harmony.CreateAndPatchAll(typeof(Patches));
            Logger.LogInfo("Did the mischief!!");
        }
        catch (Exception ex)
        {
            Logger.LogError("FUCK");
            ex.LogDetailed();
        }
    }
}