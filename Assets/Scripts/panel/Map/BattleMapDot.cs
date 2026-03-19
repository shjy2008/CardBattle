using System;
using System.Collections.Generic;
using Assets.Scripts.common;
using Assets.Scripts.data;
using Assets.Scripts.managers.archivemgr;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.BattlePanel;
using Assets.Scripts.utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleMapDotItem : MonoBehaviour, IPointerEnterHandler
{
    public enum BattleMapDotType
    {
        Start,
        End,
        Normal,
        SmallBoss
    }

    public int Row; // start from 0, top right is 0
    public int IndexInRow; // start from 0, top is 0
    public BattleMapDotType DotType;
    public bool IsDeadlyEnemy = false;
    public string CityName;
    public string LandscapeId; // landscape_0001  random 1-6, refer to table landscape.cs
    public int Difficulty;
    public bool IsNextItem = false;

    private BattleMapDot owner;

    public void SetOwner(BattleMapDot _owner) { owner = _owner; }

    void Start()
    {
        // Add transparent Image
        Image img = gameObject.AddComponent<Image>();
        Color color = img.color;
        color.a = 0;
        img.color = color;

        // Add click event listener
        EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => { OnPointerUp(eventData); });

            eventTrigger.triggers.Add(entry);
        }

        // Random city name
        CityName = RandomUtils.ListChoiceOne(CityNames.names);

        // Random landscape
        LandscapeId = string.Format("landscape_{0:0000}", UnityEngine.Random.Range(1, 7)); // 1-6

        // next item, scale action
        if (IsNextItem)
            StartScaleAction();
    }

    void Update()
    {

    }

    public void StartScaleAction()
    {
        Vector3 endScale = transform.localScale * 1.1f;
        Vector3 startScale = transform.localScale * 0.9f;
        transform.localScale = startScale;
        Sequence scaleSequence = DOTween.Sequence()
            .Append(transform.DOScale(endScale, 0.5f)) // Scale up to 1.1 over 1 second
            .Append(transform.DOScale(startScale, 0.5f)) // Scale down to 0.9 over 1 second
            .SetLoops(-1); // Repeat infinitely
        scaleSequence.Play();
    }

    public void OnPointerUp(BaseEventData arg0)
    {
        //PointerEventData peData = arg0 as PointerEventData;
        //// below it is for testing purpose.
        //if (peData != null && peData.button == PointerEventData.InputButton.Right)
        //{
        //    owner.owner.OnSelectDot(new(Row, IndexInRow));
        //    return;
        //}

        // only clicking the point with the current level can enter battle
        if (IsNextItem)
        {
            UIManager.Instance.CloseAndDestroyUI("BattleMap");
            UIManager.Instance.CloseAndDestroyUI("BattleMapDotPopUI");

            BattleManager.Instance.CreateNewBattle(Difficulty);

            UIManager.Instance.OpenUI("BattlePanel");
            BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
            battlePanel.SetLandscapeId(LandscapeId);
            battlePanel.LevelIndexInRow = IndexInRow;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsNextItem)
        {
            //Debug.Log(Row + " " + IndexInRow + " " + DotType + " " + IsDeadlyEnemy);
            if (owner.SelectedItem)
                owner.SelectedItem.SetSelected(false);
            UIManager.Instance.CloseAndDestroyUI("BattleMapDotPopUI");
            GameObject uiObj = UIManager.Instance.OpenUI("BattleMapDotPopUI");

            BattleMapDotPopUI mono = uiObj.GetComponent<BattleMapDotPopUI>();
            mono.SetTitle(CityName);
            mono.SetLandscapeId(LandscapeId);
            mono.SetIsDeadlyEnemy(IsDeadlyEnemy);
            mono.SetTargetDotObj(gameObject);
            mono.SetBattleMapDotItem(this);
            SetSelected(true);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            owner.SelectedItem = this;

            SetSelected(false);
            GameObject circleObj = new GameObject("selected_circle");
            circleObj.transform.SetParent(transform);
            circleObj.transform.localPosition = Vector3.zero;
            SpriteRenderer renderer = circleObj.AddComponent<SpriteRenderer>();

            renderer.sprite = Resources.Load<Sprite>("graphics/UI/ui_map/point_7");
            float scale = 1;
            if (DotType == BattleMapDotType.Normal)
                scale = 0.5f;
            circleObj.transform.localScale = new Vector3(scale, scale);
        }
        else
        {
            Transform circle = transform.Find("selected_circle");
            if (circle)
            {
                GameObject.Destroy(circle.gameObject);
            }
        }
    }

}

