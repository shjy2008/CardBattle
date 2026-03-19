using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.logics;

/// <summary>
/// 'DisplayAttackedUI()', 'DisplayDefenseUI()', and 'HideUI()' are public methods for calling.
/// When calling above methods, we should know which is defense and which is attack UI beforehand.
/// </summary>
public class SelectUI : MonoBehaviour
{
    [SerializeField]
    private bool isCenter = true;

    private Vector2 size1 = new Vector2(250, 50); // the center size for one row
    private Vector2 size2 = new Vector2(125, 50); // the left/right size for one row

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        //OnTesting();
#endif
    }

    void DisplaySelectedBG(Transform trans, int activeRows)
    {
        float posY;
        float width = isCenter ? size1.x : size2.x;
        float height;
        bool hasRow1 = BattleLogics.HasActiveRow(activeRows, ActiveRow.TOP_ONE) || BattleLogics.HasActiveRow(activeRows, ActiveRow.BOTTOM_ONE);
        bool hasRow2 = BattleLogics.HasActiveRow(activeRows, ActiveRow.TOP_TWO) || BattleLogics.HasActiveRow(activeRows, ActiveRow.BOTTOM_TWO);
        bool hasRow3 = BattleLogics.HasActiveRow(activeRows, ActiveRow.TOP_THREE) || BattleLogics.HasActiveRow(activeRows, ActiveRow.BOTTOM_THREE);
        if (hasRow1 && hasRow3)
        {
            posY = 0;
            height = size1.y * 3;
        }
        else if (hasRow1 && hasRow2 || hasRow2 && hasRow3)
        {
            posY = hasRow1 ? size1.y * 0.5f : -size1.y * 0.5f;
            height = size1.y * 2;
        }
        else
        {
            posY = hasRow1 ? size1.y : (hasRow2 ? 0 : -size1.y);
            height = size1.y;
        }
        var rectTrans = trans.GetComponent<RectTransform>();
        rectTrans.localPosition = new Vector3(0, posY, 0);

        float scaleX = isCenter ? width / size1.x * 12.5f : width / size2.x * 6.25f;
        float scaleY = height / size1.y * 2.5f;
        rectTrans.localScale = new Vector3(scaleX, scaleY, 1);

        Transform hitPointTrans = trans.Find("hitpoint");
        if(hitPointTrans != null)
        {
            hitPointTrans.localScale = new Vector3(15f / scaleX, 15f / scaleY, 1);
        }
    }

    /// <summary>
    /// change the Attack UI according to parameters
    /// </summary>
    /// <param name="activeRow"></param>
    /// <param name="attackedDirs"></param>
    //public void DisplayAttackedUI(int activeRow, int attackedDirs)
    //{
    //    for (int i = 0; i < 4; i++)
    //    {
    //        int dir = (1 << i);
    //        bool visible = (dir & attackedDirs) == dir;
    //        var trans = transform.Find(((AttackedDir)dir).ToString());
    //        trans.gameObject.SetActive(visible);
    //        if (visible)
    //            DisplaySelectedBG(trans, activeRow);
    //    }
    //}

    public void HideAllAttackedUI()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }

    public void DisplayAttackedUI(int activeRows, AttackedDir atkDir)
    {
        var trans = transform.Find(atkDir.ToString());
        trans.gameObject.SetActive(true);
        DisplaySelectedBG(trans, activeRows);
    }


    public void DisplayDefenseUI(int activeRows, bool gainArmor)
    {
        Transform displayTrans;
        if(gainArmor)
        {
            transform.Find("select").gameObject.SetActive(false);
            displayTrans = transform.Find("defense");
            displayTrans.gameObject.SetActive(true);
        }
        else
        {
            transform.Find("defense").gameObject.SetActive(false);
            displayTrans = transform.Find("select");
            displayTrans.gameObject.SetActive(true);
        }
        DisplaySelectedBG(displayTrans, activeRows);
    }

    /// <summary>
    /// hide all uis
    /// </summary>
    public void HideUI()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }

    public Transform GetHitPoint(int activeRows, AttackedDir atkDir)
    {
        var trans = transform.Find(atkDir.ToString());
        DisplaySelectedBG(trans, activeRows);
        return trans.GetChild(0);
    }

#if UNITY_EDITOR
    //public bool isDefense = true;
    //public bool trigger = false; // just for testing purpose...
    //public int testAttackedDir = 4; // just for testing purpose...
    //public int testActiveRow = 7; // binary '111', just for testing purpose. (111 means three rows are active)
    //private void OnTesting()
    //{
    //    if (!trigger)
    //        return;

    //    trigger = false;

    //    if (isDefense)
    //        DisplayDefenseUI(testActiveRow);
    //    else
    //        DisplayAttackedUI(testActiveRow, testAttackedDir);
    //}
#endif
}
