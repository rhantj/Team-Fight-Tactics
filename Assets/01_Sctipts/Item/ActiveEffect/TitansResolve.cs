using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitansResolve : ItemBase
{
    private const int MAX_STACK = 25;
    private const int ATK_PER_STACK = 1;
    private const int BONUS_ATK_AT_MAX = 10;

    private int stack;
    private bool maxBonusApplied;

    private int totalBonusAttack;

    public TitansResolve(ItemData data) : base(data)
    {
    }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);
        totalBonusAttack = 0;
        maxBonusApplied = false;

        owner.OnBasicAttackHit += HandleBasicAttackHit;
        owner.OnHPChanged += HandleHPChanged;
    }

    public override void OnUnequip()
    {
        if (owner!= null)
        {
            owner.OnBasicAttackHit -= HandleBasicAttackHit;
            owner.OnHPChanged -= HandleHPChanged;

            owner.AddBonusStats(-totalBonusAttack, 0, 0);
        }
        base.OnUnequip();
    }
    private void HandleBasicAttackHit() => AddStack();
    private void HandleHPChanged(int currentHP, int maxHP) => AddStack();

    private void AddStack()
    {
        if(stack >= MAX_STACK)
        {
            return;
        }

        stack++;

        totalBonusAttack += ATK_PER_STACK;

        owner.AddBonusStats(totalBonusAttack, 0, 0);

        if(stack >=MAX_STACK && !maxBonusApplied)
        {
            maxBonusApplied = true;

            totalBonusAttack += BONUS_ATK_AT_MAX;
            owner.AddBonusStats(totalBonusAttack, 0, 0);
            //Debug.Log($"[TitansResolve] 최대 중첩 발동 | {owner.name}");
        }
        //Debug.Log($"[TitansResolve] 스택 {stack}/{MAX_STACK} | {owner.name}");
    }
}
