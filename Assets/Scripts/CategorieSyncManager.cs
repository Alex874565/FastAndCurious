//using UnityEngine;
//using Photon.Pun;
//using Photon.Realtime;

//public class CategorieSyncManager : MonoBehaviourPun
//{
//    public static string CategorieSelectata = "";

//    public void AlegeCalcule()
//    {
//        if (!PhotonNetwork.IsMasterClient) return;
//        CategorieSelectata = "calcule";
//        photonView.RPC("SetCategoriePentruTotLobbyul", RpcTarget.AllBuffered, CategorieSelectata);
//    }

//    public void AlegeFormule()
//    {
//        if (!PhotonNetwork.IsMasterClient) return;
//        CategorieSelectata = "formule";
//        photonView.RPC("SetCategoriePentruTotLobbyul", RpcTarget.AllBuffered, CategorieSelectata);
//    }

//    [PunRPC]
//    public void SetCategoriePentruTotLobbyul(string categorie)
//    {
//        CategorieSelectata = categorie;
//        Debug.Log("Categorie sincronizat?: " + categorie);
//    }
//}
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class CategorieSyncManager : MonoBehaviourPun
{
    public const string CATEGORIE_KEY = "categorie";
    public static string CategorieSelectata = "";


       public void AlegeCalcule()
    {
        if (PhotonNetwork.IsMasterClient)
            CategorieSyncManager.SetCategorie("calcule");
    }



public void AlegeFormule()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CategorieSyncManager.SetCategorie("formule");
        }
    }

    public static void SetCategorie(string categorie)
    {
        CategorieSelectata = categorie;
        ExitGames.Client.Photon.Hashtable prop = new ExitGames.Client.Photon.Hashtable();
        prop[CATEGORIE_KEY] = categorie;
        PhotonNetwork.CurrentRoom.SetCustomProperties(prop);
    }
}
