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
            UnityEngine.Debug.Log("RequestGetRoom: " + eChessType);
        }
        public void RequestCreateRoom(EChessType eChessType, int gold, bool isFlash)
        {
            var msg = new Message(10);
            msg.Writer.writeByte(1);
            msg.Writer.writeByte((byte)eChessType);
            msg.Writer.writeInt(gold);
            msg.Writer.writeBool(isFlash);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("RequestCreateRoom: " + eChessType + " - " + gold + " - " + isFlash);
        }
        public void RequestJoinRoom(int idRoom, bool isViewer)
        {
            var msg = new Message(10);
            msg.Writer.writeByte(2);
            msg.Writer.writeInt(idRoom);
            msg.Writer.writeBool(isViewer);
            SessionMe.Instance.SendMessage(msg);
            UnityEngine.Debug.Log("RequestJoinRoom: " + idRoom + " - " + isViewer);
        }

        public void RequestReady(bool isReady)
        {
            var msg = new Message(11);
            msg.Writer.writeByte(3);
            msg.Writer.writeBool(isReady);
            SessionMe.Instance.SendMessage(msg);
        }

        public void RequestLeaveRoom()
        {
            var msg = new Message(11);
            msg.Writer.writeByte(4);
            SessionMe.Instance.SendMessage(msg);
        }
    }
}
