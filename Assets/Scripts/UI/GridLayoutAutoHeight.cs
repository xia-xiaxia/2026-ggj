using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 自动调整 Grid Layout 高度
/// 根据子元素数量自动计算并设置容器高度
/// </summary>
public class GridLayoutAutoHeight : MonoBehaviour
{
    private GridLayoutGroup gridLayout;
    private RectTransform rectTransform;
    private int lastChildCount = 0;

    void Start()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (gridLayout == null || rectTransform == null)
            return;

        // 只在子元素数量改变时计算
        int currentChildCount = transform.childCount;
        if (currentChildCount == lastChildCount)
            return;

        lastChildCount = currentChildCount;
        UpdateHeight();
    }

    void UpdateHeight()
    {
        int childCount = transform.childCount;
        if (childCount == 0)
            return;

        // 获取 Grid Layout 的配置
        float cellHeight = gridLayout.cellSize.y;
        float spacingY = gridLayout.spacing.y;
        float paddingTop = gridLayout.padding.top;
        float paddingBottom = gridLayout.padding.bottom;

        // 根据 Constraint 计算行数
        int rows = 1;
        if (gridLayout.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            int columns = gridLayout.constraintCount;
            rows = Mathf.CeilToInt((float)childCount / columns);
        }
        else if (gridLayout.constraint == GridLayoutGroup.Constraint.FixedRowCount)
        {
            rows = gridLayout.constraintCount;
        }

        // 计算所需高度
        float requiredHeight = (rows * cellHeight) + ((rows - 1) * spacingY) + paddingTop + paddingBottom;

        // 设置高度
        Vector2 sizeDelta = rectTransform.sizeDelta;
        sizeDelta.y = requiredHeight;
        rectTransform.sizeDelta = sizeDelta;
    }
}
