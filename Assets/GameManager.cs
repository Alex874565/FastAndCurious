using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    public IntrebareData intrebareDB;
    public TMP_Text textIntrebare;
    public Button[] butoaneRaspuns;

    [SerializeField] private GameObject questionCanvas;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    private Intrebare intrebareCurenta;
    private Action onQuestionAnsweredCorrectly;
    private List<Intrebare> intrebariDisponibile;
    private List<Intrebare> intrebariFolosite;

    private bool isPaused = false;

    [Header("Car Colors")]
    public List<Color> availableCarColors = new List<Color>()
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        new Color(1f, 0.5f, 0f), // Orange
        new Color(0.5f, 0f, 0.5f) // Purple
    };

    public const string PLAYER_COLOR_INDEX_KEY = "PlayerColorIndex";

    void Start()
    {
        questionCanvas.SetActive(false);
        pauseMenu.SetActive(false);

        resumeButton.onClick.RemoveAllListeners();
        resumeButton.onClick.AddListener(ResumeGame);

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC Pressed");
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
    }

    private void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MainMenu"); // asigură-te că această scenă există
    }

    // Gestionare întrebări
    public void StartQuestion(Action callback)
    {
        onQuestionAnsweredCorrectly = callback;
        intrebariFolosite = new List<Intrebare>();

        string categorieGlobala = PhotonNetwork.CurrentRoom.CustomProperties[CategorieSyncManager.CATEGORIE_KEY] as string;

        if (string.IsNullOrEmpty(categorieGlobala))
        {
            Debug.LogWarning("Categoria nu este setată de master.");
            return;
        }

        intrebariDisponibile = intrebareDB.intrebari
            .Where(i => i.categorie == categorieGlobala)
            .ToList();

        if (intrebariDisponibile.Count == 0)
        {
            Debug.LogWarning("Nu există întrebări pentru categoria: " + categorieGlobala);
            return;
        }

        GenerareSiAfisareIntrebareNoua();
    }

    private void GenerareSiAfisareIntrebareNoua()
    {
        List<Intrebare> intrebariRamase = intrebariDisponibile.Except(intrebariFolosite).ToList();

        if (intrebariRamase.Count == 0)
        {
            intrebariFolosite.Clear();
            intrebariRamase = intrebariDisponibile.ToList();
        }

        intrebareCurenta = intrebariRamase[UnityEngine.Random.Range(0, intrebariRamase.Count)];
        intrebariFolosite.Add(intrebareCurenta);
        AfiseazaIntrebarea();
    }

    [PunRPC]
    void RPC_TrimiteIntrebare(string text, string[] variante, int indexCorect, string categorie)
    {
        intrebareCurenta = new Intrebare
        {
            text = text,
            variante = variante,
            indexCorect = indexCorect,
            categorie = categorie
        };

        AfiseazaIntrebarea();
    }

    void AfiseazaIntrebarea()
    {
        questionCanvas.SetActive(true);
        textIntrebare.text = intrebareCurenta.text;

        for (int i = 0; i < butoaneRaspuns.Length; i++)
        {
            int index = i;
            butoaneRaspuns[i].GetComponentInChildren<TMP_Text>().text = intrebareCurenta.variante[i];
            butoaneRaspuns[i].onClick.RemoveAllListeners();
            butoaneRaspuns[i].onClick.AddListener(() => VerificaRaspuns(index));
        }
    }

    void VerificaRaspuns(int indexAles)
    {
        if (indexAles == intrebareCurenta.indexCorect)
        {
            Debug.Log("Corect!");
            questionCanvas.SetActive(false);
            onQuestionAnsweredCorrectly?.Invoke();
        }
        else
        {
            Debug.Log("Greșit! Se încarcă o nouă întrebare...");
            GenerareSiAfisareIntrebareNoua();
        }
    }

    // Photon Events: Player Color Management
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AssignUniqueColorToPlayer(newPlayer);
        }
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AssignUniqueColorToPlayer(PhotonNetwork.LocalPlayer);
        }
    }

    private void AssignUniqueColorToPlayer(Player targetPlayer)
    {
        List<int> usedColorIndexes = new List<int>();
        foreach (var p in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (p.CustomProperties.ContainsKey(PLAYER_COLOR_INDEX_KEY))
            {
                usedColorIndexes.Add((int)p.CustomProperties[PLAYER_COLOR_INDEX_KEY]);
            }
        }

        int assignedColorIndex = -1;
        for (int i = 0; i < availableCarColors.Count; i++)
        {
            if (!usedColorIndexes.Contains(i))
            {
                assignedColorIndex = i;
                break;
            }
        }

        if (assignedColorIndex == -1)
        {
            Debug.LogWarning("Nu mai sunt culori unice disponibile. Reutilizăm culori.");
            assignedColorIndex = UnityEngine.Random.Range(0, availableCarColors.Count);
        }

        Hashtable playerProps = new Hashtable();
        playerProps.Add(PLAYER_COLOR_INDEX_KEY, assignedColorIndex);
        targetPlayer.SetCustomProperties(playerProps);

        Debug.Log($"Master Client a atribuit culoarea {availableCarColors[assignedColorIndex]} (index {assignedColorIndex}) jucătorului {targetPlayer.NickName}");
    }

    public void AssignCarColorOnInstantiate(GameObject playerCarGameObject, Player playerOwner)
    {
        PlayerBehaviour playerBehaviour = playerCarGameObject.GetComponent<PlayerBehaviour>();
        if (playerBehaviour != null)
        {
            if (playerOwner.CustomProperties.ContainsKey(PLAYER_COLOR_INDEX_KEY))
            {
                int colorIndex = (int)playerOwner.CustomProperties[PLAYER_COLOR_INDEX_KEY];
                if (colorIndex >= 0 && colorIndex < availableCarColors.Count)
                {
                    Color assignedColor = availableCarColors[colorIndex];
                    playerBehaviour.photonView.RPC("RPC_SetCarColor", RpcTarget.AllBuffered, assignedColor.r, assignedColor.g, assignedColor.b);
                    Debug.Log($"Aplicat culoarea {assignedColor} la mașina lui {playerOwner.NickName}");
                }
                else
                {
                    Debug.LogError($"Index de culoare invalid: {colorIndex} pentru jucătorul {playerOwner.NickName}");
                }
            }
            else
            {
                Debug.LogWarning($"Jucătorul {playerOwner.NickName} nu are o proprietate de culoare setată. Atribuim o culoare aleatorie.");
                Color fallbackColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                playerBehaviour.photonView.RPC("RPC_SetCarColor", RpcTarget.AllBuffered, fallbackColor.r, fallbackColor.g, fallbackColor.b);
            }
        }
        else
        {
            Debug.LogError("PlayerBehaviour component not found on instantiated car object.");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Jucătorul {otherPlayer.NickName} a părăsit camera.");
    }
}
