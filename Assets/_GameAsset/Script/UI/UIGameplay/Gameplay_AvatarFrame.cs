using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Gameplay_AvatarFrame : MonoBehaviour
{
    public Image imAvatar;
    public TMP_Text txtName;

    public void SetData(DataPlayerInRoom data)
    {
        bool isHasData = data != null;
        this.gameObject.SetActive(isHasData);
        if (!isHasData)
        {
            return;
        }
        txtName.text = data.name;
        int idAvt = int.Parse(data.avatar);
        imAvatar.sprite = ResourcesManager.Instance.avatarResourcesSO.GetSpriteAvatar(idAvt);   
    }

}
