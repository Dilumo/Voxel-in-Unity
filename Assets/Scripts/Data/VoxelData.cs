using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    public static readonly int WorldSizeInChunks = 1000;

    // Lighting Values
    public static float minLightLevel = 0.25f;
    public static float maxLightLevel = 0.8f;

    public static int seed;

    public static float unitOfLight
    {
        get { return 1f / 16f; }
    }

    public static int WorldCenter
    {
        get
        {
            return (WorldSizeInChunks * ChunkWidth) / 2;
        }
    }

    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * ChunkWidth; }
    }

    public static readonly int textureAtlasSizeInBlock = 16;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)textureAtlasSizeInBlock; }
    }

    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3 (0, 0, 0), // 0
        new Vector3 (1, 0, 0), // 1
        new Vector3 (1, 1, 0), // 2
        new Vector3 (0, 1, 0), // 3
        new Vector3 (0, 0, 1), // 4
        new Vector3 (1, 0, 1), // 5
        new Vector3 (1, 1, 1), // 6
        new Vector3 (0, 1, 1) // 7
    };

    public static readonly Vector3Int[] facesCheck = new Vector3Int[6]
    {
        new Vector3Int (0, 0, -1),
        new Vector3Int (0, 0, 1),
        new Vector3Int (0, 1, 0),
        new Vector3Int (0, -1, 0),
        new Vector3Int (-1, 0, 0),
        new Vector3Int (1, 0, 0)
    };

    public static readonly int[] revFaceCheckIndex = new int[6] { 1, 0, 3, 2, 5, 4 };

    public static readonly int[,] voxelTris = new int[6, 4]
    { 
        { 0, 3, 1, 2 }, // back face
        { 5, 6, 4, 7 }, // front face
        { 3, 7, 2, 6 }, // top face
        { 1, 5, 0, 4 }, // bottom face
        { 4, 7, 0, 3 }, // left face
        { 1, 2, 5, 6 } // right face
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)
    };
}