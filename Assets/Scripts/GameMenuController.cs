using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class GameMenuController : MonoBehaviourPunCallbacks
{
    public TMP_Text playerCount;
    public GameObject startButton;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private Button StartRaceButton;
    [SerializeField] private Button LeaveRoomButton;
    [SerializeField] private Button CalculationsButton;
    [SerializeField] private Button FormulasButton;

    public static int Laps = 3; // Default

    [SerializeField] private TMP_InputField lapsInput;

    [SerializeField] private AudioClip typingSound;
    

    // Start is called before the first frame update
    void Awake()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            // If the player is not the master client, disable the start game button
            startButton.SetActive(false);
        }
        else
        {
            // If the player is the master client, enable the start game button
            startButton.SetActive(true);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        lapsInput.onValueChanged.AddListener(_ => PlayTypingSound());
    }

    

    // Update is called once per frame
    private void Update()
    {
        UpdatePlayerCount();
    }

    public void OnLapInputChanged()
    {
        if (int.TryParse(lapsInput.text, out int inputLaps))
        {
            if (inputLaps < 1)
            {
                inputLaps = 1; // Clamp to minimum value
                lapsInput.text = "1"; // Update field visually
            }

            Laps = inputLaps;
            Debug.Log("Laps set to: " + Laps);
        }
        else
        {
            // If input is invalid (blank, etc), reset to default or 1
            lapsInput.text = "1";
            Laps = 1;
        }
    }

    public void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound);
    }

    private void PlayTypingSound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(typingSound);
        }
    }

    public void StartGame()
    {
        // Load the game scene
        PhotonNetwork.LoadLevel("Game");
    }

    public void UpdatePlayerCount()
    {
        playerCount.text = PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + " players";
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MainMenu");
    }
}
