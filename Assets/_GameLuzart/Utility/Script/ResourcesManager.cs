using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager : Singleton<ResourcesManager>
{
    public SpriteResourcesSO spriteResourcesSO;
    public AvatarResourcesSO avatarResourcesSO;
    public IconResourceSO iconResourcesSO;

    public Sprite GetIconByString(string iconName)
    {
        Sprite sprite = iconResourcesSO.GetIconByString(iconName);
        return sprite;
    }
    public Sprite GetAvatar(string str)
    {
        int idAvt = int.Parse(str);
        return avatarResourcesSO.GetSpriteAvatar(idAvt);
    }
}

