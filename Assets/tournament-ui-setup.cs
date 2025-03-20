using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

public class TournamentUISetup : UdonSharpBehaviour
{
    [Header("トーナメントシステム参照")]
    [SerializeField] private TournamentSystem tournamentSystem;

    [Header("プレイヤー登録UI")]
    [SerializeField] private GameObject playerRegistrationPanel;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private TextMeshProUGUI registeredPlayersText;
    [SerializeField] private Button startTournamentButton;

    [Header("試合操作UI")]
    [SerializeField] private GameObject matchControlPanel;
    [SerializeField] private TextMeshProUGUI currentMatchText;
    [SerializeField] private Button player1WinButton;
    [SerializeField] private Button player2WinButton;
    [SerializeField] private TextMeshProUGUI player1NameText;
    [SerializeField] private TextMeshProUGUI player2NameText;

    [UdonSynced] private string[] registeredPlayers;
    [UdonSynced] private int registeredPlayerCount = 0;
    [UdonSynced] private bool registrationOpen = true;

    private bool isAdmin = false;
    
    void Start()
    {
        // 初期化
        registeredPlayers = new string[tournamentSystem.maxPlayers];
        UpdateRegistrationUI();
        
        // 管理者権限チェック
        CheckAdminStatus();
        
        // 初期UIステート
        playerRegistrationPanel.SetActive(true);
        matchControlPanel.SetActive(false);
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // 管理者権限の確認
        CheckAdminStatus();
        UpdateRegistrationUI();
    }

    private void CheckAdminStatus()
    {
        // インスタンスのマスターまたはオブジェクトのオーナーが管理者
        isAdmin = Networking.IsMaster || Networking.IsOwner(Networking.LocalPlayer, gameObject);
        
        // 管理者用ボタンの有効/無効
        startTournamentButton.interactable = isAdmin && registeredPlayerCount >= 2;
    }

    // プレイヤー登録
    public void RegisterPlayer()
    {
        if (!registrationOpen) return;
        
        string playerName = playerNameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName)) return;
        
        // 重複チェック
        for (int i = 0; i < registeredPlayerCount; i++)
        {
            if (registeredPlayers[i] == playerName)
            {
                // 重複エラー表示
                return;
            }
        }
        
        // プレイヤーを登録
        if (registeredPlayerCount < tournamentSystem.maxPlayers)
        {
            // オーナーシップ取得
            if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            
            registeredPlayers[registeredPlayerCount] = playerName;
            registeredPlayerCount++;
            
            // UI更新
            UpdateRegistrationUI();
            
            // 入力フィールドをクリア
            playerNameInput.text = "";
            
            // 変更を同期
            RequestSerialization();
        }
    }

    // 登録UI更新
    private void UpdateRegistrationUI()
    {
        // 登録済みプレイヤーリスト
        string playerList = "登録済みプレイヤー (" + registeredPlayerCount + "/" + tournamentSystem.maxPlayers + "):\n";
        for (int i = 0; i < registeredPlayerCount; i++)
        {
            playerList += (i + 1) + ". " + registeredPlayers[i] + "\n";
        }
        registeredPlayersText.text = playerList;
        
        // 登録ボタンの有効/無効
        registerButton.interactable = registrationOpen && registeredPlayerCount < tournamentSystem.maxPlayers;
        
        // 開始ボタンの有効/無効（管理者のみ）
        startTournamentButton.interactable = isAdmin && registeredPlayerCount >= 2;
    }

    // トーナメント開始
    public void StartTournament()
    {
        if (!isAdmin || registeredPlayerCount < 2) return;
        
        // オーナーシップ確認
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        // 登録を締め切り
        registrationOpen = false;
        
        // トーナメントシステムのオーナーシップ取得
        tournamentSystem.RequestOwnership();
        
        // プレイヤー名をトーナメントシステムにセット
        // 注: 実際のコードでは、トーナメントシステムにプレイヤー名を設定するメソッドが必要
        
        // トーナメント開始
        tournamentSystem.StartTournament();
        
        // UIモード切り替え
        playerRegistrationPanel.SetActive(false);
        matchControlPanel.SetActive(true);
        
        // 最初の試合情報を表示
        UpdateMatchControlUI();
        
        // 変更を同期
        RequestSerialization();
    }

    // 試合コントロールUI更新
    private void UpdateMatchControlUI()
    {
        // 現在の試合情報を取得
        int currentRound = tournamentSystem.currentRound;
        int currentMatch = tournamentSystem.currentMatch;
        
        // 現在の試合のプレイヤーを取得
        int player1Index = tournamentSystem.GetPlayerIndex(currentRound, currentMatch, 0);
        int player2Index = tournamentSystem.GetPlayerIndex(currentRound, currentMatch, 1);
        
        // UI更新
        currentMatchText.text = "ラウンド " + (currentRound + 1) + " - 試合 " + (currentMatch + 1);
        player1NameText.text = registeredPlayers[player1Index];
        player2NameText.text = registeredPlayers[player2Index];
        
        // ボタン有効化（管理者のみ）
        player1WinButton.interactable = isAdmin;
        player2WinButton.interactable = isAdmin;
    }

    // プレイヤー1勝利
    public void Player1Win()
    {
        if (!isAdmin) return;
        
        // 現在の試合情報を取得
        int currentRound = tournamentSystem.currentRound;
        int currentMatch = tournamentSystem.currentMatch;
        
        // プレイヤー1のインデックスを取得
        int player1Index = tournamentSystem.GetPlayerIndex(currentRound, currentMatch, 0);
        
        // 試合結果を記録
        tournamentSystem.RecordMatchResult(player1Index);
        
        // UI更新
        UpdateMatchControlUI();
    }

    // プレイヤー2勝利
    public void Player2Win()
    {
        if (!isAdmin) return;
        
        // 現在の試合情報を取得
        int currentRound = tournamentSystem.currentRound;
        int currentMatch = tournamentSystem.currentMatch;
        
        // プレイヤー2のインデックスを取得
        int player2Index = tournamentSystem.GetPlayerIndex(currentRound, currentMatch, 1);
        
        // 試合結果を記録
        tournamentSystem.RecordMatchResult(player2Index);
        
        // UI更新
        UpdateMatchControlUI();
    }

    // トーナメント終了または中断時
    public void ResetUI()
    {
        playerRegistrationPanel.SetActive(true);
        matchControlPanel.SetActive(false);
        
        // 登録をリセット
        registrationOpen = true;
        registeredPlayerCount = 0;
        
        // UI更新
        UpdateRegistrationUI();
        
        // 変更を同期
        RequestSerialization();
    }

    // トーナメントリセット
    public void ResetTournament()
    {
        if (!isAdmin) return;
        
        // オーナーシップ確認
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        // トーナメントシステムのオーナーシップ取得
        tournamentSystem.RequestOwnership();
        
        // トーナメントリセット
        tournamentSystem.ResetTournament();
        
        // UI状態をリセット
        ResetUI();
    }

    // 同期コールバック
    public override void OnDeserialization()
    {
        UpdateRegistrationUI();
        
        // UIモードの切り替え
        if (!registrationOpen)
        {
            playerRegistrationPanel.SetActive(false);
            matchControlPanel.SetActive(true);
            UpdateMatchControlUI();
        }
        else
        {
            playerRegistrationPanel.SetActive(true);
            matchControlPanel.SetActive(false);
        }
    }
}
