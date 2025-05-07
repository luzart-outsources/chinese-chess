namespace Luzart
{
    using DG.Tweening;
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class ItemFlightEnduranceSeat : MonoBehaviour
    {
        public DataSeatFlight dataSeat;
        public BaseSelect bsMe;
        public Image imAvt;
        public GameObject obDu;
        public void Initialize(DataSeatFlight dataSeat, bool isMe)
        {
            this.dataSeat = dataSeat;
            if (dataSeat.id == -1)
            {
                gameObject.SetActive(false);
            }
            imAvt.sprite = DataWrapperGame.AllSpriteAvatars[dataSeat.idAvt];
            bsMe.Select(isMe);
        }
        Sequence sq;
        public void InitAnimation(Transform parentIt, Action onPull = null)
        {
            Vector3 pos = new Vector3(transform.position.x + UnityEngine.Random.Range(-2, 2), transform.position.y + UnityEngine.Random.Range(-2, 0));
            Vector3 posDown = new Vector3(transform.position.x + UnityEngine.Random.Range(-2, 2), transform.position.y + UnityEngine.Random.Range(-25, -15));
            sq = DOTween.Sequence();
            sq.AppendInterval(UnityEngine.Random.Range(0.1f, 0.6f));
            sq.AppendCallback(() =>
            {
                transform.SetParent(parentIt);
                onPull?.Invoke();
            });
            sq.Append(transform.DOMove(pos, 0.5f));
            sq.Join(transform.DOScale(0.5f, 0.5f));
    
            sq.AppendCallback(() =>
            {
                obDu.SetActive(true);
            });
            sq.Append(transform.DOMove(posDown, 2f));
            sq.Join(transform.DOScale(1f, 0.5f));
            sq.SetId(this);
    
        }
        private void OnDisable()
        {
            this.DOKill();
        }
    }
}
