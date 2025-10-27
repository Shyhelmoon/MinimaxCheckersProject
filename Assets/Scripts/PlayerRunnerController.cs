using UnityEngine;
using UnityEngine.Events;

public class PlayerRunnerController : MonoBehaviour
{
    [SerializeField] private float gridSize = 0.3f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float raycastDistance = 0.25f;
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
        else
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

        if (Input.GetKeyDown(KeyCode.W))
        {
            direction = Vector3.forward;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            direction = Vector3.back;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            direction = Vector3.left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
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
        
        // Cast a ray from current position to the target position
        if (!Physics.Raycast(transform.localPosition, direction, raycastDistance, obstacleLayer))
        {
            targetPosition = newPosition;
            isMoving = true;
        }
    }

    void MoveToTarget()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);
        
        if (Vector3.Distance(transform.localPosition, targetPosition) < 0.001f)
        {
            transform.localPosition = targetPosition;
            isMoving = false;
            CheckGoal();
        }
    }

    void CheckGoal()
    {
        Collider[] hits = Physics.OverlapSphere(transform.localPosition, 0.1f, goalLayer);
        if (hits.Length > 0)
        {
            OnReachGoal();
        }
    }

    void OnReachGoal()
    {
        Debug.Log("Goal reached!");
        onReachGoal?.Invoke();
    }

    // Trigger detection as alternative method
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            OnReachGoal();
        }
    }
}
