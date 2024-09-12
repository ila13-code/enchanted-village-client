using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingDialog : MonoBehaviour
{
    public static TrainingDialog Instance { get; private set; }
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private GameObject UIPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDialog()
    {
        UIPanel.SetActive(false);
        dialogPanel.SetActive(true);

    }

    public void HideDialog()
    {
        dialogPanel.SetActive(false);
    }
}
