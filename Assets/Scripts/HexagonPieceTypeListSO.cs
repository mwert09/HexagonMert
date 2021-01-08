using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ------------ OLD ---------------  DEPRECATED ---------------- WE DON'T USE SCRIPTABLE OBJECTS ANYMORE */
[CreateAssetMenu(menuName = "ScriptableObjects/HexagonPieceTypeListSO")]
public class HexagonPieceTypeListSO : ScriptableObject
{
    public List<HexagonPieceSO> list;
}
