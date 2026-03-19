using System;
using Assets.Scripts.logics;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.BattlePanel;

public class BattleCardCost
{
    private const int totalCost = 8;
    private int restCost;
    private int lastRoundRemainingCost;

    public BattleCardCost()
    {
        restCost = totalCost;
    }

    public int GetCardCost(BattleCardData cardData)
    {
        if (cardData.actionData.cost == -1)
        {
            return restCost;
        }
        else
        {
            int cost = cardData.actionData.cost - cardData.minusCost;
            if (cost < 0)
                cost = 0;
            return cost;
        }
    }

    public bool CheckCanUseCard(BattleCardData cardData)
    {
        return (GetCardCost(cardData) <= restCost);
    }

    public void UseCardConsumeCost(BattleCardData cardData)
    {
        int cost = GetCardCost(cardData);
        restCost -= cost;
        UpdateUIText();
    }

    public void ResetCost()
    {
        restCost = totalCost;
        UpdateUIText();
    }

    public int GetRestCost()
    {
        return restCost;
    }

    public void UpdateCost(int costDiff)
    {
        restCost += costDiff;
        UpdateUIText();
    }

    public int GetLastRoundRemainingCost() { return lastRoundRemainingCost; }
    public void SetLastRoundRemainingCost() { lastRoundRemainingCost = restCost; }

    private void UpdateUIText()
    {
        BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
        BattleCostNum battleCostNum = battlePanel.transform.Find("bottom_right/card_num").GetComponent<BattleCostNum>();
        battleCostNum.UpdateText();
    }
}
