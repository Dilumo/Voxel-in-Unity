using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

using UnityEngine;

public class AtlasPacker : EditorWindow
{
    int blockPixelSize = 16;
    int atlasSizeInBlock = 16;
    int atlasSize;

    Object[] rawTextures = new Object[256];
    List<Texture2D> sortedTextures = new List<Texture2D> ();
    Texture2D atlas;

    [MenuItem ("Minecraft/Atlas Packer")]
    public static void ShowWindow ()
    {
        EditorWindow.GetWindow (typeof (AtlasPacker));
    }

    private void OnGUI ()
    {
        atlasSize = blockPixelSize * atlasSizeInBlock;
        GUILayout.Label ("Minecraf texture atlas packer", EditorStyles.boldLabel);

        blockPixelSize = EditorGUILayout.IntField ("Block Pixel Size", blockPixelSize);
        blockPixelSize = EditorGUILayout.IntField ("Atlas Size in Block", atlasSizeInBlock);

        GUILayout.Label (atlas);

        if (GUILayout.Button ("Load Textues"))
        {
            LoadTextures ();
            PackAtlas ();
            Debug.Log ($"<color=green>Atlas Packer successfully loaded!</color>");
        }
        if (GUILayout.Button ("Clear Textues"))
        {
            atlas = new Texture2D (atlasSize, atlasSize);
            Debug.Log ($"<color=green>Atlas Packer successfully cleared.</color>");
        }

        if (GUILayout.Button ("Save Atlas"))
        {
            byte[] bytes = atlas.EncodeToPNG();

            try
            {
                File.WriteAllBytes($"{Application.dataPath}/Textures/Packed_Atlas.png", bytes);
                Debug.Log ($"<color=green>Atlas Packer successfully saved.</color>");
            }
            catch (System.Exception ex)
            {
                 Debug.Log ($"<color=red>Atlas Packer: Couldn't save atlas to file. //n {ex.Message}</color>");
            }
            
        }
    }

    void LoadTextures ()
    {
        sortedTextures.Clear ();

        rawTextures = Resources.LoadAll ("AtlasPacker", typeof (Texture2D));
        int index = 0;
        foreach (Object tex in rawTextures)
        {
            Texture2D t = (Texture2D) tex;
            if (t.width == blockPixelSize && t.height == blockPixelSize)
                sortedTextures.Add (t);
            else
                Debug.Log ($"<color=yellow>Atlas Packer: {tex.name} incorrect size. Texture no load!</color>");

            index++;
        }
        int qtnLoaded = sortedTextures.Count;
        if (qtnLoaded > 0)
            Debug.Log ($"<color=green>Atlas Packer {qtnLoaded} textures successfully loaded.</color>");
        else
            Debug.Log ($"<color=yellow>Atlas Packer no texture loaded!</color>");

    }

    void PackAtlas ()
    {
        atlas = new Texture2D (atlasSize, atlasSize);
        Color[] pixels = new Color[atlasSize * atlasSize];

        int currentBlockX = 0;
        int currentBlockY = 0;
        int index = 0;
        int currentPixelX = 0;
        int currentPixelY = 0;
        for (var x = 0; x < atlasSize; x++)
            for (var y = 0; y < atlasSize; y++)
            {
                // Get current block
                currentBlockX = x / blockPixelSize;
                currentBlockY = y / blockPixelSize;

                index = currentBlockY * atlasSizeInBlock + currentBlockX;

                // Get the pixel
                currentPixelX = x - (currentBlockX * blockPixelSize);
                currentPixelY = y - (currentBlockY * blockPixelSize);

                if (index < sortedTextures.Count)
                    pixels[(atlasSize - y - 1) * atlasSize + x] = sortedTextures[index].GetPixel (x, blockPixelSize - y - 1);
                else
                    pixels[(atlasSize - y - 1) * atlasSize + x] = new Color (0f, 0f, 0f, 0f);
            }

        atlas.SetPixels (pixels);

        atlas.Apply ();
    }
}