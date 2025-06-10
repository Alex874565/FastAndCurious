using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Asigură-te că folosești TextMeshPro pentru text

public class GameMenuController : MonoBehaviourPunCallbacks
{
    public TMP_Text playerCount;
    public TMP_Text roomCodeText;

    public TMP_Text[] playerTexts; // Drag & Drop Player1, Player2, Player3, Player4 în Inspector

    public GameObject startButton;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip typingSound;

    [SerializeField] private TMP_InputField lapsInput; // Ai folosit TMP_InputField, foarte bine!

    [SerializeField] private Button StartRaceButton;
    [SerializeField] private Button LeaveRoomButton;
    [SerializeField] private Button CalculationsButton; // Butonul pentru "Calcule"
    [SerializeField] private Button FormulasButton;     // Butonul pentru "Formule"

    [SerializeField] private Sprite formuleSelected;
    [SerializeField] private Sprite formuleUnselected;
    [SerializeField] private Sprite calculeSelected;
    [SerializeField] private Sprite calculeUnselected;

    [SerializeField] private GameObject errorMsg; // GameObject-ul mesajului de eroare, trebuie să fie dezactivat inițial în Inspector

    public static int Laps = 3; // Default
    private bool isCategorySelected = false; // Flag care indică dacă o categorie a fost selectată

    private string category; // Variabila locală pentru categoria curentă

    private void Awake()
    {
        category = null;
        startButton.SetActive(PhotonNetwork.IsMasterClient); // Doar Master Client-ul vede butonul de Start
        DontDestroyOnLoad(gameObject); // Păstrează acest GameObject la schimbarea scenelor
    }

    private void Start()
    {
        // Adaugă un listener pentru sunetul de tastare când textul din lapsInput se schimbă
        lapsInput.onValueChanged.AddListener(_ => PlayTypingSound());

        // Doar Master Client-ul poate interacționa cu butoanele de selecție a categoriei
        bool isMaster = PhotonNetwork.IsMasterClient;
        CalculationsButton.interactable = isMaster;
        FormulasButton.interactable = isMaster;

        // Dezactivează tranzițiile vizuale pentru non-Master Clients (pentru a evita efecte nedorite)
        if (!isMaster)
        {
            CalculationsButton.transition = Selectable.Transition.None;
            FormulasButton.transition = Selectable.Transition.None;
        }

        UpdateRoomCode(); // Actualizează codul camerei

        // Ascunde mesajul de eroare la începutul scenei
        if (errorMsg != null)
            errorMsg.SetActive(false);

        // Forțează actualizarea vizuală a categoriei dacă aceasta a fost deja setată în rețea
        string initialCategory = CategorieSyncManager.GetCategorie();
        if (!string.IsNullOrEmpty(initialCategory))
        {
            UpdateCategory(initialCategory);
        }
    }

    private void Update()
    {
        UpdatePlayerCount(); // Actualizează numărul de jucători
        UpdatePlayerNames(); // Actualizează numele jucătorilor
    }

    // Metodă pentru a reda sunetul de tastare
    private void PlayTypingSound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(typingSound);
        }
    }

    // Metodă pentru a reda sunetul de click
    public void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound);
    }

    // Gestiunea inputului pentru numărul de ture (Laps)
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

    // Metodă pentru a selecta vizual o categorie (apelată local sau din rețea)
    private void SelectCategory(string category)
    {
        isCategorySelected = true; // Setează flag-ul că o categorie a fost aleasă
        PlayClickSound();
        if (category == "calcule")
        {
            // Dezactivează butonul selectat și activează celălalt, schimbă sprite-urile
            CalculationsButton.interactable = false;
            FormulasButton.interactable = true;
            CalculationsButton.image.sprite = calculeSelected;
            FormulasButton.image.sprite = formuleUnselected;
        }
        else if (category == "formule")
        {
            CalculationsButton.interactable = true;
            FormulasButton.interactable = false;
            FormulasButton.image.sprite = formuleSelected;
            CalculationsButton.image.sprite = calculeUnselected;
        }
        Debug.Log("Category selected: " + category);
    }

    // Metodă apelată când categoria se actualizează (inclusiv din rețea)
    private void UpdateCategory(string new_category)
    {
        // Se asigură că actualizarea are loc doar dacă noua categorie este diferită
        if (new_category != null && new_category != category)
        {
            isCategorySelected = true; // Re-confirmă că o categorie este selectată
            category = new_category;
            SelectCategory(category); // Actualizează vizual butoanele
        }
    }

    // Metoda apelată când se apasă butonul de Start
    public void StartGame()
    {
        if (!isCategorySelected) // Aici se verifică dacă o categorie a fost selectată
        {
            Debug.LogWarning("Please select a category before starting the game!");

            if (errorMsg != null)
            {
                errorMsg.SetActive(true); // Afișează mesajul de eroare
                CancelInvoke(nameof(HideErrorMsg)); // Anulează orice invocare anterioară a HideErrorMsg
                Invoke(nameof(HideErrorMsg), 3f); // Programează ascunderea mesajului după 3 secunde
            }
            return; // Oprește execuția funcției, jocul nu va porni
        }

        PhotonNetwork.LoadLevel("Game"); // Începe jocul dacă o categorie este selectată
    }

    // Metodă pentru a ascunde mesajul de eroare
    private void HideErrorMsg()
    {
        if (errorMsg != null)
            errorMsg.SetActive(false);
    }

    // Actualizează textul care afișează numărul de jucători
    public void UpdatePlayerCount()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            playerCount.text = PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + " players";
        }
    }

    // Actualizează numele jucătorilor în UI
    private void UpdatePlayerNames()
    {
        // Curăță toate sloturile de jucători la început
        for (int i = 0; i < playerTexts.Length; i++)
        {
            playerTexts[i].text = "";
        }

        // Populează sloturile cu numele jucătorilor prezenți în cameră
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length && i < playerTexts.Length; i++)
        {
            string displayName = players[i].NickName;
            if (players[i] == PhotonNetwork.LocalPlayer)
            {
                displayName += " (YOU)"; // Adaugă "(YOU)" pentru jucătorul local
            }
            playerTexts[i].text = displayName;
        }
    }

    // Actualizează textul care afișează codul camerei
    private void UpdateRoomCode()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            roomCodeText.text = "Room Code: " + PhotonNetwork.CurrentRoom.Name;
        }
    }

    // Callback-uri Photon
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

    // Metodă pentru a părăsi camera
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MainMenu");
    }

    // Callback apelat când proprietățile camerei sunt actualizate (inclusiv categoria)
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey(CategorieSyncManager.CATEGORIE_KEY))
        {
            string newCategory = changedProps[CategorieSyncManager.CATEGORIE_KEY]?.ToString();
            if (!string.IsNullOrEmpty(newCategory))
            {
                UpdateCategory(newCategory); // Aici se apelează funcția de actualizare a categoriei
            }
        }
    }

    // Metodă pentru a dezactiva vizual butoanele (dacă este necesar)
    void DisableButtonVisuals(Button button, Sprite sprite)
    {
        var spriteState = new SpriteState
        {
            highlightedSprite = sprite,
            pressedSprite = sprite,
            selectedSprite = sprite,
            disabledSprite = sprite
        };
        button.spriteState = spriteState;
        button.image.sprite = sprite;
        button.interactable = false;
    }
}