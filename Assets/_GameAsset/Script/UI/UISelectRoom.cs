using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISelectRoom : UIBase
{
    public ItemSelectRoom itemSelectRoomPf;
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        //MasterHelper.InitListObj<ItemSelectRoom>()
    }
}
