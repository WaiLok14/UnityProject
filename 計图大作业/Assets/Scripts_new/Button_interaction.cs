using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInteraction : MonoBehaviour
{
    [Header("呼叫器设置")]
    [SerializeField] private CallDevice callDevice;  // 拖拽呼叫器预制体到这里
    [SerializeField] private float interactionDistance = 3f;  // 交互距离

    [Header("按钮点光源设置")]
    [SerializeField] private Light buttonPointLight;  // 按钮的点光源（子物体）
    [SerializeField] private float buttonLightIntensity = 2f;  // 按钮点光源强度
    [SerializeField] private float buttonLightDuration = 1f;  // 按钮点光源持续时间

    private Camera playerCamera;
    private bool isPlayerLooking = false;
    private bool isButtonLightActive = false;
    private float buttonLightActivationTime = 0f;

    private void Start()
    {
        // 查找玩家相机
        playerCamera = Camera.main;

        // 自动查找按钮子物体中的点光源
        if (buttonPointLight == null)
        {
            buttonPointLight = GetComponentInChildren<Light>();
        }

        // 初始化按钮点光源状态
        if (buttonPointLight != null)
        {
            buttonPointLight.intensity = 0f;  // 初始关闭
            buttonPointLight.color = Color.red;  // 默认白色
        }
        else
        {
            Debug.LogWarning($"按钮 {gameObject.name} 没有找到点光源");
        }
    }

    private void Update()
    {
        if (playerCamera == null) return;

        // 检测玩家是否看着按钮
        CheckIfPlayerIsLooking();

        // 检测交互输入
        if (isPlayerLooking && Input.GetMouseButtonDown(0))
        {
            OnButtonClicked();
        }

        // 更新按钮点光源状态
        UpdateButtonLight();
    }

    private void CheckIfPlayerIsLooking()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        isPlayerLooking = Physics.Raycast(ray, out hit, interactionDistance) &&
                         hit.collider.gameObject == gameObject;
    }

    private void OnButtonClicked()
    {
        Debug.Log("按钮被点击!");

        // 激活按钮点光源
        ActivateButtonLight();

        // 触发呼叫器发光
        if (callDevice != null)
        {
            callDevice.ActivateLight();
        }
    }

    private void ActivateButtonLight()
    {
        isButtonLightActive = true;
        buttonLightActivationTime = Time.time;

        // 设置按钮点光源亮度
        if (buttonPointLight != null)
        {
            buttonPointLight.intensity = buttonLightIntensity;
        }

        Debug.Log($"按钮点光源激活，持续 {buttonLightDuration} 秒");
    }

    private void UpdateButtonLight()
    {
        // 如果按钮点光源激活中，检查是否到达持续时间
        if (isButtonLightActive && Time.time - buttonLightActivationTime >= buttonLightDuration)
        {
            DeactivateButtonLight();
        }
    }

    private void DeactivateButtonLight()
    {
        isButtonLightActive = false;

        // 关闭按钮点光源
        if (buttonPointLight != null)
        {
            buttonPointLight.intensity = 0f;
        }

        Debug.Log("按钮点光源关闭");
    }

    // 在Unity编辑器中显示交互距离和光源范围
    private void OnDrawGizmosSelected()
    {
        // 显示交互距离
        Gizmos.color = Color.blue;
        Vector3 startPos = transform.position;
        if (playerCamera != null)
        {
            startPos = playerCamera.transform.position;
        }
        Gizmos.DrawLine(startPos, startPos + transform.forward * interactionDistance);

        // 显示按钮点光源范围（如果存在且激活）
        if (buttonPointLight != null && buttonPointLight.intensity > 0)
        {
            Gizmos.color = new Color(Color.white.r, Color.white.g, Color.white.b, 0.2f);
            Gizmos.DrawSphere(buttonPointLight.transform.position, buttonPointLight.range);
        }
    }
}