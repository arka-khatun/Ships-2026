## BFS Design


### Current state


- `Pathfinder` uses BFS with 4-directional (axial) movement. `GetNeighbours` is called with `includeDiagonal: false`.
- Diagonals are doubly switched off: `GetNeighbours` uses `includeDiagonal && PathfindingConfig.AllowDiagonalPathfinding`, and the config constant is `false`. Flipping only the config does nothing for BFS, because BFS passes `false` itself.
- BFS counts steps, not cost. Every step is 1 and `Cell.Cost` is ignored. This is fine for axial movement, where every step is the same real distance (one cell edge).

Everything below follows from one fact: **a diagonal step is about 1.41 times longer than an axial step in the real world, but BFS counts both as 1.**


### 1. The movement range changes shape


With axial movement, N steps is a diamond. With diagonal steps at cost 1 it becomes a square. Numbers for an open grid:

| N | Axial (4) | Diagonal (8, cost 1) | Diagonal (8, cost 10/14) |
|--:|----------:|---------------------:|-------------------------:|
| 1 |         4 |                    8 |                        4 |
| 2 |        12 |                   24 |                       12 |
| 3 |        24 |                   48 |                       28 |
| 5 |        60 |                  120 |                       72 |

- The square roughly doubles the number of reachable cells for the same N. Movement range, and with it game balance, changes a lot just by flipping a switch.
- The corners of the square are 5 cells away in each axis for N = 5. That is 7.05 world units, while the edge is 5.00 units away. The ship moves 41 % further when it goes diagonally, for the same step count.


### 2. Fewest steps is no longer shortest path (zigzag)


BFS returns a path with the fewest steps. When diagonals cost 1, a path that wanders sideways costs exactly the same as a straight line:

- 8 cells east in a straight line: 8 steps, 8.00 units.
- 8 cells east as NE, SE, NE, SE...: 8 steps, 11.28 units. BFS sees these as equal.

Among all equal paths, BFS keeps whichever it discovers first. That is decided by the loop order in `GetNeighbours` (X outer, Y inner), not by anything intentional. This means that the ship can visibly zigzag across the whole trip, which looks broken in a game.

This is the classic reason many games do not give diagonals the same cost as axial moves.


### 3. Corner cutting and leaking through objects


`GetNeighbours` only checks that the destination cell is walkable. A diagonal step between two non-walkable cells squeezes through a gap that has no width:

For `GetReachableCells` this is worse than for a single path: the movement range would highlight cells behind an obstacle that looks solid, for example two rocks placed diagonally. The usual fix is a corner-cutting rule: a diagonal step is allowed only if both axial cells next to it are walkable.


### 4. More work per search


Each cell has up to 8 neighbours instead of 4, so the number of cells within N steps is doubled: at 10 steps . A big movement range costs about twice as much time and memory. It is not a problem now, but it matters for large grids or many ships.


### 5. Movement costs


Once `Cell.Cost` is used (water types, currents, reefs), the diagonal factor has to combine with it: entering a reef diagonally should cost 1.41 x 7, not 7. BFS cannot do this at all.