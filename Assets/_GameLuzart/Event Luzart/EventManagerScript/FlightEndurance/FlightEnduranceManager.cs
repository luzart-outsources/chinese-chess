namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;

    public class FlightEnduranceManager : BaseEventManager
    {
        private const string PATH_FLIGHT_ENDURANCE = "flight_endurance";
        public const int CountToWin = 7;
        public const int TotalPeople = 50;
        public override EEventName eEvent => EEventName.FlightEndurance;
        private const int TimeCountdownLoss = 30*60;
        private const int TimeCountdownWin = 2 * 24 * 60 * 60;
    
        public int CountReward
        {
            get
            {
                return dataFlightEndurance.dbEvent.GetRewardType(ETypeResource.None).gifts[0].groupGift.dataResources[0].amount;
            }
        }
        public DataFlightEndurance dataFlightEndurance;
        public bool isFirstTimeFailed { get; set; } = false;
        public override TimeEvent GetTimeEvent
        {
            get
            {
                TimeEvent timeEvent = new TimeEvent();
                timeEvent.timeStart = TimeUtils.GetLongTimeStartToday;
                timeEvent.timeEndPreview = 0;
                timeEvent.timeStartReview = 24*60*60;
                timeEvent.timeEnd = 0;
                return timeEvent;
            }
        }
    
        public override void LoadData()
        {
            if (!EventManager.Instance.IsHasEvent(EEventName.FlightEndurance))
            {
                return;
            }
            DB_Event dbEvent = EventManager.Instance.GetEvent(EEventName.FlightEndurance);
            dataFlightEndurance = SaveLoadUtil.LoadDataPrefs<DataFlightEndurance>(PATH_FLIGHT_ENDURANCE);
            if (dataFlightEndurance == null)
            {
                dataFlightEndurance = new DataFlightEndurance();
                dataFlightEndurance.dbEvent = new DB_Event();
            }
            DB_Event db_EventLocal = dataFlightEndurance.dbEvent;
            if (string.IsNullOrEmpty(db_EventLocal.idEvent) || !db_EventLocal.idEvent.Equals(dbEvent.idEvent))
            {
                long timeCountWin = dataFlightEndurance.countTimeWin;
                dataFlightEndurance = new DataFlightEndurance();
                dataFlightEndurance.dbEvent = dbEvent;
                dataFlightEndurance.countTimeWin = (int)timeCountWin;
            }
            else
            {
                db_EventLocal.idEvent = dbEvent.idEvent;
                db_EventLocal.timeEvent = dbEvent.timeEvent;
                int length = dbEvent.allTypeResources.Length;
                List<DB_GiftEvent> list = new List<DB_GiftEvent>();
                for (int i = 0; i < length; i++)
                {
                    DB_GiftEvent db = new DB_GiftEvent();
                    db.type = dbEvent.allTypeResources[i].type;
                    int lengthGift = dbEvent.allTypeResources[i].gifts.Count;
                    db.gifts = new List<GiftEvent>();
                    for (int j = 0; j < lengthGift; j++)
                    {
                        GiftEvent gift = new GiftEvent();
                        gift.groupGift = dbEvent.allTypeResources[i].gifts[j].groupGift;
                        gift.require = dbEvent.allTypeResources[i].gifts[j].require;
                        gift.isClaimed = db_EventLocal.allTypeResources[i].gifts[j].isClaimed;
                        db.gifts.Add(gift);
                    }
                    list.Add(db);
                }
                db_EventLocal.allTypeResources = list.ToArray();
    
            }
            CalculateTimeReturn();
        }
        private void CalculateTimeReturn()
        {
            DB_Event dbEvent = EventManager.Instance.GetEvent(EEventName.FlightEndurance);
            long timeLoss = dataFlightEndurance.countTimeLoss;
            long timeWin = dataFlightEndurance.countTimeWin;
            long timeEndOnline = dataFlightEndurance.timeEndOnline;
            long timeCurrent = TimeUtils.GetLongTimeCurrent;
            long delta = timeCurrent - timeEndOnline;
    
            if (timeLoss > 0)
            {
                long timeRemain = timeLoss - delta;
                if (timeRemain > 0)
                {
                    dataFlightEndurance.countTimeLoss = (int)timeRemain;
                }
                else
                {
                    dataFlightEndurance = new DataFlightEndurance();
                    dataFlightEndurance.dbEvent = dbEvent;
                }
            }
            if(timeWin > 0)
            {
                long timeRemain = timeWin - delta;
                if (timeRemain > 0)
                {
                    dataFlightEndurance.countTimeWin = (int)timeRemain;
                }
                else
                {
                    dataFlightEndurance = new DataFlightEndurance();
                    dataFlightEndurance.dbEvent = dbEvent;
                }
            }
    
            
    
        }
    
        public override void SaveData()
        {
            SaveLoadUtil.SaveDataPrefs(dataFlightEndurance, PATH_FLIGHT_ENDURANCE);
        }
    
        public override void OnStartGame(int level)
        {
            base.OnStartGame(level);
            int total = dataFlightEndurance.totalPeople;
            int count = CountToWin - dataFlightEndurance.winStreak;
            playerMissing = GameUtil.GetRandomValueInCountStack(total, dataFlightEndurance.countPlayerEnd, count, 3);
            isInGame = true;
        }
        public bool isInGame = false;
    
        public override void OnCompleteLevelData(int level)
        {
            base.OnCompleteLevelData(level);
            if (!isInGame)
            {
                return;
            }
            isInGame = false;
            if (IsLoss || dataFlightEndurance.isWin || !dataFlightEndurance.isShowStart)
            {
                return;
            }
            dataFlightEndurance.winStreak++;
            RemovePlayer();
            if (dataFlightEndurance.winStreak >= 7)
            {
                dataFlightEndurance.countTimeWin = TimeCountdownWin;
                dataFlightEndurance.isShowNoti = false;
                dataFlightEndurance.isShowStart = false;
                int count = dataFlightEndurance.listTotalPeople.Count;
                int gold = CountReward / count;

                DataWrapperGame.ReceiveReward(ValueFirebase.FlightEnduranceReceive, new DataResource(new DataTypeResource(RES_type.Gold), gold));
                //DataManager.Instance.ReceiveRes(dataResource: new DataResource(new DataTypeResource(RES_type.Gold), gold)); 
            }
            SaveData();
        }
        public long TimeContain
        {
            get
            {
                long timeCurrent = TimeUtils.GetLongTimeCurrent;
                long timeContain = dataFlightEndurance.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
                return timeContain;
            }
        }
        public override void OnLoseLevelData(int level)
        {
            base.OnLoseLevelData(level);
            if (!isInGame)
            {
                return;
            }
            isInGame = false;
            if (IsLoss || !dataFlightEndurance.isShowStart || dataFlightEndurance.isWin)
            {
                return;
            }
            isFirstTimeFailed = true;
            dataFlightEndurance.winStreak = 0;
            dataFlightEndurance.countTimeLoss = TimeCountdownLoss;
            dataFlightEndurance.isShowNoti = false;
            dataFlightEndurance.isShowStart = false;
            RemovePlayer(true);
            SaveData();
    
        }
        private void Start()
        {
            Observer.Instance.AddObserver(ObserverKey.TimeActionPerSecond, OnPerSecond);
        }
        private void OnDestroy()
        {
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnPerSecond);
        }
        public void StartEvent()
        {
            dataFlightEndurance.winStreak = 0;
            dataFlightEndurance.listVisual = new List<DataSeatFlight>();
            int maxAvt = DataWrapperGame.AllSpriteAvatars.Length;
            int length = TotalPeople;
            for (int i = 0; i < length; i++)
            {
                int index = i;
                int idRan = UnityEngine.Random.Range(1, maxAvt);
                DataSeatFlight dataSeatFlight = new DataSeatFlight();
                dataSeatFlight.id = index;
                dataSeatFlight.idAvt = idRan;
                dataFlightEndurance.listVisual.Add(dataSeatFlight);
            }
            int id = UnityEngine.Random.Range(0, length);
            dataFlightEndurance.idMe = id;
            var data = dataFlightEndurance.listVisual[id];
            data.id = id;
            data.idAvt = DataWrapperGame.IDAvatarPlayer;
            dataFlightEndurance.listVisual[id]= data;
            CalculateUserEnd();
    
    
        }
        private void CalculateUserEnd()
        {
            int levelCurrent = DataWrapperGame.CurrentLevel;
            if(levelCurrent >= 110)
            {
                dataFlightEndurance.countPlayerEnd = 1;
            }
            else
            {
                dataFlightEndurance.countPlayerEnd = (100 - levelCurrent) / 10 + 2;
            }
        }
        private void OnPerSecond(object data)
        {
            if (dataFlightEndurance == null || dataFlightEndurance.dbEvent == null)
            {
                return;
            }
            dataFlightEndurance.countTimeLoss--;
            dataFlightEndurance.countTimeWin--;
            dataFlightEndurance.timeEndOnline = TimeUtils.GetLongTimeCurrent;
        }
        public bool IsLoss
        {
            get
            {
                return dataFlightEndurance.countTimeLoss >= 0;
            }
        }
        public bool IsWin
        {
            get
            {
                return dataFlightEndurance.isWin;
            }
        }
        public int playerMissing = 0;
        public List<DataSeatFlight> listTrueCache = new List<DataSeatFlight>();
        private void RemovePlayer(bool hasMe = false)
        {
            int idMe = -1;
            int playerCount = playerMissing;
            if (!hasMe)
            {
                idMe = dataFlightEndurance.idMe;
            }
            listTrueCache = new List<DataSeatFlight>();
            List<DataSeatFlight> listCanSub = new List<DataSeatFlight>();
            int length = dataFlightEndurance.listVisual.Count;
            for (int i = 0; i < length; i++)
            {
                if (dataFlightEndurance.listVisual[i].id != -1 && (dataFlightEndurance.listVisual[i].id != idMe))
                {
                    listCanSub.Add(dataFlightEndurance.listVisual[i]);
                }
            }
            int max = listCanSub.Count;
            if(max == 0)
            {
                return;
            }
            if (hasMe)
            {
                int id = dataFlightEndurance.idMe;
                DataSeatFlight foo = dataFlightEndurance.listVisual[id];
                listTrueCache.Add(foo);
                listCanSub.Remove(foo);
                max--;
            }
            for (int i = 0; i < playerCount; i++)
            {
                int ran = UnityEngine.Random.Range(0,max);
                DataSeatFlight foo = listCanSub[ran];
                listTrueCache.Add(foo);
                listCanSub.Remove(foo);
                max--;
            }
    
            int lengthTrue = listTrueCache.Count;
            for (int i = 0; i < lengthTrue; i++)
            {
                var data = dataFlightEndurance.listVisual[listTrueCache[i].id];
                data.id = -1;
                dataFlightEndurance.listVisual[listTrueCache[i].id] = data;
            }
        }
    
    }
    [System.Serializable]
    public class DataFlightEndurance
    {
        public DB_Event dbEvent;
    #if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
    #endif
        public int totalPeople
        {
            get
            {
                return listTotalPeople.Count;
            }
        }
    #if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
    #endif
        public List<DataSeatFlight> listTotalPeople
        {
            get
            {
                List < DataSeatFlight > listCache = new List < DataSeatFlight >();
                if (listVisual == null)
                {
                    return listCache;
                }
                int length = listVisual.Count;
                if (length == 0)
                {
                    return listCache;
                }
                for (int i = 0; i < length; i++)
                {
                    if (listVisual[i].id != -1)
                    {
                        listCache.Add(listVisual[i]);
                    }
                }
                return listCache;
            }
        }
        public bool isWin
        {
            get
            {
                return countTimeWin > 0;
            }
        }
        public int winStreak;
        public int countTimeLoss;
        public int countTimeWin;
        public bool isShowStart = false;
        public bool isShowNoti = false;
        public List<DataSeatFlight> listVisual = new List<DataSeatFlight>();
        public int idMe;
        public int countPlayerEnd = 1;
        public long timeEndOnline = 0;
     }
    [System.Serializable]
    public struct DataSeatFlight
    {
        public int id;
        public int idAvt;
    }
}
