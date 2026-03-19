using System;
using System.Collections.Generic;
using FrameWork;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.utility.Singleton;
using Assets.Scripts.utility.CommonInterface;
using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.common;
using Assets.Scripts.utility;

namespace Assets.Scripts.managers.uimgr
{
    public class UIManager : TSingleton<UIManager>, IManager
    {
        Dictionary<string, GameObject> UIDict = null;
        Dictionary<string, GameObject> UIOpenDict = null;
        GameObject NormalUIRoot = null;
        GameObject HintBoxRoot = null;
        GameObject TipsUIRoot = null;

        private UIManager() { }

        public void Init()
        {
            UIDict = new Dictionary<string, GameObject>(20);
            UIOpenDict = new Dictionary<string, GameObject>(20);

            var canvas = GameObject.Find("Dialog/DialogRoot");
            canvas.GetComponent<CanvasScaler>().matchWidthOrHeight = Utils.GetMatchSceenAdjustor();

            NormalUIRoot = GameObject.Find("Dialog/DialogRoot/NormalUI");
            HintBoxRoot = GameObject.Find("Dialog/DialogRoot/HintBoxUI");
            TipsUIRoot = GameObject.Find("Dialog/DialogRoot/TipsUI");

            if (Const.debugMode)
            {
                //UIManager.Instance.OpenUI("GMPanel");
            }
        }

        public void Update()
        {
            
        }

        public void Destroy()
        {
            
        }

        private GameObject CreateUI(string name)
        {
            UIPath.UIParam uiParam;
            if (UIPath.PathData.TryGetValue(name, out uiParam))
            {
                var go = ResourceManager.Instance.LoadResource(uiParam.path, typeof(GameObject));
                if (go != null)
                {
                    var ui = GameObject.Instantiate(go) as GameObject;
                    if (!ui.activeSelf)
                    {
                        ui.SetActive(true);
                    }
                    ui.name = name;

                    switch (uiParam.layer)
                    {
                        case UIPath.UILayer.Normal:
                            ui.transform.SetParent(NormalUIRoot.transform, false);
                            break;
                        case UIPath.UILayer.HintBox:
                            ui.transform.SetParent(HintBoxRoot.transform, false);
                            break;
                        case UIPath.UILayer.Tips:
                            ui.transform.SetParent(TipsUIRoot.transform, false);
                            break;
                        default:
                            break;
                    }

                    return ui;
                }

                Debug.Log("======UIPath==找不到==========" + uiParam.path);
            }

            Debug.Log("========UIPath找不到==========" + name);
            return null;
        }

        public GameObject OpenUI(string uiName)
        {
            GameObject uiObj = null;

            if (UIDict.ContainsKey(uiName))
            {
                UIDict.TryGetValue(uiName, out uiObj);
                if (uiObj != null) uiObj.SetActive(true);
            }
            else
            {
                uiObj = CreateUI(uiName);
                if (uiObj != null)
                {
                    UIDict.Add(uiName, uiObj);
                    UIOpenDict.Add(uiName, uiObj);
                }
            }

            if (!NormalUIRoot.activeSelf)
            {
                NormalUIRoot.SetActive(true);
            }

            //foreach (var panelName in UIDict.Keys)
            //{
            //    if (panelName != uiName)
            //    {
            //        CloseUI(panelName);
            //    }
            //}

            uiObj.transform.SetAsLastSibling();
            return uiObj;
        }

        public GameObject MyOpenUI(string uiName, bool isHideOther = true)
        {
            GameObject uiObj = null;

            if (UIDict.ContainsKey(uiName))
            {
                UIDict.TryGetValue(uiName, out uiObj);
                if (uiObj != null) uiObj.SetActive(true);
            }
            else
            {
                uiObj = CreateUI(uiName);
                if (uiObj != null)
                {
                    uiObj.transform.SetParent(NormalUIRoot.transform, false);

                    UIDict.Add(uiName, uiObj);
                    UIOpenDict.Add(uiName, uiObj);
                }
            }

            if (!NormalUIRoot.activeSelf)
            {
                NormalUIRoot.SetActive(true);
            }

            if (isHideOther == true)
            {
                foreach (var panelName in UIDict.Keys)
                {
                    if (panelName != uiName)
                    {
                        CloseUI(panelName);
                    }
                }
            }

            uiObj.transform.SetAsLastSibling();
            return uiObj;
        }

        public GameObject GetOpenUI(string uiName)
        {
            GameObject uiObj;
            if (UIOpenDict.TryGetValue(uiName, out uiObj))
            {
                return uiObj;
            }
            return null;
        }

        public GameObject GetUI(string uiName)
        {
            GameObject uiObj;
            if (UIDict.TryGetValue(uiName, out uiObj))
            {
                return uiObj;
            }
            return null;
        }

        public void CloseUI(string uiName)
        {
            GameObject uiObj;
            if (UIDict.TryGetValue(uiName, out uiObj))
            {
                HandlerClose(uiObj);
            }
        }

        public void CloseUI(GameObject uiObj)
        {
            var uiName = uiObj.name;

            if (UIDict.ContainsKey(uiName))
            {
                HandlerClose(uiObj);
            }
        }

        public void CloseAndDestroyUI(string uiName)
        {
            GameObject uiObj;
            if (UIDict.TryGetValue(uiName, out uiObj))
            {
                CloseAndDestroyUI(uiObj);
            }
        }

        public void CloseAndDestroyUI(GameObject uiObj)
        {
            string uiName = uiObj.name;
            if (UIDict.ContainsKey(uiName))
            {
                UIDict.Remove(uiName);
            }
            if (UIOpenDict.ContainsKey(uiName))
            {
                UIOpenDict.Remove(uiName);
            }
            GameObject.Destroy(uiObj);
        }

        public void Clear()
        {
            UIOpenDict.Clear();

            foreach (var item in UIDict)
            {
                GameObject.Destroy(UIDict[item.Key]);
            }
            UIDict.Clear();
        }

        private void HandlerClose(GameObject uiObj)
        {
            uiObj.SetActive(false);
            UIOpenDict.Remove(uiObj.name);
        }

    }
}


