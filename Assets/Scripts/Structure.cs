using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> GenerateMajorFlora(int index, Vector3 postion, int minTrunkHeight, int maxTrunkHeight)
    {
        switch (index)
        {
            case 0:
                return MakeTree(postion,minTrunkHeight,maxTrunkHeight);
            case 1:
                return MakeCacti(postion,minTrunkHeight,maxTrunkHeight);
            default:
                return new Queue<VoxelMod>();
        }
    }
    public static Queue<VoxelMod> MakeTree(Vector3 postion, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(postion.x, postion.z), 250f, 3f));
        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
            queue.Enqueue(new VoxelMod(new Vector3(postion.x, postion.y + i, postion.z), 6));

        for (int x = -3; x < 4; x++)
            for (int y = 0; y < 7; y++)
                for (int z = -3; z < 4; z++)
                    queue.Enqueue(new VoxelMod(new Vector3(postion.x + x, postion.y + height + y, postion.z + z), 11));

        return queue;
    }

    public static Queue<VoxelMod> MakeCacti(Vector3 postion, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(postion.x, postion.z), 23456f, 2f));
        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i <= height; i++)
            queue.Enqueue(new VoxelMod(new Vector3(postion.x, postion.y + i, postion.z), 12));


        return queue;
    }
}
