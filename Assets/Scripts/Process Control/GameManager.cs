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

    [Header("选择面板")]
    [SerializeField]
    private GameObject actionSelectPanel; // 选择面板（投料/销毁）
    [SerializeField]
    public TextMeshProUGUI actionSelectFactoryNameText; // 选择面板工厂名称显示
    [SerializeField]
    private Button selectInvestButton; // 选择投料
    [SerializeField]
    private Button selectDestroyButton; // 选择销毁

    [Header("销毁确认面板")]
    [SerializeField]
    private GameObject destroyConfirmPanel; // 销毁确认面板
    [SerializeField]
    public TextMeshProUGUI destroyConfirmFactoryNameText; // 销毁确认面板工厂名称显示
    [SerializeField]
    private Button confirmDestroyButton; // 确认销毁按钮
    [SerializeField]
    private Button cancelDestroyButton; // 取消销毁按钮

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

    void Update()
    {
        // if(Keyboard.current.jKey.wasPressedThisFrame)
        // {
        //     // 测试：按 J 键创建水热发电厂
        //     TurnBasedFactory newFactory = CreateFactory("hydroThermal");
        //     OnFactoryObjectClicked(newFactory);
        // }
        // if(Keyboard.current.lKey.wasPressedThisFrame)
        // {
        //     // 测试：按 L 键删除第一个工厂
        //     if (factories.Count > 0)
        //     {
        //         RemoveFactory(factories[0]);
        //     }
        // }
        // if(Keyboard.current.iKey.wasPressedThisFrame)
        // {
        //     // 测试：按 I 键显示所有工厂数量
        //     Debug.Log($"当前工厂数量: {GetFactoryCount()}");
        // }
        // if(Keyboard.current.oKey.wasPressedThisFrame)
        // {
        //     // 测试：按 O 键创建酸塔工厂
        //     TurnBasedFactory newFactory = CreateFactory("acidTower");
        //     OnFactoryObjectClicked(newFactory);
        // }
        // if(Keyboard.current.pKey.wasPressedThisFrame)
        // {
        //     // 测试：按 P 键创建湖泊采集工厂
        //     TurnBasedFactory newfactor = CreateFactory("lakeTreat");
        //     OnFactoryObjectClicked(newfactor);
            
        // }
        // if(Keyboard.current.kKey.wasPressedThisFrame)
        // {
        //     InvestAllFactories();
        // }
    } 

    void Start()
    {
        // 订阅回合事件
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.OnTurnStarted += UpdateUI;
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

        // 设置选择面板按钮
        if (selectInvestButton != null)
        {
            selectInvestButton.onClick.AddListener(OnSelectInvest);
        }
        if (selectDestroyButton != null)
        {
            selectDestroyButton.onClick.AddListener(OnSelectDestroy);
        }

        // 设置销毁确认面板按钮
        if (confirmDestroyButton != null)
        {
            confirmDestroyButton.onClick.AddListener(OnConfirmDestroy);
        }
        if (cancelDestroyButton != null)
        {
            cancelDestroyButton.onClick.AddListener(OnCancelDestroy);
        }

        // 初始隐藏投料面板
        if (investmentPanel != null)
        {
            investmentPanel.SetActive(false);
        }

        // 初始隐藏选择面板
        if (actionSelectPanel != null)
        {
            actionSelectPanel.SetActive(false);
        }

        // 初始隐藏销毁确认面板
        if (destroyConfirmPanel != null)
        {
            destroyConfirmPanel.SetActive(false);
        }

        InitializeUI();
    }

    void OnDestroy()
    {
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.OnTurnStarted -= UpdateUI;
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

    /// 创建工厂UI项（在放置物体时调用，自动绑定物体）
    public void CreateFactoryItem(GameObject factoryObj)
    {
        if (factoryObj == null) return;
        if (factoryObj.name == "WalkWay") return;

        TurnBasedFactory factory = factoryObj.GetComponent<TurnBasedFactory>();
        if (factory == null)
        {
            string rawName = factoryObj.name;
            if (!System.Enum.TryParse(rawName, true, out FactoryType factoryType))
            {
                Debug.LogWarning($"无法解析工厂类型: {rawName}");
                return;
            }

            FactoryInfoData factoryData = FactoryDatabaseLoader.GetFactory(factoryType.ToString());
            if (factoryData == null)
            {
                Debug.LogError($"找不到工厂类型: {factoryType}");
                return;
            }

            factory = factoryObj.AddComponent<TurnBasedFactory>();
            factory.factoryType = factoryType;
            factory.factoryName = factoryData.factoryName;
            factory.buildCosts = factoryData.buildCosts;
            factory.recipes = factoryData.recipes;
        }

        if (!factories.Contains(factory))
        {
            factories.Add(factory);
        }

        // 给工厂GameObject添加点击监听（需要Collider组件）
        if (factory.GetComponent<Collider>() == null && factory.GetComponent<Collider2D>() == null)
        {
            factory.gameObject.AddComponent<BoxCollider>();
        }

        FactoryClickHandler clickHandler = factory.gameObject.GetComponent<FactoryClickHandler>();
        if (clickHandler == null)
        {
            clickHandler = factory.gameObject.AddComponent<FactoryClickHandler>();
        }
        clickHandler.OnFactoryClicked = () => OnFactoryObjectClicked(factory);
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

    /// 更新工厂项状态
    void UpdateFactoryItemStatus()
    {
        foreach (var factory in factories)
        {
            if (!factoryUIItems.ContainsKey(factory)) continue;

            GameObject item = factoryUIItems[factory];
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
            Debug.LogError($"找不到工厂类型: {factoryType}");
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
            Debug.Log($"成功创建工厂: {factoryData.factoryName}");
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
            Debug.Log($"已删除工厂: {factory.factoryName}");
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
        Debug.Log($"批量投料完成，成功 {successCount}/{factories.Count}");
    }

    /// 获取工厂数量
    public int GetFactoryCount()
    {
        return factories.Count;
    }

    // ============ 放置和交互功能 ============

    /// 放置工厂（待实现）
    public void PlaceFactory(string factoryType, Vector3 position)
    {

        Debug.Log($"放置工厂: {factoryType} 在位置 {position}");
        
        // 临时实现：直接创建工厂
        TurnBasedFactory factory = CreateFactory(factoryType);
        if (factory != null)
        {
            factory.transform.position = position;
        }
    }

    /// 工厂物体被点击
    void OnFactoryObjectClicked(TurnBasedFactory factory)
    {
        selectedFactory = factory;
        ShowActionSelectPanel(factory);
    }

    /// 显示选择面板（投料/销毁）
    void ShowActionSelectPanel(TurnBasedFactory factory)
    {
        if (actionSelectPanel == null) return;
        // 显示工厂名称
        if (actionSelectFactoryNameText != null)
        {
            actionSelectFactoryNameText.text = factory.factoryName;
        }

        // 先关闭其他面板
        if (investmentPanel != null) investmentPanel.SetActive(false);
        if (destroyConfirmPanel != null) destroyConfirmPanel.SetActive(false);

        actionSelectPanel.SetActive(true);
    }

    /// 选择投料
    void OnSelectInvest()
    {
        if (actionSelectPanel != null) actionSelectPanel.SetActive(false);
        if (selectedFactory != null)
        {
            ShowInvestmentPanel(selectedFactory);
        }
    }

    /// 选择销毁
    void OnSelectDestroy()
    {
        if (actionSelectPanel != null) actionSelectPanel.SetActive(false);
        if (selectedFactory != null)
        {
            ShowDestroyConfirmPanel(selectedFactory);
        }
    }

    /// 显示销毁确认面板，并在一旁面板显示信息
    void ShowDestroyConfirmPanel(TurnBasedFactory factory)
    {
        if (destroyConfirmPanel == null) return;
        // 显示工厂名称
        if (destroyConfirmFactoryNameText != null)
        {
            destroyConfirmFactoryNameText.text = factory.factoryName;
        }

        destroyConfirmPanel.SetActive(true);

        bool hasInputs = true;
        UpdateInvestmentPanelInfo(factory, ref hasInputs);
    }

    /// 显示投料面板
    void ShowInvestmentPanel(TurnBasedFactory factory)
    {
        if (investmentPanel == null) return;

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
    }

    /// 资源变化时刷新投料面板库存显示
    void OnResourceChanged(ResourceChangedEvent evt)
    {
        bool shouldRefresh = (investmentPanel != null && investmentPanel.activeSelf)
            || (destroyConfirmPanel != null && destroyConfirmPanel.activeSelf);

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

        // 获取玩家输入的份数
        int count = 1;
        if (investmentCountInput != null)
        {
            
            if(investmentCountInput.text == "")
            {
                selectedFactory.Invest();
                investmentPanel.SetActive(false);
                Debug.Log($"成功投料 {count} 份到 {selectedFactory.factoryName}");
                UpdateUI();
                return;
            }
            else if (!int.TryParse(investmentCountInput.text, out count) || count <= 0)
            {
                Debug.LogWarning("请输入有效的投料份数");
                return;
            }
        }

        // 检查资源是否足够
        bool success = selectedFactory.Invest(count);
        if (success)
        {
            Debug.Log($"成功投料 {count} 份到 {selectedFactory.factoryName}");
            UpdateUI();
        }
        else
        {
            Debug.LogWarning($"投料失败：{selectedFactory.factoryName}");
            return;
        }

        // 关闭面板
        investmentPanel.SetActive(false);
        selectedFactory = null;
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

    /// 确认销毁
    void OnConfirmDestroy()
    {
        if (selectedFactory == null) return;

        RemoveFactory(selectedFactory);

        if (destroyConfirmPanel != null)
        {
            destroyConfirmPanel.SetActive(false);
        }
        selectedFactory = null;
    }

    /// 取消销毁
    void OnCancelDestroy()
    {
        if (destroyConfirmPanel != null)
        {
            destroyConfirmPanel.SetActive(false);
        }
        selectedFactory = null;
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
