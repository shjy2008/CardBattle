using System;
using System.Collections;
using Assets.Scripts.data;
using Assets.Scripts.managers.archivemgr;
using Assets.Scripts.managers.inputmgr;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.common
{
    public class UIUtils
    {
        public UIUtils()
        {

        }

        public static bool PositionInTransform(Vector2 position, Transform transform)
        {
            Camera uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
            Vector3 pos = uiCamera.WorldToScreenPoint(transform.position) / Const.GetResolutionRatio();
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            float top = pos.y + rectTransform.rect.height * (1 - rectTransform.pivot.y);
            float right = pos.x + rectTransform.rect.width * (1 - rectTransform.pivot.x);
            float bottom = pos.y - rectTransform.rect.height * rectTransform.pivot.y;
            float left = pos.x - rectTransform.rect.width * rectTransform.pivot.x;

            if (position.x < left || position.x > right ||
                position.y < bottom || position.y > top)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool TouchPosInTransform(Transform transform)
        {
            Camera uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            Vector2 pivot = rectTransform.pivot;
            Vector2 pos = (Vector2)uiCamera.WorldToScreenPoint(transform.position) / Const.GetResolutionRatio() -
                new Vector2(rectTransform.rect.width * pivot.x, rectTransform.rect.height * pivot.y);
            Rect rect = new Rect(pos, rectTransform.rect.size);

            Vector2 touchPos = InputHelper.GetTouchPos() / Const.GetResolutionRatio();
            bool contains = rect.Contains(touchPos);
            return contains;
        }

        public enum LanguageType
        {
            CNS, // chinese standard mandarin
            CNT, // traditional chinese
            EN,  // english
            FR   // french
        }

        public enum FontType
        {
            Title,
            Desc
        };

        public static void SetFontType(TMP_Text text, FontType fontType)
        {
            string fontAssetPath = AssetsMapper.GetFontAsset(fontType, ArchiveManager.Instance.GetCurrentArchiveData().playerData.GetLanguage());
            TMP_FontAsset aaa = Resources.Load<TMP_FontAsset>(fontAssetPath);
            text.font = Resources.Load<TMP_FontAsset>(fontAssetPath);
        }

        public static string GetTextFromLanguageTab(string id)
        {
            var language = ArchiveManager.Instance.GetCurrentArchiveData().playerData.GetLanguage();
            switch (language)
            {
                case LanguageType.CNS: return Table_language.data[id].text_cns;
                case LanguageType.CNT: return Table_language.data[id].text_cnt;
                case LanguageType.EN: return Table_language.data[id].text_en;
                case LanguageType.FR: return Table_language.data[id].text_fr;
            }
            return null;
        }

        public static IEnumerator DelayedAction(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action();
        }

        public static void SetPopPanelPosTopRight(Vector2 topRightPos, GameObject uiObj)
        {
            RectTransform rectTransform = uiObj.transform.GetComponent<RectTransform>();
            float x = rectTransform.rect.width * rectTransform.pivot.x - 10; // Because we set z to -30, x should be closer to the cursor
            float y = rectTransform.rect.height * (1 - rectTransform.pivot.y);
            uiObj.transform.localPosition = new Vector3(topRightPos.x + x, topRightPos.y - y, uiObj.transform.localPosition.z);
        }
    }
}
