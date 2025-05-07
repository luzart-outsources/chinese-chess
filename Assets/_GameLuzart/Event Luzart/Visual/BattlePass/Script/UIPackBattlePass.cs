namespace Luzart
{
    using TMPro;

    public class UIPackBattlePass : UIPack
    {
        public TMP_Text txtTime;
        private void Awake()
        {
            Observer.Instance?.AddObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        private void OnDestroy()
        {
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        private void OnTimePerSecond(object data)
        {
            long timeCurrent = TimeUtils.GetLongTimeCurrent;
            long timeContain = EventManager.Instance.battlePassManager.dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
            txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
        }
        public override void InitIAP()
        {

            if (!EventManager.Instance.IsHasEvent(EEventName.BattlePass) || EventManager.Instance.battlePassManager.dataEvent.isBuyIAP)
            {
                Hide();
                return;
            }
            OnTimePerSecond(null);
        }
        protected override void OnCompleteBuy()
        {
            PackManager.Instance.SaveBuyPack(db_Pack.productId);
            EventManager.Instance.battlePassManager.dataEvent.isBuyIAP = true;
            EventManager.Instance.SaveDataEvent();

            Hide();
            UIManager.Instance.RefreshUI();
        }

        public void BuyCompleteBattlePass()
        {
            PackManager.Instance.SaveBuyPack(db_Pack.productId);
            EventManager.Instance.battlePassManager.dataEvent.isBuyIAP = true;
            EventManager.Instance.SaveDataEvent();

            Hide();
            UIManager.Instance.RefreshUI();
        }

        public override void HideCallAnimFly()
        {

        }
    }
}

