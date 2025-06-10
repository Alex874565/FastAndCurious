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

    private string currentCategory; // Variabila locală pentru categoria curentă

    private bool isLoadingScene = false; // Flag pentru a preveni încărcări multiple ale scenei

    private void Awake()
    {
        currentCategory = null;

        // Asigură-te că lapsInput este interactiv doar pentru Master Client
        if (lapsInput != null)
        {
            lapsInput.interactable = PhotonNetwork.IsMasterClient;
        }

        // Butonul de Start este activ doar pentru Master Client
        if (startButton != null)
        {
            startButton.SetActive(PhotonNetwork.IsMasterClient);
        }

        // COMENTAT/ȘTERS: Nu DontDestroyOnLoad(gameObject); pentru un controller specific scenei de lobby.
        // Dacă acest script controlează UI-ul care se distruge, nu are sens să-l păstrezi.
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Asigură-te că isLoadingScene este false la începutul scenei de lobby
        isLoadingScene = false;

        // Adaugă un listener pentru sunetul de tastare când textul din lapsInput se schimbă
        if (lapsInput != null)
        {
            lapsInput.onValueChanged.AddListener(OnLapsInputChangedLocally); // Apelăm o metodă locală
            lapsInput.onValueChanged.AddListener(_ => PlayTypingSound()); // Sunetul rămâne
        }

        // Butoanele de categorie sunt interactive doar pentru Master Client
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
        else // Pentru Master Client, asigură-te că tranzițiile sunt active
        {
            if (CalculationsButton != null) CalculationsButton.transition = Selectable.Transition.ColorTint;
            if (FormulasButton != null) FormulasButton.transition = Selectable.Transition.ColorTint;
        }


        UpdateRoomCode(); // Actualizează codul camerei

        // Ascunde mesajul de eroare la începutul scenei
        if (errorMsg != null)
            errorMsg.SetActive(false);

        // La intrarea în cameră (sau la încărcarea scenei de lobby), sincronizează setările inițiale de la Master Client
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(LAPS_KEY))
            {
                Laps = (int)PhotonNetwork.CurrentRoom.CustomProperties[LAPS_KEY];
                if (lapsInput != null) lapsInput.text = Laps.ToString(); // Actualizează vizual lapsInput
            }
            else // Dacă nu există Laps în proprietățile camerei, setează-l (doar Master Client-ul)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    Hashtable initialLapsProps = new Hashtable();
                    initialLapsProps[LAPS_KEY] = Laps; // Folosește valoarea default din variabila statică
                    PhotonNetwork.CurrentRoom.SetCustomProperties(initialLapsProps);
                }
            }

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(CATEGORIE_KEY))
            {
                string initialCategory = PhotonNetwork.CurrentRoom.CustomProperties[CATEGORIE_KEY]?.ToString();
                if (!string.IsNullOrEmpty(initialCategory))
                {
                    UpdateCategoryUI(initialCategory); // Actualizează vizual categoria
                }
            }
            else // Dacă nu există Category în proprietățile camerei, setează-l (doar Master Client-ul)
            {
                if (PhotonNetwork.IsMasterClient && string.IsNullOrEmpty(currentCategory))
                {
                    // Asigură-te că currentCategory este setat la o valoare implicită sau la null inițial
                    // În acest caz, nu setăm o categorie default, ci așteptăm ca MC să aleagă.
                    // Această ramură ar trebui să se întâmple doar dacă nimeni nu a ales încă o categorie.
                }
            }
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
                if (lapsInput != null) lapsInput.text = "1"; // Actualizează vizual la 1
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
            if (lapsInput != null) lapsInput.text = "1";
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

        if (isLoadingScene) // Previne apelurile multiple dacă deja se încarcă o scenă
        {
            Debug.LogWarning("Scene load already in progress or initiated.");
            return;
        }

        if (!isCategorySelected)
        {
            Debug.LogWarning("Please select a category before starting the game!");
            if (errorMsg != null)
            {
                errorMsg.SetActive(true); // Afișează mesajul de eroare
                CancelInvoke(nameof(HideErrorMsg)); // Anulează orice invocare anterioară a HideErrorMsg
                Invoke(nameof(HideErrorMsg), 3f); // Programează ascunderea mesajului după 3 secunde
            }
            return;
        }

        // --- ELIMINAT: Verificarea numărului minim de jucători ---
        // Acum poți juca și singur.
        // if (PhotonNetwork.CurrentRoom.PlayerCount < 2) { ... return; }

        // Dacă totul este în regulă, setează flag-ul și încarcă scena
        isLoadingScene = true;
        // Opțional: Dezactivează butonul de Start imediat după ce ai apăsat,
        // pentru a oferi feedback vizual utilizatorului
        if (StartRaceButton != null)
        {
            StartRaceButton.interactable = false;
        }

        Debug.Log("Master Client initiating scene load: Game");
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
        
        // La plecarea unui jucător, actualizează starea UI-ului în funcție de noul Master Client
        // Sau asigură-te că non-Master Client-ii își pierd interactivitatea
        ApplyMasterClientUIState();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Master Client switched to: {newMasterClient.NickName}");
        // La schimbarea Master Client-ului, actualizează starea UI-ului pentru toți
        ApplyMasterClientUIState();

        // Noul Master Client trebuie să se asigure că Laps și Category sunt preluate din proprietățile camerei
        // sau setate dacă lipsesc (de exemplu, dacă vechiul MC a părăsit înainte de a seta).
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(LAPS_KEY))
                {
                    Laps = (int)PhotonNetwork.CurrentRoom.CustomProperties[LAPS_KEY];
                    if (lapsInput != null) lapsInput.text = Laps.ToString();
                }
                else // Dacă nu există, noul MC setează default-ul
                {
                    Hashtable initialLapsProps = new Hashtable();
                    initialLapsProps[LAPS_KEY] = Laps;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(initialLapsProps);
                }

                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(CATEGORIE_KEY))
                {
                    string newCategory = PhotonNetwork.CurrentRoom.CustomProperties[CATEGORIE_KEY]?.ToString();
                    UpdateCategoryUI(newCategory);
                }
                // Dacă nu există categorie, noul MC va alege una sau va aștepta.
            }
        }
    }

    // Metodă auxiliară pentru a aplica starea UI-ului în funcție de rolul de Master Client
    private void ApplyMasterClientUIState()
    {
        bool isMaster = PhotonNetwork.IsMasterClient;

        if (startButton != null) startButton.SetActive(isMaster);
        if (lapsInput != null) lapsInput.interactable = isMaster;

        if (CalculationsButton != null) CalculationsButton.interactable = isMaster;
        if (FormulasButton != null) FormulasButton.interactable = isMaster;

        // Gestionează tranzițiile vizuale ale butoanelor de categorie
        if (isMaster)
        {
            if (CalculationsButton != null) CalculationsButton.transition = Selectable.Transition.ColorTint;
            if (FormulasButton != null) FormulasButton.transition = Selectable.Transition.ColorTint;
        }
        else
        {
            if (CalculationsButton != null) CalculationsButton.transition = Selectable.Transition.None;
            if (FormulasButton != null) FormulasButton.transition = Selectable.Transition.None;
        }

        // Asigură-te că selecția categoriei este vizualizată corect după schimbarea MC
        if (!string.IsNullOrEmpty(currentCategory))
        {
            UpdateCategoryUI(currentCategory); // Re-aplică vizualizarea categoriei curente
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
        // PhotonNetwork.LoadLevel("MainMenu"); // Scena se va încărca în OnLeftRoom
    }
}