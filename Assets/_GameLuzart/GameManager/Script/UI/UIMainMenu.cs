using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainMenu : UIBase
{
    public void OnClickChinaChess()
    {
        var ui = UIManager.Instance.ShowUI<UISelectRoom>(UIName.SelectRoom);
        ui.GetRoom(EChessType.ChinaChess);
    }
    public void OnClickChinaChessVisible()
    {
        var ui = UIManager.Instance.ShowUI<UISelectRoom>(UIName.SelectRoom);
        ui.GetRoom(EChessType.ChinaChessVisible);
    }
    public void OnClickChess()
    {
        var ui = UIManager.Instance.ShowUI<UISelectRoom>(UIName.SelectRoom);
        ui.GetRoom(EChessType.Chess);
    }
    public void OnClickChessVisible()
    {
        var ui = UIManager.Instance.ShowUI<UISelectRoom>(UIName.SelectRoom);
        ui.GetRoom(EChessType.ChessVisible);
    }
}
