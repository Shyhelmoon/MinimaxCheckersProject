using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public bool isWhiteTurn = true;
    public Text turnText;
    public Text winText;
    public Text powerupText; // NEW: Display active powerup
    public Button restartButton;
    public GameObject camera;
    public GameObject mazeRace1;
    public GameObject mazeRace2;
    public GameObject mazeRace3;
    public GameObject playerRunner;
    public GameObject runner1;
    public GameObject runner2;
    public GameObject runner3;
    public PlayerRunnerController playerScript;
    public AIRunnerController runner1Script;
    public AIRunnerController runner2Script;
    public AIRunnerController runner3Script;
    
    private int turnCount = 0;
    private BoardManager boardManager;
    private bool gameOver = false;
    
    // NEW: Powerup system
    private bool hasExtraTurn = false;
    private bool hasProtectedPiece = false;
    private int protectedPieceX = -1;
    private int protectedPieceY = -1;

    // AI powerups
    private bool hasAIExtraTurn = false;

    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        UpdateTurnDisplay();

        if (winText != null)
            winText.gameObject.SetActive(false);
            
        if (powerupText != null)
            powerupText.gameObject.SetActive(false);

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.AddListener(ResetGame);
        }

        mazeRace1.SetActive(false);
        mazeRace2.SetActive(false);
        mazeRace3.SetActive(false);
        playerRunner.SetActive(false);
        runner1.SetActive(false);
        runner2.SetActive(false);
        runner3.SetActive(false);
        playerScript.onReachGoal.AddListener(PlayerWonRace);
        runner1Script.onReachGoal.AddListener(AIWonRace);
        runner2Script.onReachGoal.AddListener(AIWonRace);
        runner3Script.onReachGoal.AddListener(AIWonRace);
    }
    
    void OnDestroy()
    {
        playerScript.onReachGoal.RemoveListener(PlayerWonRace);
        runner1Script.onReachGoal.RemoveListener(AIWonRace);
        runner2Script.onReachGoal.RemoveListener(AIWonRace);
        runner3Script.onReachGoal.RemoveListener(AIWonRace);
    }
    
    public void EndTurn()
    {
        // Check for win condition before changing turns
        string winner = boardManager.CheckWinCondition();
        if (winner != null)
        {
            GameOver(winner);
            return;
        }
        
        // Check for player extra turn powerup
        if (hasExtraTurn && isWhiteTurn)
        {
            hasExtraTurn = false;
            UpdatePowerupDisplay("Extra Turn Used!");
            Invoke("ClearPowerupDisplay", 2f);
            return;
        }
        
        // NEW: Check for AI extra turn powerup
        if (hasAIExtraTurn && !isWhiteTurn)
        {
            hasAIExtraTurn = false;
            UpdatePowerupDisplay("AI Used Extra Turn!");
            Invoke("ClearPowerupDisplay", 2f);
            return;
        }
        
        isWhiteTurn = !isWhiteTurn;
        turnCount++;
        
        UpdateTurnDisplay();
        
        // Check for maze trigger every 7 turns (14 half-turns)
        if (turnCount % 14 == 0)
        {
            TriggerMaze();
        }
        
        // Check win condition again after turn change
        winner = boardManager.CheckWinCondition();
        if (winner != null)
        {
            GameOver(winner);
        }
    }
    
    void UpdateTurnDisplay()
    {
        if (turnText != null && !gameOver)
        {
            turnText.text = (isWhiteTurn ? "White" : "Black") + "'s Turn";
        }
    }
    
    void UpdatePowerupDisplay(string message)
    {
        if (powerupText != null)
        {
            powerupText.text = message;
            powerupText.gameObject.SetActive(true);
        }
    }
    
    void ClearPowerupDisplay()
    {
        if (powerupText != null)
        {
            powerupText.gameObject.SetActive(false);
        }
    }
    
    void GameOver(string winner)
    {
        gameOver = true;
        
        if (winText != null)
        {
            winText.text = winner + " Wins!";
            winText.gameObject.SetActive(true);
        }
        
        if (turnText != null)
        {
            turnText.text = "Game Over!";
        }
        
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
        
        Debug.Log(winner + " wins the game!");

        if (winner == "White")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("VictoryScene");
        }
        else if (winner == "Black")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DefeatScene");
        }
    }
    
    public void ResetGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void TriggerMaze()
    {
        if (!gameOver)
        {
            Debug.Log("Maze triggered!");

            int rand = Random.Range(0, 3);
            switch (rand)
            {
                case 0:
                    mazeRace1.SetActive(true);
                    break;
                case 1:
                    mazeRace2.SetActive(true);
                    break;
                case 2:
                    mazeRace3.SetActive(true);
                    break;
            }
            playerRunner.SetActive(true);
            runner1.SetActive(true);
            runner2.SetActive(true);
            runner3.SetActive(true);

            camera.transform.position = new Vector3(0, 12, 0);
            camera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    void PlayerWonRace()
    {
        camera.transform.position = new Vector3(0, 12, -5);
        camera.transform.rotation = Quaternion.Euler(60, 0, 0);
        mazeRace1.SetActive(false);
        mazeRace2.SetActive(false);
        mazeRace3.SetActive(false);
        playerRunner.SetActive(false);
        runner1.SetActive(false);
        runner2.SetActive(false);
        runner3.SetActive(false);
        
        // NEW: Grant random powerup
        GrantRandomPowerup();
    }
    
    void AIWonRace()
    {
        camera.transform.position = new Vector3(0, 12, -5);
        camera.transform.rotation = Quaternion.Euler(60, 0, 0);
        mazeRace1.SetActive(false);
        mazeRace2.SetActive(false);
        mazeRace3.SetActive(false);
        playerRunner.SetActive(false);
        runner1.SetActive(false);
        runner2.SetActive(false);
        runner3.SetActive(false);
        
        // NEW: Grant AI extra turn
        hasAIExtraTurn = true;
        UpdatePowerupDisplay("AI Earned Extra Turn!");
        Invoke("ClearPowerupDisplay", 2f);
        Debug.Log("AI gained Extra Turn powerup!");
    }

    public void ResetMazeRace()
    {
        Debug.Log("Resetting Maze Race...");

        var aiRunners = FindObjectsOfType<AIRunnerController>();
        bool firstRunner = true;

        foreach (var ai in aiRunners)
        {
            ai.ResetRunner(firstRunner);
            firstRunner = false;
        }

        var player = FindObjectOfType<PlayerRunnerController>();
        if (player != null)
            player.ResetRunner();
    }
    
    // NEW: Powerup system
    void GrantRandomPowerup()
    {
        int powerupChoice = Random.Range(0, 5);
        
        switch (powerupChoice)
        {
            case 0:
                GrantExtraTurn();
                break;
            case 1:
                GrantKingUpgrade();
                break;
            case 2:
                GrantRemoveOpponentPiece();
                break;
            case 3:
                GrantProtectPiece();
                break;
            case 4:
                GrantForceOpponentSkip();
                break;
        }
    }
    
    // Powerup 1: Extra Turn
    void GrantExtraTurn()
    {
        hasExtraTurn = true;
        UpdatePowerupDisplay("Powerup: Extra Turn!");
        Debug.Log("Player gained Extra Turn powerup!");
    }
    
    // Powerup 2: Upgrade Random Piece to King
    void GrantKingUpgrade()
    {
        // Find all non-king white pieces
        System.Collections.Generic.List<Piece> eligiblePieces = new System.Collections.Generic.List<Piece>();
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece piece = boardManager.GetPiece(x, y);
                if (piece != null && piece.isWhite && !piece.isKing)
                {
                    eligiblePieces.Add(piece);
                }
            }
        }
        
        if (eligiblePieces.Count > 0)
        {
            Piece randomPiece = eligiblePieces[Random.Range(0, eligiblePieces.Count)];
            randomPiece.BecomeKing();
            UpdatePowerupDisplay("Powerup: Random Piece Promoted to King!");
            Debug.Log("Player piece upgraded to king!");
        }
        else
        {
            // Fallback to extra turn if no eligible pieces
            GrantExtraTurn();
        }
    }
    
    // Powerup 3: Remove Random Opponent Piece
    void GrantRemoveOpponentPiece()
    {
        // Find all black pieces
        System.Collections.Generic.List<Piece> blackPieces = new System.Collections.Generic.List<Piece>();
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece piece = boardManager.GetPiece(x, y);
                if (piece != null && !piece.isWhite)
                {
                    blackPieces.Add(piece);
                }
            }
        }
        
        if (blackPieces.Count > 0)
        {
            Piece randomPiece = blackPieces[Random.Range(0, blackPieces.Count)];
            boardManager.RemovePiece(randomPiece.x, randomPiece.y);
            UpdatePowerupDisplay("Powerup: Opponent Piece Removed!");
            Debug.Log("Random AI piece removed!");
        }
    }
    
    // Powerup 4: Protect One Piece from Capture (one time)
    void GrantProtectPiece()
    {
        // Find all white pieces
        System.Collections.Generic.List<Piece> whitePieces = new System.Collections.Generic.List<Piece>();
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece piece = boardManager.GetPiece(x, y);
                if (piece != null && piece.isWhite)
                {
                    whitePieces.Add(piece);
                }
            }
        }
        
        if (whitePieces.Count > 0)
        {
            Piece randomPiece = whitePieces[Random.Range(0, whitePieces.Count)];
            protectedPieceX = randomPiece.x;
            protectedPieceY = randomPiece.y;
            hasProtectedPiece = true;
            
            // Visual indicator (optional - you could add a shield effect)
            randomPiece.gameObject.transform.localScale *= 1.2f;
            
            UpdatePowerupDisplay("Powerup: One Piece Protected!");
            Debug.Log($"Piece at ({protectedPieceX}, {protectedPieceY}) is protected!");
        }
    }
    
    // Powerup 5: Force Opponent to Skip Next Turn
    void GrantForceOpponentSkip()
    {
        // This will skip the AI's next turn
        isWhiteTurn = true; // Keep it white's turn
        UpdatePowerupDisplay("Powerup: Opponent Skips Next Turn!");
        Debug.Log("AI will skip next turn!");
        
        // We need to do an extra turn switch to actually skip
        Invoke("SkipAITurn", 1.5f);
    }
    
    void SkipAITurn()
    {
        // Just stay on white's turn
        UpdateTurnDisplay();
        ClearPowerupDisplay();
    }
    
    // Method to check if a piece is protected
    public bool IsPieceProtected(int x, int y)
    {
        if (hasProtectedPiece && x == protectedPieceX && y == protectedPieceY)
        {
            hasProtectedPiece = false; // Use up the protection
            UpdatePowerupDisplay("Protected Piece Saved!");
            Invoke("ClearPowerupDisplay", 2f);
            return true;
        }
        return false;
    }
}