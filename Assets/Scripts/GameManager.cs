using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// 游戏管理器 - 管理整个游戏流程和UI
/// </summary>
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

    private List<TurnBasedFactory> factories = new List<TurnBasedFactory>();
    private Dictionary<TurnBasedFactory, GameObject> factoryUIItems = new Dictionary<TurnBasedFactory, GameObject>();

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
        if(Keyboard.current.jKey.wasPressedThisFrame)
        {
            // 测试：按 J 键创建水热发电厂
            CreateFactory("hydroThermal");
        }
        if(Keyboard.current.kKey.wasPressedThisFrame)
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
        }

        // 设置下一回合按钮
        if (nextTurnButton != null)
        {
            nextTurnButton.onClick.AddListener(OnNextTurnClicked);
        }

        InitializeUI();
    }

    void OnDestroy()
    {
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.OnTurnStarted -= UpdateUI;
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
        if (factoryListContainer == null) return;

        // 清空旧的UI
        foreach (Transform child in factoryListContainer)
        {
            Destroy(child.gameObject);
        }
        factoryUIItems.Clear();

        // 创建工厂UI
        factories = new List<TurnBasedFactory>(FindObjectsByType<TurnBasedFactory>(FindObjectsSortMode.None));
        foreach (var factory in factories)
        {
            CreateFactoryItem(factory);
        }
    }

    /// 创建工厂UI项
    void CreateFactoryItem(TurnBasedFactory factory)
    {
        if (factoryItemPrefab == null) return;

        GameObject item = Instantiate(factoryItemPrefab, factoryListContainer);
        item.name = factory.factoryName;

        // 设置工厂名称
        TextMeshProUGUI nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = factory.factoryName;
        }

        // 设置投料按钮
        Button investButton = item.transform.Find("InvestButton")?.GetComponent<Button>();
        if (investButton != null)
        {
            investButton.onClick.AddListener(() => OnFactoryInvestClicked(factory, item));
        }

        // 设置状态显示
        TextMeshProUGUI statusText = item.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
        if (statusText != null)
        {
            statusText.text = factory.GetStatus();
        }

        factoryUIItems[factory] = item;

        // 订阅工厂状态变化
        InvokeRepeating(nameof(UpdateFactoryItemStatus), 0.1f, 0.1f);
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
}
