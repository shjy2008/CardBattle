using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Assets.Scripts.managers.inputmgr;
using System.Collections.Generic;
using System;
using Assets.Scripts.utility;
using Assets.Scripts.panel.BattlePanel;
using Assets.Scripts.managers.uimgr;
using TMPro;
using Assets.Scripts.common;
using Assets.Scripts.logics;
using ChristinaCreatesGames.Typography.TooltipForTMP;
using Core.Events;
using UnityEngine.UI;
using Assets.Scripts.managers.battlemgr;

public class BattleCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool isUI = false;

    public GameObject elementIcon0;

    public static float cardScale = 0.71f; // Scale of card in hand
    const float cardScaleSelect = 0.9f;
    float useCardPosY = 400.0f; // If card's position.y > this number, use this card

    private int cardIndex;
    private BattleCardData cardData;

    private Vector3 originalBgScale;
    private Vector3 originalLocalPos;
    private int originalSiblingIndex;
    private Camera uiCamera;
    private Dictionary<Renderer, int> rendererToSortingOrder;

    private Tweener scaleTweener = null;
    private Tweener moveTweener = null;
    private Tweener bgTweener = null;

    private Vector2 touchBeganPos;
    private bool isClicking = false;
    private bool isMoving = false;
    private bool hasUsed = false;
    private bool isFlying = false;
    private List<Tuple<float, Vector3>> timeToPosList = new List<Tuple<float, Vector3>>();
    private const float timeAgoElapse = 0.1f;

    private bool isEnemy = false;

    private List<GameObject> newlyCreatedSequenceObjs = new List<GameObject>();
    private List<GameObject> newlyCreatedElementObjs = new List<GameObject>();


    // Use this for initialization
    void Start()
    {
        if (!isUI)
        {
            //transform.localScale = new Vector3(cardScale, cardScale, cardScale);
            originalSiblingIndex = transform.GetSiblingIndex();
            uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();

            originalBgScale = transform.Find("mask/bg").localScale;

            // Set order in layer to ensure render properly
            MeshRenderer descriptionTitleRenderer = transform.Find("description/title/text").GetComponent<MeshRenderer>();
            descriptionTitleRenderer.sortingLayerName = "Card";
            descriptionTitleRenderer.sortingOrder += 4;

            MeshRenderer descriptionDescRenderer = transform.Find("description/desc/text").GetComponent<MeshRenderer>();
            descriptionDescRenderer.sortingLayerName = "Card";
            descriptionDescRenderer.sortingOrder += 4;

            MeshRenderer infoNumRenderer = transform.Find("info/num").GetComponent<MeshRenderer>();
            infoNumRenderer.sortingLayerName = "Card";
            infoNumRenderer.sortingOrder += 4;

            MeshRenderer cubeRenderer = transform.Find("cube").GetComponent<MeshRenderer>();
            cubeRenderer.sortingLayerName = "Card";
            cubeRenderer.sortingOrder += -1;

            setDescriptionCloserToCamera(false);

        }

        UpdateUI();

        if (!isUI)
        {
            rendererToSortingOrder = Utils.GetChildrenRendererToSortingOrder(transform);
        }
    }

    public void SetCardIndex(int index) { cardIndex = index; }
    public void SetCardData(BattleCardData _cardData)
    {
        cardData = _cardData;
    }
    public BattleCardData GetCardData() { return cardData; }
    public void SetOriginalLocalPos(Vector3 pos) { originalLocalPos = pos; }
    public float GetCardScale() { return cardScale; }
    public void SetIsFlying(bool value) { isFlying = value; }

    public static List<string> GetSequenceList(BattleCardData _cardData)
    {
        List<string> sequenceList = new List<string>();
        for (int i = 0; i < _cardData.actionData.actionTypes.Count; ++i)
        {
            string actionType = _cardData.actionData.actionTypes[i];
            if (actionType == "SPECIAL" || actionType == "MODIFIER")
                sequenceList.Add("circle");
            else if (actionType == "ATTACK")
                sequenceList.Add("triangle");
            else if (actionType == "DEFENSE")
                sequenceList.Add("square");
        }
        return sequenceList;
    }

    public void UpdateUI()
    {
        if (cardData == null)
            return;

        Sprite bgSprite = Resources.Load<Sprite>("images/tactic/tactic_bg/" + cardData.actionData.faceimage);
        if (isUI)
        {
            transform.Find("mask/bg").GetComponent<Image>().sprite = bgSprite;
        }
        else
        {
            transform.Find("mask/bg").GetComponent<SpriteRenderer>().sprite = bgSprite;
        }
        transform.Find("description/title/text").gameObject.GetComponent<TMP_Text>().text = cardData.actionData.name;
        //transform.Find("description/desc/text").gameObject.GetComponent<TMP_Text>().text = cardData.actionData.description;

        // desc
        UpdateDesc();

        // cost
        UpdateCost();

        // Action sequence
        foreach (GameObject obj in newlyCreatedSequenceObjs)
        {
            GameObject.Destroy(obj);
        }
        newlyCreatedSequenceObjs.Clear();
        List<string> sequenceList = GetSequenceList(cardData);
        for (int i = 0; i < sequenceList.Count; ++i)
        {
            string sequenceStr = sequenceList[i];
            string iconPath = "images/tactic/tactic_sequence/tactic_seq_" + sequenceStr;
            GameObject obj0 = transform.Find("info/sequence/icon0").gameObject;
            GameObject obj;
            if (i == 0)
            {
                obj = obj0;
            }
            else
            {
                obj = GameObject.Instantiate(obj0, obj0.transform.parent);
                Vector3 pos = obj0.transform.localPosition;
                pos.x += 20 * i;
                obj.transform.localPosition = pos;
                obj.transform.localScale = obj0.transform.localScale;
                newlyCreatedSequenceObjs.Add(obj);
            }
            if (isUI)
                obj.GetComponent<Image>().sprite = Resources.Load<Sprite>(iconPath);
            else
                obj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(iconPath);
        }

        // Element
        UpdateElement();

        // Border
        GameObject borderObj = transform.Find("frame").gameObject;
        if (cardData.actionData.ifSpecial)
        {
            borderObj.SetActive(true);
            string path = "images/tactic/tactic_border/card_border_";
            if (cardData.actionData.cost == 1)
                path += "bronze";
            else if (cardData.actionData.cost == 2)
                path += "silver";
            else if (cardData.actionData.cost == 3)
                path += "gold";
            else if (cardData.actionData.cost == -1)
                path += "empire";
            if (isUI)
                borderObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
            else
                borderObj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(path);
        }
        else
        {
            borderObj.SetActive(false);
        }
    }

    public void UpdateCost()
    {
        string numText;
        if (cardData.actionData.cost == -1)
            numText = "x";
        else
        {
            int cost = cardData.actionData.cost - cardData.minusCost;
            if (cost < 0)
                cost = 0;
            numText = cost.ToString();
        }
        transform.Find("info/num").gameObject.GetComponent<TMP_Text>().text = numText;
    }

    public void UpdateElement()
    {
        foreach (GameObject obj in newlyCreatedElementObjs)
        {
            GameObject.Destroy(obj);
        }
        newlyCreatedElementObjs.Clear();

        //elementIcon0.SetActive(false);
        for (int i = 0; i < cardData.actionData.element.Count; ++i)
        {
            int elementId = BattleElementComponent.GetIdByElementName(cardData.actionData.element[i]);
            string iconPath = "images/tactic/tactic_element/element_" + elementId.ToString();

            //GameObject obj0 = elementIcon0;
            GameObject obj0 = transform.Find("info/element/icon0").gameObject;
            //Transform trans = obj0.transform.parent.Find(i.ToString());
            GameObject obj;
            if (i == 0)
                obj = obj0;// trans.gameObject;
            else
            {
                obj = GameObject.Instantiate(obj0, obj0.transform.parent);
                obj.GetComponent<BattleCardElementMono>().highlightObj = obj.transform.Find("highlight").gameObject;
                newlyCreatedElementObjs.Add(obj);
            }
            //obj.name = i.ToString();

            // highlight
            bool isNextElement = false;
            if (!isUI)
                isNextElement = (elementId == BattlePanel.GetBattleElementComponent().GetNextElementId());
            obj.GetComponent<BattleCardElementMono>().SetHighlighted(isNextElement);

            obj.SetActive(true);
            Vector3 pos = obj0.transform.localPosition;
            pos.y -= 20 * i;
            obj.transform.localPosition = pos;
            obj.transform.localScale = obj0.transform.localScale;
            if (isUI)
                obj.GetComponent<Image>().sprite = Resources.Load<Sprite>(iconPath);
            else
                obj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(iconPath);
        }
    }

    public void UpdateDesc()
    {
        GameObject descTextObj = transform.Find("description/desc/text").gameObject;
        descTextObj.GetComponent<TMP_Text>().text = BattleCardDesc.GetDescWithCardData(cardData);

        List<TooltipInfos> tooltipContentList = new List<TooltipInfos>();
        TooltipInfos tooltipInfo = new TooltipInfos();
        tooltipInfo.Keyword = "testLink"; // TODO
        tooltipInfo.Description = "test link description";
        tooltipContentList.Add(tooltipInfo);
        descTextObj.GetComponent<TooltipHandlerHover>().tooltipContentList = tooltipContentList;
    }

    private bool CanControl()
    {
        if (isUI)
            return false;

        if (BattlePanel.GetBattleCardComponent().IsUsingCard)
            return false;

        return (!isClicking && !isMoving && !hasUsed && !isFlying && !isEnemy);
    }

    public void SetIsEnemy(bool _isEnemy) { isEnemy = _isEnemy; }

    // Update is called once per frame
    void Update()
    {
        Vector2 touchPos = InputHelper.GetTouchPos() / Const.GetResolutionRatio();
        if (isClicking && !hasUsed)
        {
            // Move card
            float threshold = 100.0f;
            if ((touchPos - touchBeganPos).magnitude > threshold) // Card follow cursor only when move exceed a threshold
            {
                isClicking = false;
                isMoving = true;
            }
        }

        if (isMoving && !hasUsed)
        {
            transform.position = uiCamera.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, transform.position.z));

            timeToPosList.Add(new Tuple<float, Vector3>(Time.time, transform.position));

            // Find pos at 'timeAgoElapse' ago
            float timeAgo = Time.time - timeAgoElapse;
            Vector3 timeAgoPos = new Vector3();
            int i = 0;
            bool found = false;
            for (; i < timeToPosList.Count; ++i)
            {
                Tuple<float, Vector3> tuple = timeToPosList[i];
                if (tuple.Item1 < timeAgo)
                {
                    timeAgoPos = tuple.Item2;
                    found = true;
                    break;
                }
            }

            // Rotate card while moving
            if (found)
            {
                timeToPosList.RemoveRange(0, i + 1);

                Vector3 posDiff = transform.position - timeAgoPos;
                if (posDiff != Vector3.zero)
                {
                    float t = timeAgoElapse;
                    float speedX = posDiff.x / t;
                    float speedY = posDiff.y / t;

                    float angleX = speedY / 60.0f;
                    float angleY = speedX / 60.0f;
                    const float maxAngle = 30.0f;
                    angleX = Mathf.Clamp(angleX, -maxAngle, maxAngle);
                    angleY = Mathf.Clamp(angleY, -maxAngle, maxAngle);
                    transform.localRotation = Quaternion.Euler(angleX, angleY, 0);
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanControl())
            return;

        touchBeganPos = eventData.position / Const.GetResolutionRatio();
        isClicking = true;
        BattlePanel.GetBattleCardComponent().CurrentMovingCard = gameObject;
        setDescriptionCloserToCamera(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (hasUsed || isFlying || isUI || BattleManager.Instance.GetCurrentBattle().IsClientAutoUseDelayedCard())
            return;

        BattleCardComponent cardComponent = BattlePanel.GetBattleCardComponent();
        cardComponent.CurrentMovingCard = null;

        Vector3 endPos = uiCamera.WorldToScreenPoint(transform.position) / Const.GetResolutionRatio();
        if (endPos.y > useCardPosY)
        {
            bool canUse = BattlePanel.GetBattleCardComponent().GetBattleCardCost().CheckCanUseCard(cardData);
            if (canUse)
            {
                Debug.Log("Use card! CardIndex: " + cardIndex);

                hasUsed = true;
                BattlePanel.GetBattleCardComponent().UseCard(cardData, gameObject);
            }
            else
            {
                BackToHand();
            }
        }
        else
        {
            BackToHand();
        }
    }

    private void BackToHand()
    {
        isMoving = false;
        isClicking = false;
        transform.SetSiblingIndex(originalSiblingIndex);
        transform.localPosition = originalLocalPos;
        transform.localScale = new Vector3(cardScale, cardScale, cardScale);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        timeToPosList.Clear();
        transform.Find("mask/bg").localScale = originalBgScale;
        // Disable sprite mask, because bug appear when two sprite masks overlaps
        transform.Find("mask").GetComponent<SpriteMask>().enabled = false;
        transform.Find("mask/bg").GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
        setDescriptionCloserToCamera(false);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!CanControl())
            return;

        if (BattlePanel.GetBattleCardComponent().CurrentMovingCard != null)
            return;

        // Rotate while moving cursor but not moving card
        Vector3 cardMidPos = uiCamera.WorldToScreenPoint(transform.position) / Const.GetResolutionRatio();
        float diffX = eventData.position.x / Const.GetResolutionRatio() - cardMidPos.x;
        float diffY = eventData.position.y / Const.GetResolutionRatio() - cardMidPos.y;

        float angleX = diffY / 40.0f;
        float angleY = -diffX / 40.0f;
        transform.localRotation = Quaternion.Euler(angleX, angleY, 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanControl())
            return;

        if (BattlePanel.GetBattleCardComponent().CurrentMovingCard != null)
            return;

        transform.SetAsLastSibling();
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - 50);

        Utils.AddChildrenSortingOrder(transform, 1000);

        // Scale the card
        if (scaleTweener != null)
        {
            scaleTweener.Kill();
        }
        scaleTweener = transform.DOScale(cardScaleSelect, 0.3f);
        scaleTweener.onComplete = () =>
        {
            scaleTweener = null;
        };

        // Move the card upward
        if (moveTweener != null)
        {
            moveTweener.Kill();
        }
        moveTweener = transform.DOLocalMoveY(160, 0.3f);
        moveTweener.onComplete = () =>
        {
            moveTweener = null;
        };

        // Scale background image
        if (bgTweener != null)
        {
            bgTweener.Kill();
        }
        Transform bg = transform.Find("mask/bg");
        bgTweener = bg.DOScale(1.2f * originalBgScale.x, 0.3f);
        bgTweener.onComplete = () =>
        {
            bgTweener = null;
        };

        // Enable sprite mask, because bug appear when two sprite masks overlaps
        transform.Find("mask").GetComponent<SpriteMask>().enabled = true;
        transform.Find("mask/bg").GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        // Preview card effect in battle field
        //BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
        //BattleFieldComponent battlefieldComponent = battlePanel.GetUIComponent<BattleFieldComponent>();
        //battlefieldComponent.TryPreviewCard(cardData);
        EventManager.Instance.SendEventSync(Core.Events.EventType.UI_OnPreviewCard, cardData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CanControl())
            return;

        if (BattlePanel.GetBattleCardComponent().CurrentMovingCard != null)
            return;

        //Utils.AddChildrenSortingOrder(transform, -1000);

        transform.SetSiblingIndex(originalSiblingIndex);
        // Reset sorting order in children
        foreach (KeyValuePair<Renderer, int> kvp in rendererToSortingOrder)
        {
            if (kvp.Key)
            {
                kvp.Key.sortingOrder = kvp.Value;
            }
        }

        if (scaleTweener != null)
        {
            scaleTweener.Kill();
            scaleTweener = null;
        }
        transform.localScale = new Vector3(cardScale, cardScale, cardScale);

        if (moveTweener != null)
        {
            moveTweener.Kill();
            moveTweener = null;
        }

        if (bgTweener != null)
        {
            bgTweener.Kill();
            bgTweener = null;
        }
        transform.Find("mask/bg").localScale = originalBgScale;
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        transform.localPosition = originalLocalPos;
        // Disable sprite mask, because bug appear when two sprite masks overlaps
        transform.Find("mask").GetComponent<SpriteMask>().enabled = false;
        transform.Find("mask/bg").GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;

        // Stop preview card effect in battle field
        //BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
        //BattleFieldComponent battlefieldComponent = battlePanel.GetUIComponent<BattleFieldComponent>();
        //battlefieldComponent.EndPreviewCard();
        EventManager.Instance.SendEventSync(Core.Events.EventType.UI_OnEndPreviewCard);

        UIManager.Instance.CloseAndDestroyUI("DescPanel");
    }

    private void setDescriptionCloserToCamera(bool isCloser)
    {
        int z = 0;
        if (isCloser)
        {
            z = -15;
        }
        Transform descriptionTransform = transform.Find("description").transform;
        Vector3 temp = descriptionTransform.localPosition;
        temp.z = z;
        descriptionTransform.localPosition = temp;

        Transform infoTransform = transform.Find("info").transform;
        temp = infoTransform.localPosition;
        temp.z = z;
        infoTransform.localPosition = temp;
    }
}
