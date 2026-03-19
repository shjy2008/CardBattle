using UnityEngine;
using System.Collections;
using Assets.Scripts.managers.inputmgr;
using System.Collections.Generic;

namespace Assets.Scripts.common
{
    public class HoverShowObj : MonoBehaviour
    {
        public List<GameObject> objList;


        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            bool contains = UIUtils.TouchPosInTransform(transform);
            if (contains)
            {
                foreach (GameObject obj in objList)
                    obj.SetActive(true);
            }
            else
            {
                foreach (GameObject obj in objList)
                    obj.SetActive(false);
            }
        }
    }
}
