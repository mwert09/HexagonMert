using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/HexagonPieceTypeListSO")]
public class HexagonPieceTypeListSO : ScriptableObject
{
    public List<HexagonPieceSO> list;
}
