using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    [SerializeField] private AudioSource musicSource;

    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // prevent duplicates
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // persist across scenes
        DontDestroyOnLoad(musicSource);
    }

    private void Start()
    {
        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        Destroy(gameObject); // Optional: Clean up for future
    }

    public void PlayClickSound()
    {
        Debug.Log("PlayClickSound triggered");
        audioSource.PlayOneShot(clickSound);
    }

}

