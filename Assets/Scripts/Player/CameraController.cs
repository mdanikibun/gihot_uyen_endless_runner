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

    void Awake() {
        cinemachineCamera = GetComponent<CinemachineCamera>();
    }

    public void ChangeCameraFOV(float speedAmount)
    {
        StopAllCoroutines();
        StartCoroutine(ChangeFOVRoutine(speedAmount));

        if (speedAmount > 0) {
            speedUpParticle.Play();
        } else {
            speedUpParticle.Stop();
        }
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
