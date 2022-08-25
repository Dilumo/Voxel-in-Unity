using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DebugScreen : MonoBehaviour
{
    private World _world;
    private Text _text;

    private float _frameRate;
    private float _timer;

    private int _halfWorldSizeInVoxels;
    private int _halfWorldSizeInChunks;

    private void Start()
    {
        _world = GameObject.Find("World").GetComponent<World>(); // junk
        _text = GetComponent<Text>();

        _halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
        _halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
    }

    private void Update()
    {
        if (_world.inUI) return;

        var posPLayer = _world.playerTrans.transform.position;

        var debugText = "Code a game like Minecraft in Unity...";
        debugText += "\n";
        debugText += _frameRate + " fps";
        debugText += "\n\n";
        debugText += $"Position: [{Mathf.FloorToInt(posPLayer.x) -  _halfWorldSizeInVoxels}, {Mathf.FloorToInt(posPLayer.y)}, {Mathf.FloorToInt(posPLayer.x) - _halfWorldSizeInVoxels}]";
        debugText += "\n";
        debugText += $"Chunk: [{_world.playerChunkCoord.x - _halfWorldSizeInChunks}, {_world.playerChunkCoord.z - _halfWorldSizeInChunks}]";

        var direction = _world.player.orientation switch
        {
            0 => "South",
            5 => "East",
            1 => "North",
            4 => "West",
            _ => "???"
        };

        debugText += "\n";

        debugText += $"Direction facing: {direction}";


        _text.text = debugText;

        if (_timer > 1f)
        {
            _frameRate = (int)(1f / Time.unscaledDeltaTime);
            _timer = 0;
        }
        else
            _timer += Time.deltaTime;
    }
}
