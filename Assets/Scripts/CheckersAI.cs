using UnityEngine;
using System.Collections.Generic;

// Represents a possible move in the game
public class Move
{
    public int startX, startY;
    public int endX, endY;
    public List<Move> additionalJumps; // For multi-jump sequences
    
    public Move(int startX, int startY, int endX, int endY)
    {
        this.startX = startX;
        this.startY = startY;
        this.endX = endX;
        this.endY = endY;
        this.additionalJumps = new List<Move>();
    }
}

public class CheckersAI : MonoBehaviour
{
    public BoardManager board;
    public GameManager gameManager;
    public int difficulty = 3; // Minimax depth (1-7 recommended, higher = smarter but slower)
    public float moveDelay = 0.5f; // Delay before AI makes move (for visual clarity)
    
    private bool isThinking = false;
    
    void Update()
    {
        // Check if it's AI's turn (black pieces)
        if (!gameManager.isWhiteTurn && !isThinking && !gameManager.gameObject.GetComponent<GameManager>().enabled == false)
        {
            isThinking = true;
            Invoke("MakeAIMove", moveDelay);
        }
    }
    
    void MakeAIMove()
    {
        Move bestMove = GetBestMove();
        
        if (bestMove != null)
        {
            ExecuteMove(bestMove);
        }
        
        isThinking = false;
    }
    
    // Main Minimax entry point
    Move GetBestMove()
    {
        List<Move> possibleMoves = GenerateAllMoves(false); // false = black (AI)
        
        if (possibleMoves.Count == 0)
            return null;
        
        Move bestMove = null;
        float bestScore = float.NegativeInfinity;
        
        foreach (Move move in possibleMoves)
        {
            // Simulate the move
            GameState simulatedState = SimulateMove(move, false);
            
            // Evaluate using minimax
            float score = Minimax(simulatedState, difficulty - 1, float.NegativeInfinity, float.PositiveInfinity, true);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        
        return bestMove;
    }
    
    // Minimax algorithm with alpha-beta pruning
    float Minimax(GameState state, int depth, float alpha, float beta, bool isMaximizingPlayer)
    {
        // Base case: reached depth limit or game over
        if (depth == 0 || IsTerminalState(state))
        {
            return EvaluateBoard(state);
        }
        
        if (isMaximizingPlayer) // White's turn (human)
        {
            float maxEval = float.NegativeInfinity;
            List<Move> moves = GenerateAllMovesFromState(state, true);
            
            foreach (Move move in moves)
            {
                GameState newState = SimulateMoveOnState(state, move, true);
                float eval = Minimax(newState, depth - 1, alpha, beta, false);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                
                if (beta <= alpha)
                    break; // Beta cutoff
            }
            
            return maxEval;
        }
        else // Black's turn (AI)
        {
            float minEval = float.PositiveInfinity;
            List<Move> moves = GenerateAllMovesFromState(state, false);
            
            foreach (Move move in moves)
            {
                GameState newState = SimulateMoveOnState(state, move, false);
                float eval = Minimax(newState, depth - 1, alpha, beta, true);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                
                if (beta <= alpha)
                    break; // Alpha cutoff
            }
            
            return minEval;
        }
    }
    
    // Evaluation function - assigns a score to a board state
    float EvaluateBoard(GameState state)
    {
        float score = 0;
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                PieceData piece = state.board[x, y];
                if (piece == null) continue;
                
                float pieceValue = 0;
                
                // Basic piece value
                if (piece.isKing)
                    pieceValue = 5f; // Kings are worth more
                else
                    pieceValue = 3f; // Regular pieces
                
                // Positional bonuses
                // Pieces closer to becoming kings are more valuable
                if (!piece.isKing)
                {
                    if (piece.isWhite)
                        pieceValue += y * 0.1f; // White pieces advance upward
                    else
                        pieceValue += (7 - y) * 0.1f; // Black pieces advance downward
                }
                
                // Center control bonus
                float centerDistance = Mathf.Abs(x - 3.5f) + Mathf.Abs(y - 3.5f);
                pieceValue += (7 - centerDistance) * 0.05f;
                
                // Edge penalty (pieces on edges are less flexible)
                if (x == 0 || x == 7 || y == 0 || y == 7)
                    pieceValue -= 0.2f;
                
                // Add or subtract based on color
                if (piece.isWhite)
                    score += pieceValue;
                else
                    score -= pieceValue;
            }
        }
        
