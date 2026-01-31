using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;

public class PlaceItemListManager : Singleton<PlaceItemListManager>
{
    public List<Transform> factoryTypes;
    public List<GameObject> Items;
    public FactoryUIItem selectedFactory;
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
            if (fac == selectedFactory)
                return;
            selectedFactory.selectedFrame.SetActive(false);
            selectedFactory.previewObj.SetActive(false);
        }
        selectedFactory = fac;
        if (selectedFactory != null)
        {
            selectedFactory.selectedFrame.SetActive(true);
            selectedFactory.factoryPrefab.SetActive(true);
        }
    }
}
