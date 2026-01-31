using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [Header("网格大小")]
    public int width = 64;
    public int height = 64;
    public float cellSize = 1f;

    [Header("材质")]
    public Color lineColor = Color.white;
    public Material lineMaterial; // 指定用于运行时渲染线的材质

    [Header("网格组件")]
    public Transform grid;
    public Transform crossMarkerParent;
    public MeshRenderer mr;
    public MeshFilter mf;

    public enum CellUsage
    {
        Empty,
        Obstacle,
        Walkway,
        Building
    }
    [Header("网格使用情况")]
    public List<List<CellUsage>> gridUsage;

    [Header("地形检测层级")]
    public LayerMask groundLayer;

    [Header("预制体")]
    public GameObject crossMarkerPrefab;



    private void Start()
    {
        GenerateGrid();
        InitGridUsage();
        StartCoroutine(SetObstacle());
    }
    private void GenerateGrid()
    {
        Mesh gridMesh = new() { name = "GridMesh" };
        var verts = new List<Vector3>();
        var indices = new List<int>();
        Vector3 origin = Vector3.zero;
        for (int x = 0; x <= width; x++) // 垂直线
        {
            Vector3 a = origin + new Vector3(x * cellSize, 0f, 0f);
            Vector3 b = origin + new Vector3(x * cellSize, 0f, height * cellSize);
            int baseIndex = verts.Count;
            verts.Add(a);
            verts.Add(b);
            indices.Add(baseIndex);
            indices.Add(baseIndex + 1);
        }
        for (int y = 0; y <= height; y++) // 水平线
        {
            Vector3 a = origin + new Vector3(0f, 0f, y * cellSize);
            Vector3 b = origin + new Vector3(width * cellSize, 0f, y * cellSize);
            int baseIndex = verts.Count;
            verts.Add(a);
            verts.Add(b);
            indices.Add(baseIndex);
            indices.Add(baseIndex + 1);
        }
        gridMesh.SetVertices(verts);
        gridMesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        gridMesh.RecalculateBounds();

        mf.sharedMesh = gridMesh;
        mr.sharedMaterial.SetColor("_Color", lineColor);
    }
    private void InitGridUsage()
    {
        gridUsage = new List<List<CellUsage>>(width);

        bool[] leftCol = new bool[height + 1];
        for (int y = 0; y <= height; y++)
            leftCol[y] = IsWalkable(new Vector3(0f, 0f, y * cellSize));
        for (int x = 1; x <= width; x++)
        {
            var colCellUsage = new List<CellUsage>(height);
            bool colUnder = IsWalkable(new Vector3(x * cellSize, 0f, 0f));
            for (int y = 1; y <= height; y++)
            {
                bool curCorner = IsWalkable(new Vector3(x * cellSize, 0f, y * cellSize));
                int cornerCount = (curCorner ? 1 : 0)
                                + (colUnder ? 1 : 0)
                                + (leftCol[y - 1] ? 1 : 0)
                                + (leftCol[y] ? 1 : 0);
                if (cornerCount >= 3)
                    colCellUsage.Add(CellUsage.Empty);
                else
                    colCellUsage.Add(CellUsage.Obstacle);
                leftCol[y - 1] = colUnder;
                colUnder = curCorner;
            }
            leftCol[height] = colUnder;
            gridUsage.Add(colCellUsage);
        }
    }
    private bool IsWalkable(Vector3 meshLocalPos)
    {
        Vector3 worldPoint = grid.TransformPoint(meshLocalPos);
        float rayHeight = 50f;
        float sphereRadius = 0.1f;
        Ray ray = new (worldPoint + Vector3.up * rayHeight, Vector3.down);
        if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, rayHeight * 2f, groundLayer))
                return true;
        return false;
    }
    private IEnumerator SetObstacle()
    {
        while (crossMarkerParent.childCount > 0)
        {
            GameObject child = crossMarkerParent.GetChild(0).gameObject;
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
            yield return null;
        }
        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(gridUsage[x][y] == CellUsage.Obstacle)
                {
                    Vector3 pos = new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);
                    Instantiate(crossMarkerPrefab, crossMarkerParent.TransformPoint(pos), Quaternion.identity, crossMarkerParent);
                }
            }
            yield return null;
        }
    }
    //private void OnValidate()
    //{
    //    width = Mathf.Max(0, width);
    //    height = Mathf.Max(0, height);
    //    cellSize = Mathf.Max(0.001f, cellSize);

    //    if (Application.isPlaying == false)
    //    {
    //        GenerateGrid();
    //        InitGridUsage();
    //    }
    //}
}
