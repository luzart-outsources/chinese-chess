using NetworkClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._GameAsset.Script.Session
{
    public class GlobalServices
    {
        private static GlobalServices instance;
        public static GlobalServices Instance => instance ?? (instance = new GlobalServices());

        public void login(string username, string password)
        {
            try
            {
                var msg = new Message(1);
                msg.Writer.writeByte(0);
                msg.Writer.writeString(username);
                msg.Writer.writeString(password);

                SessionMe.Instance.SendMessage(msg);
                UnityEngine.Debug.Log("[Post] Login: " + username);
            }
            catch (Exception e)
            {

            }
        }
        public void Register(string username, string phoneNumber, string password)
        {
            try
            {
                var msg = new Message(1);
                msg.Writer.writeByte(1);
                msg.Writer.writeString(username);
                msg.Writer.writeString(phoneNumber);
                msg.Writer.writeString(password);

                SessionMe.Instance.SendMessage(msg);
                UnityEngine.Debug.Log("[Post] Register: " + username + " - " + phoneNumber);
            }
            catch (Exception e)
            {

            }
        }
        public void PostNameUser(string namePlayer)
        {
            try
            {
                var msg = new Message(2);
                msg.Writer.writeString(namePlayer);
                SessionMe.Instance.SendMessage(msg);
                UnityEngine.Debug.Log("[Post] PostNameUser: " + namePlayer);
            }
            catch (Exception e)
            {

            }
        }
        public void RequestGetRoom(EChessType eChessType)
        {
            var msg = new Message(10);
            msg.Writer.writeByte(0);
            msg.Writer.writeByte((byte)eChessType);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post]RequestGetRoom: " + eChessType);
        }
        public void RequestCreateRoom(EChessType eChessType, int gold, bool isFlash)
        {
            var msg = new Message(10);
            msg.Writer.writeByte(1);
            msg.Writer.writeByte((byte)eChessType);
            msg.Writer.writeInt(gold);
            msg.Writer.writeBool(isFlash);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post]RequestCreateRoom: " + eChessType + " - " + gold + " - " + isFlash);
        }
        public void RequestJoinRoom(int idRoom, bool isViewer)
        {
            var msg = new Message(10);
            msg.Writer.writeByte(2);
            msg.Writer.writeInt(idRoom);
            msg.Writer.writeBool(isViewer);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post]RequestJoinRoom: " + idRoom + " - " + isViewer);
        }

        public void RequestReady(bool isReady)
        {
            var msg = new Message(11);
            msg.Writer.writeByte(3);
            msg.Writer.writeBool(isReady);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post] RequestReady: " + isReady);
        }

        public void RequestLeaveRoom()
        {
            var msg = new Message(11);
            msg.Writer.writeByte(4);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post] RequestLeaveRoom");
        }

        public void RequestMove(short idPiece, short toRow, short toCol, sbyte type = -1 )
        {
            var msg = new Message(12);
            msg.Writer.writeByte(1);
            msg.Writer.writeShort(idPiece);
            msg.Writer.writeShort(toCol);
            msg.Writer.writeShort(toRow);
            msg.Writer.writeSByte(type);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post] RequestMove: " + idPiece + " - " + toCol + " - " + toRow);

        }
        public void RequestHoa(bool isXinThua)
        {
            var msg = new Message(12);
            msg.Writer.writeByte(2);
            msg.Writer.writeByte(isXinThua ? (byte)1 : (byte)0);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post] RequestHoa");
        }
        public void RequestChatInGame(byte user = 0 , string content = ":)")
        {
            var msg = new Message(5);
            msg.Writer.writeByte(user);
            msg.Writer.writeString(content);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post] RequestChatInGame: " + content);
        }

        public void RequestChatWorld(string content = "Hello everyone!")
        {
            var msg = new Message(5);
            msg.Writer.writeByte(2);
            msg.Writer.writeString(content);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post] RequestChatWorld: " + content);
        }

        public void RequestChatP2P(string username, string content = "Hello!")
        {
            var msg = new Message(13);
            msg.Writer.writeByte(2);
            msg.Writer.writeString(username);
            msg.Writer.writeString(content);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post] RequestChatP2P: " + username + " - " + content);
        }
        public void RequestSeeChess(EChessType eChessType)
        {
            var msg = new Message(10);
            msg.Writer.writeByte(3);
            msg.Writer.writeByte(4);
            msg.Writer.writeByte(0);
            msg.Writer.writeByte(1);
            msg.Writer.writeByte(2);
            msg.Writer.writeByte(3);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post]RequestSeeChess: " + eChessType);
        }
        public void RequestGetInfoUser(string username)
        {
            var msg = new Message(13);
            msg.Writer.writeByte(1);
            msg.Writer.writeString(username);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("[Post]RequestGetInfoUser");
        }
    }
}
