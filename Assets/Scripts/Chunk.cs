using System.Collections.Generic;
using Data;
using UnityEngine;

public class Chunk
{
    public readonly ChunkCoord coord;

    private readonly GameObject _chunkObject;
    private readonly MeshFilter _meshFilter;

    private readonly List<Vector3> _vertices = new List<Vector3>();
    private readonly List<int> _triangles = new List<int>();
    private readonly List<int> _transparentTriangles = new List<int>();
    private readonly List<int> _waterTriangles = new List<int>();
    private readonly Material[] _materials = new Material[3];
    private readonly List<Vector2> _uvs = new List<Vector2>();
    private readonly List<Color> _colors = new List<Color>();
    private readonly List<Vector3> _normals = new List<Vector3>();

    private readonly ChunkData _chunkData;

    private int _vertexIndex = 0;

    private readonly Vector3 _position;

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            if (_chunkObject)
                _chunkObject.SetActive(value);
        }
    }

    public Chunk(ChunkCoord coord)
    {
        this.coord = coord;
        IsActive = true;

        _chunkObject = new GameObject();
        _meshFilter = _chunkObject.AddComponent<MeshFilter>();
        var meshRenderer = _chunkObject.AddComponent<MeshRenderer>();

        _materials[0] = World.Instance.material;
        _materials[1] = World.Instance.transparentMaterial;
        _materials[2] = World.Instance.waterMaterial;
        meshRenderer.materials = _materials;

        _chunkObject.transform.SetParent(World.Instance.transform);
        _chunkObject.transform.position = new Vector3(this.coord.x * VoxelData.ChunkWidth, 0f, this.coord.z * VoxelData.ChunkWidth);
        _chunkObject.name = $"Chunk X:{this.coord.x}, Z:{this.coord.z}";
        _position = _chunkObject.transform.position;

        _chunkData = World.Instance.worldData.RequestChunk(new Vector2Int((int)_position.x, (int)_position.z), true);
        _chunkData.chunk = this;

        World.Instance.AddChunkToUpdate(this);
    }

    public void EditVoxel(Vector3 pos, byte newId)
    {
        var xCheck = Mathf.FloorToInt(pos.x);
        var yCheck = Mathf.FloorToInt(pos.y);
        var zCheck = Mathf.FloorToInt(pos.z);

        var position = _chunkObject.transform.position;
        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        _chunkData.ModifyVoxel(new Vector3Int(xCheck, yCheck, zCheck), newId, World.Instance.player.orientation);

        UpdateSurrondingVoxels(xCheck, yCheck, zCheck);
    }

    void UpdateSurrondingVoxels(int x, int y, int z)
    {
        var thisVoxel = new Vector3(x, y, z);

        for (var f = 0; f < 6; f++)
        {
            var currentVoxel = thisVoxel + VoxelData.FacesCheck[f];

            if (!_chunkData.IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
                World.Instance.AddChunkToUpdate(World.Instance.GetChunkForVector3(currentVoxel + _position), true);
        }
    }

    public VoxelState GetVoxelInGlobalVector3(Vector3 pos)
    {
        var xCheck = Mathf.FloorToInt(pos.x);
        var yCheck = Mathf.FloorToInt(pos.y);
        var zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(_position.x);
        zCheck -= Mathf.FloorToInt(_position.z);

        return _chunkData.map[xCheck, yCheck, zCheck];
    }

    public void UpdateChunk()
    {
        ClearMeshData();

        for (var y = 0; y < VoxelData.ChunkHeight; y++)
            for (var x = 0; x < VoxelData.ChunkWidth; x++)
                for (var z = 0; z < VoxelData.ChunkWidth; z++)
                    if (World.Instance.blockTypes[_chunkData.map[x, y, z].id].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));

        World.Instance.chunksToDraw.Enqueue(this);
    }

    private void ClearMeshData()
    {
        _vertexIndex = 0;
        _vertices.Clear();
        _triangles.Clear();
        _transparentTriangles.Clear();
        _uvs.Clear();
        _colors.Clear();
        _normals.Clear();
    }

    private void UpdateMeshData(Vector3 pos)
    {
        var x = Mathf.FloorToInt(pos.x);
        var y = Mathf.FloorToInt(pos.y);
        var z = Mathf.FloorToInt(pos.z);

        var voxel = _chunkData.map[x, y, z];
        var rot = voxel.orientation switch
        {
            0 => 180f,
            5 => 270f,
            1 => 0f,
            _ => 90f
        };

        for (var f = 0; f < voxel.Properties.meshData.faces.Length; f++)
        {
            var translatedP = f;

            if (voxel.orientation != 1)
            {
                switch (voxel.orientation)
                {
                    case 0 when f == 0:
                        translatedP = 1;
                        break;
                    case 0 when f == 1:
                        translatedP = 0;
                        break;
                    case 0 when f == 4:
                        translatedP = 5;
                        break;
                    case 0:
                    {
                        if (f == 5) translatedP = 4;
                        break;
                    }
                    case 5 when f == 0:
                        translatedP = 5;
                        break;
                    case 5 when f == 1:
                        translatedP = 4;
                        break;
                    case 5 when f == 4:
                        translatedP = 0;
                        break;
                    case 5:
                    {
                        if (f == 5) translatedP = 1;
                        break;
                    }
                    case 4 when f == 0:
                        translatedP = 4;
                        break;
                    case 4 when f == 1:
                        translatedP = 5;
                        break;
                    case 4 when f == 4:
                        translatedP = 1;
                        break;
                    case 4:
                    {
                        if (f == 5) translatedP = 0;
                        break;
                    }
                }
            }

            var neighbor = _chunkData.map[x, y, z].neighbours[translatedP];
            var noRenderSideFaces = neighbor == null ||
                                    !neighbor.Properties.renderNeighborFaces ||
                                    (voxel.Properties.isWater && _chunkData.map[x, y + 1, z].Properties.isWater);

            if (noRenderSideFaces) continue;

            var lightLevel = neighbor.LightAsFloat;
            var faceVertCount = 0;
            var facesData = voxel.Properties.meshData.faces[f];

            foreach (var t in facesData.vertsData)
            {
                _vertices.Add(pos + t.GetRotatePosition(new Vector3(0, rot, 0)));
                _normals.Add(facesData.normal);
                if (voxel.Properties.isWater)
                    _uvs.Add(t.uv);
                else
                    AddTexture(voxel.Properties.GetTextureID(f), t.uv);
                _colors.Add(new Color(0, 0, 0, lightLevel));
                faceVertCount++;
            }

            if (!neighbor.Properties.renderNeighborFaces)
                foreach (var t in facesData.triangels)
                    _triangles.Add(_vertexIndex + t);
            else
            {
                if (voxel.Properties.isWater)
                    foreach (var t in facesData.triangels)
                        _waterTriangles.Add(_vertexIndex + t);
                else
                    foreach (var t in facesData.triangels)
                        _transparentTriangles.Add(_vertexIndex + t);
            }

            _vertexIndex += faceVertCount;
        }
    }

    private void AddTexture(int textureID, Vector2 uv)
    {
        // ReSharper disable once PossibleLossOfFraction
        float y = textureID / VoxelData.TextureAtlasSizeInBlock;
        var x = textureID - (y * VoxelData.TextureAtlasSizeInBlock);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        x += VoxelData.NormalizedBlockTextureSize * uv.x;
        y += VoxelData.NormalizedBlockTextureSize * uv.y;

        _uvs.Add(new Vector2(x, y));
    }

    public void CreateMesh()
    {
        var mesh = new Mesh();

        mesh.vertices = _vertices.ToArray();

        mesh.subMeshCount = 3;
        mesh.SetTriangles(_triangles.ToArray(), 0);
        mesh.SetTriangles(_transparentTriangles.ToArray(), 1);
        mesh.SetTriangles(_waterTriangles.ToArray(), 2);

        mesh.uv = _uvs.ToArray();
        mesh.colors = _colors.ToArray();
        mesh.normals = _normals.ToArray();

        _meshFilter.mesh = mesh;
    }
}