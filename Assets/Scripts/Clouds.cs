using System.Collections;
using System.Collections.Generic;
using System.IO;
using Data;
using UnityEngine;
using UnityEngine.Serialization;

public class Clouds : MonoBehaviour
{
    private Settings _settings = new Settings();

    public int cloudHeight = 100;
    public int cloudDepth = 4;

    [SerializeField] private Texture2D cloudPatter = null;
    [SerializeField] private Material cloudMaterial = null;
    [SerializeField] private World world = null;
    /// <summary>
    /// Represent local
    /// </summary>
    private bool[,] _cloudData;

    private int _cloudTextWidth;

    private int _cloudTileSize;
    [FormerlySerializedAs("_offset")] [SerializeField]
    private Vector3Int offset;

    private readonly Dictionary<Vector2Int, GameObject> _clouds = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {
        var jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
        _settings = JsonUtility.FromJson<Settings>(jsonImport);

        _cloudTextWidth = cloudPatter.width;
        _cloudTileSize = VoxelData.ChunkWidth;
        offset = new Vector3Int(-(_cloudTextWidth / 2), 0, -(_cloudTextWidth / 2));

        transform.position = new Vector3(VoxelData.WorldCenter, cloudHeight, VoxelData.WorldCenter);

        LoadCloudData();
        CreateClouds();

    }

    private void LoadCloudData()
    {
        _cloudData = new bool[_cloudTextWidth, _cloudTextWidth];
        var cloudTex = cloudPatter.GetPixels();

        for (var x = 0; x < _cloudTextWidth; x++)
            for (var y = 0; y < _cloudTextWidth; y++)
                _cloudData[x, y] = (cloudTex[y * _cloudTextWidth + x].a > 0);
    }

    private void CreateClouds()
    {
        if (_settings.cloudStyle == CloudStyle.Off) return;

        for (var x = 0; x < _cloudTextWidth; x += _cloudTileSize)
            for (var y = 0; y < _cloudTextWidth; y += _cloudTileSize)
            {
                var cloudMesh = _settings.cloudStyle == CloudStyle.Fast ? CreateFastCloudMesh(x, y) : CreateFancyCloudMesh(x, y);
                var position = new Vector3(x, cloudHeight, y);
                position += transform.position - new Vector3(_cloudTextWidth / 2f, 0f, _cloudTextWidth / 2f);
                position.y = cloudHeight;
                _clouds.Add(CloudTilePosFromV3(position), CreateCloudTile(cloudMesh, position));
            }
    }


    public void UpdateClouds()
    {
        if (_settings.cloudStyle == CloudStyle.Off) return;

        for (var x = 0; x < _cloudTextWidth; x += _cloudTileSize)
            for (var y = 0; y < _cloudTextWidth; y += _cloudTileSize)
            {
                var position = world.playerTrans.position + new Vector3(x, 0, y) + offset;
                position = new Vector3(RoundToCloud(position.x), cloudHeight, RoundToCloud(position.z));
                var cloudPosition = CloudTilePosFromV3(position);

                _clouds[cloudPosition].transform.position = position;
            }
    }

    private int RoundToCloud(float value)
        => Mathf.FloorToInt(value / _cloudTileSize) * _cloudTileSize;

    /// <summary>
    /// Populate vectors to create clouds
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z">Because is about in the world</param>
    private Mesh CreateFastCloudMesh(int x, int z)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var normals = new List<Vector3>();
        var vertCount = 0;

        for (var xIncrement = 0; xIncrement < _cloudTileSize; xIncrement++)
            for (var zIncrement = 0; zIncrement < _cloudTileSize; zIncrement++)
            {
                var xValue = x + xIncrement;
                var zValue = z + zIncrement;

                if (!_cloudData[xValue, zValue]) continue;

                vertices.Add(new Vector3(xIncrement, 0, zIncrement));
                vertices.Add(new Vector3(xIncrement, 0, zIncrement + 1));
                vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement + 1));
                vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement));

                for (int i = 0; i < 4; i++)
                    normals.Add(Vector3.down);

                // #1
                triangles.Add(vertCount + 1);
                triangles.Add(vertCount);
                triangles.Add(vertCount + 2);
                // #2
                triangles.Add(vertCount + 2);
                triangles.Add(vertCount);
                triangles.Add(vertCount + 3);

                vertCount += 4;
            }

        return new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            normals = normals.ToArray()
        };
    }
    private Mesh CreateFancyCloudMesh(int x, int z)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var normals = new List<Vector3>();
        var vertCount = 0;

        for (var xIncrement = 0; xIncrement < _cloudTileSize; xIncrement++)
            for (var zIncrement = 0; zIncrement < _cloudTileSize; zIncrement++)
            {
                var xValue = x + xIncrement;
                var zValue = z + zIncrement;

                if (!_cloudData[xValue, zValue]) continue;

                for (var f = 0; f < 6; f++)
                {
                    if (CheckCloudData(new Vector3Int(xValue, 0, zValue) + VoxelData.FacesCheck[f])) continue;

                    for (var i = 0; i < 4; i++)
                    {
                        Vector3 vert = new Vector3Int(xIncrement, 0, zIncrement);
                        vert += VoxelData.VoxelVerts[VoxelData.voxelTris[f, i]];
                        vert.y *= cloudDepth;
                        vertices.Add(vert);
                        normals.Add(VoxelData.FacesCheck[f]);
                    }

                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 1);
                    triangles.Add(vertCount + 2);
                    triangles.Add(vertCount + 2);
                    triangles.Add(vertCount + 1);
                    triangles.Add(vertCount + 3);

                    vertCount += 4;
                }
            }

        return new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            normals = normals.ToArray()
        };
    }

    private bool CheckCloudData(Vector3Int point)
    {
        if (point.y != 0) return false;

        var x = point.x;
        var z = point.z;
        // Out of range
        if (point.x < 0) x = _cloudTextWidth - 1;
        if (point.x > _cloudTextWidth - 1) x = 0;
        if (point.z < 0) z = _cloudTextWidth - 1;
        if (point.z > _cloudTextWidth - 1) z = 0;

        return _cloudData[x, z];
    }

    private GameObject CreateCloudTile(Mesh mesh, Vector3 position)
    {
        var newCloudTile = new GameObject
        {
            name = $"Chunk X:{position.x}, Z:{position.z}",
            transform =
            {
                position = position,
                parent = transform
            }
        };

        var mF = newCloudTile.AddComponent<MeshFilter>();
        var mR = newCloudTile.AddComponent<MeshRenderer>();

        mR.material = cloudMaterial;
        mF.mesh = mesh;

        return newCloudTile;
    }

    private Vector2Int CloudTilePosFromV3(Vector3 pos)
        => new Vector2Int(CloudTileCoordFromFloat(pos.x), CloudTileCoordFromFloat(pos.z));

    private int CloudTileCoordFromFloat(float value)
    {
        var a = value / (float)_cloudTextWidth;
        a -= Mathf.FloorToInt(a);
        return Mathf.FloorToInt((float)_cloudTextWidth * a);
    }
}

public enum CloudStyle
{
    Off,
    Fast,
    Fancy
}