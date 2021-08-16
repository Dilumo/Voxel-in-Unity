using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>(); // junk
        text = GetComponent<Text>();

        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
    }

    private void Update()
    {
        if (world.inUI) return;

        Vector3 posPLayer = world.playerTrans.transform.position;

        string debugText = "Code a game like Minecraft in Unity...";
        debugText += "\n";
        debugText += frameRate + " fps";
        debugText += "\n\n";
        debugText += $"Position: [{Mathf.FloorToInt(posPLayer.x) -  halfWorldSizeInVoxels}, {Mathf.FloorToInt(posPLayer.y)}, {Mathf.FloorToInt(posPLayer.x) - halfWorldSizeInVoxels}]";
        debugText += "\n";
        debugText += $"Chunk: [{world.playerChunkCoord.x - halfWorldSizeInChunks}, {world.playerChunkCoord.z - halfWorldSizeInChunks}]";

        string direction = "";

        switch (world.player.orientation)
        {
            case 0:
                direction = "South";
                break;
            case 5:
                direction = "East";
                break;
            case 1:
                direction = "North";
                break;
            case 4:
                direction = "West";
                break;
            default:
                direction = "???";
                break;

        }

        debugText += "\n";

        debugText += $"Direction facing: {direction}";


        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
            timer += Time.deltaTime;
    }
}
