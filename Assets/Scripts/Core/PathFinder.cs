using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Node used for a* algo calculation
    private class Node
    {
        public Vector2 position;
        public Node parent;
        public float gCost; // Distance from start
        public float hCost; // Distance to end
        public float fCost => gCost + hCost;

        public Node(Vector2 pos, Node parent,
                    float g, float h)
        {
            position = pos;
            this.parent = parent;
            gCost = g;
            hCost = h;
        }
    }

    public List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        // Snap both positions to grid centers
        start = SnapToGrid(start);
        end = SnapToGrid(end);

        List<Node> openList = new List<Node>();
        HashSet<Vector2> closedList = new HashSet<Vector2>();

        // Add starting node
        openList.Add(new Node(start, null, 0,
                    GetDistance(start, end)));

        int maxIterations = 200;
        int iterations = 0;

        while (openList.Count > 0)
        {
            iterations++;
            if (iterations > maxIterations)
            {
                Debug.Log("Pathfinding exceeded max iterations");
                return null;
            }

            // Get node with lowest fCost
            Node current = GetLowestFCost(openList);

            // Reached the destination
            if (current.position == end)
                return RetracePath(current);

            openList.Remove(current);
            closedList.Add(current.position);

            // Check all four 
            foreach (Vector2 neighbor in
                     GetNeighbors(current.position))
            {
                if (closedList.Contains(neighbor))
                    continue;

                if (!IsTileWalkable(neighbor))
                {
                    closedList.Add(neighbor);
                    continue;
                }

                float newG = current.gCost + 1;
                Node existingNode = openList.Find(
                    n => n.position == neighbor
                );

                if (existingNode == null)
                {
                    openList.Add(new Node(
                        neighbor,
                        current,
                        newG,
                        GetDistance(neighbor, end)
                    ));
                }
                else if (newG < existingNode.gCost)
                {
                    existingNode.gCost = newG;
                    existingNode.parent = current;
                }
            }
        }

        // No path found
        return null;
    }

    List<Vector2> RetracePath(Node endNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node current = endNode;

        while (current != null)
        {
            path.Add(current.position);
            current = current.parent;
        }

        path.Reverse();

        // player is already there
        if (path.Count > 0)
            path.RemoveAt(0);

        return path;
    }

    Node GetLowestFCost(List<Node> list)
    {
        Node lowest = list[0];
        foreach (Node n in list)
        {
            if (n.fCost < lowest.fCost)
                lowest = n;
        }
        return lowest;
    }

    List<Vector2> GetNeighbors(Vector2 position)
    {
        return new List<Vector2>
        {
            new Vector2(position.x + 1, position.y),
            new Vector2(position.x - 1, position.y),
            new Vector2(position.x, position.y + 1),
            new Vector2(position.x, position.y - 1)
        };
    }

    float GetDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x) +
               Mathf.Abs(a.y - b.y);
    }

    bool IsTileWalkable(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapBox(
            position,
            Vector2.one * 0.9f,
            0f,
            LayerMask.GetMask("Walls")
        );
        return hit == null;
    }

    Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(
            Mathf.Floor(position.x) + 0.5f,
            Mathf.Floor(position.y) + 0.5f
        );
    }
}
