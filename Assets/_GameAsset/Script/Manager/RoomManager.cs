using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Assets._GameAsset.Script.Session;

public class RoomManager : Singleton<RoomManager>
{
    public DataRoom currentRoom;
    public void PostRequestRoom()
    {
        GlobalServices.Instance.RequestGetRoom(EChessType.Chess);
        GlobalServices.Instance.RequestGetRoom(EChessType.ChinaChess);
        GlobalServices.Instance.RequestGetRoom(EChessType.ChessVisible);
        GlobalServices.Instance.RequestGetRoom(EChessType.ChinaChessVisible);
    }
    public Dictionary<EChessType, List<DataServerRoom>> listRoomDatas = new Dictionary<EChessType, List<DataServerRoom>>();
    public void UpdateRoom(EChessType eChessType, List<DataServerRoom> listDataServerRooms)
    {
        if (listRoomDatas.ContainsKey(eChessType))
        {
            listRoomDatas[eChessType] = listDataServerRooms;
        }
        else
        {
            listRoomDatas.Add(eChessType, listDataServerRooms);
        }
        Observer.Instance.Notify(ObserverKey.OnRefreshRoom);
    }
    public List<ConfigDataRoom> GetConfigDataRooms(EChessType eChessType)
    {
        if (listRoomDatas.ContainsKey(eChessType))
        {
            var listConfigDataRooms = new List<ConfigDataRoom>();
            for (int i = 0; i < listRoomDatas[eChessType].Count; i++)
            {
                var dataServerRoom = listRoomDatas[eChessType][i];
                var configDataRoom = new ConfigDataRoom
                {
                    idRoom = dataServerRoom.id,
                    name = dataServerRoom.nameBoss,
                    gold = dataServerRoom.gold,
                    index = i,
                    isFilled = dataServerRoom.numPeoplePlay >= 1,
                };
                listConfigDataRooms.Add(configDataRoom);
            }
            return listConfigDataRooms;
        }
        return null;
    }
    public void JoinRoom(DataRoom roomData)
    {
        currentRoom = roomData;
    }
    public void LeaveRoom()
    {
        currentRoom = null;
    }
}
public enum EChessType
{
    ChinaChess = 0,
    ChinaChessVisible = 1,
    Chess = 2,
    ChessVisible = 3,
}

[System.Serializable]
public class ListDataServerRoom
{
    public EChessType eChessType;
    public List<DataServerRoom> listRoom = new List<DataServerRoom>();
}

[System.Serializable]
public class ConfigDataRoom
{
    public int idRoom;
    public string name;
    public int gold;
    public int index;
    public bool isFilled;
}
[System.Serializable]
public class DataServerRoom
{
    public int id;
    public string nameBoss;
    public int numPeoplePlay;
    public int numPeopleSee;
    public int gold;
    public bool isFlash;
}
[System.Serializable]
public class DataRoom
{
    public int idRoom;
    public EChessType eChessType;
    public int goldRate;
    public int viewer;
    public bool isMaster;
    public DataPlayerInRoom dataMe;
    public DataPlayerInRoom dataMember2;

}
[System.Serializable]
public class DataPlayerInRoom
{
    public int idSession;
    public string name;
    public string avatar;
    public long gold;
    public bool isReady;
}