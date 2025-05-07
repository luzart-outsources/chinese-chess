namespace Luzart
{
#if ENABLE_IAP
    using BG_Library.IAP;
    using BG_Library.NET;
    using BG_Library.UserInventory;
#endif
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UIJourneyToSuccess : UIBase
    {
        private EEventName eEvent = EEventName.JourneyToSuccess;
        public Transform parentSpawn;
        public ItemJourneyToSuccess itemPf;
        public List<ItemJourneyToSuccess> listJournetToSuccess = new List<ItemJourneyToSuccess>();
        private DB_Event db_Event;
        public GameObject obBlock;
        private JourneyToSuccessManager journeyToSuccessManager
        {
            get
            {
                return EventManager.Instance.journeyToSuccessManager;
            }
        }
        public override void RefreshUI()
        {
            obBlock.SetActive(false);
            db_Event = journeyToSuccessManager.dataEvent.dbEvent;
            if (db_Event == null || db_Event.eventStatus == EEventStatus.Finish)
            {
                Hide();
                return;
            }
            var gift = db_Event.GetRewardType(ETypeResource.None).gifts;
            if (gift == null || gift.Count == 0)
            {
                Hide();
                UIManager.Instance.RefreshUI();
                return;
            }
            int length = gift.Count;
            if (journeyToSuccessManager.dataEvent.level >= length)
            {
                Hide();
                UIManager.Instance.RefreshUI();
                return;
            }
            int levelCurrent = journeyToSuccessManager.dataEvent.level;
            MasterHelper.InitListObj(length, itemPf, listJournetToSuccess, parentSpawn, (item, index) =>
            {
                item.gameObject.SetActive(true);
                EStateClaim eState = EStateClaim.WillClaim;
                if (index < levelCurrent)
                {
                    eState = EStateClaim.Claimed;
                }
                else if (index == levelCurrent)
                {
                    eState = EStateClaim.CanClaim;
                }
                //var dataGift = gift[index].groupGift.dataResources != null ? gift[index].groupGift.dataResources.ToArray() : new DataResource[0];
                item.InitializeData(index, eState, ClickItem, gift[index].groupGift);
            });
        }
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            RefreshUI();
        }
        private ItemJourneyToSuccess itemCache = null;
    
        private void ClickItem(ItemJourneyToSuccess item)
        {
            this.itemCache = item;
            if(itemCache.eState == EStateClaim.WillClaim)
            {
                UIManager.Instance.ShowToast(KeyToast.UnlockPreviousItem);
                return;
            }
            int index = item.index;
            var db = journeyToSuccessManager.GetDBJourneyResources(index);
            switch (db.eTypeResources)
            {
                case ETypeResources.IAP:
                    {
                        BuyIAP();
                        break;
                    }
                case ETypeResources.ADS:
                    {
                        BuyAds();
                        break;
                    }
                default:
                    {
                        BuyFree();
                        break;
                    }
            }
        }
        private void BuyFree()
        {
            int index = itemCache.index;
            var gift = db_Event.GetRewardType(ETypeResource.None).gifts;
            Vector3 pos = RectTransformUtility.WorldToScreenPoint(null, itemCache.listResUI.transform.position);
            DataWrapperGame.ReceiveRewardShowPopUpAnim(ValueFirebase.JourneyToSuccess, OnCompleteBuy, true, pos, gift[index].groupGift.dataResources.ToArray());
            //DataWrapperGame.ReceiveRewardShowPopUp(ValueFirebase.JourneyToSuccess, OnCompleteBuy, gift[index].groupGift.dataResources.ToArray());
            journeyToSuccessManager.ClaimItem(index);
        }

        private void BuyAds()
        {
            int index = itemCache.index;
            var db = journeyToSuccessManager.GetDBJourneyResources(index);
            int countAdsCurrent = journeyToSuccessManager.dataEvent.GetCountADS(index);
            if (countAdsCurrent >= db.countAds)
            {
                BuyFree();
            }
            else
            {
                AdsWrapperManager.ShowReward(KeyAds.AdsJourneyToSuccess, OnDoneWatchADS, UIManager.Instance.ShowToastInternet);
            }
        }
        private void OnDoneWatchADS()
        {
            int index = itemCache.index;
            EventManager.Instance.journeyToSuccessManager.dataEvent.AddDataCountADS(index);
            var db = journeyToSuccessManager.GetDBJourneyResources(index);
            int countAdsCurrent = journeyToSuccessManager.dataEvent.GetCountADS(index);
            if (countAdsCurrent >= db.countAds)
            {
                BuyFree();
            }
            else
            {
                RefreshUI();
            }

        }

        public virtual void BuyIAP()
        {
#if ENABLE_IAP
            IAPManager.PurchaseResultListener += OnPurchaseComplete;
#endif
            SetBuyProduct();
        }
        private void SetBuyProduct()
        {
#if ENABLE_IAP
            IAPManager.PurchaseProduct(itemCache.dbPack.where, itemCache.dbPack.productId);
#endif
        }
#if ENABLE_IAP
        private void OnPurchaseComplete(IAPPurchaseResult iappurchaseresult)
        {
            IAPManager.PurchaseResultListener -= OnPurchaseComplete;
            switch (iappurchaseresult.Result)
            {
                case IAPPurchaseResult.EResult.Complete:
                    OnCompleteBuyIAP();
                    // iappurchaseresult.Product.Reward - Reward setup in stats
                    // iappurchaseresult.Product.Reward.PackRewardValue - give reward amount
                    // iappurchaseresult.Product.Reward.Reward - Type Reward > REMOVE_AD, CURRENCY (CASH OR GOLD), CUSTOM (Item Or Tool)
                    // iappurchaseresult.Product.Reward.atlas - Reward give Currency Id or Item, Tool Id (example: CASH, GOLD, TOOL_1...)
                    // todo give product reward
                    break;
                case IAPPurchaseResult.EResult.WrongInstance:
                    // Purchase faield: IAP Manager instance null (Read Setup IAP)  
                    break;
                case IAPPurchaseResult.EResult.WrongProduct:
                    // Purchase faield: can't find product with id 
                    break;
                case IAPPurchaseResult.EResult.WrongStoreController:
                    // Purchase faield: IAP initialized faield
                    break;
            }
        }
#endif
        private void OnCompleteBuyIAP()
        {
            int index = itemCache.index;
            var gift = db_Event.GetRewardType(ETypeResource.None).gifts;
            Vector3 pos = RectTransformUtility.WorldToScreenPoint(null, itemCache.listResUI.transform.position);
            //DataWrapperGame.ReceiveRewardShowPopUpAnim(ValueFirebase.JourneyToSuccess, OnCompleteBuy, true, pos, gift[index].groupGift.dataResources.ToArray());
            var ui = Luzart.UIManager.Instance.ShowUI<UIReceiveRes>(UIName.ReceiveRes, OnCompleteBuy);
            ui.Initialize(true, pos, gift[index].groupGift.dataResources.ToArray());
            PackManager.Instance.SaveBuyPack(itemCache.dbPack.productId);
            journeyToSuccessManager.ClaimItem(index);
        }
        public VerticalLayoutGroup verticalLayoutGroup;
        private void OnCompleteBuy()
        {
            obBlock.SetActive(true);
            itemCache.OnHide(OnDoneBuy, () =>
            {
                verticalLayoutGroup.enabled = false;
                verticalLayoutGroup.enabled = true;
            });
        }
        private void OnDoneBuy()
        {
            //obBlock.SetActive(false);
            int index = itemCache.index + 1;
    
            var gift = db_Event.GetRewardType(ETypeResource.None).gifts;
            if (gift == null || gift.Count == 0)
            {
                Hide();
                return;
            }
            int length = gift.Count;
    
            if (index >= length)
            {
                Hide();
                return;
            }
            listJournetToSuccess[index].UnlockAnim(RefreshUI);
        }
        public override void Hide()
        {
            base.Hide();
            Observer.Instance.Notify(ObserverKey.OnCheckNotiTreasure);
        }
    }
}
