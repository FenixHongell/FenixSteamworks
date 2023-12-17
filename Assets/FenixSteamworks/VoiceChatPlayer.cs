using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;

namespace FenixSteamworks
{
    public class VoiceChatPlayer : MonoBehaviour
    {
        public AudioSource audioSource;
        
        public void PlayAudioFromSource(byte[] destBuffer, uint bytesWritten)
        {
            byte[] destBuffer2 = new byte[22050 * 2];
            uint bytesWritten2;
            EVoiceResult ret = SteamUser.DecompressVoice(destBuffer, bytesWritten, destBuffer2, (uint)destBuffer2.Length, out bytesWritten2, 22050);
            if(ret == EVoiceResult.k_EVoiceResultOK && bytesWritten2 > 0)
            {
                audioSource.clip = AudioClip.Create(UnityEngine.Random.Range(100, 1000000).ToString(), 22050, 1, 22050, false);
 
                float[] test = new float[22050];
                for (int i = 0; i < test.Length; i++)
                {
                    test[i] = (short)(destBuffer2[i * 2] | destBuffer2[i * 2 + 1] << 8) / 32768.0f;
                }
                audioSource.clip.SetData(test, 0);
                audioSource.Play();
            }
        }
    }
}