using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TournamentSystem : UdonSharpBehaviour
{
    // UI要素
    [Header("UI要素")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject bracketDisplay;
    [SerializeField] private TextMeshProUGUI[] playerNameTexts;
    [SerializeField] private TextMeshProUGUI[] scoreTexts;
    [SerializeField] private TextMeshProUGUI tournamentNameText;
    [SerializeField] private TextMeshProUGUI currentRoundText;
    [SerializeField] private GameObject winnerDisplayPanel;
    [SerializeField] private TextMeshProUGUI winnerNameText;

    // トーナメント設定
    [Header("トーナメント設定")]
    [SerializeField] private int maxPlayers = 8; // プレイヤー数（2の累乗: 4, 8, 16, 32）
    [SerializeField] private string tournamentName = "VRChatトーナメント";

    // 同期変数
    [UdonSynced] private string[] playerNames;
    [UdonSynced] private int[] matchResults;
    [UdonSynced] private int currentRound = 0;
    [UdonSynced] private int currentMatch = 0;
    [UdonSynced] private bool tournamentInProgress = false;
    [UdonSynced] private string winnerId = "";

    // ローカル変数
    private bool isHost = false;
    private int totalRounds;
    private int totalMatches;

    void Start()
    {
        // 初期化
        totalRounds = Mathf.CeilToInt(Mathf.Log(maxPlayers, 2));
        totalMatches = maxPlayers - 1;
        
        playerNames = new string[maxPlayers];
        matchResults = new int[totalMatches];

        // 初期UIセットアップ
        tournamentNameText.text = tournamentName;
        UpdateUI();
        
        // メインメニューを表示
        mainMenu.SetActive(true);
        bracketDisplay.SetActive(false);
        winnerDisplayPanel.SetActive(false);
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // ホスト確認
        isHost = Networking.GetOwner(gameObject) == Networking.LocalPlayer;
        
        // 現在の状態を更新
        UpdateUI();
    }

    // トーナメント開始
    public void StartTournament()
    {
        if (!isHost || tournamentInProgress) return;

        // 変数初期化
        tournamentInProgress = true;
        currentRound = 0;
        currentMatch = 0;
        winnerId = "";
        
        for (int i = 0; i < matchResults.Length; i++)
        {
            matchResults[i] = -1; // -1 = まだ試合が行われていない
        }

        // プレイヤー名を設定（実際のゲームではVRChatのプレイヤー名を使用）
        // ここではサンプルとしてダミーデータを設定
        for (int i = 0; i < maxPlayers; i++)
        {
            playerNames[i] = "Player " + (i + 1);
        }

        // UI更新
        UpdateUI();
        
        // トーナメント表示に切り替え
        mainMenu.SetActive(false);
        bracketDisplay.SetActive(true);
        
        // 変更を同期
        RequestSerialization();
    }

    // 試合結果を記録
    public void RecordMatchResult(int winnerId)
    {
        if (!isHost || !tournamentInProgress) return;
        
        // 現在の試合の結果を記録
        matchResults[GetMatchIndex(currentRound, currentMatch)] = winnerId;
        
        // 次の試合へ進む
        currentMatch++;
        
        // ラウンドの全試合が終了した場合、次のラウンドへ
        int matchesInRound = maxPlayers >> (currentRound + 1);
        if (currentMatch >= matchesInRound)
        {
            currentRound++;
            currentMatch = 0;
            
            // トーナメントが終了した場合
            if (currentRound >= totalRounds)
            {
                tournamentInProgress = false;
                winnerId = playerNames[matchResults[matchResults.Length - 1]];
                ShowWinner();
            }
        }
        
        // UI更新
        UpdateUI();
        
        // 変更を同期
        RequestSerialization();
    }

    // 試合インデックスを計算
    private int GetMatchIndex(int round, int match)
    {
        int offset = 0;
        for (int i = 0; i < round; i++)
        {
            offset += maxPlayers >> (i + 1);
        }
        return offset + match;
    }

    // プレイヤーインデックスを計算
    private int GetPlayerIndex(int round, int match, int position)
    {
        if (round == 0)
        {
            // 最初のラウンドでは直接インデックスを返す
            return match * 2 + position;
        }
        else
        {
            // 前のラウンドの試合結果から勝者を取得
            int prevRound = round - 1;
            int prevMatch1 = match * 2;
            int prevMatch2 = match * 2 + 1;
            
            if (position == 0)
            {
                return matchResults[GetMatchIndex(prevRound, prevMatch1)];
            }
            else
            {
                return matchResults[GetMatchIndex(prevRound, prevMatch2)];
            }
        }
    }

    // UI更新
    private void UpdateUI()
    {
        // 現在のラウンドを表示
        currentRoundText.text = tournamentInProgress 
            ? $"ラウンド: {currentRound + 1}/{totalRounds}" 
            : "トーナメント準備中";

        // プレイヤー名とスコアを更新
        for (int round = 0; round < totalRounds; round++)
        {
            int matchesInRound = maxPlayers >> (round + 1);
            
            for (int match = 0; match < matchesInRound; match++)
            {
                // この試合のプレイヤーを取得
                int player1Index = GetPlayerIndex(round, match, 0);
                int player2Index = GetPlayerIndex(round, match, 1);
                
                // 対応するUIを更新
                int uiIndex = GetMatchIndex(round, match);
                
                if (round == 0)
                {
                    // 最初のラウンドはプレイヤー名を直接表示
                    playerNameTexts[uiIndex * 2].text = player1Index < playerNames.Length ? playerNames[player1Index] : "TBD";
                    playerNameTexts[uiIndex * 2 + 1].text = player2Index < playerNames.Length ? playerNames[player2Index] : "TBD";
                }
                else
                {
                    // 他のラウンドは前の試合の勝者を表示
                    int prevMatchIndex1 = GetMatchIndex(round - 1, match * 2);
                    int prevMatchIndex2 = GetMatchIndex(round - 1, match * 2 + 1);
                    
                    // 前の試合が終了している場合のみ名前を表示
                    playerNameTexts[uiIndex * 2].text = matchResults[prevMatchIndex1] >= 0 
                        ? playerNames[matchResults[prevMatchIndex1]] 
                        : "TBD";
                    
                    playerNameTexts[uiIndex * 2 + 1].text = matchResults[prevMatchIndex2] >= 0 
                        ? playerNames[matchResults[prevMatchIndex2]] 
                        : "TBD";
                }
                
                // スコアを更新
                int matchIndex = GetMatchIndex(round, match);
                if (matchResults[matchIndex] >= 0)
                {
                    // 試合が終了している場合、勝者を表示
                    scoreTexts[matchIndex * 2].text = matchResults[matchIndex] == player1Index ? "Win" : "";
                    scoreTexts[matchIndex * 2 + 1].text = matchResults[matchIndex] == player2Index ? "Win" : "";
                }
                else
                {
                    // 試合がまだの場合
                    scoreTexts[matchIndex * 2].text = "";
                    scoreTexts[matchIndex * 2 + 1].text = "";
                }
            }
        }
    }

    // 勝者を表示
    private void ShowWinner()
    {
        winnerNameText.text = "優勝: " + winnerId;
        bracketDisplay.SetActive(false);
        winnerDisplayPanel.SetActive(true);
    }

    // トーナメントをリセット
    public void ResetTournament()
    {
        if (!isHost) return;
        
        tournamentInProgress = false;
        currentRound = 0;
        currentMatch = 0;
        winnerId = "";
        
        for (int i = 0; i < matchResults.Length; i++)
        {
            matchResults[i] = -1;
        }
        
        // UI更新
        UpdateUI();
        
        // メインメニューに戻る
        mainMenu.SetActive(true);
        bracketDisplay.SetActive(false);
        winnerDisplayPanel.SetActive(false);
        
        // 変更を同期
        RequestSerialization();
    }

    // オーナーシップ確認
    public void RequestOwnership()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            isHost = true;
        }
    }

    // 同期コールバック
    public override void OnDeserialization()
    {
        // データが同期されたらUIを更新
        UpdateUI();
        
        // 状態に応じたUIの表示/非表示
        if (tournamentInProgress)
        {
            mainMenu.SetActive(false);
            bracketDisplay.SetActive(true);
            winnerDisplayPanel.SetActive(false);
        }
        else if (!string.IsNullOrEmpty(winnerId))
        {
            mainMenu.SetActive(false);
            bracketDisplay.SetActive(false);
            winnerDisplayPanel.SetActive(true);
        }
        else
        {
            mainMenu.SetActive(true);
            bracketDisplay.SetActive(false);
            winnerDisplayPanel.SetActive(false);
        }
    }
}
