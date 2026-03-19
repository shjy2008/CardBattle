using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Assets.Scripts.gm
{
    public class GM : MonoBehaviour
    {
        [SerializeField]
        private bool trigger = false; // hook it in Inspector to invoke the GM

        public string GMStr;
        public string GMParams;

        private Dictionary<string, MethodInfo> gmMethodInfoDict = new Dictionary<string, MethodInfo>();

        public void Start()
        {
            var methodInfos = typeof(GMFunc).GetMethods();
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.Name.StartsWith("GM"))
                {
                    gmMethodInfoDict.Add(methodInfo.Name, methodInfo);
                }
            }
        }

        ////当脚本被加载或者检视面板的值被修改时调用,仅编辑器下
        //private void OnValidate()
        //{
        //    if (!string.IsNullOrEmpty(GMStr) && GMStr.EndsWith("#"))
        //    {
        //        string gmMethodName = GMStr.Substring(0, GMStr.Length - 1);
        //        ExecGM(gmMethodName);
        //        GMStr = null;
        //        GMParams = null;
        //    }
        //}

        public void ExecGM(string gmMethodName)
        {
            MethodInfo gmMethodInfo = null;
            gmMethodInfoDict.TryGetValue(gmMethodName, out gmMethodInfo);
            if (gmMethodInfo != null)
            {
                Debug.Log("ExecGM: " + gmMethodName);
                try
                {
                    if (!string.IsNullOrEmpty(GMParams))
                    {
                        var parameters = gmMethodInfo.GetParameters();
                        var gmParamsStr = GMParams.Split(',');
                        Debug.Assert(parameters.Length == gmParamsStr.Length);
                        // try to convert string[] to the correct types of arguments of the calling GM function.
                        object[] gmParams = new object[parameters.Length];
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if(parameters[i].ParameterType.IsEnum)
                            {
                                gmParams[i] = Enum.Parse(parameters[i].ParameterType, gmParamsStr[i]);
                            }
                            else
                            {
                                gmParams[i] = Convert.ChangeType(gmParamsStr[i], parameters[i].ParameterType);
                            }
                        }
                        gmMethodInfo.Invoke(null, gmParams);
                    }
                    else
                    {
                        gmMethodInfo.Invoke(null, null);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        private void Update()
        {
            if(trigger)
            {
                trigger = false;
                if (!string.IsNullOrEmpty(GMStr))
                {
                    ExecGM(GMStr);
                    //GMStr = null;
                    //GMParams = null;
                }
            }
        }
    }
}