using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class PlaceManager : MonoBehaviour
{
    public Transform grid;

    private Vector2Int mouseCoord;
    private Vector2 curScreenPos;
    private Vector2 clickStartScreenPos;
    private float maxClickMoveDistance = 10f;
    private GameObject toSelectObj;



    public void Pointer(InputAction.CallbackContext context)
    {
        var placeItemListManager = PlaceItemListManager.GetInstance();
        curScreenPos = context.ReadValue<Vector2>();
        if (placeItemListManager.selectedFactory == null)
            return;
        Ray ray = Camera.main.ScreenPointToRay(curScreenPos);
        Plane plane = new(grid.up, grid.position);
        if (plane.Raycast(ray, out float enter))
        {
            if (IsPointerOverUI(curScreenPos)) // UI遮挡检测
            {
                var previewObjUI = placeItemListManager.selectedFactory.previewObj;
                if (previewObjUI != null)
                    previewObjUI.gameObject.SetActive(false);
                return;
            }
            Vector3 worldPos = ray.GetPoint(enter);
            Vector3 localPos = grid.InverseTransformPoint(worldPos);
            Vector3 currentGridLocalPosition = localPos;
            mouseCoord = new(Mathf.FloorToInt(currentGridLocalPosition.x), Mathf.FloorToInt(currentGridLocalPosition.z));
            var width = GridManager.GetInstance().width;
            var height = GridManager.GetInstance().height;
            var previewObj = placeItemListManager.selectedFactory.previewObj;
            var size = placeItemListManager.selectedFactory.size;
            var hght = placeItemListManager.selectedFactory.height;
            var material = previewObj.GetComponentInChildren<MeshRenderer>(true).material;
            if (mouseCoord.x >= 0 && mouseCoord.x < width && mouseCoord.y >= 0 && mouseCoord.y < height)
            {
                previewObj.gameObject.SetActive(true);
                int left = mouseCoord.x - Mathf.FloorToInt(size.x / 2);
                int right = left + size.x - 1;
                int bottom = mouseCoord.y - Mathf.FloorToInt(size.y / 2);
                int top = bottom + size.y - 1;
                previewObj.transform.position = grid.TransformPoint(new Vector3(left + size.x / 2f, hght / 2f, bottom + size.y / 2f));
                if (Check(left, right, bottom, top, PlaceItemListManager.GetInstance().selectedFactory.need))
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
        if (IsPointerOverUI(curScreenPos))
            return;
        var placeItemListManager = PlaceItemListManager.GetInstance();
        if (placeItemListManager == null)
            return;

        if (context.performed)
        {
            clickStartScreenPos = curScreenPos;
            // 删除预览
            if (placeItemListManager.curPlaceMode == PlaceItemListManager.placeMode.Delete)
            {
                int factoryLayer = LayerMask.NameToLayer("Factory");
                Ray ray = Camera.main.ScreenPointToRay(curScreenPos);
                int layerMask = 1 << factoryLayer;
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    toSelectObj = hit.collider.gameObject;
                    var material = toSelectObj.GetComponentInChildren<Renderer>(false).material;
                    material.color = Color.red;
                }
                return;
            }
            // 投料预览
            else if (placeItemListManager.curPlaceMode == PlaceItemListManager.placeMode.Input)
            {
                int factoryLayer = LayerMask.NameToLayer("Factory");
                Ray ray = Camera.main.ScreenPointToRay(curScreenPos);
                int layerMask = 1 << factoryLayer;
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    if (hit.collider.gameObject.name == "Walkway")
                        return;
                    toSelectObj = hit.collider.gameObject;
                    var material = toSelectObj.GetComponentInChildren<Renderer>(false).material;
                    material.color = Color.yellow;
                }
                return;
            }
            return;
        }
        else if (context.canceled)
        {
            if (toSelectObj != null)
            {
                var material = toSelectObj.GetComponentInChildren<Renderer>().material;
                material.color = Color.white;
            }
            var _toSelectObj = toSelectObj;
            toSelectObj = null;
            if (Vector2.Distance(clickStartScreenPos, curScreenPos) > maxClickMoveDistance)
                return;
            // 删除模式
            if (placeItemListManager.curPlaceMode == PlaceItemListManager.placeMode.Delete && _toSelectObj != null)
            {
                int factoryLayer = LayerMask.NameToLayer("Factory");
                Ray ray = Camera.main.ScreenPointToRay(curScreenPos);
                int layerMask = 1 << factoryLayer;
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    GameObject target = hit.collider.gameObject;
                    if (target != _toSelectObj)
                        return;

                    var gm = GridManager.GetInstance();
                    var facGrid = GerFactoryGridByRenderer(target);
                    if (target.name == "Walkway") // 删除道路需要判读有没有依赖
                    {
                        int wx = facGrid[0];
                        int wy = facGrid[2];
                        gm.gridUsage[wx][wy] = GridManager.CellUsage.Empty;

                        bool canDelete = true;
                        (int nx, int ny)[] dirs = { (0, 1), (0, -1), (-1, 0), (1, 0) };
                        foreach (var (dx, dy) in dirs)
                        {
                            int nx = wx + dx;
                            int ny = wy + dy;
                            if (nx < 0 || nx >= gm.width || ny < 0 || ny >= gm.height)
                                continue;
                            if (gm.gridUsage[nx][ny] != GridManager.CellUsage.Factory)
                                continue;

                            // 在该邻格中心做小范围检测以找到工厂根对象（layer 为 Factory）
                            Vector3 cellLocal = new Vector3(nx + 0.5f, 0f, ny + 0.5f);
                            Vector3 worldCenter = grid.TransformPoint(cellLocal);
                            float radius = Mathf.Max(0.1f, gm.cellSize * 0.45f);
                            Collider[] cols = Physics.OverlapSphere(worldCenter, radius);

                            GameObject factoryRoot = null;
                            foreach (var col in cols)
                            {
                                if (col == null) continue;
                                var root = col.transform.root.gameObject;
                                if (root == null) continue;
                                if (factoryLayer == -1 || root.layer != factoryLayer) continue;
                                factoryRoot = root;
                                break;
                            }
                            if (factoryRoot == null)
                            {
                                canDelete = false;
                                break;
                            }

                            // 获取该工厂占用的网格范围
                            var fGrid = GerFactoryGridByRenderer(factoryRoot);
                            int fl = fGrid[0], fr = fGrid[1], fb = fGrid[2], ft = fGrid[3];
                            // 检查该工厂是否仍然满足临路要求
                            if (!CheckNeeds(fl, fr, fb, ft, FactoryUIItem.PlaceNeed.WalkwayBeside))
                            {
                                canDelete = false;
                                break;
                            }
                        }

                        if (!canDelete)
                        {
                            placeItemListManager.Broadcast("无法删除该道路！");
                            gm.gridUsage[wx][wy] = GridManager.CellUsage.Walkway;
                        }
                        else
                        {
                            Debug.Log(wx + "," + wy);
                            Destroy(target);
                        }
                    }
                    else // 删除工厂
                    {
                        for (int x = facGrid[0]; x <= facGrid[1]; x++)
                            for (int y = facGrid[2]; y <= facGrid[3]; y++)
                            {
                                Debug.Log(x + "," + y);
                                gm.gridUsage[x][y] = GridManager.CellUsage.Empty;
                            }
                        Destroy(target);
                    }
                    GameManager.Instance.RemoveFactory(target.GetComponent<TurnBasedFactory>());
                }
                return;
            }
            // 投料模式
            else if (placeItemListManager.curPlaceMode == PlaceItemListManager.placeMode.Input && _toSelectObj != null)
            {
                int factoryLayer = LayerMask.NameToLayer("Factory");
                Ray ray = Camera.main.ScreenPointToRay(curScreenPos);
                int layerMask = 1 << factoryLayer;
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    GameObject target = hit.collider.gameObject;
                    if (target != _toSelectObj)
                        return;
                    // target投料目标
                    var factory = target.GetComponent<TurnBasedFactory>();
                    if (factory == null)
                        return;
                    if (GameManager.Instance != null)
                    {
                        if (!FactoryHasInputs(factory))
                        {
                            factory.Invest();
                            GameManager.Instance.UpdateUI();
                        }
                        else
                        {
                            GameManager.Instance.ShowInvestmentPanel(factory);
                        }
                    }
                    return;
                }
            }
            // 放置模式
            else if (placeItemListManager.curPlaceMode == PlaceItemListManager.placeMode.Place)
            {
                if (placeItemListManager.selectedFactory == null)
                    return;
                var previewObj = placeItemListManager.selectedFactory.previewObj;
                var size = placeItemListManager.selectedFactory.size;
                var hght = placeItemListManager.selectedFactory.height;
                var placePrefab = placeItemListManager.selectedFactory.factoryPrefab;
                int left = mouseCoord.x - Mathf.FloorToInt(size.x / 2f);
                int right = left + size.x - 1;
                int bottom = mouseCoord.y - Mathf.FloorToInt(size.y / 2f);
                int top = bottom + size.y - 1;
                if (!Check(left, right, bottom, top, PlaceItemListManager.GetInstance().selectedFactory.need))
                    return;
            var go = Instantiate(placePrefab, previewObj.transform.position, previewObj.transform.rotation);
                go.name = placePrefab.name;
                go.name = placePrefab.name;
            if (placePrefab.name != "Walkway")
            {
                var factory = go.GetComponent<TurnBasedFactory>();
                if (factory != null && !factory.Build())
                {
                    Destroy(go);
                    return;
                }
            }
            // 放置道路时，更新道路及临路的网格和表现
                if (placeItemListManager.selectedFactory.need == FactoryUIItem.PlaceNeed.None)
                {
                    GridManager.GetInstance().gridUsage[left][bottom] = GridManager.CellUsage.Walkway;
                    SetWalkway(left, bottom, go);
                    // 更新四邻居的道路表现
                    var gm = GridManager.GetInstance();
                    int width = gm.width;
                    int height = gm.height;
                    // 四个方向偏移
                    (int dx, int dy)[] dirs = new (int, int)[] { (0, 1), (0, -1), (-1, 0), (1, 0) };
                    foreach (var (dx, dy) in dirs)
                    {
                        int nx = left + dx;
                        int ny = bottom + dy;
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                            continue;
                        if (gm.gridUsage[nx][ny] != GridManager.CellUsage.Walkway)
                            continue;

                        // 在网格中心位置进行物体查找（用小范围重叠检测）
                        Vector3 cellLocal = new Vector3(nx + 0.5f, 0, ny + 0.5f);
                        Vector3 worldCenter = grid.TransformPoint(cellLocal);
                        float radius = Mathf.Max(0.1f, gm.cellSize * 0.45f);
                        Collider[] cols = Physics.OverlapSphere(worldCenter, radius);
                        GameObject neighborGo = null;
                        int factoryLayer = LayerMask.NameToLayer("Factory");
                        foreach (var col in cols)
                        {
                            if (col == null) continue;
                            // 取根对象，避免取到子碰撞体
                            var root = col.transform.root.gameObject;
                            if (root == null) continue;
                            // 忽略新放置的 go 自身（位置可能重合）
                            if (root == go) continue;
                            if (factoryLayer == -1 || root.layer != factoryLayer) continue;
                            neighborGo = root;
                            break;
                        }
                        if (neighborGo != null)
                            SetWalkway(nx, ny, neighborGo);
                    }
                }
                else
                    for (int x = left; x <= right; x++)
                        for (int y = bottom; y <= top; y++)
                            GridManager.GetInstance().gridUsage[x][y] = GridManager.CellUsage.Factory;
            }
        }
    }
    private bool Check(int left, int right, int bottom, int top, FactoryUIItem.PlaceNeed need)
    {
        int width = GridManager.GetInstance().width;
        int height = GridManager.GetInstance().height;
        var gridUsage = GridManager.GetInstance().gridUsage;
        if (left < 0 || right >= width || bottom < 0 || top >= height)
            return false;
        // 检查区域内是否都为空位
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
        return CheckNeeds(left, right, bottom, top, need);
    }
    private bool CheckNeeds(int left, int right, int bottom, int top, FactoryUIItem.PlaceNeed need)
    {
        int width = GridManager.GetInstance().width;
        int height = GridManager.GetInstance().height;
        var gridUsage = GridManager.GetInstance().gridUsage;
        // 不同工厂不同放置需求
        // 放置道路无需求
        if (need == FactoryUIItem.PlaceNeed.None)
            return true;
        // 临路
        bool isWalkwayBeside = false;
        // 检查上下邻居（在水平范围外侧）
        for (int x = left; x <= right; x++)
        {
            if (isWalkwayBeside)
                break;
            int yBelow = bottom - 1;
            if (yBelow >= 0 && gridUsage[x][yBelow] == GridManager.CellUsage.Walkway)
                isWalkwayBeside = true;
            int yAbove = top + 1;
            if (yAbove < height && gridUsage[x][yAbove] == GridManager.CellUsage.Walkway)
                isWalkwayBeside = true;
        }
        // 检查左右邻居（在垂直范围外侧）
        for (int y = bottom; y <= top; y++)
        {
            if (isWalkwayBeside)
                break;
            int xLeft = left - 1;
            if (xLeft >= 0 && gridUsage[xLeft][y] == GridManager.CellUsage.Walkway)
                isWalkwayBeside = true;
            int xRight = right + 1;
            if (xRight < width && gridUsage[xRight][y] == GridManager.CellUsage.Walkway)
                isWalkwayBeside = true;
        }
        if (!isWalkwayBeside)
            return false;
        else if (need == FactoryUIItem.PlaceNeed.WalkwayBeside)
        {
            return true;
        }
        else if (need == FactoryUIItem.PlaceNeed.LakeBeside)
        {
            // 检查上下邻居（在水平范围外侧）
            for (int x = left; x <= right; x++)
            {
                int yBelow = bottom - 1;
                if (yBelow >= 0 && gridUsage[x][yBelow] == GridManager.CellUsage.Lake)
                    return true;
                int yAbove = top + 1;
                if (yAbove < height && gridUsage[x][yAbove] == GridManager.CellUsage.Lake)
                    return true;
            }
            // 检查左右邻居（在垂直范围外侧）
            for (int y = bottom; y <= top; y++)
            {
                int xLeft = left - 1;
                if (xLeft >= 0 && gridUsage[xLeft][y] == GridManager.CellUsage.Lake)
                    return true;
                int xRight = right + 1;
                if (xRight < width && gridUsage[xRight][y] == GridManager.CellUsage.Lake)
                    return true;
            }
            return false;
        }
        else if (need == FactoryUIItem.PlaceNeed.NoLakeBeside)
        {
            // 检查上下邻居（在水平范围外侧）
            for (int x = left; x <= right; x++)
            {
                int yBelow = bottom - 1;
                if (yBelow >= 0 && gridUsage[x][yBelow] == GridManager.CellUsage.Lake)
                    return false;
                int yAbove = top + 1;
                if (yAbove < height && gridUsage[x][yAbove] == GridManager.CellUsage.Lake)
                    return false;
            }
            // 检查左右邻居（在垂直范围外侧）
            for (int y = bottom; y <= top; y++)
            {
                int xLeft = left - 1;
                if (xLeft >= 0 && gridUsage[xLeft][y] == GridManager.CellUsage.Lake)
                    return false;
                int xRight = right + 1;
                if (xRight < width && gridUsage[xRight][y] == GridManager.CellUsage.Lake)
                    return false;
            }
            return true;
        }
        return false;
    }
    private void SetWalkway(int x, int y, GameObject walkway)
    {
        // 初始方向：1：上下；2：右下；3：上右下；4：上下左右
        var gm = GridManager.GetInstance();
        int width = gm.width;
        int height = gm.height;
        var gridUsage = gm.gridUsage;

        bool hasUp = (y + 1) < height && gridUsage[x][y + 1] == GridManager.CellUsage.Walkway;
        bool hasDown = (y - 1) >= 0 && gridUsage[x][y - 1] == GridManager.CellUsage.Walkway;
        bool hasLeft = (x - 1) >= 0 && gridUsage[x - 1][y] == GridManager.CellUsage.Walkway;
        bool hasRight = (x + 1) < width && gridUsage[x + 1][y] == GridManager.CellUsage.Walkway;

        Transform t1 = walkway.transform.GetChild(0);
        Transform t2 = walkway.transform.GetChild(1);
        Transform t3 = walkway.transform.GetChild(2);
        Transform t4 = walkway.transform.GetChild(3);
        t1.gameObject.SetActive(false);
        t2.gameObject.SetActive(false);
        t3.gameObject.SetActive(false);
        t4.gameObject.SetActive(false);

        int neighborCount = (hasUp ? 1 : 0) + (hasDown ? 1 : 0) + (hasLeft ? 1 : 0) + (hasRight ? 1 : 0);

        Transform active;
        int rotationZ = 0; // 0/90/180/270

        // 四向
        if (hasUp && hasDown && hasLeft && hasRight)
        {
            active = t4;
            rotationZ = 0;
        }
        // 直线（上下）
        else if (hasUp && hasDown && !hasLeft && !hasRight)
        {
            active = t1;
            rotationZ = 0;
        }
        // 直线（左右）
        else if (!hasUp && !hasDown && hasLeft && hasRight)
        {
            active = t1;
            rotationZ = 90;
        }
        // 转角（两边相邻）
        else if (neighborCount == 2 && ((hasRight && hasDown) || (hasDown && hasLeft) || (hasLeft && hasUp) || (hasUp && hasRight)))
        {
            active = t2;
            if (hasRight && hasDown) rotationZ = 0;       // 初始为 右-下
            else if (hasDown && hasLeft) rotationZ = 90;  // 右-下 逆时针 90 -> 下-左
            else if (hasLeft && hasUp) rotationZ = 180;   // 再转 -> 左-上
            else if (hasUp && hasRight) rotationZ = 270;  // 再转 -> 上-右
        }
        // 三向（缺一侧）
        else if (neighborCount == 3)
        {
            active = t3;
            if (!hasLeft) rotationZ = 0;    // 缺左：上-右-下（初始）
            else if (!hasUp) rotationZ = 90;   // 缺上：右-下-左
            else if (!hasRight) rotationZ = 180; // 缺右：下-左-上
            else if (!hasDown) rotationZ = 270;  // 缺下：左-上-右
        }
        // 单邻居或没有邻居：尽量使用直线（t1），并旋转使其朝向邻居（如果只有一个邻居）
        else if (neighborCount == 1)
        {
            active = t1;
            if (hasUp) rotationZ = 0;
            else if (hasRight) rotationZ = 90;
            else if (hasDown) rotationZ = 180;
            else if (hasLeft) rotationZ = 270;
        }
        else
        {
            // 没有邻居且未匹配到其他情形：使用单向默认（若存在）
            active = t1;
            rotationZ = 0;
        }

        if (active != null)
        {
            active.gameObject.SetActive(true);
            active.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        }
    }
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;
        var eventData = new PointerEventData(EventSystem.current) { position = screenPosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    private bool FactoryHasInputs(TurnBasedFactory factory)
    {
        if (factory == null) return false;
        if (factory.recipes == null || factory.recipes.Count == 0) return false;
        var recipe = factory.recipes[factory.currentRecipeIndex];
        return recipe != null && recipe.inputs != null && recipe.inputs.Count > 0;
    }
    private List<int> GerFactoryGridByRenderer(GameObject target)
    {
        var gm = GridManager.GetInstance();
        var renderer = target.GetComponentInChildren<Renderer>();
        // 将包围盒的 min/max 世界坐标转换到 grid 的本地坐标系
        Vector3 localMin = grid.InverseTransformPoint(renderer.bounds.min);
        Vector3 localMax = grid.InverseTransformPoint(renderer.bounds.max);

        int l = Mathf.FloorToInt(localMin.x);
        int r = Mathf.CeilToInt(localMax.x) - 1;
        int b = Mathf.FloorToInt(localMin.z);
        int t = Mathf.CeilToInt(localMax.z) - 1;

        // 限定到网格范围内
        int width = gm.width;
        int height = gm.height;
        l = Mathf.Clamp(l, 0, width - 1);
        r = Mathf.Clamp(r, 0, width - 1);
        b = Mathf.Clamp(b, 0, height - 1);
        t = Mathf.Clamp(t, 0, height - 1);

        var facGrid = new List<int>();
        facGrid.Add(l);
        facGrid.Add(r);
        facGrid.Add(b);
        facGrid.Add(t);

        return facGrid;
    }
}
