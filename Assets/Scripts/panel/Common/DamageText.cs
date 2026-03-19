using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

namespace Assets.Scripts.panel.common
{
    public class DamageText : MonoBehaviour
    {
        [SerializeField]
        private Text text = null;

        const float actionTime = 1.0f;

        // Use this for initialization
        void Start()
        {
        }

        public void SetText(string s, bool isCrit)
        {
            text.text = s;

            transform.DOLocalMoveY(transform.localPosition.y + 250f, actionTime);
            text.DOFade(0.5f, actionTime);

            if (isCrit)
            {
                transform.DOScale(3.0f, 0.2f);
            }

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