public class BattleMapDot
{
    // 0: item0
    // 1: item0, item1, item2, item3
    // 2: item0, item1, item2
    // ...
    // 5: item0, item1
    // 6: item0
    // index 0 is on the top-right circle
    private List<List<BattleMapDotItem>> dotItemListList = new List<List<BattleMapDotItem>>();
    public BattleMapDotItem SelectedItem = null;
    internal BattleMap owner;

    public BattleMapDot(BattleMap _owner)
    {
        owner = _owner;
    }

    public List<List<BattleMapDotItem>> GetDotItemListList()
    {
        return dotItemListList;
    }

    public List<BattleMapDotItem> GetDeadlyEnemyList()
    {
        List<BattleMapDotItem> retList = new List<BattleMapDotItem>();
        foreach (List<BattleMapDotItem> list in dotItemListList)
        {
            foreach (BattleMapDotItem item in list)
            {
                if (item.IsDeadlyEnemy)
                {
                    retList.Add(item);
                }
            }
        }
        return retList;
    }

    public void UpdateCurLevel()
    {
        // connecting lines
        for (int i = 0; i < ArchiveManager.Instance.GetCurrentArchiveData().playerData.MapRowIndexList.Count; ++i)
        {
            owner.OnSelectDot(new Tuple<int, int>(i, ArchiveManager.Instance.GetCurrentArchiveData().playerData.MapRowIndexList[i]));
        }

        int curLayerIndex = ArchiveManager.Instance.GetCurrentArchiveData().playerData.MapRowIndexList.Count - 1;
        BattleMapDotItem curItem = dotItemListList[curLayerIndex][ArchiveManager.Instance.GetCurrentArchiveData().playerData.MapRowIndexList[curLayerIndex]];
        List<BattleMapDotItem> nextItems = owner.GetNextConnectedDotItemList(curItem);
        foreach (BattleMapDotItem item in nextItems)
        {
            item.IsNextItem = true;
        }

        // dots opacity
        for (int i = 0; i < dotItemListList.Count; ++i)
        {
            List<BattleMapDotItem> itemsInRow = dotItemListList[i];
            for (int j = 0; j < itemsInRow.Count; ++j)
            {
                bool showHighlight = false;
                //if (i == ArchiveManager.Instance.GetCurrentArchiveData().playerData.MapRowIndexList.Count)
                //    showHighlight = true;
                if (i < ArchiveManager.Instance.GetCurrentArchiveData().playerData.MapRowIndexList.Count && j == ArchiveManager.Instance.GetCurrentArchiveData().playerData.MapRowIndexList[i])
                    showHighlight = true;
                foreach (BattleMapDotItem nextItem in nextItems)
                {
                    if (i == nextItem.Row && j == nextItem.IndexInRow)
                    {
                        showHighlight = true;
                    }
                }

                if (!showHighlight)
                {
                    BattleMapDotItem item = itemsInRow[j];
                    Color color = item.gameObject.GetComponent<SpriteRenderer>().color;
                    color.a *= 0.2f;
                    item.gameObject.GetComponent<SpriteRenderer>().color = color;
                }
            }
        }
    }

