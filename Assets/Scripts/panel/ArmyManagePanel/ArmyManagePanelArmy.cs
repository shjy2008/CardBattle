using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.managers.uimgr;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Scripts.managers.inputmgr;
using Assets.Scripts.common;

namespace Assets.Scripts.panel.ArmyManage
{
    public class ArmyManagePanelArmy : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        private Vector3 originalLocalPos;
        private int armyIndex;
        private Camera uiCamera;
        private bool isMoving = false;

        // Use this for initialization
        void Start()
        {
            originalLocalPos = gameObject.transform.localPosition;
            armyIndex = int.Parse(gameObject.name);
            uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            if (isMoving)
            {
                Vector2 touchPos = InputHelper.GetTouchPos();
                transform.position = uiCamera.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, transform.position.z));
            }
        }

        public int GetArmyIndex() { return armyIndex; }

        public void OnPointerDown(PointerEventData eventData)
        {
            ArmyManagePanel panel = UIManager.Instance.GetOpenUI("ArmyManagePanel").GetComponent<ArmyManagePanel>();
            panel.CurrMovingArmy = gameObject;
            panel.OnHoverOutArmyItem(gameObject.GetComponent<HoverToShowUI>());
            isMoving = true;
            transform.SetAsLastSibling();
        }

        public void OnPointerMove(PointerEventData eventData)
        {

        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ArmyManagePanel panel = UIManager.Instance.GetOpenUI("ArmyManagePanel").GetComponent<ArmyManagePanel>();
            panel.CurrMovingArmy = null;

            transform.localPosition = originalLocalPos;
            isMoving = false;

            Vector2 pos = new Vector2(0, 0);
            if (eventData != null)
                pos = eventData.position / Const.GetResolutionRatio();
            panel.finishMovingArmy(armyIndex, pos);
        }
    }
}
