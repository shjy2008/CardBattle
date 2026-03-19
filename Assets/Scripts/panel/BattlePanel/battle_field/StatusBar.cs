using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.logics;
using Core.Events;
using System;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.common;
using Assets.Scripts.managers.uimgr;
using System.Data;

public class StatusBar : MonoBehaviour
{
    [SerializeField]
    private GroupSide groupSide;

    public float iconSpacing = 0f;
    private const float iconSize = 50;

    void Start()
    {

    }

    private void OnDestroy()
    {
        if (hasRegisterEvent)
            EventManager.Instance.UnRegisterEventHandler<Action<GroupSide>>(Core.Events.EventType.Battle_OnStatusUpdate, HandleOnStatusUpdate);
    }

    // This function should be called when initializing the BattleUI.
    private void OnUpdateStatus(List<Tuple<string, string, string>> iconParamList)
    {
        transform.RemoveAllChildren();
        var iconPrefab = ResourceManager.Instance.LoadResource(
            "ui_prefab/panel/battlepanel/battlefield/status_icon", typeof(GameObject), true);
        int num = iconParamList.Count;
        int cntAtRow = groupSide.IsMiddle() ? 4 : 3;
        int lastRowNum = num % cntAtRow;
        int colNum = lastRowNum == 0 ? num / cntAtRow : num / cntAtRow + 1;
        float startY = groupSide.IsTop() ? -40 : 20;
        float offset = iconSize + iconSpacing;
        float signedY = groupSide.IsTop() ? 1 : -1;
        for (int i = 0; i < num; i++)
        {
            int rowIdx = i % cntAtRow;
            int colIdx = i / cntAtRow;
            int maxRowNum = ((lastRowNum == 0) || (colIdx < colNum - 1)) ? cntAtRow : lastRowNum;
            float centerIdx = (maxRowNum - 1) * 0.5f;
            float x = (rowIdx - centerIdx) * offset;
            float y = startY + colIdx * offset * signedY;
            GameObject status_icon = Instantiate(iconPrefab) as GameObject;
            status_icon.transform.SetParent(transform);
            status_icon.transform.ResetTransform();
            status_icon.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
            // set image, label
            Transform icon = status_icon.transform.Find("icon");

            //Debug.LogFormat("Item1: {0}, Item2: {1}", iconParamList[i].Item1, iconParamList[i].Item2);

            var sprite = ResourceManager.Instance.LoadResource(iconParamList[i].Item1, typeof(Sprite), true) as Sprite;

#if UNITY_EDITOR
            if(sprite == null)
                Debug.LogErrorFormat("status id:{0} can not load the status icon {1}", iconParamList[i].Item3, iconParamList[i].Item1);
#endif

            icon.GetComponent<SpriteRenderer>().sprite = sprite;
            Transform label = status_icon.transform.Find("label");
            label.GetComponent<TextMeshPro>().text = iconParamList[i].Item2;

            status_icon.name = iconParamList[i].Item3;
            var hoverTrans = status_icon.transform.Find("hover");
            var comp = hoverTrans.GetComponent<HoverToShowUI>();
            comp.Clear();
            comp.actionHover.AddListener(OnHoverStatusIcon);
            comp.actionHoverOut.AddListener(OnHoverOutStatusIcon);
        }
    }

    private void HandleOnStatusUpdate(GroupSide side)
    {
        if (side != groupSide) return;

        var statusDataSet = BattleManager.Instance.GetCurrentBattle().GetBattleStatusDataSetByGroupSide(groupSide);
        if (statusDataSet != null) 
        {
            var iconParamList = new List<Tuple<string, string, string>>();
            foreach (var statusData in statusDataSet)
                iconParamList.Add(new(statusData.GetIconPath(), statusData.GetValueString(), statusData.tabData.id));
            OnUpdateStatus(iconParamList);
        }
    }

    private void OnHoverStatusIcon(HoverToShowUI hoverToShowUI)
    {
        string statusID = hoverToShowUI.transform.parent.gameObject.name;
        Debug.LogFormat("OnHoverStatusIcon, statusID: {0}", statusID);
        var battleStatusData = BattleManager.Instance.GetCurrentBattle().GetBattleStatusData(groupSide, statusID);

        GameObject uiObj = UIManager.Instance.OpenUI("DescPanel");
        var descPanel = uiObj.GetComponent<DescPanel>();
        descPanel.Clear();
        descPanel.SetTitle(battleStatusData.tabData.name.ToUpper());

        string originDesc = battleStatusData.tabData.description;
        string finalDesc = originDesc;

        var statusActualValueList = battleStatusData.GetAllStatusActualValues();

        if(!string.IsNullOrEmpty(originDesc))
        {
            foreach(var statusActualValue in statusActualValueList)
            {
                //int pos1 = originDesc.IndexOf('{');
                //int pos2 = originDesc.IndexOf('}');

                //string expression = originDesc.Substring(pos1 + 1, pos2 - pos1 - 1); // without "{}"
                //Debug.LogFormat("expression:{0}", expression);

                //string evalStr = expression.Replace("value", value.ToString());
                //Debug.LogFormat("evalStr:{0}", evalStr);

                //DataTable table = new DataTable();
                //float evalRes = Convert.ToSingle(table.Compute(evalStr, ""));
                //Debug.LogFormat("evalRes:{0}", evalRes);

                string statusActualValueStr = battleStatusData.tabData.isPercentage ?
                    ((int)(statusActualValue * 100)).ToString() : statusActualValue.ToString("0.0").TrimEnd('0').TrimEnd('.');

                if (battleStatusData.tabData.isPercentage) statusActualValueStr = (statusActualValue > 0 ? "+" : "") + statusActualValueStr;

                finalDesc = finalDesc.Replace("{}", statusActualValueStr);
            }
        }
        descPanel.SetDescription(finalDesc);

        Vector2 topRightPos = hoverToShowUI.GetTopRightPosBasedOnCursor() / Const.GetResolutionRatio();
        uiObj.transform.localPosition = new Vector3(topRightPos.x, topRightPos.y, uiObj.transform.localPosition.z);
    }

    private void OnHoverOutStatusIcon(HoverToShowUI hoverToShowUI)
    {
        Debug.LogFormat("OnHoverOutStatusIcon, iconName: {0}", hoverToShowUI.gameObject.name);
        UIManager.Instance.CloseUI("DescPanel");
    }

    private bool hasRegisterEvent = false;
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);

        if (visible && !hasRegisterEvent)
        {
            hasRegisterEvent = true;
            EventManager.Instance.RegisterEventHandler<Action<GroupSide>>(Core.Events.EventType.Battle_OnStatusUpdate, HandleOnStatusUpdate);
        }
        else if(!visible && hasRegisterEvent)
        {
            hasRegisterEvent = false;
            EventManager.Instance.UnRegisterEventHandler<Action<GroupSide>>(Core.Events.EventType.Battle_OnStatusUpdate, HandleOnStatusUpdate);
        }
    }
}
