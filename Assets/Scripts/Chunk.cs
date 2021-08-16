using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord Coord;

    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<int> transparentTriangles = new List<int>();
    List<int> waterTriangles = new List<int>();
    Material[] materials = new Material[3];
    List<Vector2> uvs = new List<Vector2>();
    List<Color> colors = new List<Color>();
    List<Vector3> normals = new List<Vector3>();

    ChunkData chunkData;

    int vertexIndex = 0;

    public Vector3 position;

    private bool _isActive;
    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }

    public Chunk(ChunkCoord coord)
    {
        Coord = coord;
        IsActive = true;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = World.Instance.material;
        materials[1] = World.Instance.transparentMaterial;
        materials[2] = World.Instance.waterMaterial;
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(World.Instance.transform);
        chunkObject.transform.position = new Vector3(Coord.x * VoxelData.ChunkWidth, 0f, Coord.z * VoxelData.ChunkWidth);
        chunkObject.name = $"Chunk X:{Coord.x}, Z:{Coord.z}";
        position = chunkObject.transform.position;

        chunkData = World.Instance.worldData.RequestChunk(new Vector2Int((int)position.x, (int)position.z), true);
        chunkData.chunk = this;

        World.Instance.AddChunkToUpdate(this);
    }

    public void EditVoxel(Vector3 pos, byte newId)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        chunkData.ModifyVoxel(new Vector3Int(xCheck, yCheck, zCheck), newId, World.Instance.player.orientation);

        UpdateSurrondingVoxels(xCheck, yCheck, zCheck);
    }

    void UpdateSurrondingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int f = 0; f < 6; f++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.facesCheck[f];

            if (!chunkData.IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
                World.Instance.AddChunkToUpdate(World.Instance.GetChunkForVector3(currentVoxel + position), true);
        }
    }

    public VoxelState GetVoxelInGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        return chunkData.map[xCheck, yCheck, zCheck];
    }

    public void UpdateChunk()
    {
        ClearMeshData();

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                    if (World.Instance.blockTypes[chunkData.map[x, y, z].id].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));

        World.Instance.chunksToDraw.Enqueue(this);
    }

    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
        colors.Clear();
        normals.Clear();
    }

    private void UpdateMeshData(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        VoxelState voxel = chunkData.map[x, y, z];
        VoxelState neighbor;
        float rot = voxel.orientation switch
        {
            0 => 180f,
            5 => 270f,
            1 => 0f,
            _ => 90f
        };

        for (int f = 0; f < voxel.properties.meshData.faces.Length; f++)
        {
            int translatedP = f;

            if (voxel.orientation != 1)
            {
                if (voxel.orientation == 0)
                {
                    if (f == 0) translatedP = 1;
                    else if (f == 1) translatedP = 0;
                    else if (f == 4) translatedP = 5;
                    else if (f == 5) translatedP = 4;
                }
                else if (voxel.orientation == 5)
                {
                    if (f == 0) translatedP = 5;
                    else if (f == 1) translatedP = 4;
                    else if (f == 4) translatedP = 0;
                    else if (f == 5) translatedP = 1;
                }
                else if (voxel.orientation == 4)
                {
                    if (f == 0) translatedP = 4;
                    else if (f == 1) translatedP = 5;
                    else if (f == 4) translatedP = 1;
                    else if (f == 5) translatedP = 0;
                }
            }

            neighbor = chunkData.map[x, y, z].neighbours[translatedP];
            bool noRenderSideFaces = neighbor == null ||
                !neighbor.properties.renderNeighborFaces ||
                (voxel.properties.isWater && chunkData.map[x, y + 1, z].properties.isWater);

            if (noRenderSideFaces) continue;

            float lightLevel = neighbor.lightAsFloat;
            int faceVertCount = 0;
            FaceMeshData facesData = voxel.properties.meshData.faces[f];

            for (int i = 0; i < facesData.vertsData.Length; i++)
            {
                vertices.Add(pos + facesData.vertsData[i].GetRotatePosition(new Vector3(0, rot, 0)));
                normals.Add(facesData.normal);
                if (voxel.properties.isWater)
                    uvs.Add(facesData.vertsData[i].uv);
                else
                    AddTexture(voxel.properties.GetTextureID(f), facesData.vertsData[i].uv);
                colors.Add(new Color(0, 0, 0, lightLevel));
                faceVertCount++;
            }

            if (!neighbor.properties.renderNeighborFaces)
                for (int i = 0; i < facesData.triangels.Length; i++)
                    triangles.Add(vertexIndex + facesData.triangels[i]);
            else
            {
                if (voxel.properties.isWater)
                    for (int i = 0; i < facesData.triangels.Length; i++)
                        waterTriangles.Add(vertexIndex + facesData.triangels[i]);
                else
                    for (int i = 0; i < facesData.triangels.Length; i++)
                        transparentTriangles.Add(vertexIndex + facesData.triangels[i]);
            }

            vertexIndex += faceVertCount;
        }
    }

    void AddTexture(int textuID, Vector2 uv)
    {
        float y = textuID / VoxelData.textureAtlasSizeInBlock;
        float x = textuID - (y * VoxelData.textureAtlasSizeInBlock);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        x += VoxelData.NormalizedBlockTextureSize * uv.x;
        y += VoxelData.NormalizedBlockTextureSize * uv.y;

        uvs.Add(new Vector2(x, y));
    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();

        mesh.subMeshCount = 3;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        mesh.SetTriangles(waterTriangles.ToArray(), 2);

        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();

        meshFilter.mesh = mesh;
    }
}