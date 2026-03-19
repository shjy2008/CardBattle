using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Math;
using Assets.Scripts.BattleField;
using System;
using static UnityEngine.ParticleSystem;
using Assets.Scripts.common;
using Assets.Scripts.managers.timermgr;

namespace Assets.Scripts.BattleField
{
    [SerializeField]
    public struct EffectConfig
    {
        public List<Vector2> points; // Control points on XY plane.
        public float baseHeight;
        public float maxHeight;
        public float heightScale; // parabola in Z axis.
        public float duration; // duration from spawn to destroy
        public Func<float, float> sampleFunc; // for linear it is y=x, for power it is y=x^2
        public Vector3 vfxScales; // scales for (projectile, muzzle, hit)
        public string projectilePrefabPath; // effect spawns at the start point and moves to the end point.
        public string muzzlePrefabPath; // effect spawns at the start point.
        public List<string> hitPrefabPaths; // effect spawns at the end point.
        public Func<float, float> alphaFunc;
    }
}
// This script should be attached to the VFX gameobject.
public class EffectPlayer : MonoBehaviour
{
    // TODO: just a reminder:
    // For stationary objects, we can just pass two close points to continue the general logic
    // The facing direction is also decided by the points.
    // E.g. defense VFX: assets/Resources/vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_15_muzzle

    // bezier curve on X-Y plane
    [SerializeField]
    private float baseHeight = 0f;

    [SerializeField]
    private float maxHeight = 0f; // when heightScale is 0... it means no need Parabola in Z axis. By using baseHeight/maxHeight to do linear change in Z axis.

    [SerializeField]
    private List<Vector2> points; // Control points on XY plane.

    [SerializeField]
    private float heightScale = 0.6f; // parabola in Z axis.

    [SerializeField]
    private float duration = 0.5f; // duration from spawn to destroy

    [SerializeField]
    private float timeScale = 1.0f;

    [SerializeField]
    private Vector3 vfxScales = Vector4.one; // scales for (projectile, muzzle, hit)

    private bool isPlaying = false;
    private float time = 0.0f;
    private List<Vector3> ctrlPoints; // Control points. First one is start point, last one is end point. its z is sampling from Parabola curve.

    #region VFX related
    // the projectile VFX is using +Z direction as default forward direction.
    // below paths should be start with "Assets/Resources/"
    [SerializeField]
    private string projectilePrefabPath; // effect spawns at the start point and moves to the end point.

    [SerializeField]
    private string muzzlePrefabPath; // effect spawns at the start point.

    [SerializeField]
    private List<string> hitPrefabPaths; // effect spawns at the end point.

    private Func<float, float> sampleFunc; // for linear it is y=x, for power it is y=x^2

    private Func<float, float> alphaFunc;

    private Dictionary<Transform, float> alphaRecordDict;

    private Action<GameObject> updateHitPrefabCallBack; // just for hit VFXs function call.

    private GameObject mainGameObj;

    public void SetUpdateHitPrefabCallBack(Action<GameObject> callBack)
    {
        updateHitPrefabCallBack = callBack;
    }

    void InitProjectilePrefab()
    {
        if (!string.IsNullOrEmpty(projectilePrefabPath))
        {
            mainGameObj = ResourceManager.Instance.FetchResourceFromPool(projectilePrefabPath);
            if (mainGameObj != null)
            {
                mainGameObj.transform.SetParent(transform);
                mainGameObj.transform.ResetTransform();
                mainGameObj.transform.localScale = Vector3.one * vfxScales.x;
                InitAlphaRecord(mainGameObj);
            }
        }
    }

    void InitMuzzlePrefab()
    {
        if (!string.IsNullOrEmpty(muzzlePrefabPath))
        {
            var muzzleVFX = ResourceManager.Instance.FetchResourceFromPool(muzzlePrefabPath);
            muzzleVFX.transform.position = transform.position;
            muzzleVFX.transform.rotation = Quaternion.identity;
            muzzleVFX.transform.forward = transform.forward;
            muzzleVFX.transform.localScale = Vector3.one * vfxScales.y;

            var psMuzzle = muzzleVFX.GetComponent<ParticleSystem>();
            float delayTime = psMuzzle != null ? 
                psMuzzle.main.duration :
                muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>().main.duration;

            //if (psMuzzle != null)
            //{
            //    //Destroy(muzzleVFX, psMuzzle.main.duration);
            //}
            //else
            //{
            //    var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
            //    Destroy(muzzleVFX, psChild.main.duration);
            //}

            var gameObj = muzzleVFX;
            var gameObjPath = muzzlePrefabPath;
            GameCore.DelayCall(delayTime, () =>
            {
                ResourceManager.Instance.CacheResourceIntoPool(gameObjPath, gameObj);
            });
        }
    }

