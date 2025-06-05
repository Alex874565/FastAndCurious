using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CategorieSyncManager : MonoBehaviourPun
{
    public static string CategorieSelectata = "";

    public void AlegeCalcule()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        CategorieSelectata = "calcule";
        photonView.RPC("SetCategoriePentruTotLobbyul", RpcTarget.AllBuffered, CategorieSelectata);
    }

    public void AlegeFormule()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        CategorieSelectata = "formule";
        photonView.RPC("SetCategoriePentruTotLobbyul", RpcTarget.AllBuffered, CategorieSelectata);
    }

    [PunRPC]
    public void SetCategoriePentruTotLobbyul(string categorie)
    {
        CategorieSelectata = categorie;
        Debug.Log("Categorie sincronizat?: " + categorie);
    }
}
