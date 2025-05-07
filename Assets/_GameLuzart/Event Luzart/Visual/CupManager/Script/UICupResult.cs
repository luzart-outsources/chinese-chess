namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UICupResult : UIBase
    {
        public ScrollRect scrollRect;
        public Transform parentItemCup;
        public ItemCupUI itemCupPrefabs;
        private List<ItemCupUI> listItemCup = new List<ItemCupUI>();
    
        public Button btnNext;
        public TMP_Text txtNext;
    
        private ItemCupUI itemCacheCupMe;
        private CupManager cupManager
        {
            get
            {
                return EventManager.Instance.cupManager;
            }
        }
        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnNext, ClickNext, true);
        }
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            RefreshUI();
            OnMoveToAnimation();
        }
        private bool isGetGift = false;
        private int indexMe = 0;
        public override void RefreshUI()
        {
            base.RefreshUI();
            var list = cupManager.dataCup.listDataBot;
            var listGift = cupManager.dataCup.dbEvent.GetRewardType(ETypeResource.None).gifts;
            int maxGift = listGift.Count;
            int length = list.Count;
    
            MasterHelper.InitListObj<ItemCupUI>(length, itemCupPrefabs, listItemCup, parentItemCup, (item, index) =>
            {
                item.gameObject.SetActive(true);
                GiftEvent gift = new GiftEvent();
                if (index < maxGift)
                {
                    gift = listGift[index];
                }
                item.Initialize(index, list[index], gift, ClickGift);
                if (list[index].idBot == -1)
                {
                    itemCacheCupMe = item;
                }
            });
            indexMe = cupManager.GetIndexMe();
            isGetGift = indexMe < maxGift;
            string strGetGift = isGetGift ? "Claim" : "Next";
            txtNext.text = strGetGift;
        }
        private void OnMoveToAnimation()
        {
            GameUtil.Instance.WaitAndDo(0.1f, () =>
            {
                var preBattle = itemCacheCupMe;
                scrollRect.FocusOnRectTransform(preBattle.rectTransform);
            });
        }
        private void ClickGift(ItemCupUI item)
        {
            if (item.giftEvent.groupGift.dataResources.Count == 0 || item.giftEvent.groupGift.dataResources == null)
            {
                return;
            }
            item.boxInforMess.gameObject.SetActive(true);
        }
        private void ClickNext()
        {
            Hide();
            if (isGetGift)
            {
                var listGift = cupManager.dataCup.dbEvent.GetRewardType(ETypeResource.None).gifts;
                var gift = listGift[indexMe].groupGift.dataResources.ToArray();
                
                DataWrapperGame.ReceiveRewardShowPopUp(ValueFirebase.CaptainTrophyReceive, OnCompleteReceive, gift);
            }
            cupManager.DeleteDBEvent();
            UIManager.Instance.RefreshUI();
        }
        private void OnCompleteReceive()
        {

        }
    }
}
