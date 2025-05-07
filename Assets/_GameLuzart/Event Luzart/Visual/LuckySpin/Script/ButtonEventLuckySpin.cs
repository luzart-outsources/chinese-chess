using UnityEngine;
using Luzart;
using System;
using TMPro;
using DG.Tweening;

public class ButtonEventLuckySpin : ButtonEvent
{
    public TMP_Text txtProgress;
    public ProgressBarUI progressBarUI;
    public TMP_Text txtCountSpin;
    public GameObject obNoti;

    private LuckySpinManager luckySpinManager => EventManager.Instance.luckySpinManager;
    public override void InitEvent(Action action)
    {
        base.InitEvent(ClickLuckySpin);

    }
    protected override void InitButton()
    {
        base.InitButton();
        SetVisual(DataWrapperGame.CurrentLevel);
    }
    public void ClickLuckySpin()
    {
        Debug.LogError("Click LuzkySpin");
    }
    private void SetVisual(int level)
    {
        int levelProgress = luckySpinManager.GetProgress(level);
        if(levelProgress == -1)
        {
            levelProgress = 0;
        }
        float percent = (float)levelProgress / LuckySpinManager.MaxLevelProgress;
        progressBarUI.SetSlider(percent, percent,0,null);
        txtProgress.text = $"{levelProgress}/{LuckySpinManager.MaxLevelProgress}";
        SetObNoti(luckySpinManager.dataEvent.countTicket);
    }

    public void SetVisualInAnim(int level)
    {
        int levelProgress = luckySpinManager.GetProgress(level);
        float percent = (float)levelProgress / LuckySpinManager.MaxLevelProgress;
        progressBarUI.SetSlider(percent, percent, 0, null);
        txtProgress.text = $"{levelProgress}/{LuckySpinManager.MaxLevelProgress}";
        SetObNotiLevel(level);
    }
    private void SetObNotiLevel(int level)
    {
        int ticket = luckySpinManager.GetTotalTicketAtLevel(level);
        SetObNoti(ticket);
    }
    private void SetObNoti(int countTicket)
    {
        bool isShowNoti = countTicket > 0;
        txtCountSpin.text = $"{countTicket}";
        obNoti?.SetActive(isShowNoti);
    }

    public Tween PreClaimIfHas(int preLevelBreak, int levelCurrent, float timeSlider = 0.2f)
    {
        int thisLevel = levelCurrent;
        int ticketAdd = luckySpinManager.TicketDontClaim(preLevelBreak, levelCurrent);

        preLevelBreak = luckySpinManager.GetProgress(preLevelBreak);
        levelCurrent = luckySpinManager.GetProgress(levelCurrent);

        float preProgress = (float)preLevelBreak / (float)LuckySpinManager.MaxLevelProgress;
        float curProgress = (float)levelCurrent / (float)LuckySpinManager.MaxLevelProgress;

        int ticketCurrent = luckySpinManager.dataEvent.countTicket;

        Sequence sq = DOTween.Sequence();

        if (ticketAdd == 0)
        {
            sq.Append(progressBarUI.SetSliderTween(preProgress, curProgress, timeSlider, null));
            sq.Join(DOVirtual.Int(preLevelBreak, levelCurrent, timeSlider, (x) => txtProgress.text = $"{x}/{LuckySpinManager.MaxLevelProgress}"));
        }
        else
        {
            timeSlider = timeSlider / (ticketAdd + 1);
            sq.AppendCallback(() =>
            {
                progressBarUI.SetSlider(preProgress, 1, timeSlider, null);
            });
            sq.Append(DOVirtual.Int(preLevelBreak, LuckySpinManager.MaxLevelProgress, timeSlider, (x) => txtProgress.text = $"{x}/{LuckySpinManager.MaxLevelProgress}"));
            sq.AppendCallback(() =>
            {
                ticketCurrent = ticketCurrent + 1;
            });
            for (int i = 0; i < ticketAdd - 1; i++)
            {
                sq.AppendCallback(() =>
                {
                    progressBarUI.SetSlider(0, 1, timeSlider, null);
                });
                sq.Join(DOVirtual.Int(0, LuckySpinManager.MaxLevelProgress, timeSlider, (x) => txtProgress.text = $"{x}/{LuckySpinManager.MaxLevelProgress}"));
                sq.AppendCallback(() => ticketCurrent = ticketCurrent + 1);
            }
            sq.AppendCallback(() =>
        {
            progressBarUI.SetSlider(0, curProgress, timeSlider, null);
        });
            sq.Append(DOVirtual.Int(0, levelCurrent, timeSlider, (x) =>
            {
                txtProgress.text = $"{x}/{LuckySpinManager.MaxLevelProgress}";
            }));
        }
        sq.AppendCallback(() => SetObNotiLevel(thisLevel));
        return sq;

    }
}
