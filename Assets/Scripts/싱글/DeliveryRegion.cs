using UnityEngine;

/// <summary>
/// 지형 타일(바닥) 같은 오브젝트에 붙여서 “이 구역의 색”을 지정.
/// 콜라이더를 둔 다음(추천: BoxCollider, isTrigger 체크) 손님/플레이어가 위에 있을 때 색을 알아낼 수 있음.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DeliveryRegion : MonoBehaviour
{
    public RegionColor regionColor = RegionColor.Red;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true; // 권장
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color c = Color.white;
        switch (regionColor)
        {
            case RegionColor.Red: c = new Color(1, 0.3f, 0.3f, 0.4f); break;
            case RegionColor.Blue: c = new Color(0.3f, 0.6f, 1f, 0.4f); break;
            case RegionColor.Green: c = new Color(0.3f, 1f, 0.5f, 0.4f); break;
            case RegionColor.Yellow: c = new Color(1f, 0.95f, 0.3f, 0.45f); break;
        }
        Gizmos.color = c;
        Gizmos.DrawCube(GetComponent<Collider>().bounds.center, GetComponent<Collider>().bounds.size);
    }
#endif
}
