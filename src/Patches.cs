using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ExtraCommands.unity;
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

    private static void CreateBossbarSection(Component prop, AlterMenuElements elementManager)
    {
        elementManager.CreateTitle("bossbar");
        BossbarPropHolder bossbarPropHolder = prop.GetComponent<BossbarPropHolder>();
        float initial = 0;
        if (bossbarPropHolder is not null)
        {
            initial = bossbarPropHolder.layers;
        }

        if (initial < 1) initial = 1;
        elementManager.CreateFloatRowLimited("Bossbars", initial, 1, 16, f =>
        {
            EnemyIdentifier eid = prop.GetComponentInChildren<EnemyIdentifier>();
            
            if (eid is null)
            {
                Plugin.BpxLogger.LogError("enemy identifier is null while changing bossbar amount, what the fuck?");
                return; // no bossbar exists or enemy is invalid, skip
            }

            BossbarPropHolder propHolder = prop.GetComponent<BossbarPropHolder>() ?? prop.gameObject.AddComponent<BossbarPropHolder>();

            propHolder.layers = (int)f;
        });
        elementManager.CreateButtonRow("Apply", () =>
        {
            EnemyIdentifier eid = prop.GetComponentInChildren<EnemyIdentifier>();
            
            if (eid is null)
            {
                Plugin.BpxLogger.LogError("enemy identifier doesn't exist while applying bossbar amount, what the shit?");
                return; // no bossbar exists or enemy is invalid, skip
            }

            BossbarPropHolder propHolder = prop.GetComponent<BossbarPropHolder>();
            int nOfBars = 1;
            if (propHolder is null)
            {
                Plugin.BpxLogger.LogWarning("IDIOT USER clicked apply without changing bossbar amount, using 1 as amount");
            }
            else
            {
                nOfBars = propHolder.layers;
            }

            BossHealthBar p = eid.gameObject.GetComponent<BossHealthBar>();

            if (p is not null) Object.Destroy(p);
            eid.ForceGetHealth();
            float hp = eid.health; // yes this could be less than the original health, there's no fix :/
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
            BossHealthBar bhb = eid.gameObject.AddComponent<BossHealthBar>();
            bhb.healthLayers = hl;
            prop.gameObject.SetActive(prev);
        });
    }

    private static void CreateBCSection(Component prop, AlterMenuElements elementManager)
    {
        EnemyIdentifier eid = prop.GetComponentInChildren<EnemyIdentifier>();
        if (eid is null) return; // no enemy
        elementManager.CreateTitle("blood");
        int initialColor = 0xFF0000;
        BspColorHolder bspColorHolder = eid.GetComponent<BspColorHolder>();
        if (bspColorHolder is not null) initialColor = bspColorHolder.bspColor;
        else
        {
            BspColorHolder addComponent = eid.gameObject.AddComponent<BspColorHolder>();
            addComponent.bspColor = 0xFF0000; // pure red
        }
        int iR = initialColor >> 16 & 0xFF;
        int iG = initialColor >> 8 & 0xFF;
        int iB = initialColor & 0xFF;
        elementManager.CreateFloatRowLimited("Red", iR, 0, 255, f =>
        {
            BspColorHolder ih = eid.GetComponent<BspColorHolder>();
            if (ih is null) return; // huh?
            int ic = ih.bspColor;
            int r = (int) f;
            int g = ic >> 8 & 0xFF;
            int b = ic & 0xFF;
            ih.bspColor = r << 16 | g << 8 | b;
        });
        elementManager.CreateFloatRowLimited("Green", iG, 0, 255, f =>
        {
            BspColorHolder ih = eid.GetComponent<BspColorHolder>();
            if (ih is null) return; // huh?
            int ic = ih.bspColor;
            int r = ic >> 16 & 0xFF;
            int g = (int) f;
            int b = ic & 0xFF;
            ih.bspColor = r << 16 | g << 8 | b;
        });
        elementManager.CreateFloatRowLimited("Blue", iB, 0, 255, f =>
        {
            BspColorHolder ih = eid.GetComponent<BspColorHolder>();
            if (ih is null) return; // huh?
            int ic = ih.bspColor;
            int r = ic >> 16 & 0xFF;
            int g = ic >> 8 & 0xFF;
            int b = (int) f;
            ih.bspColor = r << 16 | g << 8 | b;
        });
    }

    [HarmonyPatch(typeof(SandboxAlterMenu), "Show")]
    [HarmonyPostfix]
    private static void AddShit(SandboxAlterMenu __instance, SandboxSpawnableInstance prop, AlterMenuElements ___elementManager)
    {
        if (prop is not SandboxEnemy) return;
        CreateBossbarSection(prop, ___elementManager);
        CreateBCSection(prop, ___elementManager);
    }

    [HarmonyPatch(typeof(EnemyIdentifier), "DeliverDamage")]
    [HarmonyPrefix]
    private static void EnemyIdentifierHurtPrefix(EnemyIdentifier __instance)
    {
        BspColorHolder bsColorHolder = __instance.GetComponent<BspColorHolder>();
        if (bsColorHolder is not null) CustomBspColors.UpdateBspColors(bsColorHolder.bspColor);
    }

    [HarmonyPatch(typeof(EnemyIdentifier), "DeliverDamage")]
    [HarmonyPostfix]
    private static void EnemyIdentifierHurtPost(EnemyIdentifier __instance)
    {
        CustomBspColors.UpdateBspColors(0xFF0000); // default
    }
}