using System;

namespace Unical.Demacs.EnchantedVillage
{
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