using UnityEngine;
using System.Collections.Generic;

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
/// 资源成本 - 表示某种资源的数量
/// </summary>
[System.Serializable]
public class ResourceCost
{
    public ResourceType resourceType;
    public int quantity;

    public ResourceCost() { }

    public ResourceCost(ResourceType type, int amount)
    {
        resourceType = type;
        quantity = amount;
    }
}

/// <summary>
/// 生产配方 - 定义输入和输出资源
/// </summary>
[System.Serializable]
public class ProductionRecipe
{
    public string recipeName = "默认配方";
    public List<ResourceCost> inputs = new List<ResourceCost>();
    public List<ResourceCost> outputs = new List<ResourceCost>();

    public ProductionRecipe() { }

    public ProductionRecipe(string name)
    {
        recipeName = name;
    }

    public ProductionRecipe(string name, List<ResourceCost> inputList, List<ResourceCost> outputList)
    {
        recipeName = name;
        inputs = inputList;
        outputs = outputList;
    }

    /// <summary>
    /// 获取配方描述
    /// </summary>
    public string GetDescription()
    {
        string desc = $"{recipeName}\n";
        
        if (inputs.Count > 0)
        {
            desc += "输入: ";
            foreach (var input in inputs)
            {
                desc += $"{input.resourceType}×{input.quantity} ";
            }
            desc += "\n";
        }

        if (outputs.Count > 0)
        {
            desc += "输出: ";
            foreach (var output in outputs)
            {
                desc += $"{output.resourceType}×{output.quantity} ";
            }
        }

        return desc;
    }
}
