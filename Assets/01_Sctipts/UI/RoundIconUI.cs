using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum RoundIconState
{
    None,       
    Current,    
    Win,        
    Lose        
}

public class RoundIconUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    [Header("Sprites")]
    public Sprite noneSprite;
    public Sprite currentSprite;
    public Sprite winSprite;
    public Sprite loseSprite;

    public void SetState(RoundIconState state)
    {
        switch (state)
        {
            case RoundIconState.None:
                icon.sprite = noneSprite;
                break;
            case RoundIconState.Current:
                icon.sprite = currentSprite;
                break;
            case RoundIconState.Win:
                icon.sprite = winSprite;
                break;
            case RoundIconState.Lose:
                icon.sprite = loseSprite;
                break;
        }
    }
}
