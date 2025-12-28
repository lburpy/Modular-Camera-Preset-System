using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;

public class CameraController : MonoBehaviour
{
    [Header("Preset Ayarlarý")]
    [Tooltip("Kameranýn kullanacaðý tüm presetler")]
    public List<CameraPresetData> presets;

    [Tooltip("Oyuna baþlanacak preset indexi")]
    public int startPresetIndex = 0;

    [Header("Geçiþ Ayarlarý")]
    public float moveDuration = 0.3f;       // Kamera geçiþ süresi
    public AnimationCurve moveCurve;        // Easing curve

    private int currentIndex;               // Aktif preset index
    private bool isMoving;                  // Kamera hareket halinde mi?
    private Camera cam;                     // Main Camera
    private CameraInputActions controls;    // Input system

    private void Awake()
    {
        // Kamera referansý
        cam = Camera.main;

        // Input sistemi oluþturmak için
        controls = new CameraInputActions();
        controls.Camera.Move.performed += OnMove;
        controls.Enable();

        // Baþlangýç preseti ayarlanýr
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

    // Hedef presete geçiþi kontrol eder
    private void TryMove(int targetIndex)
    {
        // -1 ya da geçersiz indexi sýnýr olarak alýr
        if (targetIndex < 0 || targetIndex >= presets.Count)
            return;

        StopAllCoroutines();
        StartCoroutine(MoveCamera(targetIndex));
    }

    // Kamerayý burada taþýyoruz
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
            t += Time.deltaTime / moveDuration;
            float eased = moveCurve.Evaluate(t);

            cam.transform.position = Vector3.Lerp(startPos, endPos, eased);
            cam.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        currentIndex = targetIndex;
        isMoving = false;
    }
}
