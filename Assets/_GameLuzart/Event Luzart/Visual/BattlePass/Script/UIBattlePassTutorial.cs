namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class UIBattlePassTutorial : UIBase
    {
        protected override void Setup()
        {
            isAnimBtnClose = true;
            base.Setup();
        }
        public override void OnClickClose()
        {
            base.OnClickClose();
        }
    }
}
