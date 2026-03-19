using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts.panel.Card;
using Assets.Scripts.managers.uimgr;

public class CardPanelSequenceFilter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string sequenceName;

    private string suffixNormal = "normal";
    private string suffixHover = "hover";
    private string suffixSelect = "select";

    private bool isSelected = false;

    // Use this for initialization
    void Start()
    {
        SetImage(suffixNormal);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool GetIsSelected()
    {
        return isSelected;
    }

    public void OnClick()
    {
        isSelected = !isSelected;
        if (isSelected)
            SetImage(suffixSelect);
        else
            SetImage(suffixNormal);

        UIManager.Instance.GetOpenUI("CardPanel").GetComponent<CardPanel>().UpdateScrollViewData();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
            SetImage(suffixHover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected)
            SetImage(suffixSelect);
        else
            SetImage(suffixNormal);
    }


    // suffix: normal, hover, select
    public void SetImage(string suffix)
    {
        string path = string.Format("images/tactic/tactic_sequence_ui/{0}_{1}", sequenceName, suffix);
        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
    }

}
