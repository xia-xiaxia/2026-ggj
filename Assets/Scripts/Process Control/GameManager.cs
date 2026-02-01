using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

/// 游戏管理器 - 管理整个游戏流程和UI
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI显示")]
    [SerializeField]
    private TextMeshProUGUI turnText; // 显示当前回合
    [SerializeField]
    private Button nextTurnButton; // 下一回合按钮
    [SerializeField]
    private Transform factoryListContainer; // 工厂列表容器
    [SerializeField]
    private GameObject factoryItemPrefab; // 工厂项预制体

    public TextMeshProUGUI infText; // 信息显示

    [Header("投料面板")]
    [SerializeField]
    private GameObject investmentPanel; // 投料面板
    [SerializeField]
    private TextMeshProUGUI factoryNameText; // 工厂名称显示
    [SerializeField]
    private TextMeshProUGUI inputInfoText; // 输入信息显示
    [SerializeField]
    private TextMeshProUGUI outputInfoText; // 输出信息显示
    [SerializeField]
    private TMP_InputField investmentCountInput; // 投料份数输入
    [SerializeField]
    private Button confirmInvestButton; // 确认投料按钮
    [SerializeField]
    private Button cancelInvestButton; // 取消按钮

    [Header("回合遮罩")]
    [SerializeField]
    private GameObject productionMask; // 回合结束黑幕
    [SerializeField]
    private TextMeshProUGUI productionMaskText; // 黑幕文本

    private List<TurnBasedFactory> factories = new List<TurnBasedFactory>();
    private Dictionary<TurnBasedFactory, GameObject> factoryUIItems = new Dictionary<TurnBasedFactory, GameObject>();
    private TurnBasedFactory selectedFactory; // 当前选中的工厂

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetInfoText(string message)
    {
        if (infText != null)
        {
            infText.text = message;
        }
    }

    void LogInfo(string message)
    {
        SetInfoText(message);
    }

    void LogWarning(string message)
    {
        if (infText != null)
        {
            infText.text = $"[警告] {message}";
        }
    }

    void LogError(string message)
    {
        if (infText != null)
        {
            infText.text = $"[错误] {message}";
        }
    }

    void Update()
    {
        // 一键投料
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            InvestAllFactories();
        }
    } 

    void Start()
    {
        // 订阅回合事件
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.OnTurnStarted += UpdateUI;
            TurnSystem.Instance.OnTurnStarted += HideProductionMask;
            TurnSystem.Instance.OnTurnEnded += ShowProductionMask;
        }

        // 订阅资源变化事件，用于刷新投料面板库存显示
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
        }

        // 设置下一回合按钮
        if (nextTurnButton != null)
        {
            nextTurnButton.onClick.AddListener(OnNextTurnClicked);
        }

        // 设置投料面板按钮
        if (confirmInvestButton != null)
        {
            confirmInvestButton.onClick.AddListener(OnConfirmInvestment);
        }
        if (cancelInvestButton != null)
        {
            cancelInvestButton.onClick.AddListener(OnCancelInvestment);
        }


        // 初始隐藏投料面板
        if (investmentPanel != null)
        {
            investmentPanel.SetActive(false);
        }

        HideProductionMask();

        InitializeUI();
    }

    void OnDestroy()
    {
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.OnTurnStarted -= UpdateUI;
            TurnSystem.Instance.OnTurnStarted -= HideProductionMask;
            TurnSystem.Instance.OnTurnEnded -= ShowProductionMask;
        }

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged -= OnResourceChanged;
        }
    }

    /// 初始化UI
    void InitializeUI()
    {
        UpdateTurnDisplay();
    }

    /// 更新UI
    public void UpdateUI()
    {
        UpdateTurnDisplay();
        UpdateFactoryDisplay();
    }

    /// 更新回合显示
    void UpdateTurnDisplay()
    {
        if (turnText != null)
        {
            int turn = TurnSystem.Instance.GetCurrentTurn();
            turnText.text = $"第 {turn} 回合";
        }
    }

    /// 更新工厂显示
    void UpdateFactoryDisplay()
    {
        factoryUIItems.Clear();

        // 仅同步工厂数据列表（不创建UI）
        factories = new List<TurnBasedFactory>(FindObjectsByType<TurnBasedFactory>(FindObjectsSortMode.None));
    }

    /// 工厂投料按钮点击
    void OnFactoryInvestClicked(TurnBasedFactory factory, GameObject item)
    {
        bool success = factory.Invest();

        if (success)
        {
            // 禁用按钮
            Button investButton = item.transform.Find("InvestButton")?.GetComponent<Button>();
            if (investButton != null)
            {
                investButton.interactable = false;
            }

            // 更新状态
            TextMeshProUGUI statusText = item.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                statusText.text = factory.GetStatus();
            }
        }
    }

    /// 下一回合按钮点击
    void OnNextTurnClicked()
    {
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.EndTurn();
            UpdateUI();
        }
    }

    // ============ 工厂管理功能 ============

    /// 创建新工厂（动态生成）
    public TurnBasedFactory CreateFactory(string factoryType)
    {
        FactoryInfoData factoryData = FactoryDatabaseLoader.GetFactory(factoryType);
        if (factoryData == null)
        {
            LogError($"找不到工厂类型: {factoryType}");
            return null;
        }

        // 创建新的工厂GameObject
        GameObject factoryObj = new GameObject(factoryData.factoryName);
        TurnBasedFactory factory = factoryObj.AddComponent<TurnBasedFactory>();

        // 从数据库配置工厂
        factory.factoryName = factoryData.factoryName;
        factory.buildCosts = factoryData.buildCosts;
        factory.recipes = factoryData.recipes;

        // 尝试建造
        if (factory.Build())
        {
            factories.Add(factory);
            UpdateFactoryDisplay();
            LogInfo($"成功创建工厂: {factoryData.factoryName}");
            return factory;
        }
        else
        {
            Destroy(factoryObj);
            return null;
        }
    }

    /// 删除工厂
    public void RemoveFactory(TurnBasedFactory factory)
    {
        if (factories.Contains(factory))
        {
            factories.Remove(factory);
            Destroy(factory.gameObject);
            UpdateFactoryDisplay();
            LogInfo($"已删除工厂: {factory.factoryName}");
        }
    }

    /// 获取所有工厂
    public List<TurnBasedFactory> GetAllFactories()
    {
        return new List<TurnBasedFactory>(factories);
    }

    /// 按名称查找工厂
    public TurnBasedFactory GetFactoryByName(string name)
    {
        return factories.Find(f => f.factoryName == name);
    }

    /// 获取所有已投料的工厂
    public List<TurnBasedFactory> GetInvestedFactories()
    {
        return factories.FindAll(f => f.HasInvestedThisTurn());
    }

    /// 获取所有未投料的工厂
    public List<TurnBasedFactory> GetIdleFactories()
    {
        return factories.FindAll(f => !f.HasInvestedThisTurn());
    }

    /// 一键投料所有工厂
    public void InvestAllFactories()
    {
        int successCount = 0;
        foreach (var factory in factories)
        {
            if (factory.Invest())
            {
                successCount++;
            }
        }
        UpdateUI();
        LogInfo($"批量投料完成，成功 {successCount}/{factories.Count}");
    }

    /// 获取工厂数量
    public int GetFactoryCount()
    {
        return factories.Count;
    }

    /// 选择投料
    void OnSelectInvest()
    {
        if (selectedFactory != null)
        {
            ShowInvestmentPanel(selectedFactory);
        }
    }

    /// 显示投料面板
    public void ShowInvestmentPanel(TurnBasedFactory factory)
    {
        if (investmentPanel == null) return;
        if (factory == null) return;

        selectedFactory = factory;

        investmentPanel.SetActive(true); 
        investmentCountInput.gameObject.SetActive(false);    // 先隐藏以防止残留数据
        bool hasInputs = true;

        UpdateInvestmentPanelInfo(factory, ref hasInputs);

        // 重置投料份数输入
        if (investmentCountInput != null && hasInputs)
        {
            investmentCountInput.gameObject.SetActive(true);
            investmentCountInput.text = "请输入投料份数......";
        }
        else if (investmentCountInput != null)
        {
            investmentCountInput.text = string.Empty;
        }
    }

    /// 资源变化时刷新投料面板库存显示
    void OnResourceChanged(ResourceChangedEvent evt)
    {
        bool shouldRefresh = (investmentPanel != null && investmentPanel.activeSelf);

        if (!shouldRefresh) return;
        if (selectedFactory == null) return;

        bool hasInputs = true;
        UpdateInvestmentPanelInfo(selectedFactory, ref hasInputs);
    }

    /// 更新投料面板信息（输入/输出）
    void UpdateInvestmentPanelInfo(TurnBasedFactory factory, ref bool hasInputs)
    {
        // 显示工厂名称
        if (factoryNameText != null)
        {
            factoryNameText.text = factory.factoryName;
        }

        // 显示输入信息
        if (inputInfoText != null)
        {
            string inputInfo = "输入资源:\n";
            if (factory.recipes != null && factory.recipes.Count > 0 && factory.recipes[0].inputs.Count > 0)
            {
                var recipe = factory.recipes[0];
                foreach (var input in recipe.inputs)
                {
                    int available = ResourceManager.Instance.GetResourceQuantity(input.resourceType);
                    inputInfo += $"- {input.resourceType}: {input.quantity} (库存: {available})\n";
                }
            }
            else
            {
                inputInfo += "无需输入资源";
                hasInputs = false;
            }
            inputInfoText.text = inputInfo;
        }

        // 显示输出信息
        if (outputInfoText != null)
        {
            string outputInfo = "输出资源:\n";
            if (factory.recipes != null && factory.recipes.Count > 0)
            {
                var recipe = factory.recipes[0];
                foreach (var output in recipe.outputs)
                {
                    outputInfo += $"- {output.resourceType}: {output.quantity}\n";
                }
            }
            else
            {
                outputInfo += "无输出";
            }
            outputInfoText.text = outputInfo;
        }
    }

    /// 确认投料
    void OnConfirmInvestment()
    {
        if (selectedFactory == null) return;

        bool hasInputs = FactoryHasInputs(selectedFactory);

        // 获取玩家输入的份数
        int count = 1;
        if (investmentCountInput != null && hasInputs)
        {
            
            if(investmentCountInput.text == "")
            {
                selectedFactory.Invest();
                investmentPanel.SetActive(false);
                LogInfo($"成功投料 {count} 份到 {selectedFactory.factoryName}");
                UpdateUI();
                return;
            }
            else if (!int.TryParse(investmentCountInput.text, out count) || count <= 0)
            {
                LogWarning("请输入有效的投料份数");
                return;
            }
        }

        // 检查资源是否足够
        bool success = selectedFactory.Invest(count);
        if (success)
        {
            LogInfo($"成功投料 {count} 份到 {selectedFactory.factoryName}");
            UpdateUI();
        }
        else
        {
            LogWarning($"投料失败：{selectedFactory.factoryName}");
            return;
        }

        // 关闭面板
        investmentPanel.SetActive(false);
        selectedFactory = null;
    }

    bool FactoryHasInputs(TurnBasedFactory factory)
    {
        if (factory == null) return false;
        if (factory.recipes == null || factory.recipes.Count == 0) return false;
        var recipe = factory.recipes[factory.currentRecipeIndex];
        return recipe != null && recipe.inputs != null && recipe.inputs.Count > 0;
    }

    /// 取消投料
    void OnCancelInvestment()
    {
        if (investmentPanel != null)
        {
            investmentPanel.SetActive(false);
        }
        selectedFactory = null;
    }

    void ShowProductionMask()
    {
        if (productionMaskText != null)
        {
            productionMaskText.text = "生产中...";
        }
        if (productionMask != null)
        {
            productionMask.SetActive(true);
        }
    }

    void HideProductionMask()
    {
        if (productionMask != null)
        {
            productionMask.SetActive(false);
        }
    }

}

/// 工厂点击处理器（挂载到工厂GameObject上）
public class FactoryClickHandler : MonoBehaviour
{
    public System.Action OnFactoryClicked;

    void OnMouseDown()
    {
        OnFactoryClicked?.Invoke();
    }
}
