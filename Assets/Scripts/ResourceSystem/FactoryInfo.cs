using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 工厂类型分类
/// </summary>
public enum FactoryCategory
{
    发电,  // 发电工厂
    采集,  // 采集工厂
    加工,  // 加工工厂
    组装   // 组装工厂
}

/// <summary>
/// 工厂详细信息数据结构
/// </summary>
[System.Serializable]
public class FactoryInfo
{
    public FactoryType factoryType;
    public string factoryName; // 中文名称
    public string englishName; // 英文翻译
    public FactoryCategory category; // 工厂类别
    
    [TextArea(2, 4)]
    public string description; // 简介说明
    
    [TextArea(1, 3)]
    public string remarks; // 备注

    public int landOccupancy = 1; // 占地（所占地块数）
    
    // 建造成本
    public List<ResourceCost> buildCosts = new List<ResourceCost>();
    
    // 生产配方列表
    public List<ProductionRecipe> recipes = new List<ProductionRecipe>();

    public FactoryInfo(FactoryType type, string name, string english, FactoryCategory cat)
    {
        factoryType = type;
        factoryName = name;
        englishName = english;
        category = cat;
        buildCosts = new List<ResourceCost>();
        recipes = new List<ProductionRecipe>();
    }
}

/// <summary>
/// 游戏中所有工厂的数据库
/// </summary>
[System.Serializable]
public class FactoryDatabase
{
    public List<FactoryInfo> factories = new List<FactoryInfo>();

    public FactoryInfo GetFactory(FactoryType type)
    {
        return factories.Find(f => f.factoryType == type);
    }
}
