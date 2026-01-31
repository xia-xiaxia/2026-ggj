using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 工厂类型枚举
/// </summary>
public enum FactoryType
{
    hydroThermal,  // 水热发电厂
    dryRock,       // 干热岩电厂
    pvFarm,        // 光伏发电厂
    acidTower,     // 酸素捕集塔
    lakeTreat,     // 湖水处理厂
    mudDigger,     // 泥底泥挖厂
    oreMine,       // 矿物采掘场
    thermoFarm,    // 喷热蓄热农场
    cuFac,         // 铜精炼厂
    feFac,         // 铁回收厂
    alFac,         // 铝提取厂
    mulFac,        // 联合冶炼厂
    bioFac,        // 有机化工厂
    matFac,        // 建筑材料厂
    pvFac,         // 光伏制造厂
    mask1Fac       // Mask-1工厂
}

/// <summary>
/// 工厂配置数据
/// </summary>
[System.Serializable]
public class FactoryConfig
{
    public FactoryType factoryType;
    public string factoryName;
    public string description;
    
    /// <summary>
    /// 建筑条件 - 建造所需的资源
    /// </summary>
    public Dictionary<ResourceType, int> buildingRequirements = new Dictionary<ResourceType, int>();
    
    /// <summary>
    /// 生产周期（秒）
    /// </summary>
    public float productionTime = 1f;
    
    /// <summary>
    /// 是否已建造
    /// </summary>
    [SerializeField]
    public bool isBuilt = false;
    
    /// <summary>
    /// 生产配方
    /// </summary>
    public Recipe productionRecipe;

    public FactoryConfig(FactoryType type, string name)
    {
        factoryType = type;
        factoryName = name;
        buildingRequirements = new Dictionary<ResourceType, int>();
    }

    public string GetDebugInfo()
    {
        string info = $"Factory: {factoryName} ({factoryType})\n";
        info += $"Built: {isBuilt}\n";
        info += "Building Requirements:\n";
        foreach (var req in buildingRequirements)
        {
            info += $"  - {req.Key}: {req.Value}\n";
        }
        return info;
    }
}

/// <summary>
/// 工厂实例
/// </summary>
[System.Serializable]
public class FactoryInstance
{
    public int factoryID;
    public FactoryConfig config;
    public bool isOperating = false;
    public float operationProgress = 0f;
    
    public FactoryInstance(int id, FactoryConfig cfg)
    {
        factoryID = id;
        config = cfg;
    }

    public string GetStatus()
    {
        if (!config.isBuilt)
            return "未建造";
        if (isOperating)
            return $"生产中 ({operationProgress * 100:F1}%)";
        return "就绪";
    }
}

/// <summary>
/// 工厂管理系统
/// </summary>
public class FactorySystem : MonoBehaviour
{
    public static FactorySystem Instance { get; private set; }

    // 工厂配置库
    private Dictionary<FactoryType, FactoryConfig> factoryConfigs = new Dictionary<FactoryType, FactoryConfig>();
    
    // 建造的工厂实例
    private List<FactoryInstance> builtFactories = new List<FactoryInstance>();
    
    // 当前运行的工厂
    private Dictionary<FactoryInstance, float> runningFactories = new Dictionary<FactoryInstance, float>();

