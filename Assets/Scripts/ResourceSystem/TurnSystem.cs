using UnityEngine;
using System.Collections.Generic;
using System;

/// 回合系统 - 管理游戏回合
public class TurnSystem : MonoBehaviour
{
    public static TurnSystem Instance { get; private set; }

    [Header("回合设置")]
    public int currentTurn = 1;

    // 事件系统
    public event Action OnTurnStarted;
    public event Action OnTurnEnded;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// 开始新回合
    public void StartNewTurn()
    {
        currentTurn++;
        Debug.Log($"========== 第 {currentTurn} 回合开始 ==========");
        OnTurnStarted?.Invoke();
    }

    /// 结束当前回合（手动调用）
    public void EndTurn()
    {
        Debug.Log($"========== 第 {currentTurn} 回合结束 ==========");
        OnTurnEnded?.Invoke();
        StartNewTurn();
    }

    /// 获取当前回合数
    public int GetCurrentTurn()
    {
        return currentTurn;
    }
}
