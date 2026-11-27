---
date: 2026-11-27
tags:
  - Godot
  - CSharp
---
### Информация и представления об VoxelWorld системе

---
#### Файлы:
* Chunk.cs - Структура чанков
* ChunkGenerator.cs - Генерация вокселей из шумов
* ChunkManager.cs - управления чанками
* ChunkMesher.cs - Создание и удаления мешей чанков
* Voxel.cs - структура вокселей
* VoxelType.cs - Enum типов вокселей(например: воздух, трава, камень, грязь)
* WorldViewer.cs - Наблюдатель сцена за таргет

---
### Асинхронная работа ChunkManager

#### Жизненный цикл Chunk
```mermaid
flowchart TD
F[Failed] --- A
A["Queued - чанк ожидает генерации"] --> B["Generating - создаются данные вокселей"]
B --> C["Generated - данные готовы"]
C --> D["Meshing - создаётся геометрия"]
D --> E["Ready - чанк отображается в сцене"]
```
> [!warning] Из любого рабочего состояния также возможен переход в Failed при ошибке.

---
#### Работа класса ChunkManager:
```mermaid
flowchart TB

    n1["WorldViewer.UpdateChunkCoordinates()"] --> n2["ChunkManager.UpdateViewerChunk()"]

    n2 --> n3@{ label: "<div style=\"color:\"><span style=\"color:\">UpdateLoadedChunk</span><span style=\"color:\">()</span></div>" }

    n3 --- n20@{ label: "<div style=\"color:\"><div><span style=\"color:\">Очищаем список чанков на ребилд<br/>    </span><span style=\"color:\">_chunksToReBuild</span><span style=\"color:\">.</span><span style=\"color:\">Clear</span><span style=\"color:\">()</span></div></div>" }

    n4@{ label: "<div style=\"color:\"><div><span style=\"color:\">    </span><span style=\"color:\">RemoveChunkOutsideRange</span><span style=\"color:\">(</span><span style=\"color:\">radius</span><span style=\"color:\">)</span></div><div><span style=\"color:\"></span></div></div>" } --> n5@{ label: "<div style=\"color:\"><div><span style=\"color:\">    </span><span style=\"color:\">LoadChunkIsRange</span><span style=\"color:\">(</span><span style=\"color:\">radius</span><span style=\"color:\">)</span></div></div>" }

    n4 --- n7@{ label: "<span style=\"color:\"></span><span style=\"color:\">Очищаем список чанков на удаления<br>_chunksToRemove</span><span style=\"color:\">.</span><span style=\"color:\">Clear</span><span style=\"color:\">()<br></span>Пополняем список чанков на удаления новыми данными<br><span style=\"color:\">_chunksToRemove</span>.Add()<br>" }

    n8@{ label: "Добавляем новые чанки вошедшие в радиус в список _chunks<br><span style=\"--tw-border-spacing-x:\">AddChunk</span><span style=\"--tw-border-spacing-x:\">(</span><span style=\"--tw-border-spacing-x:\">chunk</span><span style=\"--tw-border-spacing-x:\">)</span>" } --> n11["QueueChunkAndNeighbors()<br>"]

    n11 --- n10@{ label: "<span style=\"--tw-border-spacing-x:\"></span>Добавляем новые чанки в список на ребилд а также соседей<br><span style=\"color:\">_chunksToReBuild</span><span style=\"color:\">.</span><span style=\"color:\">Add</span>" }

    n7 --> n12@{ label: "<div style=\"color:\"><span style=\"color:\">RemoveChunkMesh()</span></div>" }

    n13@{ label: "Удаляем меш из списка мешов<br><div style=\"color:\"><span style=\"color:\">_chunkMeshes</span><span style=\"color:\">.</span><span style=\"color:\">Remove()</span></div>" } --> n14@{ label: "<span style=\"color:\">Удаляем инстанс меша из сцены<br>meshInstance</span><span style=\"color:\">.</span><span style=\"color:\">QueueFree</span><span style=\"color:\">();</span>" }

    n12 --> n15@{ label: "<span style=\"color:\">      </span><span style=\"color:\">QueueChunkAndNeighbors</span><span style=\"color:\">(</span><span style=\"color:\">coordinates</span><span style=\"color:\">)</span>" }

    n5 --> n16@{ label: "<span style=\"font-family:\">RebuildQueuedChunkMeshes</span><span style=\"color:\">()</span>" }

    n5 --- n8

    n12 --- n13

    n16 --- n17@{ label: "<div style=\"color:\"><span style=\"color:\">Ребилд мешей чанков с Face culling<br/>RebuildChunkMesh()</span></div>" }

    n17 --> n18@{ label: "<span style=\"color:\"></span><span style=\"color:\">Очищаем список чанков на ребилд<br>_chunksToReBuild</span><span style=\"color:\">.</span><span style=\"color:\">Clear</span><span style=\"color:\">()</span><span style=\"color:\"></span>" }

    n15 --> n19@{ label: "Добавляем чанки в список на ребил а также соседей<br><span style=\"color:\">_chunksToReBuild</span><span style=\"color:\">.</span><span style=\"color:\">Add</span><span style=\"color:\">(</span><span style=\"color:\">)</span>" }

    n20 --> n4

  

    n1@{ shape: rect}

    n2@{ shape: rect}

    n3@{ shape: rect}

    n20@{ shape: rect}

    n4@{ shape: rect}

    n5@{ shape: rect}

    n7@{ shape: rect}

    n8@{ shape: rect}

    n10@{ shape: rect}

    n12@{ shape: rect}

    n13@{ shape: rect}

    n14@{ shape: rect}

    n15@{ shape: rect}

    n16@{ shape: rect}

    n17@{ shape: rect}

    n18@{ shape: rect}

    n19@{ shape: rect}

    linkStyle 0 stroke:#00C853,fill:none

    linkStyle 1 stroke:#00C853,fill:none

    linkStyle 2 stroke:#00C853

    linkStyle 3 stroke:#00C853,fill:none

    linkStyle 10 stroke:#00C853,fill:none

    linkStyle 16 stroke:#00C853,fill:none
```

---