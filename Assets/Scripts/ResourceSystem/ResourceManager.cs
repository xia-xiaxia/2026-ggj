using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 资源管理器 - 单例模式，管理所有游戏资源
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("初始资源配置")]
    public List<ResourceData> initialResources = new List<ResourceData>();

    // 当前资源存储
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    // 事件系统
    public event Action<ResourceChangedEvent> OnResourceChanged;
    public event Action<ResourceType, int> OnResourceAdded;
    public event Action<ResourceType, int> OnResourceRemoved;

    // 日志系统
    private List<string> transactionLog = new List<string>();
    
    [SerializeField]
    private bool enableLogging = true;

    void Awake()
    {
        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeResources();
    }

    /// <summary>
    /// 初始化资源 - 根据initialResources列表设置初始值
    /// </summary>
    void InitializeResources()
    {
        resources.Clear();

        // 初始化所有资源类型
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 0;
        }

        // 应用初始配置
        foreach (var resData in initialResources)
        {
            resources[resData.resourceType] = resData.quantity;
        }

        Log("资源系统已初始化");
        if (enableLogging)
        {
            LogAllResources();
        }
    }

    /// <summary>
    /// 增加资源
    /// </summary>
    public bool AddResource(ResourceType type, int quantity)
    {
        if (quantity < 0)
        {
            Debug.LogWarning($"尝试添加负数资源: {type} {quantity}");
            return false;
        }

        int oldQuantity = resources[type];
        resources[type] += quantity;

        OnResourceAdded?.Invoke(type, quantity);
        OnResourceChanged?.Invoke(new ResourceChangedEvent(type, oldQuantity, resources[type]));

        Log($"添加资源: {type} +{quantity} (来自:{oldQuantity} -> {resources[type]})");
        return true;
    }

    /// <summary>
    /// 消耗资源
    /// </summary>
    public bool RemoveResource(ResourceType type, int quantity)
    {
        if (quantity < 0)
        {
            Debug.LogWarning($"尝试移除负数资源: {type} {quantity}");
            return false;
        }

        if (resources[type] < quantity)
        {
            Debug.LogWarning($"资源不足! {type}: 需要{quantity}, 当前只有{resources[type]}");
            return false;
        }

        int oldQuantity = resources[type];
        resources[type] -= quantity;

        OnResourceRemoved?.Invoke(type, quantity);
        OnResourceChanged?.Invoke(new ResourceChangedEvent(type, oldQuantity, resources[type]));

        Log($"消耗资源: {type} -{quantity} (来自:{oldQuantity} -> {resources[type]})");
        return true;
    }

    /// <summary>
    /// 设置资源数量（直接设置）
    /// </summary>
    public void SetResource(ResourceType type, int quantity)
    {
        if (quantity < 0)
        {
            quantity = 0;
            Debug.LogWarning("资源数量不能为负，已设置为0");
        }

        int oldQuantity = resources[type];
        resources[type] = quantity;

        OnResourceChanged?.Invoke(new ResourceChangedEvent(type, oldQuantity, quantity));
        Log($"设置资源: {type} = {quantity} (来自:{oldQuantity})");
    }

    /// <summary>
    /// 获取资源数量
    /// </summary>
    public int GetResourceQuantity(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }

    /// <summary>
    /// 检查是否有足够的资源
    /// </summary>
    public bool HasEnoughResource(ResourceType type, int quantity)
    {
        return GetResourceQuantity(type) >= quantity;
    }

    /// <summary>
    /// 批量检查资源
    /// </summary>
    public bool HasEnoughResources(Dictionary<ResourceType, int> requiredResources)
    {
        foreach (var resource in requiredResources)
        {
            if (!HasEnoughResource(resource.Key, resource.Value))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 批量添加资源
    /// </summary>
    public bool AddResources(Dictionary<ResourceType, int> resourcesToAdd)
    {
        foreach (var resource in resourcesToAdd)
        {
            if (!AddResource(resource.Key, resource.Value))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 批量消耗资源
    /// </summary>
    public bool RemoveResources(Dictionary<ResourceType, int> resourcesToRemove)
    {
        // 先检查是否都有足够的资源
        if (!HasEnoughResources(resourcesToRemove))
        {
            return false;
        }

        // 然后执行消耗
        foreach (var resource in resourcesToRemove)
        {
            RemoveResource(resource.Key, resource.Value);
        }
        return true;
    }

    /// <summary>
    /// 获取所有资源的副本
    /// </summary>
    public Dictionary<ResourceType, int> GetAllResources()
    {
        return new Dictionary<ResourceType, int>(resources);
    }

    /// <summary>
    /// 重置所有资源到初始状态
    /// </summary>
    public void ResetResources()
    {
        InitializeResources();
    }

    /// <summary>
    /// 清空所有资源
    /// </summary>
    public void ClearAllResources()
    {
        resources.Clear();
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 0;
        }
        Log("所有资源已清空");
    }

    /// <summary>
    /// 输出所有资源信息
    /// </summary>
    public void LogAllResources()
    {
        string output = "=== 当前资源状态 ===\n";
        foreach (var resource in resources)
        {
            output += $"{resource.Key}: {resource.Value}\n";
        }
        Debug.Log(output);
    }

    /// <summary>
    /// 获取交易日志
    /// </summary>
    public List<string> GetTransactionLog()
    {
        return new List<string>(transactionLog);
    }

    /// <summary>
    /// 清空交易日志
    /// </summary>
    public void ClearTransactionLog()
    {
        transactionLog.Clear();
    }

    /// <summary>
    /// 内部日志记录
    /// </summary>
    private void Log(string message)
    {
        if (!enableLogging) return;

        string logEntry = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        transactionLog.Add(logEntry);
        Debug.Log(logEntry);

        // 限制日志大小，防止内存溢出
        if (transactionLog.Count > 1000)
        {
            transactionLog.RemoveRange(0, 100);
        }
    }

    /// <summary>
    /// 导出资源数据为JSON格式
    /// </summary>
    public string ExportToJSON()
    {
        ResourceExportData exportData = new ResourceExportData();
        foreach (var resource in resources)
        {
            exportData.resources.Add(resource.Key.ToString(), resource.Value);
        }
        return JsonUtility.ToJson(exportData);
    }

    /// <summary>
    /// 从JSON导入资源数据
    /// </summary>
    public void ImportFromJSON(string json)
    {
        try
        {
            ResourceExportData importData = JsonUtility.FromJson<ResourceExportData>(json);
            foreach (var resource in importData.resources)
            {
                if (System.Enum.TryParse(resource.Key, out ResourceType type))
                {
                    SetResource(type, resource.Value);
                }
            }
            Log("从JSON成功导入资源数据");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"导入JSON失败: {e.Message}");
        }
    }
}

/// <summary>
/// 导出数据容器
/// </summary>
[System.Serializable]
public class ResourceExportData
{
    public Dictionary<string, int> resources = new Dictionary<string, int>();
}
