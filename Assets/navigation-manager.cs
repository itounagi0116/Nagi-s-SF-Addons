using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

/// <summary>
/// 複数のナビゲーションシステム（VOR/ILS）を管理するマネージャークラス
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class NavigationManager : UdonSharpBehaviour
{
    [Header("ナビゲーションシステム")]
    [Tooltip("VORシステムの配列")]
    public VORSystem[] vorSystems;
    
    [Tooltip("ILSシステムの配列")]
    public ILSSystem[] ilsSystems;
    
    [Header("UI要素")]
    [Tooltip("ナビゲーションタイプ表示テキスト")]
    public Text navTypeText;
    
    [Tooltip("ナビゲーション識別子表示テキスト")]
    public Text navIdText;
    
    [Tooltip("ナビゲーション周波数表示テキスト")]
    public Text navFreqText;
    
    [UdonSynced] private int selectedNavType = 0; // 0: VOR, 1: ILS
    [UdonSynced] private int selectedSystemIndex = 0;
    
    /// <summary>
    /// 初期化
    /// </summary>
    void Start()
    {
        UpdateActiveSystem();
        UpdateUIDisplay();
    }
    
    /// <summary>
    /// ナビゲーションタイプを切り替える（VOR/ILS）
    /// </summary>
    public void ToggleNavigationType()
    {
        selectedNavType = (selectedNavType == 0) ? 1 : 0;
        selectedSystemIndex = 0; // 新しいタイプの最初のシステムを選択
        
        RequestSerialization();
        UpdateActiveSystem();
        UpdateUIDisplay();
    }
    
    /// <summary>
    /// 同じタイプの中で次のナビゲーションシステムに切り替える
    /// </summary>
    public void NextNavigationSystem()
    {
        if (selectedNavType == 0 && vorSystems != null && vorSystems.Length > 0)
        {
            selectedSystemIndex = (selectedSystemIndex + 1) % vorSystems.Length;
        }
        else if (selectedNavType == 1 && ilsSystems != null && ilsSystems.Length > 0)
        {
            selectedSystemIndex = (selectedSystemIndex + 1) % ilsSystems.Length;
        }
        
        RequestSerialization();
        UpdateActiveSystem();
        UpdateUIDisplay();
    }
    
    /// <summary>
    /// 周波数を調整する
    /// </summary>
    public void AdjustFrequency(float adjustment)
    {
        if (selectedNavType == 0 && vorSystems != null && vorSystems.Length > 0 && selectedSystemIndex < vorSystems.Length)
        {
            vorSystems[selectedSystemIndex].TuneFrequency(adjustment);
        }
        else if (selectedNavType == 1 && ilsSystems != null && ilsSystems.Length > 0 && selectedSystemIndex < ilsSystems.Length)
        {
            ilsSystems[selectedSystemIndex].TuneFrequency(adjustment);
        }
        
        UpdateUIDisplay();
    }
    
    /// <summary>
    /// アクティブなシステムを更新する
    /// </summary>
    private void UpdateActiveSystem()
    {
        // すべてのVORシステムを非アクティブ化
        if (vorSystems != null)
        {
            for (int i = 0; i < vorSystems.Length; i++)
            {
                if (vorSystems[i] != null && vorSystems[i].displayContainer != null)
                {
                    vorSystems[i].displayContainer.SetActive(selectedNavType == 0 && i == selectedSystemIndex);
                }
            }
        }
        
        // すべてのILSシステムを非アクティブ化
        if (ilsSystems != null)
        {
            for (int i = 0; i < ilsSystems.Length; i++)
            {
                if (ilsSystems[i] != null && ilsSystems[i].displayContainer != null)
                {
                    ilsSystems[i].displayContainer.SetActive(selectedNavType == 1 && i == selectedSystemIndex);
                }
            }
        }
    }
    
    /// <summary>
    /// UI表示を更新する
    /// </summary>
    private void UpdateUIDisplay()
    {
        if (navTypeText != null)
        {
            navTypeText.text = selectedNavType == 0 ? "VOR" : "ILS";
        }
        
        // 選択中のシステムの情報を表示
        if (selectedNavType == 0 && vorSystems != null && vorSystems.Length > 0 && selectedSystemIndex < vorSystems.Length)
        {
            VORSystem activeVOR = vorSystems[selectedSystemIndex];
            
            if (navIdText != null && activeVOR != null)
            {
                navIdText.text = activeVOR.navIdentifier;
            }
            
            if (navFreqText != null && activeVOR != null)
            {
                navFreqText.text = activeVOR.selectedFrequency.ToString("F2") + " MHz";
            }
        }
        else if (selectedNavType == 1 && ilsSystems != null && ilsSystems.Length > 0 && selectedSystemIndex < ilsSystems.Length)
        {
            ILSSystem activeILS = ilsSystems[selectedSystemIndex];
            
            if (navIdText != null && activeILS != null)
            {
                navIdText.text = activeILS.navIdentifier;
            }
            
            if (navFreqText != null && activeILS != null)
            {
                navFreqText.text = activeILS.selectedFrequency.ToString("F2") + " MHz";
            }
        }
    }
    
    /// <summary>
    /// VORシステムのOBSを調整する (専用メソッド)
    /// </summary>
    public void AdjustOBS(float adjustment)
    {
        if (selectedNavType == 0 && vorSystems != null && vorSystems.Length > 0 && selectedSystemIndex < vorSystems.Length)
        {
            vorSystems[selectedSystemIndex].AdjustOBS(adjustment);
        }
    }
}
