using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// ILS (Instrument Landing System) の実装
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ILSSystem : NavigationSystemBase
{
    [Header("ILS固有の設定")]
    [Tooltip("滑走路真方位 (度)")]
    public float runwayHeading = 270.0f;
    
    [Tooltip("グライドスロープ角度 (度)")]
    public float glideslopeAngle = 3.0f;
    
    [Tooltip("滑走路の長さ (メートル)")]
    public float runwayLength = 3000.0f;
    
    [Tooltip("ローカライザー針")]
    public GameObject localizerNeedle;
    
    [Tooltip("グライドスロープ針")]
    public GameObject glideslopeNeedle;
    
    [Tooltip("滑走路表示オブジェクト")]
    public GameObject runwayDisplayObject;
    
    // ILS専用の計算値
    private float localizerDeviation;
    private float glideslopeDeviation;
    private Vector3 runwayDirection;
    private float distanceFromThreshold;
    
    /// <summary>
    /// 初期化
    /// </summary>
    protected override void Start()
    {
        navIdentifier = navIdentifier.Length > 0 ? navIdentifier : "ILS";
        
        // 滑走路の方向ベクトルを計算
        runwayDirection = Quaternion.Euler(0, runwayHeading, 0) * Vector3.forward;
        
        base.Start();
    }
    
    /// <summary>
    /// ILS固有の計算を実行
    /// </summary>
    protected override void CalculateSpecificValues()
    {
        // パイロットとILSの相対位置
        Vector3 ilsToPilot = pilotTransform.position - systemTransform.position;
        
        // 滑走路中心線に対する投影を計算
        float dotProduct = Vector3.Dot(ilsToPilot, runwayDirection);
        
        // 滑走路末端からの距離（接地点までの距離）
        distanceFromThreshold = dotProduct;
        
        // 滑走路中心線からの横方向偏差を計算
        Vector3 projection = dotProduct * runwayDirection;
        Vector3 lateralDeviation = ilsToPilot - projection;
        
        // ローカライザー偏差を計算 (±2.5度が標準)
        float lateralDeviationMagnitude = lateralDeviation.magnitude;
        localizerDeviation = Vector3.Dot(lateralDeviation, Vector3.Cross(Vector3.up, runwayDirection)) > 0 ? 
                              lateralDeviationMagnitude : -lateralDeviationMagnitude;
        
        // グライドスロープの理想高度を計算
        float idealAltitude = Mathf.Tan(glideslopeAngle * Mathf.Deg2Rad) * distanceFromThreshold;
        float actualAltitude = ilsToPilot.y;
        
        // グライドスロープからの偏差を計算
        glideslopeDeviation = actualAltitude - idealAltitude;
    }
    
    /// <summary>
    /// ILS表示を更新
    /// </summary>
    protected override void UpdateSpecificDisplay()
    {
        // ローカライザー針の更新
        if (localizerNeedle != null)
        {
            // 標準的なフルスケール偏差は±2.5度または約±150m（4nm地点で）
            float maxLateralDeviation = 150.0f;
            float normalizedDeviation = Mathf.Clamp(localizerDeviation / maxLateralDeviation, -1.0f, 1.0f);
            float horizontalPosition = normalizedDeviation * 50.0f; // 50ユニットを最大変位と仮定
            
            // 針の水平位置を更新
            localizerNeedle.transform.localPosition = new Vector3(horizontalPosition, 0, 0);
        }
        
        // グライドスロープ針の更新
        if (glideslopeNeedle != null)
        {
            // 標準的なフルスケール偏差は±0.7度または約±50m（4nm地点で）
            float maxVerticalDeviation = 50.0f;
            float normalizedDeviation = Mathf.Clamp(glideslopeDeviation / maxVerticalDeviation, -1.0f, 1.0f);
            float verticalPosition = normalizedDeviation * 50.0f; // 50ユニットを最大変位と仮定
            
            // 針の垂直位置を更新
            glideslopeNeedle.transform.localPosition = new Vector3(0, verticalPosition, 0);
        }
        
        // 滑走路表示を更新（オプション）
        if (runwayDisplayObject != null)
        {
            // 接地点までの距離に基づいて滑走路アイコンのサイズを調整するなどの処理
            float scale = Mathf.Clamp(5000.0f / (distanceFromThreshold + 1000.0f), 0.5f, 5.0f);
            runwayDisplayObject.transform.localScale = new Vector3(scale, scale, 1.0f);
        }
        
        // 特別な距離表示（滑走路末端までの距離）
        if (distanceText != null)
        {
            if (distanceFromThreshold > 0)
            {
                distanceText.text = (distanceFromThreshold / 1000.0f).ToString("F1") + " km to RWY";
            }
            else
            {
                distanceText.text = "On Runway";
            }
        }
    }
    
    /// <summary>
    /// 信号がない場合の処理
    /// </summary>
    protected override void HandleNoSignal()
    {
        base.HandleNoSignal();
        
        // 針をセンター位置に
        if (localizerNeedle != null)
        {
            localizerNeedle.transform.localPosition = Vector3.zero;
        }
        
        if (glideslopeNeedle != null)
        {
            glideslopeNeedle.transform.localPosition = Vector3.zero;
        }
        
        // 滑走路表示を非表示
        if (runwayDisplayObject != null)
        {
            runwayDisplayObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// デバッグ用の描画を拡張
    /// </summary>
    void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        if (systemTransform == null) return;
        
        // 滑走路の方向を表示
        Gizmos.color = Color.blue;
        Vector3 rwDir = Quaternion.Euler(0, runwayHeading, 0) * Vector3.forward;
        Gizmos.DrawRay(systemTransform.position, rwDir * runwayLength);
        
        // グライドスロープを表示
        Gizmos.color = Color.yellow;
        Vector3 gsDir = Quaternion.Euler(-glideslopeAngle, runwayHeading, 0) * Vector3.forward;
        Gizmos.DrawRay(systemTransform.position, gsDir * 10000.0f);
        
        // ローカライザーの範囲を表示
        Gizmos.color = Color.cyan;
        float locWidth = Mathf.Tan(2.5f * Mathf.Deg2Rad) * 10000.0f;
        Vector3 leftDir = Quaternion.Euler(0, runwayHeading - 2.5f, 0) * Vector3.forward;
        Vector3 rightDir = Quaternion.Euler(0, runwayHeading + 2.5f, 0) * Vector3.forward;
        Gizmos.DrawRay(systemTransform.position, leftDir * 10000.0f);
        Gizmos.DrawRay(systemTransform.position, rightDir * 10000.0f);
    }
}
