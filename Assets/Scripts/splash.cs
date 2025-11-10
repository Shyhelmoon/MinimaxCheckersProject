using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    [SerializeField] private float delay = 2.5f;        
    [SerializeField] private RectTransform title;       
    [SerializeField] private float zoomDuration = 1.5f; // Time it takes to zoom in
    [SerializeField] private float startScale = 0.5f;   // Starting size
    [SerializeField] private float endScale = 1.0f;     // Final size

    void Start()
    {
        if (title != null)
            StartCoroutine(ZoomTitle());

        Invoke(nameof(LoadMenu), delay);
    }

    private System.Collections.IEnumerator ZoomTitle()
    {
        float elapsed = 0f;
        title.localScale = Vector3.one * startScale;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zoomDuration);
            title.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one * endScale, t);
            yield return null;
        }

        title.localScale = Vector3.one * endScale;
    }

    void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
