using UnityEngine;
using System.Collections;
using Assets.Scripts.common;
using Assets.Scripts.managers.uimgr;
using UnityEngine.UI;
using Assets.Scripts.panel.ArmyManage;
using TMPro;

public class BattleMapDotPopUI : MonoBehaviour
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

    public void SetTitle(string title)
    {
        transform.Find("top/text").GetComponent<TextMeshProUGUI>().text = title;
    }

    public void SetLandscapeId(string _landscapeId)
    {
        landscapeId = _landscapeId;
        string path = AssetsMapper.GetLandscapeIconPath(_landscapeId);
        transform.Find("items/2/image").GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
    }

    public void SetIsDeadlyEnemy(bool isDeadlyEnemy)
    {
        // if IsDeadlyEnemy, title bg set to red
        if (isDeadlyEnemy)
        {
            transform.Find("top").GetComponent<Image>().color = Color.red;
        }
    }

    public void SetTargetDotObj(GameObject obj)
    {
        transform.position = obj.transform.position;
        Transform lineTransform = transform.Find("line");
        float xMid = 0;
        float yMid = 0;
        if (obj.transform.position.x < xMid && obj.transform.position.y >= yMid) // top-left
        {
            lineTransform.localRotation = Quaternion.Euler(0, 0, -35);
        }
        else if (obj.transform.position.x < xMid && obj.transform.position.y < yMid) // bottom-left
        {
            lineTransform.localRotation = Quaternion.Euler(0, 0, 35);
        }
        else if (obj.transform.position.x >= xMid && obj.transform.position.y < yMid) // bottom-right
        {
            lineTransform.localRotation = Quaternion.Euler(0, 0, 145);
        }
        else
        {
            lineTransform.localRotation = Quaternion.Euler(0, 0, -145);
        }

        Vector3 dotPos = transform.Find("line/dot").transform.position;
        Vector2 diff = dotPos - transform.position;
        transform.position = new Vector3(dotPos.x - diff.x * 2, dotPos.y - diff.y * 2, dotPos.z);
    }

    public void SetBattleMapDotItem(BattleMapDotItem item)
    {
        Image img = transform.Find("top/img").GetComponent<Image>();
        int imgNum;
        float scale = 1.0f;
        //transform.Find("top/img").GetComponent<Image>().sprite = Resources.Load<Sprite>(item.ImgPath);
        if (item.DotType == BattleMapDotItem.BattleMapDotType.Start)
        {
            imgNum = 1;
            scale = 1.0f;
        }
        else if (item.DotType == BattleMapDotItem.BattleMapDotType.End)
        {
            imgNum = 1;
            scale = 1.5f;
        }
        else if (item.DotType == BattleMapDotItem.BattleMapDotType.SmallBoss)
        {
            imgNum = 5;
        }
        else
        {
            imgNum = 1;
            scale = 0.5f;
        }
        img.gameObject.transform.localScale = new Vector3(scale, scale);
        img.sprite = Resources.Load<Sprite>("graphics/UI/ui_map/point_" + imgNum);
    }

    public void OnHoverItem(HoverToShowUI hoverToShowUI)
    {
        UIManager.Instance.CloseAndDestroyUI("UnitCardPanel");
        GameObject uiObj = UIManager.Instance.OpenUI("UnitCardPanel");
        uiObj.GetComponent<UnitCardPanel>().SetUnitId("unit_0001"); // TODO

        Vector2 topRightPos = hoverToShowUI.GetTopRightPos();
        UIUtils.SetPopPanelPosTopRight(topRightPos, uiObj);

        //HoverOutCloseUI.AddChildToParent(uiObj, gameObject);
        hoverToShowUI.AddChildrenUI(uiObj.GetComponent<UnitCardPanel>());

    }

    public void OnHoverOutItem(HoverToShowUI hoverToShowUI)
    {
        GameObject uiObj = UIManager.Instance.GetUI("UnitCardPanel");
        //HoverOutCloseUI.RemoveChildFromParent(uiObj, gameObject);
        if (hoverToShowUI && uiObj)
            hoverToShowUI.RemoveChildrenUI(uiObj.GetComponent<UnitCardPanel>());

        UIManager.Instance.CloseAndDestroyUI("UnitCardPanel");
    }

    public void OnHoverLandscapeItem(HoverToShowUI hoverToShowUI)
    {
        UIManager.Instance.CloseAndDestroyUI("LandscapeCard");
        GameObject uiObj = UIManager.Instance.OpenUI("LandscapeCard");
        uiObj.GetComponent<LandscapeCard>().SetLandscapeId(landscapeId);

        Vector2 topRightPos = hoverToShowUI.GetTopRightPos();

        UIUtils.SetPopPanelPosTopRight(topRightPos, uiObj);

        //HoverOutCloseUI.AddChildToParent(uiObj, gameObject);
        hoverToShowUI.AddChildrenUI(uiObj.GetComponent<LandscapeCard>());
    }

    public void OnHoverOutLandscapeItem(HoverToShowUI hoverToShowUI)
    {
        GameObject uiObj = UIManager.Instance.GetUI("LandscapeCard");
        //HoverOutCloseUI.RemoveChildFromParent(uiObj, gameObject);
        if (hoverToShowUI && uiObj)
            hoverToShowUI.RemoveChildrenUI(uiObj.GetComponent<LandscapeCard>());

        UIManager.Instance.CloseAndDestroyUI("LandscapeCard");
    }

}
