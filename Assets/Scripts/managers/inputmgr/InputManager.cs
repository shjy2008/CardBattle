using Assets.Scripts.utility.CommonInterface;
using Assets.Scripts.utility.Singleton;
using Core;
using Core.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.managers.inputmgr
{
    public class InputManager : TSingleton<InputManager>, IManager
    {
        //private List<KeyCode> handleKeyCodeList = new List<KeyCode>();//for PC test
        //private List<KeyCode> keyDownList = new List<KeyCode>();//for PC test

        private bool isTouching;
        private bool hasSentSwipeEvent;
        private Vector2 beganPos;

        private InputManager()
        {
            isTouching = false;
            hasSentSwipeEvent = false;

            //for PC test
            //handleKeyCodeList.Add(KeyCode.W);
            //handleKeyCodeList.Add(KeyCode.S);
            //handleKeyCodeList.Add(KeyCode.A);
            //handleKeyCodeList.Add(KeyCode.D);
            //handleKeyCodeList.Add(KeyCode.J);
        }

        public void Init()
        {

        }

        public void Update()
        {
            //keyDownList.Clear();

            //foreach (var keyCode in handleKeyCodeList)
            //{
            //    if (Input.GetKeyDown(keyCode) || Input.GetKey(keyCode))
            //    {
            //        keyDownList.Add(keyCode);
            //    }
            //}

            //EventManager.Instance.SendEventSync(Core.Events.EventType.OnSomeKeyDown, keyDownList);

            
            if (!InputHelper.IsClickedUI())
            {
                if (InputHelper.IsBegan())
                {
                    isTouching = true;
                    hasSentSwipeEvent = false;
                    beganPos = InputHelper.GetTouchPos();
                }
                else if (InputHelper.IsMoved())
                {
                    if (isTouching && !hasSentSwipeEvent)
                    {
                        KeyCode code;
                        Vector2 diff = InputHelper.GetTouchPos() - beganPos;
                        float distance = diff.SqrMagnitude();
                        if (distance > 100.0f)
                        {
                            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
                            {
                                if (diff.x > 0)
                                {
                                    code = KeyCode.D;
                                }
                                else
                                {
                                    code = KeyCode.A;
                                }
                            }
                            else
                            {
                                if (diff.y > 0)
                                {
                                    code = KeyCode.W;
                                }
                                else
                                {
                                    code = KeyCode.S;
                                }
                            }
                            EventManager.Instance.SendEventSync(Core.Events.EventType.OnSomeKeyDown, code);
                            hasSentSwipeEvent = true;
                        }
                    }
                }
                else if (InputHelper.IsEnded())
                {
                    isTouching = false;
                    hasSentSwipeEvent = false;
                }
            }
        }

        public void Destroy()
        {

        }
    }
}
