using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public enum Biome
{
    Plains,
    Mountains,
    Desert,
    Ocean
}

public class WorldGenerator : MonoBehaviour
{
    public static Dictionary<Vector3Int, int[,,]> WorldData;
    public static Dictionary<Vector2Int, GameObject> ActiveChunks;
    public static Dictionary<Vector2Int, int[,,]> AdditiveWorldData;
    public static readonly Vector3Int ChunkSize = new Vector3Int(16, 256, 16);

    [SerializeField] private TextureLoad TextureLoaderInstance;
    [SerializeField] private Material ChunkMaterial;
    [SerializeField] private PhysicMaterial ChunkPhysicMaterial;
    [SerializeField] private GameObject droppedItem;
    [Space]
    public Vector2 NoiseScale = Vector2.one;
    public Vector2 NoiseOffset = Vector2.zero;
    [Space]
    public AnimationCurve continentalness;
    public int HeightOffset = 60;
    public float HeightIntensity = 5f;
    public int WaterOffset;

    private ChunkMeshGenerator meshCreator;
    private DataGenerator dataCreator;
    private MapGenerator map;

    void Start()
    {
        WorldData = new Dictionary<Vector3Int, int[,,]>();
        ActiveChunks = new Dictionary<Vector2Int, GameObject>();
        AdditiveWorldData = new Dictionary<Vector2Int, int[,,]>();
        meshCreator = new ChunkMeshGenerator(TextureLoaderInstance, this);
        dataCreator = new DataGenerator(this, GetComponent<StructureGenerator>());

        map = FindObjectOfType<MapGenerator>();
        map.GenerateMap();
        NoiseOffset = new Vector2(Random.Range(-1000, 1000), Random.Range(-1000, 1000));
    }

    public IEnumerator CreateChunk(Vector2Int ChunkCoord)
    {
        Vector3Int pos = new Vector3Int(ChunkCoord.x, 0, ChunkCoord.y);

        string chunkName = $"Chunk {ChunkCoord.x} {ChunkCoord.y}";

        if (chunkName == "Chunk 0 0")
        {
            Debug.Log(1);
        }

        GameObject newChunk = new GameObject(chunkName, new System.Type[]
        {
            typeof(MeshRenderer),
            typeof(MeshFilter),
            typeof(MeshCollider)
        });

        newChunk.transform.position = new Vector3(ChunkCoord.x * 16, 0f, ChunkCoord.y * 16);
        ActiveChunks.Add(ChunkCoord, newChunk);

        int[,,] dataToApply = WorldData.ContainsKey(pos) ? WorldData[pos] : null;
        Mesh meshToUse = null;

        if (dataToApply == null)
        {
            dataCreator.QueueDataToGenerate(new DataGenerator.GenData
            {
                Map = map,
                continentalness = continentalness,
                GenerationPoint = pos,
                OnComplete = x => dataToApply = x
            });

            yield return new WaitUntil(() => dataToApply != null);
        }

        meshCreator.QueueDataToDraw(new ChunkMeshGenerator.CreateMesh
        {
            DataToDraw = dataToApply,
            OnComplete = x => meshToUse = x
        });

        yield return new WaitUntil(() => meshToUse != null);

        if (newChunk != null)
        {
            MeshRenderer newChunkRenderer = newChunk.GetComponent<MeshRenderer>();
            MeshFilter newChunkFilter = newChunk.GetComponent<MeshFilter>();
            MeshCollider collider = newChunk.GetComponent<MeshCollider>();

            newChunkFilter.mesh = meshToUse;
            newChunkRenderer.material = ChunkMaterial;
            collider.sharedMesh = newChunkFilter.mesh;
            collider.material = ChunkPhysicMaterial;
        }
    }

    public void UpdateChunk(Vector2Int ChunkCoord)
    {
        if (ActiveChunks.ContainsKey(ChunkCoord))
        {
            Vector3Int DataCoords = new Vector3Int(ChunkCoord.x, 0, ChunkCoord.y);

            GameObject TargetChunk = ActiveChunks[ChunkCoord];
            MeshFilter targetFilter = TargetChunk.GetComponent<MeshFilter>();
            MeshCollider targetCollider = TargetChunk.GetComponent<MeshCollider>();

            StartCoroutine(meshCreator.CreateMeshFromData(WorldData[DataCoords], x =>
            {
                targetFilter.mesh = x;
                targetCollider.sharedMesh = x;
            }));
        }
    }

    public void SetBlock(Vector3Int WorldPosition, int BlockType = 0, bool destroy = false, GameObject special = null)
    {
        Vector2Int coords = GetChunkCoordsFromPosition(WorldPosition);
        Vector3Int DataPosition = new Vector3Int(coords.x, 0, coords.y);

        if (WorldData.ContainsKey(DataPosition))
        {
            Vector3Int coordsToChange = WorldToLocalCoords(WorldPosition, coords);

            if (destroy)
            {
                GameObject block = Instantiate(droppedItem, WorldPosition, Quaternion.Euler(new Vector3(0f, Random.rotation.y, 0f)));
                ItemBehavior item = block.GetComponent<ItemBehavior>();
                item.pos = WorldPosition;
                item.itemID = WorldData[DataPosition][coordsToChange.x, coordsToChange.y, coordsToChange.z];
            }

            if (!special) WorldData[DataPosition][coordsToChange.x, coordsToChange.y, coordsToChange.z] = BlockType;
            else Instantiate(special, WorldPosition, Quaternion.AngleAxis(FindObjectOfType<PlayerMovement>().transform.eulerAngles.y, Vector3.up));

            UpdateChunk(coords);
        }
    }

    public static Vector2Int GetChunkCoordsFromPosition(Vector3 WorldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(WorldPosition.x / ChunkSize.x),
            Mathf.FloorToInt(WorldPosition.z / ChunkSize.z)
        );
    }

    public static Vector3Int WorldToLocalCoords(Vector3Int WorldPosition, Vector2Int Coords)
    {
        return new Vector3Int
        {
            x = WorldPosition.x - Coords.x * ChunkSize.x,
            y = WorldPosition.y,
            z = WorldPosition.z - Coords.y * ChunkSize.z
        };
    }
}