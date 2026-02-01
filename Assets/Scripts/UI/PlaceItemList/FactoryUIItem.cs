using UnityEngine;

public class FactoryUIItem : MonoBehaviour
{
    public GameObject factoryPrefab;
    public GameObject previewObj;
    public Vector2Int size;
    public float height;
    public GameObject selectedFrame;
    public enum PlaceNeed
    {
        WalkwayBeside,
        None,
        LakeBeside,
        NoLakeBeside
    }
    public PlaceNeed need;



    public void OnItemClicked()
    {
        PlaceItemListManager.GetInstance().UpdateSelectedFactory(this);
    }
}
