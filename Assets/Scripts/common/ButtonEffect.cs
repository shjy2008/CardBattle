using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class ButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image originImage;
    public Image hoverImage;
    public Image clickImage;
    public TextMeshProUGUI text;

    private const float fadeTime = 0.2f;
    private bool isHovering = false;
    private Color hoverColor;
    private bool isClicking = false;

    // Use this for initialization
    void Start()
    {
        hoverColor = hoverImage.color;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject hoverObj = hoverImage.gameObject;
        if (isHovering)
        {
            if (hoverImage.color.a < 1.0f)
            {
                float speed = 1.0f / fadeTime;
                hoverColor.a += Time.deltaTime * speed;
                hoverImage.color = hoverColor;
            }
        }
        else
        {
            if (hoverImage.color.a > 0.0f)
            {
                float speed = 1.0f / fadeTime;
                hoverColor.a -= Time.deltaTime * speed;
                if (hoverColor.a <= 0.0f)
                {
                    hoverObj.SetActive(false);
                }
                hoverImage.color = hoverColor;
            }
        }

        if (isClicking)
        {
            if (hoverImage.color.a < 1.0f)
            {
                float speed = 1.0f / 0.1f;
                hoverColor.a += Time.deltaTime * speed;
                if (hoverColor.a >= 1.0f)
                    isClicking = false;
                hoverImage.color = hoverColor;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        originImage.gameObject.SetActive(false);
        hoverImage.gameObject.SetActive(false);
        clickImage.gameObject.SetActive(true);
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        originImage.gameObject.SetActive(true);
        hoverImage.gameObject.SetActive(false);
        clickImage.gameObject.SetActive(false);

        hoverColor.a = 0.0f;
        hoverImage.color = hoverColor;

        Color newTextColor = text.color;
        newTextColor.r = 255;
        newTextColor.g = 255;
        newTextColor.b = 255;
        text.color = newTextColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        GameObject hoverObj = hoverImage.gameObject;
        hoverObj.SetActive(true);
        hoverColor.a = 0.0f;
        hoverImage.color = hoverColor;

        Color newTextColor = text.color;
        newTextColor.r = 0;
        newTextColor.g = 0;
        newTextColor.b = 0;
        text.color = newTextColor;

        originImage.gameObject.SetActive(true);
        hoverImage.gameObject.SetActive(true);
        clickImage.gameObject.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        Color newTextColor = text.color;
        newTextColor.r = 255;
        newTextColor.g = 255;
        newTextColor.b = 255;
        text.color = newTextColor;

        originImage.gameObject.SetActive(true);
        hoverImage.gameObject.SetActive(true);
        clickImage.gameObject.SetActive(false);
    }
}
