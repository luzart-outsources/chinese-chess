using Assets._GameAsset.Script.Session;
using TMPro;
using UnityEngine;

public class UIPrivateChallenge : UIBase
{
    private int idRoom = -1;
    public void OnChangedValue(string value)
    {
        idRoom = int.Parse(value);
    }
    public void OnClickJoin()
    {
        if (idRoom == -1)
        {
            return;
        }
        GlobalServices.Instance.RequestJoinRoom(idRoom,false);
        Hide();
    }
}
