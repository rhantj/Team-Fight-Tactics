using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 코스트(1~3)에 따른 슬롯 UI 스타일 정보를 담는 클래스
/// </summary>
[System.Serializable]
public class CostUIInfo
{
    [Tooltip("기물 코스트 (1, 2, 3 등)")]
    public int cost;

    [Tooltip("해당 코스트일 때 슬롯 프레임 이미지")]
    public Sprite frameSprite;

    [Tooltip("해당 코스트일 때 슬롯 배경 색상")]
    public Color backgroundColor;

    [Tooltip("기물 정보 UI 프레임 스프라이트")]
    public Sprite infoFrameSprite;
}

/// <summary>
/// 코스트별 UI 스타일을 보관하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "CostUIData", menuName = "TFT/Cost UI Data")]
public class CostUIData : ScriptableObject
{
    [Tooltip("코스트별 UI 정보 리스트")]
    public List<CostUIInfo> costInfos;

    /// <summary>
    /// 입력된 코스트에 맞는 UI 정보를 반환
    /// 찾지 못할 경우 null 반환 (외부에서 null 체크 필요)
    /// </summary>
    public CostUIInfo GetInfo(int cost)
    {
        return costInfos.Find(info => info.cost == cost);
    }
}
