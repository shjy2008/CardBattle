using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text fpsLabel;

    private void Update()
    {
        fpsLabel.SetText(string.Format("FPS: {0}", Mathf.RoundToInt(1f / Time.smoothDeltaTime)));
    }
}
