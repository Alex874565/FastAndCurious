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
    }

    // Update is called once per frame
    private void Update()
    {
        UpdatePlayerCount();
    }

    public void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound);
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
