using Assets.Scripts.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveArrowConfig
{
    public Vector3 start;
    public Vector3 end;
    public List<Vector2> bendDistList; // if its count is 0, mean it is a straight line. Otherwise bend the curve.
    public bool isLeftToRight;
    public float width;
    public int sampleNum;
    public CurveArrowConfig(Vector3 _start, Vector3 _end, List<Vector2> _bendDistList, bool _isLeftToRight = true, float _width = 1.5f, int _sampleNum = 100)
    {
        start = _start;
        end = _end;
        bendDistList = _bendDistList;
        isLeftToRight = _isLeftToRight;
        width = _width;
        sampleNum = _sampleNum;
    }

    public List<Vector3> GenerateCtrlPoints()
    {
        // ctrl points are in world space, also assume they are on xy plane.
        var ctrlPoints = new List<Vector3> { start };

        if (bendDistList != null)
        {
            // bendDist is a two dist for two direction: from start to end, and its right direction.
            Vector3 U = end - start;
            float maxDist = U.magnitude;
            U = U.normalized;
            Vector3 V;
            if (Mathf.Abs(1.0f - Vector3.Dot(U, Vector3.forward)) <= float.Epsilon)
                V = U.z >= 0 ? Vector3.right : Vector3.left;
            else
                V = Vector3.Cross(U, Vector3.back).normalized;

            for (int i = 0; i < bendDistList.Count; i++)
            {
                Vector3 P = start + maxDist * bendDistList[i].x * U + maxDist * bendDistList[i].y * V;
                ctrlPoints.Add(P);
            }
        }

        ctrlPoints.Add(end);

        return ctrlPoints;
    }
}

public class CurveMeshCreator : MonoBehaviour
{
    // similar logics as "EffectPlayer". But it will sample a lot of points beforehand so as to create the mesh.
    [SerializeField]
    private List<Vector3> ctrlPoints;
    [SerializeField]
    private bool isLeftToRight = true;
    [SerializeField]
    private float width = 1.0f;
    [SerializeField]
    private int sampleNum = 100;

    // Start is called before the first frame update
    void Start()
    {

    }


    public void UpdateCtrlPoints(CurveArrowConfig config)
    {
        ctrlPoints = config.GenerateCtrlPoints();
        isLeftToRight = config.isLeftToRight;
        width = config.width;
        sampleNum = config.sampleNum;
    }

    public Vector3 GetEndPos()
    {
        return ctrlPoints[ctrlPoints.Count - 1];
    }

    // [Note] we have to return a copy of it, otherwise it will be a reference that maybe modified by outside codes.
    public List<Vector3> GetCtronlPoints()
    {
        return new List<Vector3>(ctrlPoints); 
    }

    public void GenerateMesh()
    {
        Debug.Assert(ctrlPoints != null && ctrlPoints.Count > 0);

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        // generate the vertices/uvs along with the Bezier curve.
        float step = 1.0f / sampleNum;
        for (int i = 0; i <= sampleNum; i++)
        {
            float t = i * step;
            Vector3 p = Bezier.DeCasteljau(ctrlPoints, t); // this point is on the central line through the whole shape.
            Vector3 tangent = Bezier.ComputeTangent(ctrlPoints, t);
            // assuming the shape is on the x-y plane. (in world space)
            Vector3 right;
            if (Mathf.Abs(1.0f - Vector3.Dot(tangent, Vector3.back)) <= float.Epsilon)
                right = tangent.z >= 0 ? Vector3.right : Vector3.left;
            else
                right = Vector3.Cross(tangent, Vector3.back).normalized;

            Vector3 lp = p + (-width * 0.5f) * right;
            Vector3 rp = p + (width * 0.5f) * right;
            vertices.Add(lp);
            vertices.Add(rp);
            uvs.Add(new Vector2(isLeftToRight ? 1 : 0, t));
            uvs.Add(new Vector2(isLeftToRight ? 0 : 1, t));
        }

        // build the triangle connectivity
        int pairNum = vertices.Count / 2;
        for(int i = 0; i < pairNum - 1; i++)
        {
            int l1 = i * 2; int r1 = l1 + 1; int l2 = r1 + 1; int r2 = l2 + 1;
            indices.Add(l1); indices.Add(r1); indices.Add(l2);
            indices.Add(r2); indices.Add(l2); indices.Add(r1);
        }

        Mesh curve = new Mesh();
        curve.vertices = vertices.ToArray();
        curve.uv = uvs.ToArray();
        curve.triangles = indices.ToArray();
        curve.RecalculateNormals();
        transform.GetComponent<MeshFilter>().mesh = curve;
    }


#if UNITY_EDITOR
    //public List<Transform> ctrlTransList; //just use it for generating the curve.
    //public bool updateFromTrans = false;
    //private void UpdateCtrlPointsFromTrans()
    //{
    //    ctrlPoints = new List<Vector3>();
    //    for (int i = 0; i < ctrlTransList.Count; i++)
    //        ctrlPoints.Add(ctrlTransList[i].position); // use world position
    //}

    //private void OnValidate()
    //{
    //    if (updateFromTrans)
    //    {
    //        UpdateCtrlPointsFromTrans();
    //        updateFromTrans = false;
    //    }
    //}

    //public bool trigger = false;
    //// Update is called once per frame
    //void Update()
    //{
    //    if(trigger)
    //    {
    //        GenerateMesh();
    //        trigger = false;
    //        LogBendList();
    //    }
    //}

    //private void LogBendList()
    //{
    //    Debug.Log("---------- LogBendList ----------");
    //    Vector3 start = ctrlPoints[0];
    //    Vector3 end = ctrlPoints[ctrlPoints.Count - 1];
    //    if(ctrlPoints.Count>2)
    //    {
    //        Vector3 U = end - start;
    //        float maxDist = U.magnitude;
    //        U = U.normalized;
    //        Vector3 V;
    //        if (Mathf.Abs(1.0f - Vector3.Dot(U, Vector3.forward)) <= float.Epsilon)
    //            V = U.z >= 0 ? Vector3.right : Vector3.left;
    //        else
    //            V = Vector3.Cross(U, Vector3.back).normalized;

    //        for(int i=1; i<=ctrlPoints.Count-2; i++)
    //        {
    //            Vector3 P = ctrlPoints[i];
    //            Vector3 SP = P - start;
    //            float u = Vector3.Dot(SP, U) / maxDist;
    //            float v = Vector3.Dot(SP, V) / maxDist;
    //            Debug.LogFormat("u: {0}, v: {1}", u, v);
    //        }
    //    }
    //    Debug.Log("---------- ---------- ----------");
    //}
#endif
}
