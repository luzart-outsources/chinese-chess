using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening; // <-- QUAN TRỌNG: dùng DOTween

/// <summary>
/// Struct chứa sprites cho một màu quân cờ (đỏ hoặc đen)
/// </summary>
[System.Serializable]
public struct ColorPieceSprites
{
    public Sprite rook, knight, cannon, elephant, advisor, general, soldier;
}

/// <summary>
/// Config chứa sprites cho cả hai màu quân cờ
/// </summary>
[System.Serializable]
public class PieceSpriteConfig
{
    public ColorPieceSprites red, black;
}

/// <summary>
/// Component UI hiển thị một quân cờ trên bàn cờ
/// Xử lý việc hiển thị sprite, animation chọn/bỏ chọn, và input của người chơi
/// </summary>
public class ChineseChessPieceView : PieceView
{
    [Header("UI Refs")]
    [SerializeField] private Image baseImage;   // Ảnh đế/nền của quân cờ
    [SerializeField] private Image pieceImage;  // Ảnh chính của quân cờ
    [SerializeField] private Button button;     // Button component (tùy chọn)

    [Header("Sprite Config (prefab tự chứa)")]
    [SerializeField] private PieceSpriteConfig spriteConfig;

    [Header("Select FX (DOTween)")]
    [SerializeField] private float selectedScale = 1.08f; // Tỉ lệ phóng to khi được chọn (8%)
    [SerializeField] private float scaleAnimTime = 0.08f; // Thời gian animation scale
    [SerializeField] private Ease scaleEase = Ease.OutCubic;

    // Trạng thái chọn và animation
    private bool isSelected = false;
    private Vector3 baseScale = Vector3.one;
    private Tween scaleTween; // DOTween tween cho scale animation

    /// <summary>
    /// Khởi tạo các component và thiết lập sự kiện
    /// Tự động tìm các Image component nếu chưa được gán
    /// </summary>
    private void Awake()
    {
        // Thiết lập button onClick event
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        // Lưu scale gốc để dùng cho animation
        baseScale = transform.localScale;
    }

    /// <summary>
    /// Dọn dẹp tween khi object bị disable hoặc destroy
    /// Tránh lỗi khi tween vẫn chạy sau khi object không còn tồn tại
    /// </summary>
    private void OnDisable()
    {
        if (scaleTween != null && scaleTween.IsActive())
            scaleTween.Kill();
    }


    // ========== DATA & RENDER ==========

    /// <summary>
    /// Khởi tạo dữ liệu cho quân cờ và render lần đầu
    /// </summary>
    /// <param name="id">ID duy nhất của quân cờ</param>
    /// <param name="type">Loại quân cờ</param>
    /// <param name="isRed">True nếu là quân đỏ, false nếu là quân đen</param>
    /// <param name="isShow">True nếu quân đã được lật (hiện), false nếu còn úp</param>
    public override void InitData(int id, PieceType type, bool isRed, bool isShow)
    {
        base.InitData(id, type, isRed, isShow);
    }

    /// <summary>
    /// Thiết lập sprite trực tiếp cho ảnh quân cờ
    /// </summary>
    /// <param name="s">Sprite cho quân cờ</param>
    public void SetPieceSprite(Sprite s)
    {
        if (!pieceImage) return;
        pieceImage.sprite = s;
        pieceImage.enabled = (s != null);
    }

    /// <summary>
    /// Thiết lập khả năng tương tác của quân cờ
    /// </summary>
    /// <param name="canInteract">True nếu có thể click, false nếu không</param>
    public override void SetInteractable(bool canInteract)
    {
        if (button) button.interactable = canInteract;
        if (pieceImage) pieceImage.raycastTarget = canInteract;
        if (baseImage) baseImage.raycastTarget = canInteract;
    }

    /// <summary>
    /// Cập nhật hiển thị visual dựa trên dữ liệu hiện tại
    /// Xử lý logic hiển thị quân úp/ngửa và chọn sprite phù hợp
    /// </summary>
    protected override void RefreshVisual()
    {
        if (!pieceImage) return;

        // Nếu quân còn úp, ẩn sprite
        if (!isShow)
        {
            pieceImage.sprite = null;
            pieceImage.enabled = false;
            return;
        }

        // Nếu quân đã ngửa, hiển thị sprite tương ứng
        Sprite front = GetPieceSprite(type, isRed);
        pieceImage.sprite = front;
        pieceImage.enabled = (front != null);
    }

    /// <summary>
    /// Lấy sprite phù hợp dựa trên loại quân và màu
    /// </summary>
    /// <param name="t">Loại quân cờ</param>
    /// <param name="redSide">True nếu quân đỏ, false nếu quân đen</param>
    /// <returns>Sprite tương ứng hoặc null nếu không tìm thấy</returns>
    private Sprite GetPieceSprite(PieceType t, bool redSide)
    {
        if (spriteConfig == null) return null;
        var set = redSide ? spriteConfig.red : spriteConfig.black;
        return t switch
        {
            PieceType.Xe => set.rook,
            PieceType.Ma => set.knight,
            PieceType.Phao => set.cannon,
            PieceType.Tuong => set.elephant,
            PieceType.Si => set.advisor,
            PieceType.Vua => set.general,
            PieceType.Tot => set.soldier,
            _ => null
        };
    }

    // ========== CHỌN / BỎ CHỌN (DOTween) ==========

    /// <summary>
    /// Thiết lập trạng thái chọn của quân cờ với animation scale
    /// </summary>
    /// <param name="selected">True nếu được chọn, false nếu bỏ chọn</param>
    /// <param name="instant">True nếu không cần animation, false nếu có animation</param>
    public override void SetSelected(bool selected, bool instant = false)
    {
        // Không làm gì nếu trạng thái không thay đổi (trừ khi instant = true)
        if (isSelected == selected && !instant) return;
        isSelected = selected;

        // Tính toán scale đích: phóng to nếu được chọn, về bình thường nếu bỏ chọn
        Vector3 target = isSelected ? baseScale * selectedScale : baseScale;

        // Dừng tween cũ để tránh conflict
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();

        // Nếu instant hoặc không có thời gian animation, set trực tiếp
        if (instant || scaleAnimTime <= 0f)
        {
            transform.localScale = target;
            return;
        }

        // Tạo tween scale với thời gian và easing
        scaleTween = transform
            .DOScale(target, scaleAnimTime)
            .SetEase(scaleEase)
            .SetUpdate(true); // Chạy ngay cả khi Time.timeScale = 0 (game pause)
    }

    // ========== INPUT ==========

    /// <summary>
    /// Xử lý khi button được click (nếu có button component)
    /// </summary>
    public void OnClick() => controller?.OnPieceClickedView(this);
}
