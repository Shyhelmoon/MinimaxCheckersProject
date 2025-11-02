using UnityEngine;
using UnityEngine.Events;

public class PlayerRunnerController : MonoBehaviour
{
    [SerializeField] private float gridSize = 0.3f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float raycastDistance = 0.25f;
    [SerializeField] private Vector3 startPosition = new Vector3(-2.25f, 0.05f, 2.25f);
    
    public UnityEvent onReachGoal;
    
    private LayerMask obstacleLayer;
    private LayerMask goalLayer;
    private Vector3 moveDirection;
    private Rigidbody rb;

    void Start()
    {
        obstacleLayer = LayerMask.GetMask("Obstacle");
        goalLayer = LayerMask.GetMask("Goal");
        transform.localPosition = startPosition;
        
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleInput();
        CheckGoal();
    }

    void FixedUpdate()
    {
        if (moveDirection != Vector3.zero)
        {
            MovePlayer();
        }
    }

    void OnEnable()
    {
        transform.localPosition = startPosition;
        moveDirection = Vector3.zero;
        
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void HandleInput()
    {
        moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection = Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveDirection = Vector3.back;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            moveDirection = Vector3.left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveDirection = Vector3.right;
        }
    }

    void MovePlayer()
    {
        // Check if there's an obstacle ahead
        Vector3 worldDirection = transform.TransformDirection(moveDirection);
        if (!Physics.Raycast(transform.position, worldDirection, raycastDistance, obstacleLayer))
        {
            // Move in local space
            Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
            transform.localPosition += movement;
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
        Debug.Log("Goal reached!");
        onReachGoal?.Invoke();
        enabled = false; // Stop player movement after reaching goal
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            OnReachGoal();
        }
    }
}