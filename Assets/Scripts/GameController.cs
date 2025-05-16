using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameController : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public GameObject sceneCamera;

    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
{
    int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // ActorNumber starts at 1
    float spacing = 4f; // How far apart each player should be

    Vector3 baseSpawnPosition = new Vector3(5, 2, 35); // First player's position
    Vector3 offset = new Vector3(playerIndex * spacing, 0, 0); // Offset each by X

    Vector3 spawnPosition = baseSpawnPosition + offset;

    Quaternion spawnRotation = Quaternion.Euler(0, -80f, 0); // Rotate 90° left (Y-axis)

    PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, spawnRotation);
    sceneCamera.SetActive(false);
}


}
