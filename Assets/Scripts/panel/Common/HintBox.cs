using UnityEngine;
using System.Collections;
using Assets.Scripts.managers.uimgr;
using System;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.panel.common
{
    public class HintBox : BaseUI
    {
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descText;

        public TextMeshProUGUI leftBtnText;
        public TextMeshProUGUI rightBtnText;
        public TextMeshProUGUI middleBtnText;

        public GameObject leftBtn;
        public GameObject rightBtn;
        public GameObject middleBtn;

        private Action rightCb;
        private Action leftCb;
        private Action middleCb;

        public static void ShowHintBox(string _title, string _desc, int numberOfBtns, Action _rightCb = null,
            string _leftText = null, string _rightText = null, string _middleText = null, Action _leftCb = null, Action _middleCb = null)
        {
            GameObject panelObj = UIManager.Instance.OpenUI("HintBox");
            HintBox panel = panelObj.GetComponent<HintBox>();
            panel.SetTitleText(_title);
            panel.SetDescText(_desc);
            if (numberOfBtns == 1)
            {
                panel.leftBtn.SetActive(false);
                panel.rightBtn.SetActive(false);
                panel.middleBtn.SetActive(true);
            }
            else
            {
                panel.leftBtn.SetActive(true);
                panel.rightBtn.SetActive(true);
                panel.middleBtn.SetActive(false);
            }
            panel.SetRightCb(_rightCb);
            panel.SetLeftCb(_leftCb);
            panel.SetMiddleCb(_middleCb);

            if (_leftText != null)
                panel.SetLeftText(_leftText);
            if (_rightText != null)
                panel.SetRightText(_rightText);
            if (_middleText != null)
                panel.SetMiddleText(_middleText);
        }

        // Use this for initialization
        protected override void Start()
        {

        }

        public void SetRightCb(Action _rightCb)
        {
            rightCb = _rightCb;
        }

        public void SetLeftCb(Action _leftCb)
        {
            leftCb = _leftCb;
        }

        public void SetMiddleCb(Action _middleCb)
        {
            middleCb = _middleCb;
        }

        public void SetTitleText(string text)
        {
            titleText.text = text;
        }

        public void SetDescText(string text)
        {
            descText.text = text;
        }

        public void SetRightText(string text)
        {
            rightBtnText.text = text;
        }

        public void SetLeftText(string text)
        {
            leftBtnText.text = text;
        }

        public void SetMiddleText(string text)
        {
            middleBtnText.text = text;
        }

        public void OnRightBtnClk()
        {
            rightCb?.Invoke();

            CloseAndDestroy();
        }

        public void OnLeftBtnClk()
        {
            leftCb?.Invoke();

            CloseAndDestroy();
        }

        public void OnMiddleBtnClk()
        {
            middleCb?.Invoke();

            CloseAndDestroy();
        }
    }
}
