using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Asigură-te că folosești TextMeshPro pentru text
using ExitGames.Client.Photon; // Necesare pentru Hashtable

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

    // --- CONSTANTA PENTRU PROPRIETĂȚI CUSTOM ALE CAMEREI ---
    public const string LAPS_KEY = "Laps"; // Cheia pentru numărul de ture
    public const string CATEGORIE_KEY = "Category"; // Am mutat CATEGORIE_KEY aici pentru consistență

    public static int Laps = 3; // Variabila statică, acum va fi sincronizată
    private bool isCategorySelected = false; // Flag care indică dacă o categorie a fost selectată

    private string currentCategory; // Variabila locală pentru categoria curentă (redenumită pentru a evita conflict cu o metodă)

    private void Awake()
    {
        currentCategory = null;
        // Asigură-te că lapsInput este interactiv doar pentru Master Client
        if (lapsInput != null)
        {
            lapsInput.interactable = PhotonNetwork.IsMasterClient;
        }

        startButton.SetActive(PhotonNetwork.IsMasterClient); // Doar Master Client-ul vede butonul de Start
        // Nu DontDestroyOnLoad(gameObject); dacă este un UI specific lobby-ului și se distruge la schimbarea scenei.
        // Dacă acest script controlează UI-ul care se distruge, nu are sens să-l păstrezi.
        // Dacă GameMenuController este un manager global care persistă, atunci păstrează-l.
        // Pentru un UI de Lobby care dispare la StartGame, scoate DontDestroyOnLoad.
        // Presupunând că GameMenuController este un script al scenei de Lobby și se distruge la schimbarea scenei.
        // Dacă nu se distruge, atunci ar putea intra în conflict cu alte scripturi de UI din scena de joc.
    }

    private void Start()
    {
        // Adaugă un listener pentru sunetul de tastare când textul din lapsInput se schimbă
        if (lapsInput != null)
        {
            lapsInput.onValueChanged.AddListener(OnLapsInputChangedLocally); // Apelăm o metodă locală
            lapsInput.onValueChanged.AddListener(_ => PlayTypingSound()); // Sunetul rămâne
        }

        // Doar Master Client-ul poate interacționa cu butoanele de selecție a categoriei
        bool isMaster = PhotonNetwork.IsMasterClient;
        if (CalculationsButton != null) CalculationsButton.interactable = isMaster;
        if (FormulasButton != null) FormulasButton.interactable = isMaster;

        // Adaugă listeners pentru butoanele de categorie
        if (CalculationsButton != null) CalculationsButton.onClick.AddListener(() => OnCategoryButtonClicked("calcule"));
        if (FormulasButton != null) FormulasButton.onClick.AddListener(() => OnCategoryButtonClicked("formule"));


        // Dezactivează tranzițiile vizuale pentru non-Master Clients (pentru a evita efecte nedorite)
        if (!isMaster)
        {
            if (CalculationsButton != null) CalculationsButton.transition = Selectable.Transition.None;
            if (FormulasButton != null) FormulasButton.transition = Selectable.Transition.None;
        }

        UpdateRoomCode(); // Actualizează codul camerei

        // Ascunde mesajul de eroare la începutul scenei
        if (errorMsg != null)
            errorMsg.SetActive(false);

        // La intrarea în cameră, sincronizează setările inițiale de la Master Client
        // Verifică proprietățile camerei pentru Laps și Category
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(LAPS_KEY))
            {
                Laps = (int)PhotonNetwork.CurrentRoom.CustomProperties[LAPS_KEY];
                lapsInput.text = Laps.ToString(); // Actualizează vizual lapsInput
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(CATEGORIE_KEY))
            {
                string initialCategory = PhotonNetwork.CurrentRoom.CustomProperties[CATEGORIE_KEY]?.ToString();
                if (!string.IsNullOrEmpty(initialCategory))
                {
                    UpdateCategoryUI(initialCategory); // Actualizează vizual categoria
                }
            }
        }
        else
        {
             // Fallback dacă nu suntem încă în cameră la Start (nu ar trebui să se întâmple în lobby)
             // sau dacă scriptul persistă între scene.
             // Pentru lobby, acest Start se cheamă după OnJoinedRoom.
        }

        // Conectează butoanele "Start Race" și "Leave Room" prin cod
        if (StartRaceButton != null) StartRaceButton.onClick.AddListener(StartGame);
        if (LeaveRoomButton != null) LeaveRoomButton.onClick.AddListener(LeaveRoom);
    }

    private void Update()
    {
        UpdatePlayerCount(); // Actualizează numărul de jucători
        UpdatePlayerNames(); // Actualizează numele jucătorilor
    }

    // Metodă pentru a reda sunetul de tastare
    private void PlayTypingSound()
    {
        if (audioSource != null && typingSound != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(typingSound);
        }
    }

    // Metodă pentru a reda sunetul de click
    public void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    // Gestiunea inputului pentru numărul de ture (Laps) - apelată local de Master Client
    public void OnLapsInputChangedLocally(string newLapsText)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Doar Master Client-ul poate modifica

        int inputLaps;
        if (int.TryParse(newLapsText, out inputLaps))
        {
            if (inputLaps < 1)
            {
                inputLaps = 1;
                lapsInput.text = "1"; // Actualizează vizual la 1
            }
            Laps = inputLaps; // Actualizează variabila statică local

            // --- Sincronizează numărul de ture prin proprietățile camerei ---
            Hashtable customProperties = new Hashtable();
            customProperties[LAPS_KEY] = Laps;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            Debug.Log($"Master Client set Laps to: {Laps}");
        }
        else
        {
            // Dacă inputul nu e număr, setează la 1 și sincronizează
            lapsInput.text = "1";
            Laps = 1;
            Hashtable customProperties = new Hashtable();
            customProperties[LAPS_KEY] = Laps;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            Debug.Log("Invalid Laps input, set to 1 and synchronized.");
        }
    }

    // Metodă apelată când un buton de categorie este apăsat (doar de Master Client)
    public void OnCategoryButtonClicked(string categoryName)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Doar Master Client-ul poate schimba categoria

        PlayClickSound();

        // Actualizează proprietățile camerei
        Hashtable customProperties = new Hashtable();
        customProperties[CATEGORIE_KEY] = categoryName;
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        Debug.Log($"Master Client set category to: {categoryName}");
    }


    // Metodă pentru a selecta vizual o categorie (apelată local sau din rețea)
    private void UpdateCategoryUI(string categoryToDisplay) // Redenumită pentru claritate
    {
        // Se asigură că actualizarea are loc doar dacă noua categorie este diferită
        if (categoryToDisplay != null && categoryToDisplay != currentCategory)
        {
            isCategorySelected = true; // Confirmă că o categorie este selectată
            currentCategory = categoryToDisplay; // Actualizează variabila locală

            // Activează/dezactivează butoanele și schimbă sprite-urile
            if (categoryToDisplay == "calcule")
            {
                if (CalculationsButton != null) CalculationsButton.image.sprite = calculeSelected;
                if (FormulasButton != null) FormulasButton.image.sprite = formuleUnselected;

                // Doar Master Client-ul poate interacționa cu butoanele, dar vizual se schimbă pentru toți
                if (PhotonNetwork.IsMasterClient)
                {
                    if (CalculationsButton != null) CalculationsButton.interactable = false;
                    if (FormulasButton != null) FormulasButton.interactable = true;
                }
            }
            else if (categoryToDisplay == "formule")
            {
                if (CalculationsButton != null) CalculationsButton.image.sprite = calculeUnselected;
                if (FormulasButton != null) FormulasButton.image.sprite = formuleSelected;

                if (PhotonNetwork.IsMasterClient)
                {
                    if (CalculationsButton != null) CalculationsButton.interactable = true;
                    if (FormulasButton != null) FormulasButton.interactable = false;
                }
            }
            Debug.Log("Category UI updated to: " + categoryToDisplay);
        }
    }

    // Metoda apelată când se apasă butonul de Start
    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return; // Doar Master Client-ul poate porni jocul

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

        // Asigură-te că toți jucătorii sunt pregătiți sau numărul minim de jucători este atins
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2) // Exemplu: minim 2 jucători pentru a porni
        {
            Debug.LogWarning("Not enough players to start the game!");
             if (errorMsg != null)
            {
                errorMsg.GetComponent<TMP_Text>().text = "Need at least 2 players!"; // Actualizează textul erorii
                errorMsg.SetActive(true);
                CancelInvoke(nameof(HideErrorMsg));
                Invoke(nameof(HideErrorMsg), 3f);
            }
            return;
        }

        // Dacă totul este în regulă, încarcă scena de joc
        PhotonNetwork.LoadLevel("Game"); 
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
        if (PhotonNetwork.CurrentRoom != null && playerCount != null)
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
            if (playerTexts[i] != null) playerTexts[i].text = "";
        }

        // Populează sloturile cu numele jucătorilor prezenți în cameră
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length && i < playerTexts.Length; i++)
        {
            if (playerTexts[i] != null)
            {
                string displayName = players[i].NickName;
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = "Player " + (i + 1); // Fallback dacă NickName e gol
                }

                if (players[i] == PhotonNetwork.LocalPlayer)
                {
                    displayName += " (YOU)"; // Adaugă "(YOU)" pentru jucătorul local
                }
                playerTexts[i].text = displayName;
            }
        }
    }

    // Actualizează textul care afișează codul camerei
    private void UpdateRoomCode()
    {
        if (PhotonNetwork.CurrentRoom != null && roomCodeText != null)
        {
            roomCodeText.text = "Room Code: " + PhotonNetwork.CurrentRoom.Name;
        }
    }

    // --- CALLBACKS PHOTON ---

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} entered room.");
        UpdatePlayerNames();
        UpdatePlayerCount();
        // Când un jucător nou intră, Master Client-ul re-trimite proprietățile camerei
        // pentru a asigura sincronizarea instantanee a Laps și Category pentru noul venit.
        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable customProperties = new Hashtable();
            customProperties[LAPS_KEY] = Laps; // Laps curent (setat de Master Client)
            customProperties[CATEGORIE_KEY] = currentCategory; // Categoria curentă (setată de Master Client)
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} left room.");
        UpdatePlayerNames();
        UpdatePlayerCount();
        // Dacă Master Client-ul părăsește camera, noul Master Client va fi anunțat.
        // Noul Master Client va trebui să se asigure că setările sunt corecte.
        if (PhotonNetwork.IsMasterClient)
        {
            // Noul Master Client devine activ
            startButton.SetActive(true);
            lapsInput.interactable = true;
            if (CalculationsButton != null) CalculationsButton.interactable = true; // Re-activează butoanele de categorie
            if (FormulasButton != null) FormulasButton.interactable = true;
            // Și își asumă controlul vizual
            if (CalculationsButton != null) CalculationsButton.transition = Selectable.Transition.ColorTint; // Restabilește tranzițiile
            if (FormulasButton != null) FormulasButton.transition = Selectable.Transition.ColorTint;

            // Ar trebui să re-sincronizeze laps-urile și categoria, în cazul în care au fost setate anterior.
            // Acestea ar trebui să fie deja în proprietățile camerei, dar o re-setare nu strică.
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(LAPS_KEY))
            {
                Hashtable customProps = new Hashtable();
                customProps[LAPS_KEY] = Laps; // Folosește valoarea Laps locală
                PhotonNetwork.CurrentRoom.SetCustomProperties(customProps);
            }
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(CATEGORIE_KEY))
            {
                Hashtable customProps = new Hashtable();
                customProps[CATEGORIE_KEY] = currentCategory; // Folosește valoarea categoriei locale
                PhotonNetwork.CurrentRoom.SetCustomProperties(customProps);
            }
        }
        else // Non-Master client
        {
            startButton.SetActive(false);
            lapsInput.interactable = false;
            if (CalculationsButton != null) CalculationsButton.interactable = false;
            if (FormulasButton != null) FormulasButton.interactable = false;
            // Dezactivează tranzițiile vizuale
            if (CalculationsButton != null) CalculationsButton.transition = Selectable.Transition.None;
            if (FormulasButton != null) FormulasButton.transition = Selectable.Transition.None;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Master Client switched to: {newMasterClient.NickName}");
        // La schimbarea Master Client-ului, actualizează starea UI-ului
        startButton.SetActive(PhotonNetwork.IsMasterClient);
        lapsInput.interactable = PhotonNetwork.IsMasterClient;

        bool isMaster = PhotonNetwork.IsMasterClient;
        if (CalculationsButton != null) CalculationsButton.interactable = isMaster;
        if (FormulasButton != null) FormulasButton.interactable = isMaster;

        if (!isMaster)
        {
            if (CalculationsButton != null) CalculationsButton.transition = Selectable.Transition.None;
            if (FormulasButton != null) FormulasButton.transition = Selectable.Transition.None;
        }
        else
        {
            if (CalculationsButton != null) CalculationsButton.transition = Selectable.Transition.ColorTint;
            if (FormulasButton != null) FormulasButton.transition = Selectable.Transition.ColorTint;
        }

        // Asigură-te că Laps și Category sunt preluate de noul Master Client
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(LAPS_KEY))
        {
            Laps = (int)PhotonNetwork.CurrentRoom.CustomProperties[LAPS_KEY];
            lapsInput.text = Laps.ToString();
        }
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(CATEGORIE_KEY))
        {
            string newCategory = PhotonNetwork.CurrentRoom.CustomProperties[CATEGORIE_KEY]?.ToString();
            UpdateCategoryUI(newCategory);
        }
    }

    // Callback apelat când proprietățile camerei sunt actualizate (inclusiv categoria și turele)
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log("Room Properties Updated.");
        if (changedProps.ContainsKey(CATEGORIE_KEY))
        {
            string newCategory = changedProps[CATEGORIE_KEY]?.ToString();
            if (!string.IsNullOrEmpty(newCategory))
            {
                UpdateCategoryUI(newCategory); // Aici se apelează funcția de actualizare a categoriei
            }
        }

        if (changedProps.ContainsKey(LAPS_KEY))
        {
            Laps = (int)changedProps[LAPS_KEY];
            if (lapsInput != null)
            {
                lapsInput.text = Laps.ToString(); // Actualizează vizual lapsInput pentru toți clienții
            }
            Debug.Log($"Laps updated to: {Laps}");
        }
    }

    // Metodă pentru a părăsi camera
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MainMenu"); // Asigură-te că "MainMenu" este scena corectă
    }

    // Metodă pentru a dezactiva vizual butoanele (dacă este necesar)
    // Acum, această metodă nu ar trebui să mai fie necesară în mod direct
    // deoarece `interactable` și `transition` sunt gestionate în `Start` și `OnMasterClientSwitched`.
    /*
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
    */
}