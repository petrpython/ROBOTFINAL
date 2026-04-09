using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CycleBlockHandler : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField loopCountInput;
    public Transform nestedContent;
    public TextMeshProUGUI titleText;

    private int commandId = -1;
    private ProgrammingInterface programInterface;
    private RobotCommand cycleCommand;
    private bool isUpdatingDisplay = false;

    void Start()
    {
        programInterface = FindFirstObjectByType<ProgrammingInterface>();

        if (loopCountInput != null)
        {
            loopCountInput.onEndEdit.AddListener(OnLoopCountChanged);
            Debug.Log("✅ loopCountInput настроен");
        }
        else
        {
            Debug.LogWarning("⚠️ loopCountInput НЕ назначен!");
        }

        if (nestedContent == null)
        {
            Debug.LogError("❌ nestedContent НЕ НАЗНАЧЕН в Inspector!");
        }
        else
        {
            SetupNestedContentArea();
        }
    }

    // ✅ ИСПРАВЛЕНО: правильное позиционирование nestedContent
    void SetupNestedContentArea()
    {
        if (nestedContent == null) return;

        RectTransform rt = nestedContent.GetComponent<RectTransform>();
        if (rt == null) rt = nestedContent.gameObject.AddComponent<RectTransform>();

        // ✅ Якоря - на всю ширину, снизу от заголовка
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);

        // ✅ Отступы: слева 5, справа 5, сверху 35 (под заголовком), снизу 5
        rt.offsetMin = new Vector2(5f, 5f);
        rt.offsetMax = new Vector2(-5f, -35f);

        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;

        Debug.Log($"📍 nestedContent настроен: {rt.rect}");
    }

    // ✅ ИСПРАВЛЕНО: принудительно устанавливаем минимум 1
    public void Initialize(int uniqueId, int loopCount, RobotCommand command)
    {
        if (command == null)
        {
            Debug.LogError("❌ command is NULL!");
            return;
        }

        commandId = uniqueId;
        cycleCommand = command;

        if (cycleCommand.nestedCommands == null)
        {
            cycleCommand.nestedCommands = new List<RobotCommand>();
        }

        if (loopCountInput != null)
        {
            // ✅ ИСПРАВЛЕНО: принудительно устанавливаем минимум 1
            int displayValue = command.loopCount > 0 ? command.loopCount : 1;
            command.loopCount = displayValue;
            loopCountInput.text = displayValue.ToString();
            Debug.Log($"✅ Loop count установлен: {displayValue} (из команды: {command.loopCount})");
        }

        if (titleText != null)
            titleText.text = command.name;

        UpdateNestedCommandsDisplay();
    }

    // ✅ ИСПРАВЛЕНО: сохраняем цвет команды из палитры
    public void AddCommandFromPalette(RobotCommand.CommandType type, string cmdName, Color color)
    {
        if (cycleCommand == null)
        {
            Debug.LogWarning("⚠️ cycleCommand is NULL!");
            return;
        }

        if (cycleCommand.nestedCommands == null)
        {
            cycleCommand.nestedCommands = new List<RobotCommand>();
        }

        float defaultValue = 0f;
        switch (type)
        {
            case RobotCommand.CommandType.MoveForward:
            case RobotCommand.CommandType.MoveBackward:
                defaultValue = 50f;
                break;
            case RobotCommand.CommandType.TurnLeft:
            case RobotCommand.CommandType.TurnRight:
                defaultValue = 90f;
                break;
        }

        RobotCommand newCmd = new RobotCommand(type, defaultValue, cmdName);
        newCmd.uniqueId = System.Guid.NewGuid().GetHashCode();
        newCmd.buttonColor = color; // ✅ Сохраняем цвет из палитры

        cycleCommand.nestedCommands.Add(newCmd);
        Debug.Log($"✅ Добавлено в цикл: {cmdName} = {defaultValue} (цвет: {color})");

        UpdateNestedCommandsDisplay();
    }

    public void UpdateNestedCommandsDisplay()
    {
        if (isUpdatingDisplay) return;
        isUpdatingDisplay = true;

        try
        {
            if (nestedContent == null || cycleCommand == null) return;

            if (cycleCommand.nestedCommands == null)
                cycleCommand.nestedCommands = new List<RobotCommand>();

            int commandCount = cycleCommand.nestedCommands.Count;

            for (int i = nestedContent.childCount - 1; i >= 0; i--)
            {
                Destroy(nestedContent.GetChild(i).gameObject);
            }

            for (int i = 0; i < commandCount; i++)
            {
                RobotCommand cmd = cycleCommand.nestedCommands[i];
                if (cmd == null) continue;

                CreateCommandBlock(cmd, i);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(nestedContent.GetComponent<RectTransform>());
            AdjustCycleBlockHeight(commandCount);
        }
        finally
        {
            isUpdatingDisplay = false;
        }
    }

    void AdjustCycleBlockHeight(int commandCount)
    {
        RectTransform cycleRect = GetComponent<RectTransform>();
        if (cycleRect == null) return;

        float minHeight = 45f;
        float contentHeight = commandCount > 0 ? (commandCount * 32f + 10f) : 0f;
        float targetHeight = Mathf.Max(minHeight, minHeight + contentHeight);
        targetHeight = Mathf.Min(targetHeight, 200f);

        Vector2 newSize = cycleRect.sizeDelta;
        newSize.y = targetHeight;
        cycleRect.sizeDelta = newSize;

        Debug.Log($"📏 Высота цикла: {targetHeight} (команд: {commandCount})");
    }

    // ✅ ИСПРАВЛЕНО: Image.Type.Simple вместо Sliced (чтобы цвет не был ярче)
    void CreateCommandBlock(RobotCommand cmd, int index)
    {
        if (nestedContent == null)
        {
            Debug.LogError("❌ nestedContent is NULL в CreateCommandBlock!");
            return;
        }

        GameObject block = new GameObject("Cmd_" + cmd.name);
        block.transform.SetParent(nestedContent, false);

        RectTransform blockRT = block.AddComponent<RectTransform>();
        blockRT.anchorMin = new Vector2(0f, 1f);
        blockRT.anchorMax = new Vector2(1f, 1f);
        blockRT.pivot = new Vector2(0.5f, 1f);
        blockRT.sizeDelta = new Vector2(-10f, 30f);
        blockRT.anchoredPosition = new Vector2(0f, -index * 32f - 5f);
        blockRT.localRotation = Quaternion.identity;
        blockRT.localScale = Vector3.one;

        Image bg = block.AddComponent<Image>();
        bg.color = cmd.buttonColor;
        bg.type = Image.Type.Simple; // ✅ ИСПРАВЛЕНО: Simple вместо Sliced (цвет не будет ярче)
        bg.raycastTarget = true;

        LayoutElement layoutElement = block.AddComponent<LayoutElement>();
        layoutElement.minHeight = 30;
        layoutElement.preferredHeight = 30;
        layoutElement.flexibleHeight = 0;

        bool needsValue = (cmd.type == RobotCommand.CommandType.MoveForward ||
                           cmd.type == RobotCommand.CommandType.MoveBackward ||
                           cmd.type == RobotCommand.CommandType.TurnLeft ||
                           cmd.type == RobotCommand.CommandType.TurnRight);

        if (needsValue)
        {
            GameObject container = new GameObject("Container");
            container.transform.SetParent(block.transform, false);

            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.sizeDelta = Vector2.zero;
            containerRT.anchoredPosition = Vector2.zero;

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(0.65f, 1);
            labelRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = cmd.name;
            labelText.fontSize = 11;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontStyle = FontStyles.Bold;

            GameObject inputFieldObj = new GameObject("ValueInput");
            inputFieldObj.transform.SetParent(container.transform, false);

            RectTransform inputRT = inputFieldObj.AddComponent<RectTransform>();
            inputRT.anchorMin = new Vector2(0.65f, 0);
            inputRT.anchorMax = new Vector2(1, 1);
            inputRT.sizeDelta = new Vector2(-5, -6);
            inputRT.anchoredPosition = Vector2.zero;

            Image inputBg = inputFieldObj.AddComponent<Image>();
            inputBg.color = new Color(1f, 1f, 1f, 0.9f);
            inputBg.type = Image.Type.Sliced;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputFieldObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(3, 2);
            textRT.offsetMax = new Vector2(-3, -2);

            TextMeshProUGUI inputText = textObj.AddComponent<TextMeshProUGUI>();
            inputText.text = cmd.value.ToString();
            inputText.fontSize = 10;
            inputText.color = Color.black;
            inputText.alignment = TextAlignmentOptions.Center;

            TMP_InputField inputField = inputFieldObj.AddComponent<TMP_InputField>();
            inputField.textComponent = inputText;
            inputField.text = cmd.value.ToString();
            inputField.contentType = TMPro.TMP_InputField.ContentType.DecimalNumber;
            inputField.lineType = TMPro.TMP_InputField.LineType.SingleLine;

            inputField.onEndEdit.AddListener((newValue) => {
                float parsedValue;
                if (float.TryParse(newValue, out parsedValue))
                {
                    cmd.value = parsedValue;
                }
            });
        }
        else
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(block.transform, false);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = cmd.name;
            text.fontSize = 12;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(3, 2);
            textRT.offsetMax = new Vector2(-3, -2);
        }

        SimpleDragDrop sdd = block.AddComponent<SimpleDragDrop>();
        sdd.Initialize(cmd.uniqueId, index, cmd.value);
        sdd.SetParentCycle(this);

        RightClickHandler rightClick = block.AddComponent<RightClickHandler>();
        rightClick.Initialize(cmd.uniqueId, this, cmd.name);
    }

    public void RemoveCommand(int index)
    {
        if (cycleCommand == null || cycleCommand.nestedCommands == null) return;
        if (index < 0 || index >= cycleCommand.nestedCommands.Count) return;

        cycleCommand.nestedCommands.RemoveAt(index);
        UpdateNestedCommandsDisplay();
    }

    public void OnLoopCountChanged(string newValue)
    {
        if (commandId < 0 || programInterface == null) return;

        int count;
        if (int.TryParse(newValue, out count))
        {
            count = Mathf.Max(1, Mathf.Min(count, 100));
            programInterface.UpdateLoopCount(commandId, count);

            if (loopCountInput != null)
                loopCountInput.text = count.ToString();

            Debug.Log($"🔄 Количество повторов цикла: {count}");
        }
        else
        {
            if (loopCountInput != null)
                loopCountInput.text = "1";
            programInterface.UpdateLoopCount(commandId, 1);
        }
    }

    public Transform GetNestedContent() => nestedContent;
    public RobotCommand GetCycleCommand() => cycleCommand;
    public int GetCommandId() => commandId;
}

public class RightClickHandler : MonoBehaviour, IPointerClickHandler
{
    private int commandId;
    private CycleBlockHandler parentCycle;
    private string commandName;

    public void Initialize(int id, CycleBlockHandler cycle, string cmdName)
    {
        commandId = id;
        parentCycle = cycle;
        commandName = cmdName;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log($"🖱️ ПКМ на команде в цикле: {commandName}");

            if (parentCycle != null)
            {
                RobotCommand cmd = parentCycle.GetCycleCommand();
                if (cmd != null && cmd.nestedCommands != null)
                {
                    for (int i = 0; i < cmd.nestedCommands.Count; i++)
                    {
                        if (cmd.nestedCommands[i].uniqueId == commandId)
                        {
                            Debug.Log($"🗑️ Удаляем команду {commandName} из цикла");
                            parentCycle.RemoveCommand(i);
                            break;
                        }
                    }
                }
            }
        }
    }
}