using TMPro;
using UnityEngine;

public class MainSceneBackgroundImgController : MonoBehaviour
{
    [Header("Background UI")]
    [SerializeField] private SpriteRenderer bgImage;
    [SerializeField] private Sprite dayBackground;
    [SerializeField] private Sprite nightBackground;
    
    [SerializeField] private SpriteRenderer[] buildingRobbyImages;
    [SerializeField] private SpriteRenderer buildingRoofImage;
    [SerializeField] private Sprite dayBuildingRobby;
    [SerializeField] private Sprite nightBuildingRobby;
    [SerializeField] private Sprite dayBuildingRoof;
    [SerializeField] private Sprite nightBuildingRoof;

    [Header("Controller")] 
    [SerializeField] private bool enableTestMode;
    [SerializeField] private bool isNight;

    private void Update()
    {
        if (enableTestMode) Render(!isNight);
        else Render(Utils.DayCheck.IsDaytime());
    }

    private void Render(bool flag)
    {
        bgImage.sprite = flag ? dayBackground : nightBackground;
        foreach (var buildingRobby in buildingRobbyImages)
            buildingRobby.sprite = flag ? dayBuildingRobby : nightBuildingRobby;
        buildingRoofImage.sprite = flag ? dayBuildingRoof : nightBuildingRoof;
    } 
}