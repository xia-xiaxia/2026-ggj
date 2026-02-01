using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 回合制工厂 - 每回合投料一次，回合结束时自动收获
/// </summary>
public class TurnBasedFactory : MonoBehaviour
{
    [Header("工厂配置")]
    public FactoryType factoryType;
    public string factoryName = "工厂";
    
    [Header("建造成本")]
    public List<ResourceCost> buildCosts = new List<ResourceCost>();
    
    [Header("生产配方（可以有多个）")]
    public List<ProductionRecipe> recipes = new List<ProductionRecipe>();
    
    public int currentRecipeIndex = 0; // 当前使用的配方索引
    
    // 状态
    private bool isBuilt = false;
    private bool hasInvestedThisTurn = false; // 本回合是否已投料
    private bool hasProductionToHarvest = false; // 是否有产出待收获
    private int pendingProductionCount = 0; // 本回合投料份数


    void Start()
    {
        // 订阅回合事件
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.OnTurnStarted += OnTurnStarted;
            TurnSystem.Instance.OnTurnEnded += OnTurnEnded;
        }
    }

    void OnDestroy()
    {
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.OnTurnStarted -= OnTurnStarted;
            TurnSystem.Instance.OnTurnEnded -= OnTurnEnded;
        }
    }

    /// 建造工厂
    public bool Build()
    {
        // if (isBuilt)
        // {
        //     Debug.Log($"{factoryName} 已经建造过了");
        //     return false;
        // }

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
        Debug.Log($"{factoryName} 建造成功！");
        return true;
    }

    /// 投料（每回合调用一次）- 立即扣资源，标记为待收获
    public bool Invest()
    {
        return Invest(1);
    }

    /// 投料（按份数）- 立即扣资源，标记为待收获
    public bool Invest(int count)
    {
        if (!isBuilt)
        {
            Debug.Log($"{factoryName} 未建造");
            return false;
        }

        if (hasInvestedThisTurn)
        {
            Debug.Log($"{factoryName} 本回合已投料过了");
            return false;
        }

        if (count <= 0)
        {
            Debug.LogWarning($"{factoryName} 投料份数无效：{count}");
            return false;
        }

        if (recipes.Count == 0)
        {
            Debug.Log($"{factoryName} 没有可用的配方");
            return false;
        }

        ProductionRecipe recipe = recipes[currentRecipeIndex];

        // 检查资源是否足够
        if (!CanProduce(recipe, count))
        {
            Debug.Log($"{factoryName} 资源不足，无法投料");
            return false;
        }

        // 立即扣除输入资源
        ConsumeInputs(recipe, count);

        // 标记为已投料和待收获
        hasInvestedThisTurn = true;
        hasProductionToHarvest = true;
        pendingProductionCount = count;

        Debug.Log($"{factoryName} 投料成功，配方：{recipe.recipeName}，份数：{count}");
        return true;
    }

    /// 回合结束时收获 - 由TurnSystem调用
    public void Harvest()
    {
        if (!isBuilt || !hasProductionToHarvest) return;

        ProductionRecipe recipe = recipes[currentRecipeIndex];
        
        // 产出资源
        ProduceOutputs(recipe, pendingProductionCount > 0 ? pendingProductionCount : 1);

        // 重置待收获标志
        hasProductionToHarvest = false;
        pendingProductionCount = 0;

        Debug.Log($"{factoryName} 回合结束，自动收获");
    }

    /// 检查是否可以生产
    bool CanProduce(ProductionRecipe recipe, int count)
    {
        foreach (var input in recipe.inputs)
        {
            int required = input.quantity * count;
            if (ResourceManager.Instance.GetResourceQuantity(input.resourceType) < required)
            {
                return false;
            }
        }
        return true;
    }

    /// 消耗输入资源
    void ConsumeInputs(ProductionRecipe recipe, int count)
    {
        foreach (var input in recipe.inputs)
        {
            int required = input.quantity * count;
            ResourceManager.Instance.RemoveResource(input.resourceType, required);
        }
    }

    /// 产出资源
    void ProduceOutputs(ProductionRecipe recipe, int count)
    {
        foreach (var output in recipe.outputs)
        {
            int amount = output.quantity * count;
            ResourceManager.Instance.AddResource(output.resourceType, amount);
        }
    }

    /// 回合开始事件回调 - 重置本回合投料标志
    void OnTurnStarted()
    {
        hasInvestedThisTurn = false;
    }

    /// 回合结束事件回调 - 收获产出
    void OnTurnEnded()
    {
        Harvest();
    }

    /// 切换配方
    public void SwitchRecipe(int index)
    {
        if (index >= 0 && index < recipes.Count)
        {
            currentRecipeIndex = index;
        }
    }

    /// 获取工厂状态
    public string GetStatus()
    {
        if (!isBuilt) return "未建造";
        if (!hasInvestedThisTurn) return "待投料";
        if (hasProductionToHarvest) return "生产中";
        return "完成";
    }

    /// 判断本回合是否已投料
    public bool HasInvestedThisTurn()
    {
        return hasInvestedThisTurn;
    }

    /// 判断是否有产出待收获
    public bool HasProductionToHarvest()
    {
        return hasProductionToHarvest;
    }
}
