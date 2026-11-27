using Godot;
using System;
using System.Collections.Generic;

public partial class ChunkManager : Node3D
{
    [Export(PropertyHint.Range, "0,16,1")]
    public int LoadRadius {get;set;} = 2;
    public int LoadedChunkCount => _chunks.Count;
    public Vector3I ViewerChunkCoordinates {get; private set;}
    public bool HasViewer {get; private set;}

    private readonly List<Vector3I> _chunksToRemove = new();
    private readonly Dictionary<Vector3I,Chunk> _chunks = new();

    public void UpdateViewerChunk(Vector3I coordinates)
    {
        if (HasViewer && ViewerChunkCoordinates == coordinates)
            return;

        ViewerChunkCoordinates = coordinates;
        HasViewer = true;

        GD.Print($"Viewer entered chunk: {coordinates}");

        UpdateLoadedChunk();
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

    private void UpdateLoadedChunk()
    {
        int radius = Math.Max(0,LoadRadius);

        RemoveChunkOutsideRange(radius);
        LoadChunkIsRange(radius);

        GD.Print($"Loaded chunks: {_chunks.Count}");
    }

    private void RemoveChunkOutsideRange(int radius)
    {
        _chunksToRemove.Clear();

        foreach(Vector3I coordinates in _chunks.Keys)
        {
            Vector3I offSet = coordinates - ViewerChunkCoordinates;

            if (Math.Abs(offSet.X) > radius ||
                Math.Abs(offSet.Y) > radius ||
                Math.Abs(offSet.Z) > radius)
            {
                _chunksToRemove.Add(coordinates);
            }
        }

        foreach(Vector3I coordinates in _chunksToRemove)
        {
            _chunks.Remove(coordinates);
        }
    }

    private void LoadChunkIsRange(int radius)
    {
        for (int x = -radius; x < radius; x++)
        {
            for (int y = -radius; y < radius; y++)
            {
                for (int z = -radius; z < radius; z++)
                {
                    Vector3I coordinates = new Vector3I(x,y,z) + ViewerChunkCoordinates;

                    if (!_chunks.ContainsKey(coordinates))
                    {
                        AddChunk(new Chunk(coordinates));
                    }
                } 
            }   
        }
    }
}
