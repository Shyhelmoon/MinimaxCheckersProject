using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    public Material whiteTileMaterial;
    public Material blackTileMaterial;
    public Material highlightMaterial; // NEW: Assign a bright material (green/yellow)
    
    private Piece[,] pieces = new Piece[8, 8];
    private GameObject[,] tiles = new GameObject[8, 8];
    private Material[,] originalTileMaterials = new Material[8, 8]; // NEW: Store original materials
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
                
                // Alternate colors
                Material tileMaterial;
                if ((x + y) % 2 == 0)
                    tileMaterial = whiteTileMaterial;
                else
                    tileMaterial = blackTileMaterial;
                
                tile.GetComponent<Renderer>().material = tileMaterial;
                originalTileMaterials[x, y] = tileMaterial; // NEW: Store the original material
                
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
    
    // NEW: Highlight valid moves for a piece
    public void HighlightValidMoves(int startX, int startY)
    {
        ClearHighlights(); // Clear any existing highlights first
        
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
    
    // NEW: Highlight a single tile
    void HighlightTile(int x, int y)
    {
        if (tiles[x, y] != null && highlightMaterial != null)
        {
            tiles[x, y].GetComponent<Renderer>().material = highlightMaterial;
        }
    }
    
    // NEW: Clear all highlights
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
    
    public void MovePiece(int startX, int startY, int endX, int endY)
    {
        Piece piece = pieces[startX, startY];
        pieces[startX, startY] = null;
        pieces[endX, endY] = piece;
        
        piece.x = endX;
        piece.y = endY;
        piece.transform.position = GetWorldPosition(endX, endY);
        
        // Check for jump
        if (Mathf.Abs(endX - startX) == 2)
        {
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
        
        ClearHighlights(); // NEW: Clear highlights after move
    }
    
    public Piece GetPiece(int x, int y)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return null;
        return pieces[x, y];
    }
}