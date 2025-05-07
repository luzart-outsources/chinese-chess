namespace Luzart
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class ButtonEvent : MonoBehaviour
    {
        public EEventName eEventName;
        public Button btn;
        public bool isAnimButton = true;
        public bool isAutoInit = true;

        protected virtual void Start()
        {
            GameUtil.ButtonOnClick(btn, ClickBtnEvent, isAnimButton);
            UIManager.AddActionRefreshUI(InitRefreshUI);
            if (isAutoInit)
            {
                InitEvent(null);
            }
        }
        protected virtual void OnDestroy()
        {
            UIManager.RemoveActionRefreshUI(InitRefreshUI);
        }

        public Action actionClick;
        public virtual bool IsActiveEvent
        {
            get
            {
                return EventManager.Instance.IsHasEvent(eEventName);
            }
        }
        public virtual bool IsUnlockLevel
        {
            get
            {
                return EventManager.Instance.IsUnlockLevel(eEventName, DataWrapperGame.CurrentLevel);
            }
        }
        public virtual void InitEvent(Action action)
        {
            this.actionClick = action;
            if(gameObject == null)
            {
                return;
            }
            if (IsActiveEvent && IsUnlockLevel)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
                return;
            }
            InitButton();
        }
        protected virtual void InitButton()
        {
    
        }
        private void ClickBtnEvent()
        {
            actionClick?.Invoke();
        }
        public virtual void InitRefreshUI()
        {
            InitEvent(null);
        }
    }
}
