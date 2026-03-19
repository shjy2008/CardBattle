using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.managers.inputmgr;

namespace Assets.Scripts.common
{
    public class HoverToShowUI : MonoBehaviour, IPointerEnterHandler
    {
        public UnityEvent<HoverToShowUI> actionHover;
        public UnityEvent<HoverToShowUI> actionHoverOut;

        private bool prevContainsMouse = false;
        private bool prevHasOpenedChildrenUI = false;

        private List<BaseUI> childrenUIList = new List<BaseUI>();

        private bool isHovering = false;
        private bool isShowingUIWhenHovering = false;
        private float hoverTimer = 0.0f;
        private const float hoverShowTime = 0.2f;

        private bool isExiting = false;
        private float exitTimer = 0.0f;
        private const float exitCloseTime = 0.1f;

        // Use this for initialization
        void Start()
        {

        }

        public Vector2 GetTopRightPos()
        {
            Camera uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
            Vector3 pos = uiCamera.WorldToScreenPoint(transform.position);
            pos /= Const.GetResolutionRatio();

            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            Vector2 topRightPos = new Vector2(
                pos.x + rectTransform.rect.width * (1 - rectTransform.pivot.x),
                pos.y + rectTransform.rect.height * (1 - rectTransform.pivot.y));
            return topRightPos;
        }

        public Vector2 GetTopRightPosBasedOnCursor()
        {
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            Vector2 topRightPos = new Vector2(
                Input.mousePosition.x / Const.GetResolutionRatio() + rectTransform.rect.width * (1 - rectTransform.pivot.x),
                Input.mousePosition.y / Const.GetResolutionRatio() + rectTransform.rect.height * (1 - rectTransform.pivot.y));
            return topRightPos;
        }

        public void AddChildrenUI(BaseUI childUI)
        {
            childrenUIList.Add(childUI);
        }

        public void RemoveChildrenUI(BaseUI childUI)
        {
            childrenUIList.Remove(childUI);
        }

        private bool HasOpenedChildrenUI()
        {
            foreach (BaseUI childUI in childrenUIList)
            {
                if (childUI != null && UIManager.Instance.GetOpenUI(childUI.name))
                {
                    return true;
                }
            }
            return false;
        }

        // Update is called once per frame
        void Update()
        {
            if (isHovering)
            {
                if (!isShowingUIWhenHovering)
                {
                    hoverTimer += Time.deltaTime;
                    if (hoverTimer > hoverShowTime)
                    {
                        actionHover?.Invoke(this);
                        isShowingUIWhenHovering = true;
                    }
                }
            }

            if (isExiting)
            {
                exitTimer += Time.deltaTime;
                if (exitTimer > exitCloseTime)
                {
                    actionHoverOut?.Invoke(this);
                    isExiting = false;

                    foreach (BaseUI childUI in childrenUIList)
                    {
                        if (childUI != null)
                            childUI.CloseAndDestroy();
                    }
                    childrenUIList.Clear();
                }
            }

            bool containsMouse = UIUtils.PositionInTransform(InputHelper.GetTouchPos() / Const.GetResolutionRatio(), transform);
            if (!containsMouse && prevContainsMouse)
            {
                OnPointerExit_selfImplement();
            }
            prevContainsMouse = containsMouse;

            bool hasOpenedChildrenUI = HasOpenedChildrenUI();
            if (!hasOpenedChildrenUI && prevHasOpenedChildrenUI)
            {
                OnPointerExit_selfImplement();
            }
            prevHasOpenedChildrenUI = hasOpenedChildrenUI;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverTimer = 0.0f;
            isHovering = true;
            isExiting = false;
        }

        // 因为如果用OnPointerExit，弹出的界面挡住鼠标的话，会不进去，所以自己实现
        public void OnPointerExit_selfImplement()
        {
            //// Sometime there're bugs, this function is called even when mouse in the rect
            //Vector2 touchPos = InputHelper.GetTouchPos();
            //bool contains = UIUtils.PositionInTransform(touchPos, transform);
            //if (contains)
            //{
            //    return;
            //}

            // If the pointer is in children's area, don't close
            Vector2 touchPos = InputHelper.GetTouchPos();
            foreach (BaseUI childUI in childrenUIList)
            {
                if (childUI != null && UIUtils.PositionInTransform(touchPos, childUI.transform))
                {
                    return;
                }
            }

            isHovering = false;
            hoverTimer = 0.0f;
            isShowingUIWhenHovering = false;

            Vector2 mousePos = InputHelper.GetTouchPos() / Const.GetResolutionRatio();

            // Don't close UI if move right
            Camera uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
            Vector3 pos = uiCamera.WorldToScreenPoint(transform.position) / Const.GetResolutionRatio();
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            if (mousePos.x >= (pos.x + rectTransform.rect.width * (1 - rectTransform.pivot.x) - 20) &&
                mousePos.y < (pos.y + rectTransform.rect.height * (1 - rectTransform.pivot.y)) &&
                mousePos.y > (pos.y - rectTransform.rect.height * rectTransform.pivot.y))
                return;
            else
            {
                exitTimer = 0.0f;
                isExiting = true;
            }
        }

        public void Clear()
        {
            actionHover.RemoveAllListeners();
            actionHoverOut.RemoveAllListeners();
            childrenUIList.Clear();
        }
    }
}