using UnityEngine;

namespace ProjectMSG.SaveSystem
{
    public sealed class SaveSlotButton : MonoBehaviour
    {
        public enum SlotAction
        {
            SelectOrCreateAndEnterPrep,
            SaveCurrentGameToSlot,
            LoadAndEnterPrep,
            DeleteSlot
        }

        [SerializeField] private int saveId = 1;
        [SerializeField] private SlotAction action = SlotAction.SelectOrCreateAndEnterPrep;
        [SerializeField] private SaveSlotSceneController sceneController;

        public void Press()
        {
            SaveSlotSceneController controller = GetOrCreateSceneController();

            switch (action)
            {
                case SlotAction.SelectOrCreateAndEnterPrep:
                    controller.SelectOrCreateSlot(saveId);
                    break;
                case SlotAction.SaveCurrentGameToSlot:
                    controller.SaveCurrentToSlot(saveId);
                    break;
                case SlotAction.LoadAndEnterPrep:
                    controller.LoadSlotAndEnterPrep(saveId);
                    break;
                case SlotAction.DeleteSlot:
                    controller.DeleteSlot(saveId);
                    break;
                default:
                    Debug.LogWarning($"Unhandled save slot action: {action}");
                    break;
            }
        }

        private SaveSlotSceneController GetOrCreateSceneController()
        {
            if (sceneController != null)
            {
                return sceneController;
            }

            sceneController = FindAnyObjectByType<SaveSlotSceneController>();
            if (sceneController != null)
            {
                return sceneController;
            }

            GameObject controllerObject = new GameObject("Save Slot Scene Controller");
            sceneController = controllerObject.AddComponent<SaveSlotSceneController>();
            return sceneController;
        }
    }
}
