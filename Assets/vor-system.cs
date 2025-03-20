using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// VOR (VHF Omnidirectional Range) システムの実装
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class VORSystem : NavigationSystemBase
{
    [Header("VOR固有の設定")]
    [Tooltip("VOR表示用針")]
    public GameObject vorNeedle;
    
    [Tooltip("TO/FROM表示用オブジェクト")]
    public GameObject toFromIndicator;
    
    [Tooltip("OBS (Omni Bearing Selector) ダイアル")]
    public GameObject obsDialObject;
    
    // VOR専用の計算値
    private float radialFromVOR;
    private float radialToVOR;
    private bool isApproachingVOR;
    private float selectedRadial = 0.0f; // OBSで選択された放射方位
    
    /// <summary>
    /// 初期化
    /// </summary>
    protected override void Start()
    {
        navIdentifier = navIdentifier.Length > 0 ? navIdentifier : "VOR";
        base.Start();
    }
    
    /// <summary>
    /// OBS (Omni Bearing Selector) ダイアルを調整する
    /// </summary>
    public void AdjustOBS(float adjustment)
    {
        selectedRadial = (selectedRadial + adjustment + 360.0f) % 360.0f;
        UpdateOBSDisplay();
    }
    
    /// <summary>
    /// OBS表示を更新
    /// </summary>
    private void UpdateOBSDisplay()
    {
        if (obsDialObject != null)
        {
            obsDialObject.transform.localRotation = Quaternion.Euler(0, 0, -selectedRadial);
        }
    }
    
    /// <summary>
    /// VOR固有の計算を実行
    /// </summary>
    protected override void CalculateSpecificValues()
    {
        // VORからの放射方位 (磁方位)
        radialFromVOR = (headingToStation + 180) % 360;
        
        // VORへの磁方位
        radialToVOR = headingToStation;
        
        // 機首方向とVORへの方位の差を計算して接近/離脱を判定
        if (pilotTransform != null)
        {
            float pilotHeading = pilotTransform.eulerAngles.y;
            float headingDifference = Mathf.DeltaAngle(pilotHeading, headingToStation);
            isApproachingVOR = Mathf.Abs(headingDifference) < 90.0f;
        }
    }
    
    /// <summary>
    /// VOR表示を更新
    /// </summary>
    protected override void UpdateSpecificDisplay()
    {
        if (vorNeedle != null)
        {
            // 選択ラジアルとVORからのラジアルとの偏差を計算
            float deviation = Mathf.DeltaAngle(selectedRadial, radialFromVOR);
            
            // 針の回転を設定 (-10度から+10度の範囲で表示するために偏差をスケーリング)
            float maxDeflection = 10.0f; // 最大10度の針の振れ
            float normalizedDeviation = Mathf.Clamp(deviation / 10.0f, -1.0f, 1.0f);
            float needleRotation = normalizedDeviation * maxDeflection;
            
            vorNeedle.transform.localRotation = Quaternion.Euler(0, 0, needleRotation);
            
            // TO/FROM表示を更新
            if (toFromIndicator != null)
            {
                if (Mathf.Abs(deviation) > 90.0f)
                {
                    // FROM表示
                    toFromIndicator.transform.localRotation = Quaternion.Euler(0, 0, 180);
                }
                else
                {
                    // TO表示
                    toFromIndicator.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
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
        if (vorNeedle != null)
        {
            vorNeedle.transform.localRotation = Quaternion.identity;
        }
        
        // TO/FROM表示を隠す
        if (toFromIndicator != null)
        {
            toFromIndicator.transform.localRotation = Quaternion.Euler(0, 0, 90); // 中立位置
        }
    }
    
    /// <summary>
    /// デバッグ用の描画を拡張
    /// </summary>
    void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        if (systemTransform == null) return;
        
        // VORの特定のラジアル線を表示（デバッグ用）
        Gizmos.color = Color.green;
        for (int i = 0; i < 36; i += 3)
        {
            float angle = i * 10 * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
            Gizmos.DrawRay(systemTransform.position, direction * 10000.0f); // 10km
        }
    }
}
