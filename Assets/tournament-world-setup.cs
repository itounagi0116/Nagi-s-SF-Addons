# VRChatトーナメントワールド設定手順

## 1. 必要なパッケージとツール

- [VRChat SDK 3.0 - Worlds](https://vrchat.com/home/download)
- [UdonSharp](https://github.com/MerlinVR/UdonSharp/releases)
- [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html) (Unity Package Managerから)

## 2. Unity プロジェクトの設定

1. Unity 2019.4.31f1 で新規プロジェクトを作成
2. VRChat SDK 3.0 - Worlds をインポート
3. UdonSharp をインポート
4. TextMeshPro パッケージをインストール

## 3. シーンの基本設定

1. 新しいシーンを作成
2. VRChat SDK > Utilities > Create VRCWorld を実行
3. 基本的な環境とライティングを設定

## 4. トーナメントシステムの実装

### A. スクリプトのセットアップ

1. プロジェクト内に「Scripts」フォルダを作成
2. 提供された「TournamentSystem.cs」と「TournamentUISetup.cs」ファイルをScriptsフォルダに配置
3. UdonSharpで両方のスクリプトをコンパイル

### B. トーナメント表示エリアの作成

1. シーン内に新しい空のゲームオブジェクト「TournamentSystem」を作成
2. 「TournamentSystem」にUdonBehaviourコンポーネントを追加
3. コンポーネントのタイプを「TournamentSystem」に設定

### C. UIのセットアップ

1. トーナメント表示用のCanvas作成:
   - ヒエラルキーに新しいCanvasを追加（スケールモード：Scale With Screen Size）
   - EventSystemが自動で追加されることを確認

2. メインメニューUIの作成:
   - Canvasに「MainMenu」という名前のパネルを追加
   - パネル内にトーナメント名を表示するTextMeshProを追加
   - 「トーナメント開始」ボタンを追加

3. トーナメント表示UIの作成:
   - Canvasに「BracketDisplay」という名前のパネルを追加
   - 表示するトーナメントのラウンド数に応じたマッチ枠を追加
   - 各マッチ枠内に2つのプレイヤー名テキスト（TextMeshPro）と2つのスコアテキストを配置
   - 現在のラウンドを表示するテキストを追加

4. 優勝者表示UIの作成:
   - Canvasに「WinnerDisplay」という名前のパネルを追加
   - パネル内に優勝者名を表示するTextMeshProを追加
   - 「再スタート」ボタンを追加

5. プレイヤー登録UIの作成:
   - Canvasに「PlayerRegistration」という名前のパネルを追加
   - 名前入力用のTMP_InputFieldを追加
   - 「登録」ボタンと「登録プレイヤーリスト」を表示するTextMeshProを追加

6. 試合操作UIの作成:
   - Canvasに「MatchControl」という名前のパネルを追加
   - 現在の試合情報を表示するテキストを追加
   - プレイヤー1と2の名前を表示するテキスト
   - プレイヤー1勝利/プレイヤー2勝利ボタンを追加

### D. コンポーネントの接続

1. TournamentSystemコンポーネントのインスペクターで以下を設定:
   - UI要素（MainMenu、BracketDisplay、playerNameTexts[]、scoreTexts[]など）を対応するオブジェクトにドラッグ＆ドロップ
   - トーナメント名と最大プレイヤー数を設定

2. TournamentUISetupコンポーネントをセットアップ:
   - 新しい空のゲームオブジェクト「TournamentUISetup」を作成
   - UdonBehaviourコンポーネントを追加し、タイプを「TournamentUISetup」に設定
   - TournamentSystemへの参照とUI要素を設定

### E. ボタンイベントの接続

1. 「トーナメント開始」ボタンのOnClickイベントをTournamentUISetupのStartTournamentメソッドに接続
2. 「プレイヤー登録」ボタンのOnClickイベントをTournamentUISetupのRegisterPlayerメソッドに接続
3. 「プレイヤー1勝利」ボタンのOnClickイベントをTournamentUISetupのPlayer1Winメソッドに接続
4. 「プレイヤー2勝利」ボタンのOnClickイベントをTournamentUISetupのPlayer2Winメソッドに接続
5. 「リセット」ボタンのOnClickイベントをTournamentUISetupのResetTournamentメソッドに接続

## 5. テストとデバッグ

1. Unity Editorでシーンを再生してUIの表示を確認
2. ローカルテスト用のVRChatクライアントでビルドしてテスト
3. 問題があれば修正し、再ビルド

## 6. 高度なカスタマイズ（オプション）

### A. 視覚的なポリッシュ

1. トーナメント表のグラフィックデザインを改善
   - 勝者ラインを追加して進行を可視化
   - ラウンド間の接続線を追加
   - 各ラウンドのタイトルを追加

2. アニメーションを追加
   - 試合結果が更新されるときのトランジション
   - 勝者表示時のエフェクト

### B. 機能の拡張

1. スコアシステムの拡張
   - 単純な勝敗だけでなく数値スコアを記録可能に
   - タイブレーク機能の追加

2. 観戦モードの追加
   - 現在進行中の試合を強調表示
   - 試合予測やベッティングシステムの追加（コミュニティエンゲージメント用）

3. トーナメント形式の追加
   - シングルエリミネーション以外の形式（ダブルエリミネーション、総当たり戦など）

4. ロールベースのアクセス制御
   - 主催者、参加者、観戦者それぞれに適切な権限を設定

## 7. VRChatへのアップロード

1. ワールドのテスト版をビルド
2. [VRChat SDK](https://docs.vrchat.com/docs/vrchat-202231p1) を使用してワールドをアップロード
3. プライベートテストを実施
4. フィードバックを収集して改善
5. 最終版をパブリックにアップロード

## 8. 注意事項

- UdonSharpの同期変数は慎重に使用する（ネットワーク帯域の制限）
- 多くのプレイヤーが参加する場合のパフォーマンスを考慮する
- UIの解像度と視認性をVR環境で確認する
- 全てのインタラクティブ要素がVRコントローラーで使いやすいことを確認
