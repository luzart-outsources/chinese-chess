namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class UITutorialStepBattlePass : UITutorial
    {
        public void InitTutorial(Action onDone)
        {
            //var uiMainMenu = UIManager.Instance.GetUiActive<UIMainMenu>(UIName.MainMenu);
            //var uiMainMenu_Home = uiMainMenu.GetUIScreenMainMenu(EMainMenu.Home) as MainMenu_Home;
            //if(uiMainMenu_Home == null)
            //{
            //    Hide();
            //    return;
            //}
            //ShowScreenTutorial(0, uiMainMenu_Home.btnBattlePass.gameObject, OnDoneClickTutorial);
    
            //void OnDoneClickTutorial()
            //{
            //    var uiBattlePass = UIManager.Instance.ShowUI<UIBattlePass>(UIName.BattlePass, onDone);
            //    uiBattlePass.scrollRect.ClaimedRectTransformScrollView(uiBattlePass.listItem[0].rectTransform);
            //    GameUtil.Instance.WaitAndDo(0.2f, () => ShowScreenTutorial(1, uiBattlePass.listItem[0].itemResFree.gameObject, OnDoneClickTutorialResItem));
            //}
            //void OnDoneClickTutorialResItem()
            //{
            //    var uiBattlePass = UIManager.Instance.GetUiActive<UIBattlePass>(UIName.BattlePass);
            //    ShowScreenTutorial(2, uiBattlePass.obEnergyBar, OnDoneClickTutorialEnergyBar);
            //}
            //void OnDoneClickTutorialEnergyBar()
            //{
            //    Hide();
            //    UIManager.Instance.ShowUI(UIName.BattlePassTutorial, () =>
            //    {
            //        DataManager.Instance.GameData.isTutorialBattlePass = true;
            //        DataManager.Instance.SaveGameData();
            //    });
            //}
        }
    }
    [System.Serializable]
    public enum EStateTurBP
    {
        MainMenu  = 0,
        InTuto =1,
    }
}
