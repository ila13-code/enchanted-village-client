using System;

namespace Unical.Demacs.EnchantedVillage
{
    //classe che gestisce gli eventi di movimento degli edifici
    public static class BuildingMovementEvents
    {
        public static event Action OnBuildingDragStart;
        public static event Action OnBuildingDragEnd;

        public static void TriggerBuildingDragStart()
        {
            OnBuildingDragStart?.Invoke();
        }

        public static void TriggerBuildingDragEnd()
        {
            OnBuildingDragEnd?.Invoke();
        }
    }
}