using Godot;
using System;

public readonly struct Voxel
{
    public short Type {get;} = 0;
    public bool IsEmpty => Type == 0;
    public Voxel(short type)
    {
        Type = type;
    }
}
