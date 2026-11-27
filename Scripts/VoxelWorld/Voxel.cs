using Godot;
using System;
namespace VoxelWorld;
public readonly struct Voxel
{
    public VoxelType Type {get;} = 0;
    public bool IsEmpty => Type == VoxelType.Air;
    public Voxel(VoxelType type)
    {
        Type = type;
    }
}
