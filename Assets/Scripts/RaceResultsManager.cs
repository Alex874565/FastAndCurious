using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro; // Necesare pentru TextMeshProUGUI
using UnityEngine.UI; // Necesare pentru Butoane

public class RaceResultsManager : MonoBehaviourPunCallbacks
{
    public static RaceResultsManager Instance;

    [Header("UI Elements")]
    [SerializeField] private GameObject resultPanel; // Panoul 'Result'
    [SerializeField] private TMP_Text myPlaceText;    // Textul 'Place' pentru rangul jucătorului local
    [SerializeField] private TMP_Text[] allPlayerPlacementsText; // Array pentru 1Place, 2Place, 3Place, 4Place

    // --- Referințe pentru Butoane, pentru a le trage din Inspector ---
    [Header("Action Buttons")]
    [SerializeField] private Button backToLobbyButton; // Trage butonul "Back to Lobby" aici
    [SerializeField] private Button quitGameButton;     // Trage butonul "Quit" aici


    private Dictionary<int, float> playerTimes = new Dictionary<int, float>(); // Stochează timpii de finish ai jucătorilor
    private List<int> finishOrder = new List<int>(); // Stochează ordinea în care jucătorii au terminat

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Decomentează dacă managerul trebuie să persiste între scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Asigură-te că panoul de rezultate este ascuns la începutul scenei
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        // Golește orice text de rezultate anterior
        ClearPlacementsUI();
        if (myPlaceText != null) myPlaceText.text = "";

        // --- Implementarea butoanelor prin cod ---
        if (backToLobbyButton != null)
        {
            backToLobbyButton.onClick.AddListener(BackToLobby);
            Debug.Log("Back to Lobby button listener added.");
        }
        else
        {
            Debug.LogWarning("BackToLobbyButton is not assigned in the Inspector!");
        }

