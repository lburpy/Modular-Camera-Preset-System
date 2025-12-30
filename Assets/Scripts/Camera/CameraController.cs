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

    private void Awake()
    {
        // Kamera referansı
        cam = Camera.main;

        // Input sistemi oluşturmak için
        controls = new CameraInputActions();
        controls.Camera.Move.performed += OnMove;
        controls.Enable();

        // Başlangıç preseti ayarlanır
        currentIndex = startPresetIndex;

        cam.transform.position = presets[currentIndex].position;
        cam.transform.rotation = Quaternion.Euler(presets[currentIndex].rotation);

    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
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
