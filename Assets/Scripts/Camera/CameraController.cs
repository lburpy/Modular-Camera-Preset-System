using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;

public class CameraController : MonoBehaviour
{
    [Header("Preset Ayarları")]
    [Tooltip("Kameranın kullanacağı tüm presetler")]
    public List<CameraPresetData> presets;

    [Tooltip("Oyuna başlanacak preset indexi")]
    public int startPresetIndex = 0;

    [Header("Geçiş Ayarları")]
    public float moveDuration = 0.3f;       // Kamera geçiş süresi
    public AnimationCurve moveCurve;        // Easing curve

    private int currentIndex;               // Aktif preset index
    private bool isMoving;                  // Kamera hareket halinde mi?
    private Camera cam;                     // Main Camera
    private CameraInputActions controls;    // Input system
    private bool isInitialized;

    private void Awake()
    {
        cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError($"{nameof(CameraController)}: Main Camera bulunamadı. Bileşen devre dışı bırakılıyor.");
            enabled = false;
            return;
        }

        if (presets == null || presets.Count == 0)
        {
            Debug.LogError($"{nameof(CameraController)}: En az bir kamera preseti gerekli.");
            enabled = false;
            return;
        }

        if (startPresetIndex < 0 || startPresetIndex >= presets.Count)
        {
            Debug.LogError($"{nameof(CameraController)}: Başlangıç preset indexi ({startPresetIndex}) geçersiz. 0'a ayarlanıyor.");
            startPresetIndex = 0;
        }

        controls = new CameraInputActions();
        controls.Camera.Move.performed += OnMove;

        // Başlangıç preseti ayarlanır
        currentIndex = startPresetIndex;
        ApplyPreset(currentIndex);
        isInitialized = true;
    }

    private void OnEnable()
    {
        controls?.Enable();
    }

    private void OnDisable()
    {
        controls?.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (!isInitialized)
            return;

        // Kamera inputu ve hareketi kontrol ediliyor
        if (!ctx.performed || isMoving) return;

        Vector2 input = ctx.ReadValue<Vector2>();
        CameraPresetData current = presets[currentIndex];

        if (MathF.Abs(input.x) > MathF.Abs(input.y))
        {
            if (input.x > 0) TryMove(current.right);
            else TryMove(current.left);
        }
        else
        {
            if (input.y > 0) TryMove(current.up);
            else TryMove(current.down);
        }
    }

    // Hedef presete geçişi kontrol eder
    private void TryMove(int targetIndex)
    {
        // -1 ya da geçersiz indexi sınır olarak alır
        if (targetIndex < 0 || targetIndex >= presets.Count)
            return;

        StopAllCoroutines();
        StartCoroutine(MoveCamera(targetIndex));
    }

    private void ApplyPreset(int index)
    {
        if (index < 0 || index >= presets.Count)
        {
            Debug.LogError($"{nameof(CameraController)}: Preset indexi ({index}) geçersiz.");
            return;
        }

        cam.transform.position = presets[index].position;
        cam.transform.rotation = Quaternion.Euler(presets[index].rotation);
    }

    // Kamerayı burada taşıyoruz
    private IEnumerator MoveCamera(int targetIndex)
    {
        isMoving = true;

        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        Vector3 endPos = presets[targetIndex].position;
        Quaternion endRot = Quaternion.Euler(presets[targetIndex].rotation);

        float t = 0f;

        while (t < 1f)
        {
            t = Mathf.Clamp01(t + Time.deltaTime / moveDuration);
            float eased = moveCurve.Evaluate(t);

            cam.transform.position = Vector3.Lerp(startPos, endPos, eased);
            cam.transform.rotation = Quaternion.Slerp(startRot, endRot, eased);

            yield return null;
        }

        cam.transform.position = endPos;
        cam.transform.rotation = endRot;

        currentIndex = targetIndex;
        isMoving = false;
    }
}
