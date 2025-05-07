namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UICup : UIBase
    {
        public TMP_Text txtClock;
    
        public ScrollRect scrollRect;
        public ItemCupUI itemCupPrefabs;
        public Transform parentItemCup;
        private List<ItemCupUI> listItemCup = new List<ItemCupUI>();
    
        public ItemCupUI itemCupMeUp;
        public ItemCupUI itemCupMeDown;
        private ItemCupUI itemCacheCupMe;
        private CupManager cupManager
        {
            get
            {
                return EventManager.Instance.cupManager;
            }
        }
        public BaseSelect bsItemUpdown;
        public Transform parentNoti;
    
        protected override void Setup()
        {
            base.Setup();
            scrollRect.onValueChanged.AddListener(CheckInvisible);
        }
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            if (cupManager.IsEventFinish())
            {
                Hide();
                UIManager.Instance.ShowUI(UIName.CupResult);
                return;
            }
            RefreshUI();
            CheckInvisible(Vector2.zero);
            OnMoveToAnimation();
        }
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
                if(index < maxGift)
                {
                    gift = listGift[index];
                }
                item.Initialize(index, list[index], gift, ClickGift, HideGift);
                if (list[index].idBot == -1)
                {
                    itemCacheCupMe = item;
                    itemCupMeUp.Initialize(index, list[index], gift, ClickGift);
                    itemCupMeDown.Initialize(index, list[index], gift, ClickGift);
                }
            });
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
            if (item.giftEvent.groupGift.dataResources == null || item.giftEvent.groupGift.dataResources.Count ==0)
            {
                UIManager.Instance.ShowToast("No gift in this rank !");
                return;
            }
            item.boxInforMess.gameObject.SetActive(true);
            item.boxInforMess.transform.SetParent(parentNoti);
        }
        private void HideGift(ItemCupUI item)
        {
            item.boxInforMess.transform.SetParent(item.transform);
            item.boxInforMess.rectTransform.anchoredPosition = Vector2.zero;
        }
        private void CheckInvisible(Vector2 position)
        {
            RectTransform viewport = scrollRect.viewport;
            RectTransform itemRect = itemCacheCupMe.rectTransform;
    
            Vector3[] viewportCorners = new Vector3[4];
            Vector3[] itemCorners = new Vector3[4];
    
            viewport.GetWorldCorners(viewportCorners);
            itemRect.GetWorldCorners(itemCorners);
    
            if (itemCorners[0].y > viewportCorners[0].y && itemCorners[1].y > viewportCorners[1].y)
            {
                bsItemUpdown.Select(0);
            }
            else if (itemCorners[0].y < viewportCorners[0].y && itemCorners[1].y < viewportCorners[1].y)
            {
                bsItemUpdown.Select(1);
            }
            else
            {
                bsItemUpdown.Select(2);
            }
        }
    }
}
