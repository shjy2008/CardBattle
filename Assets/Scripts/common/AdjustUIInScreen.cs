using UnityEngine;
using System.Collections;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.common;

public class AdjustUIInScreen : MonoBehaviour
{
    private Vector3 prevPos;

    // Use this for initialization
    void Start()
    {
        AdjustPos();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localPosition != prevPos)
        {
            prevPos = transform.localPosition;
            AdjustPos();
        }
    }

    private void AdjustPos()
    {
        Camera uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
        Vector3 pos = uiCamera.WorldToScreenPoint(transform.position) / Const.GetResolutionRatio();
        RectTransform rectTransform = transform.GetComponent<RectTransform>();
        float top = pos.y + rectTransform.rect.height * (1 - rectTransform.pivot.y);
        float right = pos.x + rectTransform.rect.width * (1 - rectTransform.pivot.x);
        float bottom = pos.y - rectTransform.rect.height * rectTransform.pivot.y;
        float left = pos.x - rectTransform.rect.width * rectTransform.pivot.x;

        float screenWidth = BattleManager.Instance.screenWidth;
        float screenHeight = BattleManager.Instance.screenHeight;

        float moveX = 0;
        float moveY = 0;
        if (top > screenHeight)
        {
            moveY = screenHeight - top;
        }
        else if (bottom < 0)
        {
            moveY = -bottom;
        }
        if (right > screenWidth)
        {
            moveX = screenWidth - right;
        }
        else if (left < 0)
        {
            moveX = -left;
        }

        transform.localPosition = new Vector3(transform.localPosition.x + moveX, transform.localPosition.y + moveY, transform.localPosition.z);
    }
}
