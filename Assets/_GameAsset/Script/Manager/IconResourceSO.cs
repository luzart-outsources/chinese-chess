using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/IconResourceSO", fileName = "IconResourceSO")]
public class IconResourceSO : ScriptableObject
{
    public List<Icon> listIcon = new List<Icon>();
    public Sprite GetIconByString(string iconName)
    {
        foreach (var icon in listIcon)
        {
            if (icon.iconName == iconName)
            {
                return icon.iconSprite;
            }
        }
        return null;
    }
}
[System.Serializable]
public class Icon
{
    public string iconName;
    public Sprite iconSprite;
}