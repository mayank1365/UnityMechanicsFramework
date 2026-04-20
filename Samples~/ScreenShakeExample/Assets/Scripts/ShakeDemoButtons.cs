using UnityEngine;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.ScreenShake
{
    public class ShakeDemoButtons : MonoBehaviour
    {
        [SerializeField] public float magnitude;
        [SerializeField] public float duration;

        public void Trigger()
        {
            ScreenShakeSystem_UMFOSS.Instance.TriggerShake(magnitude, duration);
        }
    }
}