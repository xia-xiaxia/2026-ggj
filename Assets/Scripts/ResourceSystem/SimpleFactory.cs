using UnityEngine;
using System.Collections.Generic;

/// 简单工厂 - 自动循环生产
public class SimpleFactory : MonoBehaviour
{
    [Header("工厂配置")]
    public FactoryType factoryType;
    public string factoryName = "工厂";
    
    [Header("建造成本")]
    public List<ResourceCost> buildCosts = new List<ResourceCost>();
    
    [Header("生产配方（可以有多个）")]
    public List<ProductionRecipe> recipes = new List<ProductionRecipe>();
    
    [Header("生产设置")]
    public float productionCycleTime = 5f; // 生产周期（秒）
    public int currentRecipeIndex = 0; // 当前使用的配方索引
    
    // 状态
    private bool isBuilt = false;
    private bool isProducing = false;
    private float productionTimer = 0f;

    void Update()
    {
        if (!isBuilt || recipes.Count == 0) return;

        // 尝试生产
        ProductionRecipe currentRecipe = recipes[currentRecipeIndex];
        
        // 检查资源是否足够
        if (CanProduce(currentRecipe))
        {
            if (!isProducing)
            {
                // 开始生产，扣除资源
                ConsumeInputs(currentRecipe);
                isProducing = true;
                productionTimer = 0f;
            }

            // 生产计时
            productionTimer += Time.deltaTime;

            if (productionTimer >= productionCycleTime)
            {
                // 生产完成，添加产出
                ProduceOutputs(currentRecipe);
                isProducing = false;
                productionTimer = 0f;
            }
        }
        else
        {
            // 资源不足，停止生产
            isProducing = false;
        }
    }

    /// 建造工厂
    public bool Build()
    {
        if (isBuilt)
        {
            Debug.Log($"{factoryName} 已经建造过了");
            return false;
        }

        // 检查建造资源
        foreach (var cost in buildCosts)
        {
            if (ResourceManager.Instance.GetResourceQuantity(cost.resourceType) < cost.quantity)
            {
                Debug.Log($"{factoryName} 建造失败：缺少 {cost.resourceType}");
                return false;
            }
        }

        // 扣除建造资源
        foreach (var cost in buildCosts)
        {
            ResourceManager.Instance.RemoveResource(cost.resourceType, cost.quantity);
        }

        isBuilt = true;
        Debug.Log($"{factoryName} 建造成功！开始自动生产");
        return true;
    }

    /// 检查是否可以生产
    bool CanProduce(ProductionRecipe recipe)
    {
        foreach (var input in recipe.inputs)
        {
            if (ResourceManager.Instance.GetResourceQuantity(input.resourceType) < input.quantity)
            {
                return false;
            }
        }
        return true;
    }

    /// 消耗输入资源
    void ConsumeInputs(ProductionRecipe recipe)
    {
        foreach (var input in recipe.inputs)
        {
            ResourceManager.Instance.RemoveResource(input.resourceType, input.quantity);
        }
    }

    /// 产出资源
    void ProduceOutputs(ProductionRecipe recipe)
    {
        foreach (var output in recipe.outputs)
        {
            ResourceManager.Instance.AddResource(output.resourceType, output.quantity);
        }
        Debug.Log($"{factoryName} 完成一次生产");
    }

    /// 切换配方
    public void SwitchRecipe(int index)
    {
        if (index >= 0 && index < recipes.Count)
        {
            currentRecipeIndex = index;
            isProducing = false; // 重置生产状态
        }
    }

    /// 获取工厂状态
    public string GetStatus()
    {
        if (!isBuilt) return "未建造";
        if (!isProducing) return "等待资源";
        float progress = productionTimer / productionCycleTime;
        return $"生产中 {progress * 100:F0}%";
    }
}

/// 资源成本/数量
[System.Serializable]
public class ResourceCost
{
    public ResourceType resourceType;
    public int quantity;
}

/// 生产配方
[System.Serializable]
public class ProductionRecipe
{
    public string recipeName = "配方";
    public List<ResourceCost> inputs = new List<ResourceCost>();
    public List<ResourceCost> outputs = new List<ResourceCost>();
}
