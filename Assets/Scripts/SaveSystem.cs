using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

public static class SaveSystem
{
    public static void SaveWorld(WorldData world)
    {
        string savePath = $"{World.Instance.appPath}/saves/{world.worldName}/";

        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        Debug.Log(savePath);
        Debug.Log($"<color=green>Saving {world.worldName}...</color>");

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream($"{savePath}world.world", FileMode.Create);

        formatter.Serialize(stream, world);
        stream.Close();

        Thread thread = new Thread(() => SaveChunks(world));
        thread.Start();
    }

    private static void SaveChunks(WorldData world)
    {
        List<ChunkData> chunks = new List<ChunkData>(world.modifiedChunks); // becaues save is slow
        world.modifiedChunks.Clear();

        int count = 0;
        foreach (ChunkData chunk in chunks)
        {
            SaveSystem.SaveChunk(chunk, world.worldName);
            count++;
        }
        Debug.Log($"<color=green>{count} chunks saved.</color>");
    }

    public static WorldData LoadWorld(string worldName, int seed = 0)
    {
        string loadPath = $"{World.Instance.appPath}/saves/{worldName}/";

        if (File.Exists($"{loadPath}world.world"))
        {
            Debug.Log($"<color=green>{worldName} found. Loading from save.</color>");

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream($"{loadPath}world.world", FileMode.Open);

            WorldData world = formatter.Deserialize(stream) as WorldData;
            stream.Close();

            return new WorldData(world);
        }
        else
        {
            Debug.Log($"<color=yellow>{worldName} not found. Create world.</color>");

            WorldData world = new WorldData(worldName, seed);

            SaveWorld(world);

            return world;
        }
    }

    public static void SaveChunk(ChunkData chunk, string worldName)
    {
        string chunkName = $"{chunk.position.x}-{chunk.position.y}";

        string savePath = $"{World.Instance.appPath}/saves/{worldName}/chunk/";

        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream($"{savePath}{chunkName}.chunk", FileMode.Create);

        formatter.Serialize(stream, chunk);
        stream.Close();
    }

    public static ChunkData LoadChunk(string worldName, Vector2Int position)
    {
        string chunkName = $"{position.x}-{position.y}";
        string loadPath = $"{World.Instance.appPath}/saves/{worldName}/chunk/{chunkName}.chunk";

        if (File.Exists(loadPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath, FileMode.Open);

            ChunkData chunk = formatter.Deserialize(stream) as ChunkData;
            stream.Close();

            return chunk;
        }
        return null;
    }
}
