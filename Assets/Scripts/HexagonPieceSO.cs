using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ------------ OLD ---------------  DEPRECATED ---------------- WE DON'T USE SCRIPTABLE OBJECTS ANYMORE */

[CreateAssetMenu(menuName = "ScriptableObjects/HexagonPiece")]
public class HexagonPieceSO : ScriptableObject
{
    public string stringName;
    //public GameObject prefab;
    public ColorSO color;
}
