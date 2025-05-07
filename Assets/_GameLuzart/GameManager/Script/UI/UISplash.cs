namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class UISplash : UIBase
    {
        public RectTransform rtSliderParent;
        public RectTransform rtSliderChild;
        public override void Show(Action onHideDone)
        {
            Vector2 sizeParent = rtSliderParent.sizeDelta;
            Vector2 size = rtSliderChild.sizeDelta;
            base.Show(onHideDone);
            GameUtil.Instance.StartLerpValue(this, 0, sizeParent.x, 5f, (x) =>
            {
                rtSliderChild.sizeDelta = new Vector2(x, size.y);
            }, InitStartGame);
        }
        private void InitStartGame()
        {
            UIManager.Instance.ShowUI(UIName.MainMenu);
            Hide();
        }
    }
}
