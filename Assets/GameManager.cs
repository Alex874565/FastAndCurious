using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public IntrebareData intrebareDB;
    public TMP_Text textIntrebare;
    public Button[] butoaneRaspuns;

    [SerializeField] private GameObject questionCanvas;

    private Intrebare intrebareCurenta;
    private Action onQuestionAnsweredCorrectly;
    private List<Intrebare> filtrate;

    void Start()
    {
        questionCanvas.SetActive(false);
        //StartCoroutine(LoopIntrebari());
    }

    IEnumerator LoopIntrebari()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f); // așteaptă 10 secunde

            Time.timeScale = 0f;
            questionCanvas.SetActive(true);

            StartQuestion(() =>
            {
                questionCanvas.SetActive(false);
                Time.timeScale = 1f;
            });

            while (questionCanvas.activeSelf)
            {
                yield return null; // așteaptă până când canvas-ul este dezactivat
            }
        }
    }

    public void StartQuestion(Action callback)
    {
        Debug.Log("StartQuestion called");
        onQuestionAnsweredCorrectly = callback;

        filtrate = intrebareDB.intrebari
        .Where(i => i.categorie == CategorieSyncManager.CategorieSelectata)
        .ToList();

        if (filtrate.Count == 0)
        {
            Debug.LogWarning("Nu există întrebări pentru categoria selectată: " + GameSettings.CategorieSelectata);
            return;
        }
        Debug.Log("Filtrate: " + filtrate.Count);

        intrebareCurenta = filtrate[UnityEngine.Random.Range(0, filtrate.Count)];
        AfiseazaIntrebarea();
    }

    void AfiseazaIntrebarea()
    {
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
            onQuestionAnsweredCorrectly?.Invoke();
        }
        else
        {
            intrebareCurenta = filtrate[UnityEngine.Random.Range(0, intrebareDB.intrebari.Count)];
            AfiseazaIntrebarea();
            Debug.Log("Greșit!");
        }
    }
}
