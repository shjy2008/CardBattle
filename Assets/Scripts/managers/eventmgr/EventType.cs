/************************************************************
     File      : EventType.cs
     brief     : EventType for EventManager.
     author    : Atin
     version   : 1.0
     date      : 2018/06/27 11:44:00
     copyright : 2018, Atin. All rights reserved.
**************************************************************/
namespace Core.Events
{
    public enum EventType
    {
        None = 0,
        //下面增加事件定义

        OnSomeKeyDown,//for PC test

        OnSceneLoadStart,

        OnSceneLoadFinish,

        //********************** Battle Event **********************//
        Battle_OnStatusUpdate,
        Battle_OnNextRound,
        Battle_OnDefenseUpdate,
        Battle_OnFinish,
        Battle_OnInheritAllActionPoint,
        Battle_OnUpdateActionPoint,
        Battle_OnDrawCard,
        Battle_OnBurnCard,
        Battle_OnDiscardCard,
        Battle_OnReduceCardCost,
        Battle_OnActionAfterTurn,

        //********************** UI Event **********************//
        UI_OnPreviewCard,
        UI_OnEndPreviewCard,
        UI_OnBeforeUseCard,
        UI_OnAfterUseCard,
        UI_OnUseCard,
        UI_OnUpdateHPBar,

        //上面增加事件定义
        Max,
    }
}
