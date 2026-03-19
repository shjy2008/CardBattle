using UnityEngine;
using System.Collections;
using Assets.Scripts.managers.uimgr;

public class MainPanel : BaseUI
{
    public GameObject quickStartBtn;
    public GameObject singlePlayerBtn;

    public GameObject quickStartPanel;
    public GameObject singlePlayerPanel;

    // Use this for initialization
    void Start()
    {
        OnQuickStartBtnClk();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnQuickStartBtnClk()
    {
        quickStartBtn.GetComponent<MainPanelTabBtn>().SetSelected(true);
        singlePlayerBtn.GetComponent<MainPanelTabBtn>().SetSelected(false);

        quickStartPanel.SetActive(true);
        singlePlayerPanel.SetActive(false);
    }

    public void OnSinglePlayerBtnClk()
    {
        quickStartBtn.GetComponent<MainPanelTabBtn>().SetSelected(false);
        singlePlayerBtn.GetComponent<MainPanelTabBtn>().SetSelected(true);

        quickStartPanel.SetActive(false);
        singlePlayerPanel.SetActive(true);
    }
}
