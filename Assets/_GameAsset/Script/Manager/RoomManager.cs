using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomManager : Singleton<RoomManager>
{
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