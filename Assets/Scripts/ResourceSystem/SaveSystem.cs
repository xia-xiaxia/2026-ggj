using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 游戏数据存储系统
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private string savePath;
    private const string SAVE_FOLDER = "/SaveData/";
    private const string RESOURCE_SAVE_FILE = "resources.json";
    private const string GAME_STATE_FILE = "gamestate.json";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Application.persistentDataPath + SAVE_FOLDER;
        InitializeSaveDirectory();
    }

    /// <summary>
    /// 初始化保存目录
    /// </summary>
    void InitializeSaveDirectory()
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log($"创建保存目录: {savePath}");
        }
    }

    /// <summary>
    /// 保存游戏资源
    /// </summary>
    public bool SaveResources()
    {
        try
        {
            if (ResourceManager.Instance == null)
            {
                Debug.LogWarning("ResourceManager 不存在，无法保存资源");
                return false;
            }

            string json = ResourceManager.Instance.ExportToJSON();
            string filePath = savePath + RESOURCE_SAVE_FILE;
            File.WriteAllText(filePath, json);

            Debug.Log($"资源已保存到: {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存资源失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 加载游戏资源
    /// </summary>
    public bool LoadResources()
    {
        try
        {
            string filePath = savePath + RESOURCE_SAVE_FILE;
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"资源文件不存在: {filePath}");
                return false;
            }

            string json = File.ReadAllText(filePath);
            ResourceManager.Instance.ImportFromJSON(json);

            Debug.Log($"资源已从以下位置加载: {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载资源失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 保存游戏状态（包括资源和其他数据）
    /// </summary>
    public bool SaveGameState()
    {
        try
        {
            GameStateData stateData = new GameStateData();
            stateData.saveTime = System.DateTime.Now.ToString();
            stateData.resources = ResourceManager.Instance.GetAllResources();

            string json = JsonUtility.ToJson(stateData, true);
            string filePath = savePath + GAME_STATE_FILE;
            File.WriteAllText(filePath, json);

            Debug.Log($"游戏状态已保存到: {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存游戏状态失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 加载游戏状态
    /// </summary>
    public bool LoadGameState()
    {
        try
        {
            string filePath = savePath + GAME_STATE_FILE;
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"游戏状态文件不存在: {filePath}");
                return false;
            }

            string json = File.ReadAllText(filePath);
            GameStateData stateData = JsonUtility.FromJson<GameStateData>(json);

            // 恢复资源
            foreach (var resource in stateData.resources)
            {
                ResourceManager.Instance.SetResource(resource.Key, resource.Value);
            }

            Debug.Log($"游戏状态已从以下位置加载: {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载游戏状态失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 删除所有保存数据
    /// </summary>
    public bool DeleteAllSaveData()
    {
        try
        {
            if (Directory.Exists(savePath))
            {
                Directory.Delete(savePath, true);
                Debug.Log("所有保存数据已删除");
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"删除保存数据失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 检查是否存在保存文件
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(savePath + GAME_STATE_FILE);
    }

    /// <summary>
    /// 获取保存路径
    /// </summary>
    public string GetSavePath()
    {
        return savePath;
    }

    /// <summary>
    /// 自动保存
    /// </summary>
    public void AutoSave()
    {
        SaveResources();
        SaveGameState();
    }
}

/// <summary>
/// 游戏状态数据
/// </summary>
[System.Serializable]
public class GameStateData
{
    public string saveTime;
    public Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
}
