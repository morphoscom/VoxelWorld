using Godot;
using System;

namespace VoxelWorld;
public partial class ChunkMesher
{
    private readonly struct Face
    {
        public Vector3I Direction {get;}
        public Vector3 A {get;}
        public Vector3 B {get;}
        public Vector3 C {get;}
        public Vector3 D {get;}
        public Vector3 Normal {get;}

        public Face(Vector3I direction, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            Direction = direction;
            A = a;
            B = b;
            C = c;
            D = d;
            Normal = normal;
        }
    }
    private static readonly Face[] _faces =
    {
        // Right (+X)
        new(
            new Vector3I(1, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 1, 1),
            new Vector3(1, 0, 1),
            Vector3.Right
        ),

        // Left (-X)
        new(
            new Vector3I(-1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 1),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 0),
            Vector3.Left
        ),

        // Top (+Y)
        new(
            new Vector3I(0, 1, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, 1, 0),
            Vector3.Up
        ),

        // Bottom (-Y)
        new(
            new Vector3I(0, -1, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            Vector3.Down
        ),

        // Front (+Z)
        new(
            new Vector3I(0, 0, 1),
            new Vector3(1, 0, 1),
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1),
            new Vector3(0, 0, 1),
            Vector3.Back
        ),

        // Back (-Z)
        new(
            new Vector3I(0, 0, -1),
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 0, 0),
            Vector3.Forward
        )
    };

    public static ArrayMesh Build(Chunk chunk, Func<Vector3I,Voxel> getVoxelAtWorldPosition)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(getVoxelAtWorldPosition);

        SurfaceTool surface = new();
        surface.Begin(Mesh.PrimitiveType.Triangles);

        Vector3I chunkOrigin = chunk.Coordinates * Chunk.SIZE;

        bool hasGeometry = false;

        for(int x = 0; x < Chunk.SIZE; x++)
        {
            for(int y = 0; y < Chunk.SIZE; y++)
            {
                for(int z = 0; z < Chunk.SIZE; z++)
                {
                    Vector3I localPosition = new(x,y,z);

                    Voxel voxel = chunk.GetVoxel(localPosition);

                    if (voxel.IsEmpty)
                        continue;

                    Vector3I worldPosition = chunkOrigin + localPosition;

                    foreach(Face face in _faces)
                    {
                        Vector3I neighborPosition = worldPosition + face.Direction;

                        Voxel neighbor = getVoxelAtWorldPosition(neighborPosition);

                        if(!neighbor.IsEmpty)
                            continue;

                        Color color = GetFaceColor(voxel.Type, face.Normal);

                        AddFace(surface, new Vector3(x,y,z), face, color);

                        hasGeometry = true;
                    }
                }
            }
        }
        if (!hasGeometry)
            return new ArrayMesh();
    
        return surface.Commit();
    }
    
    private static void AddFace(SurfaceTool surface, Vector3 origin, Face face, Color color)
    {
        // First triangle
        AddVertex(surface, origin + face.A, face.Normal, color);
        AddVertex(surface, origin + face.C, face.Normal, color);
        AddVertex(surface, origin + face.B, face.Normal, color);

        // Second triangle
        AddVertex(surface, origin + face.A, face.Normal, color);
        AddVertex(surface, origin + face.D, face.Normal, color);
        AddVertex(surface, origin + face.C, face.Normal, color);

    }

    private static void AddVertex(SurfaceTool surface, Vector3 position, Vector3 normal, Color color)
    {
        surface.SetNormal(normal);
        surface.SetColor(color);
        surface.AddVertex(position);
    }

    public static Color GetFaceColor(VoxelType type, Vector3 normal)
    {
        return type switch
        {
            VoxelType.Grass when normal == Vector3.Up =>
                new Color(0.25f, 0.65f, 0.18f),

            VoxelType.Grass =>
                new Color(0.40f, 0.29f, 0.17f),

            VoxelType.Dirt =>
                new Color(0.45f, 0.27f, 0.13f),

            VoxelType.Stone =>
                new Color(0.48f, 0.48f, 0.50f),

            _ => Colors.Magenta
        };
    }
}
