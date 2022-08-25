using System.Collections.Generic;
using Data;
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
    public byte Light
    {
        get => _light;
        set
        {
            if (value == _light) return;
            var oldLightValue = _light;
            var oldCastValue = CastLight;

            _light = value;
            if (_light < oldLightValue)
            {
                var neighbourToDarken = new List<int>();
                for (var f = 0; f < 6; f++)
                {
                    if (neighbours[f] == null) continue;
                    if (neighbours[f].Light <= oldCastValue)
                        neighbourToDarken.Add(f);
                    else
                        neighbours[f].PropagateLight();
                }

                foreach(var i in neighbourToDarken)
                {
                    neighbours[i].Light = 0;
                }

                if (chunkData.chunk != null)
                    World.Instance.AddChunkToUpdate(chunkData.chunk);
            }
            else if (_light > 1)
                PropagateLight();
        }
    }

    public float LightAsFloat => (float)Light * VoxelData.UnitOfLight;

    public byte CastLight
    {
        get
        {
            var lightLevel = _light - Properties.opacity - 1;
            if (lightLevel < 0) lightLevel = 0;
            return (byte)lightLevel;
        }
    }

    public Vector3Int GlobalPosition => new Vector3Int(position.x + chunkData.position.x, position.y, position.z + chunkData.position.y);

    public BlockType Properties => World.Instance.blockTypes[id];

    public VoxelState(byte idValue, ChunkData chunkDataValue, Vector3Int pos)
    {
        id = idValue;
        orientation = 1;
        chunkData = chunkDataValue;
        neighbours = new VoxelNeighbour(this);
        position = pos;
        Light = 0;
    }

    public void PropagateLight()
    {
        if (Light < 2) return;

        for (var f = 0; f < 6; f++)
        {
            if (neighbours[f] != null)
            {
                if (neighbours[f].Light < CastLight)
                    neighbours[f].Light = CastLight;
            }

            if (chunkData.chunk != null)
                World.Instance.AddChunkToUpdate(chunkData.chunk);
        }
    }
}