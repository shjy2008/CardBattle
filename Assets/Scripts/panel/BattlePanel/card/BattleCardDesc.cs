using System;
using System.Collections.Generic;
using Assets.Scripts.logics;
using Assets.Scripts.managers.uimgr;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.panel.BattlePanel
{
    public class BattleCardDesc
    {
        public BattleCardDesc()
        {
        }

        public static string GetDescWithCardData(BattleCardData cardData)
        {
            string ret = "";
            List<string> functions = cardData.actionData.functions;
            for (int i = 0; i < functions.Count; ++i)
            {
                string functionStr = functions[i];
                BattleFunction function = JsonConvert.DeserializeObject<BattleFunction>(functionStr);
                string effectTypeStr = function.effect.ToString();
                Table_card_desc.Data effectData = Table_card_desc.data[effectTypeStr];
                string desc = effectData.Description_en;

                // replace <from> <target> <basicValue>
                string fromStr = Table_card_desc.data[function.from.ToString()].Description_en;
                desc = desc.Replace("<from>", fromStr);

                string targetStr = Table_card_desc.data[function.target.ToString()].Description_en;
                // TODO: test add link
                //targetStr = "<link=\"testLink\">" + targetStr + "</link>";

                desc = desc.Replace("<target>", targetStr);


                // special, modifier如果是正数加加号，浮点数转换成百分比0.2-》20%，即使是2.0，整数不用加%
                // 2.0 -〉 200%
                // 特殊情况：如果规则里有(s)，数字不需要加加号
                string basicValueStr = "";
                Debug.Log("cardId: " + cardData.actionData.id);
                float basicValue = function.EvaluateBasicValue();
                if (cardData.actionData.cost == -1) // x的话，basicValue变成:剩余点数*basicValue
                {
                    GameObject battlePanelObj = UIManager.Instance.GetOpenUI("BattlePanel");
                    if (battlePanelObj)
                    {
                        int restCost = BattlePanel.GetBattleCardComponent().GetBattleCardCost().GetRestCost();
                        basicValue *= restCost;
                    }
                }

                if ((cardData.actionData.actionTypes.Contains("MODIFIER") || cardData.actionData.actionTypes.Contains("SPECIAL")) &&
                !desc.Contains("(s)"))
                {
                    if (basicValue > 0)
                    {
                        basicValueStr += "+";
                    }
                    if (basicValue.GetType() == typeof(int))
                    {
                        basicValueStr += basicValue.ToString();
                    }
                    else
                    {
                        basicValueStr += ((int)basicValue * 100).ToString();
                        basicValueStr += "%";
                    }
                }
                else
                {
                    basicValueStr = basicValue.ToString("0.0");
                }

                desc = desc.Replace("<basicValue>", basicValueStr);

                ret += desc;
                if (i != functions.Count - 1)
                {
                    ret += ".\n";
                }
            }
            return ret;
        }
    }
}
