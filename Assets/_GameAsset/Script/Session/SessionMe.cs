using NetworkClient.Interfaces;
using NetworkClient.Models;
using NetworkClient.Network.Tcp;
using NetworkClient.Network.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._GameAsset.Script.Session
{
    public class SessionMe
    {
        const string hostTCP = "103.82.21.235";
        const int port = 36526;

        private static SessionMe _instance;
        public static SessionMe Instance => _instance ?? (_instance = new SessionMe());
        public MessageHandler messageHandler;
        public INetworkConnection network;


        public SessionMe() 
        {
            messageHandler = new MessageHandler(this);
#if UNITY_ANDROID 
            network = new TcpClientHandler(messageHandler.onMessage, Disconnect);
#endif
        }
        public void Connect()
        {
            if(network == null || !network.IsConnected && !network.IsWaitConnect)
                network.Connect(hostTCP, port);
        }

        public void Disconnect()
        {
            if(network != null && !network.IsConnected)
            {
                Action action = () =>
                {
                    var ui = UIManager.Instance.ShowUI<UINotiAButton>(UIName.NotiAButton);
                    ui.InitPopup(null, "Lỗi kết nối mạng", "Không thể kết nối với máy chủ", "Thử lại");
                };
                MainThreadDispatcher.Enqueue(action);
            }

            if (network == null && network.IsConnected)
            {
                network.Disconnect();
            }
        }

        public void SendMessage(Message msg)
        {
            if (network != null && network.IsConnected)
                network.Send(msg);
            msg.Dispose();
        }
    }
}
