using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceItemListManager : Singleton<PlaceItemListManager>
{
    public Transform grid;
    public List<Transform> factoryTypes;
    public List<GameObject> Items;
    public GameObject walkwaySelectedFrame;
    public GameObject factorySelectedFrame;
    public GameObject deleteSelectedFrame;
    public GameObject needWalkwayBeside;
    public GameObject needLakeBeside;
    public GameObject needNoLakeBeside;
    public Transform broadcastParent;
    public GameObject broadcastPrefab;

    public FactoryUIItem selectedFactory;
    public enum placeMode
    {
        Place,
        Delete,
        Input
    }
    public placeMode curPlaceMode = placeMode.Input;

    private int curFactoryTypeId = -1;



    private void Start()
    {
        foreach (var fac in factoryTypes)
            fac.localScale = Vector3.one;
        foreach (var item in Items)
            item.SetActive(false);

        UpdateModeInfo();
    }

    private void UpdateModeInfo()
    {
        if (GameManager.Instance != null)
        {
            string info = string.Empty;
            if (curPlaceMode == placeMode.Place && selectedFactory != null)
            {
                string desc = GetSelectedFactoryDescription();
                string costInfo = GetSelectedFactoryBuildCosts();
                if (!string.IsNullOrEmpty(desc))
                {
                    info = desc;
                }
                if (!string.IsNullOrEmpty(costInfo))
                {
                    if (!string.IsNullOrEmpty(info))
                        info += "\n";
                    info += costInfo;
                }
            }
            GameManager.Instance.SetInfoText(info);
        }
    }

    private string GetSelectedFactoryDescription()
    {
        if (selectedFactory == null || selectedFactory.factoryPrefab == null)
            return null;

        string prefabName = selectedFactory.factoryPrefab.name;
        if (prefabName == "Walkway")
            return null;

        FactoryInfoData info = GetSelectedFactoryInfo();
        if (info != null)
        {
            string desc = info.factoryName;
            if (!string.IsNullOrEmpty(info.description))
            {
                desc += $"\n{info.description}";
            }
            if (!string.IsNullOrEmpty(info.remarks))
            {
                desc += $"\n备注：{info.remarks}";
            }
            return desc;
        }

        return prefabName;
    }

    private string GetSelectedFactoryBuildCosts()
    {
        if (selectedFactory == null || selectedFactory.factoryPrefab == null)
            return null;

        string prefabName = selectedFactory.factoryPrefab.name;
        if (prefabName == "Walkway")
            return null;

        FactoryInfoData info = GetSelectedFactoryInfo();
        if (info == null)
            return null;

        if (info.buildCosts == null || info.buildCosts.Count == 0)
            return "建造材料：无";

        string costInfo = "建造材料：";
        for (int i = 0; i < info.buildCosts.Count; i++)
        {
            var cost = info.buildCosts[i];
            costInfo += $"{cost.resourceType} x{cost.quantity}";
            if (i < info.buildCosts.Count - 1)
                costInfo += "，";
        }
        return costInfo;
    }

    private FactoryInfoData GetSelectedFactoryInfo()
    {
        if (selectedFactory == null || selectedFactory.factoryPrefab == null)
            return null;

        var factory = selectedFactory.factoryPrefab.GetComponent<TurnBasedFactory>();
        if (factory != null)
        {
            string typeKey = factory.factoryType.ToString();
            if (!string.IsNullOrEmpty(typeKey))
            {
                var infoByType = FactoryDatabaseLoader.GetFactory(typeKey);
                if (infoByType != null)
                    return infoByType;
            }
            if (!string.IsNullOrEmpty(factory.factoryName))
            {
                var infoByName = FactoryDatabaseLoader.GetFactoryByName(factory.factoryName);
                if (infoByName != null)
                    return infoByName;
            }
        }

        string prefabName = selectedFactory.factoryPrefab.name;
        if (!string.IsNullOrEmpty(prefabName))
        {
            var infoByPrefab = FactoryDatabaseLoader.GetFactoryByName(prefabName);
            if (infoByPrefab != null)
                return infoByPrefab;
        }

        return null;
    }
    public void OnFactoryTypeButtonClicked(int id)
    {
        if (id == curFactoryTypeId)
            return;
        if (curFactoryTypeId != -1)
        {
            factoryTypes[curFactoryTypeId].DOScale(1f, 0.2f);
            Items[curFactoryTypeId].SetActive(false);
            Transform content = Items[curFactoryTypeId].transform.Find("Scroll View/Viewport/Content");
            for (int i = 0; i < content.childCount; i++)
            {
                FactoryUIItem item = content.GetChild(i).GetComponent<FactoryUIItem>();
                item.selectedFrame.SetActive(false);
            }
        }
        factoryTypes[id].DOScale(1.2f, 0.3f);
        Items[id].SetActive(true);
        curFactoryTypeId = id;
        UpdateSelectedFactory(null);
    }
    public void UpdateSelectedFactory(FactoryUIItem fac)
    {
        if (selectedFactory != null)
        {
            selectedFactory.selectedFrame.SetActive(false);
            selectedFactory.previewObj.SetActive(false);
            if (fac == selectedFactory && selectedFactory.need != FactoryUIItem.PlaceNeed.None) // 如果是道路，再次点击不会取消
            {
                selectedFactory = null;
                curPlaceMode = placeMode.Input;
                needWalkwayBeside.SetActive(false);
                needLakeBeside.SetActive(false);
                needNoLakeBeside.SetActive(false);
                UpdateModeInfo();
                return;
            }
        }
        selectedFactory = fac;
        if (selectedFactory != null)
        {
            selectedFactory.selectedFrame.SetActive(true);
            selectedFactory.factoryPrefab.SetActive(true);
            switch (selectedFactory.need)
            {
                case FactoryUIItem.PlaceNeed.WalkwayBeside:
                    needWalkwayBeside.SetActive(true);
                    needLakeBeside.SetActive(false);
                    needNoLakeBeside.SetActive(false);
                    break;
                case FactoryUIItem.PlaceNeed.None:
                    needWalkwayBeside.SetActive(false);
                    needLakeBeside.SetActive(false);
                    needNoLakeBeside.SetActive(false);
                    break;
                case FactoryUIItem.PlaceNeed.LakeBeside:
                    needWalkwayBeside.SetActive(true);
                    needLakeBeside.SetActive(true);
                    needNoLakeBeside.SetActive(false);
                    break;
                case FactoryUIItem.PlaceNeed.NoLakeBeside:
                    needWalkwayBeside.SetActive(true);
                    needLakeBeside.SetActive(false);
                    needNoLakeBeside.SetActive(true);
                    break;
            }
            OnFactoryButtonClicked();
        }
    }
    public void Broadcast(string broadcast)
    {
        GameObject go = Instantiate(broadcastPrefab, broadcastParent);
        go.GetComponent<TextMeshProUGUI>().text = broadcast;
        DOTween.Sequence()
            .AppendInterval(1f)
            .Append(go.GetComponent<TextMeshProUGUI>().DOFade(0f, 0.5f))
            .AppendCallback(() => Destroy(go));
    }
    public void OnVisibleButtonClicked()
    {
        grid.gameObject.SetActive(!grid.gameObject.activeSelf);
    }
    public void OnWalkwayButtonClicked()
    {
        walkwaySelectedFrame.SetActive(true);
        factorySelectedFrame.SetActive(false);
        deleteSelectedFrame.SetActive(false);
        curPlaceMode = placeMode.Place;
        UpdateModeInfo();
    }
    public void OnFactoryButtonClicked()
    {
        walkwaySelectedFrame.SetActive(false);
        factorySelectedFrame.SetActive(true);
        deleteSelectedFrame.SetActive(false);
        curPlaceMode = placeMode.Place;
        if (selectedFactory == null)
        {
            needWalkwayBeside.SetActive(false);
            needLakeBeside.SetActive(false);
            needNoLakeBeside.SetActive(false);
        }
        UpdateModeInfo();
    }
    public void OnDeleteButtonClicked()
    {
        walkwaySelectedFrame.SetActive(false);
        factorySelectedFrame.SetActive(false);
        deleteSelectedFrame.SetActive(true);
        curPlaceMode = placeMode.Delete;
        UpdateSelectedFactory(null);
        needWalkwayBeside.SetActive(false);
        needLakeBeside.SetActive(false);
        needNoLakeBeside.SetActive(false);
        UpdateModeInfo();
    }
}
