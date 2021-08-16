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

    public int GetTextureID (int faceIndex) {
        switch (faceIndex) {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.LogError ($"Erro in GetTextureID, <color=read> invaled face index</color>");
                return 0;
        }
    }
}