    void InitHitPrefab()
    {
        if(hitPrefabPaths != null)
        {
            foreach(var hitPrefabPath in hitPrefabPaths)
            {
                // hit VFX is built by facing UP direction.
                if (!string.IsNullOrEmpty(hitPrefabPath))
                {
                    var hitVFX = ResourceManager.Instance.FetchResourceFromPool(hitPrefabPath);
                    hitVFX.transform.position = transform.position;
                    // transform.forward should be the velocity direction.
                    // [Note] Assuming all hit VFX is using its Vector3.up (0,1,0) as the hit direciton by default.
                    //Quaternion quat = Quaternion.FromToRotation(Vector3.forward, transform.forward);
                    hitVFX.transform.up = -Bezier.ComputeTangent(ctrlPoints, 1.0f);
                    hitVFX.transform.localScale = Vector3.one * vfxScales.z;

                    var psHit = hitVFX.GetComponent<ParticleSystem>();
                    float delayTime = psHit != null ?
                        psHit.main.duration :
                        hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>().main.duration;

                    //var psHit = hitVFX.GetComponent<ParticleSystem>();
                    //if (psHit != null)
                    //{
                    //    Destroy(hitVFX, psHit.main.duration);
                    //}
                    //else
                    //{
                    //    var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                    //    Destroy(hitVFX, psChild.main.duration);
                    //}

                    var gameObj = hitVFX;
                    var gameObjPath = hitPrefabPath;
                    GameCore.DelayCall(delayTime, () =>
                    {
                        ResourceManager.Instance.CacheResourceIntoPool(gameObjPath, gameObj);
                    });

                    if (updateHitPrefabCallBack != null)
                    {
                        updateHitPrefabCallBack(hitVFX);
                        updateHitPrefabCallBack = null;
                    }
                }
            }
        }
    }
    #endregion

    void UpdateTransform(float t)
    {
        if (ctrlPoints == null || ctrlPoints.Count < 2)
            return;
        if (sampleFunc != null) t = sampleFunc(t);
        Vector3 newPos = Bezier.DeCasteljau(ctrlPoints, t);
        //Debug.LogFormat("newPos: ({0}, {1}, {2})", newPos.x, newPos.y, newPos.z);
        transform.position = newPos;

        Vector3 tangent = Bezier.ComputeTangent(ctrlPoints, t);
        transform.forward = tangent;
    }

    void RemoveMainGameObj()
    {
        //transform.RemoveAllChildren();

        if(!string.IsNullOrEmpty(projectilePrefabPath) && mainGameObj != null)
        {
            ResourceManager.Instance.CacheResourceIntoPool(projectilePrefabPath, mainGameObj);
        }

        projectilePrefabPath = null;
        mainGameObj = null;
    }

    void UpdateControlPoints()
    {
        if (points.Count < 2)
            return;
        ctrlPoints = new List<Vector3>();
        if (points.Count == 2)
        {
            // If heightScale != 0, there will be a Parabola in Z axis. Otherwise, it will be a Line.
            // We need to manually sample several points along with the parabola in order to have the intermediate heights.
            Vector2 offsetXY = points[1] - points[0];
            Vector2 offsetDir = offsetXY.normalized; // from start point to end point
            float distance = offsetXY.magnitude;
            // for now just sample 10 points along with parabola
            int sampleNum = 10;
            float step = distance / sampleNum;

            Func<float, float> computeHeight = heightScale != 0 ?
                (float t) =>
                {
                    // start from the baseHeight
                    return baseHeight + Parabola.ComputeHeight(distance * heightScale, t);
                }
            :
                (float t) => { return Mathf.Lerp(baseHeight, maxHeight, t); };

            for (int i = 0; i <= sampleNum; i++)
            {
                Vector2 pXY = points[0] + offsetDir * (step * i);
                float t = Mathf.Clamp((float)i / sampleNum, 0, 1);
                float height = computeHeight(t);
                Vector3 p = new Vector3(pXY.x, pXY.y, height);
                ctrlPoints.Add(p);
            }
        }
        else
        {
            // Bezier curve combining with parabola/line in Z axis.
            Vector2 offsetXY = points[points.Count - 1] - points[0];
            Vector2 offsetDir = offsetXY.normalized; // from start point to end point
            float distance = offsetXY.magnitude;

            Func<float, float> computeHeight = heightScale != 0 ?
                (float t) =>
                {
                    // start from the baseHeight
                    return baseHeight + Parabola.ComputeHeight(distance * heightScale, t);
                }
            :
                (float t) => { return Mathf.Lerp(baseHeight, maxHeight, t); };

            for (int i = 0; i < points.Count; i++)
            {
                float t = Vector2.Dot(points[i] - points[0], offsetDir) / distance; // should be [0, 1]
                t = Mathf.Clamp(t, 0, 1);
                float height = computeHeight(t);
                Vector3 p = new Vector3(points[i].x, points[i].y, height);
                ctrlPoints.Add(p);
            }
        }
    }

