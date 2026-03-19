using UnityEngine;
using System.Collections;
using TMPro;

namespace Assets.Scripts.common
{
    public class FadeChildren : MonoBehaviour
    {
        public float fadeDuration = 1.0f; // 渐变消失的持续时间
        public float fadeFromOpacity = 1.0f;
        public float fadeToOpacity = 0.0f;
        private Renderer[] childRenderers; // 保存节点中所有子物体的渲染器组件
        private TextMeshPro[] childTextMeshes; // 保存节点中所有子物体的TextMesh组件
        //private MeshRenderer[] childMeshRenderers;

        void Start()
        {
            // 获取节点中所有子物体的渲染器组件和TextMesh组件
            childRenderers = GetComponentsInChildren<Renderer>();
            childTextMeshes = GetComponentsInChildren<TextMeshPro>();
            //childMeshRenderers = GetComponentsInChildren<MeshRenderer>();

            // 启动渐变消失的协程
            StartCoroutine(Fade());
        }

        public void RemoveRendererFromList(Renderer renderer)
        {
            for (int k = 0; k < childRenderers.Length; k++)
            {
                // 如果找到了匹配的渲染器
                if (childRenderers[k] == renderer)
                {
                    // 移除该渲染器
                    Renderer[] newArray = new Renderer[childRenderers.Length - 1];

                    // 复制原数组的内容，跳过需要移除的渲染器
                    for (int i = 0, j = 0; i < childRenderers.Length; i++)
                    {
                        if (i != k)
                        {
                            newArray[j] = childRenderers[i];
                            j++;
                        }
                    }

                    // 更新 childRenderers 数组
                    childRenderers = newArray;
                    return;
                }
            }

        }

        IEnumerator Fade()
        {
            // 逐渐减小渲染器的透明度，实现渐变消失的效果
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(fadeFromOpacity, fadeToOpacity, timer / fadeDuration);

                // 设置所有子物体的透明度
                foreach (Renderer renderer in childRenderers)
                {
                    if (renderer)
                    {
                        foreach (Material material in renderer.materials)
                        {
                            //// 如果当前Metallic值大于0，则将其设置为0
                            //int metallicPropertyIndex = Shader.PropertyToID("_Metallic");
                            //if (material.HasProperty(metallicPropertyIndex) && material.GetFloat("_Metallic") > 0.0f)
                            //{
                            //    material.SetFloat("_Metallic", 0.0f);
                            //}

                            //// 如果当前Smoothness值大于0，则将其设置为0   
                            //int smoothnessPropertyIndex = Shader.PropertyToID("_Glossiness");
                            //if (material.HasProperty(smoothnessPropertyIndex) && material.GetFloat("_Glossiness") > 0.0f)
                            //{
                            //    material.SetFloat("_Glossiness", 0.0f);
                            //}

                            if (material.HasProperty("_Color"))
                            {
                                Color color = material.color;
                                color.a = alpha;
                                material.color = color;
                            }
                        }
                    }
                }

                // 设置所有TextMesh组件的透明度
                foreach (TextMeshPro textMesh in childTextMeshes)
                {
                    Color color = textMesh.color;
                    color.a = alpha;
                    textMesh.color = color;
                }

                //// 设置MeshRenderer的透明度
                //foreach (MeshRenderer renderer in childMeshRenderers)
                //{
                //    Material material = renderer.material;
                //    Color color = material.color;
                //    color.a = alpha;
                //    material.color = color;
                //}

                yield return null;
            }

            // 渐变完成后禁用所有子物体，确保它们不再渲染
            //foreach (Renderer renderer in childRenderers)
            //{
            //    renderer.gameObject.SetActive(false);
            //}
        }
    }
}