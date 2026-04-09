using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotProgramExecutor : MonoBehaviour
{
    [Header("References")]
    public GameObject robotBody;
    public RobotMotor robotMotor;

    [Header("Settings")]
    public float moveSpeed = 100f;
    public float turnSpeed = 180f;
    public float betweenCommandDelay = 0.2f;

    private Vector3 startPosition;
    private float startRotation;
    private bool startPositionSaved = false;
    private bool isRunning = false;
    private Coroutine currentRoutine;

    void Start()
    {
        InitializeReferences();
    }

    void InitializeReferences()
    {
        if (robotMotor == null)
        {
            robotMotor = FindFirstObjectByType<RobotMotor>();
        }

        if (robotBody == null && robotMotor != null)
        {
            robotBody = robotMotor.gameObject;
        }

        SaveStartPosition();
    }

    void SaveStartPosition()
    {
        if (robotBody != null)
        {
            startPosition = robotBody.transform.position;
            startRotation = robotBody.transform.eulerAngles.z;
            startPositionSaved = true;
        }
    }

    public void ExecuteProgram(List<RobotCommand> commands)
    {
        Debug.Log("🚀 Запуск программы!");

        if (commands == null || commands.Count == 0)
        {
            Debug.LogWarning("⚠️ Программа пуста!");
            return;
        }

        if (robotMotor == null)
        {
            Debug.LogError("❌ Motor НЕ назначен!");
            return;
        }

        if (isRunning) StopProgram();

        isRunning = true;
        currentRoutine = StartCoroutine(RunProgramCoroutine(commands));
    }

    public void StopProgram()
    {
        Debug.Log("⏹ Остановка программы!");

        isRunning = false;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }
    }

    private IEnumerator RunProgramCoroutine(List<RobotCommand> commands)
    {
        for (int i = 0; i < commands.Count; i++)
        {
            if (!isRunning) yield break;

            RobotCommand cmd = commands[i];
            Debug.Log($"[{i + 1}/{commands.Count}] {cmd.name} (значение: {cmd.value})");

            if (cmd.type == RobotCommand.CommandType.LoopStart)
            {
                yield return StartCoroutine(ExecuteLoop(cmd));
            }
            else
            {
                yield return StartCoroutine(ExecuteSingleCommand(cmd));
            }

            if (i < commands.Count - 1 && isRunning)
            {
                yield return new WaitForSeconds(betweenCommandDelay);
            }
        }

        Debug.Log("✅ Программа выполнена!");
        isRunning = false;
        currentRoutine = null;
    }

    private IEnumerator ExecuteLoop(RobotCommand loopCommand)
    {
        Debug.Log($"🔁 ЦИКЛ: {loopCommand.loopCount} повторений");

        if (loopCommand.nestedCommands == null || loopCommand.nestedCommands.Count == 0)
        {
            Debug.LogWarning("⚠️ Цикл пуст!");
            yield break;
        }

        for (int iteration = 0; iteration < loopCommand.loopCount && isRunning; iteration++)
        {
            Debug.Log($"  🔁 Итерация {iteration + 1}/{loopCommand.loopCount}");

            for (int j = 0; j < loopCommand.nestedCommands.Count && isRunning; j++)
            {
                RobotCommand nestedCmd = loopCommand.nestedCommands[j];
                Debug.Log($"    → {nestedCmd.name} (значение: {nestedCmd.value})");
                yield return StartCoroutine(ExecuteSingleCommand(nestedCmd));
            }
        }

        Debug.Log("  ✅ Цикл завершён!");
    }

    private IEnumerator ExecuteSingleCommand(RobotCommand command)
    {
        switch (command.type)
        {
            case RobotCommand.CommandType.MoveForward:
                Debug.Log($"  ⬆️ ВПЕРЁД на {command.value} пикселей");
                yield return StartCoroutine(MoveDistance(command.value));
                break;

            case RobotCommand.CommandType.MoveBackward:
                Debug.Log($"  ⬇️ НАЗАД на {command.value} пикселей");
                yield return StartCoroutine(MoveDistance(-command.value));
                break;

            case RobotCommand.CommandType.TurnLeft:
                Debug.Log($"  ↺ ВЛЕВО на {command.value}°");
                yield return StartCoroutine(RotateAngle(command.value));
                break;

            case RobotCommand.CommandType.TurnRight:
                Debug.Log($"  ↻ ВПРАВО на {command.value}°");
                yield return StartCoroutine(RotateAngle(-command.value));
                break;
        }
    }

    // ✅ ИСПРАВЛЕНО: робот движется в направлении своего поворота
    IEnumerator MoveDistance(float distance)
    {
        if (robotBody == null) yield break;

        float elapsed = 0f;
        float duration = Mathf.Abs(distance) / moveSpeed;
        Vector3 startPos = robotBody.transform.position;

        // ✅ ИСПРАВЛЕНО: используем локальное направление робота (transform.up)
        Vector3 targetPos = startPos + robotBody.transform.up * distance;

        while (elapsed < duration && isRunning)
        {
            robotBody.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        robotBody.transform.position = targetPos;
        Debug.Log($"✅ Движение завершено: {distance} пикселей");
    }

    // ✅ ИСПРАВЛЕНО: правильное вращение для всех углов (включая 180°)
    IEnumerator RotateAngle(float angle)
    {
        if (robotBody == null) yield break;

        float elapsed = 0f;
        float duration = Mathf.Abs(angle) / turnSpeed;

        // ✅ Сохраняем начальный угол
        float startAngle = robotBody.transform.eulerAngles.z;

        // ✅ Нормализуем начальный угол (0-360)
        if (startAngle < 0) startAngle += 360f;

        // ✅ Вычисляем целевой угол без нормализации Unity
        float targetAngle = startAngle + angle;

        while (elapsed < duration && isRunning)
        {
            float t = elapsed / duration;
            // ✅ Интерполируем угол вручную, а не через Quaternion.Lerp
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);
            robotBody.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ✅ Устанавливаем финальный угол
        robotBody.transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        Debug.Log($"✅ Поворот завершён: от {startAngle}° до {targetAngle}°");
    }

    public void ResetRobotPosition()
    {
        if (startPositionSaved && robotBody != null)
        {
            robotBody.transform.position = startPosition;
            robotBody.transform.rotation = Quaternion.Euler(0, 0, startRotation);
            Debug.Log($"🔄 Робот сброшен на позицию: {startPosition}");
        }
    }

    void OnDestroy()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
    }
}