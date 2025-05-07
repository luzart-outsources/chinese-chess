namespace Luzart
{
    using DG.Tweening;
    using Sirenix.OdinInspector;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UIFlightEndurance : UIBase
    {
        public FlightEnduranceManager flightEnduranceManager
        {
            get
            {
                return EventManager.Instance.flightEnduranceManager;
            }
        }
        public TMP_Text txtLevel, txtPlayer;
        public BaseSelect selectWinLose;
        public Button btnInfor;
        public Transform parentFly;
        public ItemFlightEnduranceSeat[] itemFlights;
        public RectTransform rtGift, rtPosInGift;
        private List<ItemFlightEnduranceSeat> listItemFlightJump = new List<ItemFlightEnduranceSeat>();
    
        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnInfor, ClickInfor, true);
        }
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            bool isShowStart = !flightEnduranceManager.dataFlightEndurance.isShowStart && !flightEnduranceManager.IsLoss&& !flightEnduranceManager.IsWin ;
            if (isShowStart)
            {
                ShowAnimationStart();
                flightEnduranceManager.dataFlightEndurance.isShowStart = true;
                EventManager.Instance.SaveDataEvent();
            }
            SetVisual();
            InitSeat();
        }
        private void InitSeat()
        {
            int length = itemFlights.Length;
            var list = flightEnduranceManager.dataFlightEndurance.listVisual;
            for (int i = 0; i < length; i++)
            {
                var data = list[i];
                bool isMe = list[i].id == flightEnduranceManager.dataFlightEndurance.idMe;
                itemFlights[i].Initialize(list[i], isMe);
            }
        }
        public void ShowAnimationStart()
        {
            UIManager.Instance.ShowUI(UIName.FlightEnduranceStart);
        }
        public void SetVisual()
        {
            bool isWait = flightEnduranceManager.IsLoss;
            selectWinLose.Select(!isWait);
            if (!flightEnduranceManager.IsLoss)
            {
                txtLevel.text = $"{flightEnduranceManager.dataFlightEndurance.winStreak}/{FlightEnduranceManager.CountToWin}";
                txtPlayer.text = $"{flightEnduranceManager.dataFlightEndurance.totalPeople}/{FlightEnduranceManager.TotalPeople}";
            }
    
        }
        private void ClickInfor()
        {
            UIManager.Instance.ShowUI(UIName.FlightEnduranceTutorial);
        }
        public void ShowAnimNhayDu()
        {
            var listData = EventManager.Instance.flightEnduranceManager.listTrueCache;
            if (listData.Count  <= 0)
            {
                return;
            }
            listItemFlightJump = new List<ItemFlightEnduranceSeat>();
            var list = EventManager.Instance.flightEnduranceManager.listTrueCache;
    
            int length = list.Count;
            for (int i = 0; i < length; i++)
            {
                int listId = list[i].id;
                listItemFlightJump.Add(itemFlights[listId]);
            }
            listItemFlightJump.Shuffle();
            Sequence sq = DOTween.Sequence();
            length = listItemFlightJump.Count;
    
            int totalPlayer = flightEnduranceManager.dataFlightEndurance.totalPeople;
            int preTotalPlayer = totalPlayer + length;
            SetTextUser(preTotalPlayer);
            closeBtn.gameObject.SetActive(false);
            for (int i = 0; i < length; i++)
            {
                int index = i;
                listItemFlightJump[index].gameObject.SetActive(true);
                float ran = UnityEngine.Random.Range(0.1f, 0.2f);
                sq.AppendInterval(ran);
                sq.AppendCallback(() =>
                {
                    FlyAnimation(listItemFlightJump[index], () =>
                    {
                        SetTextUser(preTotalPlayer - 1 - index, true);
                    });
                });
            }
            listData.Clear();
            sq.AppendInterval(1f);
            
            if (flightEnduranceManager.dataFlightEndurance.winStreak == 7)
            {
                sq.Append(FlyAnimationWin());
                sq.Append(ContainPeople());
                sq.AppendCallback(() =>
                {
                    UIManager.Instance.ShowUI(UIName.FlightEnduranceReward);
                });
            }
            sq.AppendCallback(() => closeBtn.gameObject.SetActive(true));
            sq.SetId(this);
    
    
        }
        private Tween ContainPeople()
        {
            List<ItemFlightEnduranceSeat> listItem = new List<ItemFlightEnduranceSeat>();
            List<DataSeatFlight> listData = flightEnduranceManager.dataFlightEndurance.listTotalPeople;
            int length = listData.Count;
            for (int i = 0; i < length; i++)
            {
                var item = itemFlights[listData[i].id];
                listItem.Add(item);
            }
            int lengthList = listItem.Count;
            Sequence sq = DOTween.Sequence();
            for (int i = 0; i < lengthList; i++)
            {
                Vector2 pos = rtPosInGift.GetRandomPositionInRect();
                var item = listItem[i];
                float ranTime = UnityEngine.Random.Range(0.1f, 0.3f);
                sq.AppendCallback(()=> item.transform.SetParent(rtPosInGift));
                sq.Join(item.GetComponent<RectTransform>().DOJumpAnchorPos(pos, 5,1, ranTime));
            }
            sq.SetId(this);
            return sq;  
    
    
    
        }
        private void FlyAnimation(ItemFlightEnduranceSeat item, Action onDone)
        {
            item.InitAnimation(parentFly, onDone);
        }
        private Tween twPunch = null;
        public void SetTextUser(int total, bool isAnim = false)
        {
            txtPlayer.text = $"{total}/{FlightEnduranceManager.TotalPeople}";
            if (isAnim)
            {
                twPunch?.Kill(true);
                twPunch = txtPlayer.transform.DOPunchScale(0.2f * Vector3.one, 0.08f, 1, 0.1f);
            }
    
        }
        public RectTransform transformFlinght;
        public Tween FlyAnimationWin()
        {
            Vector2 posCurrent = transformFlinght.anchoredPosition;
            Sequence sqFlinghtWin = DOTween.Sequence();
            sqFlinghtWin.Append(transformFlinght.DOAnchorPosY(posCurrent.y + 50, 0.5f));
            sqFlinghtWin.Append(transformFlinght.DOAnchorPosY(posCurrent.y - 300, 1f));
            sqFlinghtWin.Join(transformFlinght.DOScale(0.8f, 1f));
            sqFlinghtWin.Append(transformFlinght.DOAnchorPosY(-200, 1f));
            sqFlinghtWin.Join(rtGift.DOAnchorPosY(0, 1f));
            return sqFlinghtWin;
        }
        private void OnDisable()
        {
            this.DOKill();
        }
    }
}
