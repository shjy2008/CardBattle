using UnityEngine;
using System.Collections;
using Assets.Scripts.managers.uimgr;
using TMPro;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Collections.Generic;

public class LandscapeCard : BaseUI
{
    private string landscapeId;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetLandscapeId(string _landscapeId)
    {
        landscapeId = _landscapeId;
        Table_landscape.Data tabData = Table_landscape.data[landscapeId];

        transform.Find("icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(AssetsMapper.GetLandscapeIconPath(landscapeId));
        transform.Find("bg").GetComponent<Image>().sprite = Resources.Load<Sprite>(AssetsMapper.GetLandscapeBgPath(landscapeId));

        transform.Find("description/title_bg/name").GetComponent<TextMeshProUGUI>().text = tabData.name;
        transform.Find("description/desc").GetComponent<TextMeshProUGUI>().text = tabData.description;
    }
}
