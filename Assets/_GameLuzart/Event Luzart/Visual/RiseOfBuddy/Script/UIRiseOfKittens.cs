namespace Luzart
{
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UIRiseOfKittens : UIBase
    {
        public ScrollRect scrollRect;

        public Transform parentSpawn;
        public ItemRiseOfKittens itemRisePf;
        private List<ItemRiseOfKittens> listItemRiseOfKittens = new List<ItemRiseOfKittens> ();

        public Transform parentSpawnLight;
        public BaseSelect bsLightPf;
        private List<BaseSelect> listLight = new List<BaseSelect>();

        public ItemBeMoveRise itemBeMoveRise;

        private RiseOfKittensManager riseOfKittensManager
        {
            get
            {
                return EventManager.Instance.riseOfKittensManager;
            }
        }
        private ItemRiseOfKittens itemRiseCache;
        private float height = 10000;
        public RectTransform rtHeight;
        public Button btnInformation;
        public TMP_Text txtTime;
        [Space, Header("Tutorial")]
        public Button btnInformationTutorial;
        public GameObject obTutorial;
        protected override void Setup()
        {
            base.Setup();
            height = rtHeight.sizeDelta.y ;
            GameUtil.ButtonOnClick(btnInformation, ClickInformation, true);
            GameUtil.ButtonOnClick(btnInformationTutorial, ClickInformationTutorial, true);
        }
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
            long timeContain = riseOfKittensManager.dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
            txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
        }
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            RefreshUI();
            IsOpenTutorial();
        }

        private void IsOpenTutorial()
        {
            if (EventManager.Instance.dataEvent.GetTutorialComplete(EEventName.RiseOfKittens))
            {
                obTutorial.SetActive(false);
            }
            else
            {
                obTutorial.SetActive(true);
            }
        }
        public override void RefreshUI()
        {
            base.RefreshUI();
            Initialize();
        }
   
        private void ClickInformation()
        {
            UIManager.Instance.ShowUI(UIName.RiseOfKittenTutorial);
        }
        private void ClickInformationTutorial()
        {
            obTutorial.SetActive(false);
            UIManager.Instance.ShowUI(UIName.RiseOfKittenTutorial);
        }
        private void Initialize()
        {
            // Sinh ra den
            int totalRequireMaxLevel = riseOfKittensManager.dataEvent.TotalRequireMaxLevel;
            MasterHelper.InitListObj(totalRequireMaxLevel, bsLightPf, listLight, parentSpawnLight, (item, index) =>
            {
                item.gameObject.SetActive(true);
                item.Select(false);
                var rt = item.GetComponent<RectTransform>();
                float percentItem = (float)index / (float)riseOfKittensManager.dataEvent.TotalRequireMaxLevel;
                float heightItem = percentItem * height;
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, heightItem);
            });
            // Sinh ra cac moc
            var dbGiftEvent = riseOfKittensManager.dataEvent.dbEvent.GetRewardType(ETypeResource.None);
            var list = dbGiftEvent.gifts;
            int lengthMoc = list.Count;
            MasterHelper.InitListObj(lengthMoc, itemRisePf, listItemRiseOfKittens, parentSpawn, (item, index) =>
            {
                item.gameObject.SetActive(true);
                var db = list[index];
                item.Initialize(index, db, ClickItem);
                float percentItem = (float)riseOfKittensManager.dataEvent.TotalRequireByIndex(index)/ (float)riseOfKittensManager.dataEvent.TotalRequireMaxLevel;
                float heightItem = percentItem * height;
                item.rectTransform.anchoredPosition = new Vector2(item.rectTransform.anchoredPosition.x, heightItem);
            });

            int totalCurrent = riseOfKittensManager.dataEvent.totalUse;
            SetLevel(totalCurrent);

            listItemRiseOfKittens[totalCurrent].SetClaimChest( IsClaimedTotalUse(totalCurrent) );

            CheckIfHas();
        }
        private void SetLevel(int totalUse)
        {
            float percent = (float)totalUse / (float)riseOfKittensManager.dataEvent.TotalRequireMaxLevel;
            float heightCurrent = percent * (float)height;
            // Chỉnh sửa vị trí của item di chuyển
            var pos = new Vector2(itemBeMoveRise.rectTransform.anchoredPosition.x, heightCurrent);
            itemBeMoveRise.Initialize(pos, totalUse.ToString());
            ControlLight(totalUse);
            var index = riseOfKittensManager.dataEvent.IndexByTotalUse(totalUse - 1);
            SetClaimedLevel(totalUse);
        }
        private void SetClaimedLevel(int totalUse)
        {
            int indexByTotalUse = riseOfKittensManager.dataEvent.IndexByTotalUse(totalUse-1);
            if (indexByTotalUse == -1)
            {
                return;
            }
            int length = listItemRiseOfKittens.Count;
            for (int i = 0; i < length; i++)
            {
                int index = i;
                bool isClaimedChest = index < indexByTotalUse;
                listItemRiseOfKittens[i].SetClaimChest(isClaimedChest);
            }
        }
        private bool IsClaimedTotalUse(int totalUse)
        {
            int indexByTotalUse = riseOfKittensManager.dataEvent.IndexByTotalUse(totalUse - 1);
            if (indexByTotalUse == -1)
            {
                return false;
            }
            return riseOfKittensManager.dataEvent.dbEvent.GetRewardType(ETypeResource.None)
                .gifts[indexByTotalUse]
                .isClaimed; 
        }
        private void ControlLight(int indexLight)
        {
            int length = listLight.Count;
            for (int i = 0; i < length; i++)
            {
                int index = i;
                bool isLight = indexLight >= index;
                listLight[index].Select(isLight);
            }
        }
        private void CheckIfHas()
        {
            int preLevel = riseOfKittensManager.dataEvent.totalUseCacheUI;
            int curLevel = riseOfKittensManager.dataEvent.totalUse;
            if(preLevel == curLevel)
            {
                int levelDontClaimed = riseOfKittensManager.GetLevelReturnDontClaim;
                if(levelDontClaimed == -1)
                {
                    scrollRect.ScrollToCenter(itemBeMoveRise.rectTransform, true);
                    return;
                }
                else
                {
                    MoveFromTo(levelDontClaimed, curLevel, 0.35f, OnDoneMove);
                }
            }
            else
            {
                MoveFromTo(preLevel, curLevel, 0.35f, OnDoneMove);
            }
        }
        private async UniTask OnDoneMove(int indexMoveTo)
        {
            bool isComplete = false;
            Action onDone = ()=>
            {
                isComplete = true;
            };
            bool isReceive = riseOfKittensManager.dataEvent.IsLevelRequire(indexMoveTo, out int index);
            if (isReceive)
            {
                var dataRes = riseOfKittensManager.dataEvent.dbEvent
                    .GetRewardType(ETypeResource.None)
                    .gifts[index]
                    .groupGift
                    .dataResources;

                Vector3 pos = RectTransformUtility.WorldToScreenPoint(null, listItemRiseOfKittens[index].transform.position);
                DataWrapperGame.ReceiveRewardShowPopUpAnim(ValueFirebase.WinStreakReceived, onDone, true, pos, dataRes.ToArray());

                riseOfKittensManager.UseItem(index);
                riseOfKittensManager.dataEvent.totalUseCacheUI = indexMoveTo;
                riseOfKittensManager.SaveData();
                await UniTask.WaitUntil(() => isComplete);
                GameUtil.Instance.WaitAFrame(() => listItemRiseOfKittens[index].SetClaimChest(true));
            }
        }
        private async UniTask MoveFromTo(int fromIndex, int toIndex, float timeMove, Func<int, UniTask> onMoveStepDone)
        {
            for (int i = fromIndex; i < toIndex; i++)
            {
                int index = i;
                await MoveNextLevel(index, timeMove);
                if (onMoveStepDone != null)
                    await onMoveStepDone.Invoke(index + 1); // Truyền index sau khi move xong
            }
            riseOfKittensManager.dataEvent.totalUseCacheUI = toIndex;
            riseOfKittensManager.SaveData();
        }

        private async UniTask MoveNextLevel(int index, float timeMove)
        {
            int nextIndex = index + 1;
            SetLevel(index);

            float percent = (float)nextIndex / (float)riseOfKittensManager.dataEvent.TotalRequireMaxLevel;
            float heightNext = percent * height;
            bool isComplete = false;
            Tween tw = itemBeMoveRise.rectTransform
                            .DOAnchorPos3DY(heightNext, timeMove)
                            .SetEase(Ease.Linear)
                            .OnUpdate(() => scrollRect.ScrollToCenter(itemBeMoveRise.rectTransform, true));
            tw.OnComplete(() =>
            {
                isComplete = true;
            });
            await UniTask.WaitUntil(() => isComplete == true);
            SetLevel(nextIndex);
        }

        private void ClickItem(ItemRiseOfKittens item)
        {
            itemRiseCache = item;
            switch (item.eState)
            {
                case EStateClaim.CanClaim:
                    {
                        ClaimChest(item);
                        break;
                    }
                default:
                    {
                        item.ShowChest();
                        break;
                    }
            }
        }
        private void ClaimChest(ItemRiseOfKittens item)
        {
            item.SelectAnim(true);
            SetBlock(true);
            GameUtil.Instance.WaitAndDo(0.6f, () =>
            {
                SetBlock(false);
                var data = item.dataRes;
                DataWrapperGame.ReceiveRewardShowPopUp(ValueFirebase.RiseOfKittensReceive, UIManager.Instance.RefreshUI, data);
                riseOfKittensManager.UseItem(item.index);
                UIManager.Instance.RefreshUI();
            }
        );
        }
        public override void Hide()
        {
            base.Hide();
            UIManager.Instance.RefreshUI();
        }
        public GameObject obBlock;
        public void SetBlock(bool isStatus)
        {
            obBlock?.SetActive(isStatus);
        }
    
    }
}
