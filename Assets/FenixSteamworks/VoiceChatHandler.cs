using System;
using FenixSteamworks.Enums;
using Steamworks;
using UnityEngine;

namespace FenixSteamworks
{
    public class VoiceChatHandler : MonoBehaviour
    {
        public static VoiceChatHandler Instance;
        public VoiceChatMode recordMode { get; private set; }
        public bool voiceChatEnabled = true;
        public VoiceChannel currentVoiceChannel;

        private void Awake()
        {
            //Singleton logic
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            if (!voiceChatEnabled) return;

            if (recordMode == VoiceChatMode.OpenMic)
            {
                SteamUser.StartVoiceRecording();
            }
        }

        private void Update()
        {
            uint compressed;
            EVoiceResult availableVoiceResult = SteamUser.GetAvailableVoice(out compressed);
            if (availableVoiceResult == EVoiceResult.k_EVoiceResultOK && compressed > 1024)
            {
                byte[] destBuffer = new byte[1024];
                uint bytesWritten;
                availableVoiceResult = SteamUser.GetVoice(true, destBuffer, 1024, out bytesWritten);
                if (availableVoiceResult == EVoiceResult.k_EVoiceResultOK && bytesWritten > 0)
                {
                    if (currentVoiceChannel == VoiceChannel.Global)
                    {
                        MessageHandler.SendMessageWithKey(MessageKeyType.Voice, Convert.ToBase64String(destBuffer) + ";" + bytesWritten, EP2PSend.k_EP2PSendUnreliableNoDelay,false);
                    }
                }
            }
        }

        public void PushToTalkKeyDown()
        {
            if (!enabled) return;

            SteamUser.StartVoiceRecording();
        }

        public void PushToTalkKeyUp()
        {
            SteamUser.StopVoiceRecording();
        }

        public void SetRecordMode(VoiceChatMode mode)
        {
            //Stop recording if user turned voice chat off.
            if (mode == VoiceChatMode.None) SteamUser.StopVoiceRecording();

            //Start recording if user turned open mic on.
            if (mode == VoiceChatMode.OpenMic && enabled) SteamUser.StartVoiceRecording();

            //Stop recording if user had open mic on and switched to push to talk.
            if (mode == VoiceChatMode.PushToTalk && recordMode == VoiceChatMode.OpenMic) SteamUser.StopVoiceRecording();

            //Set record mode
            recordMode = mode;
        }
    }
}