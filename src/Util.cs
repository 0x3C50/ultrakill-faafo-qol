using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Sandbox;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ExtraCommands;

public static class Util
{
    [CanBeNull]
    public static HashSet<GameObject> GetAllEnemies()
    {
        GoreZone findObjectOfType = Object.FindObjectOfType<GoreZone>();
        if (findObjectOfType is null)
            // con.PrintLine("you don't seem to be in a scene where i can do that");
            return null;

        HashSet<GameObject> go = new();
        foreach (Component f in findObjectOfType.gameObject.GetComponentsInChildren<Component>())
            if (f.TryGetComponent(out EnemyIdentifier _))
                go.Add(f.gameObject);

        go.RemoveWhere(o => o.GetComponent<EnemyIdentifier>().dead);

        return go;
    }

    public static Vector3? ParseCoordinates(string x, string y, string z, Vector3 originPoint)
    {
        float lx = 0, ly = 0, lz = 0;
        float ox = 0, oy = 0, oz = 0;
        Rigidbody rigidbody = PlayerTracker.Instance.GetRigidbody();
        Vector3 playerPos = rigidbody.position;
        switch (x[0])
        {
            case '~':
                ox = playerPos.x;
                x = x.Substring(1);
                break;
            case '^':
                ox = originPoint.x;
                x = x.Substring(1);
                break;
        }

        switch (y[0])
        {
            case '~':
                oy = playerPos.y;
                y = y.Substring(1);
                break;
            case '^':
                oy = originPoint.y;
                y = y.Substring(1);
                break;
        }

        switch (z[0])
        {
            case '~':
                oz = playerPos.z;
                z = z.Substring(1);
                break;
            case '^':
                oz = originPoint.z;
                z = z.Substring(1);
                break;
        }

        if (x.Length != 0 && !float.TryParse(x, out lx)) return null;
        if (y.Length != 0 && !float.TryParse(y, out ly)) return null;
        if (z.Length != 0 && !float.TryParse(z, out lz)) return null;
        return new Vector3(ox + lx, oy + ly, oz + lz);
    }

    public static float DistanceTo(this Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt(
            Mathf.Pow(b.x - a.x, 2) +
            Mathf.Pow(b.y - a.y, 2) +
            Mathf.Pow(b.z - a.z, 2)
        );
    }

    public static T GetFieldValue<T>(this object obj, string name)
    {
        FieldInfo field = obj.GetType()
            .GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field?.GetValue(obj);
    }

    public static void CreateFloatRowLimited(this AlterMenuElements a, string name, float initialState, float min,
        float max, Action<float> callback)
    {
        GameObject gameObject = Object.Instantiate(a.GetFieldValue<GameObject>("floatRowTemplate"),
            a.GetFieldValue<Transform>("container"), false);
        gameObject.SetActive(true);
        gameObject.GetComponentInChildren<Text>().text = name;
        Slider componentInChildren = gameObject.GetComponentInChildren<Slider>();
        componentInChildren.minValue = min;
        componentInChildren.maxValue = max;
        componentInChildren.SetValueWithoutNotify(initialState);
        componentInChildren.onValueChanged.AddListener(value => callback(value));
        a.GetFieldValue<List<int>>("createdRows").Add(gameObject.GetInstanceID());
    }

    public static void CreateButtonRow(this AlterMenuElements a, string text, Action callback)
    {
        GameObject gameObject =
            Object.Instantiate(Plugin.ButtonTemplate, a.GetFieldValue<Transform>("container"), false);
        gameObject.SetActive(true);
        gameObject.GetComponentInChildren<Button>().onClick.AddListener(() => callback());
        gameObject.GetComponentInChildren<Text>().text = text;
        a.GetFieldValue<List<int>>("createdRows").Add(gameObject.GetInstanceID());
    }
}