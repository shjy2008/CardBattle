using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.common;
using Assets.Scripts.managers.prefabmgr;
using Assets.Scripts.managers.uimgr;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.panel.Win
{
    public class WinPanel : BaseUI
    {
        enum EffectState
        {
            EffectState_rollingSaving,
            EffectState_rollingInterest1,
            EffectState_rollingInterest2,
            EffectState_rollingBounties,
            EffectState_rollingCaptured,
            EffectState_allFinished
        }

        private EffectState curState;

        List<Tuple<int, int>> bountiesIdToCoin = new List<Tuple<int, int>>();
        private Coroutine bountiesCoroutine1;
        private Coroutine bountiesCoroutine2;
        private List<int> bountiesSelectedList = new List<int>();

        List<Tuple<int, int>> capturedIdToCoin = new List<Tuple<int, int>>();
        private Coroutine capturedCoroutine1;
        private Coroutine capturedCoroutine2;
        private List<int> capturedSelectedList = new List<int>();


        // Use this for initialization
        void Start()
        {
            // Init
            RollNumber rollInterest1 = transform.Find("bg/interest1/num").GetComponent<RollNumber>();
            rollInterest1.SetNum(0);
            RollNumber rollInterest2 = transform.Find("bg/interest2/num").GetComponent<RollNumber>();
            rollInterest2.SetNum(0);
            RollNumber rollBounties = transform.Find("bg/bounties/num").GetComponent<RollNumber>();
            rollBounties.SetNum(0);
            RollNumber rollCaptured = transform.Find("bg/captured/num").GetComponent<RollNumber>();
            rollCaptured.SetNum(0);

            InitBounties();
            InitCaptured();


            // Start
            curState = EffectState.EffectState_rollingSaving;

            RollNumber rollSaving = transform.Find("bg/saving/num").GetComponent<RollNumber>();
            rollSaving.SetNum(0);
            rollSaving.StartRolling(525, 1.0f, OnRollingSavingFinish);

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void InitBounties()
        {
            // Init bounties
            bountiesIdToCoin.Add(new Tuple<int, int>(1, 5));
            bountiesIdToCoin.Add(new Tuple<int, int>(1, 5));
            bountiesIdToCoin.Add(new Tuple<int, int>(1, 5));
            bountiesIdToCoin.Add(new Tuple<int, int>(1, 5));
            bountiesIdToCoin.Add(new Tuple<int, int>(2, 2));
            bountiesIdToCoin.Add(new Tuple<int, int>(2, 3));

            int bountiesCount = bountiesIdToCoin.Count;
            Transform bounties_item = transform.Find("bg/bounties_item");
            for (int i = 0; i < bountiesCount; ++i)
            {
                bounties_item.GetChild(i).gameObject.SetActive(false);
            }

            // Init bounties effect
            Transform bounties_effect = transform.Find("bg/bounties_effect");
            GameObject coin_text = null;
            GameObject effect_parent = null;
            for (int i = 0; i < bountiesCount; ++i)
            {
                Transform child = bounties_effect.GetChild(i);
                if (i == 0)
                {
                    coin_text = child.Find("coin_text").gameObject;
                    effect_parent = child.Find("effect_parent").gameObject;
                }
                else
                {
                    GameObject clonedCoinText = GameObject.Instantiate(coin_text, child);
                    clonedCoinText.name = coin_text.name;
                    clonedCoinText.transform.localPosition = coin_text.transform.localPosition;

                    GameObject clonedEffectParent = GameObject.Instantiate(effect_parent, child);
                    clonedEffectParent.name = effect_parent.name;
                    clonedEffectParent.transform.localPosition = effect_parent.transform.localPosition;
                }
                child.gameObject.SetActive(false);
            }

            // Init bounties selected
            Transform bounties_selected = transform.Find("bg/bounties_selected");
            GameObject sell_text = null;
            GameObject highlight = null;
            for (int i = 0; i < bountiesCount; ++i)
            {
                Transform child = bounties_selected.GetChild(i);
                if (i == 0)
                {
                    sell_text = child.Find("sell_text").gameObject;
                    highlight = child.Find("highlight").gameObject;
                }
                else
                {
                    GameObject clonedSellText = GameObject.Instantiate(sell_text, child);
                    clonedSellText.name = sell_text.name;
                    clonedSellText.transform.localPosition = sell_text.transform.localPosition;

                    GameObject clonedHighlight = GameObject.Instantiate(highlight, child);
                    clonedHighlight.name = highlight.name;
                    clonedHighlight.transform.localPosition = highlight.transform.localPosition;
                }
                child.gameObject.SetActive(false);
            }
        }

        private void InitCaptured()
        {
            // Init captured
            capturedIdToCoin.Add(new Tuple<int, int>(1, 100));
            capturedIdToCoin.Add(new Tuple<int, int>(1, 100));
            capturedIdToCoin.Add(new Tuple<int, int>(1, 100));
            capturedIdToCoin.Add(new Tuple<int, int>(1, 225));

            int capturedCount = capturedIdToCoin.Count;
            Transform captured_item = transform.Find("bg/captured_item");
            for (int i = 0; i < capturedCount; ++i)
            {
                captured_item.GetChild(i).gameObject.SetActive(false);
            }

            // Init captured effect
            Transform captured_effect = transform.Find("bg/captured_effect");
            GameObject coin_text = null;
            GameObject effect_parent = null;
            for (int i = 0; i < capturedCount; ++i)
            {
                Transform child = captured_effect.GetChild(i);
                if (i == 0)
                {
                    coin_text = child.Find("coin_text").gameObject;
                    effect_parent = child.Find("effect_parent").gameObject;
                }
                else
                {
                    GameObject clonedCoinText = GameObject.Instantiate(coin_text, child);
                    clonedCoinText.name = coin_text.name;
                    clonedCoinText.transform.localPosition = coin_text.transform.localPosition;

                    GameObject clonedEffectParent = GameObject.Instantiate(effect_parent, child);
                    clonedEffectParent.name = effect_parent.name;
                    clonedEffectParent.transform.localPosition = effect_parent.transform.localPosition;
                }
                child.gameObject.SetActive(false);
            }


            // Init captured selected
            Transform captured_selected = transform.Find("bg/captured_selected");
            GameObject sell_text = null;
            GameObject highlight = null;
            for (int i = 0; i < capturedCount; ++i)
            {
                Transform child = captured_selected.GetChild(i);
                if (i == 0)
                {
                    sell_text = child.Find("sell_text").gameObject;
                    highlight = child.Find("highlight").gameObject;
                }
                else
                {
                    GameObject clonedSellText = GameObject.Instantiate(sell_text, child);
                    clonedSellText.name = sell_text.name;
                    clonedSellText.transform.localPosition = sell_text.transform.localPosition;
                    GameObject clonedHighlight = GameObject.Instantiate(highlight, child);
                    clonedHighlight.name = highlight.name;
                    clonedHighlight.transform.localPosition = highlight.transform.localPosition;
                }
                child.gameObject.SetActive(false);
            }
        }

        private void OnRollingSavingFinish()
        {
            curState = EffectState.EffectState_rollingInterest1;

            RollNumber rollInterest1 = transform.Find("bg/interest1/num").GetComponent<RollNumber>();
            rollInterest1.SetNum(0);
            rollInterest1.StartRolling(10, 1.0f, OnRollingInterest1Finish);
        }

        private void OnRollingInterest1Finish()
        {
            curState = EffectState.EffectState_rollingInterest2;

            RollNumber rollInterest2 = transform.Find("bg/interest2/num").GetComponent<RollNumber>();
            rollInterest2.SetNum(0);
            rollInterest2.StartRolling(10, 1.0f, OnRollingInterest2Finish);

        }

        private void OnRollingInterest2Finish()
        {
            curState = EffectState.EffectState_rollingBounties;

            RollNumber rollBounties = transform.Find("bg/bounties/num").GetComponent<RollNumber>();
            rollBounties.SetNum(0);

            float delayPerItem = 0.3f;
            bountiesCoroutine1 = StartCoroutine(ShowBountiesItems(delayPerItem));
            bountiesCoroutine2 = StartCoroutine(UIUtils.DelayedAction(bountiesIdToCoin.Count * delayPerItem, () =>
            {
                OnRollingBountiesFinish();
            }));
        }


        // ------------------- Bounties ---------------------

        private void ShowSpecialEffect(bool isBounties, int index)
        {
            string effectName;
            Transform trans;
            if (isBounties)
            {
                effectName = "GoldCoinBlast";
                if (bountiesIdToCoin[index].Item1 == 2)
                    effectName = "LevelupCylinderBlue";
                trans = transform.Find("bg/bounties_effect/" + index + "/effect_parent");
            }
            else
            {
                effectName = "GoldCoinBlast";
                trans = transform.Find("bg/captured_effect/" + index + "/effect_parent");
            }


            GameObject coinEffect = PrefabManager.Instance.GetNewGameObject(effectName, trans);
            StartCoroutine(UIUtils.DelayedAction(2.0f, () =>
            {
                GameObject.Destroy(coinEffect);
            }));
        }

        IEnumerator ShowBountiesItems(float delayPerItem)
        {
            RollNumber rollBounties = transform.Find("bg/bounties/num").GetComponent<RollNumber>();
            Transform bounties_item = transform.Find("bg/bounties_item");
            Transform bounties_effect = transform.Find("bg/bounties_effect");
            for (int i = 0; i < bountiesIdToCoin.Count; i++)
            {
                // Change number
                int num = CalcBountiesCoin(i + 1);
                rollBounties.SetNum(num);
                // Show item
                bounties_item.GetChild(i).gameObject.SetActive(true);
                // Show effect
                Transform child = bounties_effect.GetChild(i);
                child.gameObject.SetActive(true);
                child.Find("coin_text").GetComponent<TextMeshProUGUI>().text = bountiesIdToCoin[i].Item2.ToString();

                string effectName = "GoldCoinBlast";
                if (bountiesIdToCoin[i].Item1 == 2)
                    effectName = "LevelupCylinderBlue";
                ShowSpecialEffect(true, i);

                yield return new WaitForSeconds(delayPerItem);
            }
        }

        private int CalcBountiesCoin(int count)
        {
            int num = 0;
            for (int j = 0; j < count; ++j)
            {
                num += bountiesIdToCoin[j].Item2;
            }
            return num;
        }

        private void OnRollingBountiesFinish()
        {
            curState = EffectState.EffectState_rollingCaptured;

            RollNumber rollCaptured = transform.Find("bg/captured/num").GetComponent<RollNumber>();
            rollCaptured.SetNum(0);
            //rollCaptured.StartRolling(525, 1.0f, OnRollingCapturedFinish);

            float delayPerItem = 0.3f;
            capturedCoroutine1 = StartCoroutine(ShowCapturedItems(delayPerItem));
            capturedCoroutine2 = StartCoroutine(UIUtils.DelayedAction(capturedIdToCoin.Count * delayPerItem, () =>
            {
                OnRollingCapturedFinish();
            }));

        }

        public void OnBountiesBtnClk(GameObject obj)
        {
            int index = int.Parse(obj.name);
            Transform bounties_selected = transform.Find("bg/bounties_selected");
            bool prevSelected = bountiesSelectedList.Contains(index);
            bounties_selected.GetChild(index).gameObject.SetActive(!prevSelected);
            if (prevSelected)
            {
                bountiesSelectedList.Remove(index);
            }
            else
            {
                bountiesSelectedList.Add(index);
                TextMeshProUGUI text = bounties_selected.GetChild(index).Find("sell_text").GetComponent<TextMeshProUGUI>();
                if (bountiesIdToCoin[index].Item1 == 1)
                    text.text = "+10\nmoral";
                else if (bountiesIdToCoin[index].Item1 == 2)
                    text.text = "+20\nequip";

                ShowSpecialEffect(true, index);
            }
            // Update bounties coin
            RollNumber rollBounties = transform.Find("bg/bounties/num").GetComponent<RollNumber>();
            int coin = 0;
            for (int i = 0; i < bountiesIdToCoin.Count; ++i)
            {
                if (!bountiesSelectedList.Contains(i))
                {
                    coin += bountiesIdToCoin[i].Item2;
                }
            }
            rollBounties.SetNum(coin);
        }
        // ------------------- End Bounties ---------------------


        // ---------------- Captured -------------------
        IEnumerator ShowCapturedItems(float delayPerItem)
        {
            RollNumber rollcaptured = transform.Find("bg/captured/num").GetComponent<RollNumber>();
            Transform captured_item = transform.Find("bg/captured_item");
            Transform captured_effect = transform.Find("bg/captured_effect");
            for (int i = 0; i < capturedIdToCoin.Count; i++)
            {
                // Change number
                int num = CalcCapturedCoin(i + 1);
                rollcaptured.SetNum(num);
                // Show item
                captured_item.GetChild(i).gameObject.SetActive(true);
                // Show effect
                Transform child = captured_effect.GetChild(i);
                child.gameObject.SetActive(true);
                child.Find("coin_text").GetComponent<TextMeshProUGUI>().text = capturedIdToCoin[i].Item2.ToString();

                ShowSpecialEffect(false, i);

                yield return new WaitForSeconds(delayPerItem);
            }
        }

        private int CalcCapturedCoin(int count)
        {
            int num = 0;
            for (int j = 0; j < count; ++j)
            {
                num += capturedIdToCoin[j].Item2;
            }
            return num;
        }

        private void OnRollingCapturedFinish()
        {
            curState = EffectState.EffectState_allFinished;
            transform.Find("screen_click").gameObject.SetActive(false);
        }

        public void OnCapturedBtnClk(GameObject obj)
        {
            int index = int.Parse(obj.name);
            Transform captured_selected = transform.Find("bg/captured_selected");
            bool prevSelected = capturedSelectedList.Contains(index);
            captured_selected.GetChild(index).gameObject.SetActive(!prevSelected);
            if (prevSelected)
            {
                capturedSelectedList.Remove(index);
            }
            else
            {
                capturedSelectedList.Add(index);
                TextMeshProUGUI text = captured_selected.GetChild(index).Find("sell_text").GetComponent<TextMeshProUGUI>();
                if (capturedIdToCoin[index].Item1 == 1)
                    text.text = "+10\nmoral";
                else if (capturedIdToCoin[index].Item1 == 2)
                    text.text = "+20\nequip";

                ShowSpecialEffect(false, index);
            }
            // Update captured coin
            RollNumber rollCaptured = transform.Find("bg/captured/num").GetComponent<RollNumber>();
            int coin = 0;
            for (int i = 0; i < capturedIdToCoin.Count; ++i)
            {
                if (!capturedSelectedList.Contains(i))
                {
                    coin += capturedIdToCoin[i].Item2;
                }
            }
            rollCaptured.SetNum(coin);
        }

        // ------------------- End Captured -------------------

        public void OnNextStepBtnClk()
        {
            StartCoroutine(UIUtils.DelayedAction(0, () =>
            {
                Close();
                UIManager.Instance.OpenUI("WinAwardPanel");
            }));
        }

        public void OnScreenClick()
        {
            switch (curState)
            {
                case EffectState.EffectState_rollingSaving:
                    {
                        RollNumber rollSaving = transform.Find("bg/saving/num").GetComponent<RollNumber>();
                        rollSaving.FinishRolling();
                        OnRollingSavingFinish();
                    }
                    break;
                case EffectState.EffectState_rollingInterest1:
                    {
                        RollNumber rollInterest1 = transform.Find("bg/interest1/num").GetComponent<RollNumber>();
                        rollInterest1.FinishRolling();
                        OnRollingInterest1Finish();
                    }
                    break;
                case EffectState.EffectState_rollingInterest2:
                    {
                        RollNumber rollInterest2 = transform.Find("bg/interest2/num").GetComponent<RollNumber>();
                        rollInterest2.FinishRolling();
                        OnRollingInterest2Finish();
                    }
                    break;
                case EffectState.EffectState_rollingBounties:
                    {
                        RollNumber rollBounties = transform.Find("bg/bounties/num").GetComponent<RollNumber>();
                        int num = CalcBountiesCoin(bountiesIdToCoin.Count);
                        rollBounties.SetNum(num);
                        Transform bounties_item = transform.Find("bg/bounties_item");
                        for (int i = 0; i < bountiesIdToCoin.Count; ++i)
                        {
                             bounties_item.GetChild(i).gameObject.SetActive(true);
                        }
                        StopCoroutine(bountiesCoroutine1);
                        StopCoroutine(bountiesCoroutine2);
                        OnRollingBountiesFinish();
                    }
                    break;
                case EffectState.EffectState_rollingCaptured:
                    {
                        RollNumber rollCaptured = transform.Find("bg/captured/num").GetComponent<RollNumber>();
                        int num = CalcCapturedCoin(capturedIdToCoin.Count);
                        rollCaptured.SetNum(num);
                        Transform captured_item = transform.Find("bg/captured_item");
                        for (int i = 0; i < capturedIdToCoin.Count; ++i)
                        {
                            captured_item.GetChild(i).gameObject.SetActive(true);
                        }
                        StopCoroutine(capturedCoroutine1);
                        StopCoroutine(capturedCoroutine2);
                        OnRollingCapturedFinish();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}