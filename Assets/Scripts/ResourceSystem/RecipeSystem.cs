using UnityEngine;
using System.Collections.Generic;
using System;

/// 配方系统 - 管理资源的转换和合成
public class RecipeSystem : MonoBehaviour
{
    public static RecipeSystem Instance { get; private set; }

    // 所有配方库
    [SerializeField]
    private List<Recipe> allRecipes = new List<Recipe>();

    // 正在进行的操作队列
    private Queue<CraftingJob> craftingQueue = new Queue<CraftingJob>();

    // 当前正在进行的操作
    private CraftingJob currentJob;

    // 事件系统
    public event Action<Recipe> OnRecipeStarted;
    public event Action<Recipe> OnRecipeCompleted;
    public event Action<Recipe> OnRecipeFailed;
    public event Action<float> OnCraftingProgress; // 进度 0-1

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeRecipes();
    }

    void Update()
    {
        if (currentJob != null)
        {
            UpdateCurrentJob();
        }
        else if (craftingQueue.Count > 0)
        {
            StartNextJob();
        }
    }

    /// 初始化配方 - 在这里定义所有的配方
    void InitializeRecipes()
    {
        allRecipes.Clear();
        CreateDefaultRecipes();
    }

    /// <summary>
    /// 创建默认配方
    /// </summary>
    void CreateDefaultRecipes()
    {
        // 示例：Mask-1合成配方
        Recipe mask1Recipe = new Recipe("合成Mask-1", 10f, 2);
        mask1Recipe.description = "用加工面料和铁合成Mask-1，需要2个人力单位";
        mask1Recipe.inputs.Add(new RecipeItem(ResourceType.protectFabric, 3));
        mask1Recipe.inputs.Add(new RecipeItem(ResourceType.iron, 5));
        mask1Recipe.inputs.Add(new RecipeItem(ResourceType.people, 2));
        mask1Recipe.outputs.Add(new RecipeItem(ResourceType.Mask1, 1));
        AddRecipe(mask1Recipe);

        // 示例：Mask-2合成配方
        Recipe mask2Recipe = new Recipe("合成Mask-2", 15f, 3);
        mask2Recipe.description = "用模块化工程材料和铜合成Mask-2";
        mask2Recipe.inputs.Add(new RecipeItem(ResourceType.ModularEngineeringMaterials, 2));
        mask2Recipe.inputs.Add(new RecipeItem(ResourceType.copper, 4));
        mask2Recipe.inputs.Add(new RecipeItem(ResourceType.people, 3));
        mask2Recipe.outputs.Add(new RecipeItem(ResourceType.Mask2, 1));
        AddRecipe(mask2Recipe);

        // 示例：加工面料配方
        Recipe processFabricRecipe = new Recipe("加工面料", 5f, 1);
        processFabricRecipe.description = "从硅沙加工成面料";
        processFabricRecipe.inputs.Add(new RecipeItem(ResourceType.silica, 10));
        processFabricRecipe.outputs.Add(new RecipeItem(ResourceType.protectFabric, 5));
        AddRecipe(processFabricRecipe);

        // 示例：采集硫酸配方（模拟采集）
        Recipe collectAcidRecipe = new Recipe("采集硫酸", 3f, 1);
        collectAcidRecipe.description = "从火山硫化矿采集硫酸";
        collectAcidRecipe.inputs.Add(new RecipeItem(ResourceType.volcOre, 2));
        collectAcidRecipe.outputs.Add(new RecipeItem(ResourceType.acid, 5));
        AddRecipe(collectAcidRecipe);

        Debug.Log($"已创建 {allRecipes.Count} 个默认配方");
    }

    /// 添加配方
    public void AddRecipe(Recipe recipe)
    {
        if (recipe != null && !allRecipes.Contains(recipe))
        {
            allRecipes.Add(recipe);
        }
    }

    /// 移除配方
    public void RemoveRecipe(Recipe recipe)
    {
        allRecipes.Remove(recipe);
    }

    /// 获取所有配方
    public List<Recipe> GetAllRecipes()
    {
        return new List<Recipe>(allRecipes);
    }

    /// 根据名称获取配方
    public Recipe GetRecipeByName(string name)
    {
        return allRecipes.Find(r => r.recipeName == name);
    }

    /// 尝试执行配方
    public bool TryExecuteRecipe(Recipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("配方为空！");
            return false;
        }

        if (!recipe.isUnlocked)
        {
            Debug.LogWarning($"配方 '{recipe.recipeName}' 未解锁！");
            return false;
        }

        // 检查资源是否足够
        Dictionary<ResourceType, int> requiredResources = new Dictionary<ResourceType, int>();
        foreach (var input in recipe.inputs)
        {
            if (requiredResources.ContainsKey(input.resourceType))
            {
                requiredResources[input.resourceType] += input.quantity;
            }
            else
            {
                requiredResources[input.resourceType] = input.quantity;
            }
        }

        if (!ResourceManager.Instance.HasEnoughResources(requiredResources))
        {
            Debug.LogWarning($"资源不足，无法执行配方 '{recipe.recipeName}'");
            return false;
        }

        // 消耗输入资源
        if (!ResourceManager.Instance.RemoveResources(requiredResources))
        {
            Debug.LogError("消耗资源失败！");
            return false;
        }

        // 创建工作任务
        CraftingJob job = new CraftingJob(recipe);
        craftingQueue.Enqueue(job);

        return true;
    }

    /// 立即完成配方（跳过等待时间）
    public bool ExecuteRecipeInstantly(Recipe recipe)
    {
        if (!TryExecuteRecipe(recipe))
        {
            return false;
        }

        // 立即完成
        if (currentJob == null && craftingQueue.Count > 0)
        {
            currentJob = craftingQueue.Dequeue();
            CompleteCurrentJob();
        }

        return true;
    }

    /// 更新当前工作
    void UpdateCurrentJob()
    {
        if (currentJob == null) return;

        currentJob.elapsedTime += Time.deltaTime;

        // 计算进度
        float progress = currentJob.elapsedTime / currentJob.recipe.processingTime;
        OnCraftingProgress?.Invoke(Mathf.Clamp01(progress));

        if (currentJob.elapsedTime >= currentJob.recipe.processingTime)
        {
            CompleteCurrentJob();
        }
    }

    /// 完成当前工作
    void CompleteCurrentJob()
    {
        if (currentJob == null) return;

        Recipe recipe = currentJob.recipe;

        try
        {
            // 添加输出资源
            Dictionary<ResourceType, int> outputs = new Dictionary<ResourceType, int>();
            foreach (var output in recipe.outputs)
            {
                if (outputs.ContainsKey(output.resourceType))
                {
                    outputs[output.resourceType] += output.quantity;
                }
                else
                {
                    outputs[output.resourceType] = output.quantity;
                }
            }

            if (ResourceManager.Instance.AddResources(outputs))
            {
                OnRecipeCompleted?.Invoke(recipe);
                Debug.Log($"配方完成: {recipe.recipeName}");
            }
            else
            {
                throw new System.Exception("添加输出资源失败");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"配方执行失败: {e.Message}");
            OnRecipeFailed?.Invoke(recipe);

            // 回退：返还输入资源
            Dictionary<ResourceType, int> inputs = new Dictionary<ResourceType, int>();
            foreach (var input in recipe.inputs)
            {
                if (inputs.ContainsKey(input.resourceType))
                {
                    inputs[input.resourceType] += input.quantity;
                }
                else
                {
                    inputs[input.resourceType] = input.quantity;
                }
            }
            ResourceManager.Instance.AddResources(inputs);
        }
        finally
        {
            currentJob = null;
        }
    }

    /// 开始下一个工作
    void StartNextJob()
    {
        if (craftingQueue.Count == 0) return;

        currentJob = craftingQueue.Dequeue();
        OnRecipeStarted?.Invoke(currentJob.recipe);
        Debug.Log($"开始执行配方: {currentJob.recipe.recipeName}");
    }

    /// 获取当前工作信息
    public CraftingJob GetCurrentJob()
    {
        return currentJob;
    }

    /// 获取队列中的工作数
    public int GetQueuedJobCount()
    {
        return craftingQueue.Count + (currentJob != null ? 1 : 0);
    }

    /// 清空工作队列
    public void ClearQueue()
    {
        craftingQueue.Clear();
        currentJob = null;
    }

    /// 获取所有可执行的配方
    public List<Recipe> GetExecutableRecipes()
    {
        List<Recipe> executable = new List<Recipe>();

        foreach (var recipe in allRecipes)
        {
            if (!recipe.isUnlocked) continue;

            Dictionary<ResourceType, int> required = new Dictionary<ResourceType, int>();
            foreach (var input in recipe.inputs)
            {
                if (required.ContainsKey(input.resourceType))
                {
                    required[input.resourceType] += input.quantity;
                }
                else
                {
                    required[input.resourceType] = input.quantity;
                }
            }

            if (ResourceManager.Instance.HasEnoughResources(required))
            {
                executable.Add(recipe);
            }
        }

        return executable;
    }

    /// 解锁配方
    public void UnlockRecipe(string recipeName)
    {
        Recipe recipe = GetRecipeByName(recipeName);
        if (recipe != null)
        {
            recipe.isUnlocked = true;
            Debug.Log($"配方已解锁: {recipeName}");
        }
    }
}

/// 工作任务类
[System.Serializable]
public class CraftingJob
{
    public Recipe recipe;
    public float elapsedTime = 0f;

    public CraftingJob(Recipe r)
    {
        recipe = r;
    }

    public float GetProgress()
    {
        return recipe.processingTime > 0 ? elapsedTime / recipe.processingTime : 1f;
    }
}
