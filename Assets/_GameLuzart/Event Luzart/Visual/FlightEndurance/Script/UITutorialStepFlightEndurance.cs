namespace Luzart
{
    using System;
    
    public class UITutorialStepFlightEndurance : UITutorial
    {
        public void InitTutorial(Action onDone)
        {
            //var uiMainMenu = UIManager.Instance.GetUiActive<UIMainMenu>(UIName.MainMenu);
            //var uiMainMenu_Home = uiMainMenu.GetUIScreenMainMenu(EMainMenu.Home) as MainMenu_Home;
            //if (uiMainMenu_Home == null)
            //{
            //    Hide();
            //    return;
            //}
            //ShowScreenTutorial(0, uiMainMenu_Home.btnFlightEndurance.gameObject, OnClickFlightEndurance);
    
            //void OnClickFlightEndurance()
            //{
            //    var ui = UIManager.Instance.ShowUI<UIFlightEnduranceNoti>(UIName.FlightEnduranceNoti);
    
            //    GameUtil.Instance.WaitAndDo(0.1f, () => ShowScreenTutorial(1, ui.btnLetGo.gameObject, OnClickLetGo));
            //}
            //void OnClickLetGo()
            //{
            //    var ui = UIManager.Instance.GetUiActive<UIFlightEnduranceNoti>(UIName.FlightEnduranceNoti);
            //    ui.ClickLetGo();
            //    Hide();
            //    DataManager.Instance.GameData.isTutorialFlightEndurance = true;
            //    DataManager.Instance.SaveGameData();
            //    var uiFlight = UIManager.Instance.GetUiActive<UIFlightEndurance>(UIName.FlightEndurance);
            //    uiFlight.InitActionOnHideDone(onDone);
            //}
    
        }
    }
}
