using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.data;
using Assets.Scripts.managers.archivemgr;

public class BattleLevelBar : MonoBehaviour
{
    const string normalPath = "images/level_bar/level_bar_normal";
    const string highlightPath = "images/level_bar/level_bar_highlight";

    // Use this for initialization
    void Start()
    {
        refreshLevel();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void refreshLevel()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.Find(string.Format("{0}", i + 1));
            string path = (ArchiveManager.Instance.GetCurrentArchiveData().playerData.Level == (i + 1)) ? highlightPath : normalPath;
            child.GetComponent<Image>().sprite = ResourceManager.Instance.LoadResource(path, typeof(Sprite)) as Sprite;
        }
    }
}
