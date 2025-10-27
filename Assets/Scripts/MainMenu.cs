using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button playButton;
    public AudioSource sfxSource;
    public AudioClip clickSound;
    public Button quitButton;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        quitButton.onClick.AddListener(() => Application.Quit());

    }

    void OnPlayButtonClicked()
    {
        if (sfxSource && clickSound)
            sfxSource.PlayOneShot(clickSound);
        SceneManager.LoadScene("SampleScene");
    }
}
