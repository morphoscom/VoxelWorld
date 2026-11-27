using Godot;
using System;

public partial class WorldViewer : Node3D
{
    [Export]
    public ChunkManager ChunkManager {get; set;} = null;

    private Vector3I _lastChunkCoordinates;
    private bool _hasLastChunkCoordinates = false;
    private Camera3D camera3D;

    public override void _Ready()
    {
        camera3D = GetViewport().GetCamera3D();
        ArgumentNullException.ThrowIfNull(camera3D);

        if (ChunkManager == null)
        {
            GD.PushError("WorldViewer: ChunkManager is not assigned.");
            SetProcess(false);
            return;
        }

        UpdateChunkCoordinates();
    }

    public override void _Process(double delta)
    {
        UpdateChunkCoordinates();
    }

    private void UpdateChunkCoordinates()
    {
        Vector3 localPosition = ChunkManager.ToLocal(camera3D.GetParentNode3D().GlobalPosition);

        Vector3I chunkCoordinates = new(Mathf.FloorToInt(localPosition.X / Chunk.SIZE),
                                        Mathf.FloorToInt(localPosition.Y / Chunk.SIZE),
                                        Mathf.FloorToInt(localPosition.Z / Chunk.SIZE));

        if (_hasLastChunkCoordinates && chunkCoordinates == _lastChunkCoordinates)
        {
            return;
        }

        _lastChunkCoordinates = chunkCoordinates;
        _hasLastChunkCoordinates = true;

        ChunkManager.UpdateViewerChunk(chunkCoordinates);

    }
}
