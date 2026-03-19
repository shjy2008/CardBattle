using Assets.Scripts.logics;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.utility;
using Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBarComponent : MonoBehaviour
{
    // outside frame size
    [SerializeField, Tooltip("This size will reset the outside frame size.")]
    private Vector2 size1 = new Vector2(212, 20);
    // inner frame and background size
    [SerializeField, Tooltip("This size will reset the inner frame and background size.")]
    private Vector2 size2 = new Vector2(206, 14);
    // hp progress size
    [SerializeField, Tooltip("This size will reset the progress1/progress2 size.")]
    private Vector2 size3 = new Vector2(203, 11);

    [SerializeField, Min(0)]
    private float hp = 100;

    [SerializeField, Min(0)]
    private float maxHP = 100;

    [SerializeField, Min(0)]
    private float armor = 0;

    [SerializeField, Min(0)]
    private float damage = 0; 

    [SerializeField, Min(0)]
    private float incomingArmor = 0;

    private GameObject outsideFrame;

    private SpriteRenderer spriteRenderer1, spriteRenderer2;

    private GroupSide ownerSide;

    //public void FaceToCamera()
    //{
    //    var mainCamera = GameObject.Find("UICamera").GetComponent<Camera>();
    //    Vector3 dir = Vector3.Normalize(mainCamera.transform.position - transform.position);
    //    Quaternion quat = Quaternion.FromToRotation(Vector3.back, dir);
    //    transform.rotation = quat;
    //}

    private void Awake()
    {
        outsideFrame = transform.FindRecursively("outside_frame").gameObject;

        var background = transform.FindRecursively("background").gameObject;
        var innerFrame = transform.FindRecursively("inner_frame").gameObject;
        var progress1 = transform.FindRecursively("progress1").gameObject;
        var progress2 = transform.FindRecursively("progress2").gameObject;

        background.GetComponent<RectTransform>().sizeDelta = size2;
        background.GetComponent<RectTransform>().localScale = new Vector3(size2.x / 2, size2.y * 10, 1); // sprite renderer issue

        SpriteRenderer spriteRenderer;
        spriteRenderer = outsideFrame.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // initiate the original materials (just like copy it), and set the properties
            spriteRenderer.material = Instantiate(spriteRenderer.material);
            // [Note] when using "Image" component we have to copy its materials, but with using "SpriteRenderer",
            // it is already using MaterialPropertyBlock automatically for it.
            spriteRenderer.material.SetVector("_RenderSize", new Vector4(size1.x, size1.y, 0));
            outsideFrame.GetComponent<RectTransform>().sizeDelta = size1;
            outsideFrame.GetComponent<RectTransform>().localScale = new Vector3(size1.x / 2, size1.y * 10, 1); // sprite renderer issue
        }

        spriteRenderer = innerFrame.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.material = Instantiate(spriteRenderer.material); 
            spriteRenderer.material.SetVector("_RenderSize", new Vector4(size2.x, size2.y, 0));
            innerFrame.GetComponent<RectTransform>().sizeDelta = size2;
            innerFrame.GetComponent<RectTransform>().localScale = new Vector3(size2.x / 2, size2.y * 10, 1); // sprite renderer issue
        }

        spriteRenderer1 = progress1.GetComponent<SpriteRenderer>();
        if (spriteRenderer1 != null)
        {
            spriteRenderer1.material = Instantiate(spriteRenderer1.material);
            progress1.GetComponent<RectTransform>().sizeDelta = size3;
            progress1.GetComponent<RectTransform>().localScale = new Vector3(size3.x / 2, size3.y * 10, 1); // sprite renderer issue
        }

        spriteRenderer2 = progress2.GetComponent<SpriteRenderer>();
        if (spriteRenderer2 != null)
        {
            spriteRenderer2.material = Instantiate(spriteRenderer2.material);
            progress2.GetComponent<RectTransform>().sizeDelta = size3;
            progress2.GetComponent<RectTransform>().localScale = new Vector3(size3.x / 2, size3.y * 10, 1); // sprite renderer issue
        }
    }

    void Start()
    {
        //FaceToCamera();
        EventManager.Instance.RegisterEventHandler<Action<GroupSide>>(Core.Events.EventType.UI_OnUpdateHPBar, OnUpdateHPBar);
    }

    private void OnDestroy()
    {
        EventManager.Instance.UnRegisterEventHandler<Action<GroupSide>>(Core.Events.EventType.UI_OnUpdateHPBar, OnUpdateHPBar);
    }

    // Update is called once per frame
    void Update()
    {
        if (!visible) return;
        // three things need be updated each frame:
        // (1) outside frame: display it if armor > 0, otherwise hide it.
        // (2) progress 1: HP+Armor
        // (3) progress 2: missing hp

        float actualArmor = armor + incomingArmor;

        outsideFrame.SetActive(actualArmor > 0);

        float t0 = hp + actualArmor;
        float t1 = t0 / (maxHP + actualArmor);
        spriteRenderer1.material.SetFloat("_Value", t1);
        float t2 = hp > 0 ? 1 - actualArmor / hp : 1;
        spriteRenderer1.material.SetFloat("_Offset", t2);

        float t3 = t0 > 0 ? Mathf.Clamp(1 - (damage / t0), 0, 1) : 1;
        spriteRenderer2.material.SetFloat("_Value", t3);
        spriteRenderer2.material.SetFloat("_StartPos", t1);
    }

    public void OnPreviewDamage(float _damage)
    {
        damage = _damage;
    }

    public void EndPreviewDamage()
    {
        damage = 0;
    }

    //public void OnReceiveDamage(float _damage)
    //{
    //    armor -= _damage;
    //    hp += Mathf.Min(armor, 0);
    //    // clamp range:
    //    armor = Mathf.Max(armor, 0);
    //    hp = Mathf.Max(hp, 0);
    //    damage = 0; // no need to set it because now it is to update HP/Armor
    //    //Debug.LogFormat("hp: {0}, armor: {1}", hp, armor);
    //}

    //public void OnReceiveDamage()
    //{
    //    armor -= damage;
    //    hp += Mathf.Min(armor, 0); // it means if armor is negative, we need to minus it as damaged. Otherwise ignore it.
    //    // clamp range:
    //    armor = Mathf.Max(armor, 0);
    //    hp = Mathf.Max(hp, 0);
    //    damage = 0; // no need to set it because now it is to update HP/Armor
    //    //Debug.LogFormat("hp: {0}, armor: {1}", hp, armor);
    //}

    private void OnUpdateHP(float _hp)
    {
        damage = 0;
        hp = _hp;
    }

    private void OnUpdateHPBar(GroupSide side)
    {
        if (side != ownerSide) return;

        var amount = BattleManager.Instance.GetCurrentBattle().GetTotalAmountByGroupSide(side, true);
        OnUpdateHP(amount);
        Debug.LogFormat("OnReceiveDamage: side:{0}, amount:{1}", side.ToString(), amount);
        TestPrint();
    }

    public void OnPreviewIncomingArmor(float _armor)
    {
        incomingArmor = _armor;
    }

    public void EndPreviewIncomingArmor()
    {
        incomingArmor = 0;
    }

    //public void OnReceiveIncomingArmor()
    //{
    //    armor += incomingArmor;
    //    incomingArmor = 0;
    //}

    public void OnUpdateArmor(float _armor)
    {
        // TODO: read data from battle.
        incomingArmor = 0;
        armor = _armor;

        Debug.LogFormat("OnUpdateArmor, armor:{0},\n incomingArmor:{1},\n maxHP:{2},\n hp:{3},\n",
            armor, incomingArmor, maxHP, hp);
    }

    public float GetArmor() { return armor; }

    public void SetHPArmor(float _hp, float _maxHP, float _armor)
    {
        hp = _hp;
        maxHP = _maxHP;
        armor = _armor;
    }

    private bool visible = true;
    public void SetVisible(bool _visible)
    {
        visible = _visible;
        gameObject.SetActive(visible);
    }

    public void TestPrint()
    {
        Debug.LogFormat("hp: {0}, damage: {1}, armor: {2}, maxHP: {3}, incomingArmor: {4}", hp, damage, armor, maxHP, incomingArmor);
    }

    public void SetGroupSide(GroupSide side)
    {
        ownerSide = side;
    }
}
