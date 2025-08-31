using NetworkClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace Assets._GameAsset.Script.Session
{
    public class ReadMessage
    {
        public MessageHandler handler { get; protected set; }
        public ReadMessage(MessageHandler handler)
        {
            this.handler = handler;
        }

        public void LoginComplete(Message msg)
        {
            UIManager.Instance.HideAllUiActive();
            UIManager.Instance.ShowUI(UIName.MainMenu);
        }
        public void RefreshData(Message msg)
        {
            int indexRead = msg.Reader.readInt();
            string name = msg.Reader.readString();
            string avt = msg.Reader.readString();
            long gold = msg.Reader.readLong();
            DataManager.Instance.DataUser.id = indexRead;
            DataManager.Instance.DataUser.name = name;
            DataManager.Instance.DataUser.avt = avt;
            DataManager.Instance.DataUser.gold = gold;

            if (indexRead == -1)
            {
                UIManager.Instance.ShowUI(UIName.CreateName);
            }
            Observer.Instance.Notify(ObserverKey.RefreshDataMeByServer);
        }

        public void ShowDialog(Message msg)
        {
            byte type = msg.Reader.readByte();
            switch (type)
            {
                case 1:
                    {
                        string title = msg.Reader.readString();
                        string noti = msg.Reader.readString();
                        var uiNoti = UIManager.Instance.ShowUI<UINotiAButton>(UIName.NotiAButton);
                        if (uiNoti != null)
                        {
                            uiNoti.InitPopup(null, title, noti);
                        }
                        break;
                    }
            }
        }

        public void OnReceiveCreateData(Message msg)
        {
            byte type = msg.Reader.readByte();
            DataCreateName data = new DataCreateName();
            data.indexStatus = type;
            if (type == 0)
            {
                string strStatus = msg.Reader.readString();
                data.str = strStatus;
            }
            Observer.Instance.Notify(ObserverKey.OnReceiveCreateName, data);
        }
        public void OnReceiveListRoomData(Message msg)
        {
            byte first = msg.Reader.readByte();
            short numValue = msg.Reader.readShort();
            List<DataServerRoom> listDataServerRooms = new List<DataServerRoom>();
            for (int i = 0; i < numValue; i++)
            {
                int id = msg.Reader.readInt();
                string name = msg.Reader.readString();
                byte numPeople = msg.Reader.readByte();
                int numPeopleSee = msg.Reader.readInt();
                int gold = msg.Reader.readInt();
                bool isFlash = msg.Reader.readBool();
                DataServerRoom data = new DataServerRoom();
                data.id = id;
                data.nameBoss = name;
                data.numPeoplePlay = numPeople;
                data.numPeopleSee = numPeopleSee;
                data.gold = gold;
                data.isFlash = isFlash;
                listDataServerRooms.Add(data);
            }
            RoomManager.Instance.UpdateRoom((EChessType)first, listDataServerRooms);
        }
    }
}
