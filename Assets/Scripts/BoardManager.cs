using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    public Material whiteTileMaterial;
    public Material blackTileMaterial;
    public Material highlightMaterial;
    
    private Piece[,] pieces = new Piece[8, 8];
    private GameObject[,] tiles = new GameObject[8, 8];
    private Material[,] originalTileMaterials = new Material[8, 8];
    private Vector3 boardOffset = new Vector3(-4f, 0, -4f);
    private float tileSize = 1f;
    
    void Start()
    {
        GenerateBoard();
        SpawnAllPieces();
    }
    
    void GenerateBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
                tile.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
                tile.transform.position = new Vector3(x * tileSize, 0, y * tileSize) + boardOffset;
                tile.transform.parent = transform;
                tile.name = $"Tile_{x}_{y}";
                
                Material tileMaterial;
                if ((x + y) % 2 == 0)
                    tileMaterial = whiteTileMaterial;
                else
                    tileMaterial = blackTileMaterial;
                
                tile.GetComponent<Renderer>().material = tileMaterial;
                originalTileMaterials[x, y] = tileMaterial;
                
                tiles[x, y] = tile;
            }
        }
    }
    
    void SpawnAllPieces()
    {
        // Spawn white pieces (bottom)
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if ((x + y) % 2 == 1)
                    SpawnPiece(x, y, true);
            }
        }
        
        // Spawn black pieces (top)
        for (int y = 5; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if ((x + y) % 2 == 1)
                    SpawnPiece(x, y, false);
            }
        }
    }
    
    void SpawnPiece(int x, int y, bool isWhite)
    {
        GameObject prefab = isWhite ? whitePiecePrefab : blackPiecePrefab;
        GameObject obj = Instantiate(prefab, GetWorldPosition(x, y), Quaternion.identity);
        obj.transform.parent = transform;
        
        Piece piece = obj.GetComponent<Piece>();
        piece.x = x;
        piece.y = y;
        piece.isWhite = isWhite;
        
        pieces[x, y] = piece;
    }
    
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * tileSize, 0.5f, y * tileSize) + boardOffset;
    }
    
    public void HighlightValidMoves(int startX, int startY)
    {
        ClearHighlights();
        
        Piece piece = pieces[startX, startY];
        if (piece == null) return;
        
        // Check all possible moves
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (IsValidMove(startX, startY, x, y))
                {
                    HighlightTile(x, y);
                }
            }
        }
    }
    
    void HighlightTile(int x, int y)
    {
        if (tiles[x, y] != null && highlightMaterial != null)
        {
            tiles[x, y].GetComponent<Renderer>().material = highlightMaterial;
        }
    }
    
    public void ClearHighlights()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (tiles[x, y] != null)
                {
                    tiles[x, y].GetComponent<Renderer>().material = originalTileMaterials[x, y];
                }
            }
        }
    }
    
    public bool IsValidMove(int startX, int startY, int endX, int endY)
    {
        // Check bounds
        if (endX < 0 || endX >= 8 || endY < 0 || endY >= 8)
            return false;
        
        // Check if destination is empty
        if (pieces[endX, endY] != null)
            return false;
        
        Piece piece = pieces[startX, startY];
        if (piece == null) return false;
        
        int deltaX = endX - startX;
        int deltaY = endY - startY;
        
        // Regular move (one diagonal)
        if (Mathf.Abs(deltaX) == 1 && Mathf.Abs(deltaY) == 1)
        {
            // Check direction based on color and king status
            if (piece.isKing)
                return true;
            
            if (piece.isWhite && deltaY > 0)
                return true;
            if (!piece.isWhite && deltaY < 0)
                return true;
        }
        
        // Jump move (two diagonal)
        if (Mathf.Abs(deltaX) == 2 && Mathf.Abs(deltaY) == 2)
        {
            int midX = (startX + endX) / 2;
            int midY = (startY + endY) / 2;
            Piece middlePiece = pieces[midX, midY];
            
            if (middlePiece != null && middlePiece.isWhite != piece.isWhite)
            {
                // Check direction
                if (piece.isKing)
                    return true;
                
                if (piece.isWhite && deltaY > 0)
                    return true;
                if (!piece.isWhite && deltaY < 0)
                    return true;
            }
        }
        
        return false;
    }
    
    // NEW: Check if a piece can make a jump from its current position
    public bool CanJumpFrom(int x, int y)
    {
        Piece piece = pieces[x, y];
        if (piece == null) return false;
        
        // Check all four diagonal jump directions
        int[] dx = { -2, -2, 2, 2 };
        int[] dy = { -2, 2, -2, 2 };
        
        for (int i = 0; i < 4; i++)
        {
            int newX = x + dx[i];
            int newY = y + dy[i];
            
            if (IsValidMove(x, y, newX, newY))
            {
                // Check if it's a jump (has a piece to capture in the middle)
                int midX = (x + newX) / 2;
                int midY = (y + newY) / 2;
                if (pieces[midX, midY] != null && pieces[midX, midY].isWhite != piece.isWhite)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    public void MovePiece(int startX, int startY, int endX, int endY)
    {
        Piece piece = pieces[startX, startY];
        pieces[startX, startY] = null;
        pieces[endX, endY] = piece;
        
        piece.x = endX;
        piece.y = endY;
        piece.transform.position = GetWorldPosition(endX, endY);
        
        bool wasJump = false;
        
        // Check for jump
        if (Mathf.Abs(endX - startX) == 2)
        {
            wasJump = true;
            int midX = (startX + endX) / 2;
            int midY = (startY + endY) / 2;
            Piece capturedPiece = pieces[midX, midY];
            
            if (capturedPiece != null)
            {
                pieces[midX, midY] = null;
                Destroy(capturedPiece.gameObject);
            }
        }
        
        // Check for king promotion
        if ((piece.isWhite && endY == 7) || (!piece.isWhite && endY == 0))
        {
            piece.BecomeKing();
        }
        
        // NEW: Check for multi-jump capability
        if (wasJump && CanJumpFrom(endX, endY))
        {
            // Piece can continue jumping - highlight valid jumps
            piece.canMultiJump = true;
            HighlightValidMoves(endX, endY);
        }
        else
        {
            // No more jumps possible, clear highlights and end turn
            piece.canMultiJump = false;
            ClearHighlights();
        }
    }
    
    public Piece GetPiece(int x, int y)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return null;
        return pieces[x, y];
    }
    
    // NEW: Win condition checking
    public int CountPieces(bool isWhite)
    {
        int count = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (pieces[x, y] != null && pieces[x, y].isWhite == isWhite)
                    count++;
            }
        }
        return count;
    }
    
    // NEW: Check if a player has any valid moves
    public bool HasValidMoves(bool isWhite)
    {
        for (int startX = 0; startX < 8; startX++)
        {
            for (int startY = 0; startY < 8; startY++)
            {
                Piece piece = pieces[startX, startY];
                if (piece != null && piece.isWhite == isWhite)
                {
                    // Check if this piece can move anywhere
                    for (int endX = 0; endX < 8; endX++)
                    {
                        for (int endY = 0; endY < 8; endY++)
                        {
                            if (IsValidMove(startX, startY, endX, endY))
                                return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    
    // NEW: Check for win condition
    public string CheckWinCondition()
    {
        int whitePieces = CountPieces(true);
        int blackPieces = CountPieces(false);
        
        // Win by elimination
        if (whitePieces == 0)
            return "Black";
        if (blackPieces == 0)
            return "White";
        
        // Win by no valid moves (stalemate = loss for player who can't move)
        if (!HasValidMoves(true))
            return "Black";
        if (!HasValidMoves(false))
            return "White";
        
        return null; // Game continues
    }
}