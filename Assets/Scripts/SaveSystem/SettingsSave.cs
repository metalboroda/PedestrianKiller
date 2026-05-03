using System;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class SettingsSave
    {
        public bool isMusicOn = true;
        public bool isSfxOn = true;
        public bool isVibrationOn = true;
        public bool tutorialShown;

        public SettingsSave SaveMusic(bool musicOn)
        {
            isMusicOn = musicOn;
            return this;
        }

        public SettingsSave SaveSfx(bool sfxOn)
        {
            isSfxOn = sfxOn;
            return this;
        }

        public SettingsSave SaveVibration(bool vibrationOn)
        {
            isVibrationOn = vibrationOn;
            return this;
        }

        public SettingsSave SaveTutorialShown(bool newTutorialShown)
        {
            tutorialShown = newTutorialShown;
            return this;
        }
    }
}