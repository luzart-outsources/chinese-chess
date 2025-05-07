namespace Luzart
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class IconEvent : MonoBehaviour
    {
        //public IconEventSprite[] iconEvents;
        public EEventName eventName;
        private int value = 0;
        public Image im;
        public TMP_Text txt;

        public void Initialize(EEventName eEvent, int value = 0)
        {
            this.value = value;
            bool isStatus = eventName == eEvent;
            gameObject.SetActive(isStatus);
            if (txt != null)
            {
                GameUtil.SetActiveCheckNull(txt.gameObject, value != 0);
                GameUtil.SetTextCheckNull(txt, value.ToString());
            }

        }
    }
    [System.Serializable]
    public class IconEventSprite
    {
        public EEventName eEvent;
        public Sprite sp;
    }
}

