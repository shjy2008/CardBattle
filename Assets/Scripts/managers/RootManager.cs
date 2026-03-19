using Assets.Scripts.utility.CommonInterface;
using Assets.Scripts.utility.Singleton;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Core
{
    public class RootManager : TSingleton<RootManager>, IManager
    {
        public delegate void onAllManagerInit();
        public static bool HasInit { get; private set; }
        private List<IManager> mgrList;

        //构造函数
        private RootManager()
        {
            mgrList = new List<IManager>();
        }

        //所有实现了IManager的,并且派生自TSingleton<>的都会被调用Init()
        public void Init()
        {
            // 其实最好的方式是subManager单独做成一个c#dll工程,这边load dll去处理,避免遍历一些不必要的类型,也方便热更(虽然不会有这一步估计),先这样做
            Assembly asm = Assembly.GetExecutingAssembly();
            Type t = null;
            IManager sysInst = null;
            var allTypes = asm.GetTypes();
            for (int i = 0; i < allTypes.Length; i++)
            {
                t = allTypes[i];
                if (t.IsInterface || t == typeof(RootManager))
                {
                    continue;
                }
                if (typeof(IManager).IsAssignableFrom(t))
                {
                    var tBase = t.BaseType;
                    if (tBase.IsGenericType && tBase.GetGenericTypeDefinition() == typeof(TSingleton<>))
                    {
                        //既要实现IManager接口的,又要继承于TSingleton<>的
                        sysInst = tBase.GetProperties()[0].GetValue(null, null) as IManager;
                        if (sysInst != null && !mgrList.Contains(sysInst))
                        {
                            sysInst.Init();
                            mgrList.Add(sysInst);
                        }
                    }
                }
            }
            OnAllManagerInit();
        }

        public void Update()
        {
            try
            {
                for (int i = 0; i < mgrList.Count; i++)
                {
                    mgrList[i].Update();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void LateUpdate()
        {
            try
            {
                ILateUpdate obj = null;
                for (int i = 0; i < mgrList.Count; i++)
                {
                    obj = mgrList[i] as ILateUpdate;
                    if (obj != null)
                    {
                        obj.LateUpdate();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Destroy()
        {
            BeforeAllManagerDestroy();
            foreach (var sysInst in mgrList)
            {
                sysInst.Destroy();
            }
            mgrList.Clear();
            HasInit = false;
        }

        private void OnAllManagerInit()
        {
            HasInit = true;
            foreach(var mgr in mgrList)
            {
                IOnAllManagerInit temp = mgr as IOnAllManagerInit;
                if (temp != null)
                {
                    try
                    {
                        temp.OnAllManagerInit();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        private void BeforeAllManagerDestroy()
        {
            HasInit = true;
            foreach (var mgr in mgrList)
            {
                IBeforeAllManagerDestroy temp = mgr as IBeforeAllManagerDestroy;
                if (temp != null)
                {
                    try
                    {
                        temp.BeforeAllManagerDestroy();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}
