using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

namespace Sunyunie.UniLib
{
    public class DebugConsoleUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private GameObject consolePanel;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text previewText;
        [SerializeField] private TMP_Text logText;
        [SerializeField] private KeyCode toggleKey = KeyCode.Insert;

        private bool isVisible = false;

        private const int maxLogLines = 50;
        private readonly Queue<string> logLines = new();

        private readonly List<string> commandHistory = new();
        private int historyIndex = -1;

        private void Start()
        {
            consolePanel.SetActive(false);
            inputField.onSubmit.AddListener(OnCommandEntered);
            inputField.onValueChanged.AddListener(OnInputChanged);
            logText.text = "";
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
                consolePanel.SetActive(isVisible);

                if (isVisible)
                {
                    inputField.text = "";
                    inputField.Select();
                    inputField.ActivateInputField();
                }
            }

            if (!isVisible) return;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (commandHistory.Count > 0)
                {
                    historyIndex = Mathf.Clamp(historyIndex - 1, 0, commandHistory.Count - 1);
                    inputField.text = commandHistory[historyIndex];
                    inputField.caretPosition = inputField.text.Length;
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                historyIndex = Mathf.Clamp(historyIndex + 1, 0, commandHistory.Count);
                if (historyIndex < commandHistory.Count)
                {
                    inputField.text = commandHistory[historyIndex];
                    inputField.caretPosition = inputField.text.Length;
                }
                else
                {
                    inputField.text = "";
                }
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                inputField.text = AutoComplete(inputField.text);
                inputField.caretPosition = inputField.text.Length;
            }
        }

        private string AutoComplete(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            var matches = DebugConsole.GetMatchingCommands(input);

            if (matches.Count == 1)
            {
                return matches[0];
            }
            else if (matches.Count > 1)
            {
                PrintLog($"Command List: {string.Join(", ", matches)}");
            }

            return input;
        }

        private void OnCommandEntered(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            commandHistory.Add(input);
            historyIndex = commandHistory.Count;

            PrintLog($"> {input}");
            DebugConsole.Execute(input);

            inputField.text = "";
            inputField.ActivateInputField();
        }

        private void OnInputChanged(string currentInput)
        {
            if (string.IsNullOrWhiteSpace(currentInput))
            {
                previewText.text = "";
                return;
            }

            var split = currentInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
            {
                previewText.text = "";
                return;
            }

            string commandPart = split[0];

            string signature = DebugConsole.GetSignature(commandPart);
            string description = DebugConsole.GetDescription(commandPart);

            var matches = DebugConsole.GetMatchingCommands(commandPart);

            if (matches.Count == 1)
            {
                previewText.text = $"🔹 {matches[0]}{signature} - {description}";
            }
            else if (matches.Count > 1)
            {
                previewText.text = $"사용 가능: {string.Join(", ", matches)}";
            }
            else
            {
                previewText.text = "";
            }
        }

        public void PrintLog(string message)
        {
            logLines.Enqueue(message);

            if (logLines.Count > maxLogLines)
                logLines.Dequeue();

            logText.text = string.Join("\n", logLines);
        }

        [DebugCommand("clear", "로그 지우기")]
        public void ClearLog()
        {
            logLines.Clear();
            logText.text = "";
        }

        [DebugCommand("help", "도움말")]
        public void Help()
        {
            var commands = DebugConsole.GetAllCommands();
            if (commands.Count == 0)
            {
                PrintLog("debug console에 등록된 명령어가 없습니다.");
                return;
            }

            PrintLog("사용 가능한 명령어:");
            foreach (var cmd in commands)
            {
                PrintLog($"- {cmd.command}: {cmd.description}");
            }
        }
    }
}
