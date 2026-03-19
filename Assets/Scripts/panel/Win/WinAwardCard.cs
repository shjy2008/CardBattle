using UnityEngine;
using System.Collections;
using Assets.Scripts.managers.inputmgr;
using UnityEngine.EventSystems;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.common;

namespace Assets.Scripts.panel.Win
{
    public class WinAwardCard : MonoBehaviour
    {
        private int index;
        private Camera uiCamera;
        private Vector3 originalPos;
        public GameObject highlight;

        // Use this for initialization
        void Start()
        {
            index = int.Parse(gameObject.name);
            uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
            originalPos = transform.localPosition;
        }

        // Update is called once per frame
        void Update()
        {
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            Vector2 pivot = rectTransform.pivot;
            Vector2 pos = (Vector2)uiCamera.WorldToScreenPoint(transform.position) / Const.GetResolutionRatio() -
                new Vector2(rectTransform.rect.width * pivot.x, rectTransform.rect.height * pivot.y);
            Rect rect = new Rect(pos, rectTransform.rect.size);

            Vector2 touchPos = InputHelper.GetTouchPos() / Const.GetResolutionRatio();
            bool contains = rect.Contains(touchPos);

            GameObject panelObj = UIManager.Instance.GetOpenUI("WinAwardPanel");

            if (contains || (panelObj != null && panelObj.GetComponent<WinAwardPanel>().GetSelectedIndex() == index))
            {
                transform.localPosition = new Vector3(originalPos.x, originalPos.y, -100);
                highlight.SetActive(true);
            }
            else
            {
                transform.localPosition = originalPos;
                highlight.SetActive(false);
            }
        }

        public void OnButtonClk()
        {
            WinAwardPanel panel = UIManager.Instance.GetOpenUI("WinAwardPanel").GetComponent<WinAwardPanel>();
            panel.ClickAwardCard(index);
        }
    }
}
