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
public class ChessPieceView : MonoBehaviour, IPointerClickHandler
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

    // Data runtime của quân cờ
    public int id { get; private set; }
    public PieceType type { get; private set; }
    public bool isRed { get; private set; }
    public bool isShow { get; private set; }

    // Trạng thái chọn và animation
    private bool isSelected = false;
    private Vector3 baseScale = Vector3.one;
    private Tween scaleTween; // DOTween tween cho scale animation

    private BoardController controller;

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

    /// <summary>
    /// Liên kết với BoardController để xử lý input
    /// </summary>
    /// <param name="c">BoardController instance</param>
    public void BindController(BoardController c) => controller = c;

    // ========== DATA & RENDER ==========
    
    /// <summary>
    /// Khởi tạo dữ liệu cho quân cờ và render lần đầu
    /// </summary>
    /// <param name="id">ID duy nhất của quân cờ</param>
    /// <param name="type">Loại quân cờ</param>
    /// <param name="isRed">True nếu là quân đỏ, false nếu là quân đen</param>
    /// <param name="isShow">True nếu quân đã được lật (hiện), false nếu còn úp</param>
    public void InitData(int id, PieceType type, bool isRed, bool isShow)
    {
        this.id = id; this.type = type; this.isRed = isRed; this.isShow = isShow;
        RefreshVisual();
        SetSelected(false, instant: true); // Đảm bảo scale về chuẩn khi spawn
    }

    /// <summary>
    /// Cập nhật loại quân cờ và refresh hiển thị
    /// </summary>
    /// <param name="newType">Loại quân mới</param>
    public void SetType(PieceType newType) { type = newType; RefreshVisual(); }
    
    /// <summary>
    /// Cập nhật màu quân cờ và refresh hiển thị
    /// </summary>
    /// <param name="red">True nếu đỏ, false nếu đen</param>
    public void SetColor(bool red) { isRed = red; RefreshVisual(); }
    
    /// <summary>
    /// Cập nhật trạng thái hiển thị (úp/ngửa) và refresh hiển thị
    /// </summary>
    /// <param name="show">True nếu ngửa, false nếu úp</param>
    public void SetShow(bool show) { isShow = show; RefreshVisual(); }

    /// <summary>
    /// Thiết lập sprite cho ảnh đế/nền
    /// </summary>
    /// <param name="s">Sprite cho đế</param>
    public void SetBaseSprite(Sprite s) { if (baseImage) baseImage.sprite = s; }

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
    public void SetInteractable(bool canInteract)
    {
        if (button) button.interactable = canInteract;
        if (pieceImage) pieceImage.raycastTarget = canInteract;
        if (baseImage) baseImage.raycastTarget = canInteract;
    }

    /// <summary>
    /// Cập nhật hiển thị visual dựa trên dữ liệu hiện tại
    /// Xử lý logic hiển thị quân úp/ngửa và chọn sprite phù hợp
    /// </summary>
    private void RefreshVisual()
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
            PieceType.Rook => set.rook,
            PieceType.Knight => set.knight,
            PieceType.Cannon => set.cannon,
            PieceType.Elephant => set.elephant,
            PieceType.Advisor => set.advisor,
            PieceType.General => set.general,
            PieceType.Soldier => set.soldier,
            _ => null
        };
    }

    // ========== CHỌN / BỎ CHỌN (DOTween) ==========
    
    /// <summary>
    /// Thiết lập trạng thái chọn của quân cờ với animation scale
    /// </summary>
    /// <param name="selected">True nếu được chọn, false nếu bỏ chọn</param>
    /// <param name="instant">True nếu không cần animation, false nếu có animation</param>
    public void SetSelected(bool selected, bool instant = false)
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
    
    /// <summary>
    /// Xử lý pointer click event từ EventSystem
    /// Được gọi khi click trực tiếp vào Image có raycastTarget = true
    /// </summary>
    /// <param name="eventData">Dữ liệu sự kiện pointer</param>
    public void OnPointerClick(PointerEventData eventData) => controller?.OnPieceClickedView(this);
}
