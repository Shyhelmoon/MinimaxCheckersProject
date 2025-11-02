using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class AIRunnerController : MonoBehaviour
{
    [SerializeField] private float gridSize = 0.2f;
    public float moveSpeed = 1f;
    [SerializeField] private Vector3 startPosition = new Vector3(-2.25f, 0.05f, 2.25f);
    [SerializeField] private Transform goalTransform;
    [SerializeField] private Vector2Int gridWorldSize = new Vector2Int(30, 30);
    
    [Header("Goal Settings")]
    public UnityEvent onReachGoal;
    
    private bool isMoving = false;
    private Vector3 targetPosition;
    private LayerMask obstacleLayer;
    private LayerMask goalLayer;
    private Rigidbody rb;
    
    private Node[,] grid;
    private List<Node> path;
    private int currentPathIndex = 0;

    void Start()
    {
        obstacleLayer = LayerMask.GetMask("Obstacle");
        goalLayer = LayerMask.GetMask("Goal");
        
        targetPosition = transform.localPosition;
        
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        
        CreateGrid();
    }

    void OnEnable()
    {
        startPosition = new Vector3(-2.25f, 0.05f, 2.25f);
        transform.localPosition = startPosition;
        targetPosition = startPosition;
        isMoving = false;
        currentPathIndex = 0;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        StartCoroutine(InitializePathfinding());
    }
    
    System.Collections.IEnumerator InitializePathfinding()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        
        CreateGrid();
        FindPath();
    }

    void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
        }
        else if (path != null && currentPathIndex < path.Count)
        {
            MoveAlongPath();
        }
    }

    void CreateGrid()
    {
        Physics.SyncTransforms();
        
        grid = new Node[gridWorldSize.x, gridWorldSize.y];
        Vector3 worldBottomLeft = transform.parent.position - Vector3.right * gridWorldSize.x / 2 * gridSize 
                                                            - Vector3.forward * gridWorldSize.y / 2 * gridSize;

        for (int x = 0; x < gridWorldSize.x; x++)
        {
            for (int y = 0; y < gridWorldSize.y; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * gridSize + gridSize / 2) 
                                                    + Vector3.forward * (y * gridSize + gridSize / 2);
                
                bool walkable = !Physics.CheckSphere(worldPoint, gridSize * 0.4f, obstacleLayer, QueryTriggerInteraction.Ignore);
                
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    void FindPath()
    {
        if (goalTransform == null)
        {
            Debug.LogError("Goal Transform is null!");
            return;
        }
        
        Node startNode = NodeFromWorldPoint(transform.position);
        Node targetNode = NodeFromWorldPoint(goalTransform.position);

        Debug.Log($"Start Node: {(startNode != null ? $"({startNode.gridX},{startNode.gridY}) walkable={startNode.walkable}" : "NULL")}");
        Debug.Log($"Target Node: {(targetNode != null ? $"({targetNode.gridX},{targetNode.gridY}) walkable={targetNode.walkable}" : "NULL")}");

        if (startNode == null)
        {
            Debug.LogError("Start node is null! AI position: " + transform.position);
            return;
        }
        
        if (targetNode == null)
        {
            Debug.LogError("Target node is null! Goal position: " + goalTransform.position);
            return;
        }
        
        if (!targetNode.walkable)
        {
            Debug.LogError("Target node is not walkable!");
            return;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        
        int iterations = 0;
        int maxIterations = 1000;

        while (openSet.Count > 0)
        {
            iterations++;
            if (iterations > maxIterations)
            {
                Debug.LogError("A* exceeded max iterations! Path may be impossible.");
                break;
            }
            
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || 
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                Debug.Log($"Path found in {iterations} iterations!");
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        
        Debug.LogError("No path found! Open set exhausted.");
    }

    void RetracePath(Node startNode, Node endNode)
    {
        path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        currentPathIndex = 0;
        
        Debug.Log($"Path has {path.Count} nodes");
    }

    void MoveAlongPath()
    {
        if (path == null || currentPathIndex >= path.Count) return;

        Node targetNode = path[currentPathIndex];
        Vector3 localTarget = transform.parent.InverseTransformPoint(targetNode.worldPosition);
        localTarget.y = startPosition.y;

        targetPosition = localTarget;
        isMoving = true;
        // Don't increment here - wait until we arrive at the target
    }

    void MoveToTarget()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);
        
        if (Vector3.Distance(transform.localPosition, targetPosition) < 0.001f)
        {
            transform.localPosition = targetPosition;
            isMoving = false;
            currentPathIndex++; // Only increment after arriving at target
            CheckGoal();
        }
    }

    void CheckGoal()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, gridSize * 0.5f, goalLayer);
        if (hits.Length > 0)
        {
            OnReachGoal();
        }
    }

    void OnReachGoal()
    {
        Debug.Log("AI reached goal!");
        onReachGoal?.Invoke();
        path = null;
        enabled = false; // Stop the AI
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            OnReachGoal();
        }
    }

    List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = node.gridX + dx[i];
            int checkY = node.gridY + dy[i];

            if (checkX >= 0 && checkX < gridWorldSize.x && checkY >= 0 && checkY < gridWorldSize.y)
            {
                neighbors.Add(grid[checkX, checkY]);
            }
        }

        return neighbors;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        return dstX + dstY;
    }

    Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - (transform.parent.position - Vector3.right * gridWorldSize.x / 2 * gridSize 
                                                                      - Vector3.forward * gridWorldSize.y / 2 * gridSize);
        
        int x = Mathf.RoundToInt(localPos.x / gridSize);
        int y = Mathf.RoundToInt(localPos.z / gridSize);

        if (x >= 0 && x < gridWorldSize.x && y >= 0 && y < gridWorldSize.y)
            return grid[x, y];
        
        return null;
    }

    void OnDrawGizmos()
    {
        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = n.walkable ? Color.white : Color.red;
                Gizmos.DrawWireCube(n.worldPosition, Vector3.one * gridSize * 0.9f);
            }
        }

        if (path != null)
        {
            foreach (Node n in path)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * gridSize * 0.5f);
            }
        }
    }
}

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int gCost;
    public int hCost;
    public Node parent;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }
}