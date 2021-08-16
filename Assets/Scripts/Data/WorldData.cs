using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldData
{
    public string worldName = "Prototype";
    public int seed;

    [System.NonSerialized]
    public Dictionary<Vector2Int, ChunkData> chunks = new Dictionary<Vector2Int, ChunkData>();
    [System.NonSerialized]
    public List<ChunkData> modifiedChunks = new List<ChunkData>();

    public WorldData(string nameValue, int seedValue)
    {
        worldName = nameValue;
        seed = seedValue;
    }

    public WorldData(WorldData wD)
    {
        worldName = wD.worldName;
        seed = wD.seed;
    }

    public void AddToModifiedChunkList(ChunkData chunk)
    {
        if (!modifiedChunks.Contains(chunk))
            modifiedChunks.Add(chunk);
    }

    public ChunkData RequestChunk(Vector2Int coord, bool create)
    {
        ChunkData c;

        //lock (World.Instance.ChunkListThreadLock)
        //{
            if (chunks.ContainsKey(coord))
                c = chunks[coord];
            else if (!create)
                c = null;
            else
            {
                LoadChunk(coord);
                c = chunks[coord];
            }
        //}

        return c;
    }

    internal void LoadChunk(Vector2Int coord)
    {
        if (chunks.ContainsKey(coord)) return;

        ChunkData chunk = SaveSystem.LoadChunk(worldName, coord);
        if (chunk != null)
        {
            chunks.Add(coord, chunk);
            return;
        }

        chunks.Add(coord, new ChunkData(coord));
        chunks[coord].Populate();
    }

    bool IsVoxelInWorld(Vector3 pos)
        => pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels &&
           pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
           pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels;

    public void SetVoxel(Vector3 pos, byte value, int diretion)
    {
        if (!IsVoxelInWorld(pos)) return;

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        ChunkData chunk = RequestChunk(new Vector2Int(x, z), true);

        Vector3Int voxel = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        chunk.ModifyVoxel(voxel, value, diretion);
    }

    public VoxelState GetVoxel(Vector3 pos)
    {
        if (!IsVoxelInWorld(pos)) return null;

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        ChunkData chunk = RequestChunk(new Vector2Int(x, z), false);

        if (chunk == null) return null;

        Vector3Int voxel = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        return chunk.map[voxel.x, voxel.y, voxel.z];
    }
}
