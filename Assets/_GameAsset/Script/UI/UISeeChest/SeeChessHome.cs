using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeeChessHome : MonoBehaviour
{
    public ItemSeeChess itemSeeChest;
    private void OnEnable()
    {
        if (ieIECountTime != null)
        {
            StopCoroutine(ieIECountTime);
            ieIECountTime = null;
        }
        ieIECountTime = StartCoroutine(IECountTime());
        Observer.Instance.AddObserver(ObserverKey.OnRefreshRoomSee, Refresh);
        RefreshUI();
    }
    private void OnDisable()
    {
        Observer.Instance.RemoveObserver(ObserverKey.OnRefreshRoomSee, Refresh);
        if(ieIECountTime != null)
            StopCoroutine(ieIECountTime);
    }
    private void Refresh(object data)
    {
        RefreshUI();
    }
    private void RefreshUI()
    {
        bool isShow = false;
        var dictRoom = RoomManager.Instance.dictRoomSeeDatas;
        if (dictRoom != null && dictRoom.Count > 0)
        {
            List<DataRoom> listRoom = new List<DataRoom>();
            bool isEChess = Random.Range(0, 2) == 0;
            if (isEChess)
            {
                if (dictRoom.ContainsKey(EChessType.Chess))
                    listRoom.AddRange(dictRoom[EChessType.Chess]);

                if (dictRoom.ContainsKey(EChessType.ChessVisible))
                    listRoom.AddRange(dictRoom[EChessType.ChessVisible]);
            }
            else
            {
                if (dictRoom.ContainsKey(EChessType.ChinaChess))
                    listRoom.AddRange(dictRoom[EChessType.ChinaChess]);
                if (dictRoom.ContainsKey(EChessType.ChinaChessVisible))
                    listRoom.AddRange(dictRoom[EChessType.ChinaChessVisible]);
            }
            if(listRoom.Count > 0)
            {
                isShow = true;
                itemSeeChest.InitData(listRoom[0], (data) => OnClickListSeeChess());
            }

        }
        itemSeeChest.gameObject.SetActive(isShow);


    }
    public void OnClickListSeeChess()
    {
        UIManager.Instance.ShowUI(UIName.SeeChess);
    }
    private Coroutine ieIECountTime = null;
    private IEnumerator IECountTime()
    {
        WaitForSeconds wait = new WaitForSeconds(30);
        while (true)
        {
            yield return wait;
            RoomManager.Instance.RequestSeeRoom();
        }
    }
}
