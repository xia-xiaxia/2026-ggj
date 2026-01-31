using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 自动调整 Vertical Layout Group 高度
/// 根据子元素数量自动计算并设置容器高度
/// </summary>
public class VerticalLayoutAutoHeight : MonoBehaviour
{
    private VerticalLayoutGroup verticalLayout;
    private RectTransform rectTransform;
    private int lastChildCount = 0;

    void Start()
    {
        verticalLayout = GetComponent<VerticalLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (verticalLayout == null || rectTransform == null)
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
        Vector2 sizeDelta = rectTransform.sizeDelta;
        if (childCount == 0)
        {
            sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y = 0;
            rectTransform.sizeDelta = sizeDelta;
            return;
        }

        // 获取配置
        float spacing = verticalLayout.spacing;
        float paddingTop = verticalLayout.padding.top;
        float paddingBottom = verticalLayout.padding.bottom;

        // 计算所有子元素的总高度
        float totalHeight = paddingTop + paddingBottom;
        totalHeight += (spacing * (childCount - 1)); // 间距

        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child != null)
            {
                totalHeight += child.rect.height;
            }
        }

        // 设置高度
        sizeDelta = rectTransform.sizeDelta;
        sizeDelta.y = totalHeight;
        rectTransform.sizeDelta = sizeDelta;
    }
}
