using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Seed")]
    [SerializeField] public int seed;

    [Header("Map Size")]
    [SerializeField] public Vector2Int mapSize = new Vector2Int(100, 100);

    [Header("Map Offset")]
    [SerializeField] public Vector2 offset =  new Vector2(0f, 0f);

    [Header("Noise Values")]
    [SerializeField] public float noiseScale = 100;
    [SerializeField] public int octaves = 10;
    [SerializeField]
    public float
        persistance = 0.49f,
        lacunarity = 2.34f;

    internal float[,] map;

    public void GenerateMap()
    {
        map = Noise.GenerateNoise(mapSize, noiseScale, seed, octaves, persistance, lacunarity, offset);
    }
}