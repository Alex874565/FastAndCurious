using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IntrebariDB", menuName = "Database/Intrebari")]
public class IntrebareData : ScriptableObject
{
    public List<Intrebare> intrebari;
}

[Serializable]
public struct Intrebare
{
    public string text;
    public string[] variante;
    public int indexCorect;
    public string categorie;
}
