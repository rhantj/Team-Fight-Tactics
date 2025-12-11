using System;
using UnityEngine;

public static class SelectionSubject
{
    // ChessStatData를 구독자(ChessInfoUI 등)에게 전달하는 이벤트
    public static Action<ChessStatData> OnUnitSelected;
}
