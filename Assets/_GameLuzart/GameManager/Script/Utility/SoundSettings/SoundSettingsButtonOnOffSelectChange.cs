using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsButtonOnOffSelectChange : SoundSettings
{
    public Button btnSFX, btnMusic, btnHapstic;

    public BaseSelect bsSFX, bsMusic, bsHapstic;

    protected override void Setup()
    {
        base.Setup();
        SetUpButton();
    }
    public override void Show()
    {
        base.Show();
        bsMusic.Select(AudioManager.Instance.isMusic);
        bsSFX.Select(AudioManager.Instance.isSFX);
        bsHapstic.Select(AudioManager.Instance.isVibra);
    }
    private void SetUpButton()
    {
        GameUtil.ButtonOnClick(btnMusic, ClickMusic, false);
        GameUtil.ButtonOnClick(btnSFX, ClickSFX, false);
        GameUtil.ButtonOnClick(btnHapstic, ClickVibrate, false);
    }
    private void ClickMusic()
    {
        bool isOn = AudioManager.Instance.isMusic;
        isOn = !isOn;
        AudioManager.Instance.isMusic = isOn;
        bsMusic.Select(isOn);
        if (isOn)
        {
            //Observer.Instance.Notify();
        }
    }
    private void ClickSFX()
    {
        bool isOn = AudioManager.Instance.isSFX;
        isOn = !isOn;
        AudioManager.Instance.isSFX = isOn;
        bsSFX.Select(isOn);
    }
    private void ClickVibrate()
    {
        bool isOn = AudioManager.Instance.isVibra;
        isOn = !isOn;
        AudioManager.Instance.isVibra = isOn;
        bsHapstic.Select(isOn);
    }
}
