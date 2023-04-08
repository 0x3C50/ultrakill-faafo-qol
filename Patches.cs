using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using GameConsole;
using HarmonyLib;
using UnityEngine;

namespace ExtraCommands;

public class Patches
{
    [HarmonyPatch(typeof(Console), "Awake")]
    [HarmonyPostfix]
    private static void RegisterOurCommands(Console __instance)
    {
        __instance.RegisterCommands(Plugin.Commands);
    }

    [HarmonyPatch(typeof(NewMovement), "GetHurt")]
    [HarmonyPrefix]
    private static bool CancelDamage()
    {
        return Plugin.TakeDamage;
    }

    [HarmonyPatch(typeof(CheatsManager), "RebuildIcons")]
    [HarmonyPostfix]
    private static void AddCheatIcons(CheatsManager __instance, Dictionary<string, Sprite> ___spriteIcons)
    {
        foreach (KeyValuePair<string, Sprite> additionalCheatIcon in Plugin.AdditionalCheatIcons)
        {
            System.Console.WriteLine(additionalCheatIcon);
            ___spriteIcons.Add(additionalCheatIcon.Key, additionalCheatIcon.Value);
        }
    }

    [HarmonyPatch(typeof(CheatsManager), "Start")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> AddCheats(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction codeInstruction in instructions)
        {
            if (codeInstruction.opcode == OpCodes.Call && codeInstruction.operand is MethodInfo { Name: "RebuildMenu" })
                // consume our existing this on the stack and push it back for Rebuild to do its thing
                yield return Transpilers.EmitDelegate(delegate(CheatsManager c)
                {
                    foreach (ICheat cheat in Plugin.Cheats) c.RegisterExternalCheat(cheat);

                    return c;
                });
            yield return codeInstruction;
        }
    }
}