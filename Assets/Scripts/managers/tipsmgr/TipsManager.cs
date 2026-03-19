using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.utility.CommonInterface;
using Assets.Scripts.utility.Singleton;
using UnityEngine;

namespace Assets.Scripts.managers.uimgr
{
    public class TipsManager : TSingleton<TipsManager>, IManager, IOnAllManagerInit
    {
        private GameObject tipsPrefab;
        private const string tipsPath = "ui_prefab/common/Tips";
        private GameObject tipsRoot;

        private TipsManager()
        {
        }

        public void Init()
        {
            
        }

        public void Update()
        {

        }

        public void Destroy()
        {

        }

        public void OpenTips(string msg)
        {
            var tipsGo = Object.Instantiate(tipsPrefab, tipsRoot.transform, false);
            tipsGo.transform.localScale = Vector3.one;
            var tips = tipsGo.GetComponent<Tips>();
            tips.SetMassage(msg);
        }

        public void OnAllManagerInit()
        {
            tipsPrefab = ResourceManager.Instance.LoadResource(tipsPath, typeof(GameObject)) as GameObject;
            tipsRoot = GameObject.Find("Dialog/DialogRoot/TipsUI");
        }
    }
}
