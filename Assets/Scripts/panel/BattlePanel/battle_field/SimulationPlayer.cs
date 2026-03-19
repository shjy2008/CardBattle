using Assets.Scripts.BattleField;
using Assets.Scripts.common;
using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BattleField
{
    public struct SimulationPlayerConfig
    {
        public Vector2Int rectSize;
        public int cubeNum;
        public float cubeSpacing;
        public float cubeSize;
        public float grayPercentage;

        public void Init(int num, float percentage)
        {
            //rectSize = new Vector2Int(15, 10);
            rectSize = new Vector2Int(8, 5);
            cubeSpacing = 1e-3f;
            //cubeSize = 0.4f;
            cubeSize = 0.75f;

            cubeNum = num;
            grayPercentage = percentage; // cubes grayPercentage: only bottom cubes could have gray, top cubes are all white.
        }
    }

    [System.Serializable]
    public struct ExplosionConfig
    {
        public Vector3 center;
        public float radius;
        public float force;

        public ExplosionConfig(Vector3 _center, float _radius, float _force)
        {
            center = _center;
            radius = _radius;
            force = _force;
        }
    }
}


public class SimulationPlayer : MonoBehaviour
{
    [SerializeField] // using [SerializeField] to make a private variable visible in inspector.
    private Vector2Int rectSize = new Vector2Int(8, 5);

    [SerializeField]
    private int cubeNum = 40;

    [SerializeField]
    private float cubeSpacing = 1e-3f;
    
    [SerializeField]
    private float cubeSize = 0.4f; // will be used to set cube gameobject scale.

    //[SerializeField]
    //private List<ExplosionConfig> explosionConfigs;

    [SerializeField]
    private Dictionary<int, List<ExplosionConfig>> explosionConfigsDict;

    [SerializeField]
    private bool trigger = false;

    [SerializeField]
    private int curFunctionIndex;

    private const float gravity = 0.7f;
    private int grayStartIndex, grayEndIndex;
    private bool isTop = true;
    private string cubePrefabPath = "3d_prefab/explosion_cube";

    public static int maxCubeNum = 40;

    private bool enableSimulation = false;

    public void Init(SimulationPlayerConfig config, bool _isTop, bool _enableSimulation)
    {
        cubeNum = config.cubeNum; 
        cubeSpacing = config.cubeSpacing;
        cubeSize = config.cubeSize;
        explosionConfigsDict = new Dictionary<int, List<ExplosionConfig>>();
        //explosionConfigs = config.explosionConfigs;

        int grayCubeNum = Mathf.Clamp((int)(config.grayPercentage * cubeNum), 0, cubeNum);
        grayStartIndex = Mathf.Max(cubeNum - grayCubeNum, 0);
        grayEndIndex = cubeNum - 1;

        isTop = _isTop;
        enableSimulation = _enableSimulation;

        OnSpawn();
    }

    public void AddExplosionConfig(int functionIndex, ExplosionConfig config)
    {
        if (!explosionConfigsDict.ContainsKey(functionIndex)) explosionConfigsDict.Add(functionIndex, new List<ExplosionConfig>());
        explosionConfigsDict[functionIndex].Add(config);
    }

    public void OnTriggerExplosion(int functionIndex)
    {
        trigger = true;
        curFunctionIndex = functionIndex;
    }

    public void SetAlpha(float alpha)
    {
        for (int i = 0; i < transform.childCount; i++) 
        {
            if(i < cubeNum)
            {
                var cube = transform.GetChild(i);
                var newColor = GetCubeColor(i);
                newColor.a = alpha;
                var renderer = cube.GetComponent<MeshRenderer>();
                var mpb = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(mpb);
                mpb.SetColor("_BaseColor", newColor);
                renderer.SetPropertyBlock(mpb);
            }
        }
    }

    Color GetCubeColor(int index)
    {
        if (grayStartIndex <= index && index <= grayEndIndex) return Color.gray;
        else return Color.white;
    }

    //public void HideAllCubes()
    //{
    //    for(int i = 0; i < transform.childCount; i++)
    //        transform.GetChild(i).gameObject.SetActive(false);
    //}

    public void PreloadCubes()
    {
        // need to create the new cubes
        for (int i = 0; i < maxCubeNum; i++)
        {
            GameObject cube = ResourceManager.Instance.FetchResourceFromPool(cubePrefabPath);
            cube.transform.SetParent(transform);
            cube.transform.ResetTransform();
            cube.SetActive(false);
        }
    }

    public void ReleaseCubes()
    {
        int n = transform.childCount;
        for (int i = 0; i < n; i++)
        {
            GameObject cube = transform.GetChild(0).gameObject;
            ResourceManager.Instance.CacheResourceIntoPool(cubePrefabPath, cube);
        }
        Debug.Assert(transform.childCount <= 0);
    }

