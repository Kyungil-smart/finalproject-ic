using TMPro;
using UnityEngine;

public class T03ItemUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    
    public void SetString(string itemName) => itemNameText.text = itemName;
}