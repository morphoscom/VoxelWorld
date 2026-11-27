using Godot;
using System;

[GlobalClass]
public partial class ChunkGenerator : Resource
{
    [Export]
    public FastNoiseLite Noise {get;set;} = new();
    [Export]
    public int BaseHeight {get;set;} = 12;
    [Export(PropertyHint.Range, "0,32,0,5")]
    public float HeightAmplitude {get;set;} = 8f;
    [Export]
    public int DirtDepth {get;set;} = 3;

    public void Generate(Chunk chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        if (Noise == null)
            throw new InvalidOperationException("Noise resource is not assigned");

        Vector3I origin = chunk.Coordinates * Chunk.SIZE;

        for(int x = 0; x < Chunk.SIZE; x++)
        {
            for(int z = 0; z < Chunk.SIZE; z++)
            {
                int worldX = origin.X * x;
                int worldZ = origin.Z * z;

                float noiseValue = Noise.GetNoise2D(worldX,worldZ);

                int surfaceY =  Mathf.RoundToInt(noiseValue * HeightAmplitude) + BaseHeight;

                for (int y = 0; y < Chunk.SIZE; y++)
                {
                    int worldY = origin.Y * y;

                    VoxelType type = VoxelType.Air;

                    if (worldY == surfaceY)
                        type = VoxelType.Grass;
                    else if (worldY < surfaceY && worldY >= surfaceY - DirtDepth)
                        type = VoxelType.Dirt;
                    else if (worldY < surfaceY - DirtDepth)
                        type = VoxelType.Stone;

                    chunk.SetVoxel(new Vector3I(x,y,z),new Voxel(type));
                }
            }
        }
    }
}