    void OnSpawn()
    {
        isPlaying = true;
        time = 0;

        UpdateControlPoints();

        UpdateTransform(0);

        InitProjectilePrefab();
        InitMuzzlePrefab();

        UpdateAlpha(0);
    }

    public void Init(EffectConfig config)
    {
        RemoveMainGameObj(); // remove the prefab used in last time

        points = config.points;
        baseHeight = config.baseHeight;
        maxHeight = config.maxHeight;
        heightScale = config.heightScale;
        duration = config.duration;
        vfxScales = config.vfxScales;
        projectilePrefabPath = config.projectilePrefabPath;
        muzzlePrefabPath = config.muzzlePrefabPath;
        hitPrefabPaths = config.hitPrefabPaths;
        sampleFunc = config.sampleFunc;
        alphaFunc = config.alphaFunc;
        updateHitPrefabCallBack = null;
        alphaRecordDict = null;

        OnSpawn();
    }

    public void ReleaseResources()
    {
        RemoveMainGameObj();
    }

    // Start is called before the first frame update
    void Start()
    {
        //OnSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            time += Time.deltaTime * timeScale;
            if (time > duration)
            {
                time = duration;
                isPlaying = false;
            }

            float t = duration <= 0 ? 0 : time / duration;

            UpdateTransform(t);
            UpdateAlpha(t);

            if (!isPlaying)
            {
                RemoveMainGameObj();
                InitHitPrefab();
                time = 0.0f;
            }
        }
    }

    void InitAlphaRecord(GameObject parent)
    {
        alphaRecordDict = new Dictionary<Transform, float>();
        Queue<Transform> nodes = new Queue<Transform>();
        nodes.Enqueue(parent.transform);
        while (nodes.Count > 0)
        {
            Transform trans = nodes.Dequeue();
            // handle the obj
            ParticleSystem ps = trans.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                alphaRecordDict[trans] = ps.main.startColor.color.a;
            }

            // push all children of obj into queue.
            for (int i = 0; i < trans.childCount; i++)
                nodes.Enqueue(trans.GetChild(i));
        }
    }

    MinMaxGradient GetMinMaxGradient(MinMaxGradient origin, float alpha)
    {
        MinMaxGradient newColor = origin;

        Color color = origin.color;
        color.a = alpha;
        Color colorMin = origin.colorMin;
        colorMin.a = alpha;
        Color colorMax = origin.colorMax;
        colorMax.a = alpha;

        newColor.color = color;
        newColor.colorMin = colorMin;
        newColor.colorMax = colorMax;

        return newColor;
    }

    void UpdateAlpha(float t)
    {
        if (alphaFunc == null || alphaRecordDict == null) return;
        // update particl system start color alpha
        Queue<Transform> nodes = new Queue<Transform>();
        nodes.Enqueue(transform);
        while (nodes.Count > 0)
        {
            Transform trans = nodes.Dequeue();

            // handle the obj
            if (alphaRecordDict.ContainsKey(trans))
            {
                //Debug.Log("Update trans: " + trans.name);
                var ps = trans.GetComponent<ParticleSystem>();
                float alpha = alphaFunc(t) * alphaRecordDict[trans];
                var mainModule = ps.main;
                mainModule.startColor = GetMinMaxGradient(mainModule.startColor, alpha);

                //var mainModule = ps.main;
                //mainModule.startColor = Color.green;
                //ps.main = mainModule;
            }

            // push all children of obj into queue.
            for (int i = 0; i < trans.childCount; i++)
                nodes.Enqueue(trans.GetChild(i));
        }
    }

#if UNITY_EDITOR
    //public bool updateFromTransList = true;
    //public List<Transform> transList; // only used in debugging, just for quick setting up the control points.

    //[ExecuteInEditMode]
    //void UpdatePointsFromTransforms()
    //{
    //    if (transList != null && transList.Count >= 2)
    //    {
    //        points.Clear();
    //        foreach (Transform t in transList)
    //        {
    //            points.Add(new Vector2(t.position.x, t.position.y));
    //        }
    //        baseHeight = transList[0].position.z; // using the first transform's z.
    //        Debug.Log("Update points via Transforms...");
    //    }
    //}

    //private void OnValidate()
    //{
    //    if (updateFromTransList)
    //        UpdatePointsFromTransforms();
    //}
#endif
}
