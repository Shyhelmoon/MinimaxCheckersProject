using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class AIRunnerController : MonoBehaviour
{
    [SerializeField] private float gridSize = 0.2f;
    public float moveSpeed = 2f;
    private Vector3 startPosition; // Remove SerializeField so code controls it
    [SerializeField] private int runnerIndex = 0; // 0, 1, or 2 to differentiate runners
    [SerializeField] private Transform goalTransform;
    private Vector3 mazeParent = new Vector3(3f, 1f, -0.5f);
    [SerializeField] private Vector2Int gridWorldSize = new Vector2Int(30, 30);
    
    [Header("Potential Fields")]
    [SerializeField] private float attractiveForceWeight = 1f;
    [SerializeField] private float obstacleRepulsionWeight = 0.5f;
    [SerializeField] private float aiRepulsionWeight = 2f;
    [SerializeField] private float aiRepulsionRange = 0.9f;
    [SerializeField] private string aiTag = "AI";
    
    [Header("Goal Settings")]
    public UnityEvent onReachGoal;
    
    private static int totalRunners = 3;
    private static int runnersAtGoal = 0;
    private static bool goalEventTriggered = false;
    private static List<AIRunnerController> allRunners = new List<AIRunnerController>();
    
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
        
        // Tag this AI so others can detect it
        if (!gameObject.CompareTag(aiTag))
        {
            gameObject.tag = aiTag;
        }
        
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
        // Set start position based on runner index
        Vector3[] startPositions = {
            new Vector3(0.75f, 1.05f, 1.65f), // Runner 0
            new Vector3(0.7f, 1.05f, 1.75f),  // Runner 1
            new Vector3(0.8f, 1.05f, 1.75f)   // Runner 2
        };
        
        if (runnerIndex >= 0 && runnerIndex < startPositions.Length)
        {
            startPosition = startPositions[runnerIndex];
        }
        
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
        Vector3 worldBottomLeft = mazeParent - Vector3.right * gridWorldSize.x / 2 * gridSize 
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
        targetPosition = targetNode.worldPosition;
        targetPosition.y = startPosition.y;

        isMoving = true;
        Debug.Log($"Moving to node {currentPathIndex}/{path.Count}, target: {targetPosition}");
    }

    void MoveToTarget()
    {
        // Calculate potential field forces
        Vector3 potentialFieldForce = CalculatePotentialFieldForce();
        
        // Combine A* path following with potential field adjustments
        Vector3 pathDirection = (targetPosition - transform.localPosition).normalized;
        Vector3 adjustedDirection = (pathDirection * attractiveForceWeight + potentialFieldForce).normalized;
        
        // Move with adjusted direction
        float distanceMoved = moveSpeed * Time.deltaTime;
        
        // Check if adjusted movement would hit an obstacle
        Vector3 worldAdjustedDir = transform.TransformDirection(adjustedDirection);
        bool adjustedPathBlocked = Physics.Raycast(transform.position, worldAdjustedDir, distanceMoved * 1.5f, obstacleLayer);
        
        Vector3 movement;
        if (!adjustedPathBlocked)
        {
            // Use potential field adjusted direction
            movement = adjustedDirection * distanceMoved;
        }
        else
        {
            // Fall back to pure A* path direction
            Vector3 worldPathDir = transform.TransformDirection(pathDirection);
            bool pathBlocked = Physics.Raycast(transform.position, worldPathDir, distanceMoved * 1.5f, obstacleLayer);
            
            if (!pathBlocked)
            {
                movement = pathDirection * distanceMoved;
            }
            else
            {
                // If both blocked, try to slide along wall
                movement = Vector3.zero;
                
                // Try moving perpendicular to find opening
                Vector3[] slideDirections = {
                    Vector3.Cross(pathDirection, Vector3.up).normalized,
                    Vector3.Cross(pathDirection, Vector3.down).normalized
                };
                
                foreach (Vector3 slideDir in slideDirections)
                {
                    Vector3 worldSlideDir = transform.TransformDirection(slideDir);
                    if (!Physics.Raycast(transform.position, worldSlideDir, distanceMoved * 1.5f, obstacleLayer))
                    {
                        movement = slideDir * distanceMoved * 0.5f; // Slower slide movement
                        break;
                    }
                }
            }
        }
        
        transform.localPosition += movement;
        
        float distance = Vector3.Distance(transform.localPosition, targetPosition);
        
        // Check if close enough to target OR if stuck
        if (distance < 0.15f || (movement.magnitude < 0.001f && distance < gridSize))
        {
            transform.localPosition = targetPosition;
            isMoving = false;
            currentPathIndex++;
            Debug.Log($"Arrived at node {currentPathIndex-1}, moving to next");
            CheckGoal();
        }
    }
    
    Vector3 CalculatePotentialFieldForce()
    {
        Vector3 totalForce = Vector3.zero;
        
        // Repulsive force from other AIs
        GameObject[] otherAIs = GameObject.FindGameObjectsWithTag(aiTag);
        foreach (GameObject otherAI in otherAIs)
        {
            if (otherAI == gameObject) continue; // Skip self
            
            float distance = Vector3.Distance(transform.position, otherAI.transform.position);
            
            if (distance < aiRepulsionRange && distance > 0.01f)
            {
                // Calculate repulsive direction (away from other AI)
                Vector3 awayDirection = (transform.position - otherAI.transform.position).normalized;
                
                // Stronger repulsion when closer
                float forceMagnitude = aiRepulsionWeight * (1f - distance / aiRepulsionRange);
                
                awayDirection.y = 0; // Keep on same plane
                
                totalForce += awayDirection * forceMagnitude;
            }
        }
        
        // Repulsive force from nearby obstacles
        Collider[] nearbyObstacles = Physics.OverlapSphere(transform.position, gridSize * 2f, obstacleLayer);
        foreach (Collider obstacle in nearbyObstacles)
        {
            float distance = Vector3.Distance(transform.position, obstacle.transform.position);
            if (distance > 0.01f)
            {
                Vector3 awayDirection = (transform.position - obstacle.transform.position).normalized;
                float forceMagnitude = obstacleRepulsionWeight / distance;
                
                awayDirection.y = 0;
                
                totalForce += awayDirection * forceMagnitude;
            }
        }
        
        return totalForce;
    }

    void CheckGoal()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, gridSize * 0.5f, goalLayer);
        if (hits.Length > 0)
        {
            OnRunnerReachGoal();
        }
    }

    void OnRunnerReachGoal()
    {
        Debug.Log($"AI Runner {runnerIndex} reached goal!");
        
        // Increment runners at goal
        runnersAtGoal++;
        
        // Stop this runner's movement
        path = null;
        enabled = false;
        
        Debug.Log($"{runnersAtGoal}/{totalRunners} runners at goal");
        
        // Check if all runners have reached the goal
        if (runnersAtGoal >= totalRunners && !goalEventTriggered)
        {
            goalEventTriggered = true;
            Debug.Log("All AI runners reached goal! Invoking events...");
            
            // Invoke the event on all runners (in case listeners are on different instances)
            foreach (AIRunnerController runner in allRunners)
            {
                if (runner != null && runner.onReachGoal != null)
                {
                    runner.onReachGoal.Invoke();
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            OnRunnerReachGoal();
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
        Vector3 localPos = worldPosition - (mazeParent - Vector3.right * gridWorldSize.x / 2 * gridSize 
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

    public void ResetRunner(bool isGlobalReset = false)
    {
        Debug.Log($"Resetting AI Runner {runnerIndex}");

        // Re-enable this AI in case it was disabled after finishing
        enabled = true;

        // Reset position and movement state
        transform.localPosition = startPosition;
        targetPosition = startPosition;
        isMoving = false;
        currentPathIndex = 0;

        // Reset physics
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Clear old path and regenerate
        path = null;

        // (Re)initialize grid and start moving again
        CreateGrid();
        StartCoroutine(InitializePathfinding());

        // Only the first runner should reset the global counters
        if (isGlobalReset)
        {
            runnersAtGoal = 0;
            goalEventTriggered = false;
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