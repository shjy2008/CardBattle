using UnityEngine;
using System.Collections;
using Assets.Scripts.managers.uimgr;
using TMPro;

public class EventPanel : BaseUI
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetEventId(string eventId)
    {
        Table_event.Data data = Table_event.data[eventId];

        transform.Find("bottom/title/text").GetComponent<TMP_Text>().text = data.eventTitle;
        //transform.Find("q")
    }
}
