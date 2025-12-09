using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallDevice : MonoBehaviour
{
    [Header("发光设置")]
    [SerializeField] private float lightDuration = 3f;  // 发光持续时间
    [SerializeField] private Color lightColor = Color.green;  // 发光颜色
    [SerializeField] private float lightIntensity = 2f;  // 发光强度

    [Header("组件引用")]
    [SerializeField] private Light pointLight;  // 点光源（子物体）

    [Header("材质设置")]
    [SerializeField] private Material activatedMaterial;  // 激活时的材质
    [SerializeField] private Material deactivatedMaterial;  // 关闭时的材质

    [Header("音频设置")]
    [SerializeField] private AudioClip activationAudio;  // 激活时播放的音频
    [SerializeField] private float audioVolume = 0.5f;  // 音频音量

    private MeshRenderer deviceRenderer;
    private AudioSource audioSource;
    private bool isActive = false;
    private float activationTime = 0f;

    private void Start()
    {
        // 优先查找胶囊体（子物体）的Renderer
        FindCapsuleRenderer();

        // 如果没找到胶囊体，则使用Cube的Renderer
        if (deviceRenderer == null)
        {
            deviceRenderer = GetComponent<MeshRenderer>();
        }

        // 自动查找子物体中的点光源
        if (pointLight == null)
        {
            pointLight = GetComponentInChildren<Light>();
        }

        // 添加音频源组件
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f;  // 3D音效
        audioSource.volume = audioVolume;

        // 设置初始状态
        if (pointLight != null)
        {
            pointLight.intensity = 0f;  // 关闭时亮度为0
            pointLight.color = lightColor;
        }

        // 设置初始材质
        if (deviceRenderer != null && deactivatedMaterial != null)
        {
            deviceRenderer.material = deactivatedMaterial;
        }
    }

    private void FindCapsuleRenderer()
    {
        MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in childRenderers)
        {
            // 通过名称识别胶囊体
            string objName = renderer.gameObject.name.ToLower();
            if (objName.Contains("capsule") || objName.Contains("indicator"))
            {
                deviceRenderer = renderer;
                break;
            }
        }
    }

    private void Update()
    {
        // 如果发光中，检查是否到达持续时间
        if (isActive && Time.time - activationTime >= lightDuration)
        {
            DeactivateLight();
        }
    }

    public void ActivateLight()
    {
        if (isActive) return;  // 已经在发光则不重复触发

        isActive = true;
        activationTime = Time.time;

        // 改变材质
        if (deviceRenderer != null && activatedMaterial != null)
        {
            deviceRenderer.material = activatedMaterial;
        }

        // 设置点光源亮度
        if (pointLight != null)
        {
            pointLight.intensity = lightIntensity;
        }

        // 播放音频
        PlayActivationAudio();

        Debug.Log("呼叫器开始发光，持续 " + lightDuration + " 秒");
    }

    private void PlayActivationAudio()
    {
        if (activationAudio != null && audioSource != null)
        {
            audioSource.clip = activationAudio;
            audioSource.Play();
        }
        else if (activationAudio == null)
        {
            Debug.LogWarning("呼叫器未设置激活音频");
        }
    }

    private void DeactivateLight()
    {
        isActive = false;

        // 恢复材质
        if (deviceRenderer != null && deactivatedMaterial != null)
        {
            deviceRenderer.material = deactivatedMaterial;
        }

        // 设置点光源亮度为0
        if (pointLight != null)
        {
            pointLight.intensity = 0f;
        }

        // 停止音频（如果还在播放）
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Debug.Log("呼叫器停止发光");
    }
}