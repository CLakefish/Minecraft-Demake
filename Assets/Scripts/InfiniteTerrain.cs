using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private int RenderDistance;
    private WorldGenerator worldGenerator;
    private List<Vector2Int> CoordsToRemove;

    // Start is called before the first frame update
    void Start()
    {
        worldGenerator = FindObjectOfType<WorldGenerator>();
        CoordsToRemove = new List<Vector2Int>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2Int playerPos = new
        (
            (int)player.position.x / WorldGenerator.ChunkSize.x,
            (int)player.position.z / WorldGenerator.ChunkSize.z
        );

        CoordsToRemove.Clear();

        foreach (KeyValuePair<Vector2Int, GameObject> active in WorldGenerator.ActiveChunks)
        {
            CoordsToRemove.Add(active.Key);
        }

        for (int x = playerPos.x - RenderDistance; x <= playerPos.x + RenderDistance; x++)
        {

            for (int y = playerPos.y - RenderDistance; y <= playerPos.y + RenderDistance; y++)
            {
                Vector2Int chunkCord = new Vector2Int(x, y);

                if (!WorldGenerator.ActiveChunks.ContainsKey(chunkCord))
                {
                    StartCoroutine(worldGenerator.CreateChunk(chunkCord));
                }

                CoordsToRemove.Remove(chunkCord);
            }
        }

        foreach (Vector2Int pos in CoordsToRemove)
        {
            GameObject chunkToDelete = WorldGenerator.ActiveChunks[pos];
            WorldGenerator.ActiveChunks.Remove(pos);
            Destroy(chunkToDelete);
        }
    }
}