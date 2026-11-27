using Godot;
using System;

namespace VoxelWorld;
public sealed class Chunk
{
    public ChunkState state {get; private set;} = ChunkState.QueuedForGeneration;
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

    public void ChangeState(ChunkState newState)
    {
        if (!CanTransitionTo(newState))
        {
            throw new InvalidOperationException($"Invailid chunk state transition: {state} -> {newState}");
        }

        state = newState;
    }

    private bool CanTransitionTo(ChunkState nextState)
    {
        return (state, nextState) switch
        {
            (ChunkState.QueuedForGeneration, ChunkState.Generating) => true,
            (ChunkState.QueuedForGeneration, ChunkState.Failed) => true,

            (ChunkState.Generating, ChunkState.Generated) => true,
            (ChunkState.Generating, ChunkState.Failed) => true,

            (ChunkState.Generated, ChunkState.Meshing) => true,
            (ChunkState.Generated, ChunkState.Failed) => true,

            (ChunkState.Meshing, ChunkState.Ready) => true,
            (ChunkState.Meshing, ChunkState.Failed) => true,

            (ChunkState.Ready, ChunkState.Meshing) => true,
            (ChunkState.Ready, ChunkState.Failed) => true,

            (ChunkState.Failed, ChunkState.QueuedForGeneration) => true,

            _ => false
        };
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
