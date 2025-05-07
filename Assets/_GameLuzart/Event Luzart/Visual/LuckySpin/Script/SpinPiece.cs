using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinPiece : MonoBehaviour
{
    public ResUI resUI;
    public int index;
    public BaseSelect bsFX;
    public DataResource dataRes;
    public void InitializePiece(int index , DataResource dataRes)
    {
        this.index = index;
        this.dataRes = dataRes;

        resUI.InitData(dataRes);
    }
}
