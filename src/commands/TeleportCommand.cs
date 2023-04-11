using System.Collections.Generic;
using GameConsole;
using UnityEngine;
using UnityEngine.AI;

namespace ExtraCommands.commands;

public class TeleportCommand : CommandRoot
{
    public override string Name => "teleport";
    public override string Description => "Teleports all enemies to some coordinate";

    protected override void BuildTree(Console con)
    {
        Branch("enemies", () =>
        {
            Leaf("near", delegate(float distance, string x, string y, string z)
            {
                HashSet<GameObject> gameObjects = Util.GetAllEnemies();
                if (gameObjects is null)
                {
                    con.PrintLine("No GZ in scene, are you sure you can spawn enemies here?");
                    return;
                }

                Vector3 ppos = PlayerTracker.Instance.GetRigidbody().position;
                gameObjects.RemoveWhere(o => o.transform.position.DistanceTo(ppos) > distance);

                if (gameObjects.Count == 0)
                {
                    con.PrintLine("No enemies in distance");
                    return;
                }

                foreach (GameObject gameObject in gameObjects)
                {
                    Vector3? coordinates = Util.ParseCoordinates(x, y, z, gameObject.transform.position);
                    if (!coordinates.HasValue)
                    {
                        PrintCoordHelp(con);
                        return;
                    }

                    Vector3 actual = coordinates.Value;
                    con.PrintLine(gameObject + " -> " + coordinates);
                    TeleportObj(gameObject, actual);
                    // gameObject.transform.position.Set(actual.x, actual.y, actual.z);
                }
            });
            Leaf("all", delegate(string x, string y, string z)
            {
                HashSet<GameObject> gameObjects = Util.GetAllEnemies();
                if (gameObjects is null)
                {
                    con.PrintLine("No GZ in scene, are you sure you can spawn enemies here?");
                    return;
                }

                if (gameObjects.Count == 0)
                {
                    con.PrintLine("No enemies to teleport");
                    return;
                }


                foreach (GameObject gameObject in gameObjects)
                {
                    Vector3? coordinates = Util.ParseCoordinates(x, y, z, gameObject.transform.position);
                    if (!coordinates.HasValue)
                    {
                        PrintCoordHelp(con);
                        return;
                    }

                    Vector3 actual = coordinates.Value;
                    con.PrintLine(gameObject + " -> " + coordinates);
                    TeleportObj(gameObject, actual);
                    // gameObject.transform.SetPositionAndRotation(actual, gameObject.transform.rotation);
                    // gameObject.transform.position.Set(actual.x, actual.y, actual.z);
                }
            });
        });
        Leaf("self", delegate(string x, string y, string z)
        {
            Rigidbody transform = PlayerTracker.Instance.GetRigidbody();
            Vector3? coordinates = Util.ParseCoordinates(x, y, z, transform.position);
            if (!coordinates.HasValue)
            {
                con.PrintLine("Coordinates are invalid, syntax: [\\^~]?[0-9,]+(\\.[0-9]+)?");
                con.PrintLine("Prefix ~ refers to your current position");
                con.PrintLine("Prefix ^ refers to the target's current position (yourself)");
                return;
            }

            transform.MovePosition(coordinates.Value);
            //transform.SetPositionAndRotation(coordinates.Value, transform.rotation);
        });
    }

    private static void TeleportObj(GameObject go, Vector3 to)
    {
        go.transform.SetPositionAndRotation(to, go.transform.rotation);
        NavMeshAgent[] nmas = go.GetComponentsInChildren<NavMeshAgent>();
        foreach (NavMeshAgent navMeshAgent in nmas) navMeshAgent.Warp(to);
    }

    private static void PrintCoordHelp(Console con)
    {
        con.PrintLine("Coordinates are invalid, syntax: [\\^~]?[0-9,]+(\\.[0-9]+)?");
        con.PrintLine("Prefix ~ refers to your current position");
        con.PrintLine("Prefix ^ refers to the enemy's current position");
    }
}