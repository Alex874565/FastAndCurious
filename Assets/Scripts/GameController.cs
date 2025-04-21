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
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(Random.Range(-10, 10), 10, 0), Quaternion.identity);
        sceneCamera.SetActive(false);
    }

}
