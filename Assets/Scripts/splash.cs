using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    [SerializeField] private float delay = 2.5f;

    void Start()
    {
        Invoke(nameof(LoadMenu), delay);
    }

    void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
