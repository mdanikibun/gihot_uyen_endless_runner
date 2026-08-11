using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ParticleSystem speedUpParticle;
    
    [Header("Settings")]
    [SerializeField] float minFOV = 35f;
    [SerializeField] float maxFOV = 85f;
    [SerializeField] float zoomeDuration = 1f;
    [SerializeField] float zoomSpeed = 5f;
    
    CinemachineCamera cinemachineCamera;
    float defaultFOV;

    void Awake() {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        defaultFOV = cinemachineCamera.Lens.FieldOfView;
    }

    public void ChangeCameraFOV(float speedAmount, float currentSpeed, float speedDefault)
    {
        StopAllCoroutines();
        StartCoroutine(ChangeFOVRoutine(speedAmount));

        if (currentSpeed > speedDefault) {
            speedUpParticle.Play();
        } else {
            speedUpParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void ResetToDefault() {
        StopAllCoroutines();
        cinemachineCamera.Lens.FieldOfView = defaultFOV;
        speedUpParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    IEnumerator ChangeFOVRoutine(float speedAmount) {
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float targetFOV = Mathf.Clamp(startFOV + speedAmount * zoomSpeed, minFOV, maxFOV);
        float timeElapsed = 0f;

        while (timeElapsed < zoomeDuration) {
            float t = timeElapsed / zoomeDuration;
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        cinemachineCamera.Lens.FieldOfView = targetFOV;
    }
}
