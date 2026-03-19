using Assets.Scripts.logics;
using Assets.Scripts.panel.BattlePanel;
using Assets.Scripts.utility.Common;
using Assets.Scripts.utility.CommonInterface;
using Assets.Scripts.utility.Singleton;
using Core.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.managers.resourcemgr
{
    public delegate void LoadSceneCallBack();
    public class ResourceManager : TSingleton<ResourceManager>, IManager
    {
        private ResourceManager() { }

        private Dictionary<string, UnityEngine.Object> prefabCacheDict;
        private Dictionary<string, Queue<GameObject>> resourcePool; // resource pool
        private GameObject resourcePoolRoot;

        public void Init()
        {
            curSceneName = null;
            loadSceneOp = new CTuple2<string, AsyncOperation>();
            unLoadSceneOp = new CTuple2<string, AsyncOperation>();
            prefabCacheDict = new Dictionary<string, UnityEngine.Object>();
            resourcePool = new Dictionary<string, Queue<GameObject>>();

            var gameRoot = GameObject.Find("GameRoot");
            resourcePoolRoot = new GameObject("ResourcePoolRoot");
            resourcePoolRoot.transform.parent = gameRoot.transform;
        }

        public void Update()
        {
            if (!string.IsNullOrEmpty(unLoadSceneOp.arg1))
            {
                if (unLoadSceneOp.arg2.isDone)
                {
                    unLoadSceneOp.Clear();
                }
            }

            if (!string.IsNullOrEmpty(loadSceneOp.arg1))
            {
                if (loadSceneOp.arg2.isDone)
                {
                    curSceneName = loadSceneOp.arg1;
                    var curScene = SceneManager.GetSceneByName(curSceneName);
                    SceneManager.SetActiveScene(curScene);
                    EventManager.Instance.SendEventSync(Core.Events.EventType.OnSceneLoadFinish, curSceneName);
                    loadSceneOp.Clear();
                }
            }
        }

        public void Destroy()
        {
            unLoadSceneOp.Clear();
            loadSceneOp.Clear();
        }

        #region scene
        private string curSceneName;
        private CTuple2<string, AsyncOperation> unLoadSceneOp;
        private CTuple2<string, AsyncOperation> loadSceneOp;

        public bool LoadSceneAsync(string sceneName)
        {
            if (curSceneName == sceneName)
            {
                //当前已经是该场景了
                return false;
            }
            if (loadSceneOp.arg1 == sceneName || unLoadSceneOp.arg1 == sceneName)
            {
                //如果是正在加载该场景,那就不用处理
                //如果是正在卸载该场景,得等卸载完
                return false;
            }
            //卸载当前场景(非默认场景)
            if (!string.IsNullOrEmpty(curSceneName))
            {
                unLoadSceneOp.arg1 = curSceneName;
                unLoadSceneOp.arg2 = SceneManager.UnloadSceneAsync(curSceneName);
                curSceneName = null;
            }

            loadSceneOp.arg1 = sceneName;
            loadSceneOp.arg2 = SceneManager.LoadSceneAsync(sceneName);

            EventManager.Instance.SendEventSync(Core.Events.EventType.OnSceneLoadStart, sceneName);
            return true;
        }

        public bool LoadScene(string sceneName)
        {
            if (curSceneName == sceneName)
            {
                //当前已经是该场景了
                return false;
            }
            if (loadSceneOp.arg1 == sceneName || unLoadSceneOp.arg1 == sceneName)
            {
                //如果是正在加载该场景,那就不用处理
                //如果是正在卸载该场景,得等卸载完
                return false;
            }
            //卸载当前场景(非默认场景)
            if (!string.IsNullOrEmpty(curSceneName))
            {
                unLoadSceneOp.arg1 = curSceneName;
                unLoadSceneOp.arg2 = SceneManager.UnloadSceneAsync(curSceneName);
                curSceneName = null;
            }
            EventManager.Instance.SendEventSync(Core.Events.EventType.OnSceneLoadStart, sceneName);
            SceneManager.LoadScene(sceneName);
            EventManager.Instance.SendEventSync(Core.Events.EventType.OnSceneLoadFinish, sceneName);
            return true;
        }

        public bool IsChangingScene()
        {
            return false;
        }
        #endregion

        //todo:资源缓存.
        //todo:资源卸载.

        public UnityEngine.Object LoadResource(string path, Type typ, bool needCache = false)
        {
            //todo:到时候改为用 assetbundle.
            // Load assets from the Resources folder.  Ignore other named and typed assets.
            //unity这个接口需要在assets文件夹下再创建一个Resources文件夹,路径都是相对于这个文件夹的路径
            if (prefabCacheDict.ContainsKey(path)) return prefabCacheDict[path];
            else
            {
                var obj = Resources.Load(path, typ);
                if (needCache) prefabCacheDict[path] = obj;
                return obj;
            }
        }

        public GameObject LoadResourceAsGameObject(string path, bool needCache = false)
        {
            var go = LoadResource(path, typeof(GameObject), needCache);
            if (go != null)
            {
                return GameObject.Instantiate(go) as GameObject;
            }
            return null;
        }

        public ResourceRequest LoadResourceAsync(string path, Type typ)
        {
            //todo:到时候改为用 assetbundle.
            // Load assets from the Resources folder.  Ignore other named and typed assets.
            //unity这个接口需要在assets文件夹下再创建一个Resources文件夹,路径都是相对于这个文件夹的路径
            return Resources.LoadAsync(path, typ);
        }

        public bool IsLoadingScene()
        {
            return !string.IsNullOrEmpty(loadSceneOp.arg1) && !loadSceneOp.arg2.isDone;
        }

        public string GetCurSceneName()
        {
            return curSceneName;
        }

        public void LoadResourceIntoPool(string path, int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject gameObj = LoadResourceAsGameObject(path, true);
                CacheResourceIntoPool(path, gameObj);
            }
        }

        public GameObject FetchResourceFromPool(string path)
        {
            if(resourcePool.ContainsKey(path))
            {
                var allObjs = resourcePool[path];
                if(allObjs.Count > 0)
                {
                    var gameObj = allObjs.Dequeue();
                    gameObj.SetActive(true);
                    gameObj.transform.parent = null;
                    return gameObj;
                }
            }

            Debug.LogFormat("Can not FetchResourceFromPool at path: {0}", path);

            // reach at here meaning not found.
            return LoadResourceAsGameObject(path, true);
        }

        public void CacheResourceIntoPool(string path, GameObject gameObj)
        {
            if (!resourcePool.ContainsKey(path)) resourcePool.Add(path, new Queue<GameObject>());
            gameObj.SetActive(false);
            gameObj.transform.parent = resourcePoolRoot.transform;
            resourcePool[path].Enqueue(gameObj);
        }

        public void ClearCacheResources(string path)
        {
            if (resourcePool.ContainsKey(path))
            {
                var allObjs = resourcePool[path];
                foreach (var gameObj in allObjs)
                {
                    UnityEngine.Object.Destroy(gameObj);
                }
                resourcePool.Remove(path);
            }
        }

        public void PreloadResources()
        {
            // TODO: below preload assets number is too large for some assets.
            // need to compute it carefully for its maximum value.

            Debug.Log("---------- PreloadResources Start ----------");
            // TODO: make a loading scene for pre-loading all assets
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // preload the resources
            LoadResourceIntoPool("3d_prefab/explosion_cube", SimulationPlayer.maxCubeNum * 72);

            LoadResourceIntoPool("3d_prefab/battlefield/CurveMeshCreator", 17);

            LoadResourceIntoPool(BattleFieldComponent.armyGroupPrefabPath, 72);
            
            // ------------------------ VFXs related ------------------------
            const int meleeVFXsMaxNum = 7;

            var effectPlayerMaxNum = BattleFieldComponent.GetEffectPlayerMaxNum();
            Debug.LogFormat("effectPlayerMaxNum: {0}", effectPlayerMaxNum);

            var battleActionEffectTypeList = Enum.GetValues(typeof(BattleActionEffectType));
            foreach (var item in battleActionEffectTypeList)
            {
                BattleActionEffectType actionEffectType = (BattleActionEffectType)item;
                string projectile, muzzle, hit;
                AssetsMapper.GetVFXPaths(actionEffectType, out projectile, out muzzle, out hit);

                //Debug.LogFormat("actionEffectType: {0},\n projectile: {1},\n muzzle: {2},\n hit: {3}\n",
                //    actionEffectType.ToString().ToString(), projectile, muzzle, hit);

                // preload the VFXs according the approximated maximum number
                int count = BattleLogics.IsDistantAttack(actionEffectType) ?
                        effectPlayerMaxNum : meleeVFXsMaxNum;
                
                if (!string.IsNullOrEmpty(projectile)) LoadResourceIntoPool(projectile, count);
                if (!string.IsNullOrEmpty(muzzle)) LoadResourceIntoPool(muzzle, count);
                if (!string.IsNullOrEmpty(hit)) LoadResourceIntoPool(hit, count);
            }

            LoadResourceIntoPool("vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_Defense", 3 + meleeVFXsMaxNum);
            LoadResourceIntoPool("vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_Defense_hit", effectPlayerMaxNum);

            LoadResourceIntoPool("vfx/VFX_Klaus/Prefabs/Hyper Casual FX/HCFX_Buff_Up", 6);
            LoadResourceIntoPool("vfx/VFX_Klaus/Prefabs/Hyper Casual FX/HCFX_Buff_Down", 6);

            // ---------------------------------------------------------------
            stopwatch.Stop();
            var elapsedTime = stopwatch.Elapsed;
            Debug.LogFormat("Totoal time spent: {0} ms", elapsedTime.TotalMilliseconds);
            Debug.Log("---------- PreloadResources Done ----------");
        }
    }
}
