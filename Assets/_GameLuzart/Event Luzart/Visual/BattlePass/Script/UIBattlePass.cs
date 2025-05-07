namespace Luzart
{
    using DG.Tweening;
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIBattlePass : UIBase
    {

        [Space, Header("Scroll View")]
        public ScrollRect scrollRect;  // ScrollRect để di chuyển qua các phần thưởng
        public BattlePassItemUI battlePassItemPf;  // Prefab cho BattlePassItem
        public Transform parentBattlePassItem;  // Vị trí chứa các BattlePassItem
        public List<BattlePassItemUI> listItem = new List<BattlePassItemUI>();  // Danh sách các BattlePassItem

        [Space, Header("Other")]
        public TMP_Text txtContainCurrent;  // Hiển thị số phần thưởng đã nhận
        public ProgressBarUI progressBarUI;  // Thanh tiến độ
        public TMP_Text txtLevelCurrent;  // Cấp độ hiện tại
        public TMP_Text txtTime;  // Thời gian còn lại
        public GameObject obEnergyBar;  // Thanh năng lượng

        private DB_Event dBEvent;  // Dữ liệu sự kiện
        private DB_GiftEvent dataNormal;  // Dữ liệu phần thưởng miễn phí
        private DB_GiftEvent dataVIP;  // Dữ liệu phần thưởng VIP

        private BattlePassManager battlePassManager
        {
            get
            {
                return EventManager.Instance.battlePassManager;  // Truy cập BattlePassManager từ EventManager
            }
        }

        private BattlePassItemUI battlePassItemUICache = null;  // Biến lưu phần thưởng hiện tại
        public GameObject obBlockRaycast;

        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            // Hiển thị UI BattlePass và làm mới thông tin
            dBEvent = battlePassManager.dataEvent.dbEvent;
            // Nếu sự kiện đã kết thúc, ẩn UI
            if (dBEvent == null || dBEvent.eventStatus == EEventStatus.Finish)
            {
                Hide();
                return;
            }
            //
            RefreshUI();
            OnTimePerSecond(null);  // Cập nhật thời gian mỗi giây
            if(battlePassManager.dataEvent.cacheKeyUI == 0)
            {
                // Nếu người chơi đã đạt cấp độ tối đa
                int level = GetLevelCurrent(battlePassManager.dataEvent.totalUse);
                battlePassItemUICache = listItem[level];
                OnMoveToAnimation();  // Di chuyển ScrollRect đến phần thưởng hiện tại
                ShowTutorial();  // Hiển thị hướng dẫn nếu cần
            }
            else
            {
                int preTotalUse = battlePassManager.dataEvent.totalUse - battlePassManager.dataEvent.cacheKeyUI;
                int totalUse = battlePassManager.dataEvent.totalUse;
                CheckIfBattlePass(preTotalUse, totalUse);
            }
        }
        private int GetLevelCurrent(int totalUse)
        {
            int levelCurrent = battlePassManager.dataEvent.LevelByTotalUse(totalUse);  // Lấy cấp độ hiện tại

            // Nếu người chơi đã đạt cấp độ tối đa
            if (levelCurrent == battlePassManager.dataEvent.MaxLevel)
            {
                return listItem.Count - 1;
            }
            else
            {
                return levelCurrent;
            }
        }
        public override void RefreshUI()
        {
            // Làm mới UI
            base.RefreshUI();

            dBEvent = battlePassManager.dataEvent.dbEvent;

            // Nếu sự kiện đã kết thúc, ẩn UI
            if (dBEvent == null || dBEvent.eventStatus == EEventStatus.Finish)
            {
                Hide();
                return;
            }

            // Lấy dữ liệu phần thưởng từ event
            dataNormal = dBEvent.GetRewardType(ETypeResource.None);
            dataVIP = dBEvent.GetRewardType(ETypeResource.VIP);

            // Khởi tạo danh sách các BattlePassItem
            ReInitAllItem();
            SetUpProgressKey();  // Cập nhật tiến độ và cấp độ
        }
        private void ReInitAllItem(int totalUse = -1, bool isControlSlider = true)
        {
            if(totalUse == -1)
            {
                totalUse = battlePassManager.dataEvent.totalUse;
            }
            // Khởi tạo danh sách các BattlePassItem
            int length = dataNormal.gifts.Count;
            MasterHelper.InitListObj(length, battlePassItemPf, listItem, parentBattlePassItem, (item, index) =>
            {
                item.gameObject.SetActive(true);
                InitItem(item, index, totalUse, isControlSlider);  // Khởi tạo từng BattlePassItem
            });
        }
        private void SetUpProgressKey(int totalUse = -1)
        {
            if(totalUse == -1)
            {
                totalUse = battlePassManager.dataEvent.totalUse;
            }
            // Cập nhật thông tin về tiến độ và cấp độ
            int levelCurrent = GetLevelCurrent(totalUse);
            int contain = battlePassManager.dataEvent.ContainByTotalUse(totalUse);
            int totalRequire = battlePassManager.dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts[levelCurrent].require;

            // Cập nhật UI
            txtContainCurrent.text = $"{contain}/{totalRequire}";
            txtLevelCurrent.text = $"{levelCurrent + 1}";
            float percent = (float)contain / (float)totalRequire;
            progressBarUI.SetSlider(percent, percent, 0f, null);  // Cập nhật thanh tiến độ
        }
        private void SetUpProgressKey(int preTotalUse, int totalUse, float timeMove = 0.1f)
        {
            // Cập nhật thông tin về tiến độ và cấp độ
            int preLevelCurrent = GetLevelCurrent(preTotalUse);
            int preContain = battlePassManager.dataEvent.ContainByTotalUse(preTotalUse);
            int preTotalRequire = battlePassManager.dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts[preLevelCurrent].require;

            // Cập nhật thông tin về tiến độ và cấp độ
            int levelCurrent = GetLevelCurrent(totalUse);
            int contain = battlePassManager.dataEvent.ContainByTotalUse(totalUse);
            int totalRequire = battlePassManager.dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts[levelCurrent].require;

            // Cập nhật UI
            txtContainCurrent.text = $"{preContain}/{preTotalRequire}";
            txtLevelCurrent.text = $"{preLevelCurrent + 1}";

            float prePercent = (float)preContain / (float)preTotalRequire;

            float percent = (float)contain / (float)totalRequire;

            if(percent == 0)
            {
                Sequence sq = DOTween.Sequence();
                sq.Append(progressBarUI.SetSliderTween(prePercent, 1, timeMove, () =>
                {
                    txtContainCurrent.text = $"{preTotalRequire}/{preTotalRequire}";
                    txtLevelCurrent.text = $"{preLevelCurrent + 1}";
                }));
                sq.AppendCallback(()=> progressBarUI.SetSliderTween(0, 0, 0));
                sq.AppendCallback(() =>
                {
                    txtContainCurrent.text = $"{contain}/{totalRequire}";
                    txtLevelCurrent.text = $"{levelCurrent + 1 }";
                });
            }
            else
            {
                progressBarUI.SetSliderTween(prePercent, percent, timeMove, () =>
                {
                    txtContainCurrent.text = $"{contain}/{totalRequire}";
                    txtLevelCurrent.text = $"{levelCurrent + 1}";
                });
            }
        }
        private void InitItem(BattlePassItemUI item, int index, int totalUse = -1, bool isControlSlider = true)
        {
            if(totalUse == -1)
            {
                totalUse = battlePassManager.dataEvent.totalUse;
            }
            // Khởi tạo phần thưởng cho từng item trong danh sách
            int levelCurrent = battlePassManager.dataEvent.LevelByTotalUse(totalUse);
            int contain = battlePassManager.dataEvent.ContainByTotalUse(totalUse);
            int totalRequire = 0;

            if (!(levelCurrent == battlePassManager.dataEvent.MaxLevel))
            {
                totalRequire = battlePassManager.dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts[levelCurrent].require;
            }

            float percent = 0f;
            EStatePass eStatePass = EStatePass.WillPass;

            // Cập nhật trạng thái phần thưởng
            if (levelCurrent > index)
            {
                percent = 1f;
                eStatePass = EStatePass.Pass;
            }
            else if (levelCurrent == index)
            {
                percent = (float)contain / (float)totalRequire;
                eStatePass = EStatePass.Current;
            }

            // Khởi tạo phần thưởng miễn phí và VIP
            var dataNor = dataNormal.gifts[index];
            var dataVip = dataVIP.gifts[index];

            item.InitSliderBattlePass(eStatePass, index, percent);  // Khởi tạo thanh tiến độ cho item
            if (isControlSlider)
            {
                item.SetSliderOpen(eStatePass == EStatePass.Current);
            }

            // Phần thưởng miễn phí
            EStateClaim eStateNormal = EStateClaim.WillClaim;
            if (dataNor.isClaimed)
            {
                eStateNormal = EStateClaim.Claimed;
            }
            else if (levelCurrent > index)
            {
                eStateNormal = EStateClaim.CanClaim;
            }
            item.InitItemFree(dataNor.groupGift.dataResources.ToArray(), eStateNormal, ClickItemNormal, dataNor.typeChest);

            // Phần thưởng VIP
            EStateClaim eStateVIP = EStateClaim.NeedIAP;
            if (battlePassManager.dataEvent.isBuyIAP)
            {
                eStateVIP = EStateClaim.WillClaim;
                if (dataVip.isClaimed)
                {
                    eStateVIP = EStateClaim.Claimed;
                }
                else if (levelCurrent > index)
                {
                    eStateVIP = EStateClaim.CanClaim;
                }
            }
            item.InitItemIAP(dataVip.groupGift.dataResources.ToArray(), eStateVIP, ClickItemVIP, dataVip.typeChest);
        }

        // Sự kiện khi người chơi nhấn vào phần thưởng miễn phí
        private void ClickItemNormal(ItemResBattlePass item)
        {
            switch (item.eState)
            {
                case EStateClaim.CanClaim:
                    DataWrapperGame.ReceiveRewardShowPopUp(ValueFirebase.BattlePassFree, dataResource: item.dataRes);
                    battlePassManager.UseItemNormal(item.index);
                    RefreshUI();
                    break;
                default:
                    item.boxInforMess.gameObject.SetActive(true);
                    break;
            }
        }

        // Sự kiện khi người chơi nhấn vào phần thưởng VIP
        private void ClickItemVIP(ItemResBattlePass item)
        {
            switch (item.eState)
            {
                case EStateClaim.CanClaim:
                    DataWrapperGame.ReceiveRewardShowPopUp(ValueFirebase.BattlePassPay, dataResource: item.dataRes);
                    battlePassManager.UseItemVIP(item.index);
                    RefreshUI();
                    break;
                default:
                    item.boxInforMess.gameObject.SetActive(true);
                    break;
            }
        }

        // Hàm di chuyển đến phần thưởng hiện tại trong ScrollRect
        private void OnMoveToAnimation()
        {
            if (battlePassItemUICache != null)
            {
                GameUtil.Instance.WaitAFrame(() =>
                {
                    int indexNext = GetLevelCurrent(battlePassManager.dataEvent.totalUse);
                    indexNext = Mathf.Clamp(indexNext - 1, 0, listItem.Count);
                    var battlePassItemUICacheMove = listItem[indexNext];
                    scrollRect.FocusOnRectTransform(battlePassItemUICacheMove.rectTransform);  // Di chuyển đến phần thưởng hiện tại
                });
            }
        }

        public override void Hide()
        {
            base.Hide();
            UIManager.Instance.RefreshUI();
        }

        public void OnClickShowBattlePass()
        {
            if (battlePassManager.dataEvent.isBuyIAP)
            {
                return;
            }
            UIManager.Instance.ShowUI(UIName.PackBattlePass);
        }

        public void OnClickInforBattlePass()
        {
            UIManager.Instance.ShowUI(UIName.BattlePassTutorial);
        }
        public float timeDelayToMove = 0.5f;
        public float timeMoveInit = 0.3f;
        private void CheckIfBattlePass(int preTotalUse, int totalUse)
        {
            ReInitAllItem(preTotalUse, false);
            SetUpProgressKey(preTotalUse);  // Cập nhật tiến độ và cấp độ
            battlePassItemUICache = listItem[GetLevelCurrent(preTotalUse)];
            scrollRect.FocusOnRectTransform(battlePassItemUICache.rectTransform);
            //
            battlePassManager.dataEvent.CalculateProgress(preTotalUse, out int preLevelCurrent, out int preContain, out int preTotalRequire, out float prePercent);
            battlePassManager.dataEvent.CalculateProgress(totalUse, out int levelCurrent, out int contain, out int totalRequire, out float percent);
            //
            Sequence sq = DOTween.Sequence();
            int totalCur = preTotalUse;
            int length = totalUse - preTotalUse;
            sq.AppendInterval(timeDelayToMove);
            if (preLevelCurrent != levelCurrent)
            {
                BlockRaycast(true);
                sq.AppendCallback(()=> battlePassItemUICache.SetAnimLineDown(false, timeMoveInit));
                sq.AppendInterval(timeMoveInit);
            }

            for (int i = 0; i < length; i++)
            {
                sq.AppendCallback(() => 
                {
                    int indexNext = GetLevelCurrent(totalCur + 1);
                    battlePassItemUICache = listItem[indexNext];
                    indexNext = Mathf.Clamp(indexNext - 1, 0, listItem.Count);
                    var itemCache = listItem[indexNext];
                    scrollRect.FocusOnRectTransform(itemCache.rectTransform,false,timeMoveInit);
                    SetUpProgressKey(totalCur, totalCur + 1, timeMoveInit);
                    MoveSliderMain(totalCur, totalCur + 1, timeMoveInit);
                });
                sq.AppendInterval(timeMoveInit);
                sq.AppendCallback(() => 
                {
                    totalCur++;
                    ReInitAllItem(totalCur, false);
                });
            }
            sq.AppendCallback(() => battlePassItemUICache.SetAnimLineDown(true, timeMoveInit));
            sq.AppendInterval(timeMoveInit);
            sq.AppendCallback(() =>
            {
                BlockRaycast(false);
                battlePassManager.dataEvent.cacheKeyUI = 0;
                battlePassManager.SaveData();
            });
            sq.SetId(this);
        }
        private Tween MoveSliderMain(int preTotalUse, int totalUse, float timeMove)
        {
            battlePassManager.dataEvent.CalculateProgress(preTotalUse, out int preLevelCurrent, out int preContain, out int preTotalRequire, out float prePercent);
            battlePassManager.dataEvent.CalculateProgress(totalUse, out int levelCurrent, out int contain, out int totalRequire, out float percent);
            if(preLevelCurrent == levelCurrent)
            {
                var item = listItem[levelCurrent];
                return item.InitSliderBattlePass(prePercent,percent,timeMove);
            }
            else
            {
                Sequence sq = DOTween.Sequence();
                var itemPre = listItem[preLevelCurrent];
                sq.Append(itemPre.InitSliderBattlePass(prePercent, 1, timeMove)); // Di chuyển đến 100% trong nửa thời gian

                // Tính toán thời gian cần cho mỗi bước giữa các cấp độ
                float stepTime = timeMove / (levelCurrent - preLevelCurrent + 1); // Chia đều thời gian cho tất cả các bước
                for (int i = 0; i < (levelCurrent - preLevelCurrent - 1); i++)
                {
                    int index = i;
                    int indexFor = preLevelCurrent + index + 1;
                    var itemFor = listItem[indexFor];
                    sq.Append(itemFor.InitSliderBattlePass(0, 1, stepTime)); // Di chuyển từ 0 đến 100% cho các bước cấp độ
                }

                var curItem = listItem[levelCurrent];
                // Cuối cùng, di chuyển thanh progress từ 0 đến percent trong thời gian còn lại
                sq.Append(curItem.InitSliderBattlePass(0, percent, stepTime));
                return sq;
            }
        }
        private void Awake()
        {
            // Đăng ký sự kiện theo dõi thời gian mỗi giây
            Observer.Instance?.AddObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }

        private void OnDestroy()
        {
            // Hủy sự kiện khi đối tượng bị hủy
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        private void OnDisable()
        {
            this.DOKill(true);
        }

        private void OnTimePerSecond(object data)
        {
            // Cập nhật thời gian còn lại mỗi giây
            long timeCurrent = TimeUtils.GetLongTimeCurrent;
            long timeContain = battlePassManager.dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
            txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
        }

        [Space, Header("Tutorial")]
        public GameObject obTutorial;
        public Button btnTutorial;

        public void OnClickBtnTutorial()
        {
            obTutorial.SetActive(false);
            UIManager.Instance.ShowUI(UIName.BattlePassTutorial);
        }

        public void ShowTutorial()
        {
            bool isTuto = EventManager.Instance.dataEvent.GetTutorialComplete(EEventName.BattlePass);
            if (!isTuto)
            {
                obTutorial.SetActive(true);
                EventManager.Instance.dataEvent.SetTutorialComplete(EEventName.BattlePass, true);
                return;
            }
        }

        public void BlockRaycast(bool isBlock)
        {
            obBlockRaycast?.SetActive(isBlock);
        }
    }
}


//private void InitItem(BattlePassItemUI item, int index, int preTotalUse, int totalUse)
//{
//    int levelCurrent = GetLevelCurrent(preTotalUse);

//    EStatePass eStatePass = EStatePass.WillPass;
//    // Cập nhật trạng thái phần thưởng
//    if (levelCurrent > index)
//    {
//        percent = 1f;
//        eStatePass = EStatePass.Pass;
//    }
//    else if (levelCurrent == index)
//    {
//        percent = (float)contain / (float)totalRequire;
//        eStatePass = EStatePass.Current;
//    }

//    Sequence sq = DOTween.Sequence();
//    sq.AppendCallback(() =>
//    {
//        InitItem(item, index, preTotalUse);
//    });
//    sq.Append(item.InitSliderBattlePass(eStatePass, index, percent, percent);  // Khởi tạo thanh tiến độ cho item)

//}
