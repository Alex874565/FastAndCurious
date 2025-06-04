using UnityEngine;

public class CategorieSelector : MonoBehaviour
{
    public void AlegeCalcule()
    {
        GameSettings.CategorieSelectata = "calcule";
        Debug.Log("Categorie aleas?: " + GameSettings.CategorieSelectata);
    }

    public void AlegeFormule()
    {
        GameSettings.CategorieSelectata = "formule";
        Debug.Log("Categorie aleas?: " + GameSettings.CategorieSelectata);
    }
}
