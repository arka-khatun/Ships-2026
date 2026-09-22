## Pathfinding Design


### Movement Cost


| Water Type | Cost | Description |
|---|---|---|
| High Seas | 1 | Wide open sea. Does not impede travel. |
| Open Water | 2 | Fine tuning the step from coasts to high seas. |
| Coast Water | 3 | Coast waters. A bit slower travel. |
| Current | 4 | A strong current. Notably slows down the ship. |
| Shallows | 5 | Shallows. Slow the ship down. |
| Reef | 7 | Reefs and small rocks. Extremely slow down the ship. |
| Whirlpool | 9 | A huge whirlpool. Slows the ship down almost to halt. |
| Land | -1 | Land. Ships do not sail on dry land. |
| Rock | -1 | A big rock. Large enough to block the access to the cell. |


### Grid Graph (Pseudocode)


```
enum WaterType:
    HIGH_SEAS, OPEN_WATER, COAST_WATER, CURRENT, SHALLOWS, REEF, WHIRLPOOL, LAND, ROCK

CostTable: Dictionary<WaterType, int> = {
    HIGH_SEAS:    1,
    OPEN_WATER:   2,
    COAST_WATER:  3,
    CURRENT:      4,
    SHALLOWS:     5,
    REEF:         7,
    WHIRLPOOL:    9,
    LAND:        -1,
    ROCK:        -1,
}

class GridNode:
    position: Vector2i
    cost: int                      # cost to ENTER this node; negative = non-navigable
    neighbors: List<GridNode>

    function IsNavigable() -> bool:
        return cost >= 0


class PathfindingGrid:
    nodes: Dictionary<Vector2i, GridNode>

    function BuildFromTileMap(tileMap: TileMapLayer):
        # 1. One GridNode per used cell, cost resolved through the lookup table.
        for cellPos in tileMap.get_used_cells():
            tileData  = tileMap.get_cell_tile_data(cellPos)
            waterType = tileData.get_custom_data("water_type")
            nodes[cellPos] = new GridNode(cellPos, CostTable[waterType])

        # 2. Link orthogonal neighbors that exist inside the map.
        directions = [Vector2i(0,-1), Vector2i(0,1), Vector2i(-1,0), Vector2i(1,0)]
        for node in nodes.values():
            for direction in directions:
                neighborPos = node.position + direction
                if nodes.contains_key(neighborPos):
                    node.neighbors.add(nodes[neighborPos])

    function ApplyHazardPatch(cells: List<Vector2i>, newCost: int):
        # Called when a moving Area2D hazard enters/leaves a cell, instead of
        # rebuilding the whole grid.
        for cellPos in cells:
            if nodes.contains_key(cellPos):
                nodes[cellPos].cost = newCost
```


### Pathfinding (Pseudocode)


```
struct PathEntry : IComparable<PathEntry>:
    node:     GridNode
    distance: float          # g: actual accumulated cost from the start
    priority: float          # f = g + heuristic (plain Dijkstra: priority == distance)

    function CompareTo(other: PathEntry) -> int:
        return priority.CompareTo(other.priority)


function FindPath(grid: PathfindingGrid, start: Vector2i, goal: Vector2i) -> List<Vector2i>:
    startNode = grid.nodes[start]

    frontier  = new PriorityQueue<PathEntry>()
    frontier.Enqueue(PathEntry(startNode, 0, Heuristic(start, goal)))

    cameFrom   = new Dictionary<GridNode, GridNode>()
    costSoFar  = new Dictionary<GridNode, float>()
    costSoFar[startNode] = 0

    while frontier.Count > 0:
        entry   = frontier.Dequeue()
        current = entry.node

        # Stale entry: a cheaper path to 'current' was already processed earlier.
        if entry.distance > costSoFar[current]:
            continue

        if current.position == goal:
            break

        for neighbor in current.neighbors:
            if not neighbor.IsNavigable():
                continue                          # skip land, rocks, etc.

            newCost = costSoFar[current] + neighbor.cost

            if neighbor not in costSoFar or newCost < costSoFar[neighbor]:
                costSoFar[neighbor] = newCost
                priority = newCost + Heuristic(neighbor.position, goal)
                frontier.Enqueue(PathEntry(neighbor, newCost, priority))
                cameFrom[neighbor] = current

    return ReconstructPath(cameFrom, startNode, grid.nodes[goal])


function Heuristic(a: Vector2i, b: Vector2i) -> float:
    # Manhattan distance for 4-directional movement.
    return abs(a.x - b.x) + abs(a.y - b.y)


function ReconstructPath(cameFrom: Dictionary<GridNode, GridNode>, start: GridNode, goal: GridNode) -> List<Vector2i>:
    path = []
    node = goal

    while node != start:
        path.insert(0, node.position)
        node = cameFrom[node]

    path.insert(0, start.position)
    return path
```
