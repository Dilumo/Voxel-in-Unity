using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    int x;
    int y;
    public Vector2Int position
    {
        get { return new Vector2Int(x, y); }
        set
        {
            x = value.x;
            y = value.y;
        }
    }
    [System.NonSerialized] public Chunk chunk;
    [HideInInspector]
    public VoxelState[,,] map = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    public ChunkData(Vector2Int pos) => position = pos;
    public ChunkData(int xValue, int yValue)
    {
        x = xValue;
        y = yValue;
    }

    public void Populate()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    Vector3 voxelGlobalPos = new Vector3(x + position.x, y, z + position.y);
                    map[x, y, z] = new VoxelState(World.Instance.GetVoxel(voxelGlobalPos), this, new Vector3Int(x, y, z));
                    for (int f = 0; f < 6; f++)
                    {
                        Vector3Int neighbourV3 = new Vector3Int(x, y, z) + VoxelData.facesCheck[f];
                        if (IsVoxelInChunk(neighbourV3))
                            map[x, y, z].neighbours[f] = VoxelFromV3Int(neighbourV3);
                        else
                            map[x, y, z].neighbours[f] = World.Instance.worldData.GetVoxel(voxelGlobalPos + VoxelData.facesCheck[f]);
                    }
                }

        Lighting.RecalculateNaturalLight(this);
        World.Instance.worldData.AddToModifiedChunkList(this);
    }

    public void ModifyVoxel(Vector3Int pos, byte idValue, int direction)
    {
        if (map[pos.x, pos.y, pos.z].id == idValue) return;

        VoxelState voxel = map[pos.x, pos.y, pos.z];
        //BlockType newVoxel = World.Instance.blockTypes[idValue];
        byte oldOpacity = voxel.properties.opacity;

        voxel.id = idValue;
        voxel.orientation = direction;
        if (voxel.properties.opacity != oldOpacity &&
            (pos.y == VoxelData.ChunkHeight - 1 ||
            map[pos.x, pos.y + 1, pos.z].light == 15))
            Lighting.CastNaturalLight(this,pos.x, pos.z, pos.y + 1);

        World.Instance.worldData.AddToModifiedChunkList(this);

        if (chunk != null)
            World.Instance.AddChunkToUpdate(chunk);
    }

    public bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;
        else
            return true;
    }

    public bool IsVoxelInChunk(Vector3Int pos)
        => IsVoxelInChunk(pos.x, pos.y, pos.z);

    public VoxelState VoxelFromV3Int(Vector3Int pos)
    => map[pos.x, pos.y, pos.z];
}
