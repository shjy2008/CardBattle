using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.managers.uimgr
{
    public class Tips : MonoBehaviour
    {
        [SerializeField]
        private Animator ani;

        [SerializeField]
        private Text text = null;

        [SerializeField]
        private Image bg = null;

        const float actionTime = 3.0f;

        void Start()
        {
            transform.DOLocalMoveY(transform.localPosition.y + 150f, actionTime);
            bg.DOFade(0.5f, actionTime);
            text.DOFade(0.5f, actionTime);
        }


        public void SetMassage(string msg)
        {
            text.text = msg;
            StartCoroutine(Wait());
        }

        void OpenFinish()
        {
            Destroy(gameObject);
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(actionTime);
            OpenFinish();
        }
    }
}