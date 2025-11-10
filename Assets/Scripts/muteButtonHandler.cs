using UnityEngine;
using TMPro;

public class MuteButtonHandler : MonoBehaviour
{
    public TextMeshProUGUI buttonText;

    private PersistentMusic music;

    void Start()
    {
        // Finds the PersistentMusic object across scenes
        music = FindObjectOfType<PersistentMusic>();

        if (music == null)
        {
            Debug.LogWarning("PersistentMusic not found!");
            return;
        }

        UpdateText();
    }

    public void ToggleMute()
    {
        if (music == null) return;

        music.ToggleMute();
        UpdateText();
    }

    void UpdateText()
    {
        if (music == null || buttonText == null) return;

        buttonText.text = music.IsMuted() ? "Unmute" : "Mute";
    }
}
