using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSaveArea : MonoBehaviour
{
    private RectTransform rectTransform;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        ResetAnchor();
    }

    private void ResetAnchor()
    {
        Rect rect = Screen.safeArea;
        Vector2 anchorMin = rect.position;
        Vector2 anchorMax = rect.position + rect.size;
        anchorMin.x /= Screen.width;
        anchorMax.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}
