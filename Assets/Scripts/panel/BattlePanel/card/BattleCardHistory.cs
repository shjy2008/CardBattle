using System;
using System.Collections.Generic;
using Assets.Scripts.logics;

public class BattleCardHistory
{
    private List<BattleCardData> curRoundUsedCardList = new List<BattleCardData>();
    private List<BattleCardData> curRoundDrawnCardList = new List<BattleCardData>();

    public BattleCardHistory()
    {
    }

    public void AddUsedCard(BattleCardData cardData)
    {
        curRoundUsedCardList.Add(cardData);
    }

    public void AddDrawnCard(BattleCardData cardData)
    {
        curRoundDrawnCardList.Add(cardData);
    }

    public void StartNewRound()
    {
        curRoundUsedCardList.Clear();
        curRoundDrawnCardList.Clear();
    }

    public List<BattleCardData> GetCurRoundUsedCardList() { return curRoundUsedCardList; }
    public List<BattleCardData> GetCurRoundDrawnCardList() { return curRoundDrawnCardList; }
}
