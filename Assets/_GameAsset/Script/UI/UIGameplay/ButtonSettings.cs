using UnityEngine;

public class ButtonSettings : MonoBehaviour
{
    public void OnClickButtonSettings()
    {
        UIManager.Instance.ShowUI(UIName.Settings);
    }
}
