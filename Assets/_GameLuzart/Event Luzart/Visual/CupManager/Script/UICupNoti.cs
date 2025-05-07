namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UICupNoti : UIBase
    {
        public Button btnPlay;
    
        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnPlay, ClickPlay, true);
        }
        private void ClickPlay()
        {
            Debug.LogError("Play Game");
        }
    }
}
