using UnityEngine;
using System.Collections;
using Assets.Scripts.managers.inputmgr;
using System.Collections.Generic;

namespace Assets.Scripts.common
{
    public class HoverScaleObj : MonoBehaviour
    {
        public GameObject obj;
        public float scale = 1.2f;

        private Vector3 originalScale;

        // Use this for initialization
        void Start()
        {
            originalScale = obj.transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            bool contains = UIUtils.TouchPosInTransform(transform);
            if (contains)
            {
                obj.transform.localScale = new Vector3(originalScale.x * scale, originalScale.y * scale, originalScale.z * scale);
            }
            else
            {
                obj.transform.localScale = originalScale;
            }
        }
    }
}
