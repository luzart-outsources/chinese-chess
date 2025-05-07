using Luzart;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;

public class RacingManager : BaseEventManager
{
    private const string PATH_RACING = "data_racing";
    public override EEventName eEvent => EEventName.Racing;
    private bool isLoadData = false;

    public override TimeEvent GetTimeEvent
    {
        get
        {
            TimeEvent timeEvent = new TimeEvent();
            timeEvent.timeStart = TimeUtils.GetLongTimeStartToday;
            timeEvent.timeStartReview = 60 * 60 * 24;
            return timeEvent;
        }
    }
    private const int timeEventDuration = 60 * 60 * 3; // 3h
    public const int CountUserRace = 4;
    public int MaxStep
    {
        get
        {
            return dbBotRacingSO.MaxLevel;
        }
    }
    public DataRacing dataEvent;
    public DB_BotRacingSO dbBotRacingSO;
    private string[] names = new string[]
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
    private string GetStringName()
    {
        int index = UnityEngine.Random.Range(0, names.Length);
        return names[index];
    }
    public override void LoadData()
    {
        if (!EventManager.Instance.IsHasEvent(eEvent))
        {
            return;
        }
        LoadPathData();
        DB_Event db_EventLocal = dataEvent.dbEvent;
        DB_Event dbEvent = EventManager.Instance.GetEvent(eEvent);
        if (string.IsNullOrEmpty(db_EventLocal.idEvent) || !db_EventLocal.idEvent.Equals(dbEvent.idEvent))
        {
            dataEvent = new DataRacing();
            dataEvent.dbEvent = dbEvent;
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
                    if (i < db_EventLocal.allTypeResources.Length && j < db_EventLocal.allTypeResources[i].gifts.Count)
                    {
                        gift.isClaimed = db_EventLocal.allTypeResources[i].gifts[j].isClaimed;
                    }
                    db.gifts.Add(gift);
                }
                list.Add(db);
            }
            db_EventLocal.allTypeResources = list.ToArray();
        }
        UpdateAndGetListDataBotRacingCurrent();
        RegisterTimeToCount();
    }

    public override void SaveData()
    {
        SaveLoadUtil.SaveDataPrefs(dataEvent, PATH_RACING);
    }
    private void LoadPathData()
    {
        dataEvent = SaveLoadUtil.LoadDataPrefs<DataRacing>(PATH_RACING);
        if (dataEvent == null)
        {
            dataEvent = new DataRacing();
            dataEvent.dbEvent = new DB_Event();
        }
    }
    private void RegisterTimeToCount()
    {
        if (IsEventProgress)
        {
            Observer.Instance?.AddObserver(ObserverKey.TimeActionPerSecond, OnPerSecond);
        }
    }
    public void StartEvent()
    {
        dataEvent.timeStartEvent = TimeUtils.GetLongTimeCurrent;
        dataEvent.timeEndEvent = dataEvent.timeStartEvent + timeEventDuration;
        RegisterTimeToCount();
        SaveData();
    }
    public void StartInitBot()
    {
        dataEvent.listDataBotRacing = new List<DataBotRacing>();
        var listDBBot = dbBotRacingSO.dbBotRacings.ToList();
        var randomNoRepeat = new RandomNoRepeat<DBBotRacing>(listDBBot);
        for (int i = 0; i < CountUserRace; i++)
        {
            var dbBot = randomNoRepeat.Random();
            //
            DataBotRacing dataBotRacing = new DataBotRacing();
            dataBotRacing.name = GetStringName();
            dataBotRacing.idBot = dbBot.id;
            dataBotRacing.lv = 0;
            dataEvent.listDataBotRacing.Add(dataBotRacing);
        }
        dataEvent.listDataBotRacing.Insert(0,dataPlayer);
        dataEvent.CloneListDataBotRacing();
        SaveData();
    }
    public List<DataBotRacing> UpdateAndGetListDataBotRacingCurrent()
    {
        if (!IsEventProgress)
        {
            return dataEvent.listDataBotRacing;
        }
        List<DataBotRacing> listDataBot = new List<DataBotRacing>();
        int length = dataEvent.listDataBotRacing.Count;
        for (int i = 0; i < length; i++)
        {
            var dataBot = dataEvent.listDataBotRacing[i];
            if(dataBot.idBot == -1)
            {
                dataBot.lv = dataEvent.totalUse;
            }
            else
            {
                dataBot.lv = GetCurrentLevelDataBotRacing(dataBot.idBot);
                if(dataBot.lv >= MaxStep && dataBot.timeEndRace == -1)
                {
                    dataBot.timeEndRace = TimeUtils.GetLongTimeCurrent - dataEvent.timeStartEvent;
                }
            }

            listDataBot.Add(dataBot);
        }
        var listSort = listDataBot
            .OrderByDescending(x => x.lv)
            .ThenBy(x => x.timeEndRace)
            .ToList();
        // Gán rank cho các phần tử trong listDataBot dựa trên thứ tự trong listSort
        for (int i = 0; i < listSort.Count; i++)
        {
            // Tìm phần tử trong listDataBot có idBot tương ứng và gán rank
            var dataBot = listDataBot.FirstOrDefault(x => x.idBot == listSort[i].idBot);
            if (dataBot != null)
            {
                dataBot.rank = i; // Rank bắt đầu từ 1
            }
        }
        CheckPass();
        return listDataBot;
    }
    private void CheckPass()
    {
        int length = dataEvent.listDataBotRacing.Count;
        int count = 0;
        for (int i = 0; i < length; i++)
        {
            var dataBot = dataEvent.listDataBotRacing[i];
            if (dataBot.lv >= 5)
            {
                count++;
                if(dataBot.idBot == -1)
                {
                    dataEvent.isEventComplete = true;
                }
            }
        }
        if(count > 2)
        {
            dataEvent.isEventComplete = true;
        }
    }
    private DataBotRacing dataPlayer => new DataBotRacing
    {
        idBot = -1,
        name = "Player",
        lv = dataEvent.totalUse,
    };
    public int GetCurrentLevelDataBotRacing(int idBot)
    {
        long timeCurrent = TimeUtils.GetLongTimeCurrent;
        long timeDelta = timeCurrent - dataEvent.timeStartEvent;
        DBBotRacing db = dbBotRacingSO.GetDBBotRacing(idBot);
        var array = db.timePassLevel;
        int index = GameUtil.FindIndexInMiddleArray(array, (int)timeDelta) + 1 ;
        return index;
    }
    public bool IsEventProgress => dataEvent.timeStartEvent != -1 && !dataEvent.isEventComplete;
    public override void OnCompleteLevelData(int level)
    {
        base.OnCompleteLevelData(level);
        if (!IsEventProgress)
        {
            return;
        }
        dataEvent.totalUse++;
        dataEvent.cacheLevel++;
        UpdateAndGetListDataBotRacingCurrent();
        SaveData();
    }

    public override void OnCompleteLevelToUnlock(int level)
    {
        base.OnCompleteLevelToUnlock(level);
        if (EventManager.Instance.IsLevelUnlockExactly(eEvent, level + 1))
        {
            UnlockDataInLevel(level);
        }
        else if (EventManager.Instance.IsUnlockLevel(eEvent, level + 1) && dataEvent.timeStartEvent == -1)
        {
            UnlockDataInLevel(level);
        }
    }
    private void UnlockDataInLevel(int level)
    {
        dataEvent.dbEvent.timeEvent = GetTimeEvent;
        SaveData();
        EventManager.Instance.SaveAllData();
        EventManager.Instance.Initialize();
    }

    private void OnPerSecond(object data)
    {
        if (!IsEventProgress)
        {
            return;
        }
        long timeCurrent = TimeUtils.GetLongTimeCurrent;
        long timeLongContain = timeCurrent - dataEvent.timeStartEvent;
        if(timeLongContain < 0)
        {
            UpdateAndGetListDataBotRacingCurrent();
            dataEvent.isEventComplete = true;
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnPerSecond);
            return;
        }
        if (dbBotRacingSO.IsTimeToCheck(timeLongContain))
        {
            UpdateAndGetListDataBotRacingCurrent();
        }
    }

    private void OnDestroy()
    {
        Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnPerSecond);
    }

}
[System.Serializable]
public class DataRacing
{
    public DB_Event dbEvent;
    public int totalUse = 0;
    public long timeStartEvent = -1;
    public long timeEndEvent = -1;
    public int cacheLevel = 0;
    public bool isEventComplete = false;
    public bool isRewardClaimed = false;
    public List<DataBotRacing> listDataBotRacing = new List<DataBotRacing>();
    public List<DataBotRacing> listDataBotRacingCache = new List<DataBotRacing>();
    public void CloneListDataBotRacing()
    {
        int length = listDataBotRacing.Count;
        List<DataBotRacing> listRacing = new List<DataBotRacing>();
        for (int i = 0; i < length; i++)
        {
            var data = listDataBotRacing[i];
            listRacing.Add(data.Clone());
        }
        listDataBotRacingCache = listRacing;
    }
}
[System.Serializable]
public class DataBotRacing
{
    public int idBot;
    public string name;
    public int lv = 0;
    public int rank = 0;
    public long timeEndRace = -1;

    public DataBotRacing Clone()
    {
        DataBotRacing data = new DataBotRacing();
        data.idBot = idBot;
        data.name = name;
        data.lv = lv;
        data.rank = rank;
        data.timeEndRace = timeEndRace;
        return data;
    }
}
