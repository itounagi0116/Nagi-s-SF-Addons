using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 航空ナビゲーションシステムの共通機能を提供するベースクラス
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public abstract class NavigationSystemBase : UdonSharpBehaviour
{
    [Header("基本設定")]
    [Tooltip("ナビゲーション識別子（例: 'KYO'）")]
    public string navIdentifier = "NAV";
    
    [Tooltip("周波数 (MHz)")]
    public float frequency = 113.5f;
    
    [Tooltip("システムの位置")]
    public Transform systemTransform;
    
    [Tooltip("磁気偏差 (度)")]
    public float magneticVariation = 7.0f;
    
    [Header("共通UI要素")]
    public GameObject displayContainer;
    public UnityEngine.UI.Text frequencyText;
    public UnityEngine.UI.Text identifierText;
    public UnityEngine.UI.Text distanceText;
    
    [Header("パイロット参照")]
    public Transform pilotTransform;
    
    // 共通の計算値
    protected float distanceToStation;
    protected float headingToStation;
    protected bool isReceiving;
    
    [UdonSynced] protected float selectedFrequency = 113.5f;
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    protected virtual void Start()
    {
        UpdateDisplays();
    }
    
    /// <summary>
    /// 毎フレーム更新
    /// </summary>
    protected virtual void Update()
    {
        if (pilotTransform == null)
        {
            pilotTransform = Networking.LocalPlayer.GetTransform();
            if (pilotTransform == null) return;
        }
        
        // 受信可能かを確認 (選択した周波数が設定周波数と一致するか)
        isReceiving = Mathf.Approximately(selectedFrequency, frequency);
        
        if (isReceiving)
        {
            CalculateBaseValues();
            CalculateSpecificValues();
            UpdateSpecificDisplay();
        }
        else
        {
            // 受信できない場合の処理
            HandleNoSignal();
        }
    }
    
    /// <summary>
    /// 周波数を調整する
    /// </summary>
    public void TuneFrequency(float adjustment)
    {
        selectedFrequency += adjustment;
        
        // 周波数範囲の制限 (VHF NAV帯域: 108.0 - 117.95 MHz)
        if (selectedFrequency < 108.0f) selectedFrequency = 108.0f;
        if (selectedFrequency > 117.95f) selectedFrequency = 117.95f;
        
        RequestSerialization();
        UpdateDisplays();
    }
    
    /// <summary>
    /// UI表示を更新する
    /// </summary>
    protected virtual void UpdateDisplays()
    {
        // 周波数表示を更新
        if (frequencyText != null)
        {
            frequencyText.text = selectedFrequency.ToString("F2") + " MHz";
        }
        
        // 識別子表示を更新
        if (identifierText != null)
        {
            identifierText.text = navIdentifier;
        }
    }
    
    /// <summary>
    /// 基本的な位置関係の計算
    /// </summary>
    protected virtual void CalculateBaseValues()
    {
        if (systemTransform == null || pilotTransform == null) return;
        
        // ステーションからの距離を計算
        Vector3 stationToPilot = pilotTransform.position - systemTransform.position;
        distanceToStation = stationToPilot.magnitude / 1000.0f; // キロメートル単位
        
        // 2D平面上での方向を計算 (高度は無視)
        Vector3 stationToPilot2D = new Vector3(stationToPilot.x, 0, stationToPilot.z).normalized;
        
        // 北を基準とした角度を計算
        headingToStation = Mathf.Atan2(stationToPilot2D.x, stationToPilot2D.z) * Mathf.Rad2Deg;
        headingToStation = (headingToStation + 360) % 360; // 0-360の範囲に正規化
        
        // 磁気偏差を適用
        headingToStation = (headingToStation + magneticVariation + 360) % 360;
        
        // 距離表示を更新
        if (distanceText != null)
        {
            distanceText.text = distanceToStation.ToString("F1") + " km";
        }
    }
    
    /// <summary>
    /// 派生クラスで実装する特定の計算
    /// </summary>
    protected abstract void CalculateSpecificValues();
    
    /// <summary>
    /// 派生クラスで実装する特定の表示更新
    /// </summary>
    protected abstract void UpdateSpecificDisplay();
    
    /// <summary>
    /// 信号がない場合の処理
    /// </summary>
    protected virtual void HandleNoSignal()
    {
        // 距離表示を無効化
        if (distanceText != null)
        {
            distanceText.text = "NO SIGNAL";
        }
    }
    
    /// <summary>
    /// デバッグ用の描画
    /// </summary>
    void OnDrawGizmos()
    {
        if (systemTransform == null) return;
        
        // ステーションの位置を示す
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(systemTransform.position, 5.0f);
        
        // 受信範囲を示す（仮定値）
        Gizmos.color = new Color(1, 1, 0, 0.1f);
        Gizmos.DrawSphere(systemTransform.position, 50000.0f); // 50km
    }
}
