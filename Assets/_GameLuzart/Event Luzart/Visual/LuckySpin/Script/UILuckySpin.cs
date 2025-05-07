using DG.Tweening;
using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILuckySpin : UIBase
{
    private LuckySpinManager luckySpinManager => EventManager.Instance.luckySpinManager;

    public LuckySpinVisual luckySpinVisual;
    public ProgressBarUI progressBar;
    public TMP_Text txtProgress;
    public TMP_Text txtTicket;
    public SpinPiece[] spinPieces;
    public int forcedSectorIndex = -1;
    public BaseSelect bsSpinning;
    public BaseSelect bsCanSpinNormal;

    public GameObject obBlock;

    public BaseSelect[] bsLight;
    public Animator animator;
    private EEventName eEvent = EEventName.LuckySpin;

    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        RefreshUI();
    }

    public override void RefreshUI()
    {
        luckySpinVisual.Initialize(luckySpinManager.listSpinWheels);
        int length = spinPieces.Length;
        for (int i = 0; i < length; i++)
        {
            int index = i;
            var data = luckySpinManager.listSpinWheels[index];
            var spinPiece = spinPieces[i];
            spinPiece.InitializePiece(index, data.dataRes);
        }
        //
        int countSpin = luckySpinManager.dataEvent.countTicket;
        int ticketNeedSpin = LuckySpinManager.TicketNeedSpin;
        txtTicket.text = $"SPIN {countSpin}/{ticketNeedSpin}";

        bool isSpin = countSpin >= ticketNeedSpin;
        bsCanSpinNormal.Select(isSpin);

        //
        int currentProgress = luckySpinManager.GetCurrentProgress;
        int maxProgress = LuckySpinManager.MaxLevelProgress;
        float progress = (float)currentProgress / (float)maxProgress;

        txtProgress.text = $"{currentProgress}/{maxProgress}";
        progressBar.SetSlider(progress, progress, 0, null);


    }

    public void OnClickSpinAds()
    {
        AdsWrapperManager.ShowReward(KeyAds.AdsSpin, Spin, UIManager.Instance.ShowToastInternet);
    }

    public void OnClickSpin()
    {
        int countSpin = luckySpinManager.dataEvent.countTicket;
        int ticketNeedSpin = LuckySpinManager.TicketNeedSpin;
        if(countSpin < ticketNeedSpin)
        {
            UIManager.Instance.ShowToast("Reach more level to spin !");
            return;
        }
        Spin();

    }
    private void Spin()
    {
        if (luckySpinVisual.isSpinning)
        {
            UIManager.Instance.ShowToast("Spinning in progress, please wait...");
            return;
        }
        forcedSectorIndex = luckySpinManager.GetRandomIndex();
        if (forcedSectorIndex < 0 || forcedSectorIndex >= luckySpinVisual.listDBSpin.Count)
        {
            Debug.LogError("Chỉ số sector không hợp lệ!");
            return;
        }
        GameUtil.StepToStep(new Action<Action>[]
        {
            OnAnimBeforeSpin,
            OnCallStartVisual
        });
    }

    private void OnAnimBeforeSpin(Action onDone)
    {
        animator.enabled = false;
        int length = bsLight.Length;
        for (int i = 0; i < length; i++)
        {
            int index = i;
            bsLight[index].Select(false);
        }
        Sequence sq = DOTween.Sequence();
        for (int i = 0; i < length; i++)
        {
            int index = i;
            sq.AppendCallback(() => bsLight[index].Select(true));
            sq.AppendInterval(0.05f);
        }
        sq.AppendCallback(()=> onDone?.Invoke());
    }

    private void OnCallStartVisual(Action onDone)
    {
        luckySpinVisual.StartSpin(forcedSectorIndex, actionStartSpin: OnStartSpin, actionOnDoneSpin: OnDoneSpin);
        twLoop = AutoLoop();
        twLoop.SetId(this);
        twLoop.SetLoops(-1);
    }
    private Tween twLoop = null;
    private Tween AutoLoop()
    {
        Sequence sq = DOTween.Sequence();
        int length = bsLight.Length;
        List<BaseSelect> listBaseSelect = bsLight.ToList();
        listBaseSelect.Remove(bsLight[length - 1]);

        length = listBaseSelect.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            int index = i;
            var item = listBaseSelect[index];
            sq.AppendCallback(() => item.Select(false));
            sq.AppendInterval(0.15f);
        }
        for (int i = length - 1; i >= 0; i--)
        {
            int index = i;
            var item = listBaseSelect[index];
            sq.AppendCallback(() => item.Select(true));
            sq.AppendInterval(0.15f);
        }
        return sq;
    }

    public void OnStartSpin()
    {
        bsSpinning.Select(true);
    }

    public void OnDoneSpin(int indexSpin)
    {
        bsSpinning.Select(false);
        RefreshUI();
        OnReceiveReward();
        OnMissTicket();
        twLoop?.Kill(true);
        animator.enabled = true;
    }
    

    public void OnReceiveReward()
    {
        var itemReceive = spinPieces[forcedSectorIndex];
        var posSpawn = RectTransformUtility.WorldToScreenPoint(null,itemReceive.transform.position);
        DataWrapperGame.ReceiveRewardShowPopUpAnim(ValueFirebase.LuckySpinReceived, OnDoneReceiveReward, true,posSpawn, itemReceive.dataRes);
    }
    private void OnDoneReceiveReward()
    {
        UIManager.Instance.RefreshUI();
    }
    public void OnMissTicket()
    {
        luckySpinManager.ChangeTicket(-LuckySpinManager.TicketNeedSpin);
#if FIREBASE_ENABLE
        FirebaseEvent.LogEvent(KeyFirebase.Event_Claimed, new Firebase.Analytics.Parameter[]
{
                new Firebase.Analytics.Parameter(TypeFirebase.EventName,eEvent.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,luckySpinManager.dataEvent.countTicket.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelID,UserLevel.GetCurrentLevel().ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.Type,"ads"),

});
#endif
    }
}