        return score;
    }
    
    // Generate all possible moves for a player
    List<Move> GenerateAllMoves(bool isWhite)
    {
        List<Move> moves = new List<Move>();
        List<Move> jumpMoves = new List<Move>();
        
        for (int startX = 0; startX < 8; startX++)
        {
            for (int startY = 0; startY < 8; startY++)
            {
                Piece piece = board.GetPiece(startX, startY);
                if (piece == null || piece.isWhite != isWhite)
                    continue;
                
                // Check all possible destinations
                for (int endX = 0; endX < 8; endX++)
                {
                    for (int endY = 0; endY < 8; endY++)
                    {
                        if (board.IsValidMove(startX, startY, endX, endY))
                        {
                            Move move = new Move(startX, startY, endX, endY);
                            
                            // Check if it's a jump
                            if (Mathf.Abs(endX - startX) == 2)
                            {
                                jumpMoves.Add(move);
                                // TODO: Check for multi-jump sequences
                            }
                            else
                            {
                                moves.Add(move);
                            }
                        }
                    }
                }
            }
        }
        
        // In checkers, if jumps are available, you MUST take them
        if (jumpMoves.Count > 0)
            return jumpMoves;
        
        return moves;
    }
    
    // Generate moves from a simulated game state
    List<Move> GenerateAllMovesFromState(GameState state, bool isWhite)
    {
        List<Move> moves = new List<Move>();
        List<Move> jumpMoves = new List<Move>();
        
        for (int startX = 0; startX < 8; startX++)
        {
            for (int startY = 0; startY < 8; startY++)
            {
                PieceData piece = state.board[startX, startY];
                if (piece == null || piece.isWhite != isWhite)
                    continue;
                
                for (int endX = 0; endX < 8; endX++)
                {
                    for (int endY = 0; endY < 8; endY++)
                    {
                        if (IsValidMoveInState(state, startX, startY, endX, endY))
                        {
                            Move move = new Move(startX, startY, endX, endY);
                            
                            if (Mathf.Abs(endX - startX) == 2)
                                jumpMoves.Add(move);
                            else
                                moves.Add(move);
                        }
                    }
                }
            }
        }
        
        if (jumpMoves.Count > 0)
            return jumpMoves;
        
        return moves;
    }
    
    // Check if a move is valid in a simulated state
    bool IsValidMoveInState(GameState state, int startX, int startY, int endX, int endY)
    {
        if (endX < 0 || endX >= 8 || endY < 0 || endY >= 8)
            return false;
        
        if (state.board[endX, endY] != null)
            return false;
        
        PieceData piece = state.board[startX, startY];
        if (piece == null) return false;
        
        int deltaX = endX - startX;
        int deltaY = endY - startY;
        
        // Regular move
        if (Mathf.Abs(deltaX) == 1 && Mathf.Abs(deltaY) == 1)
        {
            if (piece.isKing)
                return true;
            
            if (piece.isWhite && deltaY > 0)
                return true;
            if (!piece.isWhite && deltaY < 0)
                return true;
        }
        
        // Jump move
        if (Mathf.Abs(deltaX) == 2 && Mathf.Abs(deltaY) == 2)
        {
            int midX = (startX + endX) / 2;
            int midY = (startY + endY) / 2;
            PieceData middlePiece = state.board[midX, midY];
            
            if (middlePiece != null && middlePiece.isWhite != piece.isWhite)
            {
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
    
    // Simulate a move and return the resulting game state
    GameState SimulateMove(Move move, bool isWhite)
    {
        GameState newState = new GameState();
        
        // Copy current board state
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece piece = board.GetPiece(x, y);
                if (piece != null)
                {
                    newState.board[x, y] = new PieceData(piece.isWhite, piece.isKing);
                }
            }
        }
        
        // Apply the move
        PieceData movingPiece = newState.board[move.startX, move.startY];
        newState.board[move.startX, move.startY] = null;
        newState.board[move.endX, move.endY] = movingPiece;
        
        // Handle capture
        if (Mathf.Abs(move.endX - move.startX) == 2)
        {
            int midX = (move.startX + move.endX) / 2;
            int midY = (move.startY + move.endY) / 2;
            newState.board[midX, midY] = null;
        }
        
        // Handle king promotion
        if ((movingPiece.isWhite && move.endY == 7) || (!movingPiece.isWhite && move.endY == 0))
        {
            movingPiece.isKing = true;
        }
        
        return newState;
    }
    
    // Simulate a move on an existing state
    GameState SimulateMoveOnState(GameState state, Move move, bool isWhite)
    {
        GameState newState = state.Clone();
        
        PieceData movingPiece = newState.board[move.startX, move.startY];
        newState.board[move.startX, move.startY] = null;
        newState.board[move.endX, move.endY] = movingPiece;
        
        if (Mathf.Abs(move.endX - move.startX) == 2)
        {
            int midX = (move.startX + move.endX) / 2;
            int midY = (move.startY + move.endY) / 2;
            newState.board[midX, midY] = null;
        }
        
        if ((movingPiece.isWhite && move.endY == 7) || (!movingPiece.isWhite && move.endY == 0))
        {
            movingPiece.isKing = true;
        }
        
        return newState;
    }
    
    // Execute the chosen move on the actual board
    void ExecuteMove(Move move)
    {
        Piece piece = board.GetPiece(move.startX, move.startY);
        if (piece != null)
        {
            board.MovePiece(move.startX, move.startY, move.endX, move.endY);
            
            // If there's a multi-jump, the board will handle it
            // For now, we end the turn
            if (!piece.canMultiJump)
            {
                gameManager.EndTurn();
            }
        }
    }
    
    // Check if a state is terminal (game over)
    bool IsTerminalState(GameState state)
    {
        int whitePieces = 0;
        int blackPieces = 0;
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (state.board[x, y] != null)
                {
                    if (state.board[x, y].isWhite)
                        whitePieces++;
                    else
                        blackPieces++;
                }
            }
        }
        
        return whitePieces == 0 || blackPieces == 0;
    }
}

// Represents a simulated game state for minimax
public class GameState
{
    public PieceData[,] board = new PieceData[8, 8];
    
    public GameState Clone()
    {
        GameState newState = new GameState();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] != null)
                {
                    newState.board[x, y] = new PieceData(board[x, y].isWhite, board[x, y].isKing);
                }
            }
        }
        return newState;
    }
}

// Lightweight piece representation for simulation
public class PieceData
{
    public bool isWhite;
    public bool isKing;
    
    public PieceData(bool isWhite, bool isKing)
    {
        this.isWhite = isWhite;
        this.isKing = isKing;
    }
}