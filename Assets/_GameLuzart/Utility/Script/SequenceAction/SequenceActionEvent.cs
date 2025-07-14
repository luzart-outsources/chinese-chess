using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SequenceActionEvent : MonoBehaviour
{
    public abstract void PreInit();
    public abstract void Init(Action callback);
}
