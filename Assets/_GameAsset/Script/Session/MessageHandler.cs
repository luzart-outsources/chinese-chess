using NetworkClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            switch (msg.Command)
            {
                case 1:
                    readMsg.login(msg);
                    break;
            }
        }
    }
}
