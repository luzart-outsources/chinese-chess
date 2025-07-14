using UnityEngine;
using UnityEngine.UI;

public class AutoSetCanvas : MonoBehaviour
{
    public CanvasScaler _canvasScaler;
    public CanvasScaler canvasScaler
    {
        get
        {
            if( _canvasScaler == null)
            {
                _canvasScaler = GetComponent<CanvasScaler>();
            }
            return _canvasScaler;
        }
    }
    private void Awake()
    {
        float width = Screen.width;
        float height = Screen.height;
        float delta = height/width;
        if(delta >= 2.33)
        {
            float a = -1.43f;
            float b = 4.333f;
            float x = a * delta + b;
            canvasScaler.matchWidthOrHeight = x;
        }
    }
}
