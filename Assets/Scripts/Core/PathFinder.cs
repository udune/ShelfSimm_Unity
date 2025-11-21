using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public static class PathFinder
    {
        public class Node
        {
            public Vector2Int pos;
            public float g;
            public float h;
            public float f => g + h;
            public Node parent;
        }

        public static List<Vector2Int> FindPath(
            Vector2Int start,
            Vector2Int goal,
            HashSet<Vector2Int> obstacles,
            int maxWidth,
            int maxHeight)
        {
            if (start == goal)
            {
                return new List<Vector2Int>() { start };
            }

            if (obstacles.Contains(goal))
            {
                return null;
            }

            List<Node> openList = new List<Node>();
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

            Node startNode = new Node
            {
                pos = start,
                g = 0,
                h = Heuristic(start, goal),
                parent = null
            };
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                Node current = GetLowestF(openList);
                openList.Remove(current);

                if (current.pos == goal)
                {
                    return ReconstructPath(current);
                }

                closedSet.Add(current.pos);

                Vector2Int[] neighbors = new Vector2Int[]
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, -1),
                    new Vector2Int(-1, 0)
                };

                foreach (var dir in neighbors)
                {
                    Vector2Int neighborPos = current.pos + dir;

                    if (neighborPos.x < 0 || neighborPos.x >= maxWidth ||
                        neighborPos.y < 0 || neighborPos.y >= maxHeight)
                    {
                        continue;
                    }

                    if (obstacles.Contains(neighborPos))
                    {
                        continue;
                    }

                    if (closedSet.Contains(neighborPos))
                    {
                        continue;
                    }

                    float tentativeG = current.g + 1;

                    Node existingNode = openList.Find(x => x.pos == neighborPos);
                    if (existingNode == null)
                    {
                        Node newNode = new Node
                        {
                            pos = neighborPos,
                            g = tentativeG,
                            h = Heuristic(neighborPos, goal),
                            parent = current
                        };
                        openList.Add(newNode);
                    }
                    else if (tentativeG < existingNode.g)
                    {
                        existingNode.g = tentativeG;
                        existingNode.parent = current;
                    }
                }
            }

            return null;
        }

        private static float Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static Node GetLowestF(List<Node> nodes)
        {
            Node lowest = nodes[0];
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].f < lowest.f)
                {
                    lowest = nodes[i];
                }
            }

            return lowest;
        }

        private static List<Vector2Int> ReconstructPath(Node endNode)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Node current = endNode;
            while (current != null)
            {
                path.Add(current.pos);
                current = current.parent;
            }
            path.Reverse();
            return path;
        }
    }
}