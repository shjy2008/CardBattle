using Assets.Scripts.common;
using Assets.Scripts.data;
using Assets.Scripts.logics;
using Assets.Scripts.panel.BattlePanel;
using Assets.Scripts.utility;
using Assets.Scripts.utility.Singleton;
using Core;
using Core.Events;
using System;
using System.Security.Cryptography;
using UnityEngine;

public class GameCore : MonoBehaviour
{
    private static GameCore instance;

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(this);

        if (instance == null) instance = this;
        else throw new Exception("Can not have two GameCore instance at the same time");

#if UNITY_EDITOR
        VerifyTableData();
#endif

        RootManager.Instance.Init();
    }

    // Update is called once per frame
    void Update()
    {
        RootManager.Instance.Update();
    }

    private void OnDestroy()
    {
        RootManager.Instance.Destroy();
    }

    void LateUpdate()
    {
        RootManager.Instance.LateUpdate();
    }

    // 用于计算离线时间
    private void OnApplicationPause(bool pause)
    {

    }

    private void OnApplicationQuit()
    {

    }
    
    public static void DelayCall(float delay, Action action)
    {
        instance.StartCoroutine(UIUtils.DelayedAction(delay, action));
    }

#if UNITY_EDITOR

    private void VerifyTableData()
    {
        Debug.Log("------------------- Start to VerifyTableData -------------------");
        // check 'status.xlsl'
        foreach(var data in Table_status.data)
        {
            var battleActionEffectType = Utils.ConvertStrToEnum<BattleActionEffectType>(data.Key);
            if (!Utils.HasAttribute<StatusFlag, BattleActionEffectType>(battleActionEffectType))
            {
                Debug.LogErrorFormat("status id {0} has not added 'StatusFlag' attribute on its enum definition.", data.Key);
            }
        }

        // check data correctness in 'action.xlsl'
        foreach(var data in Table_action.data)
        {
            var battleCardData = new BattleCardData(data.Key);
            foreach(var function in battleCardData.functionList)
            {
                function.VerifyBasicValueExpression(); // check whether it can be interpreted.
            }
        }

        Debug.Log("------------------- End VerifyTableData -------------------");
    }
#endif
}
