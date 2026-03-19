using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.managers.uimgr
{
    public class BaseUI : MonoBehaviour
    {
        [SerializeField] protected Animator animator;

        protected bool isAnimating = false;

        protected List<UIComponent> uiComponentList = new List<UIComponent>();

        protected virtual void Awake()
        {
        }

        protected virtual void Start() { }

        protected virtual void Update()
        {
            foreach (UIComponent uiComp in uiComponentList)
            {
                uiComp.Update();
            }
        }

        protected virtual void OnEnable()
        {
            
        }

        protected virtual void OnDisable()
        {
        }

        public virtual void Close()
        {
            RemoveAllUIComponents();
            UIManager.Instance.CloseUI(gameObject);
        }

        public virtual void CloseAndDestroy()
        {
            RemoveAllUIComponents();
            UIManager.Instance.CloseAndDestroyUI(gameObject);
        }

        protected virtual void PlayCloseAni()
        {
            if (animator)
            {
                if(!isAnimating)
                    animator.Play("Close");
            }
            else
            {
                Close();
            }
        }

        protected virtual void OpenStart()
        {
            isAnimating = true;
        }

        protected virtual void OpenFinish()
        {
            isAnimating = false;
        }

        protected virtual void CloseStart()
        {
            isAnimating = true;
        }

        protected virtual void CloseFinish()
        {
            isAnimating = false;
            Close();
        }

        protected virtual void AddUIComponent(UIComponent uiComp)
        {
            uiComponentList.Add(uiComp);
            uiComp.Init();
        }

        protected virtual void RemoveUIComponent(UIComponent uiComp)
        {
            uiComponentList.Remove(uiComp);
            uiComp.Destroy();
        }

        protected virtual void RemoveAllUIComponents()
        {
            foreach (UIComponent uiComp in uiComponentList)
            {
                uiComp.Destroy();
            }
            uiComponentList.Clear();
        }

        public virtual T GetUIComponent<T>() where T : UIComponent
        {
            foreach (UIComponent comp in uiComponentList)
            {
                if (comp.GetType() == typeof(T))
                {
                    return (T)comp;
                }
            }

            return null;
        }
    }
}


