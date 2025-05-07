namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class ItemTicketTallyUI : MonoBehaviour
    {
        public Button btnClick;
        public int level;
        public List<DataResource> dataRes { get; set; }
        public ResUI resUI;
        public TMP_Text txtStt;
        public BaseSelect bsIconLock;
        public BaseSelect bsLock;
        public BoxInforMess boxInforMess;
        void Start()
        {
            GameUtil.ButtonOnClick(btnClick, Click);
        }
        private Action<ItemTicketTallyUI> ActionClick = null;
        private void Click()
        {
            ActionClick?.Invoke(this);
        }
        public void Initialize(int level, GiftEvent giftEvent, EStateClaim eState, Action<ItemTicketTallyUI> actionClick)
        {
            this.ActionClick = actionClick;
            this.level = level;
            this.dataRes = giftEvent.groupGift.dataResources;
            txtStt.text = $"{level + 1}";
            if (giftEvent.IsHasChest)
            {
                resUI.InitData(new DataResource(giftEvent.groupGift.typeChest, 1));
            }
            else
            {
                resUI.InitData(giftEvent.groupGift.dataResources[0]);
            }
            SwitchState(eState);
            boxInforMess.InitMess(eState, null);
        }
        private void SwitchState(EStateClaim eState)
        {
            switch (eState)
            {
                case EStateClaim.CanClaim:
                    {
                        bsLock?.Select(0);
                        bsIconLock?.Select(0);
                        break;
                    }
                case EStateClaim.Claimed:
                    {
                        bsLock?.Select(1);
                        bsIconLock?.Select(1);
                        break;
                    }
                case EStateClaim.WillClaim:
                    {
                        bsLock?.Select(2);
                        bsIconLock?.Select(2);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}

