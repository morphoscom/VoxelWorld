using Godot;
using System;
using System.Collections.Generic;

public partial class Chunk
{
    private Vector3I _position = new Vector3I();
    private Voxel[] _voxels = new Voxel[16*16*16];

    public Chunk(Vector3I position)
    {
        _position = position;
    }

    public void SetVoxels(Voxel[] voxels)
    {
        _voxels = voxels;
    }
}
