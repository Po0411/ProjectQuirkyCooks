using UnityEngine;

[CreateAssetMenu(menuName = "Delivery/Region Food Map")]
public class RegionFoodMap : ScriptableObject
{
    [Header("색상별로 배달할 ItemData 지정")]
    public ItemData red;
    public ItemData blue;
    public ItemData green;
    public ItemData yellow;

    public ItemData GetItem(RegionColor color)
    {
        switch (color)
        {
            case RegionColor.Red: return red;
            case RegionColor.Blue: return blue;
            case RegionColor.Green: return green;
            case RegionColor.Yellow: return yellow;
        }
        return null;
    }
}
