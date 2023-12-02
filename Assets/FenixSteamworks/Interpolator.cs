using System.Collections.Generic;
using UnityEngine;

namespace FenixSteamworks
{
    public class Interpolator : MonoBehaviour
    {
        [SerializeField] private float timeElapsed = 0f;
        [SerializeField] private float timeToReachTarget = 0.05f;
        [SerializeField] private float movementThreshold = 0.05f;

        private readonly List<TransformUpdate> futureTransformUpdates = new List<TransformUpdate>();
        private float squareMovementThreshold;
        private TransformUpdate to;
        private TransformUpdate from;
        private TransformUpdate previous;

        private void Start()
        {
            squareMovementThreshold = movementThreshold * movementThreshold;
            to = new TransformUpdate(NetworkManager.Instance.ServerTick, transform.position, transform.rotation);
            from = new TransformUpdate(NetworkManager.Instance.InterpolationTick, transform.position, transform.rotation);
            previous = new TransformUpdate(NetworkManager.Instance.InterpolationTick, transform.position, transform.rotation);
        }

        private void Update()
        {
            for (int i = 0; i < futureTransformUpdates.Count; i++)
            {
                if (NetworkManager.Instance.ServerTick >= futureTransformUpdates[i].Tick)
                {
                    previous = to;
                    to = futureTransformUpdates[i];
                    from = new TransformUpdate(NetworkManager.Instance.InterpolationTick, transform.position,
                        transform.rotation);
                    
                    futureTransformUpdates.RemoveAt(i);
                    i--;
                    timeElapsed = 0f;
                    timeToReachTarget = (to.Tick - from.Tick) * Time.fixedDeltaTime;
                }
            }

            timeElapsed += Time.deltaTime;
            InterpolateTransforms(timeElapsed / timeToReachTarget);
        }

        private void InterpolateTransforms(float lerpAmount)
        {
            if ((to.Position - previous.Position).sqrMagnitude < squareMovementThreshold)
            {
                if (to.Position != from.Position)
                {
                    if (to.Position != from.Position)
                        transform.position = Vector3.Lerp(from.Position, to.Position, lerpAmount);
                }
                else
                {
                    transform.position = Vector3.LerpUnclamped(from.Position, to.Position, lerpAmount);
                }
            }

            transform.rotation = to.Rotation;
        }

        public void NewUpdate(ushort tick, Vector3 position, Quaternion rotation)
        {
            if (tick <= NetworkManager.Instance.InterpolationTick) return;

            for (int i = 0; i < futureTransformUpdates.Count; i++)
            {
                if (tick < futureTransformUpdates[i].Tick)
                {
                    futureTransformUpdates.Insert(i, new TransformUpdate(tick, position, rotation));
                    return;
                }
            }
            
            futureTransformUpdates.Add(new TransformUpdate(tick, position, rotation));
        }
    }
}