using System.Collections;
using System.Collections.Generic;
using Assets._GameAsset.Script.Session;
using UnityEngine;

public class UILeaveRoom : UIBase
{
    public void OnClickLeaveRoom()
    {
        GlobalServices.Instance.RequestLeaveRoom();
    }
    public void OnClickHoa()
    {

    }
    public void OnClick()
    {

    }
}
