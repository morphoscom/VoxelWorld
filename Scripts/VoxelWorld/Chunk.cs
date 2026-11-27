using Godot;
using System;

public sealed class Chunk
{
    public const sbyte SIZE = 16;
    private readonly Voxel[] _voxels = new Voxel[SIZE * SIZE * SIZE];

    public Vector3I Coordinates {get;}
    
    public Chunk(Vector3I coordinates)
    {
        Coordinates = coordinates;
    }

    public Voxel GetVoxel(Vector3I localPosition)
    {
        int index = GetIndex(localPosition);
        return _voxels[index];
    }

    public void SetVoxel(Vector3I localPosition, Voxel voxel)
    {
        int index = GetIndex(localPosition);
        _voxels[index] = voxel;
    }

    private static int GetIndex(Vector3I position)
    {
        if (position.X < 0 || position.X >= SIZE ||
            position.Y < 0 || position.Y >= SIZE ||
            position.Z < 0 || position.Z >= SIZE)
        {
            throw new ArgumentOutOfRangeException(nameof(position),"Voxel position is outside chunk boudaries.");
        }
        
        return  position.X +
                position.Y * SIZE +
                position.Z * SIZE * SIZE;
    }
}
