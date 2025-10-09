using System;
using UnityEngine;
using System.Collections.Generic;
using Assets._GameAsset.Script.Session;

public class UISeeChess : UIBase
{
    public ItemSeeChess itemSeeChest;
    public Transform parentSpawn;
    public List<ItemSeeChess> listItem = new List<ItemSeeChess>();

    private bool isEChess = false;
    private void OnEnable()
    {
        Observer.Instance.AddObserver(ObserverKey.OnRefreshRoomSee, Refresh);
        RefreshUI();
    }
    private void OnDisable()
    {
        Observer.Instance.RemoveObserver(ObserverKey.OnRefreshRoomSee, Refresh);
    }
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
    }
    private void Refresh(object data)
    {
        RefreshUI();
    }
    public override void RefreshUI()
    {
        var dictRoom = RoomManager.Instance.dictRoomSeeDatas;
        if (dictRoom != null && dictRoom.Count >0 )
        {
            List<DataRoom> listRoom = new List<DataRoom>();
            if (isEChess)
            {
                if(dictRoom.ContainsKey(EChessType.Chess))
                    listRoom.AddRange(dictRoom[EChessType.Chess]);

                if (dictRoom.ContainsKey(EChessType.ChessVisible))
                    listRoom.AddRange(dictRoom[EChessType.ChessVisible]);
            }
            else
            {
                if (dictRoom.ContainsKey(EChessType.ChinaChess))
                    listRoom.AddRange(dictRoom[EChessType.ChinaChess]);
                if (dictRoom.ContainsKey(EChessType.ChinaChessVisible))
                    listRoom.AddRange(dictRoom[EChessType.ChinaChessVisible]);
            }
            int length = listRoom.Count;
            MasterHelper.InitListObj(length, itemSeeChest, listItem, parentSpawn, (item, index) =>
            {
                var data = listRoom[index];
                item.InitData(data, OnClickRoomSeeChess);
                item.gameObject.SetActive(true);
            });
        }
    }
    private void OnClickRoomSeeChess(DataRoom dataRoom)
    {
        Debug.Log($"OnClickRoomSeeChess : {dataRoom.idRoom}");
        GlobalServices.Instance.RequestJoinRoom(dataRoom.idRoom,true);
    }
    public void OnClickChess(bool isChess)
    {
        this.isEChess = isChess;
        RefreshUI();
    }
    public void OnClickRefresh()
    {
        RoomManager.Instance.RequestSeeRoom();
    }
}