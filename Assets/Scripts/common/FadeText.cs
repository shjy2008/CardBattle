using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.common
{
    public class FadeText : MonoBehaviour
    {
        public float fadeTime = 0.5f;
        public float localMoveY = 50;
        public float fadeToOpacity = 0.5f;

        private float timer = 0.0f;
        private TextMeshProUGUI text;
        private bool isFading = false;

        // Use this for initialization
        void Start()
        {
            transform.DOLocalMove(transform.localPosition + new Vector3(0, localMoveY, 0), fadeTime);
            text = gameObject.GetComponent<TextMeshProUGUI>();
            isFading = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (isFading)
            {
                timer += Time.deltaTime;
                if (timer > fadeTime)
                {
                    timer = fadeTime;
                    isFading = false;
                    gameObject.SetActive(false);
                }
                Color color = text.color;
                color.a = 1 - timer / fadeTime * (1 - fadeToOpacity);
                text.color = color;
            }
        }
    }
}
