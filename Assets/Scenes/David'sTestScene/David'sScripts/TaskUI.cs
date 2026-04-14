using UnityEngine;
using TMPro;

public class TaskUI : MonoBehaviour
{
    [Header("References")]
    public CommandParent commandScript; 
    public TextMeshProUGUI taskText; 

    [Header("Display Settings")]
    public string manualLabel = "Task: Manual";
    public string foodLabel = "Task: Food";

    void Update()
    {
        if (commandScript == null || taskText == null) return;

        if (commandScript.taskToAssign == AntTask.Manual)
        {
            taskText.text = manualLabel;
            taskText.color = Color.white;
        }
        else if (commandScript.taskToAssign == AntTask.Food)
        {
            taskText.text = foodLabel;
            taskText.color = Color.green;
        }
    }
}
