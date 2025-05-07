namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class ItemResBattlePass : MonoBehaviour
    {
        private RectTransform _rt;
        public RectTransform rectTransform
        {
            get
            {
                if (_rt == null)
                {
                    _rt = GetComponent<RectTransform>();
                }
                return _rt;
            }
        }
        public int index { get; set; } = 0;
        public Button btnClick;
        private Action<ItemResBattlePass> actionClick;
        public ResUI resUI;
        public BoxInforMess boxInforMess;
        public BaseSelect bsClaimed;
        
        void Start()
        {
            GameUtil.ButtonOnClick(btnClick, OnClickButton);
        }
        public void OnClickButton()
        {
            actionClick?.Invoke(this);
        }
        public DataResource[] dataRes;
        public EStateClaim eState;
        public void InitVisual(int index,DataTypeResource chest,EStateClaim stateClaim, Action<ItemResBattlePass> actionClick, params DataResource[] data)
        {
            this.index = index;
            this.dataRes = data;
            eState = stateClaim;
            this.actionClick = actionClick;
            if (chest.id != 0)
            {
                resUI.InitData(new DataResource(chest, 1));
            }
            else
            {
                resUI.InitData(data[0]);
            }
    
            SwitchState();
            if(chest.id != 0)
            {
                stateClaim = EStateClaim.Chest;
            }
            boxInforMess.InitMess(stateClaim, data);
        }
        private void SwitchState()
        {
            //DisableAllButton();
            switch (eState)
            {
                case EStateClaim.CanClaim:
                    {
                        bsClaimed?.Select(0);
                        break;
                    }
                case EStateClaim.Claimed:
                    {
                        bsClaimed?.Select(1);
                        break;
                    }
                case EStateClaim.WillClaim:
                    {
                        bsClaimed?.Select(2);
                        break;
                    }
                case EStateClaim.NeedIAP:
                    {
                        bsClaimed?.Select(3);
                        break;
                    }
                default: 
                            break;
            }
        }
    }
    
}
