using Luzart;
using System.Collections.Generic;
using UnityEngine;

public class LuckySpinManager : BaseEventManager
{
    private List<DB_SpinWheel> _listSpin = new List<DB_SpinWheel>();
    public const int MaxLevelProgress = 4;
    public const int TicketNeedSpin = 1;
    public List<DB_SpinWheel> listSpinWheels
    {
        get
        {
            if(_listSpin == null || _listSpin.Count == 0)
            {
                GetListSpinWheel();
            }
            return _listSpin;
        }
    }

    private void GetListSpinWheel()
    {
        _listSpin = new List<DB_SpinWheel>();
        int length = dataEvent.dbEvent.allTypeResources[0].gifts.Count;
        for (int i = 0; i < length; i++)
        {
            int index = i;
            var item = dataEvent.dbEvent.allTypeResources[0].gifts[index];
            DB_SpinWheel dbSpinWheel = new DB_SpinWheel();
            dbSpinWheel.chance = item.require;
            dbSpinWheel.dataRes = item.groupGift.dataResources[0];
            _listSpin.Add(dbSpinWheel);
        }
    }
    public override EEventName eEvent => EEventName.LuckySpin;
    private const string PATH_LUCKYSPIN = "lucky_spin";
    public DataLuckySpin dataEvent;
    public override TimeEvent GetTimeEvent
    {
        get
        {
            TimeEvent timeEvent = new TimeEvent();
            timeEvent.timeStart = TimeUtils.GetLongTimeStartDay(EventManager.Instance.dataEvent.timeFirstTime);
            timeEvent.timeStartReview = int.MaxValue;
            return timeEvent; 
        }
    }

    public override void LoadData()
    {
        if (!EventManager.Instance.IsHasEvent(eEvent))
        {
            return;
        }
        DB_Event dbEvent = EventManager.Instance.GetEvent(EEventName.LuckySpin);
        dataEvent = SaveLoadUtil.LoadDataPrefs<DataLuckySpin>(PATH_LUCKYSPIN);
        if (dataEvent == null)
        {
            dataEvent = new DataLuckySpin();
            dataEvent.dbEvent = new DB_Event();
        }
        DB_Event db_EventLocal = dataEvent.dbEvent;
        if (string.IsNullOrEmpty(db_EventLocal.idEvent) || !db_EventLocal.idEvent.Equals(dbEvent.idEvent))
        {
            dataEvent = new DataLuckySpin();
            dataEvent.dbEvent = new DB_Event();
            dataEvent.dbEvent = dbEvent;
            PushFirebaseShow();
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
        if(dataEvent.levelBreakVisual == -1)
        {
            dataEvent.levelBreakVisual = DataWrapperGame.CurrentLevel;
        }
    }


    public override void SaveData()
    {
        SaveLoadUtil.SaveDataPrefs(dataEvent, PATH_LUCKYSPIN);
    }

    public override void OnCompleteLevelData(int level)
    {
        base.OnCompleteLevelData(level);
        if (level == 0) return;
        dataEvent.progress++;
        int ticketAdd = dataEvent.progress / 4;
        dataEvent.progress = (int)Mathf.Repeat(dataEvent.progress, MaxLevelProgress);
        ChangeTicket(ticketAdd);
        SaveData();
    }
    public override void OnCompleteLevelToUnlock(int level)
    {
        base.OnCompleteLevelToUnlock(level);
        if(level <= EventManager.Instance.GetLevelUnlock(EEventName.LuckySpin))
        {
            dataEvent.levelBreakVisual = level;
        }
    }
    public int GetRandomIndex()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        float cumulative = 0f;
        for (int i = 0; i < listSpinWheels.Count; i++)
        {
            cumulative += listSpinWheels[i].chance;
            if (randomValue < cumulative)
            {
                return i;
            }
        }
        return listSpinWheels.Count - 1;
    }
    public bool IsHasTicketOrHasProgress
    {
        get
        {
            if(dataEvent.countTicket > 0)
            {
                return true;
            }
            else
            {
                return IsHasProgressCurrent;
            }
        }
    }
    public bool IsHasProgressCurrent
    {
        get
        {
            return TicketDontClaimedCurrent > 0;
        }
    }
    public int TicketDontClaimedCurrent
    {
        get
        {
            return TicketDontClaim(DataWrapperGame.CurrentLevel);
        }
    }

    public bool IsHasTicket()
    {
        return dataEvent.countTicket > 0;
    }
    public bool IsHasProgress(int level)
    {
        return TicketDontClaim(level) > 0;
    }

    public int TicketDontClaim(int level)
    {
        return TicketDontClaim(dataEvent.levelBreakVisual,level);
    }
    public int TicketDontClaim(int fromLevel, int toLevel)
    {
        if (toLevel <= fromLevel) return 0;

        // Tính progress giả định tại fromLevel
        int levelFromProgress = (toLevel - 1 - dataEvent.progress);
        int absoluteProgress = levelFromProgress - ((levelFromProgress % MaxLevelProgress + MaxLevelProgress) % MaxLevelProgress);

        int count = 0;
        for (int i = fromLevel + 1; i <= toLevel; i++)
        {
            if ((i - absoluteProgress) % MaxLevelProgress == 0)
            {
                count++;
            }
        }

        return count;
    }

    public int GetTotalTicketAtLevel(int levelX)
    {
        int ticketDontClaim = TicketDontClaim(levelX,DataWrapperGame.CurrentLevel);
        int ticketBack = dataEvent.countTicket - ticketDontClaim;
        return ticketBack;
    }

    public int GetCurrentProgress
    {
        get
        {
            return GetProgress(DataWrapperGame.CurrentLevel);
        }

    }
    public int GetProgress(int level)
    {
        //int spaceLevel = Mathf.Clamp(level - EventManager.Instance.GetLevelUnlock(EEventName.LuckySpin),0,int.MaxValue);
        int levelSpace = DataWrapperGame.CurrentLevel - level;
        int containProgress = levelSpace % MaxLevelProgress;

        int levelRepeat = dataEvent.progress;
        if(levelRepeat == 0)
        {
            levelRepeat = MaxLevelProgress;
        }
        int thisProgress = levelRepeat - containProgress;
        int progress = thisProgress % MaxLevelProgress;
        return progress;
    }

    public void ChangeTicket(int amount)
    {
        dataEvent.countTicket += amount;
        dataEvent.countTicket = Mathf.Clamp(dataEvent.countTicket, 0, int.MaxValue);
        SaveData();
    }
    private void PushFirebaseShow()
    {
        if (dataEvent == null)
        {
            return;
        }
#if FIREBASE_ENABLE
        FirebaseEvent.LogEvent(KeyFirebase.Event_Show, new Firebase.Analytics.Parameter[]
{
                new Firebase.Analytics.Parameter(TypeFirebase.EventName, eEvent.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,"0"),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelID,UserLevel.GetCurrentLevel().ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.EventDuration, dataEvent.dbEvent.timeEvent.TimeContainEvent.ToString())

});
#endif
    }

}

[System.Serializable]
public class DataLuckySpin
{
    public DB_Event dbEvent;
    public int progress = 0;
    public int countTicket = 1;
    public int levelBreakVisual = - 1;
}

[System.Serializable]
public class DB_SpinWheel
{
    [Range(0, 100)]
    public float chance;

    public DataResource dataRes;
}