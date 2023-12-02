using UnityEngine;

namespace FenixSteamworks
{
    public class TransformUpdate
    {
        public ushort Tick { get; private set; }
        public Vector3 Position;
        public Quaternion Rotation;

        public TransformUpdate(ushort tick, Vector3 position, Quaternion rotation)
        {
            Tick = tick;
            Position = position;
            Rotation = rotation;
        }
    }
}