
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameController : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public GameObject sceneCamera;
    public GameManager gameManager; 

    // Start is called before the first frame update
    void Start()
    {
       
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found in scene! Please assign it in the Inspector or ensure it exists.");
                return; 
            }
        }

        SpawnPlayer();

        MusicManager music = FindObjectOfType<MusicManager>();
        if (music != null)
        {
            music.StopMusic();
        }
    }

    public void SpawnPlayer()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // ActorNumber starts at 1
        float spacing = 3f; // How far apart each player should be

        Vector3 baseSpawnPosition = new Vector3(5, -1, 31); // First player's position
        Vector3 offset = new Vector3(0, 0, playerIndex * spacing); // Offset each by X

        Vector3 spawnPosition = baseSpawnPosition + offset;

        Quaternion spawnRotation = Quaternion.Euler(0, -80f, 0); // Rotate 90° left (Y-axis)

        
        GameObject instantiatedPlayerCar = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, spawnRotation);
        sceneCamera.SetActive(false);

        
        if (gameManager != null)
        {
            gameManager.AssignCarColorOnInstantiate(instantiatedPlayerCar, PhotonNetwork.LocalPlayer);
        }
        else
        {
            Debug.LogError("GameManager is null. Cannot assign car color.");
        }
    }
}

























//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Photon.Pun;

//public class GameController : MonoBehaviourPunCallbacks
//{
//    public GameObject playerPrefab;
//    public GameObject sceneCamera;

//    // Start is called before the first frame update
//    void Start()
//    {
//        SpawnPlayer();

//        MusicManager music = FindObjectOfType<MusicManager>();
//        if (music != null)
//        {
//            music.StopMusic();
//        }
//    }

//    public void SpawnPlayer()
//{
//    int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // ActorNumber starts at 1
//    float spacing = 3f; // How far apart each player should be

//    Vector3 baseSpawnPosition = new Vector3(5, 2, 31); // First player's position
//    Vector3 offset = new Vector3(0, 0, playerIndex * spacing); // Offset each by X

//    Vector3 spawnPosition = baseSpawnPosition + offset;

//    Quaternion spawnRotation = Quaternion.Euler(0, -80f, 0); // Rotate 90° left (Y-axis)

//    PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, spawnRotation);
//    sceneCamera.SetActive(false);
//}


//}
