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

    private void Awake()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        mainMenu.SetActive(true);
        roomCode.interactable = false;
        joinButton.interactable = false;
        createButton.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public void ChangeUsernameInput()
    {
        if (usernameInput.text.Length > 3)
        {
            roomCode.interactable = true;
            joinButton.interactable = true;
            createButton.interactable = true;
        }else
        {
            roomCode.interactable = false;
            joinButton.interactable = false;
            createButton.interactable = false;
        }
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