    public void GenerateDots(BattleMap battleMap, List<BattleMap.CircularArc> circularArcList)
    {
        // Reset
        Transform dotRoot = battleMap.transform.Find("dot_root");
        dotRoot.RemoveAllChildren();
        dotItemListList.Clear();

        // Generate

        // start point
        GameObject startObj = new GameObject("start");
        startObj.transform.SetParent(dotRoot);
        startObj.transform.position = battleMap.transform.Find("start").position;
        startObj.transform.localPosition = new Vector3(startObj.transform.localPosition.x, startObj.transform.localPosition.y, 0);
        SpriteRenderer startRenderer = startObj.AddComponent<SpriteRenderer>();
        startRenderer.sprite = Resources.Load<Sprite>("graphics/UI/ui_map/point_6");
        startObj.transform.localScale = new Vector3(0.2f, 0.2f);

        BattleMapDotItem startItem = startObj.AddComponent<BattleMapDotItem>();
        startItem.Row = 0;
        startItem.IndexInRow = 0;
        startItem.DotType = BattleMapDotItem.BattleMapDotType.Start;
        startItem.SetOwner(this);
        dotItemListList.Add(new List<BattleMapDotItem>() { startItem });



        Transform bottomLeftTransform = battleMap.transform.Find("background/bottom_left");
        Transform topRightTransform = battleMap.transform.Find("background/top_right");
        Rect screenRect = new Rect(bottomLeftTransform.position, topRightTransform.position - bottomLeftTransform.position);

        // 是否是多点的行
        bool morePoints = UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f; // more: 4-5, less: 2-3

        // 是否小怪
        //bool[] isSmallBossList = new bool[circularArcList.Count];
        //int nextSmallBossIndex = UnityEngine.Random.Range(0, 3);
        //for (int i = 0; i < circularArcList.Count; ++i)
        //{
        //    if (i == nextSmallBossIndex)
        //    {
        //        isSmallBossList[i] = true;
        //        nextSmallBossIndex = i + UnityEngine.Random.Range(2, 5);// 2-4
        //    }
        //    else
        //    {
        //        isSmallBossList[i] = false;
        //    }
        //}

        string mapId = "map" + ArchiveManager.Instance.GetCurrentArchiveData().playerData.curMapId;
        string mapDifficulty = "difficulty" + ArchiveManager.Instance.GetCurrentArchiveData().playerData.curMapDifficulty;

        Table_level.Data mapData = Table_level.data[mapId];
        string mapDataStr = (string)mapData.GetType().GetField(mapDifficulty).GetValue(mapData);
        mapDataStr = mapDataStr.Trim('[', ']');
        string[] parts = mapDataStr.Split("],");
        string listPart = parts[0].Trim('[', ']');
        string[] monsterDifficultyList = listPart.Split(',');
        int bossDifficulty = int.Parse(parts[1]);

        for (int i = 0; i < circularArcList.Count; ++i)
        {
            BattleMap.CircularArc circularArc = circularArcList[i];

            float midRadius = (circularArc.R + circularArc.r) * 0.5f;
            List<Vector2> intersections = GetCircleAndRectIntersections(screenRect, circularArc.center, midRadius);
            List<float> angles = new List<float>(); // right is 0
            foreach (Vector2 intersection in intersections)
            {
                //GameObject obj = new GameObject("point");
                //obj.transform.SetParent(dotRoot);
                //obj.transform.position = new Vector3(intersection.x, intersection.y);
                //obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
                //SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
                //spriteRenderer.sprite = Resources.Load<Sprite>("graphics/UI/ui_map/point_6");
                //obj.transform.localScale = new Vector3(0.2f, 0.2f);

                float angle = GetSegmentAngle(circularArc.center, intersection);
                angles.Add(angle);
            }
            angles.Sort();
            if (angles.Count > 2) // Only get the two closer to the bottom-left corner
            {
                angles = angles.GetRange(angles.Count - 2, 2);
            }

            // point count in this row
            int count;
            if (morePoints)
            {
                count = UnityEngine.Random.Range(4, 6); // 4-5
            }
            else
            {
                count = UnityEngine.Random.Range(2, 4); // 2-3
            }
            morePoints = !morePoints;

            //int pointImgNum = RandomUtils.ListChoiceOne(new List<int>() { 4, 6, 7 });
            int pointImgNum;
            int monsterDifficulty = int.Parse(monsterDifficultyList[i]);
            monsterDifficulty += UnityEngine.Random.Range(-2, 3); // 每一层随机调整-2到2
            if (monsterDifficulty <= 0)
                monsterDifficulty = 1;
            bool isSmallBoss = monsterDifficulty >= 7;// isSmallBossList[i];
            if (isSmallBoss)
            {
                pointImgNum = 4; // 小boss
            }
            else
            {
                pointImgNum = 9; // 小怪
            }
            string imgPath = "graphics/UI/ui_map/point_" + pointImgNum;

            // 死敌
            int deadlyEnemyIndex = 0;
            if (isSmallBoss)
            {
                deadlyEnemyIndex = UnityEngine.Random.Range(0, count);
            }

            List<BattleMapDotItem> dotItemList = new List<BattleMapDotItem>();
            for (int j = 0; j < count; ++j)
            {
                float angle = angles[0] + (angles[1] - angles[0]) * ((float)(j + 1) / (count + 1));
                Vector2 pos = GetPositionOnCircle(circularArc.center, midRadius, angle);

                // random offset
                float maxOffset = (circularArc.R - circularArc.r) * 0.5f * 0.5f;
                float randomZeroToOneX = UnityEngine.Random.Range(0, 1);
                float offsetMultiplyerX = 1 - randomZeroToOneX * randomZeroToOneX;
                offsetMultiplyerX *= (UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f ? 1 : -1) * maxOffset;

                float randomZeroToOneY = UnityEngine.Random.Range(0, 1);
                float offsetMultiplyerY = 1 - randomZeroToOneY * randomZeroToOneY;
                offsetMultiplyerY *= (UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f ? 1 : -1) * maxOffset;

                pos.x += offsetMultiplyerX;
                pos.y += offsetMultiplyerY;

                // Create GameObject
                GameObject obj = new GameObject("point");
                obj.transform.SetParent(dotRoot);
                obj.transform.position = new Vector3(pos.x, pos.y);
                obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
                SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();

                spriteRenderer.sprite = Resources.Load<Sprite>(imgPath);
                obj.transform.localScale = new Vector3(0.2f, 0.2f);

                // 死敌
                bool isDeadlyEnemy = isSmallBoss && j == deadlyEnemyIndex;
                if (isDeadlyEnemy)
                {
                    GameObject objDeadlyEnemy = new GameObject("pointDeadlyEnemy");
                    objDeadlyEnemy.transform.SetParent(obj.transform);
                    objDeadlyEnemy.transform.localPosition = Vector3.zero;
                    SpriteRenderer spriteRendererDeadlyEnemy = objDeadlyEnemy.AddComponent<SpriteRenderer>();

                    spriteRendererDeadlyEnemy.sprite = Resources.Load<Sprite>("graphics/UI/ui_map/point_8");
                    objDeadlyEnemy.transform.localScale = new Vector3(1, 1);
                }

                // Create dot item
                BattleMapDotItem item = obj.AddComponent<BattleMapDotItem>();
                item.Row = i + 1;
                item.IndexInRow = j;
                if (isSmallBoss)
                    item.DotType = BattleMapDotItem.BattleMapDotType.SmallBoss;
                else
                    item.DotType = BattleMapDotItem.BattleMapDotType.Normal;

                if (isDeadlyEnemy)
                    item.IsDeadlyEnemy = true;
                else
                    item.IsDeadlyEnemy = false;
                item.Difficulty = monsterDifficulty;
                item.SetOwner(this);
                dotItemList.Add(item);
            }
            dotItemListList.Add(dotItemList);
        }

        // end point
        GameObject endObj = new GameObject("end");
        endObj.transform.SetParent(dotRoot);
        endObj.transform.position = battleMap.transform.Find("end").position;
        endObj.transform.localPosition = new Vector3(endObj.transform.localPosition.x, endObj.transform.localPosition.y, 0);
        SpriteRenderer endRenderer = endObj.AddComponent<SpriteRenderer>();
        endRenderer.sprite = Resources.Load<Sprite>("graphics/UI/ui_map/point_6");
        endObj.transform.localScale = new Vector3(0.6f, 0.6f);

        BattleMapDotItem endItem = endObj.AddComponent<BattleMapDotItem>();
        endItem.Row = dotItemListList.Count;
        endItem.IndexInRow = 0;
        endItem.DotType = BattleMapDotItem.BattleMapDotType.End;
        endItem.Difficulty = bossDifficulty;
        endItem.SetOwner(this);
        dotItemListList.Add(new List<BattleMapDotItem>() { endItem });
    }



