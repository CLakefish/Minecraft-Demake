using System.Collections;
using System.Collections.Generic; using System.Threading.Tasks;
using UnityEngine;

public class DataGenerator
{
    public class GenData
    {
        public System.Action<int[,,]> OnComplete;
        public Vector3Int GenerationPoint;
        public Biome biome;
        public MapGenerator Map;
        public AnimationCurve continentalness;
    }

    private WorldGenerator GeneratorInstance;
    private Queue<GenData> DataToGenerate;
    public bool Terminate;

    private StructureGenerator structureGen;
    public DataGenerator(WorldGenerator worldGen, StructureGenerator structureGen = null)
    {
        GeneratorInstance = worldGen;
        DataToGenerate = new Queue<GenData>();
        this.structureGen = structureGen;

        worldGen.StartCoroutine(DataGenLoop());
    }

    public void QueueDataToGenerate(GenData data)
    {
        DataToGenerate.Enqueue(data);
    }

    public IEnumerator DataGenLoop()
    {
        while (Terminate == false)
        {
            if (DataToGenerate.Count > 0)
            {
                GenData gen = DataToGenerate.Dequeue();
                yield return GeneratorInstance.StartCoroutine(GenerateData(gen.GenerationPoint, gen.biome, gen.continentalness, gen.Map, gen.OnComplete));
            }

            yield return null;
        }
    }

    public IEnumerator GenerateData(Vector3Int offset, Biome biome, AnimationCurve continentalness, MapGenerator mapGen, System.Action<int[,,]> callback)
    {
        Vector3Int ChunkSize = WorldGenerator.ChunkSize;
        Vector2 NoiseOffset = GeneratorInstance.NoiseOffset;
        Vector2 NoiseScale = GeneratorInstance.NoiseScale;

        float HeightIntensity = GeneratorInstance.HeightIntensity;
        float HeightOffset = GeneratorInstance.HeightOffset;
        int WaterOffset = GeneratorInstance.WaterOffset;

        int[,,] TempData = new int[ChunkSize.x, ChunkSize.y, ChunkSize.z];

        if (WorldGenerator.AdditiveWorldData.TryGetValue(new Vector2Int(offset.x, offset.z), out int[,,] addedData))
        { // new
            TempData = addedData;
            WorldGenerator.AdditiveWorldData.Remove(new Vector2Int(offset.x, offset.z));
        }

        Task t = Task.Factory.StartNew(delegate
        {
            for (int x = 0; x < ChunkSize.x; x++)
            {
                for (int z = 0; z < ChunkSize.z; z++)
                {
                    int HeightGen = Mathf.RoundToInt(continentalness.Evaluate(mapGen.map[x, z]));

                    //float PerlinCoordX = NoiseOffset.x + (x + (offset.x * 16f)) / ChunkSize.x * NoiseScale.x;
                    //float PerlinCoordY = NoiseOffset.y + (z + (offset.z * 16f)) / ChunkSize.z * NoiseScale.y;
                    //int HeightGen = Mathf.RoundToInt(Mathf.PerlinNoise(PerlinCoordX, PerlinCoordY) * HeightIntensity + HeightOffset);

                    //if (Mathf.PerlinNoise(PerlinCoordX * 0.48f, PerlinCoordY * 0.48f) <= .4f)
                    //{
                    //    biome = Biome.Desert;
                    //}
                    //else if (Mathf.PerlinNoise(PerlinCoordX * 0.6f, PerlinCoordY * 0.6f) <= .3f && HeightGen >= 90)
                    //{
                    //    biome = Biome.Mountains;
                    //}
                    //else biome = Biome.Plains;

                    Debug.Log(biome);

                    for (int y = HeightGen; y >= 0; y--)
                    {

                        int BlockTypeToAssign = 0;

                        //Set everything at height 0 to bedrock.

                        if (WaterOffset - HeightGen > 0 && y >= HeightGen - 2)
                        {
                            BlockTypeToAssign = 9;

                            if (WaterOffset - HeightGen - 2 > 0)
                            {
                                for (int w = y + 1; w < WaterOffset - 1; w++)
                                {
                                    BlockTypeToAssign = 8;
                                    TempData[x, w, z] = BlockTypeToAssign;
                                }
                            }
                        }
                        else
                        {
                            switch (biome)
                            {
                                case (Biome.Plains):
                                    BlockTypeToAssign = 1;

                                    //Set next 3 layers to dirt
                                    if (y < HeightGen && y > HeightGen - 4) BlockTypeToAssign = 2;

                                    // Set everything else to stone
                                    if (y <= HeightGen - 4 && y > 0) BlockTypeToAssign = 3;

                                    break;

                                case (Biome.Desert):

                                    //Set next 3 layers to sand
                                    if (y <= HeightGen && y > HeightGen - 4) BlockTypeToAssign = 9;

                                    //Set everything else to stone
                                    if (y <= HeightGen - 4 && y > 0) BlockTypeToAssign = 3;

                                    break;

                                case (Biome.Ocean):
                                    BlockTypeToAssign = 9;

                                    break;

                                case (Biome.Mountains):
                                    if (y >= 120) BlockTypeToAssign = 9;
                                    else if (y == HeightGen) BlockTypeToAssign = 3;
                                    else BlockTypeToAssign = 2;

                                    if (y <= HeightGen - 4 && y > 0) BlockTypeToAssign = 3;

                                    break;
                            }
                        }

                        //if (BlockTypeToAssign == 3)
                        //{
                        //    if (Mathf.PerlinNoise(PerlinCoordX * 10 + y, PerlinCoordY * 10 + y) <= .25f)
                        //    {
                        //        BlockTypeToAssign = 10;
                        //    }
                        //    if (Mathf.PerlinNoise(PerlinCoordX * 11 * y, PerlinCoordY * 11 * y) <= .25f)
                        //    {
                        //        BlockTypeToAssign = 11;
                        //    }
                        //}

                        if (y < HeightGen - 6 && Noise.Perlin3D((x + offset.x * 16f) * .1f, (y + offset.y * 16f) * .1f, (z + offset.z * 16f) * .1f) >= .5f) BlockTypeToAssign = 0;

                        if (BlockTypeToAssign == 3)
                        {
                            if (Noise.Perlin3D((x + offset.x * 16f) * .22f, (y + offset.y * 16f) * .22f, (z + offset.z * 16f) * .22f) <= .29f) BlockTypeToAssign = 10;

                            if (Noise.Perlin3D((x + offset.x * 16f) * .3f, (y + offset.y * 16f) * .3f, (z + offset.z * 16f) * .3f) >= .6f) BlockTypeToAssign = 11;
                        }

                        if (y == 0) BlockTypeToAssign = 4;

                        TempData[x, y, z] = BlockTypeToAssign;
                    }

                    if (structureGen != null)
                    {
                        structureGen.GenerateStructure(new Vector2Int(offset.x, offset.z), ref TempData, x, z);
                    }
                }
            }
        });

        yield return new WaitUntil(() => {
            return t.IsCompleted || t.IsCanceled;
        });

        if (t.Exception != null)
            Debug.LogError(t.Exception);

        if(!WorldGenerator.WorldData.ContainsKey(offset)) WorldGenerator.WorldData.Add(offset, TempData);
        callback(TempData);
    }
}
