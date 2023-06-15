using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Seed")]
    [SerializeField] public int seed;

    [Header("Map Size")]
    [SerializeField] public Vector2Int mapSize;

    [Header("Map Offset")]
    [SerializeField] public Vector2 offset;

    [Header("Noise Values")]
    [SerializeField] public float noiseScale;
    [SerializeField] public int octaves;
    [SerializeField]
    public float
        persistance,
        lacunarity;

    internal float[,] map;

    public void GenerateMap()
    {
        map = Noise.GenerateNoise(mapSize, noiseScale, seed, octaves, persistance, lacunarity, offset);
    }
}