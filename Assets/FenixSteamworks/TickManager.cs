using System;
using UnityEngine;
using UnityEngine.Events;

namespace FenixSteamworks
{
    public class TickManager : MonoBehaviour
    {
        public static TickManager Singleton { get; private set; }
        public ulong Tick { get; private set; } = 0;
        public float tickTimer { get; private set; }
        [SerializeField] [Min(0.01f)]
        private float tickTimerMax = 0.2f;

        public UnityEvent<ulong> onTick;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else if (Singleton != null && Singleton != this)
            {
                Destroy(this);
                Debug.LogWarning("TickManager instance already exists.");
            }
        }

        private void Update()
        {
            tickTimer += Time.deltaTime;
            while (tickTimer >= tickTimerMax)
            {
                tickTimer -= tickTimerMax;
                Tick++;
                onTick?.Invoke(Tick);
            }
        }
    }
}