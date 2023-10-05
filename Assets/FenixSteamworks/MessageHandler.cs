using System;
using UnityEngine;
using UnityEngine.Events;

namespace FenixSteamworks
{
    [Serializable]
    public struct KeyActionPair
    {
        public string key;
        public UnityEvent<string> action;
    }
    public class MessageHandler : MonoBehaviour
    {
        public KeyActionPair[] KeyActionPairs;
    }
}