using Assets.Scripts.common;
using Assets.Scripts.data;
using Assets.Scripts.logics;
using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.managers.timermgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel;
using Assets.Scripts.utility.CommonInterface;
using Assets.Scripts.utility.Singleton;
using Core.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.managers.battlemgr
{
    public class BattleManager : TSingleton<BattleManager>, IManager, IOnAllManagerInit, IBeforeAllManagerDestroy
    {
        public float screenWidth;
        public float screenHeight;

        private BattleManager() { }

        public void Init()
        {
            screenHeight = Const.gameHeight;
            screenWidth = (float)UnityEngine.Screen.width / UnityEngine.Screen.height * screenHeight;
        }


        public void OnAllManagerInit()
        {
            //event
            EventManager.Instance.RegisterEventHandler<Action<string>>(Core.Events.EventType.OnSceneLoadFinish, OnSceneLoadFinish);

            GameCore.DelayCall(0, OnEnterDefaultScene);
        }

        public void BeforeAllManagerDestroy()
        {
            EventManager.Instance.UnRegisterEventHandler<Action<string>>(Core.Events.EventType.OnSceneLoadFinish, OnSceneLoadFinish);
        }

        public void Update()
        {
            if (curBattle != null) curBattle.Update();
        }

        public void Destroy()
        {
            
        }

        public void OnEnterDefaultScene()
        {
            Debug.Log("OnEnterDefaultScene!!!");

            //进入场景后.
            ResourceManager.Instance.LoadScene("BattleScene");

        }

        public void OnSceneLoadFinish(string curSceneName)
        {
            Debug.Log("OnSceneLoadFinish: " + curSceneName);

            ResourceManager.Instance.PreloadResources();

            var battleMapUIObj = UIManager.Instance.OpenUI("BattleMap");
            BattleMap battleMap = battleMapUIObj.GetComponent<BattleMap>();
            battleMap.SetTrigger();

            //CreateNewBattle();

            //UIManager.Instance.OpenUI("BattlePanel");
        }


        private Battle curBattle;
        public void CreateNewBattle(int difficulty)
        {
            // destroy previous battle first
            if(curBattle != null)
            {
                curBattle.Destroy();
                curBattle = null;
            }

            curBattle = new Battle();
            curBattle.Init(difficulty);
        }

        public Battle GetCurrentBattle()
        {
            return curBattle;
        }

        public bool IsClientRound()
        {
            return curBattle.IsClientRound();
        }
    }
}
