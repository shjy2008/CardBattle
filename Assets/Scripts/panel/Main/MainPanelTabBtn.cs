using UnityEngine;
using System.Collections;
using TMPro;

public class MainPanelTabBtn : MonoBehaviour
{
    public GameObject selectedObj;
    public TextMeshProUGUI text;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetSelected(bool selected)
    {
        selectedObj.SetActive(selected);
        if (selected)
            text.color = new Color(0, 0, 0);
        else
            text.color = new Color(1, 1, 1);
    }
}
