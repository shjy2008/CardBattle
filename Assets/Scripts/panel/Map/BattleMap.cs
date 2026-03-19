using Assets.Scripts.managers.archivemgr;
using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.Card;
using Assets.Scripts.utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class BattleMap : BaseUI
{
    public struct CircularArc
    {
        public Vector2 center, up;
        public float R, r; // outward radius, inner radius.

        public CircularArc(Vector2 _center, Vector2 _up, float _R, float _r)
        {
            center = _center;
            up = _up;
            R = _R;
            r = _r;
        }
    }

    private struct DottedArc
    {
        public Vector2 center;
        public float R;
        public DottedArc(Vector2 _center, float _R) { center = _center;R = _R; }
    }

    [SerializeField]
    public int N = 5; // how many circles/intervals

    [SerializeField]
    public float MAX_ANGLE = 180;

    [SerializeField]
    public float T_MAX = 1.1f;

    [SerializeField, Range(0, 1.0f)]
    public float shrinkRatioMin = 0.3f;

    [SerializeField, Range(0, 1.0f)]
    public float shrinkRatioMax = 0.5f;

    [SerializeField, Range(0, 1.0f)]
    public float dottedArcRadiusRatio = 0.5f;

    public Transform startTrans, endTrans;
    public Transform arcRootTrans, curveRootTrans;

    private List<CircularArc> circularArcList;
    private List<DottedArc> dottedArcList;

    private const string
        circularArcPrefabPath = "ui_prefab/panel/map/arc_parent",
        dottedArcPrefabPath = "ui_prefab/panel/map/dotted_arc_parent",
        arrowPrefabPath = "3d_prefab/battlefield/CurveMeshCreator";

    // [Note] if the UI elements changes, the below ratios need to be changed as well.
    // Currently,
    // body is 3575 * 3575
    // mask is 2812 * 2812
    // 2812 to 200 pixels(UI is 1:100, and radius is 1 means the whole size is 2, so it is 200)
    private const int bodySize = 3575;
    private const int maskSize = 2812;
    private const int bodySize2 = 4680;

    private float radiusScale1 = 200f / bodySize; // the prefab scale is "0.05594406f"(200f / 3575) then it means radius = 1f. For the body UI
    private float radiusScale2 = 200f / maskSize; // the prefab scale is "0.07112376"(200f / 2812) then it means radius = 1f. For the mask UI
    private float radiusScale3 = 200f / bodySize2;

    private BattleMapDot battleMapDot;
    private List<List<BattleMapDotItem>> dotItemListList;

    // key is from start dot(layerIndex, index in layer) to end dot (layerIndex, index in layer).
    private Dictionary<Tuple<int, int, int, int>, ValueTuple<CurveArrowConfig, CurveMeshCreator>> curvesDict;

    private HashSet<Tuple<int, int>> selectedDotsRecord;

    private void Init()
    {
        UnityEngine.Random.InitState((int)ArchiveManager.Instance.GetCurrentArchiveData().playerData.curMapRandomSeed);

        circularArcList = new List<CircularArc>();

        // prepare things
        Vector2 startPos = startTrans.position;
        Vector2 endPos = endTrans.position;
        Vector2 U = startPos - endPos; // from endPos to startPos
        float distance = U.magnitude;
        float interval = distance / N;

        U = U / distance;
        // stupid Unity is using left-hand cross product...
        Vector2 V = Vector3.Cross(Vector3.forward, new Vector3(U.x, U.y, 0));
        V = V.normalized;

        //Debug.LogFormat("U: {0}, V: {1}", U, V);

        for (int n = 0; n < N; n++)
        {
            Vector2 start = n == 0 ? startPos : circularArcList[n - 1].center;
            float inner_radius = n == 0 ? Mathf.Max(0.439f, interval * 0.2f) : circularArcList[n - 1].R; // changed the first value according to UI resources.
            //inner_radius += 0.5f; // for adding some spacing
            //Debug.LogFormat("inner_radius: {0}, interval*0.5f: {1}", inner_radius, interval * 0.5f);
            Vector2 end = startPos - (n + 1) * interval * U; // [Note] use 'startPos' instead of 'start'
            float dist_max = Vector2.Dot(start - end, U); // from 'start' to the edge which is parallel to V.
            Debug.Assert(dist_max > 0);

            //Debug.LogFormat("dist_max: {0}, interval: {1}", dist_max, interval);
            var circularArc = GenerateCircularArc(dist_max, inner_radius, ref start, ref U, ref V);
            circularArcList.Add(circularArc);
        }

        // Map dot
        battleMapDot = new BattleMapDot(this);
        battleMapDot.GenerateDots(this, circularArcList);

        // all dots
        dotItemListList = battleMapDot.GetDotItemListList();
        selectedDotsRecord = new HashSet<Tuple<int, int>>() { new(0, 0) }; // By default select the Start Dot automatically.

        // dotted arc
        dottedArcList = new List<DottedArc>();

        // Boss point needs a dotted arc
        var dottedArc = GenerateDottedArc(distance, ref endPos, ref U, ref V);
        dottedArcList.Add(dottedArc);

        var deadlyDotItemList = battleMapDot.GetDeadlyEnemyList();
        foreach (var deadlyDotItem in deadlyDotItemList)
        {
            Vector2 pos = deadlyDotItem.transform.position;
            dottedArc = GenerateDottedArc(distance, ref pos, ref U, ref V);
            dottedArcList.Add(dottedArc);
        }
    }

    private CircularArc GenerateCircularArc(float dist_max, float inner_radius, ref Vector2 start, ref Vector2 U, ref Vector2 V)
    {
        float RAD = Mathf.PI / 180f * MAX_ANGLE; // 
        //const float RAD = -Mathf.PI * 0.5f;
        float rad = UnityEngine.Random.Range(0, 2 * RAD) - RAD;
        float u = Mathf.Cos(rad);
        float v = Mathf.Sin(rad);
        Vector2 dir = u * U + v * V;
        dir = dir.normalized;

        Debug.Assert(dist_max > inner_radius);

        const float minThicknessRatio = 0.5f;
        float t_max = (1 - u) == 0 ? T_MAX : (dist_max - inner_radius) / (1 - u);
        t_max = Mathf.Min(t_max, T_MAX); // set a upper bound

        float t_min = t_max * (1.0f - minThicknessRatio);

        Debug.Assert(t_min <= t_max);
        // once the angle is determined, the max thickness (R_max-R_min) is also determine
        float t = UnityEngine.Random.Range(t_min, t_max);
        //t = 0; // for testing

        float R = dist_max + t * u; // R_max
        float r = t + inner_radius; // R_min

        //Debug.LogFormat("t_min: {0}, t_max: {1}, t: {2}", t_min, t_max, t);
        //Debug.LogFormat("u: {0}, v: {1}", u, v);
        //Debug.LogFormat("dist_max: {0}, inner_radius: {1}, t: {2}", dist_max, inner_radius, t);
        //Debug.LogFormat("R: {0}, r: {1}, thickness: {2}", R, r, R - r);

        // shrink the R, r
        float middle = (R + r) * 0.5f;
        Debug.Assert(shrinkRatioMin <= shrinkRatioMax);
        float shrinkRatio = UnityEngine.Random.Range(shrinkRatioMin, shrinkRatioMax);
        float thickness = (R - r) * shrinkRatio;
        R = middle + thickness * 0.5f;
        r = R - thickness;

        Debug.Assert(r <= R);

        Vector2 center = start + t * dir;

        //Debug.LogFormat("center: {0}, R: {1}, r: {2}", center, R, r);

        Vector2 up = -U;
        return new CircularArc(center, up, R, r);
    }

    private void UpdateUI()
    {
        arcRootTrans.RemoveAllChildren();
        float zInWorld = arcRootTrans.TransformPoint(Vector3.zero).z;

        int orderInLayer = circularArcList.Count - 1;
        foreach(var data in circularArcList)
        {
            GameObject circularArcObj = ResourceManager.Instance.LoadResourceAsGameObject(circularArcPrefabPath);

            circularArcObj.transform.SetParent(arcRootTrans);
            circularArcObj.transform.position = new Vector3(data.center.x, data.center.y, zInWorld);

            var bodyTrans = circularArcObj.transform.Find("body");
            float scale = data.R * radiusScale1;
            bodyTrans.localScale = new Vector3(scale, scale, scale);
            bodyTrans.up = data.up;
            var spRenderer = bodyTrans.GetComponent<SpriteRenderer>();
            spRenderer.sortingOrder = orderInLayer;
            var color = spRenderer.color;
            color.a = UnityEngine.Random.Range(10, 20) / 255f;
            spRenderer.color = color;

            var maskTrans = circularArcObj.transform.Find("mask");
            scale = data.r * radiusScale2;
            maskTrans.localScale = new Vector3(scale, scale, scale);
            maskTrans.up = data.up;
            var spMask = maskTrans.GetComponent<SpriteMask>();
            spMask.frontSortingOrder = orderInLayer;
            spMask.backSortingOrder = orderInLayer - 1;

            orderInLayer--;
        }

        foreach(var data in dottedArcList)
        {
            GameObject dottedArcObj = ResourceManager.Instance.LoadResourceAsGameObject(dottedArcPrefabPath);
            dottedArcObj.transform.SetParent(arcRootTrans);
            dottedArcObj.transform.position = new Vector3(data.center.x, data.center.y, zInWorld);

            var bodyTrans = dottedArcObj.transform.Find("body");
            float scale = data.R * radiusScale3;
            bodyTrans.localScale = new Vector3(scale, scale, scale);
            var spRenderer = bodyTrans.GetComponent<SpriteRenderer>();
            spRenderer.sortingOrder = circularArcList.Count;
        }

        ConnectAllDots();

        battleMapDot.UpdateCurLevel();
    }

    private DottedArc GenerateDottedArc(float distance, ref Vector2 start, ref Vector2 U, ref Vector2 V)
    {
        float RAD = Mathf.PI / 180f * 45f;
        float rad = UnityEngine.Random.Range(0, 2 * RAD) - RAD;
        float u = Mathf.Cos(rad);
        float v = Mathf.Sin(rad);
        Vector2 dir = (u * U + v * V).normalized;
        float radius = distance * dottedArcRadiusRatio;
        return new DottedArc(start + radius * dir, radius);
    }

    private ValueTuple<CurveArrowConfig, CurveMeshCreator> ConnectDot(Vector3 start, Vector3 end, Vector2 uv)
    {
        // from start dot to end dot.
        List<Vector2> bendDistList = new List<Vector2>() { uv };
        //float width = UnityEngine.Random.Range(0.03f, 0.05f);
        float width = 0.03f;
        CurveArrowConfig config = new CurveArrowConfig(start, end, bendDistList, uv.y > 0, width, 50);
        GameObject arrowGameObj = ResourceManager.Instance.LoadResourceAsGameObject(arrowPrefabPath);

        arrowGameObj.transform.SetParent(curveRootTrans);
        arrowGameObj.transform.ResetTransform();

        var curveArrow = arrowGameObj.GetComponent<CurveMeshCreator>();
        curveArrow.UpdateCtrlPoints(config);
        curveArrow.GenerateMesh();

        var meshRenderer = arrowGameObj.GetComponent<MeshRenderer>();
        var mpb = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_AlphaScale", 0.2f);
        meshRenderer.SetPropertyBlock(mpb);


        return new(config, curveArrow);
    }

    private bool HasSelectDotInLayer(int layer)
    {
        for (int index = 0; index < dotItemListList[layer].Count; index++)
        {
            Tuple<int, int> tempIndex = new(layer, index);
            if (selectedDotsRecord.Contains(tempIndex)) return true;
        }
        return false;
    }

    public void OnSelectDot(Tuple<int, int> dotIndex)
    {
        //Debug.LogFormat("OnSelectDot: {0}", dotIndex);
        if (selectedDotsRecord.Contains(dotIndex)) return;

        int curLayer = dotIndex.Item1;
        if (HasSelectDotInLayer(curLayer)) return; // if already select one dot in current layer

        int preLayer = curLayer - 1;

        if (preLayer >= 0 && !HasSelectDotInLayer(preLayer)) return; // if not select any dot in previous layer.

        Tuple<int, int> start = new(-1, -1), end = dotIndex;

        // find start
        for (int index = 0; index < dotItemListList[preLayer].Count; index++)
        {
            var dotKey = new Tuple<int, int>(preLayer, index);
            if (selectedDotsRecord.Contains(dotKey))
            {
                start = dotKey;
                break;
            }
        }

        Debug.Assert(start.Item1 != -1 && start.Item2 != -1);

        var curveKey = new Tuple<int, int, int, int>(start.Item1, start.Item2, end.Item1, end.Item2);

        if (!curvesDict.ContainsKey(curveKey)) return;

        // handle select dot, update the curve accordingly.
        selectedDotsRecord.Add(dotIndex);

        var curveData = curvesDict[new(start.Item1, start.Item2, end.Item1, end.Item2)];
        var config = curveData.Item1;
        config.width = 0.05f;

        var curveArrow = curveData.Item2;
        curveArrow.UpdateCtrlPoints(config);
        curveArrow.GenerateMesh();

        var meshRenderer = curveArrow.gameObject.GetComponent<MeshRenderer>();
        var mpb = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_AlphaScale", 1f);
        meshRenderer.SetPropertyBlock(mpb);
    }

    private List<float> GenVList(Vector3 start, List<BattleMapDotItem> dotItemList)
    {
        // connect point to all dots inside dotItemList
        // generate the uvs for all lines (start, dotItem1), (start, dotItem2) ... 
        // make sure there is no intersection.
        List<float> vList = new List<float>();

        Vector3 prevPoint, nextPoint;
        Vector3 curPoint;
        float D_MAX = (startTrans.localPosition - endTrans.localPosition).magnitude;
        float sign = UnityEngine.Random.Range(0f, 1f) <= 0.5f ? -1f : 1f;
        for (int i = 0; i < dotItemList.Count; i++)
        {
            curPoint = dotItemList[i].transform.localPosition;
            float D = (curPoint - start).magnitude;
            float v_max = D / D_MAX * 0.5f;

            if (i > 0)
            {
                prevPoint = dotItemList[i - 1].transform.localPosition;
                Vector3 U = (prevPoint - start).normalized;
                float dist = Vector3.Cross(curPoint - start, U).magnitude;
                v_max = Mathf.Min(v_max, dist * 0.5f / D);
            }
            
            if(i < dotItemList.Count - 1)
            {
                nextPoint = dotItemList[i + 1].transform.localPosition;
                Vector3 U = (nextPoint - start).normalized;
                float dist = Vector3.Cross(curPoint - start, U).magnitude;
                v_max = Mathf.Min(v_max, dist * 0.5f / D);
            }

            float v = UnityEngine.Random.Range(0f, v_max) * sign;
            vList.Add(v);
        }

        return vList;
    }

    private void ConnectAllDots()
    {
        curvesDict = new Dictionary<Tuple<int, int, int, int>, (CurveArrowConfig, CurveMeshCreator)>();
        curveRootTrans.RemoveAllChildren(); 

        int layerMAX = dotItemListList.Count - 1;
        var connectivityDict = new Dictionary<Tuple<int, int>, HashSet<Tuple<int, int>>>(); // for each start dot, store all its connected end dots.
        for (int curLayer = 0; curLayer < layerMAX; curLayer++)
        {
            int dotNumInCurLayer = dotItemListList[curLayer].Count;
            int nextLayer = curLayer + 1;
            int dotNumInNextLayer = dotItemListList[nextLayer].Count;
            int lastIndexInNextLayer = -1;

            if(dotNumInCurLayer == 1)
            {
                // if curLayer has only one dot, connect all dots in next layer
                var startKey = new Tuple<int, int>(curLayer, 0);
                for (int index = 0; index < dotItemListList[nextLayer].Count; index++)
                {
                    var endKey = new Tuple<int, int>(nextLayer, index);
                    if (!connectivityDict.ContainsKey(startKey)) connectivityDict.Add(startKey, new HashSet<Tuple<int, int>>());
                    connectivityDict[startKey].Add(endKey);
                }
            }
            else if (dotNumInNextLayer == 1)
            {
                // if nextLayer has only one dot, from all dots in current layer to connect it
                var endKey = new Tuple<int, int>(nextLayer, 0);
                for (int index = 0; index < dotItemListList[curLayer].Count; index++)
                {
                    var startKey = new Tuple<int, int>(curLayer, index);
                    if (!connectivityDict.ContainsKey(startKey)) connectivityDict.Add(startKey, new HashSet<Tuple<int, int>>());
                    connectivityDict[startKey].Add(endKey);
                }
            }
            else
            {
                // for those dots in the intermediate layer:
                // each dot will first check its previous dot in the same layer, to get the last connected dot in next layer from its previous dot
                // then starting with this last connected dot, randomly pick [1, 3] dots to connected(continuous dots) in next layer.
                // then move to next dot in the current layer.
                for (int index = 0; index < dotItemListList[curLayer].Count; index++)
                {
                    var startKey = new Tuple<int, int>(curLayer, index);
                    if (!connectivityDict.ContainsKey(startKey)) connectivityDict.Add(startKey, new HashSet<Tuple<int, int>>());

                    int connectNum = 1 + UnityEngine.Random.Range(0, 3); // 1, 2 or 3 dots
                    while (connectNum > 0)
                    {
                        connectNum--;
                        lastIndexInNextLayer++;
                        lastIndexInNextLayer = Mathf.Min(dotNumInNextLayer - 1, lastIndexInNextLayer);

                        var endKey = new Tuple<int, int>(nextLayer, lastIndexInNextLayer);
                        if (!connectivityDict[startKey].Contains(endKey)) connectivityDict[startKey].Add(endKey);

                        if (lastIndexInNextLayer >= dotNumInNextLayer - 1) break;
                    }
                }
            }
        }

        foreach (var connectivity in connectivityDict)
        {
            var startDotItem = dotItemListList[connectivity.Key.Item1][connectivity.Key.Item2];
            Vector3 start = startDotItem.transform.localPosition;

            List<BattleMapDotItem> endDotItemlist = new List<BattleMapDotItem>();
            foreach(var endKey in connectivity.Value) endDotItemlist.Add(dotItemListList[endKey.Item1][endKey.Item2]);
            var vList = GenVList(start, endDotItemlist);

            for(int i=0; i<endDotItemlist.Count; i++)
            {
                Vector2 uv = new Vector2(0.5f, vList[i]);
                var dotItem = endDotItemlist[i];
                var curveData = ConnectDot(start, dotItem.transform.localPosition, uv);
                curvesDict.Add(new(connectivity.Key.Item1, connectivity.Key.Item2,
                    dotItem.Row, dotItem.IndexInRow), curveData);
            }
        }
    }

    public List<BattleMapDotItem> GetNextConnectedDotItemList(BattleMapDotItem item)
    {
        var result = new List<BattleMapDotItem>();
        int layerIndex = item.Row;
        int index = item.IndexInRow;
        foreach(var curve in curvesDict)
        {
            if(curve.Key.Item1 == layerIndex && curve.Key.Item2 == index)
            {
                result.Add(dotItemListList[curve.Key.Item3][curve.Key.Item4]);
            }
        }
        return result;
    }    

    [SerializeField]
    private bool trigger = false;

    // Update is called once per frame
    override protected void Update()
    {
        if(trigger)
        {
            Init();
            UpdateUI();
            trigger = false;
        }
    }

    public void SetTrigger() { trigger = true; }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (shrinkRatioMin > shrinkRatioMax) shrinkRatioMax = shrinkRatioMin;
    }
#endif

    public void OnBackgroundClick()
    {
        if (battleMapDot.SelectedItem)
            battleMapDot.SelectedItem.SetSelected(false);
        UIManager.Instance.CloseAndDestroyUI("BattleMapDotPopUI");
    }

    public void OnCardBtnClk()
    {
        GameObject obj = UIManager.Instance.OpenUI("CardPanel");
        obj.GetComponent<CardPanel>().Init(false);
    }

    public void OnArmyManageBtnClk()
    {
        UIManager.Instance.OpenUI("ArmyManagePanel");
    }
}
