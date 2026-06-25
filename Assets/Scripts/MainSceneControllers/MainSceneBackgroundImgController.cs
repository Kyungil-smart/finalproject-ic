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

    private void Update()
    {
        bgImage.sprite = Utils.DayCheck.IsDaytime() ? dayBackground : nightBackground;
        foreach (var buildingRobby in buildingRobbyImages)
            buildingRobby.sprite = Utils.DayCheck.IsDaytime() ? dayBuildingRobby : nightBuildingRobby;
        buildingRoofImage.sprite = Utils.DayCheck.IsDaytime() ? dayBuildingRoof : nightBuildingRoof;
    }
}