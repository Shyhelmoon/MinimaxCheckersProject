using UnityEngine;

public class Piece : MonoBehaviour
{
    public int x;
    public int y;
    public bool isWhite;
    public bool isKing = false;
    public bool canMultiJump = false; // NEW: Track if this piece is in a multi-jump sequence
    
    public GameObject kingCrown;
    
    private Vector3 dragOffset;
    private float dragHeight = 1.5f;
    private BoardManager board;
    private GameManager gameManager;
    
    void Start()
    {
        board = FindObjectOfType<BoardManager>();
        gameManager = FindObjectOfType<GameManager>();
        
        if (kingCrown != null)
            kingCrown.SetActive(false);
    }
    
    void OnMouseDown()
    {
        // Only allow moves if it's this piece's turn
        if (gameManager.isWhiteTurn != isWhite)
            return;
        
        dragOffset = transform.position - GetMouseWorldPos();
        
        // Highlight valid moves when picking up the piece
        board.HighlightValidMoves(x, y);
    }
    
    void OnMouseDrag()
    {
        if (gameManager.isWhiteTurn != isWhite)
            return;
        
        Vector3 newPos = GetMouseWorldPos() + dragOffset;
        newPos.y = dragHeight;
        transform.position = newPos;
    }
    
    void OnMouseUp()
    {
        if (gameManager.isWhiteTurn != isWhite)
        {
            transform.position = board.GetWorldPosition(x, y);
            board.ClearHighlights();
            return;
        }
        
        // Convert mouse position to board coordinates
        Vector3 mousePos = GetMouseWorldPos();
        int newX = Mathf.RoundToInt((mousePos.x + 4f) / 1f);
        int newY = Mathf.RoundToInt((mousePos.z + 4f) / 1f);
        
        if (board.IsValidMove(x, y, newX, newY))
        {
            int oldX = x;
            int oldY = y;
            bool wasJump = Mathf.Abs(newX - oldX) == 2;
            
            board.MovePiece(x, y, newX, newY);
            
            // NEW: Only end turn if not in a multi-jump sequence
            if (!canMultiJump)
            {
                gameManager.EndTurn();
            }
            // If canMultiJump is true, the player must continue jumping with this piece
        }
        else
        {
            // Invalid move, snap back
            transform.position = board.GetWorldPosition(x, y);
            board.ClearHighlights();
        }
    }
    
    Vector3 GetMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;
        
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            return ray.GetPoint(rayDistance);
        }
        return Vector3.zero;
    }
    
    public void BecomeKing()
    {
        isKing = true;
        if (kingCrown != null)
            kingCrown.SetActive(true);
    }
}