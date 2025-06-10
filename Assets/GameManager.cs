using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    public IntrebareData intrebareDB;
    public TMP_Text textIntrebare;
    public Button[] butoaneRaspuns;

    [SerializeField] private GameObject questionCanvas;

    private Intrebare intrebareCurenta;
    private Action onQuestionAnsweredCorrectly;
    private List<Intrebare> intrebariDisponibile; // Lista de întrebări disponibile pentru categoria curentă
    private List<Intrebare> intrebariFolosite; // Lista de întrebări deja folosite






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



    // O cheie custom pentru proprietățile jucătorilor, pentru a stoca indexul culorii
    public const string PLAYER_COLOR_INDEX_KEY = "PlayerColorIndex";




    void Start()
    {
        questionCanvas.SetActive(false);
    }

    public void StartQuestion(Action callback)
    {
        onQuestionAnsweredCorrectly = callback;
        intrebariFolosite = new List<Intrebare>(); // Resetează lista de întrebări folosite la fiecare început de sesiune

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
        // Filtrează întrebările disponibile pentru a exclude cele deja folosite
        List<Intrebare> intrebariRamase = intrebariDisponibile.Except(intrebariFolosite).ToList();

        if (intrebariRamase.Count == 0)
        {
            Debug.LogWarning("Nu mai sunt întrebări disponibile pentru această categorie.");
            // se reseteaza intrebarile folosite si genereaza una noua.
            intrebariFolosite.Clear();
            intrebariRamase = intrebariDisponibile.ToList(); // Reincarca toate intrebarile daca s-au epuizat
            if (intrebariRamase.Count == 0) // O ultima verificare daca nu exista deloc întrebari
            {
                Debug.LogError("Nici o întrebare disponibilă chiar și după resetare. Verificați baza de date.");
                return;
            }
        }

        intrebareCurenta = intrebariRamase[UnityEngine.Random.Range(0, intrebariRamase.Count)];
        intrebariFolosite.Add(intrebareCurenta); // Adauga intrebarea la lista de intrebari folosite
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
            // Dacă răspunsul este greșit, generează și afișează o nouă întrebare
            GenerareSiAfisareIntrebareNoua();
        }
    }





    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
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

    private void AssignUniqueColorToPlayer(Photon.Realtime.Player targetPlayer)
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

    public void AssignCarColorOnInstantiate(GameObject playerCarGameObject, Photon.Realtime.Player playerOwner)
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
                Debug.LogWarning($"Jucătorul {playerOwner.NickName} nu are o proprietate de culoare setată. Atribuim o culoare implicită sau aleatorie.");
                Color fallbackColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                playerBehaviour.photonView.RPC("RPC_SetCarColor", RpcTarget.AllBuffered, fallbackColor.r, fallbackColor.g, fallbackColor.b);
            }
        }
        else
        {
            Debug.LogError("PlayerBehaviour component not found on instantiated car object.");
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"Jucătorul {otherPlayer.NickName} a părăsit camera.");
    }
}











//

//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
//using System;
//using Photon.Pun;


//public class GameManager : MonoBehaviour
//{
//    public IntrebareData intrebareDB;
//    public TMP_Text textIntrebare;
//    public Button[] butoaneRaspuns;

//    [SerializeField] private GameObject questionCanvas;

//    private Intrebare intrebareCurenta;
//    private Action onQuestionAnsweredCorrectly;
//    private List<Intrebare> filtrate;

//    void Start()
//    {
//        questionCanvas.SetActive(false);
//        //StartCoroutine(LoopIntrebari());
//    }

//    IEnumerator LoopIntrebari()
//    {
//        while (true)
//        {
//            yield return new WaitForSeconds(10f); // așteaptă 10 secunde

//            Time.timeScale = 0f;
//            questionCanvas.SetActive(true);

//            StartQuestion(() =>
//            {
//                questionCanvas.SetActive(false);
//                Time.timeScale = 1f;
//            });

//            while (questionCanvas.activeSelf)
//            {
//                yield return null; // așteaptă până când canvas-ul este dezactivat
//            }
//        }
//    }

//    public void StartQuestion(Action callback)
//    {
//        Debug.Log("StartQuestion called");
//        onQuestionAnsweredCorrectly = callback;

//        string categorie = PhotonNetwork.CurrentRoom.CustomProperties[CategorieSyncManager.CATEGORIE_KEY] as string;

//        if (string.IsNullOrEmpty(categorie))
//        {
//            Debug.LogWarning("Categoria nu este setată în Custom Properties!");
//            return;
//        }

//        List<Intrebare> filtrate = intrebareDB.intrebari
//            .Where(i => i.categorie == categorie)
//            .ToList();


//        if (filtrate.Count == 0)
//        {
//            Debug.LogWarning("Nu există întrebări pentru categoria selectată: " + GameSettings.CategorieSelectata);
//            return;
//        }
//        Debug.Log("Filtrate: " + filtrate.Count);

//        intrebareCurenta = filtrate[UnityEngine.Random.Range(0, filtrate.Count)];
//        AfiseazaIntrebarea();
//    }

//    void AfiseazaIntrebarea()
//    {
//        textIntrebare.text = intrebareCurenta.text;

//        for (int i = 0; i < butoaneRaspuns.Length; i++)
//        {
//            int index = i;
//            butoaneRaspuns[i].GetComponentInChildren<TMP_Text>().text = intrebareCurenta.variante[i];
//            butoaneRaspuns[i].onClick.RemoveAllListeners();
//            butoaneRaspuns[i].onClick.AddListener(() => VerificaRaspuns(index));
//        }
//    }

//    void VerificaRaspuns(int indexAles)
//    {
//        if (indexAles == intrebareCurenta.indexCorect)
//        {
//            Debug.Log("Corect!");
//            onQuestionAnsweredCorrectly?.Invoke();
//        }
//        else
//        {
//            intrebareCurenta = filtrate[UnityEngine.Random.Range(0, intrebareDB.intrebari.Count)];
//            AfiseazaIntrebarea();
//            Debug.Log("Greșit!");
//        }
//    }
//}
