using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class BubbleAutoResize : MonoBehaviour
{
    [Header("Refs")]
    public TextMeshProUGUI tmp;             // Text
    public RectTransform background;        // Khung nền (9-sliced)

    [Header("Layout")]
    public Vector2 padding = new(64, 40);   // (trái+phải, trên+dưới)
    public Vector2 minSize = new(200, 120); // Kích thước tối thiểu
    public float maxWidth = 600f;           // Bề rộng tối đa trước khi wrap

    void Reset()
    {
        if (!tmp) tmp = GetComponentInChildren<TextMeshProUGUI>(true);
        if (!background && transform.childCount > 0)
            background = transform.GetChild(0).GetComponent<RectTransform>();
    }

    void OnEnable() => UpdateBubble();

    [ContextMenu("Update Bubble Now")]
    public void UpdateBubble()
    {
        if (!tmp || !background) return;

        tmp.enableWordWrapping = true;

        // 1) Giới hạn width: nhỏ nhất (min - padding) và không vượt quá maxWidth
        float targetTextWidth = Mathf.Clamp(
            tmp.GetPreferredValues(tmp.text, Mathf.Infinity, 0).x,
            Mathf.Max(10f, minSize.x - padding.x),
            maxWidth
        );

        // 2) Lấy kích thước ưa thích của text khi bị khóa width
        Vector2 pref = tmp.GetPreferredValues(tmp.text, targetTextWidth, 0);
        float textW = targetTextWidth;
        float textH = pref.y;

        // 3) Áp width cho Text để nó wrap đúng
        tmp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textW);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tmp.rectTransform);

        // 4) Resize background theo text + padding
        float bgW = Mathf.Max(minSize.x, textW + padding.x);
        float bgH = Mathf.Max(minSize.y, textH + padding.y);

        background.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bgW);
        background.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bgH);
    }

    public void SetText(string s)
    {
        if (!tmp) return;
        tmp.text = s;
        UpdateBubble();
    }
    public string strTest;
    private void OnValidate()
    {
        SetText(strTest);
    }
}
