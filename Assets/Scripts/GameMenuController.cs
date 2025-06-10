using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameMenuController : MonoBehaviourPunCallbacks
{
    public TMP_Text playerCount;
    public TMP_Text roomCodeText;

    public TMP_Text[] playerTexts; // Drag & Drop Player1, Player2, Player3, Player4 în Inspector

    public GameObject startButton;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip typingSound;

    [SerializeField] private TMP_InputField lapsInput;

    [SerializeField] private Button StartRaceButton;
    [SerializeField] private Button LeaveRoomButton;
    [SerializeField] private Button CalculationsButton;
    [SerializeField] private Button FormulasButton;

    public static int Laps = 3; // Default
    private bool isCategorySelected = false;

    private void Awake()
    {
        startButton.SetActive(PhotonNetwork.IsMasterClient);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        lapsInput.onValueChanged.AddListener(_ => PlayTypingSound());
        CalculationsButton.onClick.AddListener(() => SelectCategory("Calculations"));
        FormulasButton.onClick.AddListener(() => SelectCategory("Formulas"));

        UpdateRoomCode();
    }

    private void Update()
    {
        UpdatePlayerCount();
        UpdatePlayerNames();
    }

    private void PlayTypingSound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(typingSound);
        }
    }

    public void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound);
    }

    public void OnLapInputChanged()
    {
        if (int.TryParse(lapsInput.text, out int inputLaps))
        {
            if (inputLaps < 1)
            {
                inputLaps = 1;
                lapsInput.text = "1";
            }
            Laps = inputLaps;
        }
        else
        {
            lapsInput.text = "1";
            Laps = 1;
        }
    }

    private void SelectCategory(string category)
    {
        isCategorySelected = true;
        PlayClickSound();
        Debug.Log("Category selected: " + category);
    }

    public void StartGame()
    {
        if (!isCategorySelected)
        {
            Debug.LogWarning("Please select a category before starting the game!");
            return;
        }

        PhotonNetwork.LoadLevel("Game");
    }

    public void UpdatePlayerCount()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            playerCount.text = PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + " players";
        }
    }

    private void UpdatePlayerNames()
    {
        // Resetează textele la gol înainte de a popula
        for (int i = 0; i < playerTexts.Length; i++)
        {
            playerTexts[i].text = "";
        }

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length && i < playerTexts.Length; i++)
        {
            string displayName = players[i].NickName;
            if (players[i] == PhotonNetwork.LocalPlayer)
            {
                displayName += " (YOU)";
            }
            playerTexts[i].text = displayName;
        }
    }

    private void UpdateRoomCode()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            roomCodeText.text = "Room Code: " + PhotonNetwork.CurrentRoom.Name;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerNames();
        UpdatePlayerCount();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerNames();
        UpdatePlayerCount();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MainMenu");
    }
}
