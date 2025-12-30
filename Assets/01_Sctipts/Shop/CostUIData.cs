using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 코스트 값을 기준으로 UI 스타일을 정의하기 위한 데이터 구조
/// 상점 슬롯 UI와 기물 정보 UI에서 공통으로 사용하는
/// 코스트별 프레임, 배경색, 정보 UI 프레임을 묶어서 관리한다.
/// 코스트 1개에 대한 UI 스타일 묶음이라 보면 편혀
/// </summary>
[System.Serializable]
public class CostUIInfo
{
    [Tooltip("기물 코스트 값 (예 : 1, 2, 3)")]
    public int cost;

    [Tooltip("상점 슬롯에서 사용하는 코스트 프레임 스프라이트")]
    public Sprite frameSprite;

    [Tooltip("상점 슬롯 배경에 적용되는 코스트별 색상")]
    public Color backgroundColor;

    [Tooltip("기물 정보 UI에서 사용하는 코스트 프레임 스프라이트")]
    public Sprite infoFrameSprite;
}

/// <summary>
/// 기물의 코스트에 따라 적용될 UI 스타일 규칙을 관리하는 SO
/// 코스트 값을 입력받아 해당 코스트에 맞는 UI 설정 데이터를 제공한다.
/// UI에 직접 작용하지 않고, 조회 전용 데이터 역할만 수행한다.
/// 여러 CostUIInfo를 관리하고 조회하는 데이터 테이블
/// </summary>
[CreateAssetMenu(fileName = "CostUIData", menuName = "TFT/Cost UI Data")]
public class CostUIData : ScriptableObject
{
    [Tooltip("코스트별 UI 스타일 데이터 목록")]
    public List<CostUIInfo> costInfos;

    /// <summary>
    /// 전달받은 코스트 값에 해당하는 UI 스타일 데이터를 반환한다.
    /// 일치하는 코스트 정보가 없을 경우 null을 반환하며,
    /// 호출하는 쪽에서 반드시 null 체크가 필요하다.
    /// </summary>
    public CostUIInfo GetInfo(int cost)
    {
        return costInfos.Find(info => info.cost == cost);
    }
}
