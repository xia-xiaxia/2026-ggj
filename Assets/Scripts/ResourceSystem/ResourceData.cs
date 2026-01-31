using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 资源类型枚举
/// </summary>
public enum ResourceType
{
    // 人力资源
    people,
    mask,
    
    // 工业原料
    acid,
    volcOre,
    ignRock,
    lakeMud,
    iron,
    copper,
    alum,
    silica,
    
    // 工业半成品
    protectFabric,
    ModularEngineeringMaterials,
    
    // 工业成品
    Mask1,
    Mask2,
    Mask3,
    
    // 其他
    biomass,
    WasteElectro,
    richElectro,
    Electricity,
    solarMod,
    engMod
}

/// <summary>
/// 资源数据 - 存储单个资源的信息
/// </summary>
[System.Serializable]
public class ResourceData
{
    public ResourceType resourceType;
    public int quantity = 0;
    [TextArea(2, 3)]
    public string description;
}

/// <summary>
/// 配方项 - 配方中的单个资源需求或产出
/// </summary>
[System.Serializable]
public class RecipeItem
{
    public ResourceType resourceType;
    public int quantity;
    
    public RecipeItem(ResourceType type, int qty)
    {
        resourceType = type;
        quantity = qty;
    }
}

/// <summary>
/// 配方数据 - 用于资源转换/合成
/// </summary>
[System.Serializable]
public class Recipe
{
    [SerializeField]
    public string recipeName = "New Recipe";
    
    [SerializeField]
    public string description;
    
    /// <summary>
    /// 需要的输入资源
    /// </summary>
    [SerializeField]
    public List<RecipeItem> inputs = new List<RecipeItem>();
    
    /// <summary>
    /// 产出的资源
    /// </summary>
    [SerializeField]
    public List<RecipeItem> outputs = new List<RecipeItem>();
    
    /// <summary>
    /// 需要的时间（秒）
    /// </summary>
    [SerializeField]
    public float processingTime = 1f;
    
    /// <summary>
    /// 需要的人力资源
    /// </summary>
    [SerializeField]
    public int requiredLaborForce = 1;
    
    /// <summary>
    /// 是否已解锁
    /// </summary>
    [SerializeField]
    public bool isUnlocked = true;

    public Recipe()
    {
        inputs = new List<RecipeItem>();
        outputs = new List<RecipeItem>();
    }

    public Recipe(string name, float time, int labor)
    {
        recipeName = name;
        processingTime = time;
        requiredLaborForce = labor;
        inputs = new List<RecipeItem>();
        outputs = new List<RecipeItem>();
    }

    /// <summary>
    /// 检查是否满足配方的输入需求
    /// </summary>
    public bool CanExecute(Dictionary<ResourceType, int> availableResources)
    {
        foreach (var input in inputs)
        {
            if (!availableResources.ContainsKey(input.resourceType) || 
                availableResources[input.resourceType] < input.quantity)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 获取配方的调试字符串
    /// </summary>
    public string GetDebugInfo()
    {
        string info = $"Recipe: {recipeName}\n";
        info += $"Processing Time: {processingTime}s, Labor Required: {requiredLaborForce}\n";
        info += "Inputs:\n";
        foreach (var input in inputs)
        {
            info += $"  - {input.resourceType}: {input.quantity}\n";
        }
        info += "Outputs:\n";
        foreach (var output in outputs)
        {
            info += $"  - {output.resourceType}: {output.quantity}\n";
        }
        return info;
    }
}

/// <summary>
/// 游戏事件 - 资源变化时触发
/// </summary>
public class ResourceChangedEvent
{
    public ResourceType resourceType;
    public int oldQuantity;
    public int newQuantity;
    public int change;
    
    public ResourceChangedEvent(ResourceType type, int old, int newVal)
    {
        resourceType = type;
        oldQuantity = old;
        newQuantity = newVal;
        change = newVal - old;
    }
}
