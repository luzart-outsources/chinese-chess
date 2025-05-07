namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UIFlightEnduranceReward : UIBase
    {
        public Image imMe, imPf;
        private List<Image> listLocal = new List<Image>();
        public Transform parentTf;
        public ResUI resUI;
        public FlightEnduranceManager flightEnduranceManager
        {
            get
            {
                return EventManager.Instance.flightEnduranceManager;
            }
        }
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            var list = flightEnduranceManager.dataFlightEndurance.listTotalPeople;
            var dataMe = flightEnduranceManager.dataFlightEndurance.listVisual[flightEnduranceManager.dataFlightEndurance.idMe];
            list.Remove(dataMe);
            int count = flightEnduranceManager.dataFlightEndurance.countPlayerEnd-1;
            MasterHelper.InitListObj(count, imPf, listLocal, parentTf, (item,index) =>
            {
                item.gameObject.SetActive(true);
                item.sprite = DataWrapperGame.AllSpriteAvatars[list[index].idAvt];
            });
            int gold = flightEnduranceManager.CountReward/(count+1);
            resUI.InitData(new DataResource(new DataTypeResource(RES_type.Gold), gold));
            int id = dataMe.idAvt;
            imMe.sprite = DataWrapperGame.AllSpriteAvatars[id];
        }
        public override void Hide()
        {
            base.Hide();
        }
    }
}
