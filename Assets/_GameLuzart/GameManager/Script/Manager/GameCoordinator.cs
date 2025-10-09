using System;
using UnityEngine;

public class GameCoordinator : MonoBehaviour
{
    public Action ActionOnLoadDoneLevel = null;
    public Action<bool> ActionOnEndGame = null;
    public bool IsInRoom = false;
    public BoardController boardController;
    public void StartGame()
    {
        var ui = UIManager.Instance.GetUI<UIGameplay>(UIName.Gameplay);
        if(ui!= null)
        {
            ui.StartGame();
        }
    }
    public void OpenRoom(DataRoom dataRoom)
    {
        if (IsInRoom)
        {
            return;
        }
        IsInRoom = false;
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
    public void OnTurn(bool isMe, int count, int totalTime, int totalTimeOponent)
    {
        var ui = UIManager.Instance.GetUI<UIGameplay>(UIName.Gameplay);
        ui.CountdownPlayer(isMe, count, totalTime, totalTimeOponent);
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
