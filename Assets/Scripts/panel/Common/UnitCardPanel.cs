using UnityEngine;
using System.Collections;
using Assets.Scripts.managers.uimgr;
using TMPro;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Collections.Generic;

public class UnitCardPanel : BaseUI
{
    private string unitId;
    private bool pressingShift = false;
    public Transform propertyParent;
    public GameObject propertyItem;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        // Check if the Shift key is pressed
        if (shiftPressed && !pressingShift)
        {
            transform.Find("description/desc").gameObject.SetActive(false);
            propertyParent.gameObject.SetActive(true);
        }
        if (!shiftPressed && pressingShift)
        {
            transform.Find("description/desc").gameObject.SetActive(true);
            propertyParent.gameObject.SetActive(false);
        }

        pressingShift = shiftPressed;
    }

    public void SetUnitId(string _unitId)
    {
        unitId = _unitId;
        Table_unit.Data data = Table_unit.data[unitId];
        //data.level;

        transform.Find("bg").GetComponent<Image>().sprite = Resources.Load<Sprite>("images/unit/unit_bg/" + data.faceimage);
        transform.Find("info/num").GetComponent<TextMeshProUGUI>().text = data.level.ToString();
        transform.Find("info/group_icon").GetComponent<Image>().sprite = Resources.Load<Sprite>("images/unit/unit_type/" + data.group);
        transform.Find("description/title_bg/name").GetComponent<TextMeshProUGUI>().text = data.name;
        transform.Find("description/desc").GetComponent<TextMeshProUGUI>().text = data.description;

        List<string> propertyNames = new List<string>() { "basicEquipment", "basicTraining", "basicMoral", "basicDiscipline",
        "priotizedAttack", "meleeAttack", "projectileAttack", "armorPiercingAttack", "siegeAttack",
        "range",
        "terrainDefense", "fortificationDefense", "elasticDefense", "formationDefense", "armourDefense" };

        List<Tuple<string, float>> nameToNumList = new List<Tuple<string, float>>();

        Type type = typeof(Table_unit.Data);
        foreach (string propertyName in propertyNames)
        {
            FieldInfo field = type.GetField(propertyName);

            if (field != null)
            {
                Type dataType = field.GetValue(data).GetType();
                float num;
                if (dataType == typeof(System.Int32)) // float
                {
                    int integer = (int)field.GetValue(data);
                    num = (float)integer;
                }
                else
                {
                    num = (float)field.GetValue(data);
                }
                if (num > 0)
                {
                    nameToNumList.Add(new Tuple<string, float>(propertyName, num));
                    //nameToNumList.Add(new Tuple<string, int>(propertyName, num));
                    //nameToNumList.Add(new Tuple<string, int>(propertyName, num));
                    //Debug.Log("aaa " + propertyName + " " + num);
                }
            }
        }

        int countPerRow = 4;
        int rowCount = (nameToNumList.Count - 1) / countPerRow + 1;
        for (int i = 0; i < nameToNumList.Count; ++i)
        {
            GameObject itemObj = GameObject.Instantiate<GameObject>(propertyItem);
            itemObj.SetActive(true);
            itemObj.transform.SetParent(propertyParent);
            itemObj.transform.localScale = propertyItem.transform.localScale;
            itemObj.transform.Find("icon").GetComponent<Image>().sprite = Resources.Load<Sprite>("images/unit/unit_icon/" + nameToNumList[i].Item1);
            itemObj.transform.Find("num").GetComponent<TextMeshProUGUI>().text = nameToNumList[i].Item2.ToString();


            int indexInRow = i % countPerRow;
            int rowIndex = i / countPerRow;
            float rowGap = 26;
            float firstRowY = -33 + (rowCount * 0.5f) * rowGap;
            float x = -115 + indexInRow * 70;
            float y = firstRowY - rowIndex * rowGap;
            itemObj.transform.localPosition = new Vector3(x, y);
        }

        propertyParent.gameObject.SetActive(false);
    }
}
