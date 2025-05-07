using DG.Tweening;
using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIRacing : UIBase
{
    public ItemCarRacingUI[] itemCarRacingUI;
    public ItemRacingProfileUI[] itemRacingProfiles;
    private List<DataBotRacing> listDataBotRacing = new List<DataBotRacing>();
    private RacingManager racingManager
    {
        get
        {
            return EventManager.Instance.racingManager;
        }
    }
    public TMP_Text txtTime;
    private void Awake()
    {
        Observer.Instance?.AddObserver(ObserverKey.TimeActionPerSecond, OnPerSecond);
    }
    private void OnDestroy()
    {
        Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnPerSecond);
    }
    private void OnPerSecond(object data)
    {
        long timeContain = 0;
        long timeCurrent = TimeUtils.GetLongTimeCurrent;
        if (!racingManager.IsEventProgress)
        {
            timeContain = racingManager.dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
        }
        else
        {
            timeContain = racingManager.dataEvent.timeEndEvent - timeCurrent;
        }
        txtTime.text = timeContain.ToUnixTimeString();
        if (timeContain <= 0)
        {
            RefreshUI();
        }
    }
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        RefreshUI();
        ShowTutorial();
        GameUtil.StepToStep(new Action<Action>[]
{
            CheckIfAnimation,
            CheckIfReward
});
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        listDataBotRacing = racingManager.UpdateAndGetListDataBotRacingCurrent();
        SetVisual(listDataBotRacing);
    }
    private void SetVisual(List<DataBotRacing> listDataBotRacing)
    {
        for (int i = 0; i < itemCarRacingUI.Length; i++)
        {
            var itemCar = itemCarRacingUI[i];
            var itemProfile = itemRacingProfiles[i];
            var dataCar = listDataBotRacing[i];
            itemCar.SetVisual(dataCar.lv);
            itemCar.SetMedal(dataCar.rank);
            itemProfile.SetVisual(dataCar.name);
        }
    }
    private void CheckIfReward(Action onDone = null)
    {
        DataBotRacing dataBotMe = new DataBotRacing();
        int length = listDataBotRacing.Count;
        for (int i = 0; i < length; i++)
        {
            if (listDataBotRacing[i].idBot == -1)
            {
                dataBotMe = listDataBotRacing[i];
                break;
            }
        }
        if (!racingManager.dataEvent.isRewardClaimed && racingManager.dataEvent.isEventComplete)
        {
            OnShowReward(dataBotMe.rank);
        }
        else
        {
            onDone?.Invoke();
        }


    }

    private void OnShowReward(int index)
    {
        bool isWin = false;
        var dbGiftEvent = racingManager.dataEvent.dbEvent.GetRewardType(ETypeResource.None);
        if (index < dbGiftEvent.gifts.Count)
        {
            var reward = dbGiftEvent.gifts[index].groupGift.dataResources;
            if (reward != null)
            {
                DataWrapperGame.ReceiveReward(ValueFirebase.RacingReceived, reward.ToArray());
                var ui = UIManager.Instance.ShowUI<UIRacingWin>(UIName.RacingWin);
                ui.Initialize(index);
                isWin = true;
            }
        }
        racingManager.dataEvent.isRewardClaimed = true;
        racingManager.SaveData();
        if (!isWin)
        {
            UIManager.Instance.ShowUI(UIName.RacingLose);
        }
    }

    private void CheckIfAnimation(Action onDone = null)
    {

        bool isHasAnim = GameUtil.AreListsEqual<DataBotRacing>(racingManager.dataEvent.listDataBotRacing, racingManager.dataEvent.listDataBotRacingCache);
        if (isHasAnim)
        {
            SetVisual(racingManager.dataEvent.listDataBotRacingCache);
            int length = racingManager.dataEvent.listDataBotRacing.Count;
            Sequence sqAll = DOTween.Sequence();
            for (int i = 0; i < length; i++)
            {
                int index = i;
                var itemCar = itemCarRacingUI[index];
                int lvOld = racingManager.dataEvent.listDataBotRacingCache[index].lv;
                int lvNew = racingManager.dataEvent.listDataBotRacing[index].lv;
                int rank = racingManager.dataEvent.listDataBotRacing[index].rank;
                Sequence sq = DOTween.Sequence();
                sq.Append(itemCar.SetVisualMove(lvOld, lvNew, 0.8f));
                sq.AppendCallback(() => itemCar.SetMedal(rank));
                sqAll.Insert(0.15f * index, sq);
            }
            sqAll.AppendCallback(() => onDone?.Invoke());
            racingManager.dataEvent.CloneListDataBotRacing();
            racingManager.SaveData();
        }
        else
        {
            onDone?.Invoke();
        }

    }

    public void OnClickInfor()
    {
        UIManager.Instance.ShowUI(UIName.RacingTutorial);
    }

    public override void Hide()
    {
        base.Hide();
        UIManager.Instance.RefreshUI();
    }

    [Space, Header("Tutorial")]
    public GameObject obTutorial;

    public void OnClickBtnTutorial()
    {
        obTutorial.SetActive(false);
        UIManager.Instance.ShowUI(UIName.RacingTutorial);
    }

    public void ShowTutorial()
    {
        bool isTuto = EventManager.Instance.dataEvent.GetTutorialComplete(EEventName.Racing);
        if (!isTuto)
        {
            obTutorial.SetActive(true);
            EventManager.Instance.dataEvent.SetTutorialComplete(EEventName.Racing, true);
            return;
        }
    }
}
