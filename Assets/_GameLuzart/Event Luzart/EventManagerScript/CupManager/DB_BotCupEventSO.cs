namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    [CreateAssetMenu(fileName = "DB_BOTCupEventSO", menuName = "SO/DB_BOTCupEventSO")]
    public class DB_BotCupEventSO : ScriptableObject
    {
        [SerializeField]
        private DBBotCup[] dbBotCups;
        private Dictionary<int, DBBotCup> dictDBBotCup = new Dictionary<int, DBBotCup> ();


        public int GetMaxDBBot()
        {
            if(dbBotCups == null )
            {
                return 0;
            }
            return dbBotCups.Length;
        }
    
        public DBBotCup GetDB_Bot(int index)
        {
            InitDictionary();
            return dbBotCups[index];
        }
        private void InitDictionary()
        {
            if(dictDBBotCup!=null && dictDBBotCup.Count > 0)
            {
                return;
            }
            dictDBBotCup.Clear();
            int length = dbBotCups.Length;
            for (int i = 0; i < length; i++)
            {
                int idBot = i;
                var dbBot = dbBotCups [idBot];
                dictDBBotCup.Add(idBot, dbBot);
            }
        }
    }
}
