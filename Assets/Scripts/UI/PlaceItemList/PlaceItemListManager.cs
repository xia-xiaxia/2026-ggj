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
    }
}
