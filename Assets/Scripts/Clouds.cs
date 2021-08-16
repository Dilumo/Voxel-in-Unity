using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    Settings settings = new Settings();

    public int cloudHeight = 100;
    public int cloudDepth = 4;

    [SerializeField] private Texture2D cloudPatter = null;
    [SerializeField] private Material cloudMaterial = null;
    [SerializeField] private World world = null;
    /// <summary>
    /// Represent local
    /// </summary>
    bool[,] cloudData;

    int cloudTextWidth;

    int cloudTileSize;
    Vector3Int offset;

    Dictionary<Vector2Int, GameObject> clouds = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {
        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
        settings = JsonUtility.FromJson<Settings>(jsonImport);

        cloudTextWidth = cloudPatter.width;
        cloudTileSize = VoxelData.ChunkWidth;
        offset = new Vector3Int(-(cloudTextWidth / 2), 0, -(cloudTextWidth / 2));

        transform.position = new Vector3(VoxelData.WorldCenter, cloudHeight, VoxelData.WorldCenter);

        LoadCloudData();
        CreateClouds();

    }

    private void LoadCloudData()
    {
        cloudData = new bool[cloudTextWidth, cloudTextWidth];
        Color[] cloudTex = cloudPatter.GetPixels();

        for (var x = 0; x < cloudTextWidth; x++)
            for (var y = 0; y < cloudTextWidth; y++)
                cloudData[x, y] = (cloudTex[y * cloudTextWidth + x].a > 0);
    }

    private void CreateClouds()
    {
        if (settings.cloudStyle == CloudStyle.Off) return;

        Mesh cloudMesh;
        Vector3 position = new Vector3();
        for (var x = 0; x < cloudTextWidth; x += cloudTileSize)
            for (var y = 0; y < cloudTextWidth; y += cloudTileSize)
            {
                if (settings.cloudStyle == CloudStyle.Fast)
                    cloudMesh = CreateFastCloudMesh(x, y);
                else
                    cloudMesh = CreateFancyCloudMesh(x, y);
                position = new Vector3(x, cloudHeight, y);
                position += transform.position - new Vector3(cloudTextWidth / 2f, 0f, cloudTextWidth / 2f);
                position.y = cloudHeight;
                clouds.Add(CloudTilePosFromV3(position), CreateCloudTile(cloudMesh, position));
            }
    }


    public void UpdateClouds()
    {
        if (settings.cloudStyle == CloudStyle.Off) return;

        Vector3 position = new Vector3();
        Vector2Int cloudPosition = new Vector2Int();
        for (var x = 0; x < cloudTextWidth; x += cloudTileSize)
            for (var y = 0; y < cloudTextWidth; y += cloudTileSize)
            {
                position = world.playerTrans.position + new Vector3(x, 0, y) + offset;
                position = new Vector3(RoundToCloud(position.x), cloudHeight, RoundToCloud(position.z));
                cloudPosition = CloudTilePosFromV3(position);

                clouds[cloudPosition].transform.position = position;
            }
    }

    private int RoundToCloud(float value)
        => Mathf.FloorToInt(value / cloudTileSize) * cloudTileSize;

    /// <summary>
    /// Populate vectors to create clouds
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z">Because is about in the world</param>
    private Mesh CreateFastCloudMesh(int x, int z)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;

        for (int xIncrement = 0; xIncrement < cloudTileSize; xIncrement++)
            for (int zIncrement = 0; zIncrement < cloudTileSize; zIncrement++)
            {
                int xValue = x + xIncrement;
                int zValue = z + zIncrement;

                if (!cloudData[xValue, zValue]) continue;

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
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;

        for (int xIncrement = 0; xIncrement < cloudTileSize; xIncrement++)
            for (int zIncrement = 0; zIncrement < cloudTileSize; zIncrement++)
            {
                int xValue = x + xIncrement;
                int zValue = z + zIncrement;

                if (!cloudData[xValue, zValue]) continue;

                for (int f = 0; f < 6; f++)
                {
                    if (CheckCloudData(new Vector3Int(xValue, 0, zValue) + VoxelData.facesCheck[f])) continue;

                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 vert = new Vector3Int(xIncrement, 0, zIncrement);
                        vert += VoxelData.voxelVerts[VoxelData.voxelTris[f, i]];
                        vert.y *= cloudDepth;
                        vertices.Add(vert);
                        normals.Add(VoxelData.facesCheck[f]);
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

        int x = point.x;
        int z = point.z;
        // Out of range
        if (point.x < 0) x = cloudTextWidth - 1;
        if (point.x > cloudTextWidth - 1) x = 0;
        if (point.z < 0) z = cloudTextWidth - 1;
        if (point.z > cloudTextWidth - 1) z = 0;

        return cloudData[x, z];
    }

    private GameObject CreateCloudTile(Mesh mesh, Vector3 position)
    {
        GameObject newCloudTile = new GameObject
        {
            name = $"Chunk X:{position.x}, Z:{position.z}"
        };

        newCloudTile.transform.position = position;
        newCloudTile.transform.parent = transform;

        MeshFilter mF = newCloudTile.AddComponent<MeshFilter>();
        MeshRenderer mR = newCloudTile.AddComponent<MeshRenderer>();

        mR.material = cloudMaterial;
        mF.mesh = mesh;

        return newCloudTile;
    }

    private Vector2Int CloudTilePosFromV3(Vector3 pos)
        => new Vector2Int(CloudTileCoordFromFloat(pos.x), CloudTileCoordFromFloat(pos.z));

    private int CloudTileCoordFromFloat(float value)
    {
        float a = value / (float)cloudTextWidth;
        a -= Mathf.FloorToInt(a);
        return Mathf.FloorToInt((float)cloudTextWidth * a);
    }
}

public enum CloudStyle
{
    Off,
    Fast,
    Fancy
}