using Godot;
using System;
using System.Collections.Generic;

public partial class ChunkManager : Node3D
{
    [Export]
    public ChunkGenerator chunkGenerator {get;set;}

    [Export(PropertyHint.Range, "0,16,1")]
    public int LoadRadius {get;set;} = 2;
    public int LoadedChunkCount => _chunks.Count;
    public Vector3I ViewerChunkCoordinates {get; private set;}
    public bool HasViewer {get; private set;}

    private readonly List<Vector3I> _chunksToRemove = new();
    private readonly Dictionary<Vector3I,Chunk> _chunks = new();
    private readonly Dictionary<Vector3I, MeshInstance3D> _chunkMeshes = new();
    private readonly HashSet<Vector3I> _chunksToReBuild = new();
    private StandardMaterial3D _chunkMaterial;

    private static readonly Vector3I[] _neighborOffset = 
    {
        new(1, 0, 0),
        new(-1, 0, 0),
        new(0, 1, 0),
        new(0, -1, 0),
        new(0, 0, 1),
        new(0, 0, -1)
    };

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

        _chunksToReBuild.Clear();

        RemoveChunkOutsideRange(radius);
        LoadChunkIsRange(radius);

        RebuildQueuedChunkMeshes();

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

            RemoveChunkMesh(coordinates);

            QueueChunkAndNeighbors(coordinates);
        }
    }

    private void LoadChunkIsRange(int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    Vector3I coordinates = new Vector3I(x,y,z) + ViewerChunkCoordinates;

                    if (!_chunks.ContainsKey(coordinates))
                    {
                        //AddChunk(new Chunk(coordinates));

                        Chunk chunk = new(coordinates);
                        chunkGenerator.Generate(chunk);
                        AddChunk(chunk);

                        QueueChunkAndNeighbors(coordinates);
                    }
                } 
            }   
        }
    }

    private Voxel GetVoxelAtWorldPosition(Vector3I worldPosition)
    {
        Vector3I chunkCoordinates = new(
            FloorDiv(worldPosition.X,Chunk.SIZE),
            FloorDiv(worldPosition.Y,Chunk.SIZE),
            FloorDiv(worldPosition.Z,Chunk.SIZE)
        );

        if (!_chunks.TryGetValue(chunkCoordinates, out Chunk chunk))
            return new Voxel(VoxelType.Air);
        
        Vector3I chunkOrigin = chunkCoordinates * Chunk.SIZE;

        Vector3I localPosition = worldPosition - chunkOrigin;

        return chunk.GetVoxel(localPosition);
    }

    private static int FloorDiv(int value, int divisor)
    {
        int quotient = value / divisor;
        int remainder = value % divisor;

        if (remainder < 0)
            quotient--;

        return quotient;
    }

    private void RebuildChunkMesh(Chunk chunk)
    {
        RemoveChunkMesh(chunk.Coordinates);

        ArrayMesh mesh = ChunkMesher.Build(chunk, GetVoxelAtWorldPosition);

        if (mesh.GetSurfaceCount() == 0)
            return;
        
        MeshInstance3D meshInstance = new()
        {
            Name = $"Chunk_{chunk.Coordinates.X}_{chunk.Coordinates.Y}_{chunk.Coordinates.Z}",
            Mesh = mesh,
            Position = new Vector3(chunk.Coordinates.X * Chunk.SIZE,
                                    chunk.Coordinates.Y * Chunk.SIZE,
                                    chunk.Coordinates.Z * Chunk.SIZE),
            MaterialOverride = _chunkMaterial
        };

        AddChild(meshInstance);

        _chunkMeshes.Add(chunk.Coordinates,meshInstance);
    }

    private void RemoveChunkMesh(Vector3I coordinates)
    {
        if (!_chunkMeshes.TryGetValue(coordinates, out MeshInstance3D meshInstance))
            return;
        
        _chunkMeshes.Remove(coordinates);

        meshInstance.QueueFree();
    }

    private void QueueChunkAndNeighbors(Vector3I coordinates)
    {
        _chunksToReBuild.Add(coordinates);

        foreach (Vector3I offset in _neighborOffset)
        {
            _chunksToReBuild.Add(coordinates + offset);
        }
    }

    private void RebuildQueuedChunkMeshes()
    {
        foreach (Vector3I coordinates in _chunksToReBuild)
        {
            if (!_chunks.TryGetValue(coordinates, out Chunk chunk))
                return;
            
            RebuildChunkMesh(chunk);
        }

        _chunksToReBuild.Clear();
    }

    public override void _Ready()
    {
        if (chunkGenerator == null)
        {
            GD.PushError("ChunkManager: Generator is not assigned.");
            SetProcess(false);
            return;
        }
        
        _chunkMaterial = new StandardMaterial3D
        {
            VertexColorUseAsAlbedo = true
        };
    }

}
