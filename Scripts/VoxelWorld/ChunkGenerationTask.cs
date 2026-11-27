using Godot;
using System;
using System.Threading;

namespace VoxelWorld;
public sealed class ChunkGenerationTask
{
    private readonly Chunk _chunk;
    private readonly ChunkGenerator _generator;
    private long _taskId = -1;
    private Exception _exception;
    
    public Chunk chunk => _chunk;
    public bool IsStarted => _taskId >= 0;
    public bool IsCompleted => IsStarted && WorkerThreadPool.IsTaskCompleted(_taskId);

    public ChunkGenerationTask(Chunk chunk, ChunkGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(generator);

        _chunk = chunk;
        _generator = generator;
    }

    public void Start()
    {
        if (IsStarted)
            throw new InvalidOperationException("Task has arleady been started");

        _exception = null;

        _taskId = WorkerThreadPool.AddTask(Callable.From(Execute),false,$"Generate chunk {_chunk.Coordinates}");
    }

    private void Execute()
    {
        try
        {
            _generator.Generate(_chunk);
        }
        catch (Exception exception)
        {
            Volatile.Write(ref _exception,exception);
        }
    }

    public Exception Complete()
    {
        if(!IsStarted)
            throw new InvalidOperationException("Task was not started");
        
        if(!IsCompleted)
            throw new InvalidOperationException("Task is not completed yet.");

        Error result = WorkerThreadPool.WaitForTaskCompletion(_taskId);

        if(result != Error.Ok)
            throw new InvalidOperationException($"Failed to complete task: {result}");

        _taskId = -1;
        return Volatile.Read(ref _exception);
    }
}
