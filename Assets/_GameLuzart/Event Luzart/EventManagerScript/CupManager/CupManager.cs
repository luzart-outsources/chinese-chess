namespace Luzart
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using static BattlePassManager;
    
    public class CupManager : BaseEventManager
    {
        string[] names = new string[]
    {
        "Alex", "Bella", "Cameron", "Diana", "Ethan", "Fiona", "Gabriel", "Hannah",
        "Isaac", "Julia", "Kevin", "Luna", "Mason", "Nina", "Oscar", "Paula",
        "Quentin", "Riley", "Sophia", "Tyler", "Uma", "Victor", "Wendy", "Xavier",
        "Yvonne", "Zack", "Amber", "Brian", "Carly", "Derek", "Elena", "Felix",
        "Grace", "Harry", "Ivy", "Jake", "Kara", "Leo", "Mia", "Nathan", "Olivia",
        "Peter", "Quinn", "Rose", "Samuel", "Tina", "Ursula", "Vince", "Whitney",
        "Xander", "Yasmine", "Zoey", "Adam", "Brenda", "Caleb", "Daisy", "Evan",
        "Frances", "George", "Hailey", "Ian", "Jenna", "Kyle", "Laura", "Miles",
        "Nora", "Owen", "Piper", "Quincy", "Ryan", "Sierra", "Thomas", "Ulysses",
        "Vanessa", "Walter", "Xena", "Yvette", "Zane", "Annie", "Ben", "Clara",
        "Dean", "Ella", "Freddie", "Gina", "Harper", "Ira", "Jack", "Katie",
        "Liam", "Molly", "Noah", "Olga", "Paul", "Quilla", "Rita", "Scott",
        "Tara", "Ulrich", "Vera", "Wesley", "Ximena", "Yuri", "Zelda"
    };

        public DB_BotCupEventSO dbBotCupSO;
        public override EEventName eEvent => EEventName.Cup;
    
        public bool isClickPlay = false;
        public bool IsCacheShowVisual { get; set; } = false;
        public override TimeEvent GetTimeEvent 
        {
            get
            {
                TimeEvent timeEvent = new TimeEvent();
                long timeStart = TimeUtils.GetLongTimeDayOfCurrentWeek(System.DayOfWeek.Wednesday);
                timeEvent.timeStart = timeStart;
                timeEvent.timeEndPreview = 0;
                timeEvent.timeStartReview = 2*24*60*60;
                timeEvent.timeEnd = 0;
                return timeEvent;
            }
        }
        public DataCup dataCup;
        private const string PATH_EVENT_CUP = "cup";
        private DB_Event dbEventServer;
        public DifficultyKey[] diffKey = new DifficultyKey[3];
        private Dictionary<Difficulty, int> _dictDiffKey = null;
        public Dictionary<Difficulty, int> dictDiffKey
        {
            get
            {
                if (_dictDiffKey == null)
                {
                    _dictDiffKey = new Dictionary<Difficulty, int>();
                    int length = diffKey.Length;
                    for (int i = 0; i < length; i++)
                    {
                        var item = diffKey[i];
                        _dictDiffKey.Add(item.difficulty, item.amountKey);
                    }
                }
                return _dictDiffKey;
            }
        }
        public override void LoadData()
        {
            dataCup = SaveLoadUtil.LoadDataPrefs<DataCup>(PATH_EVENT_CUP);
            if(dataCup == null)
            {
                dataCup = new DataCup();
                dataCup.dbEvent = new DB_Event();
            }
            // Event local
    
            // Event Server
            dbEventServer = EventManager.Instance.GetEvent(eEvent);
            
            // Neu khong co Event Local 
            if (!isHasEventLocal)
            {
                return;
            }
    
            UpdateBotByTime(false);
            SortAllCup();
    
        }
        public void StartEvent()
        {
            dataCup = new DataCup();
            dataCup.dbEvent = dbEventServer;
            StartInitAllUser();
            dataCup.lastTimeUpdate = TimeUtils.GetLongTimeCurrent;
        }
        public void StartInitAllUser()
        {
            int maxName = names.Length;
            int countBot = dbBotCupSO.GetMaxDBBot();
            int maxAvt = DataWrapperGame.AllSpriteAvatars.Length;
            dataCup.listDataBot.Clear();
            //
            DataBotCup dbMe = new DataBotCup();
            dbMe.nameBot = DataWrapperGame.NamePlayer;
            dbMe.idBot = -1;
            dbMe.idAvt = DataWrapperGame.IDAvatarPlayer;
            dbMe.point = 1;
            dataCup.listDataBot.Add(dbMe);
            //
            for (int i = 0; i < 49; i++)
            {
                int random = Random.Range(0, maxName);
                DataBotCup db = new DataBotCup();
                db.nameBot = names[random];
                db.idBot = Random.Range(0,countBot);
                db.idAvt = Random.Range(0, maxAvt);
                dataCup.listDataBot.Add(db);    
            }
    
        }
        public DataBotCup GetDataCupMe()
        {
            return dataCup.listDataBot.FirstOrDefault(item => item.idBot == -1);
        }
        public int GetIndexMe()
        {
            SortAllCup();
            int indexMe = dataCup.listDataBot.FindIndex(item => item.idBot == -1);
            return indexMe;
        }
        public int GetPointInBot(int idBot)
        {
            return dbBotCupSO.GetDB_Bot(idBot).pointUpdate;
        }
        public override void SaveData()
        {
            SaveLoadUtil.SaveDataPrefs(dataCup,PATH_EVENT_CUP);
        }
        private bool isHasEventLocal
        {
            get
            {
                if (dataCup == null || dataCup.dbEvent == null || string.IsNullOrEmpty(dataCup.dbEvent.idEvent))
                {
                    return false;
                }
                return true;
            }
        }
        public bool IsHasEventLocal()
        {
            return isHasEventLocal;
        }
        public bool isHasEventServerCurrent 
        {
            get
            {
                return !(dbEventServer == null || string.IsNullOrEmpty(dbEventServer.idEvent));
            }
        }
        public bool IsHasEventServerCurrent()
        {
            return isHasEventServerCurrent;
        }
        public bool IsEventFinish()
        {
            if(!isHasEventLocal)
            {
                return false;
            }
            if (isHasEventServerCurrent && dbEventServer.idEvent.Equals(dataCup.dbEvent.idEvent))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void DeleteDBEvent()
        {
            dataCup = new DataCup();
            SaveData();
        }
        public int valueKey = 0;
        public int indexCache = 0;
        public override void OnCompleteLevelData(int level)
        {
            base.OnCompleteLevelVisual(level);
            if(IsEventFinish())
            {
                return;
            }
            if (!isHasEventLocal)
            {
                StartEvent();
                SaveData();
                return;
            }
            //
            Difficulty diff = DataWrapperGame.diff;
            int key = dictDiffKey[diff];
            valueKey = key;
            //
            ForceUpdateBot();
            //
            indexCache = GetIndexMe();
            //
            DataBotCup dataBot = GetDataCupMe();
            dataBot.point += key;
            SortAllCup();
            //
            IsCacheShowVisual = true;
            SaveData();
        }
        public override void OnLoseLevelData(int level)
        {
            base.OnLoseLevelData(level);
            UpdateBotByTimeOnline();
        }
        public void UpdateBotByTimeOnline()
        {
            UpdateBotByTime();
        }
        private void ForceUpdateBot()
        {
            UpdateBotByTimeUpdate(TimeDeltaToForceUpdateOnline);
        }
        public void UpdateBotByTime(bool isOnline = true)
        {
            int timeUpdate = isOnline ? TimeDeltaToUpdateOnline : TimeDeltaToUpdateOffline;
            UpdateBotByTimeUpdate(timeUpdate);
        }
        private void UpdateBotByTimeUpdate(int time)
        {
            long timePre = dataCup.lastTimeUpdate;
            if (dataCup.lastTimeUpdate == 0)
            {
                dataCup.lastTimeUpdate = TimeUtils.GetLongTimeCurrent;
            }
            long timeCurrent = TimeUtils.GetLongTimeCurrent;
            long deltaTime = timeCurrent - timePre;
            int timeUpdate = time;
            int count = ((int)deltaTime) / timeUpdate;
            for (int i = 0; i < count; i++)
            {
                UpdateBot();
            }
            if (count != 0)
            {
                dataCup.lastTimeUpdate = timeCurrent;
            }
    
        }
        private const int TimeDeltaToForceUpdateOnline = 600;
        private const int TimeDeltaToUpdateOnline = 1000;
        private const int TimeDeltaToUpdateOffline = 3600;
        public void UpdateBot()
        {
            int length = dataCup.listDataBot.Count;
            for (int i = 0; i < length; i++)
            {
                var dataBot = dataCup.listDataBot[i];
                if (dataBot.idBot == -1)
                {
                    continue;
                }
                int point = GetPointInBot(dataBot.idBot);
                dataBot.point += point;
            }
            SortAllCup();
        }
        public void SortAllCup()
        {
            dataCup.listDataBot.Sort((x, y) => y.point.CompareTo(x.point));
        }
    }
    
    [System.Serializable]
    public class DataCup
    {
        public DB_Event dbEvent;
        public bool isEnd = false;
        public List<DataBotCup> listDataBot = new List<DataBotCup> ();
        public long lastTimeUpdate;
    }
    
    [System.Serializable]
    public class DataBotCup
    {
        public int idBot;
        public string nameBot = "Player";
        public int point = 0;
        public int idAvt;
    
    }
    
    [System.Serializable]
    public class DBBotCup
    {
        // Point
        [SerializeField]
        private int pointUpdateMin = -1;
        [SerializeField]
        private int pointUpdateMax = 1;
    
        public int pointUpdate
        {
            get
            {
                int value = Random.Range(pointUpdateMin, pointUpdateMax + 1);
                if (value <= 0)
                {
                    value = 0;
                }
                return value;
            }
        }
    }
}
