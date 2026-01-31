using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// 简单资源UI - 只显示获得过的资源
public class SimpleResourceUI : MonoBehaviour
{
    [Header("UI配置")]
    public Transform resourceContainer; // 资源列表容器
    public GameObject resourceItemPrefab; // 资源项预制体
    
    // 已解锁（获得过）的资源
    private HashSet<ResourceType> unlockedResources = new HashSet<ResourceType>();
    
    // UI元素字典
    private Dictionary<ResourceType, GameObject> resourceUIItems = new Dictionary<ResourceType, GameObject>();

    void Start()
    {
        // 订阅资源变化事件
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
        }

        // 初始化已有资源的显示
        InitializeDisplay();
    }

    void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged -= OnResourceChanged;
        }
    }

    /// 初始化显示
    void InitializeDisplay()
    {
        if (ResourceManager.Instance == null) return;

        var allResources = ResourceManager.Instance.GetAllResources();
        foreach (var resource in allResources)
        {
            if (resource.Value > 0)
            {
                UnlockResource(resource.Key);
                UpdateResourceDisplay(resource.Key, resource.Value);
            }
        }
    }

    /// 资源变化回调
    void OnResourceChanged(ResourceChangedEvent evt)
    {
        // 如果是新获得的资源，解锁并创建UI
        if (evt.newQuantity > 0 && !unlockedResources.Contains(evt.resourceType))
        {
            UnlockResource(evt.resourceType);
        }

        // 更新显示
        UpdateResourceDisplay(evt.resourceType, evt.newQuantity);
    }

    /// 解锁资源（创建UI元素）
    void UnlockResource(ResourceType type)
    {
        if (unlockedResources.Contains(type)) return;

        unlockedResources.Add(type);

        // 创建UI项
        if (resourceItemPrefab != null && resourceContainer != null)
        {
            GameObject item = Instantiate(resourceItemPrefab, resourceContainer);
            item.name = type.ToString();
            resourceUIItems[type] = item;

            // 设置资源名称
            TextMeshProUGUI nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = type.ToString();
            }
        }
    }

    /// 更新资源显示
    void UpdateResourceDisplay(ResourceType type, int quantity)
    {
        if (!resourceUIItems.ContainsKey(type)) return;

        GameObject item = resourceUIItems[type];

        // 更新数量文本
        TextMeshProUGUI quantityText = item.transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
        if (quantityText != null)
        {
            quantityText.text = quantity.ToString();
            quantityText.color = quantity > 0 ? Color.blue : Color.gray;
        }

        // 可选：数量为0时变灰
        CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = quantity > 0 ? 1f : 0.5f;
        }
    }
}
