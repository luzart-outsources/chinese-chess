using System;
using UnityEngine;

public class GameCoordinator : MonoBehaviour
{
    public Action ActionOnLoadDoneLevel = null;
    public Action<bool> ActionOnEndGame = null;
    public bool IsInRoom = false;
    public BoardController boardController;
    public void OpenRoom(DataRoom dataRoom)
    {
        if (IsInRoom)
        {
            return;
        }
        IsInRoom = true;
        InitBoardGame();
    }
    public void OnResetBoard()
    {
        boardController.CloseBoard();
        InitBoardGame();
    }
    public void InitBoardGame()
    {
        boardController.InitializeRuleCode(RoomManager.Instance.currentRoom.eChessType);
    }
    public void OnMoveDenied()
    {

    }
    public void OnCheckKing()
    {
        UIManager.Instance.ShowUI(UIName.ChieuTuong);
    }
    public void OnEndGame(bool isWin)
    {
        UIName uiName = isWin ? UIName.Win : UIName.Lose;
        UIManager.Instance.ShowUI(uiName);
    }


}
