using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.managers.inputmgr
{
    public class InputHelper
    {
        public static bool IsClickedUI()
        {
            if (Input.mousePresent) // PC
            {
                return EventSystem.current.IsPointerOverGameObject() || GUIUtility.hotControl != 0;
            }
            else // mobile
            {
                if (Input.touchCount > 0)
                {
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) || GUIUtility.hotControl != 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static bool IsBegan()
        {
            if (Input.mousePresent)
            {
                return Input.GetMouseButtonDown(0);
            }
            else
            {
                return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
            }
        }

        private static Vector3 m_prevMousePos;
        private static float m_curFrameTime;
        public static bool IsMoved()
        {
            if (Input.mousePresent)
            {
                if (Input.GetMouseButton(0) && (!Input.GetMouseButtonDown(0)))
                {
                    if (m_prevMousePos != Input.mousePosition)
                    {
                        return true;
                    }

                    float curFrameTime = Time.time;
                    if (m_curFrameTime != curFrameTime)
                    {
                        m_curFrameTime = curFrameTime;
                        m_prevMousePos = Input.mousePosition;
                    }

                }
                return false;
            }
            else
            {
                return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved;
            }
        }

        public static bool IsEnded()
        {
            if (Input.mousePresent)
            {
                return Input.GetMouseButtonUp(0);
            }
            else
            {
                return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
            }
        }

        public static Vector2 GetTouchPos()
        {
            if (Input.mousePresent)
            {
                return Input.mousePosition;
            }
            else
            {
                if (Input.touchCount > 0)
                {
                    return Input.GetTouch(0).position;
                }
                else
                {
                    return new Vector2();
                }
            }
        }

    }

}