namespace Luzart
{
    using JetBrains.Annotations;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class ItemRiseOfKittens : MonoBehaviour
    {
        private RectTransform _rectTransform = null;
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }
        public BaseSelect bsChest;
        public BaseSelect bsChestClaimed;
        private Action<ItemRiseOfKittens> actionClick = null;
        public DataResource[] dataRes;
        public int index;
        public BoxInforMess boxInforMess;
        public TMP_Text txtIndex;
        private RiseOfKittensManager riseOfKittensManager
        {
            get
            {
                return EventManager.Instance.riseOfKittensManager;
            }
        }
        public EStateClaim eState;
        public void Initialize(int index , GiftEvent dB_giftEvent ,Action<ItemRiseOfKittens> actionClick = null)
        {
            this.index = index;
            this.dataRes = dB_giftEvent.groupGift.dataResources.ToArray();
            this.actionClick = actionClick;
            txtIndex.text = riseOfKittensManager.dataEvent.TotalRequireByIndex(index).ToString();
            SelectAnim(false);
            int idTypeChest = dB_giftEvent.typeChest.id;
            bsChest.Select(index);
            bool isClaim = dB_giftEvent.isClaimed;
            bsChestClaimed.Select(isClaim);
            boxInforMess.InitMess(EStateClaim.Chest, dataRes);
        }
        public void SetClaimChest(bool isClaimed)
        {
            bsChestClaimed.Select(isClaimed);
        }
        public void ShowChest()
        {
            boxInforMess.gameObject.SetActive(true);
        }
        private void ClickChest()
        {
            actionClick?.Invoke(this);
        }
        public void SelectAnim(bool isSelect)
        {

        }
    }
}
