using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Assets.Scripts.managers.uimgr;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.common;
using Assets.Scripts.managers.inputmgr;

namespace Assets.Scripts.common
{
    public class HoverOutCloseUI : MonoBehaviour, IPointerExitHandler
    {
        public float moveOutThreshold = 0.0f;

        private static Dictionary<GameObject, List<GameObject>> parentToChildrenDict = new Dictionary<GameObject, List<GameObject>>();
        private Camera mainCamera;
        private bool prevContainsMouse = false;

        // Use this for initialization
        void Start()
        {
            mainCamera = GameObject.Find("UICamera").GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            Vector2 pivot = rectTransform.pivot;
            Vector2 pos = (Vector2)mainCamera.WorldToScreenPoint(transform.position) -
                new Vector2(rectTransform.rect.width * pivot.x, rectTransform.rect.height * pivot.y);
            Rect rect = new Rect(pos, rectTransform.rect.size);
            if (moveOutThreshold > 0)
            {
                rect.x -= moveOutThreshold;
                rect.y -= moveOutThreshold;
                rect.width += moveOutThreshold * 2;
                rect.height += moveOutThreshold * 2;
            }

            Vector2 touchPos = InputHelper.GetTouchPos();
            bool contains = rect.Contains(touchPos);
            if (prevContainsMouse && !contains) // Mouse move out of rect
            {

                RemoveNullParentAndChildren();
                List<GameObject> children;
                if (parentToChildrenDict.TryGetValue(gameObject, out children))
                {
                    if (children.Count > 0)
                    {
                        return;
                    }
                }

                CloseParentWhenAllChildrenClosed(gameObject, touchPos);

                UIManager.Instance.CloseAndDestroyUI(gameObject);
            }
            prevContainsMouse = contains;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //RemoveNullParentAndChildren();
            //List<GameObject> children;
            //if (parentToChildrenDict.TryGetValue(gameObject, out children))
            //{
            //    if (children.Count > 0)
            //    {
            //        return;
            //    }
            //}

            //CloseParentWhenAllChildrenClosed(gameObject, eventData.position);

            //UIManager.Instance.CloseAndDestroyUI(gameObject);
        }


        public static void AddChildToParent(GameObject child, GameObject parent)
        {
            if (child == null || parent == null)
                return;

            List<GameObject> children;
            if (parentToChildrenDict.TryGetValue(parent, out children))
            {
                children.Add(child);
                parentToChildrenDict[parent] = children;
            }
            else
            {
                parentToChildrenDict[parent] = new List<GameObject>() { child };
            }
            RemoveNullParentAndChildren();
        }

        public static void RemoveChildFromParent(GameObject child, GameObject parent)
        {
            if (child == null || parent == null)
                return;

            CloseParentWhenAllChildrenClosed(child, Input.mousePosition);

            List<GameObject> children;
            if (parentToChildrenDict.TryGetValue(parent, out children))
            {
                if (children.Contains(child))
                {
                    children.Remove(child);
                }

                if (children.Count > 0)
                {
                    parentToChildrenDict[parent] = children;
                }
                else
                {
                    parentToChildrenDict.Remove(parent);
                }
            }
            RemoveNullParentAndChildren();
        }

        private static void RemoveNullParentAndChildren()
        {
            Dictionary<GameObject, List<GameObject>> newDict = new Dictionary<GameObject, List<GameObject>>();
            foreach (KeyValuePair<GameObject, List<GameObject>> keyValuePair in parentToChildrenDict)
            {
                if (keyValuePair.Key != null)
                {
                    List<GameObject> newValueList = new List<GameObject>();
                    foreach (GameObject value in keyValuePair.Value)
                    {
                        if (value != null)
                        {
                            newValueList.Add(value);
                        }
                    }
                    if (newValueList.Count > 0)
                    {
                        newDict[keyValuePair.Key] = newValueList;
                    }
                }
            }
            parentToChildrenDict = newDict;
        }

        private static void CloseParentWhenAllChildrenClosed(GameObject gameObject, Vector2 position)
        {
            // Remove parent if this is the last child and cursor is out of parent's rect
            foreach (KeyValuePair<GameObject, List<GameObject>> keyValuePair in parentToChildrenDict)
            {
                if (keyValuePair.Value.Count == 1 && keyValuePair.Value.Contains(gameObject))
                {
                    if (!UIUtils.PositionInTransform(position, keyValuePair.Key.transform))
                    {
                        UIManager.Instance.CloseAndDestroyUI(keyValuePair.Key);
                    }
                }
            }
        }
    }
}
