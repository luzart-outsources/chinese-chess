using NetworkClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Lumin;

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
            UnityEngine.Debug.Log("OnReceiveListRoomData: " + typeChess + " - " + listDataServerRooms.Count);
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
                        // Start Game
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
            }
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
            UnityEngine.Debug.Log("OnReceiveCreateRoomData: " + dataRoom.idRoom + " - " + dataRoom.isMaster + " - " + dataRoom.dataMe.name + " - " + (dataRoom.dataMember2 != null ? dataRoom.dataMember2.name : "null"));

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
            UnityEngine.Debug.Log("OnUpdateDataInRoom: " + dataRoom.idRoom + " - " + dataRoom.isMaster + " - " + dataRoom.dataMe.name + " - " + (dataRoom.dataMember2 != null ? dataRoom.dataMember2.name : "null"));
            RoomManager.Instance.JoinRoom(dataRoom);
            GameManager.Instance.OpenRoom(dataRoom);
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
            GameManager.Instance.OnReceiveRoomReady(data);
            UnityEngine.Debug.Log("OnReceiveReadyData: " + id + " - " + isReady);
        }
        public void OnReceiveLeaveRoom(Message msg)
        {
            RoomManager.Instance.LeaveRoom();
            GameManager.Instance.LeaveRoom();
            UIManager.Instance.HideAllUiActive();
            UIManager.Instance.ShowUI(UIName.MainMenu);
        }
        public void OnReceiveCase12(Message msg)
        {
            byte type = msg.Reader.readByte();
            switch (type)
            {
                case 0:
                    {
                        OnReceiveStartGame(msg);
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
            }
        }
        public void OnReceiveStartGame(Message msg)
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
                short x = msg.Reader.readShort(); // server row
                short y = msg.Reader.readShort(); // server col

                // Clamp an toàn
                int r = Mathf.Clamp(y, 0, rows - 1);
                int c = Mathf.Clamp(x, 0, cols - 1);

                // Tạo DTO. Lưu ý: type server đã map về PieceType của bạn.
                var dto = new PieceDTO
                {
                    id = id,
                    type = (PieceType)type,
                    isRed = !isBlack,              // Chess: isRed==true ⇔ White
                    isShow = !isUpsideDown          // “úp” → ẩn, “thường” → ngửa
                };

                grid[r][c] = dto;
                UnityEngine.Debug.Log($"[OnReceive] Piece {i}: id={id} type={type} isBlack={isBlack} at ({r},{c})");
            }

            data.grid = grid;
            if (idMember1 == DataManager.Instance.DataUser.id && RoomManager.Instance.currentRoom.isMaster)
            {
                data.iAmRed = isMyRed;
            }
            else if (idMember2 != DataManager.Instance.DataUser.id && !RoomManager.Instance.currentRoom.isMaster)
            {
                data.iAmRed = isMyRed;
            }

            // Giữ logic cũ (nếu server có field lượt đánh thì thay bằng giá trị server)
            data.myTurn = data.iAmRed;

            UnityEngine.Debug.Log($"[OnReceive] StartGame: boardType={boardType} rows={rows} cols={cols} pieces={totalPieces}");

            // TODO: truyền sang BoardController
            GameManager.Instance.gameCoordinator.boardController.InitializeFromServer(data);
        }

        private void OnReceiveTurnMove(Message msg)
        {
            int id = msg.Reader.readInt();
            long timeRemain = msg.Reader.readLong();


            UnityEngine.Debug.Log("[OnReceive] TurnMove: id=" + id + " timeRemain=" + timeRemain);
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
            UnityEngine.Debug.Log("[OnReceive] MoveResult: id=" + id + " to (" + toY + "," + toX + ") type=" + type);
        }
        private bool IsChessBoard(byte boardType, int rows, int cols)
        {
            // Ưu tiên rows/cols nếu server set chuẩn 8x8 cho Chess
            if (rows == 8 && cols == 8) return true;
            return boardType == 2 || boardType == 3;
        }
    }
}
[System.Serializable]
public class DataReceiveReady
{
    public int id;
    public bool isReady;
}
