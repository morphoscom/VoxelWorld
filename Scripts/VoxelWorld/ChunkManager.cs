using Godot;
using System;
using System.Collections.Generic;

public partial class ChunkManager : Node
{
    private readonly Dictionary<Vector3I,Chunk> _chunks = new();

    public void AddChunk(Chunk chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        if(!_chunks.TryAdd(chunk.Coordinates,chunk))
        {
            throw new InvalidOperationException($"Chunk at {chunk.Coordinates} arleady exist");
        }
    }

    public bool TryGetChunk(Vector3I coordinates, out Chunk chunk)
    {
        return _chunks.TryGetValue(coordinates,out chunk);
    }

    public bool RemoveChunk(Vector3I coordinates)
    {
        return _chunks.Remove(coordinates);
    }
}
