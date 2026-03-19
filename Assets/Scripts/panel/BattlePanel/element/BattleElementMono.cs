using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Assets.Scripts.panel.BattlePanel
{
    public class BattleElementMono : MonoBehaviour
    {
        public GameObject highlightObj;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetHighlighted(bool highlighted)
        {
            highlightObj.SetActive(highlighted);
        }

        public void SetElementId(int elementId)
        {
            GetComponent<Image>().sprite = Resources.Load<Sprite>("images/tactic/tactic_element/element_vertical_" + elementId.ToString());
        }
    }
}
