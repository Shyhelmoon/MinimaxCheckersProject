using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelect : MonoBehaviour
{
    public void SetDifficulty(float speed)
    {
        PlayerPrefs.SetFloat("AIMoveSpeed", speed);
        SceneManager.LoadScene("SampleScene");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