    // 事件系统
    public event Action<FactoryInstance> OnFactoryBuilt;
    public event Action<FactoryInstance> OnFactoryOperationStarted;
    public event Action<FactoryInstance> OnFactoryOperationCompleted;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFactories();
    }

    void Update()
    {
        UpdateRunningFactories();
    }

    /// <summary>
    /// 初始化所有工厂配置
    /// </summary>
    void InitializeFactories()
    {
        factoryConfigs.Clear();
        
        // 1. 水热发电厂
        CreateFactory(FactoryType.hydroThermal, "水热发电厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 }, { ResourceType.copper, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, 12 } },
            5f);

        // 2. 干热岩电厂
        CreateFactory(FactoryType.dryRock, "干热岩电厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 4 }, { ResourceType.alum, 3 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, 32 } },
            5f);

        // 3. 光伏发电厂
        CreateFactory(FactoryType.pvFarm, "光伏发电厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 }, { ResourceType.solarMod, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, 20 } },
            5f);

        // 4. 酸素捕集塔
        CreateFactory(FactoryType.acidTower, "酸素捕集塔",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 1 }, { ResourceType.iron, 1 } },
            new Dictionary<ResourceType, int> { { ResourceType.acid, 2 } },
            3f);

        // 5. 湖水处理厂
        CreateFactory(FactoryType.lakeTreat, "湖水处理厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, -2 }, { ResourceType.acid, 2 }, { ResourceType.lakeMud, 1 } },
            4f);

        // 6. 泥底泥挖厂
        CreateFactory(FactoryType.mudDigger, "泥底泥挖厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, -2 }, { ResourceType.lakeMud, 2 } },
            4f);

        // 7. 矿物采掘场
        CreateFactory(FactoryType.oreMine, "矿物采掘场",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 3 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, -2 }, { ResourceType.volcOre, 1 }, { ResourceType.ignRock, 2 } },
            5f);

        // 8. 喷热蓄热农场
        CreateFactory(FactoryType.thermoFarm, "喷热蓄热农场",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, -2 }, { ResourceType.acid, -1 }, { ResourceType.biomass, 1 } },
            4f);

        // 9. 铜精炼厂
        CreateFactory(FactoryType.cuFac, "铜精炼厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, -3 }, { ResourceType.volcOre, -1 }, { ResourceType.acid, -1 }, { ResourceType.copper, 2 }, { ResourceType.WasteElectro, 1 } },
            6f);

        // 10. 铁回收厂
        CreateFactory(FactoryType.feFac, "铁回收厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, -3 }, { ResourceType.WasteElectro, -1 }, { ResourceType.iron, 2 }, { ResourceType.richElectro, 1 } },
            6f);

        // 11. 铝提取厂
        CreateFactory(FactoryType.alFac, "铝提取厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, -1 }, { ResourceType.ignRock, -1 }, { ResourceType.acid, -2 }, { ResourceType.alum, 1 }, { ResourceType.silica, 1 } },
            5f);

        // 12. 联合冶炼厂
        CreateFactory(FactoryType.mulFac, "联合冶炼厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, -2 }, { ResourceType.lakeMud, -1 }, { ResourceType.copper, 1 }, { ResourceType.iron, 1 }, { ResourceType.silica, 1 }, { ResourceType.alum, 1 } },
            6f);

        // 13. 有机化工厂
        CreateFactory(FactoryType.bioFac, "有机化工厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, -1 }, { ResourceType.biomass, -1 }, { ResourceType.protectFabric, 1 } },
            4f);

        // 14. 建筑材料厂
        CreateFactory(FactoryType.matFac, "建筑材料厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 1 }, { ResourceType.silica, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, -1 }, { ResourceType.silica, -2 }, { ResourceType.iron, -2 }, { ResourceType.engMod, 6 } },
            7f);

        // 15. 光伏制造厂
        CreateFactory(FactoryType.pvFac, "光伏制造厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.Electricity, -3 }, { ResourceType.richElectro, -1 }, { ResourceType.alum, -1 }, { ResourceType.iron, -1 }, { ResourceType.solarMod, 2 } },
            6f);

        // 16. Mask-1工厂
        CreateFactory(FactoryType.mask1Fac, "Mask-1工厂",
            new Dictionary<ResourceType, int> { { ResourceType.engMod, 2 } },
            new Dictionary<ResourceType, int> { { ResourceType.protectFabric, -1 }, { ResourceType.iron, -1 }, { ResourceType.Mask1, 8 } },
            5f);

        Debug.Log($"已初始化 {factoryConfigs.Count} 个工厂配置");
    }

    /// <summary>
    /// 创建工厂配置
    /// </summary>
    void CreateFactory(FactoryType type, string name, Dictionary<ResourceType, int> buildReq, Dictionary<ResourceType, int> production, float time)
    {
        FactoryConfig config = new FactoryConfig(type, name);
        config.buildingRequirements = buildReq;
        config.productionTime = time;

        // 创建生产配方
        config.productionRecipe = new Recipe(name, time, 0);
        foreach (var prod in production)
        {
            config.productionRecipe.outputs.Add(new RecipeItem(prod.Key, Mathf.Abs(prod.Value)));
            if (prod.Value < 0)
            {
                config.productionRecipe.inputs.Add(new RecipeItem(prod.Key, Mathf.Abs(prod.Value)));
            }
        }

        factoryConfigs[type] = config;
    }

    /// <summary>
    /// 建造工厂
    /// </summary>
    public bool BuildFactory(FactoryType type)
    {
        if (!factoryConfigs.ContainsKey(type))
        {
            Debug.LogWarning($"工厂类型 {type} 不存在");
            return false;
        }

        FactoryConfig config = factoryConfigs[type];

        if (config.isBuilt)
        {
            Debug.LogWarning($"工厂 {config.factoryName} 已经建造过了");
            return false;
        }

        // 检查资源
        if (!ResourceManager.Instance.HasEnoughResources(config.buildingRequirements))
        {
            Debug.LogWarning($"资源不足，无法建造 {config.factoryName}");
            return false;
        }

        // 消耗资源
        if (!ResourceManager.Instance.RemoveResources(config.buildingRequirements))
        {
            return false;
        }

        // 标记为已建造
        config.isBuilt = true;

        // 创建工厂实例
        FactoryInstance instance = new FactoryInstance(builtFactories.Count, config);
        builtFactories.Add(instance);

        OnFactoryBuilt?.Invoke(instance);
        Debug.Log($"工厂已建造: {config.factoryName}");

        return true;
    }

    /// <summary>
    /// 启动工厂生产
    /// </summary>
    public bool StartFactoryOperation(FactoryType type)
    {
        // 找到对应的工厂实例
        FactoryInstance instance = builtFactories.Find(f => f.config.factoryType == type);
        
        if (instance == null)
        {
            Debug.LogWarning($"工厂 {type} 未建造");
            return false;
        }

        return StartFactoryOperation(instance);
    }

    /// <summary>
    /// 启动工厂生产（指定实例）
    /// </summary>
    public bool StartFactoryOperation(FactoryInstance instance)
    {
        if (instance == null || !instance.config.isBuilt)
        {
            Debug.LogWarning("工厂未建造");
            return false;
        }

        if (instance.isOperating)
        {
            Debug.LogWarning($"工厂 {instance.config.factoryName} 已在运行");
            return false;
        }

        // 检查输入资源
        Dictionary<ResourceType, int> inputs = new Dictionary<ResourceType, int>();
        foreach (var input in instance.config.productionRecipe.inputs)
        {
            inputs[input.resourceType] = input.quantity;
        }

        if (!ResourceManager.Instance.HasEnoughResources(inputs))
        {
            Debug.LogWarning($"资源不足，无法启动 {instance.config.factoryName}");
            return false;
        }

        // 消耗输入资源
        if (!ResourceManager.Instance.RemoveResources(inputs))
        {
            return false;
        }

        instance.isOperating = true;
        instance.operationProgress = 0f;
        runningFactories[instance] = 0f;

        OnFactoryOperationStarted?.Invoke(instance);
        Debug.Log($"工厂开始生产: {instance.config.factoryName}");

        return true;
    }

    /// <summary>
    /// 更新运行中的工厂
    /// </summary>
    void UpdateRunningFactories()
    {
        List<FactoryInstance> completedFactories = new List<FactoryInstance>();

        foreach (var factory in runningFactories)
        {
            factory.Key.operationProgress += Time.deltaTime / factory.Key.config.productionTime;

            if (factory.Key.operationProgress >= 1f)
            {
                completedFactories.Add(factory.Key);
            }
        }

        // 完成的工厂
        foreach (var factory in completedFactories)
        {
            CompleteFactoryOperation(factory);
            runningFactories.Remove(factory);
        }
    }

    /// <summary>
    /// 完成工厂生产
    /// </summary>
    void CompleteFactoryOperation(FactoryInstance instance)
    {
        if (instance == null) return;

        // 添加输出资源
        Dictionary<ResourceType, int> outputs = new Dictionary<ResourceType, int>();
        foreach (var output in instance.config.productionRecipe.outputs)
        {
            outputs[output.resourceType] = output.quantity;
        }

        ResourceManager.Instance.AddResources(outputs);

        instance.isOperating = false;
        instance.operationProgress = 0f;

        OnFactoryOperationCompleted?.Invoke(instance);
        Debug.Log($"工厂完成生产: {instance.config.factoryName}");
    }

    /// <summary>
    /// 获取所有已建造的工厂
    /// </summary>
    public List<FactoryInstance> GetBuiltFactories()
    {
        return new List<FactoryInstance>(builtFactories);
    }

    /// <summary>
    /// 获取工厂配置
    /// </summary>
    public FactoryConfig GetFactoryConfig(FactoryType type)
    {
        return factoryConfigs.ContainsKey(type) ? factoryConfigs[type] : null;
    }

    /// <summary>
    /// 获取所有工厂配置
    /// </summary>
    public Dictionary<FactoryType, FactoryConfig> GetAllFactoryConfigs()
    {
        return new Dictionary<FactoryType, FactoryConfig>(factoryConfigs);
    }

    /// <summary>
    /// 查询工厂建造状态
    /// </summary>
    public bool IsFactoryBuilt(FactoryType type)
    {
        return factoryConfigs.ContainsKey(type) && factoryConfigs[type].isBuilt;
    }

    /// <summary>
    /// 获取工厂运行状态
    /// </summary>
    public string GetFactoryStatus(FactoryType type)
    {
        FactoryInstance instance = builtFactories.Find(f => f.config.factoryType == type);
        return instance != null ? instance.GetStatus() : "不存在";
    }
}
