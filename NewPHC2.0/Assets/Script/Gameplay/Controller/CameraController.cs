using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    private class CameraShakeData
    {
        public float startShakeTime { get; set; }
        public float shakeDuration { get; set; }
        public float shakeTime { get; set; }
        public float shakeMagnitude { get; set; }
    }

    public static CameraController Instance;

    public float CameraSize { get => _cameraSize; }
    [SerializeField] private float _cameraSize = 7;

    [SerializeField] private CinemachineVirtualCamera cinemachine;
    [SerializeField] private CinemachineCameraOffset cameraOffset;
    [SerializeField] private Volume volume;

    private Vignette vignette;
    [SerializeField, Range(0, 1)] private float vignetteOnHurt;

    private ChromaticAberration chromaticAberration;

    private List<CameraShakeData> cameraShakeDatas = new List<CameraShakeData>();
    private float aberration = 0;
    private bool canChangeVI = true;
    private bool canResetCameraSize = true;

    private Coroutine lerpCameraSizeCoroutine;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);

        volume.profile.TryGet(out chromaticAberration);
        volume.profile.TryGet(out vignette);

        if (cinemachine != null)
            cinemachine.m_Lens.OrthographicSize = _cameraSize;

        Instance = this;
    }

    private void Update()
    {
        cameraShakeDatas.RemoveAll(c => Time.time > c.startShakeTime + c.shakeTime);

        chromaticAberration.intensity.value = Mathf.Min(aberration * 1.35f, 1);
        aberration -= Time.deltaTime * 2;

        if (canResetCameraSize)
            cinemachine.m_Lens.OrthographicSize = Mathf.Lerp(cinemachine.m_Lens.OrthographicSize, _cameraSize, Time.deltaTime * 4);
    }

    public void Hurt() => StartCoroutine(ChangeVignetteIntensityIE(0f, vignetteOnHurt, 0.05f, vignetteOnHurt, 0f, 0.1f, new Color32(255, 0, 0, 255)));

    public void ChangeVignetteIntensity(float start1, float end1, float duration1, float start2, float end2, float duration2, Color32 color) =>
        StartCoroutine(ChangeVignetteIntensityIE(start1, end1, duration1, start2, end2, duration2, color));

    private IEnumerator ChangeVignetteIntensityIE(float start1, float end1, float duration1, float start2, float end2, float duration2, Color32 color)
    {
        if (!canChangeVI) yield break;

        canChangeVI = false;

        yield return LerpVignetteIntensityIE(start1, end1, duration1, color);

        yield return new WaitForSeconds(0.1f);

        yield return LerpVignetteIntensityIE(start2, end2, duration2, color);

        canChangeVI = true;
    }

    public void LerpVignetteIntensity(float startValue, float endValue, float duration, Color32 color) =>
        StartCoroutine(LerpVignetteIntensityIE(startValue, endValue, duration, color));

    private IEnumerator LerpVignetteIntensityIE(float startValue, float endValue, float duration, Color32 color)
    {
        float timeElapsed = 0f;

        vignette.color.value = color;

        while (timeElapsed < duration)
        {
            vignette.intensity.value = Mathf.Lerp(startValue, endValue, timeElapsed / duration);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        vignette.intensity.value = endValue;
    }

    public void StopLerpCameraSize()
    {
        if (lerpCameraSizeCoroutine != null)
        {
            StopCoroutine(lerpCameraSizeCoroutine);
            lerpCameraSizeCoroutine = null;
        }

        canResetCameraSize = true;
    }

    public void LerpCameraSize(float newCameraSize, float endTime)
    {
        if (lerpCameraSizeCoroutine != null)
            StopCoroutine(lerpCameraSizeCoroutine);

        lerpCameraSizeCoroutine = StartCoroutine(LerpCameraSizeIE(newCameraSize, endTime));
    }

    private IEnumerator LerpCameraSizeIE(float newCameraSize, float endTime)
    {
        canResetCameraSize = false;

        float startTime = 0;

        while (true)
        {
            if (startTime >= endTime)
                break;

            cinemachine.m_Lens.OrthographicSize = Mathf.Lerp(cinemachine.m_Lens.OrthographicSize, newCameraSize, startTime / endTime);

            startTime += Time.deltaTime;

            yield return null;
        }

        canResetCameraSize = true;
    }

    public void TriggerShake(float shakeDuration, float shakeTime, float shakeMagnitude) =>
        StartCoroutine(TriggerShakeIE(shakeDuration, shakeTime, shakeMagnitude));

    private IEnumerator TriggerShakeIE(float shakeDuration, float shakeTime, float shakeMagnitude)
    {
        cameraShakeDatas.Add(new CameraShakeData
        {
            startShakeTime = Time.time,
            shakeDuration = shakeDuration,
            shakeTime = shakeTime,
            shakeMagnitude = shakeMagnitude
        });

        if (cameraShakeDatas.Count > 1) yield break;

        Vector3 firstPos = cameraOffset.m_Offset;
        Quaternion firstRot = cinemachine.transform.rotation; // Store initial rotation
        float maxRotation = Mathf.Lerp(0, 10, shakeMagnitude / 1.25f);

        CameraShakeData currentCameraShakeData = cameraShakeDatas.FirstOrDefault();

        aberration = currentCameraShakeData.shakeMagnitude;
        cinemachine.m_Lens.OrthographicSize = _cameraSize - 0.2f;

        while (cameraShakeDatas.Count > 0)
        {
            cameraShakeDatas.Sort((c1, c2) => c1.shakeMagnitude.CompareTo(c2.shakeMagnitude));

            var newCameraShakeData = cameraShakeDatas.FirstOrDefault();

            if (newCameraShakeData != currentCameraShakeData && newCameraShakeData != null)
            {
                currentCameraShakeData = newCameraShakeData;
                aberration = currentCameraShakeData.shakeMagnitude;
                cinemachine.m_Lens.OrthographicSize = _cameraSize - 0.2f;
            }

            float startShake = 0;
            Vector3 direction = Random.onUnitSphere * currentCameraShakeData.shakeMagnitude;
            Vector3 startPos = cameraOffset.m_Offset;
            direction.z = startPos.z;

            float randomRotationZ = Random.Range(-maxRotation, maxRotation); // Random rotation

            while (true)
            {
                yield return new WaitForEndOfFrame();
                startShake += Time.deltaTime;

                cameraOffset.m_Offset = Vector3.Lerp(startPos, direction, startShake / currentCameraShakeData.shakeDuration);

                // Apply rotation
                float rotationZ = Mathf.Lerp(0, randomRotationZ, startShake / currentCameraShakeData.shakeDuration);
                cinemachine.transform.rotation = Quaternion.Euler(0, 0, rotationZ);

                if (startShake > currentCameraShakeData.shakeDuration) break;
            }
        }

        if (currentCameraShakeData != null)
        {
            float endShake = 0;
            Vector3 endPos = cameraOffset.m_Offset;

            while (true)
            {
                yield return new WaitForEndOfFrame();
                endShake += Time.deltaTime;

                cameraOffset.m_Offset = Vector3.Lerp(endPos, firstPos, endShake / currentCameraShakeData.shakeDuration);

                // Reset rotation
                cinemachine.transform.rotation = Quaternion.Lerp(cinemachine.transform.rotation, firstRot, endShake / currentCameraShakeData.shakeDuration);

                if (endShake > currentCameraShakeData.shakeDuration) break;
            }
        }
        else
        {
            cameraOffset.m_Offset = firstPos;
            cinemachine.transform.rotation = firstRot; // Reset rotation
        }
    }

}