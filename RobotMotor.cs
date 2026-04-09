using UnityEngine;
using System.Collections;

public class RobotMotor : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 100f; // ✅ Пикселей в секунду
    public float turnSpeed = 180f; // ✅ Градусов в секунду

    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    // ✅ ИСПОЛЬЗУЕМ параметр distance из команды!
    public IEnumerator MoveForward(float distance)
    {
        Debug.Log($"⬆️ Движение вперёд на {distance} пикселей");

        float elapsed = 0f;
        float duration = distance / moveSpeed;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * distance;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        Debug.Log("✅ Движение вперёд завершено");
    }

    // ✅ ИСПОЛЬЗУЕМ параметр distance из команды!
    public IEnumerator MoveBackward(float distance)
    {
        Debug.Log($"⬇️ Движение назад на {distance} пикселей");

        float elapsed = 0f;
        float duration = distance / moveSpeed;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos - Vector3.up * distance;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        Debug.Log("✅ Движение назад завершено");
    }

    // ✅ ИСПОЛЬЗУЕМ параметр angle из команды!
    public IEnumerator TurnLeft(float angle)
    {
        Debug.Log($"⬅️ Поворот влево на {angle} градусов");

        float elapsed = 0f;
        float duration = angle / turnSpeed;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 0, angle);

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;
        Debug.Log("✅ Поворот влево завершён");
    }

    // ✅ ИСПОЛЬЗУЕМ параметр angle из команды!
    public IEnumerator TurnRight(float angle)
    {
        Debug.Log($"➡️ Поворот вправо на {angle} градусов");

        float elapsed = 0f;
        float duration = angle / turnSpeed;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 0, -angle);

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;
        Debug.Log("✅ Поворот вправо завершён");
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        Debug.Log("🔄 Позиция и поворот сброшены");
    }
}