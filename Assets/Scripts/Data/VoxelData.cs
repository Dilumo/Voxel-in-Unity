using UnityEngine;

namespace Data
{
    public static class VoxelData
    {
        public const int ChunkWidth = 16;
        public const int ChunkHeight = 128;
        public const int WorldSizeInChunks = 1000;

        // Lighting Values
        public const float MinLightLevel = 0.25f;
        public const float MaxLightLevel = 0.8f;

        public static int seed;

        public static float UnitOfLight => 1f / 16f;

        public static int WorldCenter => (WorldSizeInChunks * ChunkWidth) / 2;

        public static int WorldSizeInVoxels => WorldSizeInChunks * ChunkWidth;

        public const int TextureAtlasSizeInBlock = 16;
        public static float NormalizedBlockTextureSize => 1f / (float)TextureAtlasSizeInBlock;

        public static readonly Vector3[] VoxelVerts = new Vector3[8]
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

        public static readonly Vector3Int[] FacesCheck = new Vector3Int[6]
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
}