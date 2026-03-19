using UnityEngine;
using System.Collections;
using TMPro;
using Assets.Scripts.panel.BattlePanel;

public class BattleCostNum : MonoBehaviour
{
    public TextMeshProUGUI textUI;

    public int totalNum;

    // Use this for initialization
    void Start()
    {
        totalNum = 8;
        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateText()
    {
        int restNum = BattlePanel.GetBattleCardComponent().GetBattleCardCost().GetRestCost();

        textUI.text = string.Format("{0}/{1}", restNum, totalNum);
    }
}
