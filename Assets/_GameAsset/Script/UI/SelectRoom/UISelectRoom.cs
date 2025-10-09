using Assets._GameAsset.Script.Session;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UISelectRoom : UIBase
{
    public Transform content;
    public ItemSelectRoom itemSelectRoomPf;
    public List<ItemSelectRoom> listItemSelectRoom = new List<ItemSelectRoom>();
    public BaseSelect selectRank;
    public RoomManager roomManager
    {
        get
        {
            return RoomManager.Instance;
        }
    }
    private EChessType currentChessType;
    private int currentRank = 0;
    public void OnClickRank(int index)
    {
        this.currentRank = index;
        GetRoom(currentChessType);
    }
    public List<ConfigDataRoom> GetCurrentRank(List<ConfigDataRoom> list, int currentRank)
    {
        List<int> listInt = DataManager.Instance.dbValueRate[currentRank].value;
        int min = listInt[0];
        int max = listInt[^1];
        List<ConfigDataRoom> listResult = list.Where(x => x.gold >= min && x.gold <= max).ToList();
        return listResult;
    }
    private void OnEnable()
    {
        Observer.Instance.AddObserver(ObserverKey.OnRefreshRoom, Refresh);
    }
    private void OnDisable()
    {
        Observer.Instance.RemoveObserver(ObserverKey.OnRefreshRoom, Refresh);
    }
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        OnClickRefreshData();
    }
    public void GetRoom(EChessType eChessType)
    {
        this.currentChessType = eChessType;
        var listRoom = roomManager.GetConfigDataRooms(eChessType);
        listRoom = GetCurrentRank(listRoom, currentRank);
        if (listRoom != null)
        {
            int length = listRoom.Count;
            MasterHelper.InitListObj<ItemSelectRoom>(length, itemSelectRoomPf, listItemSelectRoom, content, (item, index) =>
            {
                item.gameObject.SetActive(true);
                var data = listRoom[index];
                item.InitData(data, OnClickJoinRoom);
            });
        }
    }
    private void Refresh(object data)
    {
        RefreshUI();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        GetRoom(currentChessType);
    }
    public void OnClickBack()
    {
        Hide();
    }
    public void OnClickCreateRoom()
    {
        var  ui = UIManager.Instance.ShowUI<UICreateRoom>(UIName.CreateRoom);
        ui.typeChess = (int) currentChessType;
        ui.SetDefaultChess();
    }
    public void OnClickJoinRoom(ConfigDataRoom dataRoom)
    {
        GlobalServices.Instance.RequestJoinRoom(dataRoom.idRoom,false);

    }
    public void OnClickRefreshData()
    {
        RoomManager.Instance.PostRequestRoom(currentChessType);
    }
    public void OnClickShowPrivateChallenge()
    {
        UIManager.Instance.ShowUI<UIPrivateChallenge>(UIName.UIPrivateChallenge);
    }
}
