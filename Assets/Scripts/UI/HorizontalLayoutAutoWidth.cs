using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 自动调整 Horizontal Layout Group 宽度
/// 根据子元素数量自动计算并设置容器宽度
/// </summary>
public class HorizontalLayoutAutoWidth : MonoBehaviour
{
    private HorizontalLayoutGroup horizontalLayout;
    private RectTransform rectTransform;
    private int lastChildCount = 0;

    void Start()
    {
        horizontalLayout = GetComponent<HorizontalLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (horizontalLayout == null || rectTransform == null)
            return;

        // 只在子元素数量改变时计算
        int currentChildCount = transform.childCount;
        if (currentChildCount == lastChildCount)
            return;

        lastChildCount = currentChildCount;
        UpdateWidth();
    }

    void UpdateWidth()
    {
        int childCount = transform.childCount;
        Vector2 sizeDelta = rectTransform.sizeDelta;
        
        if (childCount == 0)
        {
            sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = 0;
            rectTransform.sizeDelta = sizeDelta;
            return;
        }

        // 获取配置
        float spacing = horizontalLayout.spacing;
        float paddingLeft = horizontalLayout.padding.left;
        float paddingRight = horizontalLayout.padding.right;

        // 计算所有子元素的总宽度
        float totalWidth = paddingLeft + paddingRight;
        totalWidth += (spacing * (childCount - 1)); // 间距

        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child != null)
            {
                totalWidth += child.rect.width;
            }
        }

        // 设置宽度
        sizeDelta.x = totalWidth;
        rectTransform.sizeDelta = sizeDelta;
    }
}
