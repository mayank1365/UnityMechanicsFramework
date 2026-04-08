using UnityEngine;
using GameplayMechanicsUMFOSS.Systems;

public class ShakeButton : MonoBehaviour
{
    [SerializeField] public float magnitude ;
    [SerializeField] public float duration ;

    public void Trigger()
    {
        ScreenShakeSystem_UMFOSS.Instance.TriggerShake(magnitude, duration);
    }
}