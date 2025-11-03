using UnityEngine;
using UnityEngine.Events;

public class PlayerRunnerController : MonoBehaviour
{
    [SerializeField] private float gridSize = 0.3f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector3 startPosition = new Vector3(-2.25f, 0.05f, 2.25f);
    
    public UnityEvent onReachGoal;
    
    private bool isMoving = false;
    private Vector3 targetPosition;
    private LayerMask obstacleLayer;
    private LayerMask goalLayer;

    void Start()
    {
        obstacleLayer = LayerMask.GetMask("Obstacle");
        goalLayer = LayerMask.GetMask("Goal");
        transform.localPosition = startPosition;
        targetPosition = transform.localPosition;
    }

    void Update()
    {
        if (!isMoving)
        {
            HandleInput();
        }
        
        if (isMoving)
        {
            MoveToTarget();
        }
    }
    
    void OnEnable()
    {
        transform.localPosition = startPosition;
        targetPosition = startPosition;
        isMoving = false;
    }

    void HandleInput()
    {
        Vector3 direction = Vector3.zero;

        // Changed to GetKey for continuous movement
        if (Input.GetKey(KeyCode.W))
        {
            direction = Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            direction = Vector3.back;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            direction = Vector3.left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            direction = Vector3.right;
        }

        if (direction != Vector3.zero)
        {
            TryMove(direction);
        }
    }

    void TryMove(Vector3 direction)
    {
        Vector3 newPosition = transform.localPosition + direction * gridSize;
        
        // Convert to WORLD space for physics check (like the AI does)
        Vector3 worldCheckPoint = transform.parent.TransformPoint(newPosition);
        
        // Use CheckSphere instead of Raycast (same as AI)
        bool hitObstacle = Physics.CheckSphere(worldCheckPoint, gridSize * 0.4f, obstacleLayer, QueryTriggerInteraction.Ignore);
        
        // Debug
        Debug.DrawLine(transform.position, worldCheckPoint, hitObstacle ? Color.red : Color.green, 0.5f);
        Debug.Log(hitObstacle ? "BLOCKED!" : "Path clear");
        
        if (!hitObstacle)
        {
            targetPosition = newPosition;
            isMoving = true;
        }
    }

    void MoveToTarget()
    {
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition, 
            targetPosition, 
            moveSpeed * Time.deltaTime
        );
        
        // Check if we've reached the target
        if (Vector3.Distance(transform.localPosition, targetPosition) < 0.001f)
        {
            transform.localPosition = targetPosition;
            isMoving = false;
            CheckGoal();
        }
    }

    void CheckGoal()
    {
        Collider[] hits = Physics.OverlapSphere(transform.localPosition, 0.15f, goalLayer);
        if (hits.Length > 0)
        {
            OnReachGoal();
        }
    }

    void OnReachGoal()
    {
        onReachGoal?.Invoke();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            OnReachGoal();
        }
    }
}