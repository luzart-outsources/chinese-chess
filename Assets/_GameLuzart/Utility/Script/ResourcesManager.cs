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
}

