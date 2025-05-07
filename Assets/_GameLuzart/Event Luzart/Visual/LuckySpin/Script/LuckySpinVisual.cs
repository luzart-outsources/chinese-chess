using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using DG.Tweening;

public class LuckySpinVisual : MonoBehaviour
{
    [Header("Cấu hình vòng quay")]
    public Transform wheel;
    public Transform pointer; // Không dùng Rigidbody2D
    public int spinsCount = 5;
    public float spinDuration = 5f;
    public bool isSpinning = false;
    public List<DB_SpinWheel> listDBSpin = new List<DB_SpinWheel>();
    private float[] sectorMidAngles;
    private float currentAngle = 0f;
    private Coroutine pointerShakeRoutine;
    private float lastWheelAngle;
    private float wheelSpeed;
    public int forceIndex = -1;

    public Action actionStartSpin = null;
    public Action<int> actionOnDoneSpin = null;

    public void Initialize(List<DB_SpinWheel> listSpinWheels)
    {
        this.listDBSpin = listSpinWheels;
        int count = listDBSpin.Count;
        sectorMidAngles = new float[count];
        float sectorAngle = 360f / count;
        for (int i = 0; i < count; i++)
        {
            sectorMidAngles[i] = sectorAngle * (i + 0.5f);
        }
    }
    public int GetForcedSectorIndex()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        float cumulative = 0f;

        for (int i = 0; i < listDBSpin.Count; i++)
        {
            cumulative += listDBSpin[i].chance;
            if (randomValue < cumulative)
            {
                return i;
            }
        }
        return listDBSpin.Count - 1;
    }

    public void StartSpin(int forcedSectorIndex = -1, Action actionStartSpin = null, Action<int> actionOnDoneSpin = null)
    {
        if (isSpinning)
        {
            Debug.LogWarning("Đang quay, không thể bắt đầu quay mới.");
            return;
        }
        if (forcedSectorIndex == -1)
        {
            forcedSectorIndex = GetForcedSectorIndex();
        }
        if(forceIndex != -1)
        {
            forcedSectorIndex = forceIndex;
        }
        if (forcedSectorIndex < 0 || forcedSectorIndex >= listDBSpin.Count)
        {
            Debug.LogError("Chỉ số sector không hợp lệ!");
            return;
        }
        this.actionStartSpin = actionStartSpin;
        this.actionOnDoneSpin = actionOnDoneSpin;
        tw?.Kill(true);
        tw = SpinTween(forcedSectorIndex);
    }
    private Tween tw;
    private Sequence seq;
    private Tween SpinTween(int forcedSectorIndex)
    {
        isSpinning = true;
        actionStartSpin?.Invoke();

        float startAngle = currentAngle;
        float targetAngle = sectorMidAngles[forcedSectorIndex];

        float startAngleNormalized = Mathf.Repeat(startAngle, 360f);
        float deltaAngle = targetAngle - startAngleNormalized;
        if (deltaAngle < 0) deltaAngle += 360f;
        deltaAngle += spinsCount * 360f;
        float endAngle = startAngle + deltaAngle;
        seq?.Kill(true);
        seq = DOTween.Sequence();

        // 🔥 BƯỚC 1: Quay ngược một chút rồi mới quay xuôi
        seq.Append(wheel.DORotate(new Vector3(0, 0, startAngle - 45f), 0.2f, RotateMode.FastBeyond360)
                   .SetEase(Ease.OutQuad));

        // 🔥 BƯỚC 2: Quay xuôi với easing chuẩn + cập nhật pointer nhẹ
        seq.Append(wheel.DORotate(new Vector3(0, 0, endAngle), spinDuration, RotateMode.FastBeyond360)
                   .SetEase(Ease.OutQuart)
                   .OnUpdate(() =>
                   {
                       // 🔥 Nếu muốn rung nhẹ theo tốc độ, có thể chỉnh `pointer`
                       float speedFactor = Mathf.Clamp(wheelSpeed * 0.02f, 0, 5f);
                       pointer.localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(-speedFactor, speedFactor));
                   }));

        // 🔥 BƯỚC 3: Dừng quay & reset pointer về 0
        seq.OnComplete(() =>
        {
            isSpinning = false;
            currentAngle = endAngle;
            actionOnDoneSpin?.Invoke(forcedSectorIndex);
            pointer.localEulerAngles = Vector3.zero; // 🚀 Reset pointer về 0 luôn
        });

        return seq;
    }


}