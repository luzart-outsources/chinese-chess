using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Bộ sprite cho 1 màu (White/Black) trong cờ vua.
/// </summary>
[System.Serializable]
public struct ChessColorSprites
{
    public Sprite pawn;   // Soldier
    public Sprite knight; // Knight
    public Sprite bishop; // Elephant
    public Sprite rook;   // Rook
    public Sprite queen;  // Advisor
    public Sprite king;   // General
}

/// <summary>
/// Config sprite cho cả hai màu.
/// </summary>
[System.Serializable]
public class ChessPieceSpriteConfig
{
    public ChessColorSprites white;
    public ChessColorSprites black;
}

/// <summary>
/// View cho cờ vua & cờ vua úp:
/// - Khi isShow=false: chỉ hiện backImage (úp), backImage đổi sprite trắng/đen.
/// - Khi isShow=true : hiện pieceImage theo quân cờ & màu.
/// - Có animation chọn/bỏ chọn bằng DOTween.
/// - Click: gọi BoardController.OnPieceClickedView(this).
/// </summary>
public class ChessPieceView : PieceView, IPointerClickHandler
{
    [Header("UI Refs")]
    [SerializeField] private Image backImage;   // HIỆN khi úp
    [SerializeField] private Image pieceImage;  // HIỆN khi ngửa
    [SerializeField] private Button button;     // tuỳ chọn

    [Header("Back Sprites (face-down)")]
    [Tooltip("Sprite mặt sau cho quân Trắng (isRed==true)")]
    [SerializeField] private Sprite backWhite;
    [Tooltip("Sprite mặt sau cho quân Đen (isRed==false)")]
    [SerializeField] private Sprite backBlack;

    [Header("Piece Sprites (face-up)")]
    [SerializeField] private ChessPieceSpriteConfig chessSprites;

    [Header("Select FX (DOTween)")]
    [SerializeField] private float selectedScale = 1.08f;
    [SerializeField] private float scaleAnimTime = 0.08f;
    [SerializeField] private Ease scaleEase = Ease.OutCubic;

    // Trạng thái chọn & tween
    private bool isSelected = false;
    private Vector3 baseScale = Vector3.one;
    private Tween scaleTween;

    private void Reset()
    {
        // Auto-wire cơ bản nếu thiếu
        if (!pieceImage) pieceImage = GetComponentInChildren<Image>();
        if (!button) button = GetComponent<Button>();
    }

    private void Awake()
    {
        baseScale = transform.localScale;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnDisable()
    {
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
    }

    // ===== Overrides from PieceView =====

    public override void InitData(int id, PieceType type, bool isRed, bool isShow)
    {
        base.InitData(id, type, isRed, isShow);
        // base.InitData đã gọi RefreshVisual + SetSelected(false, instant:true)
    }

    public override void SetType(PieceType newType)
    {
        base.SetType(newType); // gọi RefreshVisual
    }

    public override void SetColor(bool red)
    {
        base.SetColor(red); // gọi RefreshVisual
    }

    public override void SetShow(bool show)
    {
        base.SetShow(show); // gọi RefreshVisual
    }

    public override void SetBaseSprite(Sprite s)
    {
        // Với cờ vua, "đế" = backImage (mặt úp). Cho phép override thủ công nếu muốn.
        if (backImage) backImage.sprite = s;
    }

    protected override void RefreshVisual()
    {
        // Bảo vệ null
        if (!backImage && !pieceImage) return;

        // Mặc định: ẩn cả hai
        if (backImage) backImage.enabled = false;
        if (pieceImage) pieceImage.enabled = false;

        // Mặt sau theo màu (isRed==true => White; isRed==false => Black)
        if (backImage)
        {
            backImage.sprite = isRed ? backWhite : backBlack;
            backImage.enabled = !isShow;
        }

        // Nếu đã ngửa → hiện mặt trước theo quân & màu
        if (isShow && pieceImage)
        {
            var front = GetFrontSprite(type, isRed);
            pieceImage.sprite = front;
            pieceImage.enabled = (front != null);
        }
    }

    public override void SetSelected(bool selected, bool instant = false)
    {
        if (isSelected == selected && !instant) return;
        isSelected = selected;

        var target = isSelected ? baseScale * selectedScale : baseScale;

        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();

        if (instant || scaleAnimTime <= 0f)
        {
            transform.localScale = target;
            return;
        }

        scaleTween = transform
            .DOScale(target, scaleAnimTime)
            .SetEase(scaleEase)
            .SetUpdate(true); // chạy cả khi Time.timeScale=0
    }

    public override void SetInteractable(bool canInteract)
    {
        if (button) button.interactable = canInteract;
        if (pieceImage) pieceImage.raycastTarget = canInteract;
        if (backImage) backImage.raycastTarget = canInteract;
    }

    // ===== Input =====
    public void OnClick() => controller?.OnPieceClickedView(this);
    public void OnPointerClick(PointerEventData _) => controller?.OnPieceClickedView(this);

    // ===== Helpers =====

    /// <summary>
    /// Lấy sprite mặt trước theo PieceType (mapping từ enum hiện có) và màu.
    /// Quy ước: isRed==true → White, isRed==false → Black.
    /// Map:
    /// Soldier→Pawn, Knight→Knight, Elephant→Bishop, Rook→Rook, Advisor→Queen, General→King.
    /// (Cannon không dùng trong cờ vua)
    /// </summary>
    private Sprite GetFrontSprite(PieceType t, bool white)
    {
        if (chessSprites == null) return null;
        var set = white ? chessSprites.white : chessSprites.black;

        switch (t)
        {
            case PieceType.Soldier: return set.pawn;
            case PieceType.Knight: return set.knight;
            case PieceType.Elephant: return set.bishop;
            case PieceType.Rook: return set.rook;
            case PieceType.Advisor: return set.queen;
            case PieceType.General: return set.king;
            // Cannon, None… không áp dụng trong chess
            default: return null;
        }
    }

    /// <summary>
    /// (Optional) Cho phép thay sprite mặt trước trực tiếp từ ngoài (nếu bạn muốn override runtime).
    /// </summary>
    public void SetPieceSprite(Sprite s)
    {
        if (!pieceImage) return;
        pieceImage.sprite = s;
        pieceImage.enabled = (s != null) && isShow;
    }
}
