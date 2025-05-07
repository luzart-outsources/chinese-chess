namespace Luzart
{
#if ENABLE_IAP
    using BG_Library.IAP;
#endif
    using DG.Tweening;
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class ItemJourneyToSuccess : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public CanvasGroup canvasGroupLock;
        public Button btn;
        public ListResUI listResUI;
        public BaseSelect selectLock;
        public BaseSelect selectIAP ;
        private Action<ItemJourneyToSuccess> actionClick;
#if ENABLE_IAP
        public IAPProductPriceLegacy iapPrice;
#endif
        public int index;
        public EStateClaim eState;
        public DB_Pack dbPack;
        public TMP_Text txtWatchADs;
        public GameObject obAds;
        private GroupDataResources groupDataRes;
        //
        public BoxInforMess boxInforMess;
        private void Start()
        {
            GameUtil.ButtonOnClick(btn, ClickBoxInforMess);
        }
        private void ClickBoxInforMess()
        {
            boxInforMess.InitMess(EStateClaim.Chest, groupDataRes.dataResources.ToArray());
            boxInforMess.gameObject.SetActive(true);
        }
        public void OnClick()
        {
            actionClick?.Invoke(this);
        }

        public void InitializeData(int index, EStateClaim eStateClaim, Action<ItemJourneyToSuccess> actionClick,  GroupDataResources groupDataRes)
        {
            this.actionClick = actionClick;
            this.index = index;
            this.eState = eStateClaim;
            this.groupDataRes = groupDataRes;
            bool isReceive = eStateClaim == EStateClaim.Claimed;
            if (isReceive)
            {
                gameObject.SetActive(false);
                return;
            }
            bool isLock = eStateClaim == EStateClaim.WillClaim;
            selectLock.Select(isLock);
            var db = EventManager.Instance.journeyToSuccessManager.GetDBJourneyResources(index);
            switch(db.eTypeResources)
            {
                case ETypeResources.IAP:
                    {
                        selectIAP.Select(1);
                        this.dbPack = db.dbPack;
#if ENABLE_IAP
                        iapPrice.SetIAPProductStats(db.stats);
#endif
                        break;
                    }
                case ETypeResources.ADS:
                    {
                        selectIAP.Select(2);
                        int countAdsCurrent = EventManager.Instance.journeyToSuccessManager.dataEvent.GetCountADS(index);
                        bool isUnlocked = countAdsCurrent >= db.countAds;
                        string strCountADS = "";
                        if (db.countAds == 1)
                        {
                            strCountADS = "Free";
                        }
                        else
                        {
                            strCountADS = isUnlocked ? "Unlocked" : $"{countAdsCurrent}/{db.countAds}";
                        }
                        obAds?.SetActive(!isUnlocked);
                        txtWatchADs.text = strCountADS;
                        break;
                    }
                default:
                    {
                        selectIAP.Select(0);
                        break;
                    }
            }
            if (groupDataRes.IsHasChest)
            {
                DataResource dataRes = new DataResource(groupDataRes.typeChest,1);
                listResUI.InitResUI(dataRes);
            }
            else
            {
                listResUI.InitResUI(groupDataRes.dataResources.ToArray());
            }

        }
        public void OnHide(Action onDone, Action onUpdate)
        {
            RectTransform rt = gameObject.GetComponent<RectTransform>();
            Vector2 size = rt.sizeDelta;
            Sequence sq = DOTween.Sequence();
            sq.Append(canvasGroup.DOFade(0, 0.35f));
            sq.Append(rt.DOSizeDelta(new Vector2(size.x, 0), 0.2f).OnUpdate(() =>
            {
                onUpdate?.Invoke();
            }));
            sq.AppendCallback(()=> onDone?.Invoke());
            sq.SetId(this);
        }
        public void UnlockAnim(Action onDone)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(selectLock.transform.DOScale(2.5f, 0.35f));
            sequence.Join(canvasGroupLock.DOFade(0, 0.35f));
            sequence.AppendCallback(() => { onDone?.Invoke(); });
            sequence.SetId(this);
    
            
        }
        private void OnDisable()
        {
            this.DOKill(false);
        }
    }
}
