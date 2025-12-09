using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonitorScreen_Change : MonoBehaviour
{
    [Header("材质设置")]
    [SerializeField] private Material[] screenMaterials;  // 三种屏幕材质
    [SerializeField] private int currentMaterialIndex = 0;  // 当前材质索引

    [Header("交互设置")]
    [SerializeField] private float interactionDistance = 3f;  // 交互距离
    [SerializeField] private string mouseClickButton = "Fire1";  // 交互按键（默认鼠标左键）

    [Header("屏幕效果")]
    [SerializeField] private Light screenLight;  // 屏幕光源（可选）
    [SerializeField] private float lightIntensity = 1f;  // 屏幕发光强度
    [SerializeField] private Color[] screenColors;  // 屏幕发光颜色（可选）
    [SerializeField] private float clickEffectDuration = 0.2f;  // 点击效果持续时间

    [Header("音效设置")]
    [SerializeField] private AudioClip clickSound;  // 点击音效
    [SerializeField] private AudioClip switchSound;  // 切换材质音效

    private MeshRenderer screenRenderer;
    private AudioSource audioSource;
    private Camera playerCamera;
    private bool isPlayerLooking = false;
    private bool isClickEffectActive = false;
    private float clickEffectTime = 0f;
    private Color originalLightColor;
    private float originalLightIntensity;

    private void Start()
    {
        // 获取屏幕渲染器
        screenRenderer = GetComponent<MeshRenderer>();
        if (screenRenderer == null)
        {
            Debug.LogError($"屏幕 {gameObject.name} 缺少MeshRenderer组件");
            return;
        }

        // 检查材质数组
        if (screenMaterials == null || screenMaterials.Length < 3)
        {
            Debug.LogError($"屏幕 {gameObject.name} 需要至少3种材质");
            return;
        }

        // 查找玩家相机
        playerCamera = Camera.main;

        // 自动查找屏幕光源（子物体）
        if (screenLight == null)
        {
            screenLight = GetComponentInChildren<Light>();
        }

        // 初始化光源
        if (screenLight != null)
        {
            originalLightColor = screenLight.color;
            originalLightIntensity = screenLight.intensity;

            // 如果提供了颜色数组，设置初始颜色
            if (screenColors != null && screenColors.Length > 0)
            {
                UpdateScreenLight();
            }
        }

        // 添加音频源组件
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f;  // 3D音效

        // 设置初始材质
        SetMaterial(currentMaterialIndex);
    }

    private void Update()
    {
        if (playerCamera == null) return;

        // 检测玩家是否看着屏幕
        CheckIfPlayerIsLooking();

        // 检测交互输入
        if (isPlayerLooking && Input.GetButtonDown(mouseClickButton))
        {
            OnScreenClicked();
        }

        // 更新点击效果
        UpdateClickEffect();
    }

    private void CheckIfPlayerIsLooking()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        isPlayerLooking = Physics.Raycast(ray, out hit, interactionDistance) &&
                         hit.collider.gameObject == gameObject;
    }

    private void OnScreenClicked()
    {
        // 播放点击音效
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // 触发点击效果
        StartClickEffect();

        // 切换到下一个材质
        SwitchToNextMaterial();

        Debug.Log($"屏幕被点击，切换到材质 {currentMaterialIndex + 1}/{screenMaterials.Length}");
    }

    private void SwitchToNextMaterial()
    {
        // 增加材质索引
        currentMaterialIndex++;

        // 循环索引（0, 1, 2, 0, 1, 2...）
        if (currentMaterialIndex >= screenMaterials.Length)
        {
            currentMaterialIndex = 0;
        }

        // 设置新材质
        SetMaterial(currentMaterialIndex);

        // 播放切换音效
        if (switchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(switchSound);
        }
    }

    private void SetMaterial(int index)
    {
        if (screenRenderer != null && index >= 0 && index < screenMaterials.Length)
        {
            screenRenderer.material = screenMaterials[index];

            // 更新屏幕光源
            UpdateScreenLight();
        }
    }

    private void UpdateScreenLight()
    {
        if (screenLight != null)
        {
            // 如果提供了颜色数组，根据材质索引设置颜色
            if (screenColors != null && screenColors.Length > 0)
            {
                int colorIndex = currentMaterialIndex % screenColors.Length;
                screenLight.color = screenColors[colorIndex];
            }

            // 设置发光强度
            screenLight.intensity = lightIntensity;
        }
    }

    private void StartClickEffect()
    {
        isClickEffectActive = true;
        clickEffectTime = 0f;

        // 如果屏幕光源存在，短暂增强亮度
        if (screenLight != null)
        {
            screenLight.intensity = lightIntensity * 2f;
        }
    }

    private void UpdateClickEffect()
    {
        if (!isClickEffectActive) return;

        clickEffectTime += Time.deltaTime;

        if (clickEffectTime >= clickEffectDuration)
        {
            // 恢复原始亮度
            if (screenLight != null)
            {
                screenLight.intensity = lightIntensity;
            }

            isClickEffectActive = false;
        }
        else if (screenLight != null)
        {
            // 渐变恢复亮度
            float t = clickEffectTime / clickEffectDuration;
            float intensity = Mathf.Lerp(lightIntensity * 2f, lightIntensity, t);
            screenLight.intensity = intensity;
        }
    }

    // 直接切换到指定材质（可用于事件触发）
    public void SetMaterialByIndex(int index)
    {
        if (index >= 0 && index < screenMaterials.Length)
        {
            currentMaterialIndex = index;
            SetMaterial(currentMaterialIndex);
            Debug.Log($"屏幕切换到材质 {currentMaterialIndex + 1}/{screenMaterials.Length}");
        }
    }

    // 获取当前材质索引
    public int GetCurrentMaterialIndex()
    {
        return currentMaterialIndex;
    }

    // 获取当前材质名称
    public string GetCurrentMaterialName()
    {
        if (screenRenderer != null && screenRenderer.material != null)
        {
            return screenRenderer.material.name;
        }
        return "Unknown";
    }

    // 在Unity编辑器中显示交互距离
    private void OnDrawGizmosSelected()
    {
        // 显示交互距离
        Gizmos.color = Color.cyan;
        Vector3 startPos = transform.position;
        if (playerCamera != null)
        {
            startPos = playerCamera.transform.position;
        }
        else
        {
            startPos = Camera.main != null ? Camera.main.transform.position : transform.position + Vector3.back * 2f;
        }
        Gizmos.DrawLine(startPos, startPos + transform.forward * interactionDistance);

        // 显示屏幕朝向
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f);

        // 显示屏幕范围
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        if (screenRenderer != null)
        {
            Bounds bounds = screenRenderer.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }

    // 编辑器辅助方法：自动设置初始材质
    [ContextMenu("设置初始材质")]
    private void SetInitialMaterialInEditor()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null && screenMaterials != null && screenMaterials.Length > 0)
        {
            renderer.sharedMaterial = screenMaterials[0];
            Debug.Log($"已设置初始材质: {screenMaterials[0].name}");
        }
    }

    // 编辑器辅助方法：创建默认材质数组
    [ContextMenu("创建默认材质")]
    private void CreateDefaultMaterials()
    {
        // 创建三个简单的颜色材质
        screenMaterials = new Material[3];

        // 红色材质
        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = Color.red;
        redMat.name = "Screen_Red";
        screenMaterials[0] = redMat;

        // 绿色材质
        Material greenMat = new Material(Shader.Find("Standard"));
        greenMat.color = Color.green;
        greenMat.name = "Screen_Green";
        screenMaterials[1] = greenMat;

        // 蓝色材质
        Material blueMat = new Material(Shader.Find("Standard"));
        blueMat.color = Color.blue;
        blueMat.name = "Screen_Blue";
        screenMaterials[2] = blueMat;

        Debug.Log("已创建三个默认材质：红、绿、蓝");
    }
}