    // Define the method to calculate intersections between a circle and a rectangle
    private List<Vector2> GetCircleAndRectIntersections(Rect rect, Vector2 center, float radius)
    {
        List<Vector2> intersections = new List<Vector2>();

        // Check each side of the rectangle for intersection with the circle
        CheckSegmentIntersection(rect.position, new Vector2(rect.x + rect.width, rect.y), center, radius, intersections);
        CheckSegmentIntersection(new Vector2(rect.x + rect.width, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), center, radius, intersections);
        CheckSegmentIntersection(new Vector2(rect.x + rect.width, rect.y + rect.height), new Vector2(rect.x, rect.y + rect.height), center, radius, intersections);
        CheckSegmentIntersection(new Vector2(rect.x, rect.y + rect.height), rect.position, center, radius, intersections);

        return intersections;
    }

    // Helper method to check if a segment intersects with the circle and add intersection points to the list
    private void CheckSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 center, float radius, List<Vector2> intersections)
    {
        // Vector from p1 to p2
        Vector2 d = new Vector2(p2.x - p1.x, p2.y - p1.y);

        // Vector from center of circle to p1
        Vector2 f = new Vector2(p1.x - center.x, p1.y - center.y);

        // Calculate coefficients of quadratic equation
        float a = d.x * d.x + d.y * d.y;
        float b = 2 * (f.x * d.x + f.y * d.y);
        float c = f.x * f.x + f.y * f.y - radius * radius;

        // Calculate discriminant
        float discriminant = b * b - 4 * a * c;

        // If discriminant is negative, no intersection
        if (discriminant < 0)
        {
            return;
        }

        // Otherwise, calculate intersection points
        float t1 = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
        float t2 = (-b - Mathf.Sqrt(discriminant)) / (2 * a);

        // Check if intersection points are within the line segment bounds
        if (t1 >= 0 && t1 <= 1)
        {
            Vector2 intersection1 = new Vector2(p1.x + t1 * d.x, p1.y + t1 * d.y);
            intersections.Add(intersection1);
        }

        if (t2 >= 0 && t2 <= 1)
        {
            Vector2 intersection2 = new Vector2(p1.x + t2 * d.x, p1.y + t2 * d.y);
            intersections.Add(intersection2);
        }
    }

    // Method to calculate the angle of a segment AB with respect to the positive x-axis
    private float GetSegmentAngle(Vector2 A, Vector2 B)
    {
        // Calculate the difference in x and y coordinates
        float dx = B.x - A.x;
        float dy = B.y - A.y;

        // Use arctangent function to find the angle
        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

        // Adjust the angle to be between 0 and 360 degrees
        if (angle < 0)
        {
            angle += 360;
        }

        return angle;
    }

    // Method to get position on the circumference of a circle given an angle
    private Vector2 GetPositionOnCircle(Vector2 center, float radius, float angle)
    {
        // Convert angle from degrees to radians
        float angleRad = angle * Mathf.Deg2Rad;

        // Calculate x and y coordinates using trigonometric functions
        float x = center.x + radius * Mathf.Cos(angleRad);
        float y = center.y + radius * Mathf.Sin(angleRad);

        return new Vector2(x, y);
    }
}

