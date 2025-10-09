using JetBrains.Annotations;
using NetworkClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

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
        public void RefreshMoney(Message msg)
        {
            long gold = msg.Reader.readLong();
            DataManager.Instance.DataUser.gold = gold;
            Observer.Instance.Notify(ObserverKey.CoinObserverNormal);
        }
        public void OnReceiveCase4(Message msg)
        {
            byte type = msg.Reader.readByte();
            switch (type)
            {
                case 0:
                    {
                        RefreshData(msg);
                        break;
                    }
                case 1:
                    {
                        RefreshMoney(msg);
                        break;
                    }
            }
        }

        public void ShowDialog(Message msg)
        {
            byte type = msg.Reader.readByte();
            switch (type)
            {
                case 1:
                    {
                        OnReceiveNotiAButton(msg);
                        break;
                    }
                case 2:
                    {
                        OnReceiveToast(msg);
                        break;
                    }
                case 3:
                    {
                        OnReceiveOpenShowURLOK(msg);
                        break;
                    }
                case 4:
                    {
                        OnReceiveOpenURL(msg);
                        break;
                    }
                case 6:
                    {
                        OnReceiveNotiToast(msg);
                        break;
                    }
                case 7:
                    {
                        OnReceiveNotiTwoButton(msg);
                        break;
                    }
            }
        }
        private void OnReceiveNotiTwoButton(Message msg)
        {
            string title = msg.Reader.readString();
            string noti = msg.Reader.readString();
            string textBtn1 = msg.Reader.readString();
            string textBtn2 = msg.Reader.readString();
            //var uiNoti = UIManager.Instance.ShowUI<UINotiTwoButton>(UIName.NotiTwoButton);
            //if (uiNoti != null)
            //{
            //    uiNoti.InitPopup(null, title, noti, textBtn1, textBtn2);
            //}
            UnityEngine.Debug.Log("[Receive]OnReceiveNotiTwoButton: " + title + " - " + noti + " - " + textBtn1 + " - " + textBtn2);
        }
        private void OnReceiveNotiToast(Message msg)
        {
            string noti = msg.Reader.readString();
            UIManager.Instance.ShowToast(noti);
            UnityEngine.Debug.Log("[Receive]OnReceiveNotiToast: " + noti);
        }
        private void OnReceiveNotiAButton(Message msg)
        {
            string title = msg.Reader.readString();
            string noti = msg.Reader.readString();
            var uiNoti = UIManager.Instance.ShowUI<UINotiAButton>(UIName.NotiAButton);
            if (uiNoti != null)
            {
                uiNoti.InitPopup(null, title, noti);
            }
            UnityEngine.Debug.Log("[Receive]OnReceiveNotiAButton: " + title + " - " + noti);
        }
        private void OnReceiveToast(Message msg)
        {
            string noti = msg.Reader.readString();
            UIManager.Instance.ShowToast(noti);
            UnityEngine.Debug.Log("[Receive]OnReceiveToast: " + noti);
        }
        private void OnReceiveOpenShowURLOK(Message msg)
        {
            string title = msg.Reader.readString();
            string noti = msg.Reader.readString();
            string url = msg.Reader.readString();
            var uiNoti = UIManager.Instance.ShowUI<UINotiAButton>(UIName.NotiAButton);
            if (uiNoti != null)
            {
                uiNoti.InitPopup(() => {
                    Application.OpenURL(url);
                }, title, noti, "Open");
            }
            UnityEngine.Debug.Log("[Receive]OnReceiveOpenShowURLOK: " + url);
        }
        private void OnReceiveOpenURL(Message msg)
        {
            string url = msg.Reader.readString();
            Application.OpenURL(url);
            UnityEngine.Debug.Log("[Receive]OnReceiveOpenURL: " + url);
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
            UnityEngine.Debug.Log("[Receive]OnReceiveCreateData: " + data.indexStatus);

        }
        public void OnReceiveCase10(Message msg)
        {
            byte type = msg.Reader.readByte();
            switch (type)
            {
                case 0:
                    {
                        OnReceiveListRoomData(msg);
                        break;
                    }
                case 3:
                    {
                        OnReceiveListRoomSeeData(msg);
                        break;
                    }
                default:
                    {
                        OnReceiveListRoomData(msg);
                        break;
                    }
            }
        }
        public void OnReceiveListRoomData(Message msg)
        {
            byte typeChess = msg.Reader.readByte();
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
            RoomManager.Instance.UpdateRoom((EChessType)typeChess, listDataServerRooms);
            UnityEngine.Debug.Log("[Receive]OnReceiveListRoomData: " + typeChess + " - " + listDataServerRooms.Count);
        }

        public void OnReceiveListRoomSeeData(Message msg)
        {
            short numValue = msg.Reader.readShort();
            Dictionary<EChessType, List<DataRoom>> dictRoom = new Dictionary<EChessType, List<DataRoom>>();
            for (int i = 0; i < numValue; i++)
            {
                List<DataRoom> listDataRooms = new List<DataRoom>();
                int id = msg.Reader.readInt();
                byte byteTypeChess = msg.Reader.readByte();
                EChessType typeChess = (EChessType)byteTypeChess;
                string name1 = msg.Reader.readString();
                string avatar1 = msg.Reader.readString();
                string name2 = msg.Reader.readString();
                string avatar2 = msg.Reader.readString();
                int numPeopleSee = msg.Reader.readInt();
                int gold = msg.Reader.readInt();
                bool isFlash = msg.Reader.readBool();

                if (dictRoom.ContainsKey(typeChess))
                {
                    listDataRooms = dictRoom[typeChess];
                }
                DataRoom data = new DataRoom();
                data.idRoom = id;
                data.viewer = numPeopleSee;
                data.goldRate = gold;
                data.eChessType = typeChess;
                DataPlayerInRoom dataPlayer = new DataPlayerInRoom();
                dataPlayer.name = name1;
                dataPlayer.avatar = avatar1;
                DataPlayerInRoom dataPlayer2 = new DataPlayerInRoom();
                dataPlayer2.name = name2;
                dataPlayer2.avatar = avatar2;
                data.dataMe = dataPlayer;
                data.dataMember2 = dataPlayer2;
                listDataRooms.Add(data);

                if (dictRoom.ContainsKey(typeChess))
                {
                    dictRoom[typeChess] = listDataRooms;
                }
                else
                {
                    dictRoom.Add(typeChess, listDataRooms);
                }
            }
            RoomManager.Instance.UpdateRoomSee(dictRoom);
            UnityEngine.Debug.Log("[Receive] OnReceiveListRoomSeeData: " + dictRoom.Count);
        }

        public void OnReceiveCase11(Message msg)
        {
            byte type = msg.Reader.readByte();
            switch (type)
            {
                case 0:
                    {
                        OnReceiveCreateRoomData(msg);
                        break;
                    }
                case 1:
                    {
                        // Join Room
                        OnUpdateDataInRoom(msg);
                        break;
                    }
                case 2:
                    {
                        OnReceiveViewer(msg);
                        break;
                    }
                case 3:
                    {
                        OnReceiveReadyData(msg);
                        break;
                    }
                case 4:
                    {
                        OnReceiveLeaveRoom(msg);
                        break;
                    }
                case 5:
                    {
                        OnReceiveResetBoard();
                        break;
                    }
                case 6:
                    {
                        OnReceiveTimeReady(msg);
                        break;
                    }
                case 7:
                    {
                        OnReceiveTextWait(msg);
                        break;
                    }
            }
        }
        private void OnReceiveTextWait(Message msg)
        {
            string str = msg.Reader.readString();
            GameManager.Instance.OnReceiveString(str);
            UnityEngine.Debug.Log("[Receive]OnReceiveTextWait: " + str);
        }
        public void OnReceiveCreateRoomData(Message msg)
        {
            int idRoom = msg.Reader.readInt();
            byte typeChess = msg.Reader.readByte();
            bool isViewer = msg.Reader.readBool();
            short numberViewer = msg.Reader.readShort();
            int goldRate = msg.Reader.readInt();
            DataRoom dataRoom = new DataRoom();
            dataRoom.idRoom = idRoom;
            dataRoom.goldRate = goldRate;
            dataRoom.viewer = numberViewer;
            dataRoom.eChessType = (EChessType)typeChess;
            OnUpdateDataInRoom(msg, dataRoom);
            UnityEngine.Debug.Log("[Receive]OnReceiveCreateRoomData: " + dataRoom.idRoom + " - " + dataRoom.isMaster + " - " + dataRoom.dataMe.name + " - " + (dataRoom.dataMember2 != null ? dataRoom.dataMember2.name : "null"));

        }
        private void OnReceiveTimeReady(Message msg)
        {
            int idSession = msg.Reader.readInt();
            int timeMS = msg.Reader.readInt();
            int timeS = timeMS / 1000;
            GameManager.Instance.OnShowDataTime(timeS);

            UnityEngine.Debug.Log($"[Receive] OnReceiveTimeReady + {timeS}");
        }
        public void OnUpdateDataInRoom(Message msg, DataRoom dataRoom = null)
        {
            if (dataRoom == null)
            {
                dataRoom = RoomManager.Instance.currentRoom;
            }
            string nameMember2 = "";
            string avtMember2 = "";
            long goldMember2 = 0;
            bool isReady2 = false;
            int idMember1 = msg.Reader.readInt();
            string nameMember1 = msg.Reader.readString();
            string avtMember1 = msg.Reader.readString();
            long goldMember1 = msg.Reader.readLong();
            bool isReady1 = msg.Reader.readBool();
            int idMemeber2 = msg.Reader.readInt();
            if (idMemeber2 != -1)
            {
                nameMember2 = msg.Reader.readString();
                avtMember2 = msg.Reader.readString();
                goldMember2 = msg.Reader.readLong();
                isReady2 = msg.Reader.readBool();
            }
                var dataMember1 = new DataPlayerInRoom()
                {
                    idSession = idMember1,
                    name = nameMember1,
                    avatar = avtMember1,
                    gold = goldMember1,
                    isReady = isReady1
                };
            DataPlayerInRoom dataMember2 = null;
            if (idMemeber2 != -1)
            {
                dataMember2 = new DataPlayerInRoom()
                {
                    idSession = idMemeber2,
                    name = nameMember2,
                    avatar = avtMember2,
                    gold = goldMember2,
                    isReady = isReady2
                };
            }
            bool isMeIsMaster = idMember1 == DataManager.Instance.DataUser.id;
            dataRoom.isMaster = isMeIsMaster;
            if (isMeIsMaster)
            {
                dataRoom.dataMe = dataMember1;
                dataRoom.dataMember2 = dataMember2;
            }
            else
            {
                dataRoom.dataMe = dataMember2;
                dataRoom.dataMember2 = dataMember1;
            }
            UnityEngine.Debug.Log("[Receive]OnUpdateDataInRoom: " + dataRoom.idRoom + " - " + dataRoom.isMaster + " - " + dataRoom.dataMe.name + " - " + (dataRoom.dataMember2 != null ? dataRoom.dataMember2.name : "null"));
            RoomManager.Instance.JoinRoom(dataRoom);
            GameManager.Instance.OpenRoom(dataRoom);
            if(idMemeber2 == -1)
            {
                GameManager.Instance.OnReceiveString();
            }

        }

        public void OnReceiveReadyData(Message msg)
        {
            int id = msg.Reader.readInt();
            bool isReady = msg.Reader.readBool();

            var data = new DataReceiveReady
            {
                id = id,
                isReady = isReady
            };
            GameManager.Instance.OnReceivePlayerReady(data);
            UnityEngine.Debug.Log("[Receive]OnReceiveReadyData: " + id + " - " + isReady);
        }
        public void OnReceiveLeaveRoom(Message msg)
        {
            RoomManager.Instance.LeaveRoom();
            GameManager.Instance.LeaveRoom();
            UIManager.Instance.HideAllUiActive();
            UIManager.Instance.ShowUI(UIName.MainMenu);
            UnityEngine.Debug.Log("[Receive]OnReceiveLeaveRoom");
        }
        public void OnReceiveCase12(Message msg)
        {
            byte type = msg.Reader.readByte();
            switch (type)
            {
                case 0:
                    {
                        OnReceiveDataBoardInStartGame(msg);
                        break;
                    }
                case 1:
                    {
                        OnReceiveCanMove(msg);
                        break;
                    }
                case 2:
                    {
                        OnReceiveTurnMove(msg);
                        break;
                    }
                case 4:
                    {
                        OnReceiveAnimation(msg);
                        break;
                    }
            }
        }
        public void OnReceiveDataBoardInStartGame(Message msg)
        {
            InitPayload data = new InitPayload();

            // byte đầu: loại bàn cờ
            byte boardType = msg.Reader.readByte();

            int idMember1 = msg.Reader.readInt();
            bool isMyRed = !msg.Reader.readBool();

            int idMember2 = msg.Reader.readInt();
            bool idMember2Red = !msg.Reader.readBool();

            // row, col
            byte rowCount = msg.Reader.readByte();
            byte colCount = msg.Reader.readByte();
            int rows = rowCount;
            int cols = colCount;

            // số quân
            short totalPieces = msg.Reader.readShort();

            // Khởi tạo mảng răng cưa PieceDTO (null = ô trống)
            var grid = new PieceDTO[rows][];
            for (int r = 0; r < rows; r++)
                grid[r] = new PieceDTO[cols];

            // Suy ra biến thể để set isShow ban đầu
            bool isUpsideDown = boardType == 1 || boardType == 3;
            bool isChess = IsChessBoard(boardType, rows, cols);

            for (int i = 0; i < totalPieces; i++)
            {
                short id = msg.Reader.readShort();
                sbyte type = msg.Reader.readSByte();
                bool isBlack = msg.Reader.readBool();
                short indexColumn = msg.Reader.readShort();
                short indexRow = msg.Reader.readShort();

                // Clamp an toàindex
                int c = Mathf.Clamp(indexColumn, 0, cols - 1);
                int r = Mathf.Clamp(indexRow, 0, rows - 1);

                // Tạo DTO. Lưu ý: type server đã map về PieceType của bạn.
                var dto = new PieceDTO
                {
                    id = id,
                    type = (PieceType)type,
                    isRed = !isBlack,              // Chess: isRed==true ⇔ White
                    isShow = type >= 0,          // “úp” → ẩn, “thường” → ngửa
                };

                grid[r][c] = dto;
                UnityEngine.Debug.Log($"[OnReceive] id={id} at ({r},{c})");
            }

            data.grid = grid;
            if (RoomManager.Instance.currentRoom.isMaster)
            {
                data.iAmRed = isMyRed;
            }
            else
            {
                data.iAmRed = !isMyRed;
            }

            // Giữ logic cũ (nếu server có field lượt đánh thì thay bằng giá trị server)
            data.myTurn = data.iAmRed;

            UnityEngine.Debug.Log($"[OnReceive] StartGame: boardType={boardType} I am master {RoomManager.Instance.currentRoom.isMaster} id1 = {idMember1} and isRed = {isMyRed}");


            // TODO: truyền sang BoardController
            GameManager.Instance.gameCoordinator.boardController.InitializeFromServer(data);
            GameManager.Instance.gameCoordinator.StartGame();
        }
        private void OnReceiveViewer(Message msg)
        {
            int viewer = msg.Reader.readInt();
        }
        private void OnReceiveTurnMove(Message msg)
        {
            int id = msg.Reader.readInt();
            long timeRemain = msg.Reader.readLong() / 1000;
            long timeTotalRemain = msg.Reader.readLong() / 1000;
            long timeTotalRemainOpponent = msg.Reader.readLong() / 1000;

            bool isMyTurn = id == DataManager.Instance.DataUser.id;
            GameManager.Instance.gameCoordinator.boardController.SetMyTurn(isMyTurn);
            GameManager.Instance.gameCoordinator.OnTurn(isMyTurn, (int)timeRemain, (int)timeTotalRemain, (int)timeTotalRemainOpponent);

            UnityEngine.Debug.Log("[Receive]OnReceiveTurnMove: " + id + " - " + isMyTurn + " - " + timeRemain + " - " + timeTotalRemain + " - " + timeTotalRemainOpponent);
        }

        private void OnReceiveCanMove(Message msg)
        {
            short id = msg.Reader.readShort();
            byte type = msg.Reader.readByte();
            short toX = msg.Reader.readShort();
            short toY = msg.Reader.readShort();

            ServerMoveResult r = new ServerMoveResult();
            r.pieceId = id;
            r.newType = (PieceType)type;
            r.newRow = toY;
            r.newCol = toX;

            GameManager.Instance.gameCoordinator.boardController.OnServerMoveResult(r);
            UnityEngine.Debug.Log("[Receive][OnReceive] MoveResult: id=" + id + " to (" + toY + "," + toX + ") type=" + type);
        }
        private bool IsChessBoard(byte boardType, int rows, int cols)
        {
            // Ưu tiên rows/cols nếu server set chuẩn 8x8 cho Chess
            if (rows == 8 && cols == 8) return true;
            return boardType == 2 || boardType == 3;
        }
        public void OnReceiveCase5(Message msg)
        {
            byte type = msg.Reader.readByte();
            switch (type)
            {
                case 0:
                    {
                        OnReceiveChatInGame(msg);
                        break;
                    }
                case 1:
                    {
                        // Chat private
                        break;
                    }
                case 2:
                    {
                        OnReceiveChatServer(msg);
                        break;
                    }
            }
        }
        private void OnReceiveChatInGame(Message msg)
        {
            int idSession = msg.Reader.readInt();
            string name = msg.Reader.readString();
            string chat = msg.Reader.readString();
            Observer.Instance.Notify(ObserverKey.OnReceiveChatInGame, chat);
            UnityEngine.Debug.Log("[Receive]OnReceiveChatInGame: " + idSession + " - " + chat);
        }
        private void OnReceiveChatServer(Message msg)
        {
            int id = msg.Reader.readInt();
            string name = msg.Reader.readString();
            string chat = msg.Reader.readString();
            DataMessageWorld data = new DataMessageWorld
            {
                id = id,
                name = name,
                chat = chat
            };
            UIChatGlobal.listChatGlobal.Add($"{name}: {chat}");
            Observer.Instance.Notify(ObserverKey.OnReceiveChatServer, data);
            UnityEngine.Debug.Log("[Receive]OnReceiveChatServer: " + id + " - " + chat);
        }
        private void OnReceiveAnimation(Message msg)
        {
            int idSession = msg.Reader.readInt();
            int type = msg.Reader.readByte();
            AnimationType animationType = (AnimationType)type;
            switch(animationType)
            {
                case AnimationType.MOVE_DENIED:
                    {
                        GameManager.Instance.gameCoordinator.OnMoveDenied();
                        break;
                    }
                case AnimationType.TAGET_KING:
                    {
                        GameManager.Instance.gameCoordinator.OnCheckKing();
                        break;
                    }
            }
            if(idSession == DataManager.Instance.DataUser.id)
            {
                // animation của mình
                switch (animationType)
                {
                    case AnimationType.WIN:
                        {
                            GameManager.Instance.gameCoordinator.OnEndGame(true);
                            break;
                        }
                    case AnimationType.LOSE:
                        {
                            GameManager.Instance.gameCoordinator.OnEndGame(false);
                            break;
                        }
                }
            }
            UnityEngine.Debug.Log("[Receive]OnReceiveAnimation: " + type);
        }
        private void OnReceiveResetBoard()
        {
            GameManager.Instance.gameCoordinator.OnResetBoard();
            GameManager.Instance.OpenRoom(RoomManager.Instance.currentRoom);
            UnityEngine.Debug.Log("[Receive]OnReceiveResetBoard");
        }
        public void OnReceiveCase13(Message msg)
        {
            byte type = msg.Reader.readByte();
            switch (type)
            {
                case 1:
                    {
                        OnReceiveUserInfor(msg);
                        break;
                    }
                case 2:
                    {
                        OnReceiveChatP2P(msg);
                        break;
                    }
            }
        }
        private void OnReceiveChatP2P(Message msg)
        {
            string username = msg.Reader.readString();
            string str = msg.Reader.readString();
            UnityEngine.Debug.Log("[Receive]OnReceiveChatP2P: " + username + " - " + str);
        }
        public void OnReceiveUserInfor(Message msg)
        {
            DataUser dataUser = new DataUser();
            int idSession = msg.Reader.readInt();
            string name = msg.Reader.readString();
            string avatar = msg.Reader.readString();
            long gold = msg.Reader.readLong();
            byte statusAddFriend = msg.Reader.readByte(); // 0: chưa kết bạn, 1: đã kết bạn, 2: đã gửi lời mời, 3: nhận lời mời
            bool isOnline = msg.Reader.readBool();
            byte count = msg.Reader.readByte();
            dataUser.dataHistoryGame = new List<DataHistoryGame>();
            for (int i = 0; i < count; i++)
            {
                DataHistoryGame dataHistory = new DataHistoryGame();
                byte BytertypeGame = msg.Reader.readByte();
                dataHistory.eChessType = (EChessType)BytertypeGame;
                dataHistory.win = msg.Reader.readInt();
                dataHistory.lose = msg.Reader.readInt();
                dataUser.dataHistoryGame.Add(dataHistory);
            }
            dataUser.idPlayer = idSession;
            dataUser.name = name;
            dataUser.avt = avatar;
            dataUser.gold = gold;
            dataUser.statusFriend = statusAddFriend;
            dataUser.isOnline = isOnline;

            var ui = UIManager.Instance.ShowUI<UIProfile>(UIName.Profile);
            ui.InitData(dataUser);

            UnityEngine.Debug.Log("[Receive]OnReceiveUserInfor: " + idSession + " - " + name + " - " + statusAddFriend + " - " + isOnline);
        }
    }
}
[System.Serializable]
public class DataMessageWorld
{
    public int id;
    public string name;
    public string chat;
}
[System.Serializable]
public class DataReceiveReady
{
    public int id;
    public bool isReady;
}
public enum AnimationType
{
    MOVE_DENIED = 0, // KHÔNG ĐƯỢC DI CHUYỂN
    TAGET_KING = 1,  // CHIẾU TƯỚNG
    WIN = 2,         // THẮNG
    LOSE = 3,        // THUA
}