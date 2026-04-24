using Assets.Scripts.data;
using Assets.Scripts.logics;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.utility
{
    namespace Singleton
    {
        #region 单例
        public abstract class TSingleton<T> where T : class
        {
            private static object mutex = new System.Object();
            protected static volatile T m_instance = null;//volatile允许任意线程修改
            public static T Instance
            {
                get
                {
                    if (m_instance == null)
                    {
                        lock (mutex)
                        {
                            if (m_instance == null)//不直接在第一次判断m_instance是否为空的外面lock是为了减少lock操作,在lock里面是为了保证不会重复new
                            {
                                var ctors = typeof(T).GetConstructors(System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Public);//指定搜索公有constructor是为了报异常
                                if (ctors.Length != 1)
                                {
                                    throw new InvalidOperationException(String.Format("Type {0} must have exactly one constructor.", typeof(T)));
                                }
                                if (ctors[0].GetParameters().Length != 0 || !ctors[0].IsPrivate)//构造函数必须是无参且私有的
                                {
                                    throw new InvalidOperationException(String.Format("The constructor for {0} must be private and take no parameters.", typeof(T)));
                                }
                                m_instance = ctors[0].Invoke(null) as T;
                            }
                        }
                    }
                    return m_instance;
                }
            }
        }
        #endregion


    }

    namespace CommonInterface
    {
        public interface IInit
        {
            void Init();

        }

        public interface IUpdate
        {
            void Update();
        }

        public interface IDestroy
        {
            void Destroy();
        }

        public interface ILateUpdate
        {
            void LateUpdate();
        }

        public interface IManager : IInit, IUpdate, IDestroy
        {

        }

        public interface IOnAllManagerInit
        {
            void OnAllManagerInit();
        }

        public interface IBeforeAllManagerDestroy
        {
            void BeforeAllManagerDestroy();
        }
    }

    namespace CommonAbstract
    {

    }

    namespace Common
    {
        public struct CTuple2<T1, T2>
        {
            public T1 arg1;
            public T2 arg2;

            public CTuple2(T1 _arg1, T2 _arg2)
            {
                arg1 = _arg1;
                arg2 = _arg2;
            }

            public void Clear()
            {
                arg1 = default(T1);
                arg2 = default(T2);
            }
        }

        public struct CTuple3<T1, T2, T3>
        {
            public T1 arg1;
            public T2 arg2;
            public T3 arg3;

            public CTuple3(T1 _arg1, T2 _arg2, T3 _arg3)
            {
                arg1 = _arg1;
                arg2 = _arg2;
                arg3 = _arg3;
            }

            public void Clear()
            {
                arg1 = default(T1);
                arg2 = default(T2);
                arg3 = default(T3);
            }
        }

        public struct CTuple4<T1, T2, T3, T4>
        {
            public T1 arg1;
            public T2 arg2;
            public T3 arg3;
            public T4 arg4;

            public CTuple4(T1 _arg1, T2 _arg2, T3 _arg3, T4 _arg4)
            {
                arg1 = _arg1;
                arg2 = _arg2;
                arg3 = _arg3;
                arg4 = _arg4;
            }

            public void Clear()
            {
                arg1 = default(T1);
                arg2 = default(T2);
                arg3 = default(T3);
                arg4 = default(T4);
            }
        }
    }

    // Extend the Unity.Transform class
    public static class TransformExtensions
    {
        // Find a transform with the name recursively over all children, starting from itself.
        public static Transform FindRecursively(this Transform parent, string name)
        {
            if (parent.name == name)
                return parent;
            // Breadth-First Search
            Queue<Transform> que = new Queue<Transform>();
            que.Enqueue(parent);
            Transform current = null;
            while (que.Count > 0)
            {
                current = que.Dequeue();
                if (current.name == name)
                    return current;
                for (int i = 0; i < current.childCount; i++)
                    que.Enqueue(current.GetChild(i));
            }
            return null;
        }

        public static void ResetTransform(this Transform itself)
        {
            itself.localPosition = Vector3.zero;
            itself.localRotation = Quaternion.identity;
            itself.localScale = Vector3.one;
        }

        public static void RemoveAllChildren(this Transform itself, bool immediately = false)
        {
            for(int i = itself.childCount-1; i >= 0; i--)
            {
                if(immediately)
                    GameObject.DestroyImmediate(itself.GetChild(i).gameObject);
                else
                    GameObject.Destroy(itself.GetChild(i).gameObject); ;
            }
        }
    }

    public static class Utils
    {
        private static readonly Dictionary<Tuple<Type, object, Type>, Attribute> enumAttrCache =
            new Dictionary<Tuple<Type, object, Type>, Attribute>();
        private static readonly HashSet<Tuple<Type, object, Type>> enumAttrMissCache =
            new HashSet<Tuple<Type, object, Type>>();
        private static readonly object enumAttrCacheLock = new object();

        public static Dictionary<Renderer, int> GetChildrenRendererToSortingOrder(Transform transform)
        {
            Dictionary<Renderer, int> ret = new Dictionary<Renderer, int>();
            Queue<Transform> que = new Queue<Transform>();
            que.Enqueue(transform);
            Transform current = null;
            while (que.Count > 0)
            {
                current = que.Dequeue();
                Renderer renderer = current.transform.GetComponent<Renderer>();
                if (renderer != null)
                    ret.Add(renderer, renderer.sortingOrder);
                for (int j = 0; j < current.childCount; j++)
                    que.Enqueue(current.GetChild(j));
            }
            return ret;
        }

        public static void AddChildrenSortingOrder(Transform transform, int addSortingOrder)
        {
            Queue<Transform> que = new Queue<Transform>();
            que.Enqueue(transform);
            Transform current = null;
            while (que.Count > 0)
            {
                current = que.Dequeue();
                Renderer renderer = current.transform.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.sortingOrder += addSortingOrder;
                for (int j = 0; j < current.childCount; j++)
                    que.Enqueue(current.GetChild(j));
            }
        }

        public static T ConvertStrToEnum<T>(string input) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), input);
        }

        private static float adjustor = -1;
        public static float GetMatchSceenAdjustor()
        {
            if (adjustor < -0.5f)
            {
                //计算宽高比例  
                float standardAspect = 1280f / 720f;
                float deviceAspect = Screen.width * 1.0f / Screen.height;
                //计算矫正比例  
                if (deviceAspect < standardAspect)
                {
                    adjustor = standardAspect / deviceAspect;
                }

                if (Mathf.Abs(adjustor + 1f) < float.Epsilon)
                {
                    adjustor = 1;
                }
                else
                {
                    adjustor = 0;
                }
            }

            return adjustor;
        }

        #region simple XOR encryption
        public static string EncryptToString(string input, string key)
        {
            return Encoding.UTF8.GetString(EncryptToBytes(input, key));
        }

        public static string DecryptToString(string input, string key)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return DecryptToString(inputBytes, key);
        }

        public static byte[] EncryptToBytes(string input, string key)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var outputBytes = new byte[inputBytes.Length];
            for (int i = 0; i < inputBytes.Length; i++) outputBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
            return outputBytes;
        }

        public static string DecryptToString(byte[] inputBytes, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var outputBytes = new byte[inputBytes.Length];
            for (int i = 0; i < inputBytes.Length; i++) outputBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
            return Encoding.UTF8.GetString(outputBytes);
        }
        #endregion

        // Given a class enum, we try to get the specific CustomAttribute from it.
        public static AttrType GetAttributeFromEnum<AttrType, EnumType>(EnumType input)
            where AttrType : Attribute
            where EnumType : Enum
        {
            var enumType = typeof(EnumType);
            var attrType = typeof(AttrType);
            var cacheKey = Tuple.Create(enumType, (object)input, attrType);

            lock (enumAttrCacheLock)
            {
                if (enumAttrCache.TryGetValue(cacheKey, out var cachedAttr))
                {
                    return cachedAttr as AttrType;
                }

                if (enumAttrMissCache.Contains(cacheKey))
                {
                    return null;
                }
            }

            // Reflection is expensive; do it only once per enum value + attribute type.
            FieldInfo fieldInfo = enumType.GetField(input.ToString());
            AttrType result = fieldInfo != null ? fieldInfo.GetCustomAttribute<AttrType>() : null;

            lock (enumAttrCacheLock)
            {
                if (result != null)
                {
                    enumAttrCache[cacheKey] = result;
                }
                else
                {
                    enumAttrMissCache.Add(cacheKey);
                }
            }

            return result;
        }

        public static bool HasAttribute<AttrType, EnumType>(EnumType input)
            where AttrType : Attribute
            where EnumType : Enum
        {
            return GetAttributeFromEnum<AttrType, EnumType>(input) != null;
        }
    }
}
