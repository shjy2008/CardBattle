using Assets.Scripts.common;
using Assets.Scripts.logics;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.ArmyManage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGroupIcon : MonoBehaviour
{
    [SerializeField]
    private Transform hoverTrans;

    private int row, col;
    private HoverToShowUI hoverToShowComp;

    private void Start()
    {
        hoverToShowComp = hoverTrans.GetComponent<HoverToShowUI>();
        hoverToShowComp.Clear();
        hoverToShowComp.actionHover.AddListener(OnHoverGroupIcon);
        hoverToShowComp.actionHoverOut.AddListener(OnHoverOutGroupIcon);
    }

    public void Init(int _row, int _col)
    {
        row = _row;
        col = _col;
    }

    private void OnHoverGroupIcon(HoverToShowUI hoverToShowUI)
    {
        Debug.LogFormat("OnHoverGroupIcon  In  ---> row:{0}, col:{1}", row, col);
        var battleGroupData = BattleManager.Instance.GetCurrentBattle().GetBattleGroupData(row, col);
        var unitsDict = battleGroupData.GetUnitsDictCopyBeforeLoss();

        GameObject uiObj = UIManager.Instance.OpenUI("ArmySoldierListPop");
        uiObj.GetComponent<ArmySoldierListPop>().SetUnitsDict(unitsDict);
        uiObj.transform.localPosition = hoverToShowUI.GetTopRightPos();
    }

    private void OnHoverOutGroupIcon(HoverToShowUI hoverToShowUI)
    {
        Debug.LogFormat("OnHoverOutGroupIcon  Out ---> row:{0}, col:{1}", row, col);
        UIManager.Instance.CloseAndDestroyUI("ArmySoldierListPop");
    }
}
