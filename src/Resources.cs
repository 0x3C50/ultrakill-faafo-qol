using System;
using System.IO;
using UnityEngine;

namespace ExtraCommands;

public static class Resources
{
    private static byte[] S2BArr(Stream st)
    {
        using MemoryStream ms = new();
        st.CopyTo(ms);
        return ms.ToArray();
    }

    public static Sprite LoadSpriteFrom(string assetName)
    {
        byte[] b = S2BArr(LoadResource(assetName));
        Texture2D tex = new(1, 1);
        tex.LoadImage(b);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2f, tex.height / 2f));
    }

    public static Stream LoadResource(string n)
    {
        Type rootType = typeof(Plugin);
        Stream manifestResourceStream =
            rootType.Assembly.GetManifestResourceStream($"ExtraCommands.resources.{n}");
        return manifestResourceStream;
    }
}