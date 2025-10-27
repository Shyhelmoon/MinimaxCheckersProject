using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public bool isWhiteTurn = true;
    public Text turnText;
    public Text winText;
    public Button restartButton; // NEW: Add a restart button
    public GameObject camera;
    public GameObject mazeRace;
    public PlayerRunnerController player;
    public AIRunnerController opponent;
    
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

        mazeRace.SetActive(false);
        player.onReachGoal.AddListener(PlayerWonRace);
        opponent.onReachGoal.AddListener(AIWonRace);
    }
    
    void OnDestroy()
    {
        player.onReachGoal.RemoveListener(PlayerWonRace);
        opponent.onReachGoal.RemoveListener(AIWonRace);
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
            mazeRace.SetActive(true);
            camera.transform.position = new Vector3(0, 12, 0);
            camera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    void PlayerWonRace()
    {
        camera.transform.position = new Vector3(0, 12, -5);
        camera.transform.rotation = Quaternion.Euler(60, 0, 0);
        mazeRace.SetActive(false);
        // TODO: Add powerups when player wins race
    }
    
    void AIWonRace()
    {
        camera.transform.position = new Vector3(0, 12, -5);
        camera.transform.rotation = Quaternion.Euler(60, 0, 0);
        mazeRace.SetActive(false);
        // TODO: Add powerups when AI wins race?
    }
}