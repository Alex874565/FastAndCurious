using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private MusicManager musicManager; // Reference to MusicManager

    private void Awake()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnPlayClicked()
    {
        musicManager.PlayClickSound();
        StartCoroutine(LoadSceneAfterDelay(0.2f));
    }

    private void OnQuitClicked()
    {
        musicManager.PlayClickSound();
        StartCoroutine(QuitAfterDelay(0.2f));
    }

    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(1); // Or use SceneManager.LoadScene("SceneName");
    }

    private IEnumerator QuitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // For testing in Editor
#endif
    }
}
