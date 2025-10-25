using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool isWhiteTurn = true;
    public Text turnText;
    public Text winText;
    public Button restartButton; // NEW: Add a restart button
    
    private int turnCount = 0;
    private BoardManager boardManager;
    private bool gameOver = false;
    
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        UpdateTurnDisplay();
        
        if (winText != null)
            winText.gameObject.SetActive(false);
        
        // NEW: Hide restart button at start
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.AddListener(ResetGame); // Add click listener
        }
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
        
        isWhiteTurn = !isWhiteTurn;
        turnCount++;
        
        UpdateTurnDisplay();
        
        // Check for maze trigger every 3 turns (6 half-turns)
        if (turnCount % 6 == 0)
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
        
        // NEW: Show restart button
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
        
        Debug.Log(winner + " wins the game!");
    }
    
    public void ResetGame()
    {
        // Reload the current scene to reset everything
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    
    void TriggerMaze()
    {
        if (!gameOver)
        {
            Debug.Log("Maze triggered!");
            // TODO: Load maze scene or activate maze GameObject
            // This is where Person 2 & 3 will integrate
        }
    }
}