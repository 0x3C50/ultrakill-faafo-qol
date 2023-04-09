using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Sandbox;
using UnityEngine;
using Console = GameConsole.Console;
using Object = UnityEngine.Object;

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

    [HarmonyPatch(typeof(SandboxAlterMenu), "Show")]
    [HarmonyPostfix]
    private static void AddShit(SandboxAlterMenu __instance, SandboxSpawnableInstance prop, AlterMenuElements ___elementManager)
    {
        if (prop is not SandboxEnemy) return;
        ___elementManager.CreateTitle("bossbar");
        BossHealthBar bb = prop.GetComponent<BossHealthBar>();
        float initial = 0;
        if (bb is not null)
        {
            initial = bb.healthLayers.Length;
        }

        if (initial < 1) initial = 1;
        ___elementManager.CreateFloatRowLimited("Bossbars", initial, 0, 16, f =>
        {
            EnemyIdentifier eid = prop.GetComponentInChildren<EnemyIdentifier>();
            BossHealthBar p = prop.GetComponent<BossHealthBar>();
            if (p is null || eid is null) return; // no bossbar exists or enemy is invalid, skip 
            Object.Destroy(p);
            eid.ForceGetHealth();
            float hp = eid.health; // yes this could be less than the original health, there's no fix :/
            int nOfBars = (int) Math.Floor(f);
            float oneBar = hp / nOfBars;
            BossBarManager.HealthLayer[] hl = new BossBarManager.HealthLayer[nOfBars];
            for (int i = 0; i < nOfBars; i++)
            {
                hl[i] = new BossBarManager.HealthLayer {
                    health = oneBar
                };
            }

            bool prev = prop.gameObject.activeSelf;
            prop.gameObject.SetActive(false); // shitty hack to postpone BossHealthBar#Awake
            BossHealthBar bhb = prop.gameObject.AddComponent<BossHealthBar>();
            bhb.healthLayers = hl;
            prop.gameObject.SetActive(prev);
        });
    }
}