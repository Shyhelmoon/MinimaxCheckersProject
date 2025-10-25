using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool isWhiteTurn = true;
    public Text turnText;
    
    private int turnCount = 0;
    
    void Start()
    {
        UpdateTurnDisplay();
    }
    
    public void EndTurn()
    {
        isWhiteTurn = !isWhiteTurn;
        turnCount++;
        
        UpdateTurnDisplay();
        
        // Check for maze trigger every 3 turns
        if (turnCount % 6 == 0) // Every 3 complete rounds (6 half-turns)
        {
            TriggerMaze();
        }
    }
    
    void UpdateTurnDisplay()
    {
        if (turnText != null)
        {
            turnText.text = (isWhiteTurn ? "White" : "Black") + "'s Turn";
        }
    }
    
    void TriggerMaze()
    {
        Debug.Log("Maze triggered!");
        // TODO: Load maze scene or activate maze GameObject
        // This is where Person 2 & 3 will integrate
    }
}