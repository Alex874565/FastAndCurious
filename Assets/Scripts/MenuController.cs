using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MenuController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject mainMenu;

    [SerializeField] private TMP_InputField roomCode;
    [SerializeField] private TMP_InputField usernameInput;

    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private AudioClip clickSound;

    private void Awake()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        mainMenu.SetActive(true);
        SetButtonsInteractable(false);

        usernameInput.onValueChanged.AddListener(_ => { PlayTypingSound(); ValidateInputs(); });
        roomCode.onValueChanged.AddListener(_ => { PlayTypingSound(); ValidateInputs(); });
    }

    private void PlayTypingSound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(typingSound);
        }
    }

    private void SetButtonsInteractable(bool state)
    {
        createButton.interactable = state;
        joinButton.interactable = state;
    }

    private void ValidateInputs()
    {
        bool isUsernameValid = usernameInput.text.Length > 0;
        bool isRoomCodeValid = roomCode.text.Length >= 3;
        SetButtonsInteractable(isUsernameValid && isRoomCodeValid);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound);
    }

    public void JoinGame()
    {
        if (roomCode.text.Length < 3)
        {
            roomCode.text = "Room code must be at least 3 characters long";
            return;
        }
        PhotonNetwork.NickName = usernameInput.text;
        PhotonNetwork.JoinRoom(roomCode.text);
    }

    public void CreateGame()
    {
        if (roomCode.text.Length < 3)
        {
            roomCode.text = "Room code must be at least 3 characters long";
            return;
        }
        PhotonNetwork.NickName = usernameInput.text;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(roomCode.text, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room");
        PhotonNetwork.LoadLevel("GameMenu");
    }
}
