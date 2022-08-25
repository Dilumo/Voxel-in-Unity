using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

public static class Lighting
{

    public static void RecalculateNaturalLight(ChunkData chunkData)
    {
        for (var x = 0; x < VoxelData.ChunkWidth; x++)
            for (var z = 0; z < VoxelData.ChunkWidth; z++)
                CastNaturalLight(chunkData, x, z, VoxelData.ChunkHeight - 1);
    }

    public static void CastNaturalLight(ChunkData chunkData, int x, int z, int startY)
    {
        if(startY > VoxelData.ChunkHeight -1)
        {
            startY = VoxelData.ChunkHeight - 1;
            //Debug.Log("<color=yellow>Attempted to cast natural light from above world.</color>");
        }

        var obstructed = false;

        for (var y = startY; y > -1; y--)
        {
            var voxel = chunkData.map[x, y, z];

            if (obstructed)
                voxel.Light = 0;
            else if (voxel.Properties.opacity > 0)
            {
                voxel.Light = 0;
                obstructed = true;
            }
            else
                voxel.Light = 15;

        }
    }
}
