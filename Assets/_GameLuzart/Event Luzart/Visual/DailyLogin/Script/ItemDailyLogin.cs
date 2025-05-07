namespace Luzart
{
    using Luzart;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class ItemDailyLogin : MonoBehaviour
    {
        public Button btnClick;
        public DBDailyLogin dbDailyLogin;
        public ListResUI listResUI;
        public BaseSelect bsFrameCurrentDay;
        public BaseSelect bsSelect;
        public BaseSelect bsComplete;
        public EStateClaim eStateClaim;
        public TMP_Text txtDay;
        private Action<ItemDailyLogin> actionClick;
        private DailyLoginManager dailyLoginManager
        {
            get
            {
                return EventManager.Instance.dailyLoginManager;
            }
        }
        private void Start()
        {
            GameUtil.ButtonOnClick(btnClick, Click);
        }
    
        public void Initialize(DBDailyLogin db, Action<ItemDailyLogin> actionClick)
        {
            this.dbDailyLogin = db;
            this.actionClick = actionClick;
            txtDay.text = $"Day {dbDailyLogin.index + 1}";
    
            CheckState();
            ChangeVisualOnState();
            InitResUI();
        }
        private void CheckState()
        {
            if (dailyLoginManager.dataEvent.IsClaimedDay(dbDailyLogin.index))
            {
                eStateClaim = EStateClaim.Claimed;
                return;
            }
            int today = dailyLoginManager.Today;
            if (today > dbDailyLogin.index)
            {
                eStateClaim = EStateClaim.CanClaimDontClaimed;
            }
            else if (today == dbDailyLogin.index)
            {
                eStateClaim = EStateClaim.CanClaim;
            }
            else
            {
                eStateClaim = EStateClaim.WillClaim;
            }
        }
        private void ChangeVisualOnState()
        {
            int today = dailyLoginManager.Today;
            bool isToday = today == dbDailyLogin.index;
            bsFrameCurrentDay.Select(isToday);
            switch (eStateClaim)
            {
                case EStateClaim.Claimed:
                    {
                        bsComplete.Select(2);
                        break;
                    }
                case EStateClaim.CanClaimDontClaimed:
                    {
                        bsComplete.Select(0);
                        break;
                    }
                case EStateClaim.WillClaim:
                    {
                        bsComplete.Select(1);
                        break;
                    }
                case EStateClaim.CanClaim:
                    {
                        bsComplete.Select(1);
                        break;
                    }
            }
        }
        private void Click()
        {
            actionClick?.Invoke(this);
        }
        public void Select(bool isSelect)
        {
            bsSelect.Select(isSelect);
        }
        private void InitResUI()
        {
            listResUI?.InitResUI(dbDailyLogin.dataRes.dataResources.ToArray());
        }
    
    }
    public class DBDailyLogin
    {
        public int index;
        public GroupDataResources dataRes;
    }
}
