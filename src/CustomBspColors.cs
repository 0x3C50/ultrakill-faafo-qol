using System.Collections.Generic;
using UnityEngine;

namespace ExtraCommands;

public class CustomBspColors
{
    private static void UpdateBspColorSingle(GameObject go, Color cv)
    {
        foreach (ParticleSystem ps in go.GetComponentsInChildren<ParticleSystem>())
        {
            // deprecation, not doing this doesn't work
#pragma warning disable CS0618
            ps.startColor = cv;
#pragma warning restore CS0618
        }

        foreach (ParticleSystemRenderer particleSystemRenderer in
                 go.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            Material trailMaterial = particleSystemRenderer.trailMaterial;
            if (trailMaterial is not null) trailMaterial.color = Color.white;
        }
    }

    public static void UpdateBspColors(int c)
    {
        // This is probably THE shittiest solution you can come up with
        float r = ((c >> 16) & 0xFF) / 255f;
        float g = ((c >> 8) & 0xFF) / 255f;
        float b = (c & 0xFF) / 255f;
        Color cv = new(r, g, b);
        BloodsplatterManager bsm = BloodsplatterManager.Instance;
        UpdateBspColorSingle(bsm.head, cv);
        UpdateBspColorSingle(bsm.limb, cv);
        UpdateBspColorSingle(bsm.body, cv);
        UpdateBspColorSingle(bsm.small, cv);
        UpdateBspColorSingle(bsm.smallest, cv);
        UpdateBspColorSingle(bsm.splatter, cv);
        UpdateBspColorSingle(bsm.underwater, cv);
        UpdateBspColorSingle(bsm.brainChunk, cv);
        UpdateBspColorSingle(bsm.skullChunk, cv);
        UpdateBspColorSingle(bsm.eyeball, cv);
        UpdateBspColorSingle(bsm.jawChunk, cv);
        // ParticleSystem.MinMaxGradient msp = BloodsplatterManager.Instance.small.GetComponent<ParticleSystem>().main.startColor;
        // msp.colorMin = msp.colorMax = msp.color = cv;
        foreach (GameObject gameObject in bsm.GetFieldValue<List<GameObject>>("heads"))
            UpdateBspColorSingle(gameObject, cv);
        foreach (GameObject gameObject in bsm.GetFieldValue<List<GameObject>>("limbs"))
            UpdateBspColorSingle(gameObject, cv);
        foreach (GameObject gameObject in bsm.GetFieldValue<List<GameObject>>("bodies"))
            UpdateBspColorSingle(gameObject, cv);
        foreach (GameObject gameObject in bsm.GetFieldValue<List<GameObject>>("smalls"))
            UpdateBspColorSingle(gameObject, cv);
        foreach (GameObject gameObject in bsm.GetFieldValue<List<GameObject>>("smallests"))
            UpdateBspColorSingle(gameObject, cv);
        foreach (GameObject gameObject in bsm.GetFieldValue<List<GameObject>>("splatters"))
            UpdateBspColorSingle(gameObject, cv);
        foreach (GameObject gameObject in bsm.GetFieldValue<List<GameObject>>("underwaters"))
            UpdateBspColorSingle(gameObject, cv);
        foreach (GameObject gameObject in bsm.GetFieldValue<List<GameObject>>("brainChunks"))
            UpdateBspColorSingle(gameObject, cv);
        foreach (GameObject gameObject in bsm.GetFieldValue<List<GameObject>>("skullChunks"))
            UpdateBspColorSingle(gameObject, cv);
        foreach (GameObject gameObject in bsm.GetFieldValue<List<GameObject>>("eyeballs"))
            UpdateBspColorSingle(gameObject, cv);
        foreach (GameObject gameObject in bsm.GetFieldValue<List<GameObject>>("jawChunks"))
            UpdateBspColorSingle(gameObject, cv);
    }
}