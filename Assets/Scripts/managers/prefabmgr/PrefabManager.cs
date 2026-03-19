using UnityEngine;
using System.Collections;
using Assets.Scripts.utility.Singleton;
using Assets.Scripts.utility.CommonInterface;
using System.Collections.Generic;
using Assets.Scripts.managers.resourcemgr;

// 初始化先加载一些通用的资源，后面直接Instantiate，减少io
namespace Assets.Scripts.managers.prefabmgr
{
    public class PrefabManager : TSingleton<PrefabManager>, IManager
    {
        public static Dictionary<string, string> pathData = new Dictionary<string, string>
        {
            //{"Card", "ui_prefab/common/Card" }, // 卡牌
            {"CardDissolve", "ParticlePack/EffectExamples/Misc Effects/Prefabs/DissolveFlakes" }, // 卡牌溶解
            {"CardFlyToPile", "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit Vol.2/FX_Shoot_Card_hit" }, // 卡牌飞回牌堆
            // WinPanel
            {"GoldCoinBlast", "Epic Toon FX/Prefabs/Interactive/Money/Coins/GoldCoinBlast" }, // 弹出奖励金币
            {"LevelupCylinderBlue", "Epic Toon FX/Prefabs/Interactive/Level Up/Cylinder/LevelupCylinderBlue" }, // 弹出奖励装备

        };

        private Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();

        private PrefabManager() { }

        public void Init()
        {
            foreach (KeyValuePair<string, string> data in pathData)
            {
                prefabDict[data.Key] = ResourceManager.Instance.LoadResource(data.Value, typeof(GameObject)) as GameObject;
            }
        }

        public void Update()
        {

        }

        public void Destroy()
        {

        }

        // 实例化一个对象
        public GameObject GetNewGameObject(string type, Transform parent)
        {
            GameObject gameObject = Object.Instantiate(prefabDict[type], parent);
            return gameObject;
        }

    }
}

