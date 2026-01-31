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

/// 资源数据 - 存储单个资源的信息
[System.Serializable]
public class ResourceData
{
    public ResourceType resourceType;
    public int quantity = 0;
    [TextArea(2, 3)]
    public string description;
}

/// 配方项 - 配方中的单个资源需求或产出
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

/// 配方数据 - 用于资源转换/合成
[System.Serializable]
public class Recipe
{
    [SerializeField]
    public string recipeName = "New Recipe";
    
    [SerializeField]
    public string description;
    
    /// 需要的输入资源
    [SerializeField]
    public List<RecipeItem> inputs = new List<RecipeItem>();
    
    /// 产出的资源
    [SerializeField]
    public List<RecipeItem> outputs = new List<RecipeItem>();
    
    /// 需要的时间（秒）
    [SerializeField]
    public float processingTime = 1f;
    
    /// 需要的人力资源
    [SerializeField]
    public int requiredLaborForce = 1;
    
    /// 是否已解锁
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

    /// 检查是否满足配方的输入需求
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

    /// 获取配方的调试字符串
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

/// 游戏事件 - 资源变化时触发
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
