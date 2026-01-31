using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 资源UI面板 - 显示当前资源和配方
/// </summary>
public class ResourceUIPanel : MonoBehaviour
{
    [Header("资源显示")]
    [SerializeField]
    private Transform resourceListContainer;
    [SerializeField]
    private GameObject resourceItemPrefab;

    [Header("配方显示")]
    [SerializeField]
    private Transform recipeListContainer;
    [SerializeField]
    private GameObject recipeItemPrefab;

    [Header("合成进度")]
    [SerializeField]
    private Image craftingProgressBar;
    [SerializeField]
    private TextMeshProUGUI craftingStatusText;

    [Header("日志显示")]
    [SerializeField]
    private TextMeshProUGUI logText;
    [SerializeField]
    private ScrollRect logScrollView;
    [SerializeField]
    private int maxLogLines = 20;

    private Dictionary<ResourceType, Text> resourceUIElements = new Dictionary<ResourceType, Text>();
    private List<RecipeUIItem> recipeUIItems = new List<RecipeUIItem>();

    void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    void InitializeUI()
    {
        if (ResourceManager.Instance != null)
        {
            RefreshResourceDisplay();
        }

        if (RecipeSystem.Instance != null)
        {
            RefreshRecipeDisplay();
        }

        UpdateCraftingStatus();
    }

