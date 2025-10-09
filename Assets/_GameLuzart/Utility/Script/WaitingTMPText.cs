using UnityEngine;
using System.Collections;
using TMPro;

public class WaitingText : MonoBehaviour
{
    [SerializeField] private TMP_Text textTarget; // Gán Text UI vào đây trong Inspector
    [SerializeField] private string baseMessage = "Đang chờ người chơi sẵn sàng";
    [SerializeField] private float delay = 0.5f; // Thời gian đổi dấu chấm

    private Coroutine animCoroutine;
    public void InitText(string str)
    {
        this.baseMessage = str;
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        animCoroutine = StartCoroutine(AnimateDots());
    }

    private void OnDisable()
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);
    }

    private IEnumerator AnimateDots()
    {
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount + 1) % 4; // 0,1,2,3
            string dots = new string('.', dotCount);
            textTarget.text = $"{baseMessage}{dots}";
            yield return new WaitForSeconds(delay);
        }
    }
}
