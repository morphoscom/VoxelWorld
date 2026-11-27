namespace VoxelWorld;
public enum ChunkState
{
    QueuedForGeneration,
    Generating,
    Generated,
    Meshing,
    Ready,
    Failed
}
