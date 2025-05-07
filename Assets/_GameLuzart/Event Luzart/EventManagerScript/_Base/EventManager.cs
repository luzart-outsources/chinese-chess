namespace Luzart
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public partial class EventManager : Singleton<EventManager>
    {
        private void Awake()
        {
            Initialize(); 
        }
        private void Start()
        {
            StartFuncionInGame();
        }
        private void OnDestroy()
        {
            DestroyFuncionInGame();
        }
        [Space]
        [Header("Event Manager")]
        public List<BaseEventManager> baseEventManagers;

        private const string PATH_EVENT = "data_event";
        public bool IsInit { get; set; } = false;
        public List<DB_Event> list_Event = new List<DB_Event>();
    
        //
        
        private Dictionary<EEventName, DB_Event> dictListEvent = new Dictionary<EEventName, DB_Event>();
    
        private Dictionary<EEventName, DB_Event> dictDBEEvent = new Dictionary<EEventName, DB_Event>();
    
        private Dictionary<EEventName, BaseEventManager> dictBaseEventManager = new Dictionary<EEventName, BaseEventManager>();
        //
    
        public List<EventUnlockLevel> list_EventUnlockLevel = new List<EventUnlockLevel>();
        private Dictionary<EEventName, int> dictEventUnlock = new Dictionary<EEventName, int>();
        //
    
    
        [Space]
        [Header("Only Read")]
        public List<DB_Event> events = new List<DB_Event>();

        public DataEvent dataEvent
        {
            get => dbEventJson.dataEvent;
            set => dbEventJson.dataEvent = value;
        }
        public long TimeFirstTimeStartGame
        {
            get
            {
                return dbEventJson.dataEvent.timeFirstTime;
            }
        }
        [SerializeField]
        private DB_EventJSon dbEventJson;
        //

        public void Initialize()
        {
            LoadEvent();
            InitDictEvent();
            ConfigDBEvent_Firebase();
            //
    
            ConfigDB_Event();
            InitAllEventLocal();
    
        }
        private void InitAllEventLocal()
        {
            int length = baseEventManagers.Count;
            for (int i = 0; i < length; i++)
            {
                baseEventManagers[i].LoadData();
            }
        }
        public void SaveDataEvent()
        {
            int length = baseEventManagers.Count;
            for (int i = 0; i < length; i++)
            {
                baseEventManagers[i].SaveData();
            }
        }
        private void InitDictEvent()
        {
            dictListEvent.Clear();
            dictListEvent.AddListToDictionary<DB_Event, EEventName, DB_Event>(list_Event, e => new KeyValuePair<EEventName, DB_Event>(e.eventName, e));

            dictDBEEvent.Clear();
            dictDBEEvent.AddListToDictionary<DB_Event, EEventName, DB_Event>(events, e => new KeyValuePair<EEventName, DB_Event>(e.eventName, e));

            dictBaseEventManager.Clear();
            dictBaseEventManager.AddListToDictionary<BaseEventManager, EEventName, BaseEventManager>(baseEventManagers, e => new KeyValuePair<EEventName, BaseEventManager>(e.eEvent, e));

            dictEventUnlock.Clear();
            dictEventUnlock.AddListToDictionary<EventUnlockLevel, EEventName, int>(list_EventUnlockLevel, e => new KeyValuePair<EEventName, int>(e.eventName, e.level));

        }
        private void OnApplicationFocus()
        {
            SaveEvent();
        }
        public void LoadEvent()
        {
            dbEventJson = SaveLoadUtil.LoadDataPrefs<DB_EventJSon>(PATH_EVENT);
            if(dbEventJson == null )
            {
                dbEventJson = new DB_EventJSon();
                dbEventJson.dataEvent = new DataEvent();
                events = new List<DB_Event> ();
            }
            else
            {
                events = dbEventJson.events;
            }
            if(dbEventJson.dataEvent.timeFirstTime == -1)
            {
                dbEventJson.dataEvent.timeFirstTime = TimeUtils.GetLongTimeCurrent;
            }
            IsInit = true;
        }
        private void ConfigDB_Event()
        {
            if (events != null)
            {
                events.RemoveAll((db) => db.eventStatus == EEventStatus.Finish);
                int length = events.Count;  
                for (int i = 0; i < length; i++)
                {
                    var db_Event = events[i];
                    if(db_Event == null)
                    {
                        continue;
                    }
                    if(db_Event.allTypeResources == null)
                    {
                        db_Event.allTypeResources = dictListEvent[db_Event.eventName].allTypeResources;
                    }
                }
            }
            if (IsGetDataFirebase())
            {
                events = listEventToDo;
                if(db_EventJsonFirebase.listUnlockLevel.Count == list_EventUnlockLevel.Count)
                {
                    list_EventUnlockLevel = db_EventJsonFirebase.listUnlockLevel;
                }
            }
            ConfigEventLocal();
            InitDictEvent();
    
        }
        private void ConfigEventLocal()
        {
            int lengthEvent = list_Event.Count;
            for (int i = 0; i < lengthEvent; i++)
            {
                if (IsGetDataFirebase() && !IsNeedInitDBEventLocal_Firebase(list_Event[i].eventName))
                {
                    continue;
                }
                var db_Event = list_Event[i];
                db_Event.timeEvent = dictBaseEventManager[db_Event.eventName].GetTimeEvent;
                if (db_Event.eventStatus != EEventStatus.Finish)
                {
                    bool isHasEvent = false;
                    int length = events.Count;
                    for (int j = 0; j < length; j++)
                    {
                        if (events[j].idEvent.Equals($"{db_Event.eventName.ToString()}_{db_Event.timeEvent.timeStart}") || events[j].idEvent.Contains(db_Event.eventName.ToString()))
                        {
                            isHasEvent = true;
                            db_Event.idEvent = $"{db_Event.eventName.ToString()}_{db_Event.timeEvent.timeStart}";
                            events[j] = db_Event;
                        }
                    }
                    if (!isHasEvent)
                    {
                        db_Event.idEvent = $"{db_Event.eventName.ToString()}_{db_Event.timeEvent.timeStart}";
                        events.Add(db_Event);
                    }
                }
    
            }
            SaveEvent();
        }
        public void SaveEvent()
        {
            if (!IsInit)
            {
                return;
            }
            dbEventJson.events = events;
            SaveLoadUtil.SaveDataPrefs<DB_EventJSon>(dbEventJson, PATH_EVENT);
            SaveDataEventFirebase();
    
        }

        public DB_Event GetEvent(EEventName eventName) => dictDBEEvent.ContainsKey(eventName) ? dictDBEEvent[eventName] : null;

        public bool IsHasEvent(EEventName eventName) 
        {
            DB_Event db = GetEvent(eventName);
            if(db == null || db.eventStatus == EEventStatus.Finish)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool IsUnlockLevel(EEventName eEventName) => IsUnlockLevel(eEventName, DataWrapperGame.CurrentLevel);

        public int GetLevelUnlock(EEventName eEventName) => dictEventUnlock.ContainsKey(eEventName) ? dictEventUnlock[eEventName] : -1;

        public bool IsUnlockLevel(EEventName eEventName, int level) =>
            dictEventUnlock.ContainsKey(eEventName) && level >= dictEventUnlock[eEventName];

        public bool IsLevelUnlockExactly(EEventName eEventName, int level) =>
            dictEventUnlock.ContainsKey(eEventName) && level == dictEventUnlock[eEventName];

        public List<EEventName> GetEEventCurrent() =>
            events.Where(baseEvent => IsHasEvent(baseEvent.eventName) && IsUnlockLevel(baseEvent.eventName, DataWrapperGame.CurrentLevel))
                  .Select(baseEvent => baseEvent.eventName)
                  .ToList();
#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
#endif
        public void SaveAllData()
        {
            SaveDataEvent();
            SaveEvent();
        }
    

    
    
    
    
    
    
    
    
    
    }

    [System.Serializable]
    public class DB_Event
    {
        public string idEvent;
        public EEventName eventName;
        public TimeEvent timeEvent;
        public DB_GiftEvent[] allTypeResources;
    
    #if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
    #endif
        public EEventStatus eventStatus
        {
            get
            {
                long myTime = TimeUtils.GetLongTimeCurrent;
                if (myTime > timeEvent.timeStart && myTime < timeEvent.TimeStartActionUnixTime)
                {
                    return EEventStatus.Preview;
                }
                else if (myTime > timeEvent.TimeStartActionUnixTime && myTime < timeEvent.TimeEndActionUnixTime)
                {
                    return EEventStatus.Running;
                }
                else if (myTime > timeEvent.TimeEndActionUnixTime && myTime < timeEvent.TimeEndUnixTime)
                {
                    return EEventStatus.Review;
                }
                else
                {
                    return EEventStatus.Finish;
                }
            }
        }
        // 
    
        [System.NonSerialized]
        private Dictionary<ETypeResource, DB_GiftEvent> dictReward = new Dictionary<ETypeResource, DB_GiftEvent>();
        private void InitDictionaryTypeResource()
        {
            if (dictReward == null || dictReward.Count <= 0)
            {
                dictReward.Clear();
                if(allTypeResources == null)
                {
                    return;
                }
                int length = allTypeResources.Length;
                for (int i = 0; i < length; i++)
                {
                    dictReward.Add(allTypeResources[i].type, allTypeResources[i]);
                }
            }
        }
        public DB_GiftEvent GetRewardType(ETypeResource type)
        {
            InitDictionaryTypeResource();
            if (dictReward.ContainsKey(type))
            {
                return dictReward[type];
            }
            else
            {
                return default;
            }
        }
    }
    [System.Serializable]
    public struct TimeEvent
    {
        // Time timeStart --- timeStart + timeEndPreview --- timeStart + timeEnd --- timeStartReview + timeReview
        //AWAKE = PREVIEW  ============   START   ==========    END   ==========           REVIEW = DESTROY
    
        // Thoi gian event start, lay time Unix
        public long timeStart;
        // Thoi gian tinh tu timeStart
        public long timeEndPreview;
        // Thoi gian ket thuc game, tinh tu timeStart
        public long timeStartReview;
        // Thoi gian sau khi ket thuc game, de user doi thuong, tinh tu timeStart
        public long timeEnd;
        public long TimeStartActionUnixTime
        {
            get
            {
                return timeStart + timeEndPreview;
            }
        }
        public long TimeEndActionUnixTime
        {
            get
            {
                return TimeStartActionUnixTime + timeStartReview;
            }
        }
        public long TimeEndUnixTime
        {
            get
            {
                return TimeEndActionUnixTime + timeEnd;
            }
        }
        public long TimeContainEvent
        {
            get
            {
                return TimeEndActionUnixTime - TimeUtils.GetLongTimeCurrent;
            }
        }
    }
    [System.Serializable]
    public enum EEventStatus
    {
        Preview = 0,
        Running = 1,
        Review = 2,
        Finish = 3,
    }
    
    [System.Serializable]
    public enum EEventName
    {
        None = 0,
        BattlePass = 1,
        TicketTally = 2,
        JourneyToSuccess = 3,
        FlightEndurance = 4,
        RiseOfKittens = 5,
        Cup = 6,
        DailyLogin = 7,
        LuckySpin = 8,
        Racing = 9,
    }
    [System.Serializable]
    public enum ETypeResource
    {
        None = 0,
        VIP = 1,
        VIP1 = 2,
        VIP2 =3,
    }
    [System.Serializable]
    public struct DB_GiftEvent
    {
        public ETypeResource type;
        public List<GiftEvent> gifts;
    }
    [System.Serializable]
    public struct GiftEvent
    {
        public int require;
        public GroupDataResources groupGift;
        public bool isClaimed;
    
        public bool IsHasChest
        {
            get
            {
                return typeChest.id != 0;
            }
        }
        public DataTypeResource typeChest
        {
            get
            {
                return groupGift.typeChest;
            }
        }
    
    }
    
    [System.Serializable]
    public struct EventUnlockLevel
    {
        public EEventName eventName;
        public int level;
    }

    [System.Serializable]
    public class DB_EventJSon
    {
        public DataEvent dataEvent = new DataEvent();
        public List<DB_Event> events = new List<DB_Event>();
    }

    [System.Serializable]
    public class DataEvent
    {
        public long timeFirstTime = -1;
        [SerializeField]
        private List<DataEventTutorial> listEventTutorial = new List<DataEventTutorial>();
        private Dictionary<EEventName, bool> dictEventCompleteTutorial = new Dictionary<EEventName, bool>();
        public bool GetTutorialComplete(EEventName eventName)
        {
            InitDictEventCompleteTutorial();
            if (dictEventCompleteTutorial.ContainsKey(eventName))
            {
                return dictEventCompleteTutorial[eventName];
            }
            else
            {
                return false;
            }
        }
        public void SetTutorialComplete(EEventName eventName, bool isComplete)
        {
            InitDictEventCompleteTutorial();
            if (dictEventCompleteTutorial.ContainsKey(eventName))
            {
                dictEventCompleteTutorial[eventName] = isComplete;
            }
            else
            {
                dictEventCompleteTutorial.Add(eventName, isComplete);
            }
            listEventTutorial = dictEventCompleteTutorial.Select(x => new DataEventTutorial { eventName = x.Key, isCompleteTutorial = x.Value }).ToList();
        }
        private void InitDictEventCompleteTutorial()
        {
            if (dictEventCompleteTutorial == null || dictEventCompleteTutorial.Count <= 0)
            {
                dictEventCompleteTutorial.Clear();
                int length = listEventTutorial.Count;
                for (int i = 0; i < length; i++)
                {
                    if (dictEventCompleteTutorial.ContainsKey(listEventTutorial[i].eventName))
                    {
                        dictEventCompleteTutorial[listEventTutorial[i].eventName] = listEventTutorial[i].isCompleteTutorial;
                    }
                    else
                    {
                        dictEventCompleteTutorial.Add(listEventTutorial[i].eventName, listEventTutorial[i].isCompleteTutorial);
                    }
                }
            }
        }
    }
    [System.Serializable]
    public class DataEventTutorial
    {
        public EEventName eventName;
        public bool isCompleteTutorial = false;
    }
}
