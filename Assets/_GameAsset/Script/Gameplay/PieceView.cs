using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceView : MonoBehaviour
{
    // Data runtime của quân cờ
    public int id { get; protected set; }
    public PieceType type { get; protected set; }
    public bool isRed { get; protected set; }
    public bool isShow { get; protected set; }
    protected BoardController controller;

    public virtual void InitData(int id, PieceType type, bool isRed, bool isShow)
    {

    }
    public virtual bool IsKing()
    {
        return type == PieceType.General;
    }

    /// <summary>
    /// Liên kết với BoardController để xử lý input
    /// </summary>
    /// <param name="c">BoardController instance</param>
    public virtual void BindController(BoardController c) => controller = c;
    /// <summary>
    /// Cập nhật loại quân cờ và refresh hiển thị
    /// </summary>
    /// <param name="newType">Loại quân mới</param>
    public virtual void SetType(PieceType newType) { type = newType; RefreshVisual(); }
    /// <summary>
    /// Cập nhật màu quân cờ và refresh hiển thị
    /// </summary>
    /// <param name="red">True nếu đỏ, false nếu đen</param>
    public virtual void SetColor(bool red) { isRed = red; RefreshVisual(); }

    /// <summary>
    /// Cập nhật trạng thái hiển thị (úp/ngửa) và refresh hiển thị
    /// </summary>
    /// <param name="show">True nếu ngửa, false nếu úp</param>
    public virtual void SetShow(bool show) { isShow = show; RefreshVisual(); }

    /// <summary>
    /// Thiết lập sprite cho ảnh đế/nền
    /// </summary>
    /// <param name="s">Sprite cho đế</param>ß
    protected virtual void RefreshVisual()
    {

    }
    public virtual void SetSelected(bool selected, bool instant = false)
    {

    }
    public virtual void SetInteractable(bool canInteract)
    {

    }

    public virtual void SetBaseSprite(Sprite s)
    { }

}
