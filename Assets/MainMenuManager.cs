using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static string CategorieSelectata;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //public void AlegeCalcule()
    //{
    //    GameSettings.CategorieSelectata = "calcule";
    //    SceneManager.LoadScene("MainMenu");
    //}

    //public void AlegeFormule()
    //{
    //    GameSettings.CategorieSelectata = "formule";
    //    SceneManager.LoadScene("MainMenu");
    //}


    public void AlegeCalcule()
    {
        GameSettings.CategorieSelectata = "calcule";
        Debug.Log("Categorie aleas?: " + GameSettings.CategorieSelectata);
        SceneManager.LoadScene("Game"); // sau GameScene
    }

    public void AlegeFormule()
    {
        GameSettings.CategorieSelectata = "formule";
        Debug.Log("Categorie aleas?: " + GameSettings.CategorieSelectata);
        SceneManager.LoadScene("Game");
    }
}







