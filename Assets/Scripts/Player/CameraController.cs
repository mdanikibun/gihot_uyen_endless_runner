using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ParticleSystem speedUpParticle;
    [SerializeField] GameSettings settings;
    
    CinemachineCamera cinemachineCamera;
    float defaultFOV;
    Coroutine fovCoroutine;

    void Awake() {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        defaultFOV = cinemachineCamera.Lens.FieldOfView;
    }

    public void ChangeCameraFOV(float speedAmount, float currentSpeed, float speedDefault) {
        float targetFOV = Mathf.Clamp(
            defaultFOV + speedAmount * settings.camera.zoomSpeed,
            settings.camera.minFOV,
            settings.camera.maxFOV
        );

        SmoothToFOV(targetFOV);

        if (currentSpeed > speedDefault) {
            speedUpParticle.Play();
        } else {
            speedUpParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public void ResetToDefault() {
        SmoothToFOV(defaultFOV);
        speedUpParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void SmoothToFOV(float targetFOV) {
        if (fovCoroutine != null) {
            StopCoroutine(fovCoroutine);
        }
        fovCoroutine = StartCoroutine(ChangeFOVRoutine(targetFOV));
    }

    IEnumerator ChangeFOVRoutine(float targetFOV) {
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float duration = Mathf.Max(0.01f, settings.camera.zoomDuration);
        float timeElapsed = 0f;

        while (timeElapsed < duration) {
            float t = timeElapsed / duration;
            float smoothT = t * t * (3f - 2f * t); // SmoothStep
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, smoothT);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        cinemachineCamera.Lens.FieldOfView = targetFOV;
        fovCoroutine = null;
    }
}
