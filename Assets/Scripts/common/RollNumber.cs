using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.common
{
    public class RollNumber : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public string prefix;

        private int fromNum;
        private int toNum;
        private int curNum;
        private float curRollingTime;
        private float totalRollingTime;
        private Action finishCb;

        private bool isRolling = false;

        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (isRolling)
            {
                curRollingTime += Time.deltaTime;
                if (curRollingTime > totalRollingTime)
                {
                    curRollingTime = totalRollingTime;
                    isRolling = false;
                    finishCb?.Invoke();
                }
                curNum = (int)(fromNum + curRollingTime / totalRollingTime * (toNum - fromNum));
                text.text = prefix + curNum.ToString();

            }
        }

        public void SetPrefix(string _prefix)
        {
            prefix = _prefix;
        }

        public void SetNum(int _num)
        {
            isRolling = false;
            curNum = _num;
            text.text = prefix + curNum.ToString();
        }

        public void StartRolling(int _toNum, float _time, Action _finishCb = null)
        {
            isRolling = true;
            curRollingTime = 0.0f;
            totalRollingTime = _time;
            toNum = _toNum;
            finishCb = _finishCb;
        }

        // Finish immediately and set number to the target number
        public void FinishRolling()
        {
            SetNum(toNum);
        }
    }
}