        if (quitGameButton != null)
        {
            quitGameButton.onClick.AddListener(QuitGame);
            Debug.Log("Quit Game button listener added.");
        }
        else
        {
            Debug.LogWarning("QuitGameButton is not assigned in the Inspector!");
        }
    }

    // Această metodă este apelată o singură dată per jucător, când acesta termină cursa
    public void ReportFinish(int playerID, float time)
    {
        if (Instance == null)
        {
            Debug.LogError("RaceResultsManager instance not found!");
            return;
        }

        // Apelare RPC pe PhotonView-ul acestui RaceResultsManager, trimis doar către MasterClient
        Instance.photonView.RPC("RPC_ReportFinish", RpcTarget.MasterClient, playerID, time);
    }

    // RPC doar pentru MasterClient pentru a înregistra finish-ul unui jucător
    [PunRPC]
    public void RPC_ReportFinish(int playerID, float time)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Doar MasterClient-ul procesează acest RPC

        if (!playerTimes.ContainsKey(playerID))
        {
            playerTimes[playerID] = time; // Stochează timpul
            finishOrder.Add(playerID); // Salvează ordinea în care au terminat
            Debug.Log($"Player {playerID} finished with time {time:F2} seconds");
        }
        else
        {
            Debug.LogWarning($"Player {playerID} already reported finish!");
            return; // Ieși dacă jucătorul a raportat deja pentru a preveni intrări duplicate
        }

        CheckIfAllPlayersFinished(); // Verifică dacă toți jucătorii au raportat finish-ul
    }

    // Apelată de MasterClient pentru a determina dacă toți jucătorii au terminat
    private void CheckIfAllPlayersFinished()
    {
        Debug.Log($"Checking if all players finished... Current count: {playerTimes.Count}, Expected: {PhotonNetwork.CurrentRoom.PlayerCount}");
        if (playerTimes.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Debug.Log("All players finished! Sending results...");
            ShowResultsToAll(); // Dacă toți au terminat, distribuie rezultatele
        }
    }

    // MasterClient-ul trimite locul de finish fiecărui jucător și declanșează actualizarea UI-ului
    private void ShowResultsToAll()
    {
        if (!PhotonNetwork.IsMasterClient) return; // Doar MasterClient-ul distribuie rezultatele

        int[] orderedActorIDs = finishOrder.ToArray();
        
        // Când trimitem rezultatele, trimitem și timpii pentru toți jucătorii, dacă trebuie afișați
        float[] orderedTimes = new float[orderedActorIDs.Length];
        for (int i = 0; i < orderedActorIDs.Length; i++)
        {
            orderedTimes[i] = playerTimes[orderedActorIDs[i]];
        }

        // Trimite RPC către toți clienții pentru a afișa UI-ul cu rezultate, incluzând acum timpii
        photonView.RPC("RPC_ShowResultsUI", RpcTarget.All, orderedActorIDs, orderedTimes);

        // Opțional: Păstrează RPC_FinishRaceWithPlace dacă PlayerBehaviour are nevoie de el pentru logică specifică post-cursă.
        for (int i = 0; i < finishOrder.Count; i++)
        {
            int actorID = finishOrder[i];
            int place = i + 1;

            foreach (var player in FindObjectsOfType<PlayerBehaviour>())
            {
                if (player.photonView.OwnerActorNr == actorID)
                {
                    player.photonView.RPC("RPC_FinishRaceWithPlace", player.photonView.Owner, place);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// RPC primit de toți clienții pentru a afișa UI-ul cu rezultatele cursei.
    /// Parametrii includ acum timpii ordonați.
    /// </summary>
    /// <param name="orderedActorIDs">Un array de ActorID-uri în ordinea finish-ului (locul 1, 2, etc.).</param>
    /// <param name="orderedTimes">Un array de timpi de finish corespunzători ActorID-urilor ordonate.</param>
    [PunRPC]
    public void RPC_ShowResultsUI(int[] orderedActorIDs, float[] orderedTimes)
    {
        Debug.Log("Received RPC_ShowResultsUI. Displaying results...");
        
        List<Player> orderedPlayers = new List<Player>();
        Player[] allPlayersInRoom = PhotonNetwork.PlayerList;

        // Repopulează playerTimes pe partea clientului din datele primite prin RPC
        playerTimes.Clear();
        for (int i = 0; i < orderedActorIDs.Length; i++)
        {
            playerTimes[orderedActorIDs[i]] = orderedTimes[i];
        }

        foreach (int actorID in orderedActorIDs)
        {
            Player player = allPlayersInRoom.FirstOrDefault(p => p.ActorNumber == actorID);
            if (player != null)
            {
                orderedPlayers.Add(player);
            }
        }

        // Adaugă jucătorii care nu au terminat (e.g., deconectați sau nu au raportat) la sfârșit
        foreach (Player player in allPlayersInRoom)
        {
            if (!orderedPlayers.Any(p => p.ActorNumber == player.ActorNumber))
            {
                orderedPlayers.Add(player);
            }
        }

        // Acum populează UI-ul cu jucătorii ordonați
        DisplayResultsOnUI(orderedPlayers);
    }

    /// <summary>
    /// Populează UI-ul cu rezultatele cursei pe baza listei ordonate de jucători.
    /// </summary>
    /// <param name="orderedPlayers">O listă de obiecte Photon.Realtime.Player, ordonate după rang.</param>
    private void DisplayResultsOnUI(List<Player> orderedPlayers)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true); // Afișează panoul de rezultate
        }

        ClearPlacementsUI(); // Golește intrările anterioare

        // Actualizează plasamentele individuale ale jucătorilor (locul 1, 2, etc.)
        for (int i = 0; i < orderedPlayers.Count; i++)
        {
            if (i < allPlayerPlacementsText.Length && allPlayerPlacementsText[i] != null)
            {
                Player player = orderedPlayers[i];
                int place = i + 1;
                
                // Formatează timpul: MM:SS.ms (e.g., 01:23.456)
                string timeString = "";
                if (playerTimes.ContainsKey(player.ActorNumber))
                {
                    float timeInSeconds = playerTimes[player.ActorNumber];
                    int minutes = Mathf.FloorToInt(timeInSeconds / 60);
                    int seconds = Mathf.FloorToInt(timeInSeconds % 60);
                    int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000) % 1000);
                    timeString = $" (Time: {minutes:00}:{seconds:00}.{milliseconds:000})";
                }
                
                string playerStatus = "";
                if (!playerTimes.ContainsKey(player.ActorNumber)) // Dacă jucătorul nu are un timp înregistrat (DNF)
                {
                    playerStatus = " (DNF)"; // Did Not Finish
                    timeString = ""; // Nu afișa timpul dacă e DNF
                }

                allPlayerPlacementsText[i].text = $"{place}. {player.NickName}{playerStatus}{timeString}";
            }
        }

        // Actualizează textul specific pentru locul jucătorului local
        if (myPlaceText != null && PhotonNetwork.LocalPlayer != null)
        {
            int myActorID = PhotonNetwork.LocalPlayer.ActorNumber;
            int myPlace = -1;
            for (int i = 0; i < orderedPlayers.Count; i++)
            {
                if (orderedPlayers[i].ActorNumber == myActorID)
                {
                    myPlace = i + 1;
                    break;
                }
            }
            if (myPlace != -1)
            {
                myPlaceText.text = $"Your Place: {myPlace}";
            }
            else
            {
                myPlaceText.text = "Your Place: N/A"; // Dacă jucătorul nu este în rezultate (e.g., deconectat sau DNF)
            }
        }
    }

    private void ClearPlacementsUI()
    {
        if (allPlayerPlacementsText != null)
        {
            foreach (var textMesh in allPlayerPlacementsText)
            {
                if (textMesh != null)
                {
                    textMesh.text = "";
                }
            }
        }
    }

    // Golește rezultatele stocate. Utile pentru resetarea unei noi curse.
    public void ResetResults()
    {
        playerTimes.Clear(); // Golește timpii
        finishOrder.Clear();
        ClearPlacementsUI(); // Golește UI-ul la resetare
        if (myPlaceText != null) myPlaceText.text = "";
        if (resultPanel != null) resultPanel.SetActive(false); // Ascunde panoul la resetare
        Debug.Log("Race results reset.");
    }

    /// <summary>
    /// Returnează o listă de obiecte Photon.Realtime.Player în ordinea finală a cursei (de la primul la ultimul).
    /// Această funcție ar trebui apelată de orice client după ce cursa s-a încheiat
    /// și rezultatele au fost sincronizate.
    /// </summary>
    /// <returns>O listă de obiecte Photon.Realtime.Player sortate după ordinea finish-ului lor.</returns>
    public List<Player> GetRaceResults()
    {
        List<Player> results = new List<Player>();
        Player[] allPlayersInRoom = PhotonNetwork.PlayerList;

        foreach (int actorID in finishOrder)
        {
            Player finishedPlayer = allPlayersInRoom.FirstOrDefault(p => p.ActorNumber == actorID);
            if (finishedPlayer != null)
            {
                results.Add(finishedPlayer);
            }
        }
        
        foreach (Player player in allPlayersInRoom)
        {
            if (!finishOrder.Contains(player.ActorNumber))
            {
                results.Add(player);
            }
        }

        return results;
    }


    /// <summary>
    /// Gestionează click-ul butonului "Back to Lobby".
    /// Părăsește camera Photon curentă și încarcă scena MainMenu.
    /// </summary>
    public void BackToLobby()
    {
        Debug.Log("Attempting to leave room and go to MainMenu...");
        PhotonNetwork.LeaveRoom(); // Cere să părăsească camera Photon curentă
    }

    /// <summary>
    /// Acest callback este declanșat când clientul părăsește cu succes o cameră.
    /// </summary>
    public override void OnLeftRoom()
    {
        Debug.Log("Successfully left the Photon room. Loading MainMenu scene.");
        // Asigură-te că ai o scenă numită "MainMenu" în Build Settings
        PhotonNetwork.LoadLevel("MainMenu"); 
    }

    /// <summary>
    /// Gestionează click-ul butonului "Quit Game".
    /// Deconectează de la Photon și închide aplicația.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Attempting to quit game...");

        // Deconectează de la Photon mai întâi pentru o ieșire curată
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        // Închide aplicația
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Oprește modul de joc în editor
#else
        Application.Quit(); // Închide aplicația construită
#endif
    }
}