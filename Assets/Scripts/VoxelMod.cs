using UnityEngine;

public class VoxelMod
{
    public Vector3 position;
    public readonly byte id;


    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 pos, byte voxelId)
    {
        position = pos;
        id = voxelId;
    }
}
