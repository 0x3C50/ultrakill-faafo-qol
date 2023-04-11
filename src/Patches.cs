using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ExtraCommands.extra;
using ExtraCommands.unity;
using GameConsole;
using HarmonyLib;
using Newtonsoft.Json;
using Sandbox;
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

    private static void CreateBossbarSection(Component prop, AlterMenuElements elementManager)
    {
        elementManager.CreateTitle("bossbar");
        BossbarPropHolder bossbarPropHolder = prop.GetComponent<BossbarPropHolder>();
        float initial = 0;
        if (bossbarPropHolder is not null) initial = bossbarPropHolder.layers;

        if (initial < 1) initial = 1;
        elementManager.CreateFloatRowLimited("Bossbars", initial, 1, 16, f =>
        {
            EnemyIdentifier eid = prop.GetComponentInChildren<EnemyIdentifier>();

            if (eid is null)
            {
                Plugin.BpxLogger.LogError("enemy identifier is null while changing bossbar amount, what the fuck?");
                return; // no bossbar exists or enemy is invalid, skip
            }

            BossbarPropHolder propHolder = prop.GetComponent<BossbarPropHolder>() ??
                                           prop.gameObject.AddComponent<BossbarPropHolder>();

            propHolder.layers = (int)f;
        });
        elementManager.CreateButtonRow("Apply", () =>
        {
            EnemyIdentifier eid = prop.GetComponentInChildren<EnemyIdentifier>();

            if (eid is null)
            {
                Plugin.BpxLogger.LogError(
                    "enemy identifier doesn't exist while applying bossbar amount, what the shit?");
                return; // no bossbar exists or enemy is invalid, skip
            }

            BossbarPropHolder propHolder = prop.GetComponent<BossbarPropHolder>();
            int nOfBars = 1;
            if (propHolder is null)
                Plugin.BpxLogger.LogWarning(
                    "IDIOT USER clicked apply without changing bossbar amount, using 1 as amount");
            else
                nOfBars = propHolder.layers;

            BossHealthBar p = eid.gameObject.GetComponent<BossHealthBar>();

            if (p is not null) Object.Destroy(p);
            eid.ForceGetHealth();
            float hp = eid.health; // yes this could be less than the original health, there's no fix :/
            float oneBar = hp / nOfBars;
            BossBarManager.HealthLayer[] hl = new BossBarManager.HealthLayer[nOfBars];
            for (int i = 0; i < nOfBars; i++)
                hl[i] = new BossBarManager.HealthLayer
                {
                    health = oneBar
                };

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
        if (bspColorHolder is not null)
        {
            initialColor = bspColorHolder.bspColor;
        }
        else
        {
            BspColorHolder addComponent = eid.gameObject.AddComponent<BspColorHolder>();
            addComponent.bspColor = 0xFF0000; // pure red
        }

        int iR = (initialColor >> 16) & 0xFF;
        int iG = (initialColor >> 8) & 0xFF;
        int iB = initialColor & 0xFF;
        elementManager.CreateFloatRowLimited("Red", iR, 0, 255, f =>
        {
            BspColorHolder ih = eid.GetComponent<BspColorHolder>();
            if (ih is null) return; // huh?
            int ic = ih.bspColor;
            int r = (int)f;
            int g = (ic >> 8) & 0xFF;
            int b = ic & 0xFF;
            ih.bspColor = (r << 16) | (g << 8) | b;
        });
        elementManager.CreateFloatRowLimited("Green", iG, 0, 255, f =>
        {
            BspColorHolder ih = eid.GetComponent<BspColorHolder>();
            if (ih is null) return; // huh?
            int ic = ih.bspColor;
            int r = (ic >> 16) & 0xFF;
            int g = (int)f;
            int b = ic & 0xFF;
            ih.bspColor = (r << 16) | (g << 8) | b;
        });
        elementManager.CreateFloatRowLimited("Blue", iB, 0, 255, f =>
        {
            BspColorHolder ih = eid.GetComponent<BspColorHolder>();
            if (ih is null) return; // huh?
            int ic = ih.bspColor;
            int r = (ic >> 16) & 0xFF;
            int g = (ic >> 8) & 0xFF;
            int b = (int)f;
            ih.bspColor = (r << 16) | (g << 8) | b;
        });
    }

    [HarmonyPatch(typeof(SandboxAlterMenu), "Show")]
    [HarmonyPostfix]
    private static void AddShit(SandboxAlterMenu __instance, SandboxSpawnableInstance prop,
        AlterMenuElements ___elementManager)
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

    [HarmonyPatch(typeof(SandboxEnemy), "SaveEnemy")]
    [HarmonyPrefix]
    private static bool ReplaceTheShit(SandboxEnemy __instance, ref SavedEnemy __result)
    {
        if (!__instance.enemyId || __instance.enemyId.health < 0.0 || __instance.enemyId.dead)
        {
            __result = null;
            return false;
        }

        int bloodCol = 0xFF0000;
        int bbAm = 1;
        BossbarPropHolder bossbarPropHolder = __instance.GetComponent<BossbarPropHolder>();
        BspColorHolder bspColorHolder = __instance.enemyId.GetComponent<BspColorHolder>();
        if (bossbarPropHolder is not null) bbAm = bossbarPropHolder.layers;
        if (bspColorHolder is not null) bloodCol = bspColorHolder.bspColor;
        ExtendedSavedEnemy savedEnemy = new()
        {
            Radiance = __instance.radiance,
            Bossbars = bbAm,
            BloodColor = bloodCol
        };
        SavedGeneric saveObject = savedEnemy;
        __instance.BaseSave(ref saveObject);
        __result = savedEnemy;
        return false;
    }

    [HarmonyPatch(typeof(SandboxSaver), "RecreateEnemy")]
    [HarmonyTranspiler]
    [HarmonyDebug]
    private static IEnumerable<CodeInstruction> LoadCustomData(IEnumerable<CodeInstruction> i)
    {
        foreach (CodeInstruction codeInstruction in i)
        {
            yield return codeInstruction;
            if (codeInstruction.opcode != OpCodes.Call ||
                codeInstruction.operand is not MethodInfo { Name: "ApplyData" }) continue;
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldloc_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return Transpilers.EmitDelegate((SandboxSaver sv, GameObject prop, SavedGeneric sg) =>
            {
                ExtendedSavedEnemy ese = (ExtendedSavedEnemy)sg;
                prop.AddComponent<BspColorHolder>().bspColor = ese.BloodColor;
                if (ese.Bossbars != 1)
                {
                    EnemyIdentifier eid = prop.GetComponentInChildren<EnemyIdentifier>();

                    if (eid is null)
                    {
                        Plugin.BpxLogger.LogError(
                            "enemy identifier doesn't exist while applying bossbar amount, what the shit?");
                        return; // no bossbar exists or enemy is invalid, skip
                    }

                    int nOfBars = ese.Bossbars;

                    BossHealthBar p = eid.gameObject.GetComponent<BossHealthBar>();

                    if (p is not null) Object.Destroy(p);
                    eid.ForceGetHealth();
                    float hp = eid.health; // yes this could be less than the original health, there's no fix :/
                    float oneBar = hp / nOfBars;
                    BossBarManager.HealthLayer[] hl = new BossBarManager.HealthLayer[nOfBars];
                    for (int i = 0; i < nOfBars; i++)
                        hl[i] = new BossBarManager.HealthLayer
                        {
                            health = oneBar
                        };

                    bool prev = prop.gameObject.activeSelf;
                    prop.gameObject.SetActive(false); // shitty hack to postpone BossHealthBar#Awake
                    BossHealthBar bhb = eid.gameObject.AddComponent<BossHealthBar>();
                    bhb.healthLayers = hl;
                    prop.gameObject.SetActive(prev);
                }
            });
        }
    }

    [HarmonyPatch(typeof(SandboxSaver), "Load")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> WidenSBLoad(IEnumerable<CodeInstruction> i)
    {
        // this took me an actual ungodly amount of time to figure out
        foreach (CodeInstruction codeInstruction in i)
        {
            if (codeInstruction.opcode == OpCodes.Call && codeInstruction.operand is MethodInfo
                {
                    Name: "DeserializeObject"
                })
            {
                MethodInfo methodInfos = typeof(JsonConvert).GetMethods().First(info =>
                    info.Name == "DeserializeObject" && info.GetParameters().Length == 1 &&
                    info.GetParameters()[0].ParameterType == typeof(string) && info.ReturnType != typeof(object));
                MethodInfo methodInfo = methodInfos.MakeGenericMethod(typeof(ExtendedSandboxSaveData));
                codeInstruction.operand = methodInfo;
            }

            yield return codeInstruction;
        }
    }
}