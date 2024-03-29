using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> GenerateMajorFlora(int index, Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        return index switch
        {
            0 => MakeTree(position, minTrunkHeight, maxTrunkHeight),
            1 => MakeCacti(position, minTrunkHeight, maxTrunkHeight),
            _ => new Queue<VoxelMod>()
        };
    }

    private static Queue<VoxelMod> MakeTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        var queue = new Queue<VoxelMod>();
        var height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250f, 3f));
        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (var i = 1; i < height; i++)
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 6));

        for (var x = -3; x < 4; x++)
            for (var y = 0; y < 7; y++)
                for (var z = -3; z < 4; z++)
                    queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 11));

        return queue;
    }

    private static Queue<VoxelMod> MakeCacti(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        var queue = new Queue<VoxelMod>();
        var height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 23456f, 2f));
        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (var i = 1; i <= height; i++)
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 12));


        return queue;
    }
}
