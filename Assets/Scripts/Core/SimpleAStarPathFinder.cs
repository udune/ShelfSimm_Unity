using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public class SimpleAStarPathFinder : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int gridWidth = 50;
        public int gridHeight = 50;

        private HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>();

        private class Node
        {
            public Vector2Int position;
            public int g;
            public int h;
            public int f => g + h;
            public Node parent;
        }

        private int CalculateDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        public void AddObstacle(Vector2Int pos)
        {
            obstacles.Add(pos);
        }

        public void RemoveObstacle(Vector2Int pos)
        {
            obstacles.Remove(pos);
        }

        private bool IsWalkable(Vector2Int pos)
        {
            if (pos.x < 0 || pos.x >= gridWidth || pos.y < 0 || pos.y >= gridHeight)
                return false;
            return !obstacles.Contains(pos);
        }

        private readonly Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        private List<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            var neighbors = new List<Vector2Int>();
            foreach (var dir in directions)
            {
                var neighbor = pos + dir;
                if (IsWalkable(neighbor))
                    neighbors.Add(neighbor);
            }
            return neighbors;
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            if (!IsWalkable(start) || !IsWalkable(goal))
            {
                Debug.LogWarning($"Invalid position: start({start.x},{start.y}) goal({goal.x},{goal.y})");
                return null;
            }

            if (start == goal)
                return new List<Vector2Int> { start };

            var openList = new List<Node>();
            var closedSet = new HashSet<Vector2Int>();
            var allNodes = new Dictionary<Vector2Int, Node>();

            var startNode = new Node
            {
                position = start,
                g = 0,
                h = CalculateDistance(start, goal),
                parent = null
            };

            openList.Add(startNode);
            allNodes[start] = startNode;

            while (openList.Count > 0)
            {
                var current = openList.OrderBy(n => n.f).ThenBy(n => n.h).First();

                if (current.position == goal)
                    return BuildPath(current);

                openList.Remove(current);
                closedSet.Add(current.position);

                foreach (var neighborPos in GetNeighbors(current.position))
                {
                    if (closedSet.Contains(neighborPos))
                        continue;

                    int tentativeG = current.g + 1;

                    if (!allNodes.ContainsKey(neighborPos))
                    {
                        var neighborNode = new Node
                        {
                            position = neighborPos,
                            g = tentativeG,
                            h = CalculateDistance(neighborPos, goal),
                            parent = current
                        };
                        allNodes[neighborPos] = neighborNode;
                        openList.Add(neighborNode);
                    }
                    else if (tentativeG < allNodes[neighborPos].g)
                    {
                        var neighborNode = allNodes[neighborPos];
                        neighborNode.g = tentativeG;
                        neighborNode.parent = current;
                    }
                }
            }

            Debug.LogWarning($"Path not found: start({start.x},{start.y}) goal({goal.x},{goal.y})");
            return null;
        }

        private List<Vector2Int> BuildPath(Node endNode)
        {
            var path = new List<Vector2Int>();
            var current = endNode;

            while (current != null)
            {
                path.Add(current.position);
                current = current.parent;
            }

            path.Reverse();
            return path;
        }
    }
}
