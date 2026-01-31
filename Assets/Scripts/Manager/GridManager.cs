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



    private void Start()
    {
        GenerateGrid();
        InitGridUsage();
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
        for (int x = 0; x < width; x++)
        {
            var col = new List<CellUsage>(height);
            for (int y = 0; y < height; y++)
            {
                col.Add(CellUsage.Empty);
            }
            gridUsage.Add(col);
        }
    }
    private void OnValidate()
    {
        width = Mathf.Max(0, width);
        height = Mathf.Max(0, height);
        cellSize = Mathf.Max(0.001f, cellSize);

        if (Application.isPlaying == false)
            GenerateGrid();
    }
}
