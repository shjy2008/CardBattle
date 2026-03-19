using UnityEngine;
using System.Collections;

namespace Assets.Scripts.panel.BattlePanel
{
    public class BattleCardElementMono : MonoBehaviour
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
    }
}
