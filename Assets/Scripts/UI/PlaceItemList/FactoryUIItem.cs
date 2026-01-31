using UnityEngine;

public class FactoryUIItem : MonoBehaviour
{
    public GameObject factoryPrefab;
    public GameObject previewObj;
    public Vector3Int size;
    public GameObject selectedFrame;
    


    public void OnItemClicked()
    {
        PlaceItemListManager.GetInstance().UpdateSelectedFactory(this);
    }
}
