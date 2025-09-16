using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening; // <-- QUAN TRỌNG: dùng DOTween

[System.Serializable]
public struct ColorPieceSprites
{
    public Sprite rook, knight, cannon, elephant, advisor, general, soldier;
}
[System.Serializable]
public class PieceSpriteConfig
{
    public ColorPieceSprites red, black;
}

public class ChessPieceView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Refs")]
    [SerializeField] private Image baseImage;   // đế
    [SerializeField] private Image pieceImage;  // hình quân
    [SerializeField] private Button button;     // tuỳ chọn, nếu bạn dùng Button

    [Header("Sprite Config (prefab tự chứa)")]
    [SerializeField] private PieceSpriteConfig spriteConfig;

    [Header("Select FX (DOTween)")]
    [SerializeField] private float selectedScale = 1.08f; // phóng 8%
    [SerializeField] private float scaleAnimTime = 0.08f;
    [SerializeField] private Ease scaleEase = Ease.OutCubic;

    // Data (runtime)
    public int id { get; private set; }
    public PieceType type { get; private set; }
    public bool isRed { get; private set; }
    public bool isShow { get; private set; }

    // State chọn
    private bool isSelected = false;
    private Vector3 baseScale = Vector3.one;
    private Tween scaleTween; // DOTween tween cho scale

    private BoardController controller;

    private void Awake()
    {
        if (baseImage == null) baseImage = GetComponent<Image>();
        if (pieceImage == null)
        {
            var t = transform.Find("Piece");
            pieceImage = t ? t.GetComponent<Image>() : GetComponentInChildren<Image>();
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        baseScale = transform.localScale;
    }

    private void OnDisable()
    {
        // Dọn tween khi object tắt/hủy
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
    }

    public void BindController(BoardController c) => controller = c;

    // ========== DATA & RENDER ==========
    public void InitData(int id, PieceType type, bool isRed, bool isShow)
    {
        this.id = id; this.type = type; this.isRed = isRed; this.isShow = isShow;
        RefreshVisual();
        SetSelected(false, instant: true); // đảm bảo scale về base khi spawn
    }

    public void SetType(PieceType newType) { type = newType; RefreshVisual(); }
    public void SetColor(bool red) { isRed = red; RefreshVisual(); }
    public void SetShow(bool show) { isShow = show; RefreshVisual(); }

    public void SetBaseSprite(Sprite s) { if (baseImage) baseImage.sprite = s; }

    public void SetPieceSprite(Sprite s)
    {
        if (!pieceImage) return;
        pieceImage.sprite = s;
        pieceImage.enabled = (s != null);
    }

    public void SetInteractable(bool canInteract)
    {
        if (button) button.interactable = canInteract;
        if (pieceImage) pieceImage.raycastTarget = canInteract;
        if (baseImage) baseImage.raycastTarget = canInteract;
    }

    private void RefreshVisual()
    {
        if (!pieceImage) return;

        if (!isShow)
        {
            // Ẩn quân khi chưa ngửa: sprite = null
            pieceImage.sprite = null;
            pieceImage.enabled = false;
            return;
        }

        Sprite front = GetPieceSprite(type, isRed);
        pieceImage.sprite = front;
        pieceImage.enabled = (front != null);
    }

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
    public void SetSelected(bool selected, bool instant = false)
    {
        if (isSelected == selected && !instant) return;
        isSelected = selected;

        Vector3 target = isSelected ? baseScale * selectedScale : baseScale;

        // Kill tween cũ (nếu đang chạy) để không chồng chéo
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();

        if (instant || scaleAnimTime <= 0f)
        {
            transform.localScale = target;
            return;
        }

        scaleTween = transform
            .DOScale(target, scaleAnimTime)
            .SetEase(scaleEase)
            .SetUpdate(true); // chạy ngay cả khi Time.timeScale = 0 (tạm dừng)
    }

    // ========== INPUT ==========
    public void OnClick() => controller?.OnPieceClickedView(this);
    public void OnPointerClick(PointerEventData eventData) => controller?.OnPieceClickedView(this);
}
