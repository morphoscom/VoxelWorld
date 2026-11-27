using Godot;
using System;
using System.Collections.Generic;

public partial class ChunkManager : Node3D
{
    public Vector3I ViewerChunkCoordinates {get; private set;}
    public bool HasViewer {get; private set;}



    private readonly Dictionary<Vector3I,Chunk> _chunks = new();

    public void UpdateViewerChunk(Vector3I coordinates)
    {
        if (HasViewer && ViewerChunkCoordinates == coordinates)
            return;

        ViewerChunkCoordinates = coordinates;
        HasViewer = true;

        GD.Print($"Viewer entered chunk: {coordinates}");
    }

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
