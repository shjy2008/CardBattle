using UnityEngine;
using System.Collections;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.managers.timermgr;
using System;
using UnityEngine.UI;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.data;
using Core.Events;

namespace Assets.Scripts.gm
{
    public class GMPanel : MonoBehaviour
    {
        private GameObject bgObj;

        // Use this for initialization
        void Start()
        {
            bgObj = gameObject.transform.Find("Bg").gameObject;
            bgObj.SetActive(false);
        }

        public void OnGMInputClk()
        {
            Action<object[]> cb = (object[] _) =>
            {
                string inputText = bgObj.transform.Find("GMInputBtn/InputField/Text").gameObject.GetComponent<Text>().text;
                Assets.Scripts.gm.GM gm = GameObject.Find("GameRoot").GetComponent<Assets.Scripts.gm.GM>();
                gm.ExecGM(inputText);
            };
            TimerManager.Instance.DelayExecByFrame(1, cb, null);
        }

        public void OnGMBtnClk()
        {
            bgObj.SetActive(!bgObj.activeSelf);
        }

        public void OnClearDataClk()
        {
            PlayerPrefs.DeleteAll();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

    }
}
