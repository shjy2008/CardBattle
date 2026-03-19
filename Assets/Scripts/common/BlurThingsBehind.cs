using System;
using System.Collections.Generic;
using Assets.Scripts.common;
using UnityEngine;
using UnityEngine.UI;

public class BlurThingsBehind : MonoBehaviour
{
    // Reference to the quad object with the blur shader
    public GameObject blurQuad;

    // RenderTexture to store the scene
    private RenderTexture renderTexture;

    private Camera mainCamera;

    private Vector3 prevPos = new Vector3(10000, 10000, 10000);

    Dictionary<Renderer, bool> renderersEnableDict = new Dictionary<Renderer, bool>();
    Dictionary<Graphic, bool> grahpicsEnableDict = new Dictionary<Graphic, bool>();
    Dictionary<RawImage, bool> rawImageEnableDict = new Dictionary<RawImage, bool>();

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("UICamera").GetComponent<Camera>();

        // Create a RenderTexture with the same size as the screen
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);

        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; ++i)
        {
            renderersEnableDict[renderers[i]] = renderers[i].enabled;
        }
        Graphic[] graphics = gameObject.GetComponentsInChildren<Graphic>();
        for (int i = 0; i < graphics.Length; ++i)
        {
            grahpicsEnableDict[graphics[i]] = graphics[i].enabled;
        }

        blurQuad.SetActive(false);

    }

    private void SetRenderersEnabled(bool enabled)
    {
        foreach (KeyValuePair<Renderer, bool> keyValuePair in renderersEnableDict)
        {
            if (keyValuePair.Key)
            {
                if (enabled)
                {
                    keyValuePair.Key.enabled = keyValuePair.Value;
                }
                else
                {
                    keyValuePair.Key.enabled = false;
                }
            }
        }
        foreach (KeyValuePair<Graphic, bool> keyValuePair in grahpicsEnableDict)
        {
            if (keyValuePair.Key)
            {
                if (enabled)
                {
                    keyValuePair.Key.enabled = keyValuePair.Value;
                }
                else
                {
                    keyValuePair.Key.enabled = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only need to render once the position changes
        if (transform.position != prevPos)
        {
            prevPos = transform.position;

            blurQuad.SetActive(true);

            RenderTexture prevRT = mainCamera.targetTexture;
            mainCamera.targetTexture = renderTexture;

            //gameObject.SetActive(false);
            SetRenderersEnabled(false);

            mainCamera.Render();

            // Reset
            //gameObject.SetActive(true);
            SetRenderersEnabled(true);
            mainCamera.targetTexture = prevRT;


            RectTransform rectTransform = transform.GetComponent<RectTransform>();

            Vector2 pivot = rectTransform.pivot;
            Vector2 pos = (Vector2)mainCamera.WorldToScreenPoint(transform.position) -
                new Vector2(rectTransform.rect.width * Const.GetResolutionRatio() * pivot.x, rectTransform.rect.height * Const.GetResolutionRatio() * pivot.y);

            Rect rect = new Rect(pos, rectTransform.rect.size * Const.GetResolutionRatio());

            RenderTexture.active = renderTexture;
            Texture2D croppedTexture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            int destX = 0;
            int destY = 0;
            if (rect.xMin < 0)
            {
                destX = (int)-rect.xMin;
                rect.width = rect.width + rect.xMin;
                rect.x = 0;
            }
            if (rect.xMax > Screen.width)
            {
                rect.width = rect.width - (rect.xMax - Screen.width);
                rect.x = Screen.width - rect.width;
            }
            if (rect.yMin < 0)
            {
                destY = (int)-rect.yMin;
                rect.height = rect.height + rect.yMin;
                rect.y = 0;
            }
            if (rect.yMax > Screen.height)
            {
                rect.height = rect.height - (rect.yMax - Screen.height);
                rect.y = Screen.height - rect.height;
            }

            croppedTexture.ReadPixels(rect, destX, destY);
            croppedTexture.Apply();
            RenderTexture.active = null;


            RawImage renderImage = blurQuad.GetComponent<RawImage>();
            renderImage.texture = croppedTexture;
        }
    }
}
