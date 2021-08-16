using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VoxelState
{
    public byte id;
    public int orientation;
    [System.NonSerialized] private byte _light;
    [System.NonSerialized] public ChunkData chunkData;
    [System.NonSerialized] public VoxelNeighbour neighbours;
    [System.NonSerialized] public Vector3Int position;
    public byte light
    {
        get { return _light; }
        set
        {
            if (value != _light)
            {
                byte oldLightValue = _light;
                byte oldCastValue = castLight;

                _light = value;
                if (_light < oldLightValue)
                {
                    List<int> neighbourToDarken = new List<int>();
                    for (int f = 0; f < 6; f++)
                    {
                        if (neighbours[f] != null)
                        {
                            if (neighbours[f].light <= oldCastValue)
                                neighbourToDarken.Add(f);
                            else
                                neighbours[f].PropogateLight();
                        }
                    }

                    foreach(int i in neighbourToDarken)
                    {
                        neighbours[i].light = 0;
                    }

                    if (chunkData.chunk != null)
                        World.Instance.AddChunkToUpdate(chunkData.chunk);
                }
                else if (_light > 1)
                    PropogateLight();
            }
        }
    }

    public float lightAsFloat
    {
        get { return (float)light * VoxelData.unitOfLight; }
    }
    public byte castLight
    {
        get
        {
            int lightLevel = _light - properties.opacity - 1;
            if (lightLevel < 0) lightLevel = 0;
            return (byte)lightLevel;
        }
    }

    public Vector3Int globalPosition { get { return new Vector3Int(position.x + chunkData.position.x, position.y, position.z + chunkData.position.y); } }

    public BlockType properties
    {
        get { return World.Instance.blockTypes[id]; }
    }

    public VoxelState(byte idValue, ChunkData chunkDataValue, Vector3Int pos)
    {
        id = idValue;
        orientation = 1;
        chunkData = chunkDataValue;
        neighbours = new VoxelNeighbour(this);
        position = pos;
        light = 0;
    }

    public void PropogateLight()
    {
        if (light < 2) return;

        for (int f = 0; f < 6; f++)
        {
            if (neighbours[f] != null)
            {
                if (neighbours[f].light < castLight)
                    neighbours[f].light = castLight;
            }

            if (chunkData.chunk != null)
                World.Instance.AddChunkToUpdate(chunkData.chunk);
        }
    }
}