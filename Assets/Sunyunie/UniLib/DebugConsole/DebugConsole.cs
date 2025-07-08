using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace Sunyunie.UniLib
{
    public class DebugConsole : MonoBehaviour
    {
        private static readonly Dictionary<string, Action<string[]>> commandMap = new();
        private static readonly Dictionary<string, string> commandDescriptions = new();
        private static readonly Dictionary<string, string> commandSignatures = new();

        private void Awake()
        {
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            foreach (var mono in FindObjectsOfType<MonoBehaviour>())
            {
                var type = mono.GetType();

                // 필드 등록한다냥!
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attr = field.GetCustomAttribute<DebugCommandAttribute>();
                    if (attr == null) continue;

                    string command = attr.commandName;
                    commandMap[command] = args =>
                    {
                        var value = field.GetValue(mono);
                        Debug.Log($"[{command}] = {value}");
                    };
                    commandDescriptions[command] = attr.description;
                    commandSignatures[command] = "()";
                }

                // 메서드 등록한다냥!
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attr = method.GetCustomAttribute<DebugCommandAttribute>();
                    if (attr == null) continue;

                    string command = attr.commandName;
                    commandMap[command] = args =>
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length != args.Length)
                        {
                            Debug.LogWarning($"[{command}] 인자 수가 맞지 않다냥! ({parameters.Length}개 필요)");
                            return;
                        }
                        try
                        {
                            object[] parsedArgs = new object[args.Length];
                            for (int i = 0; i < args.Length; i++)
                                parsedArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);
                            method.Invoke(mono, parsedArgs);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[{command}] 인자 변환 실패: {e.Message}");
                        }
                    };
                    commandDescriptions[command] = attr.description;
                    var paramInfo = method.GetParameters();
                    var paramString = string.Join(", ", paramInfo.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    commandSignatures[command] = $"({paramString})";
                }
            }
        }

        public static void Execute(string input)
        {
            var split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0) return;

            var cmd = split[0];
            var args = split.Length > 1 ? split[1..] : Array.Empty<string>();

            if (commandMap.TryGetValue(cmd, out var action))
                action.Invoke(args);
            else
                Debug.LogWarning($"명령어 [{cmd}]를 찾을 수 없다냥!");
        }

        public static List<string> GetMatchingCommands(string partial)
        {
            return commandMap.Keys
                .Where(cmd => cmd.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static List<(string command, string description)> GetAllCommands()
        {
            return commandDescriptions
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList();
        }

        public static string GetSignature(string command)
        {
            return commandSignatures.TryGetValue(command, out var sig) ? sig : "";
        }

        public static string GetDescription(string command)
        {
            return commandDescriptions.TryGetValue(command, out var desc) ? desc : "";
        }
    }
}
