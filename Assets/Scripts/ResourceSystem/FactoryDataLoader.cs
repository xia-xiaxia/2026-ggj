using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// 工厂数据加载器 - 从JSON加载工厂配置
public class FactoryDataLoader : MonoBehaviour
{
    [Header("配置文件")]
    public TextAsset factoryDataJson; // 拖入JSON文件
    
    [Header("工厂预制体")]
    public GameObject factoryPrefab; // TurnBasedFactory预制体
    public Transform factoryContainer; // 工厂容器

    void Start()
    {
        if (factoryDataJson != null)
        {
            LoadFactoriesFromJson();
        }
    }

    /// 从JSON加载工厂数据
    public void LoadFactoriesFromJson()
    {
        try
        {
            FactoryDataList data = JsonUtility.FromJson<FactoryDataList>(factoryDataJson.text);
            
            foreach (var factoryData in data.factories)
            {
                CreateFactory(factoryData);
            }
            
            Debug.Log($"成功加载 {data.factories.Count} 个工厂配置");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载工厂数据失败: {e.Message}");
        }
    }

    /// 创建工厂实例
    void CreateFactory(FactoryData data)
    {
        GameObject factoryObj = Instantiate(factoryPrefab, factoryContainer);
        factoryObj.name = data.factoryName;

        TurnBasedFactory factory = factoryObj.GetComponent<TurnBasedFactory>();
        if (factory == null)
        {
            factory = factoryObj.AddComponent<TurnBasedFactory>();
        }

        // 设置工厂配置
        factory.factoryName = data.factoryName;

        // 设置建造成本
        factory.buildCosts = new List<ResourceCost>();
        foreach (var cost in data.buildCosts)
        {
            factory.buildCosts.Add(new ResourceCost
            {
                resourceType = ParseResourceType(cost.resourceType),
                quantity = cost.quantity
            });
        }

        // 设置配方
        factory.recipes = new List<ProductionRecipe>();
        foreach (var recipeData in data.recipes)
        {
            ProductionRecipe recipe = new ProductionRecipe();
            recipe.recipeName = recipeData.recipeName;

            // 输入
            recipe.inputs = new List<ResourceCost>();
            foreach (var input in recipeData.inputs)
            {
                recipe.inputs.Add(new ResourceCost
                {
                    resourceType = ParseResourceType(input.resourceType),
                    quantity = input.quantity
                });
            }

            // 输出
            recipe.outputs = new List<ResourceCost>();
            foreach (var output in recipeData.outputs)
            {
                recipe.outputs.Add(new ResourceCost
                {
                    resourceType = ParseResourceType(output.resourceType),
                    quantity = output.quantity
                });
            }

            factory.recipes.Add(recipe);
        }

        // 尝试建造（消耗建造成本），失败会打印原因
        factory.Build();
    }

    /// 解析资源类型字符串
    ResourceType ParseResourceType(string typeName)
    {
        if (System.Enum.TryParse(typeName, true, out ResourceType type))
        {
            return type;
        }
        Debug.LogWarning($"无法解析资源类型: {typeName}");
        return ResourceType.people; // 默认值
    }
}

// === JSON数据结构 ===

[System.Serializable]
public class FactoryDataList
{
    public List<FactoryData> factories = new List<FactoryData>();
}

[System.Serializable]
public class FactoryData
{
    public string factoryName;
    public float productionTime = 5f;
    public List<ResourceCostData> buildCosts = new List<ResourceCostData>();
    public List<RecipeData> recipes = new List<RecipeData>();
}

[System.Serializable]
public class ResourceCostData
{
    public string resourceType;
    public int quantity;
}

[System.Serializable]
public class RecipeData
{
    public string recipeName;
    public List<ResourceCostData> inputs = new List<ResourceCostData>();
    public List<ResourceCostData> outputs = new List<ResourceCostData>();
}
