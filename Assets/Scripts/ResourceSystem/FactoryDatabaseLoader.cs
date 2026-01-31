using UnityEngine;
using System.Collections.Generic;

public class FactoryDatabaseLoader : MonoBehaviour
{
    [SerializeField] private TextAsset factoryDatabaseJson;

    private static FactoryDatabaseLoader instance;
    private FactoryDatabaseData factoryDatabase;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadFactoryDatabase();
    }

    private void LoadFactoryDatabase()
    {
        if (factoryDatabaseJson == null)
        {
            Debug.LogError("FactoryDatabase.json not found!");
            return;
        }

        FactoryDatabaseDataRaw raw = JsonUtility.FromJson<FactoryDatabaseDataRaw>(factoryDatabaseJson.text);
        factoryDatabase = ConvertFromRaw(raw);

        Debug.Log($"Loaded {factoryDatabase.factories.Count} factories from database");
    }

    private FactoryDatabaseData ConvertFromRaw(FactoryDatabaseDataRaw raw)
    {
        FactoryDatabaseData data = new FactoryDatabaseData();
        if (raw == null || raw.factories == null) return data;

        foreach (var rawFactory in raw.factories)
        {
            FactoryInfoData info = new FactoryInfoData
            {
                factoryType = rawFactory.factoryType,
                factoryName = rawFactory.factoryName,
                englishName = rawFactory.englishName,
                category = rawFactory.category,
                description = rawFactory.description,
                remarks = rawFactory.remarks,
                landOccupancy = rawFactory.landOccupancy,
                buildCosts = ConvertCosts(rawFactory.buildCosts),
                recipes = ConvertRecipes(rawFactory.recipes)
            };

            data.factories.Add(info);
        }

        return data;
    }

    private List<ResourceCost> ConvertCosts(List<FactoryResourceCostData> costs)
    {
        List<ResourceCost> result = new List<ResourceCost>();
        if (costs == null) return result;

        foreach (var cost in costs)
        {
            result.Add(new ResourceCost
            {
                resourceType = ParseResourceType(cost.resourceType),
                quantity = cost.quantity
            });
        }
        return result;
    }

    private List<ProductionRecipe> ConvertRecipes(List<FactoryRecipeData> recipes)
    {
        List<ProductionRecipe> result = new List<ProductionRecipe>();
        if (recipes == null) return result;

        foreach (var recipeData in recipes)
        {
            ProductionRecipe recipe = new ProductionRecipe
            {
                recipeName = recipeData.recipeName,
                inputs = ConvertCosts(recipeData.inputs),
                outputs = ConvertCosts(recipeData.outputs)
            };

            result.Add(recipe);
        }

        return result;
    }

    private ResourceType ParseResourceType(string typeName)
    {
        if (System.Enum.TryParse(typeName, true, out ResourceType type))
        {
            return type;
        }
        Debug.LogWarning($"无法解析资源类型: {typeName}");
        return ResourceType.people;
    }

    public static FactoryInfoData GetFactory(string factoryType)
    {
        if (instance == null || instance.factoryDatabase == null)
        {
            Debug.LogError("FactoryDatabaseLoader not initialized!");
            return null;
        }

        foreach (var factory in instance.factoryDatabase.factories)
        {
            if (string.Equals(factory.factoryType, factoryType, System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(factory.englishName, factoryType, System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(factory.factoryName, factoryType, System.StringComparison.OrdinalIgnoreCase))
            {
                return factory;
            }
        }

        Debug.LogWarning($"Factory '{factoryType}' not found in database");
        return null;
    }

    public static List<FactoryInfoData> GetAllFactories()
    {
        if (instance == null || instance.factoryDatabase == null)
        {
            Debug.LogError("FactoryDatabaseLoader not initialized!");
            return new List<FactoryInfoData>();
        }

        return instance.factoryDatabase.factories;
    }

    public static FactoryInfoData GetFactoryByName(string factoryName)
    {
        if (instance == null || instance.factoryDatabase == null)
        {
            Debug.LogError("FactoryDatabaseLoader not initialized!");
            return null;
        }

        foreach (var factory in instance.factoryDatabase.factories)
        {
            if (factory.factoryName == factoryName || factory.englishName == factoryName)
            {
                return factory;
            }
        }

        Debug.LogWarning($"Factory '{factoryName}' not found in database");
        return null;
    }

    public static List<FactoryInfoData> GetFactoriesByCategory(string category)
    {
        if (instance == null || instance.factoryDatabase == null)
        {
            Debug.LogError("FactoryDatabaseLoader not initialized!");
            return new List<FactoryInfoData>();
        }

        var result = new List<FactoryInfoData>();
        foreach (var factory in instance.factoryDatabase.factories)
        {
            if (factory.category == category)
            {
                result.Add(factory);
            }
        }

        return result;
    }
}

// JSON 数据结构（用于反序列化）
[System.Serializable]
public class FactoryInfoData
{
    public string factoryType;           // 工厂类型标识
    public string factoryName;           // 中文名称
    public string englishName;           // 英文名称
    public string category;              // 分类：发电/采集/加工/组装
    public string description;           // 简介
    public string remarks;               // 备注
    public int landOccupancy;            // 占地
    public List<ResourceCost> buildCosts = new List<ResourceCost>();  // 建筑消耗
    public List<ProductionRecipe> recipes = new List<ProductionRecipe>();  // 产物配方
}

[System.Serializable]
public class FactoryDatabaseData
{
    public List<FactoryInfoData> factories = new List<FactoryInfoData>();
}

// JSON 原始结构（用于字符串资源类型）
[System.Serializable]
public class FactoryDatabaseDataRaw
{
    public List<FactoryInfoDataRaw> factories = new List<FactoryInfoDataRaw>();
}

[System.Serializable]
public class FactoryInfoDataRaw
{
    public string factoryType;
    public string factoryName;
    public string englishName;
    public string category;
    public string description;
    public string remarks;
    public int landOccupancy;
    public List<FactoryResourceCostData> buildCosts = new List<FactoryResourceCostData>();
    public List<FactoryRecipeData> recipes = new List<FactoryRecipeData>();
}

[System.Serializable]
public class FactoryResourceCostData
{
    public string resourceType;
    public int quantity;
}

[System.Serializable]
public class FactoryRecipeData
{
    public string recipeName;
    public List<FactoryResourceCostData> inputs = new List<FactoryResourceCostData>();
    public List<FactoryResourceCostData> outputs = new List<FactoryResourceCostData>();
}
