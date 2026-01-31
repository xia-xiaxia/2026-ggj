using UnityEngine;
using UnityEngine.InputSystem;

public class PlaceManager : MonoBehaviour
{
    public Transform grid;

    private Vector2Int mouseCoord;
    private Vector2 curScreenPos;
    private Vector2 clickStartScreenPos;
    private float maxClickMoveDistance = 10f;


    public void Pointer(InputAction.CallbackContext context)
    {
        if (PlaceItemListManager.GetInstance().selectedFactory == null)
            return;
        curScreenPos = context.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(curScreenPos);
        Plane plane = new(grid.up, grid.position);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 worldPos = ray.GetPoint(enter);
            Vector3 localPos = grid.InverseTransformPoint(worldPos);
            Vector3 currentGridLocalPosition = localPos;
            mouseCoord = new(Mathf.FloorToInt(currentGridLocalPosition.x), Mathf.FloorToInt(currentGridLocalPosition.z));
            var width = GridManager.GetInstance().width;
            var height = GridManager.GetInstance().height;
            var previewObj = PlaceItemListManager.GetInstance().selectedFactory.previewObj;
            var material = previewObj.GetComponent<MeshRenderer>().material;
            var size = PlaceItemListManager.GetInstance().selectedFactory.size;
            if (mouseCoord.x >= 0 && mouseCoord.x < width && mouseCoord.y >= 0 && mouseCoord.y < height)
            {
                previewObj.gameObject.SetActive(true);
                int left = mouseCoord.x - Mathf.FloorToInt(size.x / 2);
                int right = left + size.x - 1;
                int bottom = mouseCoord.y - Mathf.FloorToInt(size.z / 2);
                int top = bottom + size.z - 1;
                previewObj.transform.position = grid.TransformPoint(new Vector3(left + size.x / 2f, size.y / 2f, bottom + size.z / 2f));
                if (Check(left, right, bottom, top))
                    material.color = Color.green;
                else
                    material.color = Color.red;
            }
            else
            {
                previewObj.gameObject.SetActive(false);
            }
        }
    }
    public void Place(InputAction.CallbackContext context)
    {
        if (PlaceItemListManager.GetInstance().selectedFactory == null)
            return;
        if (context.performed)
        {
            clickStartScreenPos = curScreenPos;
            return;
        }
        else if (context.canceled)
        {
            if (Vector2.Distance(clickStartScreenPos, curScreenPos) > maxClickMoveDistance)
                return;

            var previewObj = PlaceItemListManager.GetInstance().selectedFactory.previewObj;
            var size = PlaceItemListManager.GetInstance().selectedFactory.size;
            var placePrefab = PlaceItemListManager.GetInstance().selectedFactory.factoryPrefab;
            int left = mouseCoord.x - Mathf.FloorToInt(size.x / 2f);
            int right = left + size.x - 1;
            int bottom = mouseCoord.y - Mathf.FloorToInt(size.z / 2f);
            int top = bottom + size.z - 1;
            if (!Check(left, right, bottom, top))
                return;
            Instantiate(placePrefab, previewObj.transform.position, previewObj.transform.rotation);

            for (int x = left; x <= right; x++)
                for (int y = bottom; y <= top; y++)
                    GridManager.GetInstance().gridUsage[x][y] = GridManager.CellUsage.Building;
        }
    }
    private bool Check(int left, int right, int bottom, int top)
    {
        int width = GridManager.GetInstance().width;
        int height = GridManager.GetInstance().height;
        if (left < 0 || right >= width || bottom < 0 || top >= height)
            return false;
        for (int x = left; x <= right; x++)
        {
            for (int y = bottom; y <= top; y++)
            {
                if (GridManager.GetInstance().gridUsage[x][y] != GridManager.CellUsage.Empty)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