    /// <summary>
    /// 订阅事件
    /// </summary>
    void SubscribeToEvents()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
        }

        if (RecipeSystem.Instance != null)
        {
            RecipeSystem.Instance.OnRecipeStarted += HandleRecipeStarted;
            RecipeSystem.Instance.OnRecipeCompleted += HandleRecipeCompleted;
            RecipeSystem.Instance.OnCraftingProgress += HandleCraftingProgress;
        }
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    void UnsubscribeFromEvents()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
        }

        if (RecipeSystem.Instance != null)
        {
            RecipeSystem.Instance.OnRecipeStarted -= HandleRecipeStarted;
            RecipeSystem.Instance.OnRecipeCompleted -= HandleRecipeCompleted;
            RecipeSystem.Instance.OnCraftingProgress -= HandleCraftingProgress;
        }
    }

    /// <summary>
    /// 刷新资源显示
    /// </summary>
    public void RefreshResourceDisplay()
    {
        if (resourceListContainer == null || ResourceManager.Instance == null) return;

        // 清空旧的UI元素
        foreach (Transform child in resourceListContainer)
        {
            Destroy(child.gameObject);
        }
        resourceUIElements.Clear();

        // 创建新的资源UI
        var allResources = ResourceManager.Instance.GetAllResources();
        foreach (var resource in allResources)
        {
            CreateResourceItem(resource.Key, resource.Value);
        }
    }

    /// <summary>
    /// 创建单个资源项
    /// </summary>
    void CreateResourceItem(ResourceType type, int quantity)
    {
        if (resourceItemPrefab == null)
        {
            Debug.LogWarning("resourceItemPrefab 未设置！");
            return;
        }

        GameObject itemObj = Instantiate(resourceItemPrefab, resourceListContainer);
        itemObj.name = type.ToString();

        // 查找UI组件
        TextMeshProUGUI nameText = itemObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        Text quantityText = itemObj.transform.Find("QuantityText")?.GetComponent<Text>();

        if (nameText != null)
        {
            nameText.text = type.ToString();
        }

        if (quantityText != null)
        {
            quantityText.text = quantity.ToString();
            resourceUIElements[type] = quantityText;
        }
    }

    /// <summary>
    /// 刷新配方显示
    /// </summary>
    public void RefreshRecipeDisplay()
    {
        if (recipeListContainer == null || RecipeSystem.Instance == null) return;

        // 清空旧的配方UI
        foreach (Transform child in recipeListContainer)
        {
            Destroy(child.gameObject);
        }
        recipeUIItems.Clear();

        // 创建新的配方UI
        var allRecipes = RecipeSystem.Instance.GetAllRecipes();
        foreach (var recipe in allRecipes)
        {
            CreateRecipeItem(recipe);
        }
    }

    /// <summary>
    /// 创建单个配方项
    /// </summary>
    void CreateRecipeItem(Recipe recipe)
    {
        if (recipeItemPrefab == null)
        {
            Debug.LogWarning("recipeItemPrefab 未设置！");
            return;
        }

        GameObject itemObj = Instantiate(recipeItemPrefab, recipeListContainer);
        itemObj.name = recipe.recipeName;

        RecipeUIItem uiItem = itemObj.AddComponent<RecipeUIItem>();
        uiItem.Initialize(recipe);

        recipeUIItems.Add(uiItem);
    }

    /// <summary>
    /// 处理资源变化
    /// </summary>
    void HandleResourceChanged(ResourceChangedEvent resourceEvent)
    {
        if (resourceUIElements.ContainsKey(resourceEvent.resourceType))
        {
            Text text = resourceUIElements[resourceEvent.resourceType];
            text.text = resourceEvent.newQuantity.ToString();

            // 添加日志
            AddLog($"{resourceEvent.resourceType}: {resourceEvent.oldQuantity} → {resourceEvent.newQuantity}");
        }
    }

    /// <summary>
    /// 处理配方开始
    /// </summary>
    void HandleRecipeStarted(Recipe recipe)
    {
        AddLog($"开始: {recipe.recipeName} (耗时: {recipe.processingTime}s)");
    }

    /// <summary>
    /// 处理配方完成
    /// </summary>
    void HandleRecipeCompleted(Recipe recipe)
    {
        AddLog($"完成: {recipe.recipeName}");
    }

    /// <summary>
    /// 处理合成进度
    /// </summary>
    void HandleCraftingProgress(float progress)
    {
        UpdateCraftingStatus();
    }

    /// <summary>
    /// 更新合成状态
    /// </summary>
    void UpdateCraftingStatus()
    {
        if (RecipeSystem.Instance == null) return;

        var currentJob = RecipeSystem.Instance.GetCurrentJob();

        if (currentJob != null)
        {
            float progress = currentJob.GetProgress();
            
            if (craftingProgressBar != null)
            {
                craftingProgressBar.fillAmount = Mathf.Clamp01(progress);
            }

            if (craftingStatusText != null)
            {
                float remainingTime = currentJob.recipe.processingTime - currentJob.elapsedTime;
                craftingStatusText.text = $"合成中: {currentJob.recipe.recipeName}\n进度: {progress * 100:F1}% ({remainingTime:F1}s)";
            }
        }
        else
        {
            if (craftingProgressBar != null)
            {
                craftingProgressBar.fillAmount = 0;
            }

            if (craftingStatusText != null)
            {
                craftingStatusText.text = "空闲";
            }
        }
    }

    /// <summary>
    /// 添加日志
    /// </summary>
    void AddLog(string message)
    {
        if (logText == null) return;

        string logEntry = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        logText.text += logEntry + "\n";

        // 限制日志行数
        string[] lines = logText.text.Split('\n');
        if (lines.Length > maxLogLines)
        {
            logText.text = string.Join("\n", lines, lines.Length - maxLogLines, maxLogLines);
        }

        // 滚动到底部
        if (logScrollView != null)
        {
            Canvas.ForceUpdateCanvases();
            logScrollView.verticalNormalizedPosition = 0;
        }
    }

    void Update()
    {
        UpdateCraftingStatus();
    }
}

/// <summary>
/// 配方UI项
/// </summary>
public class RecipeUIItem : MonoBehaviour
{
    private Recipe recipe;
    private Button executeButton;
    private TextMeshProUGUI descriptionText;

    public void Initialize(Recipe recipe)
    {
        this.recipe = recipe;

        // 查找UI组件
        TextMeshProUGUI nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        descriptionText = transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        executeButton = transform.Find("ExecuteButton")?.GetComponent<Button>();

        if (nameText != null)
        {
            nameText.text = recipe.recipeName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = recipe.description;
        }

        if (executeButton != null)
        {
            executeButton.onClick.AddListener(OnExecuteButtonClicked);
        }
    }

    void OnExecuteButtonClicked()
    {
        if (RecipeSystem.Instance != null && recipe != null)
        {
            bool success = RecipeSystem.Instance.TryExecuteRecipe(recipe);
            
            if (executeButton != null)
            {
                executeButton.interactable = !success;
            }
        }
    }

    void Update()
    {
        if (executeButton == null || recipe == null || RecipeSystem.Instance == null) return;

        // 检查是否可以执行该配方
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

        bool canExecute = recipe.isUnlocked && ResourceManager.Instance.HasEnoughResources(required);
        executeButton.interactable = canExecute;
    }
}
