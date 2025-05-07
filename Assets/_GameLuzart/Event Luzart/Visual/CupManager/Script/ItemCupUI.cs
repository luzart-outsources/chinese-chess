namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class ItemCupUI : MonoBehaviour
    {
        private RectTransform _rtMe = null;
        public RectTransform rectTransform
        {
            get
            {
                if (_rtMe == null)
                {
                    _rtMe = GetComponent<RectTransform>();
                }
                return _rtMe;
            }
        }
    
        public Button btnGift;
        public TMP_Text txtName;
        public Image imAvt;
        public TMP_Text txtPoint;
        public TMP_Text txtIndex;
        public BaseSelect bsGift;
        public BaseSelect bsIndex;
        public BaseSelect bsFrameMe;
    
        public BoxInforMess boxInforMess;
        public Action<ItemCupUI> actionClickGift;
        public GiftEvent giftEvent;
        private void Start()
        {
            GameUtil.ButtonOnClick(btnGift, ClickGift, true);
        }
    
        public void Initialize(int index, DataBotCup dataBot, GiftEvent db_Gift, Action<ItemCupUI> onClickGift, Action<ItemCupUI> actionClickBox = null)
        {
            this.actionClickGift = onClickGift;
            this.giftEvent = db_Gift;
    
            bsIndex.Select(index >= 3 ? 3 : index);
            txtIndex.text = (index + 1).ToString();
            txtName.text = dataBot.nameBot;
            txtPoint.text = dataBot.point.ToString();
            imAvt.sprite = DataWrapperGame.AllSpriteAvatars[dataBot.idAvt];
            bool isMe = dataBot.idBot == -1;
            bsFrameMe.Select(isMe);
            if (index <= 2)
            {
                bsGift.Select(index);
            }
            else if (index >= 3 && db_Gift.groupGift.dataResources != null)
            {
                bsGift.Select(3);
            }
            else
            {
                bsGift.Select(index);
            }
    
            if (db_Gift.groupGift.dataResources == null)
            {
                return;
            }
            boxInforMess.InitMess(EStateClaim.Chest, db_Gift.groupGift.dataResources.ToArray());
            boxInforMess.actionClickBox = () => 
            {
                actionClickBox?.Invoke(this);
            };
    
        }
        private void ClickGift()
        {
            actionClickGift?.Invoke(this);
        }
    }
}
