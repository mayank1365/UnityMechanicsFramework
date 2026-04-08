using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    public class ScreenShakeSystem_UMFOSS : MonoBehaviour
    {
        public static ScreenShakeSystem_UMFOSS Instance;

        [Header("Shake Settings")]
        [SerializeField] private float ShakeDecay = 1.3f;
        [SerializeField] private float TraumaMultiplier = 16f;
        [SerializeField] private float PositionMagnitude = 0.8f;
        [SerializeField] private float RotationMagnitude = 10f;

        private Camera cam;
        private Vector3 originalPosition;
        private Quaternion originalRotation;

        private float trauma = 0f;
        private float traumaTimeRemaining = 0f;

        private float timeCounter;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            cam = Camera.main;

            if (cam == null)
            {
                Debug.LogError("Camera.main NOT found!");
                return;
            }

            originalPosition = cam.transform.localPosition;
            originalRotation = cam.transform.localRotation;
        }

        public void TriggerShake(float magnitude, float duration)
        {
            trauma = Mathf.Clamp01(trauma + magnitude);
            traumaTimeRemaining = duration;
        }

        float GetFloat(float seed)
        {
            return (Mathf.PerlinNoise(seed, timeCounter) - 0.5f) * 2f;
        }

        Vector3 GetVec3()
        {
            return new Vector3(
                GetFloat(1),
                GetFloat(10),
                0f
            );
        }

        void Update()
        {
            if (traumaTimeRemaining > 0f && trauma > 0f)
            {
                traumaTimeRemaining -= Time.deltaTime;

                // smoother time progression
                timeCounter += Time.deltaTime * Mathf.Pow(trauma, 0.5f) * TraumaMultiplier * 0.3f;

                float smoothTrauma = trauma * trauma;
                Vector3 offset = GetVec3() * PositionMagnitude * smoothTrauma;

                cam.transform.localPosition = originalPosition + offset;
                cam.transform.localRotation = Quaternion.Euler(offset * RotationMagnitude);

                // smoother decay
                trauma -= Time.deltaTime * ShakeDecay * (trauma + 0.3f);
                trauma = Mathf.Clamp01(trauma);
            }
            else
            {
                // smooth return
                cam.transform.localPosition =
                    Vector3.Lerp(cam.transform.localPosition, originalPosition, Time.deltaTime * 5f);

                cam.transform.localRotation =
                    Quaternion.Lerp(cam.transform.localRotation, originalRotation, Time.deltaTime * 5f);

                trauma = 0f;
            }
        }
    }
}