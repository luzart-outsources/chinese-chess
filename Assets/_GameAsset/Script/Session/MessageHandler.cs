﻿using NetworkClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets._GameAsset.Script.Session
{
    public class MessageHandler
    {
        public SessionMe session { get; protected set; }
        public ReadMessage readMsg { get; protected set; }
        public MessageHandler(SessionMe session)
        {
            this.session = session;
            readMsg = new ReadMessage(this);
        }

        public void onMessage(Message msg)
        {
            Message msgClone = msg;
            MainThreadDispatcher.Enqueue(() => OnMessageMainThread(msgClone));
        }
        private void OnMessageMainThread(Message msg)
        {
            UIManager.Instance.HideUiActive(UIName.Loading);
            switch (msg.Command)
            {
                case 1:
                    readMsg.LoginComplete(msg);
                    break;
                case 2:
                    readMsg.OnReceiveCreateData(msg);
                    break;
                case 3:
                    readMsg.ShowDialog(msg);
                    break;
                case 4:
                    readMsg.RefreshData(msg);
                    break;
                case 10:
                    readMsg.OnReceiveListRoomData(msg);
                    break;
                case 11:
                    readMsg.OnReceiveCreateRoomData(msg);
                    break;

            }
        }
    }
}
