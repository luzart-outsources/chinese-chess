namespace Luzart
{
    using DG.Tweening;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    
    public class ButtonEventBattlePass : ButtonEvent
    {
        public TextMeshProUGUI txtTime;
        public TextMeshProUGUI txtLevelCurrent;
        public TextMeshProUGUI txtProgress;
        public ProgressBarUI progressBarUI;
        public GameObject obNoti;
        private BattlePassManager battlePassManager 
        {
            get
            {
                return EventManager.Instance.battlePassManager;
            }
        }
        protected override void Start()
        {
            base.Start();
            if (IsActiveEvent)
            {
                Observer.Instance.AddObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        private void OnTimePerSecond(object data)
        {
            long timeCurrent = TimeUtils.GetLongTimeCurrent;
            long timeContain = EventManager.Instance.battlePassManager.dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
            txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
        }
        private void CheckNoti()
        {
            bool isNoti = battlePassManager.IsHasDataDontClaim();
            GameUtil.SetActiveCheckNull(obNoti, isNoti);
        }
        protected override void InitButton()
        {
            SetVisual(battlePassManager.dataEvent.totalUse);
            CheckNoti();
            OnTimePerSecond(null);
        }
        public void SetVisual(int totalUse)
        {
            int levelCurrent = battlePassManager.CounDataDontClaim(totalUse);
            int contain = battlePassManager.dataEvent.ContainByTotalUse(totalUse);
            int totalRequire = battlePassManager.dataEvent.RequireByTotalUse(totalUse);
            float percent = (float)contain / (float)totalRequire;
            txtLevelCurrent.text = $"{levelCurrent}";
            progressBarUI.SetSlider(percent, percent, 0f, null);
            txtProgress.text = $"{contain}/{totalRequire}";
        }
        public Tween SetSlider(int preTotalUse, int totalUse, float timeMove, Action onDone =null)
        {
            battlePassManager.dataEvent.CalculateProgress(preTotalUse, out int preLevelCurrent, out int preContain, out int preTotalRequire, out float prePercent);
            battlePassManager.dataEvent.CalculateProgress(totalUse, out int levelCurrent, out int contain, out int totalRequire, out float percent);
            SetVisual(preTotalUse);
            if (preLevelCurrent == levelCurrent)
            {
                return progressBarUI.SetSliderTween(prePercent, percent, timeMove);
            }
            else
            {
                Sequence sq = DOTween.Sequence();
                sq.Append(progressBarUI.SetSliderTween(prePercent, 1, timeMove / 2)); // Di chuyển đến 100% trong nửa thời gian

                // Tính toán thời gian cần cho mỗi bước giữa các cấp độ
                float stepTime = timeMove / (levelCurrent - preLevelCurrent + 1); // Chia đều thời gian cho tất cả các bước
                for (int i = 0; i < (levelCurrent - preLevelCurrent - 1); i++)
                {
                    sq.Append(progressBarUI.SetSliderTween(0, 1, stepTime)); // Di chuyển từ 0 đến 100% cho các bước cấp độ
                }
                // Cuối cùng, di chuyển thanh progress từ 0 đến percent trong thời gian còn lại
                sq.Append(progressBarUI.SetSliderTween(0, percent, stepTime));
                sq.AppendCallback(() => SetVisual(totalUse));
                return sq;
            }
        }
        public void SetSliderTotalUse(int preTotalUse, int totalUse, float timeMove, Action onDone = null)
        {
            var dataBP = battlePassManager.dataEvent;

            int levelStart = dataBP.LevelByTotalUse(preTotalUse);
            int levelEnd = dataBP.LevelByTotalUse(totalUse);

            Sequence seq = DOTween.Sequence();
            int totalDistance = totalUse - preTotalUse;

            int currentTotal = preTotalUse;

            for (int level = levelStart; level <= levelEnd; level++)
            {
                int require = dataBP.RequireByLevel(level);
                int startContain = (level == levelStart) ? dataBP.ContainByTotalUse(currentTotal) : 0;
                int endContain = (level == levelEnd) ? dataBP.ContainByTotalUse(totalUse) : require;

                float percentStart = (float)startContain / require;
                float percentEnd = (float)endContain / require;
                int delta = endContain - startContain;

                float percentOfTotal = (float)delta / totalDistance;
                float timeForLevel = timeMove * percentOfTotal;

                int displayContain = startContain;

                seq.Append(progressBarUI.SetSliderTween(percentStart, percentEnd, timeForLevel, null, (val) =>
                {
                    // Cập nhật contain theo progress
                    int c = Mathf.RoundToInt(val * require);
                    txtProgress.text = $"{c}/{require}";
                }));

                if (level < levelEnd)
                {
                    seq.AppendCallback(() =>
                    {
                        txtLevelCurrent.text = (level + 1).ToString();
                        txtProgress.text = $"0/{dataBP.RequireByLevel(level + 1)}";
                    });
                }

                currentTotal += delta;
            }

            seq.OnComplete(() => onDone?.Invoke());
        }
        private void ClickBattlePass()
        {
            UIManager.Instance.ShowUI(UIName.BattlePass);
        }
        public override void InitEvent(Action action)
        {
            base.InitEvent(ClickBattlePass);

        }

    }
}
