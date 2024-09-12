using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class TrainingBase : MonoBehaviour
    {
        private int maxTrainingCapacity = 5;
        private int currentTrainingCapacity = 0;
        

        public void ShowDialog()
        {
            TrainingDialog.Instance.ShowDialog();
        }

    }
}
