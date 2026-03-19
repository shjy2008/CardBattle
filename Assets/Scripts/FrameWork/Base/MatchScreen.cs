using Assets.Scripts.utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace FrameWork
{
    public class MatchScreen : MonoBehaviour
    {
        void Start()
        {
            gameObject.GetComponent<CanvasScaler>().matchWidthOrHeight = Utils.GetMatchSceenAdjustor();
        }
    }
}
