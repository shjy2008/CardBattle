using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Assets.Scripts.managers.inputmgr;

namespace Assets.Scripts.common
{
    public class HoverRotate : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
    {
        private Camera uiCamera;

        // Use this for initialization
        void Start()
        {
            uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 touchPos = InputHelper.GetTouchPos() / Const.GetResolutionRatio();
            bool contains = UIUtils.PositionInTransform(touchPos, transform);
            if (contains)
            {
                // Rotate while moving cursor but not moving card
                Vector3 cardMidPos = uiCamera.WorldToScreenPoint(transform.position) / Const.GetResolutionRatio();
                float diffX = touchPos.x - cardMidPos.x;
                float diffY = touchPos.y - cardMidPos.y;

                float angleX = diffY / 40.0f;
                float angleY = -diffX / 40.0f;
                transform.localRotation = Quaternion.Euler(angleX, angleY, 0);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            //// Rotate while moving cursor but not moving card
            //Vector3 cardMidPos = uiCamera.WorldToScreenPoint(transform.position);
            //float diffX = eventData.position.x - cardMidPos.x;
            //float diffY = eventData.position.y - cardMidPos.y;

            //float angleX = diffY / 40.0f;
            //float angleY = -diffX / 40.0f;
            //transform.localRotation = Quaternion.Euler(angleX, angleY, 0);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
