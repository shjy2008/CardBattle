using System;
using UnityEngine;

namespace Assets.Scripts.common
{
    public class Const
    {
        // Debug模式，正式打包设为false
        public const bool debugMode = true;

        // 屏幕设计尺寸
        public const float gameWidth = 1920;
        public const float gameHeight = 1080;

        public static float GetResolutionRatio()
        {
            return Screen.height / gameHeight;
        }
    }
}
