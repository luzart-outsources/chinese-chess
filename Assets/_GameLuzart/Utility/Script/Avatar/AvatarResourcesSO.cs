using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/AvatarResourcesSO", fileName = "AvatarResourcesSO")]
public class AvatarResourcesSO : ScriptableObject
{
    public Sprite[] sprites;

    public Sprite GetSpriteAvatar(int id)
    {
        id = Mathf.Clamp(id, 0, sprites.Length);
        return sprites[id];
    }
}

[System.Serializable]
public class AvatarSprite
{
    public int id;
    public Sprite sprite;
}
