using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FrameWork
{
    public class ScrollListView : MonoBehaviour
    {
        [SerializeField]
        private RectTransform content;
        [SerializeField]
        private int maxItemCount = 20;
        private bool isVertical = true;

        protected Dictionary<int, RectTransform> cellList;
        protected List<RectTransform> reUseCellList;
        protected List<Vector2> lengthList; //记录长度位置

        protected int count = 0;
        private ScrollRect scrollRect;
        private Vector2 viewSize;
        public bool IsReuser = true;

        private ListViewDelegate ViewDelegate;

        private void Awake()
        {
            if(content == null)
            {
                content = transform.Find("Viewport/Content").GetComponent<RectTransform>();
            }
            cellList = new Dictionary<int, RectTransform>(maxItemCount);
            reUseCellList = new List<RectTransform>(5);
            lengthList = new List<Vector2>(maxItemCount);
            viewSize = gameObject.GetComponent<RectTransform>().sizeDelta;
            scrollRect = transform.GetComponent<ScrollRect>();
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            isVertical = scrollRect.vertical;
        }

        private void Start()
        {
            
        }

        public void ReloadData()
        {
            Clear();
            InitData();
            UpdateListView();
        }

        public Vector2 GetContentSize()
        {
            return content.sizeDelta;
        }

        public bool IsVertical
        {
            get { return isVertical; }
        }


        public void SetViewDelegate(ListViewDelegate listViewDelegate)
        {
            ViewDelegate = listViewDelegate;
        }


        protected Vector2 GetCellSizeByIndex(int index)
        {
            if (ViewDelegate != null)
                return ViewDelegate.CellSizeByIndex(index);

            return Vector2.zero;
        }

        protected void InitCellCount()
        {
            if (ViewDelegate != null)
                this.count = ViewDelegate.CellCount();
        }

        protected RectTransform CellAtIndex(RectTransform cell, int index)
        {
            if (ViewDelegate != null)
                return ViewDelegate.CellAtIndex(cell, index);

            return cell;
        }

        private void InitData()
        {
            InitCellCount();
            Vector2 contentSize = Vector2.zero;
            if (lengthList == null)
                lengthList = new List<Vector2>(maxItemCount);

            for (int i = 0; i < count; ++i)
            {
                var cellSize = GetCellSizeByIndex(i);
                if (isVertical)
                {
                    lengthList.Add(Vector2.right * contentSize.y);
                    contentSize.y += cellSize.y;
                    lengthList[i] += Vector2.up * contentSize.y;
                }
                else
                {
                    lengthList.Add(Vector2.right * contentSize.x);
                    contentSize.x += cellSize.x;
                    lengthList[i] -= Vector2.up * contentSize.x;
                }
            }

            content.sizeDelta = contentSize;
        }

        private void OnScrollValueChanged(Vector2 offset)
        {
            UpdateListView();
        }

        private void UpdateListView()
        {
            float startPos = content.localPosition.y;
            float endPos = startPos + viewSize.y;
            if (!isVertical)
            {
                startPos = content.localPosition.x;
                endPos = startPos - viewSize.x;
            }

            int startIndex = GetListIndexByPos(startPos);
            int endIndex = GetListIndexByPos(endPos);

            List<int> delList = new List<int>(maxItemCount);

            if (IsReuser)
            {
                foreach (KeyValuePair<int, RectTransform> pair in cellList)
                {
                    if (pair.Key < startIndex || pair.Key > endIndex)//回收超出可见范围的子项对象
                    {
                        reUseCellList.Add(pair.Value);
                        delList.Add(pair.Key);
                        pair.Value.gameObject.SetActive(false);
                    }
                }

                for (int index = 0; index < delList.Count; index++)
                {
                    cellList.Remove(delList[index]);
                }
            }

            if(ViewDelegate != null)
            {
                ViewDelegate.StartUpdate();
            }

            for (int i = startIndex; i <= endIndex; ++i)
            {
                if (cellList.ContainsKey(i) || i > lengthList.Count-1)
                {
                    continue;
                }

                RectTransform reuseCell = null;

                if (reUseCellList.Count > 0)
                {
                    reuseCell = reUseCellList[0];
                    reUseCellList.RemoveAt(0);
                }

                var cell = CellAtIndex(reuseCell, i);

                if (cell == null)
                {
                    continue;
                }

                cell.SetParent(content, false);
                cell.gameObject.SetActive(true);
                cellList.Add(i, cell);

                if (isVertical)
                    cell.anchoredPosition = Vector2.down * lengthList[i].x;
                else
                    cell.anchoredPosition = Vector2.right * lengthList[i].x;
            }
        }

        private int GetListIndexByPos(float pos)
        {
            int index = 0;
            if (lengthList.Count <= 0)
                return 0;

            if (isVertical) // 垂直滑动往上 contern滚动Y增加
            {
                if (pos > lengthList[lengthList.Count - 1].y)
                    index = lengthList.Count - 1;
                else if (pos > lengthList[0].x)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        if (pos >= lengthList[i].x && pos <= lengthList[i].y)
                        {
                            index = i;
                            break;
                        }
                    }
                }
            }
            else // 水平滑动往左 contern滚动X减少
            {
                if (pos < lengthList[lengthList.Count - 1].y)
                    index = lengthList.Count - 1;
                else if (pos < lengthList[0].x)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        if (pos <= lengthList[i].x && pos >= lengthList[i].y)
                        {
                            index = i;
                            break;
                        }
                    }
                }
            }

            return index;
        }

        private void Clear()
        {
            foreach (KeyValuePair<int, RectTransform> pair in cellList)
            {
                GameObject.Destroy(pair.Value.gameObject);
            }

            for(int i = 0; i < reUseCellList.Count; ++i)
            {
                Destroy(reUseCellList[i].gameObject);
            }

            reUseCellList.Clear();
            cellList.Clear();
            lengthList.Clear();
        }
    }
}
