using System;
using System.Reflection;
using UnityEngine;

namespace ProjectMSG.SaveSystem
{
    public static class PrepManagerSaveBridge
    {
        private const string PrepManagerTypeName = "PrepManager";
        private const string PlayerDataFieldName = "playerData";

        public static bool TryReadSupply(out SupplyData supply, out string message)
        {
            supply = null;

            if (!TryGetPlayerData(out object playerData, out message))
            {
                return false;
            }

            if (!TryReadIntField(playerData, "baksoSupply", out int bakso) ||
                !TryReadIntField(playerData, "mieSupply", out int mie) ||
                !TryReadIntField(playerData, "mangkokSupply", out int mangkok) ||
                !TryReadIntField(playerData, "msgSupply", out int msg))
            {
                message = "PlayerData supply fields were not found.";
                return false;
            }

            supply = new SupplyData(bakso, mie, mangkok, msg);
            message = $"Read supply from PrepManager: {supply.GetDescription()}";
            return true;
        }

        public static bool TryApplySupply(SaveData saveData, out string message)
        {
            if (saveData == null)
            {
                message = "Cannot apply supply because save data is null.";
                return false;
            }

            saveData.EnsureSupply();

            if (!TryGetPlayerData(out object playerData, out message))
            {
                return false;
            }

            bool applied =
                TryWriteFieldValue(playerData, "playerName", saveData.saveName) &&
                TryWriteFieldValue(playerData, "currentMapIndex", saveData.map) &&
                TryWriteFieldValue(playerData, "currentMoney", saveData.money) &&
                TryWriteIntField(playerData, "baksoSupply", saveData.supply.bakso) &&
                TryWriteIntField(playerData, "mieSupply", saveData.supply.mie) &&
                TryWriteIntField(playerData, "mangkokSupply", saveData.supply.mangkok) &&
                TryWriteIntField(playerData, "msgSupply", saveData.supply.msg);

            if (!applied)
            {
                message = "Failed to apply one or more save data fields to PlayerData.";
                return false;
            }

            string uiMessage = TryInvokeUpdateUI();
            message = $"Applied save data to PrepManager: {saveData.GetDescription()} | {saveData.supply.GetDescription()}{uiMessage}";
            return true;
        }

        private static bool TryGetPlayerData(out object playerData, out string message)
        {
            playerData = null;

            object prepManager = GetPrepManagerInstance();
            if (prepManager == null)
            {
                message = "PrepManager.Instance was not found.";
                return false;
            }

            FieldInfo playerDataField = prepManager.GetType().GetField(PlayerDataFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (playerDataField == null)
            {
                message = "PrepManager.playerData field was not found.";
                return false;
            }

            playerData = playerDataField.GetValue(prepManager);
            if (playerData == null)
            {
                message = "PrepManager.playerData is null.";
                return false;
            }

            message = "PlayerData found.";
            return true;
        }

        private static object GetPrepManagerInstance()
        {
            Type prepManagerType = FindType(PrepManagerTypeName);
            if (prepManagerType == null)
            {
                return null;
            }

            PropertyInfo instanceProperty = prepManagerType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            object instance = instanceProperty?.GetValue(null);
            if (instance is UnityEngine.Object unityObject && unityObject == null)
            {
                instance = null;
            }

            if (instance != null)
            {
                return instance;
            }

            foreach (MonoBehaviour behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>())
            {
                if (behaviour != null && behaviour.GetType() == prepManagerType)
                {
                    return behaviour;
                }
            }

            return null;
        }

        private static Type FindType(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static bool TryReadIntField(object target, string fieldName, out int value)
        {
            value = 0;
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                return false;
            }

            value = Convert.ToInt32(field.GetValue(target));
            return true;
        }

        private static bool TryWriteIntField(object target, string fieldName, int value)
        {
            return TryWriteFieldValue(target, fieldName, value);
        }

        private static bool TryWriteFieldValue(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                return false;
            }

            if (value != null && !field.FieldType.IsAssignableFrom(value.GetType()))
            {
                value = Convert.ChangeType(value, field.FieldType);
            }

            field.SetValue(target, value);
            return true;
        }

        private static string TryInvokeUpdateUI()
        {
            object prepManager = GetPrepManagerInstance();
            MethodInfo updateUiMethod = prepManager?.GetType().GetMethod("UpdateUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (updateUiMethod == null)
            {
                return " UI refresh skipped: UpdateUI method was not found.";
            }

            try
            {
                updateUiMethod.Invoke(prepManager, null);
                return string.Empty;
            }
            catch (Exception exception)
            {
                Exception innerException = exception is TargetInvocationException && exception.InnerException != null
                    ? exception.InnerException
                    : exception;

                return $" UI refresh skipped: {innerException.Message}";
            }
        }
    }
}
