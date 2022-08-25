using UnityEngine;

[System.Serializable]
public class BlockType {
    public string blockName;
    public bool isSolid;
    public bool isWater;
    public VoxelMeshData meshData;
    public float density;
    public bool renderNeighborFaces;
    public byte opacity;
    public Sprite icon;

    [Header ("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    //back, front, top, bottom, left, right

    public int GetTextureID (int faceIndex)
    {
        return faceIndex switch
        {
            0 => backFaceTexture,
            1 => frontFaceTexture,
            2 => topFaceTexture,
            3 => bottomFaceTexture,
            4 => leftFaceTexture,
            5 => rightFaceTexture,
            _ => 0
        };
    }
}