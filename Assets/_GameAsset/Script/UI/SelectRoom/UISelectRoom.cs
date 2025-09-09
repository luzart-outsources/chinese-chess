using Assets._GameAsset.Script.Session;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
    }
    public void GetRoom(EChessType eChessType)
    {
        this.currentChessType = eChessType;
        var listRoom = roomManager.GetConfigDataRooms(eChessType);
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
    public void OnClickBack()
    {
        Hide();
    }
    public void OnClickCreateRoom()
    {
        UIManager.Instance.ShowUI<UICreateRoom>(UIName.CreateRoom);
    }
    public void OnClickJoinRoom(ConfigDataRoom dataRoom)
    {
        GlobalServices.Instance.RequestJoinRoom(dataRoom.idRoom,false);

    }
}
