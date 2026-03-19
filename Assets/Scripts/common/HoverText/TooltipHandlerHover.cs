using System.Collections.Generic;
using Assets.Scripts.common;
using Assets.Scripts.data;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.utility;
using TMPro;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace ChristinaCreatesGames.Typography.TooltipForTMP
{

    /// <summary>
    /// This class is used to handle the display of tooltips in your Unity project.
    /// It should be placed on the canvas your Tooltip is located on.
    /// </summary>
    public class TooltipHandlerHover : MonoBehaviour
    {
        public List<TooltipInfos> tooltipContentList;

        //[SerializeField] private GameObject tooltipContainer;
        //private TMP_Text _tooltipDescriptionTMP;

        private Camera _uiCamera;

        private void Awake()
        {
            //tooltipContentList = new List<TooltipInfos>();
            //TooltipInfos info1 = new TooltipInfos();
            //info1.Keyword = "keyword";
            //info1.Description = "desc";

            //GameObject obj = UIManager.Instance.OpenUI("DescPanel");
            //_tooltipDescriptionTMP = tooltipContainer.GetComponentInChildren<TMP_Text>();

            _uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
        }

        private void OnEnable()
        {
            gameObject.GetComponent<LinkHandlerForTMPTextHover>().OnHoverOnLinkEvent += GetTooltipInfo;
            gameObject.GetComponent<LinkHandlerForTMPTextHover>().OnCloseTooltipEvent += CloseTooltip;
            //LinkHandlerForTMPTextHover.
            //LinkHandlerForTMPTextHover.OnCloseTooltipEvent += CloseTooltip;
        }

        private void OnDisable()
        {
            gameObject.GetComponent<LinkHandlerForTMPTextHover>().OnHoverOnLinkEvent -= GetTooltipInfo;
            gameObject.GetComponent<LinkHandlerForTMPTextHover>().OnCloseTooltipEvent -= CloseTooltip;
        }

        private void GetTooltipInfo(string keyword, Vector3 mousePos)
        {
            // (1) First try to search the tooltipContentList, if we found a key/descrp, then use it to display
            // "DescPanelSecond" -> same prefab and codes as "DescPanel", but in order to make two of them existing in UIManager,
            // we use a new name for it.
            foreach (var entry in tooltipContentList)
            {
                if (entry.Keyword == keyword)
                {
                    if (UIManager.Instance.GetOpenUI("DescPanelSecond") == null)
                    {
                        DisplayDescPanelSecond(entry.Keyword, entry.Description);
                        return;
                    }
                }
            }

            // (2) if no keyword has been found above, then try to read from table.(common way)
            if (!TryToReadFromTable(keyword)) Debug.Log($"Keyword: {keyword} not found");
        }

        public void CloseTooltip()
        {
            UIManager.Instance.CloseAndDestroyUI("DescPanelSecond");
            //if (tooltipContainer.gameObject.activeInHierarchy)
            //    tooltipContainer.SetActive(false);
        }

        private bool TryToReadFromTable(string id)
        {
            // TODO: wait for Liu Yu to create a table with id, title, description. 
            if(Table_tip_info.data.ContainsKey(id))
            {
                var tipInfoData = Table_tip_info.data[id];
                string title = UIUtils.GetTextFromLanguageTab(tipInfoData.title_id);
                string description = UIUtils.GetTextFromLanguageTab(tipInfoData.description_id);
                DisplayDescPanelSecond(title, description);
                return true;
            }
            return false;
        }

        private void DisplayDescPanelSecond(string title, string description)
        {
            if (UIManager.Instance.GetOpenUI("DescPanelSecond") == null)
            {
                GameObject descPanelObj = UIManager.Instance.OpenUI("DescPanelSecond");
                
                var descPanel = descPanelObj.GetComponent<DescPanel>();
                descPanel.SetTitle(title);
                descPanel.SetDescription(description);

                Vector2 cursorPos = Input.mousePosition / Const.GetResolutionRatio();
                descPanelObj.transform.localPosition = cursorPos + new Vector2(50, 0);
            }
        }
    }
}
