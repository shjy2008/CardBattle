using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.common;
using ChristinaCreatesGames.Typography.TooltipForTMP;
using TMPro;
using UnityEngine;

public class DescPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text titleTMP;

    [SerializeField]
    private TMP_Text descTMP;

    public void Clear()
    {
        var tooltipHandlerHoverComp = titleTMP.GetComponent<TooltipHandlerHover>();
        if (tooltipHandlerHoverComp != null) tooltipHandlerHoverComp.tooltipContentList = new List<TooltipInfos>();

        tooltipHandlerHoverComp = descTMP.GetComponent<TooltipHandlerHover>();
        if (tooltipHandlerHoverComp != null) tooltipHandlerHoverComp.tooltipContentList = new List<TooltipInfos>();
    }

    public void SetTitle(string title)
    {
        titleTMP.text = title;
        UIUtils.SetFontType(titleTMP, UIUtils.FontType.Title);
    }

    public void SetDescription(string desc)
    {
        descTMP.text = desc;
        UIUtils.SetFontType(descTMP, UIUtils.FontType.Desc);
    }
}
