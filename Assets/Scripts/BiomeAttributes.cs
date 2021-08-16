using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    [Header("Values")]
    public string biomeName;
    public int offset;
    public float scale;

    public int terrainHeight;
    public float terrainScale;
    public byte surfaceBlock;
    public byte subSurfaceBlock;

    [Header("Major Flora")]
    public int majorFloraIndex;
    public float majorFloraZoneScale = 1.3f;
    [Range(0.1f,1f)]
    public float majorFloraZoneThreshold = 0.6f;
    public float majorFloraPlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float majorFloraPlacementThreshold = 0.8f;

    public int maxHeight = 12;
    public int minHeight = 5;

    public bool placeMajorFlora = true;

    public Lode[] lodes;

}
