using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class World : MonoBehaviour
{
    public Settings Settings;
    public BiomeAttributes[] biomes;

    [Range(0f, 1f)]
    public float globalLightLevel;

    public Color dayColor;
    public Color nightColor;

    public Transform playerTrans;
    public Player player;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;
    public Material waterMaterial;

    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activesChunks = new List<ChunkCoord>();

    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private List<Chunk> chunksToUpdate = new List<Chunk>();

    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    bool applyingModifications = false;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    public Clouds clouds;

    public CreativeInventory creativeInventory;
    public GameObject cursorSlot;

    // REMOVED
    //Thread ChunkUpdateThread;
    //public object ChunkUpdateThreadLock = new object();
    //public object ChunkListThreadLock = new object();

    private static World _instace;
    public static World Instance { get { return _instace; } }

    public WorldData worldData;

    public string appPath;

    public GameObject debugScreen;

    private bool _inUI;
    public bool inUI
    {
        get
        {
            return _inUI;
        }
        set
        {
            _inUI = value;
            if (_inUI)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;

            Cursor.visible = _inUI;
            creativeInventory.gameObject.SetActive(_inUI);
            cursorSlot.SetActive(_inUI);
        }
    }

    private void Awake()
    {
        if (_instace != null && _instace != this)
            Destroy(this.gameObject);
        else
            _instace = this;

        appPath = Application.persistentDataPath;
        player = playerTrans.GetComponent<Player>();
    }

    private void Start()
    {
        Debug.Log($"<color=green>Generating new world using seed: {VoxelData.seed}</color>");

        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
        Settings = JsonUtility.FromJson<Settings>(jsonImport);

        worldData = SaveSystem.LoadWorld("Testing");

        Random.InitState(VoxelData.seed);

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

        LoadWorld();

        player = playerTrans.GetComponent<Player>();
        SetGlobalLightValue();
        spawnPosition = new Vector3(VoxelData.WorldCenter, VoxelData.ChunkHeight - 50f, VoxelData.WorldCenter);
        playerTrans.position = spawnPosition;
        CheckViewDistance();
        playerLastChunkCoord = GetChunkCoordForVector3(playerTrans.position);

        StartCoroutine(UpdateChunkInGame());
        StartCoroutine(CheckDistance());
    }

    private IEnumerator UpdateChunkInGame()
    {
        yield return new WaitForSeconds(.1f);
        if (chunksToUpdate.Count > 0)
            UpdateChunks();
        if (!applyingModifications)
            ApplayModifications();
        if (chunksToDraw.Count > 0)
            chunksToDraw.Dequeue().CreateMesh();
        yield return UpdateChunkInGame();
    }

    private IEnumerator CheckDistance()
    {
        yield return new WaitForSeconds(.1f);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();
        yield return CheckDistance();
    }

    float lestPosPlayerX;

    private void Update()
    {
        float playerPosX = playerTrans.position.x;
        if (playerPosX != lestPosPlayerX)
        {
            playerChunkCoord = GetChunkCoordForVector3(playerTrans.position);
            lestPosPlayerX = playerPosX;
        }

        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
        if (Input.GetKeyDown(KeyCode.F1))
            SaveSystem.SaveWorld(worldData);
    }

    public void SetGlobalLightValue()
    {
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(nightColor, dayColor, globalLightLevel);
    }

    void LoadWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - Settings.LoadDistance; x < (VoxelData.WorldSizeInChunks / 2) + Settings.LoadDistance; x++)
            for (int z = (VoxelData.WorldSizeInChunks / 2) - Settings.LoadDistance; z < (VoxelData.WorldSizeInChunks / 2) + Settings.LoadDistance; z++)
            {
                worldData.LoadChunk(new Vector2Int(x, z));
            }
    }

    public void AddChunkToUpdate(Chunk chunk)
    {
        AddChunkToUpdate(chunk, false);
    }

    public void AddChunkToUpdate(Chunk chunk, bool insert)
    {
        if (!chunksToUpdate.Contains(chunk))
            if (insert)
                chunksToUpdate.Insert(0, chunk);
            else
                chunksToUpdate.Add(chunk);
    }

    void UpdateChunks()
    {
        chunksToUpdate[0].UpdateChunk();
        if (!activesChunks.Contains(chunksToUpdate[0].Coord))
            activesChunks.Add(chunksToUpdate[0].Coord);
        chunksToUpdate.RemoveAt(0);
    }

    void ApplayModifications()
    {
        applyingModifications = true;

        while (modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();
                worldData.SetVoxel(v.position, v.id, 1);
            }
        }

        applyingModifications = false;
    }

    ChunkCoord GetChunkCoordForVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkForVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return chunks[x, z];
    }

    ChunkCoord coord;
    List<ChunkCoord> previouslyActiveChunck;
    ChunkCoord newCoord;

    void CheckViewDistance()
    {
        clouds.UpdateClouds();

        coord = GetChunkCoordForVector3(playerTrans.position);

        playerLastChunkCoord = playerChunkCoord;

        previouslyActiveChunck = new List<ChunkCoord>(activesChunks);

        activesChunks.Clear();

        for (int x = coord.x - Settings.ViewDistanceInChunks; x < coord.x + Settings.ViewDistanceInChunks; x++)
            for (int z = coord.z - Settings.ViewDistanceInChunks; z < coord.z + Settings.ViewDistanceInChunks; z++)
            {
                newCoord = new ChunkCoord(x, z);

                if (IsChunkInWorld(newCoord))
                {
                    if (chunks[x, z] == null)
                        chunks[x, z] = new Chunk(newCoord);

                    chunks[x, z].IsActive = true;
                    activesChunks.Add(newCoord);
                }

                for (int c = 0; c < previouslyActiveChunck.Count; c++)
                {
                    if (previouslyActiveChunck[c].Equals(newCoord))
                        previouslyActiveChunck.RemoveAt(c);
                }
            }

        foreach (ChunkCoord cc in previouslyActiveChunck)
            chunks[cc.x, cc.z].IsActive = false;
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        VoxelState voxel = worldData.GetVoxel(pos);
        if (voxel == null) return false;
        return blockTypes[voxel.id].isSolid;
    }

    public VoxelState GetVoxelState(Vector3 pos)
    {
        return worldData.GetVoxel(pos);
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        #region IMMUTABLE PASS 
        // If outside world, return air
        if (!IsVoxelInWorld(pos))
            return 0;
        // If bottom block of chunk, return bed
        if (yPos < 1)
            return 1;
        #endregion

        #region BIOME SELECTION PASS

        int solidGoundHeight = 42;
        float sumOfHeight = 0f;
        int count = 0;
        float strongestWeight = 0f;
        int strongestBiomeIndex = 0;

        float weight = 0f;
        float height = 0;
        for (int i = 0; i < biomes.Length; i++)
        {
            weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);
            // Keep track of witch weight is strongest.
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            // Get the height of the terrain (for the current biome) and multiply it by its weight.
            height = biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale) * weight;

            // If the height value is greater 0 add to sum of heights
            if (height > 0)
            {
                sumOfHeight += height;
                count++;
            }
        }

        // Set biome to the one with the strongest weight
        BiomeAttributes biome = biomes[strongestBiomeIndex];

        // Get the average of the heights
        sumOfHeight /= count;

        int terrarinHeight = Mathf.FloorToInt(sumOfHeight + solidGoundHeight);

        #endregion

        #region BASIC TERRAIN PASS
        byte voxelValue = 0;

        if (yPos == terrarinHeight)
            voxelValue = biome.surfaceBlock;
        else if (yPos < terrarinHeight && yPos > terrarinHeight - 4)
            voxelValue = biome.subSurfaceBlock;
        else if (yPos > terrarinHeight)
        {
            if (yPos < 45)
                return 15;
            else
                return 0;
        }
        else
            voxelValue = 2;
        #endregion

        #region SECOND PASS
        if (voxelValue == 2)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }
        #endregion

        #region TREE PASS
        if (yPos == terrarinHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex, pos, biome.minHeight, biome.maxHeight));
                }
            }
        }
        #endregion

        return voxelValue;

    }

    bool IsChunkInWorld(ChunkCoord coord) => coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 &&
        coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1;

    bool IsVoxelInWorld(Vector3 pos) => pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels &&
        pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
        pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels;
}