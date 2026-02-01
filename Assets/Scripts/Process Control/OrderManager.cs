using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using TMPro;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;
    
    [SerializeField]
    public List<Order> allOrders = new List<Order>();
    
    public Queue<Order> orderQueue = new Queue<Order>();

    // 事件系统
    public event Action OnOrderChanged;

    public TextMeshProUGUI orderInfoText;

    bool isTurnBound;
    bool isResourceBound;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach(var order in allOrders)
        {
            orderQueue.Enqueue(order);
        }
    }

    void Start()
    {
        if(orderQueue == null)
        {
            orderQueue = new Queue<Order>();
        }
        if(OrderManager.Instance != null && orderQueue != null && orderQueue.Count > 0)
        {
            showOrderInfo();
        }
    }

    void OnEnable()
    {
        TryBindTurnSystem();
        TryBindResourceManager();
        if(OrderManager.Instance != null && orderQueue != null && orderQueue.Count > 0)
        {
            OrderManager.Instance.OnOrderChanged += showOrderInfo;
        }
    }

    void OnDisable()
    {
        UnbindTurnSystem();
        UnbindResourceManager();
        if(OrderManager.Instance != null && orderQueue != null && orderQueue.Count > 0)
        {
            OrderManager.Instance.OnOrderChanged -= showOrderInfo;
        }
    }

    void Update()
    {
        if (!isTurnBound)
        {
            TryBindTurnSystem();
        }
        if (!isResourceBound)
        {
            TryBindResourceManager();
        }
    }

    void HandleTurnEnded()
    {
        TimeLimitedUpdate();
    }

    void HandleResourceChanged(ResourceChangedEvent evt)
    {
        CheckOrders();
    }

    void TryBindTurnSystem()
    {
        if (isTurnBound)
            return;
        if (TurnSystem.Instance == null)
            return;

        TurnSystem.Instance.OnTurnEnded += HandleTurnEnded;
        isTurnBound = true;
    }

    void UnbindTurnSystem()
    {
        if (!isTurnBound)
            return;
        if (TurnSystem.Instance == null)
            return;

        TurnSystem.Instance.OnTurnEnded -= HandleTurnEnded;
        isTurnBound = false;
    }

    void TryBindResourceManager()
    {
        if (isResourceBound)
            return;
        if (ResourceManager.Instance == null)
            return;

        ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
        isResourceBound = true;
    }

    void UnbindResourceManager()
    {
        if (!isResourceBound)
            return;
        if (ResourceManager.Instance == null)
            return;

        ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
        isResourceBound = false;
    }

    void TimeLimitedUpdate()
    {
        if (orderQueue.Count == 0)
            return;
        if(orderQueue.Peek().turnsToComplete > 0)
        {
            int turnLimited = orderQueue.Peek().turnsToComplete;
            if(TurnSystem.Instance.GetCurrentTurn() > turnLimited)
            {
                Debug.LogWarning($"订单 {orderQueue.Peek().orderId} 超时未完成，已失败！");
                orderQueue.Dequeue();
                OnOrderChanged?.Invoke();
            }
            else
            {
                Debug.Log($"订单 {orderQueue.Peek().orderId} 还剩 {turnLimited - TurnSystem.Instance.GetCurrentTurn()} 回合结束");
                CheckOrders();
            }
        }
    }

    void CheckOrders()
    {
        if (orderQueue.Count == 0)
            return;
        Dictionary<ResourceType, int> currentResources = ResourceManager.Instance.GetAllResources();

        Order currentOrder = orderQueue.Peek();
        if (currentOrder.IsFulfilled(currentResources))
        {
            Debug.Log($"订单 {currentOrder.orderId} 已完成！");
            orderQueue.Dequeue();
            OnOrderChanged?.Invoke();
        }
    }

    void AddOrder(Order newOrder)
    {
        orderQueue.Enqueue(newOrder);
    }

    void showOrderInfo()
    {
        if(orderQueue.Count == 0) 
        {
            orderInfoText.text = "当前无订单";
            return;
        }
        Order currentOrder = orderQueue.Peek();
        string info = $"订单: 请在第: {currentOrder.turnsToComplete}周期结束前\n";
        info += $"提供资源:\n";
        foreach (var resSet in currentOrder.fulRequiredResources)
        {
            if (resSet != null && resSet.requirements != null)
            {
                foreach (var req in resSet.requirements)
                {
                    info += $"- {req.type}: {req.amount}单位\n";
                }
            }
        }
        
        orderInfoText.text = info;
    }
}