    void OnSpawn()
    {
        //if(transform.childCount < cubeNum)
        //{
        //    // need to create the new cubes
        //    int cnt = cubeNum - transform.childCount;
        //    for(int i = 0; i < cnt; i++)
        //    {
        //        GameObject cube = ResourceManager.Instance.FetchResourceFromPool(cubePrefabPath);
        //        cube.transform.SetParent(transform);
        //        cube.transform.ResetTransform();
        //        cube.SetActive(false);
        //    }
        //}
        //else
        //{
        //    // remove the extra cubes
        //    int cnt = transform.childCount - cubeNum;
        //    for (int i = 0; i < cnt; i++)
        //        ResourceManager.Instance.CacheResourceIntoPool(cubePrefabPath, transform.GetChild(0).gameObject);
        //}
        //Debug.Assert(cubeNum == transform.childCount, "cubeNum must be equal to transform.childCount");

        int lastRowNum = cubeNum % rectSize.x;
        int colNum = lastRowNum == 0 ? cubeNum / rectSize.x : cubeNum / rectSize.x + 1;
        float offset = cubeSize + cubeSpacing;
        float signedY = isTop ? 1 : -1;
        int colCenter = Mathf.Max(colNum - 1, 0) / 2;
        for (int i = 0; i < maxCubeNum; i++)
        {
            GameObject cube = transform.GetChild(i).gameObject;

            if (i < cubeNum)
            {
                cube.SetActive(true);
                int rowIdx = i % rectSize.x;
                int colIdx = i / rectSize.x;
                int maxRowNum = ((lastRowNum == 0) || (colIdx < colNum - 1)) ? rectSize.x : lastRowNum;
                float centerIdx = (maxRowNum - 1) * 0.5f;
                float x = (rowIdx - centerIdx) * offset;
                float y = (colIdx - colCenter) * offset * signedY;

                // [Note] must be careful with the localPosition of cube. Do not let it collides with "ground"
                cube.transform.localPosition = new Vector3(x, y, -cubeSize * 0.5f - cubeSpacing);
                cube.transform.localRotation = Quaternion.identity;
                cube.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);

                var rigidBody = cube.GetComponent<Rigidbody>();
                //rigidBody.mass = 0.4f;
                rigidBody.mass = 1f;
                //rigidBody.useGravity = false;
                rigidBody.isKinematic = !enableSimulation; // at first we don't need physic simulation.

                //cube.GetComponent<MeshRenderer>().material.color = GetCubeColor(i);

                var newColor = GetCubeColor(i);
                var renderer = cube.GetComponent<MeshRenderer>();
                var mpb = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(mpb);
                mpb.SetColor("_BaseColor", newColor);
                renderer.SetPropertyBlock(mpb);
            }
            else
            {
                cube.SetActive(false);
            }
        }
    }

    private void OnExplode()
    {
        if(!enableSimulation)
        {
            //Debug.Log("Non top cubes can not explode.");
            return;
        }

        if (explosionConfigsDict == null || explosionConfigsDict.Count <= 0)
        {
            //Debug.Log("ExplosionConfigs is empty.");
            return;
        }

        if(!explosionConfigsDict.ContainsKey(curFunctionIndex))
        {
            return;
        }

        //Debug.Log("OnExplode");
        for (int i = 0; i < transform.childCount; i++)
        {
            var rigidBody = transform.GetChild(i).GetComponent<Rigidbody>();

            //Debug.Log("isKinematic = " + rigidBody.isKinematic);
            var explosionConfigs = explosionConfigsDict[curFunctionIndex];
            for (int j = 0; j < explosionConfigs.Count; j++)
            {
                // [Note] using world space center instead of local space.

                //rigidBody.AddExplosionForce(
                //    explosionConfigs[j].force,
                //    rigidBody.transform.TransformPoint(explosionConfigs[j].center),
                //    explosionConfigs[j].radius,
                //    0, ForceMode.Impulse);

                rigidBody.velocity = Vector3.zero;
                rigidBody.angularVelocity = Vector3.zero;

                rigidBody.AddExplosionForce(
                    explosionConfigs[j].force,
                    explosionConfigs[j].center,
                    explosionConfigs[j].radius,
                    0, ForceMode.Impulse);

                //rigidBody.isKinematic = false; // we need physic simulation at this moment.
                //AddExplosionForce(
                //    rigidBody,
                //    explosionConfigs[j].force,
                //    explosionConfigs[j].center,
                //    explosionConfigs[j].radius);
            }
        }
    }

    void OnEnable()
    {
        //OnSpawn();
    }

    private void OnDisable()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<Rigidbody>().isKinematic = true;
    }

    void OnGravityAtZAxis()
    {
        if (!enableSimulation) return;

        // gravity on Z axis
        for (int i = 0; i < transform.childCount; i++)
        {
            var trans = transform.GetChild(i);
            if (trans.gameObject.activeSelf)
                trans.GetComponent<Rigidbody>().AddForce(Vector3.forward * gravity, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        if (trigger)
        {
            OnExplode();
            trigger = false;
        }

        // gravity on Z axis
        OnGravityAtZAxis();
    }


#if UNITY_EDITOR
    //public bool updateFromTrans = true;
    //public List<Transform> explosionTransList; // only used in debugging, just for quick setting up the center of explosion.

    //[ExecuteInEditMode]
    //void UpdateExplosionCenterFromTransform()
    //{
    //    if (explosionTransList != null && explosionTransList.Count > 0)
    //    {
    //        int maxIndex = Mathf.Min(explosionTransList.Count - 1, explosionConfigs.Count - 1);
    //        for (int i = 0; i <= maxIndex; i++)
    //        {
    //            explosionConfigs[i] = new ExplosionConfig(
    //                explosionTransList[i].position,
    //                explosionConfigs[i].radius,
    //                explosionConfigs[i].force);

    //        }
    //        Debug.Log("Update explosionCenter via Transforms...");
    //    }
    //}

    //private void OnValidate()
    //{
    //    if (updateFromTrans)
    //        UpdateExplosionCenterFromTransform();
    //}
#endif
}
