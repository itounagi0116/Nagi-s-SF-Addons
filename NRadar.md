# VRChat向けレーダーシステム - SMMV-Collab アーキテクチャ設計

## 1. アーキテクチャ概要

SMMV-Collab フレームワークの原則に基づき、VRChat向けレーダーシステムを次の層構造で設計します。

### レイヤー構造

```
[ UI/プレゼンテーション層 ] - レーダー表示・操作インターフェース
        ↓ 依存
[ アプリケーション層 ] - レーダー機能のユースケース・制御
        ↓ 依存
[ ドメイン層 ] - レーダーのビジネスロジック・データモデル
        ↑ 実装
[ インフラストラクチャ層 ] - VRChat/UdonSharp連携・データソース
```

## 2. ドメイン層（Model）

### 2.1 エンティティ

```csharp
// 航空機/艦艇エンティティ
public class TrackedObject
{
    public readonly TrackId Id; // 識別子
    public Vector3 Position { get; private set; }

## 6. 依存性注入の実装

```csharp
// 依存性注入コンテナ (シンプルな実装)
public class RadarSystemContainer : UdonSharpBehaviour
{
    [Header("Infrastructure Layer")]
    [SerializeField] private UdonTrackedObjectRepository _trackedObjectRepo;
    [SerializeField] private UdonRadarSettingsRepository _settingsRepo;
    [SerializeField] private UdonRadarDetectionService _detectionService;
    [SerializeField] private UdonTargetTrackingService _trackingService;
    
    [Header("Application Layer")]
    [SerializeField] private ScanRadarUseCase _scanRadarUseCase;
    [SerializeField] private LockTargetUseCase _lockTargetUseCase;
    [SerializeField] private ChangeRadarSettingsUseCase _changeSettingsUseCase;
    
    [Header("Presentation Layer")]
    [SerializeField] private RadarController[] _radarControllers;
    
    private void Start()
    {
        // 依存関係の注入
        InitializeApplicationLayer();
        InitializePresentationLayer();
    }
    
    private void InitializeApplicationLayer()
    {
        // ユースケースへの依存注入
        _scanRadarUseCase.Initialize(_detectionService, _trackedObjectRepo, _settingsRepo);
        _lockTargetUseCase.Initialize(_detectionService, _trackingService);
        _changeSettingsUseCase.Initialize(_settingsRepo);
    }
    
    private void InitializePresentationLayer()
    {
        // コントローラーへの依存注入
        foreach (var controller in _radarControllers)
        {
            controller.Initialize(_scanRadarUseCase, _lockTargetUseCase, _changeSettingsUseCase);
        }
    }
}
```

## 7. ディレクトリ構造

SMMV-Collab フレームワークに基づいた推奨プロジェクト構造です。

```
Assets/
├── Scripts/
│   ├── RadarSystem/
│   │   ├── Presentation/
│   │   │   ├── Controllers/
│   │   │   │   ├── RadarController.cs
│   │   │   │   ├── AircraftRadarController.cs
│   │   │   │   ├── AwacsRadarController.cs
│   │   │   │   ├── ShipRadarController.cs
│   │   │   │   └── ...
│   │   │   ├── Views/
│   │   │   │   ├── RadarView.cs
│   │   │   │   ├── TrackMarkerView.cs
│   │   │   │   └── ...
│   │   │   └── Components/
│   │   │       ├── RadarScreen.cs
│   │   │       ├── ButtonPanel.cs
│   │   │       └── ...
│   │   │
│   │   ├── Application/
│   │   │   ├── UseCases/
│   │   │   │   ├── ScanRadarUseCase.cs
│   │   │   │   ├── LockTargetUseCase.cs
│   │   │   │   ├── ChangeRadarSettingsUseCase.cs
│   │   │   │   └── ...
│   │   │   └── DTOs/
│   │   │       ├── RadarScanRequestDTO.cs
│   │   │       ├── RadarScanResultDTO.cs
│   │   │       ├── TrackedObjectDTO.cs
│   │   │       └── ...
│   │   │
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── TrackedObject.cs
│   │   │   │   └── ...
│   │   │   ├── ValueObjects/
│   │   │   │   ├── TrackId.cs
│   │   │   │   ├── RadarSettings.cs
│   │   │   │   └── ...
│   │   │   ├── Services/
│   │   │   │   ├── IRadarDetectionService.cs
│   │   │   │   ├── ITargetTrackingService.cs
│   │   │   │   └── ...
│   │   │   └── Repositories/
│   │   │       ├── ITrackedObjectRepository.cs
│   │   │       ├── IRadarSettingsRepository.cs
│   │   │       └── ...
│   │   │
│   │   └── Infrastructure/
│   │       ├── Persistence/
│   │       │   ├── UdonTrackedObjectRepository.cs
│   │       │   ├── UdonRadarSettingsRepository.cs
│   │       │   └── ...
│   │       └── Services/
│   │           ├── UdonRadarDetectionService.cs
│   │           ├── UdonTargetTrackingService.cs
│   │           └── ...
│   │
│   └── Shared/
│       ├── Utils/
│       │   ├── MathUtils.cs
│       │   └── ...
│       └── Enums/
│           ├── RadarMode.cs
│           ├── TrackType.cs
│           └── ...
│
├── Prefabs/
│   ├── RadarSystems/
│   │   ├── AircraftRadar.prefab
│   │   ├── AwacsRadar.prefab
│   │   ├── ShipRadar.prefab
│   │   └── ...
│   └── UI/
│       ├── RadarScreen.prefab
│       ├── TrackMarker.prefab
│       └── ...
│
└── Resources/
    ├── Sounds/
    │   ├── RadarScan.wav
    │   ├── TargetLock.wav
    │   └── ...
    └── Materials/
        ├── RadarScreen.mat
        ├── TrackMarker.mat
        └── ...
```

## 8. 実装のメリットと拡張性

1. **モジュール化と責任の分離**:
   - 各コンポーネントが単一の責任を持ち、理解しやすく保守しやすい
   - レーダーの種類（航空機、艦艇、AWACS）を別々のコントローラーとして実装しながら共通ロジックを共有

2. **拡張性**:
   - 新しいレーダータイプを追加する場合は、RadarControllerを継承した新しいクラスを作成するだけ
   - 新しい機能（地形マッピングなど）を追加する場合は、適切なユースケースとドメインサービスを追加

3. **テスト容易性**:
   - インターフェースを介した依存関係により、モックを使用したテストが可能
   - ドメインロジックとインフラ実装を分離することで、UdonSharpの特殊性に関係なくビジネスロジックをテスト可能

4. **VRChat/UdonSharp対応**:
   - インフラストラクチャ層で、VRChatの同期やUdonSharpの制約に対応
   - プレゼンテーション層でVRChatのUI/UX要素を扱い、下位層は純粋なビジネスロジックに集中

5. **AI協働の効率化**:
   - 明確な構造により、AIによるコード生成や拡張が容易
   - パターン化された設計により、AIが新しい機能を適切な場所に実装できる

## 9. 既存仕様との整合性

このアーキテクチャは既存の仕様書で定義された全ての機能をカバーしています：

1. **航空機搭載レーダー**: `AircraftRadarController`として実装
2. **管制塔レーダー**: `ControlTowerRadarController`として実装可能
3. **AWACSレーダー**: `AWACSRadarController`として実装可能
4. **空母搭載レーダー**: `CarrierRadarController`として実装可能
5. **護衛艦レーダー**: `DestroyerRadarController`として実装可能

共通機能（IFF識別、レンジ切替、ジャミング対応など）はドメイン層とアプリケーション層で実装され、各コントローラーから再利用されます。

## 10. AI連携と開発フロー最適化

このアーキテクチャでは、AIとの協働開発を次のように進めることができます：

1. **ドメインモデル設計**: AIにエンティティ、値オブジェクト、サービスインターフェースの設計を依頼
2. **インターフェース定義**: リポジトリやサービスのインターフェースをAIに生成させる
3. **ユースケース実装**: ビジネスフローをAIに説明し、具体的なユースケースの実装を依頼
4. **UIコントローラー実装**: プレゼンテーション層の実装をAIに依頼
5. **インフラ層実装**: UdonSharpの特殊性に配慮したリポジトリやサービスの実装をAIにガイドしてもらう

AIへのプロンプトの例:
```
「レーダーシステムのドメイン層に、航空機を表すTrackedObjectエンティティと、その識別子であるTrackIdバリューオブジェクトを設計してください。エンティティには位置、速度、アレジアンス（敵/味方）などのプロパティと、位置更新や範囲内判定などのメソッドを含めてください。」
```

```
「航空機レーダーコントローラーを実装してください。このコントローラーはRadarControllerを継承し、前方120度のスキャンコーンを持ち、検出した航空機をレーダー画面上に表示します。ヘッディングアップとノースアップの切替機能も実装してください。」
```

AIとのこの協働モデルにより、VRChat向けレーダーシステムの開発効率と品質を大幅に向上させることができます。 // 3D位置
    public Vector3 Velocity { get; private set; } // 速度ベクトル
    public TrackType Type { get; private set; } // 航空機/艦艇/ミサイル等
    public Allegiance Allegiance { get; private set; } // 敵/味方/不明
    public float Altitude { get; private set; } // 高度
    public string Callsign { get; private set; } // コールサイン
    
    // ビジネスルール・メソッド
    public void UpdatePosition(Vector3 newPosition, float deltaTime)
    {
        // 位置更新とベクトル計算
        Velocity = (newPosition - Position) / deltaTime;
        Position = newPosition;
    }
    
    public bool IsInRange(Vector3 observerPosition, float maxRange)
    {
        // 射程内判定
        return Vector3.Distance(Position, observerPosition) <= maxRange;
    }
    
    // ECM関連機能
    public float JammingEffectiveness { get; private set; }
    
    public void ApplyECM(float effectiveness)
    {
        JammingEffectiveness = Mathf.Clamp01(effectiveness);
    }
}

// 値オブジェクト - 識別子
public readonly struct TrackId
{
    public readonly string Value;
    
    public TrackId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Track ID cannot be empty");
        
        Value = value;
    }
}

// 値オブジェクト - 種別
public enum TrackType
{
    Aircraft,
    Ship,
    Missile,
    Ground,
    Unknown
}

// 値オブジェクト - 所属
public enum Allegiance
{
    Friendly,
    Hostile,
    Neutral,
    Unknown
}
```

### 2.2 ドメインサービス

```csharp
// レーダー検知サービスインターフェース
public interface IRadarDetectionService
{
    IEnumerable<TrackedObject> DetectObjects(Vector3 radarPosition, float range, float detectionCone);
    bool TryLockTarget(TrackId targetId, out TrackedObject target);
    float CalculateDetectionProbability(TrackedObject target, float distance, float ecmEffect);
}

// ターゲットトラッキングサービス
public interface ITargetTrackingService
{
    void TrackObject(TrackedObject target);
    void UntrackObject(TrackId targetId);
    IEnumerable<TrackedObject> GetTrackedObjects();
    TrackedObject GetPrimaryTarget();
    void SetPrimaryTarget(TrackId targetId);
}

// 脅威評価サービス
public interface IThreatAssessmentService
{
    ThreatLevel AssessThreat(TrackedObject target, Vector3 ownPosition);
    IEnumerable<TrackedObject> GetThreatsByPriority(Vector3 ownPosition);
}

// 値オブジェクト - 脅威レベル
public enum ThreatLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}
```

### 2.3 リポジトリインターフェース

```csharp
// トラックオブジェクトリポジトリ
public interface ITrackedObjectRepository
{
    TrackedObject GetById(TrackId id);
    IEnumerable<TrackedObject> GetAll();
    IEnumerable<TrackedObject> GetByType(TrackType type);
    IEnumerable<TrackedObject> GetByAllegiance(Allegiance allegiance);
    void Add(TrackedObject trackObject);
    void Update(TrackedObject trackObject);
    void Remove(TrackId id);
}

// レーダー設定リポジトリ
public interface IRadarSettingsRepository
{
    RadarSettings GetCurrentSettings();
    void UpdateSettings(RadarSettings settings);
}

// レーダー設定値オブジェクト
public class RadarSettings
{
    public float Range { get; private set; }
    public bool HeadingUp { get; private set; }
    public bool ECMActive { get; private set; }
    public RadarMode Mode { get; private set; }
    
    // 他のレーダー設定パラメーター
    
    // 値オブジェクトとして不変条件を強制
    public RadarSettings(float range, bool headingUp, bool ecmActive, RadarMode mode)
    {
        // バリデーション
        if (range <= 0)
            throw new ArgumentException("Range must be positive");
            
        // プロパティ設定
        Range = range;
        HeadingUp = headingUp;
        ECMActive = ecmActive;
        Mode = mode;
    }
}

// レーダーモード値オブジェクト
public enum RadarMode
{
    Standard,
    BVR,
    CloseRange,
    Maritime,
    GroundTarget
}
```

## 3. アプリケーション層

### 3.1 ユースケース

```csharp
// レーダースキャンユースケース
public class ScanRadarUseCase
{
    private readonly IRadarDetectionService _radarDetectionService;
    private readonly ITrackedObjectRepository _trackedObjectRepository;
    private readonly IRadarSettingsRepository _settingsRepository;
    
    public ScanRadarUseCase(
        IRadarDetectionService radarDetectionService,
        ITrackedObjectRepository trackedObjectRepository,
        IRadarSettingsRepository settingsRepository)
    {
        _radarDetectionService = radarDetectionService;
        _trackedObjectRepository = trackedObjectRepository;
        _settingsRepository = settingsRepository;
    }
    
    public RadarScanResultDTO Execute(RadarScanRequestDTO request)
    {
        // 設定取得
        var settings = _settingsRepository.GetCurrentSettings();
        
        // レーダースキャンの実行
        var detectedObjects = _radarDetectionService.DetectObjects(
            request.RadarPosition, 
            settings.Range, 
            request.DetectionCone);
            
        // トラックオブジェクト更新
        foreach (var obj in detectedObjects)
        {
            _trackedObjectRepository.Update(obj);
        }
        
        // 結果のDTO変換
        return new RadarScanResultDTO
        {
            DetectedObjects = detectedObjects.Select(MapToDTO).ToList(),
            ScanTimestamp = DateTime.Now,
            RadarMode = settings.Mode
        };
    }
    
    private TrackedObjectDTO MapToDTO(TrackedObject obj)
    {
        // エンティティからDTOへの変換ロジック
        return new TrackedObjectDTO
        {
            Id = obj.Id.Value,
            Position = obj.Position,
            Velocity = obj.Velocity,
            Type = obj.Type.ToString(),
            Allegiance = obj.Allegiance.ToString(),
            Altitude = obj.Altitude,
            Callsign = obj.Callsign
        };
    }
}

// ターゲットロックユースケース
public class LockTargetUseCase
{
    private readonly IRadarDetectionService _radarDetectionService;
    private readonly ITargetTrackingService _targetTrackingService;
    
    public LockTargetUseCase(
        IRadarDetectionService radarDetectionService,
        ITargetTrackingService targetTrackingService)
    {
        _radarDetectionService = radarDetectionService;
        _targetTrackingService = targetTrackingService;
    }
    
    public TargetLockResultDTO Execute(TargetLockRequestDTO request)
    {
        var targetId = new TrackId(request.TargetId);
        
        if (_radarDetectionService.TryLockTarget(targetId, out var target))
        {
            _targetTrackingService.SetPrimaryTarget(targetId);
            
            return new TargetLockResultDTO
            {
                Success = true,
                LockedTarget = new TrackedObjectDTO
                {
                    Id = target.Id.Value,
                    Position = target.Position,
                    // DTOの他フィールド
                }
            };
        }
        
        return new TargetLockResultDTO
        {
            Success = false,
            ErrorMessage = "Failed to lock target"
        };
    }
}

// レーダー設定変更ユースケース
public class ChangeRadarSettingsUseCase
{
    private readonly IRadarSettingsRepository _settingsRepository;
    
    public ChangeRadarSettingsUseCase(IRadarSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }
    
    public void Execute(ChangeRadarSettingsDTO request)
    {
        // 新しい設定値オブジェクトの作成
        var newSettings = new RadarSettings(
            request.Range,
            request.HeadingUp,
            request.ECMActive,
            MapToRadarMode(request.Mode)
        );
        
        // 設定の更新
        _settingsRepository.UpdateSettings(newSettings);
    }
    
    private RadarMode MapToRadarMode(string mode)
    {
        // 文字列からRadarModeへの変換
        return Enum.Parse<RadarMode>(mode);
    }
}
```

### 3.2 DTOモデル

```csharp
// レーダースキャンリクエストDTO
public class RadarScanRequestDTO
{
    public Vector3 RadarPosition { get; set; }
    public float DetectionCone { get; set; }
}

// レーダースキャン結果DTO
public class RadarScanResultDTO
{
    public List<TrackedObjectDTO> DetectedObjects { get; set; }
    public DateTime ScanTimestamp { get; set; }
    public RadarMode RadarMode { get; set; }
}

// トラックオブジェクトDTO
public class TrackedObjectDTO
{
    public string Id { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public string Type { get; set; }
    public string Allegiance { get; set; }
    public float Altitude { get; set; }
    public string Callsign { get; set; }
}

// ターゲットロックリクエストDTO
public class TargetLockRequestDTO
{
    public string TargetId { get; set; }
}

// ターゲットロック結果DTO
public class TargetLockResultDTO
{
    public bool Success { get; set; }
    public TrackedObjectDTO LockedTarget { get; set; }
    public string ErrorMessage { get; set; }
}

// レーダー設定変更DTO
public class ChangeRadarSettingsDTO
{
    public float Range { get; set; }
    public bool HeadingUp { get; set; }
    public bool ECMActive { get; set; }
    public string Mode { get; set; }
}
```

## 4. インフラストラクチャ層

### 4.1 リポジトリ実装

```csharp
// UdonSharpを利用したトラックオブジェクトリポジトリ
public class UdonTrackedObjectRepository : UdonSharpBehaviour, ITrackedObjectRepository
{
    // VRChatのシンクデータ連携用
    [UdonSynced]
    private List<string> _syncedTrackIds = new List<string>();
    [UdonSynced]
    private List<Vector3> _syncedPositions = new List<Vector3>();
    // 他のシンクデータフィールド
    
    // インメモリーデータストア
    private Dictionary<string, TrackedObject> _trackedObjects = new Dictionary<string, TrackedObject>();
    
    public TrackedObject GetById(TrackId id)
    {
        return _trackedObjects.TryGetValue(id.Value, out var obj) ? obj : null;
    }
    
    public IEnumerable<TrackedObject> GetAll()
    {
        return _trackedObjects.Values;
    }
    
    public IEnumerable<TrackedObject> GetByType(TrackType type)
    {
        return _trackedObjects.Values.Where(obj => obj.Type == type);
    }
    
    public IEnumerable<TrackedObject> GetByAllegiance(Allegiance allegiance)
    {
        return _trackedObjects.Values.Where(obj => obj.Allegiance == allegiance);
    }
    
    public void Add(TrackedObject trackObject)
    {
        _trackedObjects[trackObject.Id.Value] = trackObject;
        SyncDataToVRChat();
    }
    
    public void Update(TrackedObject trackObject)
    {
        if (_trackedObjects.ContainsKey(trackObject.Id.Value))
        {
            _trackedObjects[trackObject.Id.Value] = trackObject;
            SyncDataToVRChat();
        }
    }
    
    public void Remove(TrackId id)
    {
        if (_trackedObjects.ContainsKey(id.Value))
        {
            _trackedObjects.Remove(id.Value);
            SyncDataToVRChat();
        }
    }
    
    // VRChat同期処理
    private void SyncDataToVRChat()
    {
        _syncedTrackIds.Clear();
        _syncedPositions.Clear();
        // 他のシンクリストもクリア
        
        foreach (var obj in _trackedObjects.Values)
        {
            _syncedTrackIds.Add(obj.Id.Value);
            _syncedPositions.Add(obj.Position);
            // 他のデータを同期リストに追加
        }
        
        RequestSerialization();
    }
    
    // VRChat同期コールバック
    public override void OnDeserialization()
    {
        _trackedObjects.Clear();
        
        for (int i = 0; i < _syncedTrackIds.Count; i++)
        {
            // シンクデータからトラックオブジェクトを再構築
            // 実際のデシリアライズ処理はより複雑になります
        }
    }
}

// UdonSharpを利用したレーダー設定リポジトリ
public class UdonRadarSettingsRepository : UdonSharpBehaviour, IRadarSettingsRepository
{
    [UdonSynced]
    private float _syncedRange = 40f;
    [UdonSynced]
    private bool _syncedHeadingUp = true;
    [UdonSynced]
    private bool _syncedECMActive = false;
    [UdonSynced]
    private int _syncedModeValue = 0;
    
    private RadarSettings _currentSettings;
    
    private void Start()
    {
        InitializeSettings();
    }
    
    private void InitializeSettings()
    {
        _currentSettings = new RadarSettings(
            _syncedRange,
            _syncedHeadingUp,
            _syncedECMActive,
            (RadarMode)_syncedModeValue
        );
    }
    
    public RadarSettings GetCurrentSettings()
    {
        return _currentSettings;
    }
    
    public void UpdateSettings(RadarSettings settings)
    {
        _currentSettings = settings;
        
        // シンクデータを更新
        _syncedRange = settings.Range;
        _syncedHeadingUp = settings.HeadingUp;
        _syncedECMActive = settings.ECMActive;
        _syncedModeValue = (int)settings.Mode;
        
        RequestSerialization();
    }
    
    public override void OnDeserialization()
    {
        InitializeSettings();
    }
}
```

### 4.2 サービス実装

```csharp
// UdonSharpを利用したレーダー検知サービス
public class UdonRadarDetectionService : UdonSharpBehaviour, IRadarDetectionService
{
    [SerializeField]
    private LayerMask _detectableLayers;
    [SerializeField]
    private float _detectionAccuracy = 0.9f;
    
    private System.Random _random = new System.Random();
    
    public IEnumerable<TrackedObject> DetectObjects(Vector3 radarPosition, float range, float detectionCone)
    {
        var detectedObjects = new List<TrackedObject>();
        
        // VRChat環境での物理検出処理
        Collider[] hitColliders = Physics.OverlapSphere(radarPosition, range, _detectableLayers);
        
        foreach (var collider in hitColliders)
        {
            // コライダーからTrackableObjectコンポーネントを取得
            var trackableObj = collider.GetComponent<TrackableObject>();
            if (trackableObj != null)
            {
                // 検出領域内かどうかを確認
                Vector3 directionToTarget = trackableObj.transform.position - radarPosition;
                
                // 検出コーン外ならスキップ
                if (detectionCone < 360f)
                {
                    float angle = Vector3.Angle(transform.forward, directionToTarget);
                    if (angle > detectionCone * 0.5f)
                        continue;
                }
                
                // 検出率計算（ジャミング効果含む）
                float detectionProbability = CalculateDetectionProbability(
                    trackableObj.ToTrackedObject(),
                    directionToTarget.magnitude,
                    trackableObj.ECMEffectiveness);
                
                // 検出率に基づく確率チェック
                if (_random.NextDouble() <= detectionProbability)
                {
                    detectedObjects.Add(trackableObj.ToTrackedObject());
                }
            }
        }
        
        return detectedObjects;
    }
    
    public bool TryLockTarget(TrackId targetId, out TrackedObject target)
    {
        target = null;
        
        // ロックロジックの実装
        // ...
        
        return target != null;
    }
    
    public float CalculateDetectionProbability(TrackedObject target, float distance, float ecmEffect)
    {
        // レーダー反射断面積(RCS)、距離、ECM効果などを考慮した検出確率計算
        float baseProbability = _detectionAccuracy;
        
        // 距離に基づく減衰
        float distanceFactor = Mathf.Clamp01(1f - (distance / 100f));
        
        // ECM効果
        float ecmFactor = 1f - ecmEffect;
        
        // 最終検出確率
        return baseProbability * distanceFactor * ecmFactor;
    }
}

// ターゲットトラッキングサービス実装
public class UdonTargetTrackingService : UdonSharpBehaviour, ITargetTrackingService
{
    private Dictionary<string, TrackedObject> _trackedObjects = new Dictionary<string, TrackedObject>();
    private TrackId _primaryTargetId;
    
    public void TrackObject(TrackedObject target)
    {
        _trackedObjects[target.Id.Value] = target;
    }
    
    public void UntrackObject(TrackId targetId)
    {
        _trackedObjects.Remove(targetId.Value);
        
        // プライマリターゲットが削除された場合、リセット
        if (_primaryTargetId?.Value == targetId.Value)
        {
            _primaryTargetId = null;
        }
    }
    
    public IEnumerable<TrackedObject> GetTrackedObjects()
    {
        return _trackedObjects.Values;
    }
    
    public TrackedObject GetPrimaryTarget()
    {
        if (_primaryTargetId != null && _trackedObjects.TryGetValue(_primaryTargetId.Value, out var target))
        {
            return target;
        }
        return null;
    }
    
    public void SetPrimaryTarget(TrackId targetId)
    {
        if (_trackedObjects.ContainsKey(targetId.Value))
        {
            _primaryTargetId = targetId;
        }
    }
}
```

## 5. プレゼンテーション層

### 5.1 レーダーUIコントローラー

```csharp
// 基本レーダーコントローラー（抽象基底クラス）
public abstract class RadarController : UdonSharpBehaviour
{
    [SerializeField]
    protected ScanRadarUseCase _scanRadarUseCase;
    [SerializeField]
    protected LockTargetUseCase _lockTargetUseCase;
    [SerializeField]
    protected ChangeRadarSettingsUseCase _changeSettingsUseCase;
    
    [SerializeField]
    protected GameObject _radarDisplay;
    [SerializeField]
    protected AudioSource _radarAudioSource;
    
    [SerializeField]
    protected AudioClip _scanSound;
    [SerializeField]
    protected AudioClip _lockSound;
    [SerializeField]
    protected AudioClip _warningSound;
    
    // レーダー共通設定
    [SerializeField]
    protected float _updateInterval = 0.5f;
    
    protected float _nextUpdateTime;
    
    protected virtual void Start()
    {
        _nextUpdateTime = Time.time + _updateInterval;
    }
    
    protected virtual void Update()
    {
        if (Time.time >= _nextUpdateTime)
        {
            PerformRadarScan();
            _nextUpdateTime = Time.time + _updateInterval;
        }
    }
    
    protected virtual void PerformRadarScan()
    {
        var request = new RadarScanRequestDTO
        {
            RadarPosition = transform.position,
            DetectionCone = GetDetectionCone()
        };
        
        var result = _scanRadarUseCase.Execute(request);
        
        // レーダー画面の更新
        UpdateRadarDisplay(result);
        
        // 音声・効果音の再生
        PlayScanSound();
    }
    
    protected abstract float GetDetectionCone();
    
    protected abstract void UpdateRadarDisplay(RadarScanResultDTO scanResult);
    
    // UI操作メソッド - レンジ変更
    public void OnRangeButtonPressed(float newRange)
    {
        _changeSettingsUseCase.Execute(new ChangeRadarSettingsDTO 
        { 
            Range = newRange,
            HeadingUp = GetCurrentHeadingUp(),
            ECMActive = GetCurrentECMActive(),
            Mode = GetCurrentMode()
        });
    }
    
    // UI操作メソッド - ヘッディングアップ/ノースアップ切替
    public void OnOrientationTogglePressed()
    {
        _changeSettingsUseCase.Execute(new ChangeRadarSettingsDTO 
        { 
            Range = GetCurrentRange(),
            HeadingUp = !GetCurrentHeadingUp(),
            ECMActive = GetCurrentECMActive(),
            Mode = GetCurrentMode()
        });
    }
    
    // UI操作メソッド - ECM切替
    public void OnECMTogglePressed()
    {
        _changeSettingsUseCase.Execute(new ChangeRadarSettingsDTO 
        { 
            Range = GetCurrentRange(),
            HeadingUp = GetCurrentHeadingUp(),
            ECMActive = !GetCurrentECMActive(),
            Mode = GetCurrentMode()
        });
    }
    
    // UI操作メソッド - モード切替
    public void OnModeButtonPressed(string newMode)
    {
        _changeSettingsUseCase.Execute(new ChangeRadarSettingsDTO 
        { 
            Range = GetCurrentRange(),
            HeadingUp = GetCurrentHeadingUp(),
            ECMActive = GetCurrentECMActive(),
            Mode = newMode
        });
    }
    
    // 音声効果
    protected void PlayScanSound()
    {
        if (_radarAudioSource && _scanSound)
        {
            _radarAudioSource.PlayOneShot(_scanSound);
        }
    }
    
    protected void PlayLockSound()
    {
        if (_radarAudioSource && _lockSound)
        {
            _radarAudioSource.PlayOneShot(_lockSound);
        }
    }
    
    protected void PlayWarningSound()
    {
        if (_radarAudioSource && _warningSound)
        {
            _radarAudioSource.PlayOneShot(_warningSound);
        }
    }
    
    // 現在の設定を取得するヘルパーメソッド
    protected abstract float GetCurrentRange();
    protected abstract bool GetCurrentHeadingUp();
    protected abstract bool GetCurrentECMActive();
    protected abstract string GetCurrentMode();
}

// 航空機搭載レーダーコントローラー (その他のコントローラー実装は同様のパターン)
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AircraftRadarController : RadarController
{
    #region Serialized Fields and Private Variables

    [SerializeField]
    private Transform _aircraftTransform;
    [SerializeField]
    private GameObject _targetLockBox;
    [SerializeField]
    private float _detectionCone = 120f; // 前方範囲

    // レンダリング要素
    [SerializeField]
    private RectTransform _radarScreen;
    [SerializeField]
    private GameObject _trackMarkerPrefab;
    [SerializeField]
    private GameObject _rangeCirclePrefab;

    private Dictionary<string, GameObject> _trackMarkers = new Dictionary<string, GameObject>();
    private List<GameObject> _rangeCircles = new List<GameObject>();

    private ITargetTrackingService _targetTrackingService;
    private IRadarSettingsRepository _settingsRepository;

    // 現在の設定状態
    private RadarSettings _currentSettings;

    // ※ _lockTargetUseCaseが元コードでは定義されていなかったため、フィールドとして追加しました
    //【修正箇所】: _lockTargetUseCase の追加（実際の型に合わせて調整してください）
    [SerializeField]
    private LockTargetUseCase _lockTargetUseCase;

    #endregion

    #region Unity Lifecycle Methods

    protected override void Start()
    {
        base.Start();

        _targetTrackingService = FindObjectOfType<UdonTargetTrackingService>();
        _settingsRepository = FindObjectOfType<UdonRadarSettingsRepository>();

        _currentSettings = _settingsRepository.GetCurrentSettings();

        InitializeRadarDisplay();
    }

    #endregion

    #region Radar Display Initialization and Updates

    private void InitializeRadarDisplay()
    {
        // レーダー画面の初期化（レンジサークルなど）
        ClearRangeCircles();

        // 現在のレンジに基づいて新しいサークルを生成
        CreateRangeCircles(_currentSettings.Range);
    }

    protected override float GetDetectionCone()
    {
        return _detectionCone;
    }

    protected override void UpdateRadarDisplay(RadarScanResultDTO scanResult)
    {
        // まず既存のマーカーをクリア（不要になったもの）
        List<string> activeIds = scanResult.DetectedObjects.Select(obj => obj.Id).ToList();

        foreach (var trackId in _trackMarkers.Keys.ToList())
        {
            if (!activeIds.Contains(trackId))
            {
                Destroy(_trackMarkers[trackId]);
                _trackMarkers.Remove(trackId);
            }
        }

        // 新しいマーカーを作成または既存のものを更新
        foreach (var obj in scanResult.DetectedObjects)
        {
            Vector2 screenPosition = CalculateScreenPosition(obj, _aircraftTransform.position, _currentSettings.HeadingUp);

            if (_trackMarkers.TryGetValue(obj.Id, out var marker))
            {
                // 既存マーカーを更新
                marker.transform.localPosition = new Vector3(screenPosition.x, screenPosition.y, 0);
                UpdateMarkerAppearance(marker, obj);
            }
            else
            {
                // 新しいマーカーを作成
                var newMarker = Instantiate(_trackMarkerPrefab, _radarScreen);
                newMarker.transform.localPosition = new Vector3(screenPosition.x, screenPosition.y, 0);

                UpdateMarkerAppearance(newMarker, obj);

                _trackMarkers[obj.Id] = newMarker;
            }
        }

        // プライマリターゲットのハイライト
        UpdatePrimaryTargetDisplay();
    }

    #endregion

    #region Helper Methods

    private Vector2 CalculateScreenPosition(TrackedObjectDTO obj, Vector3 radarPos, bool headingUp)
    {
        // 対象の相対位置（2D水平面）
        Vector3 relativePos = obj.Position - radarPos;

        // 高度情報は別表示のため、水平面上の位置のみを使用
        Vector2 horizontalPos = new Vector2(relativePos.x, relativePos.z);

        // ヘディングアップの場合、自機の向きに対して回転
        if (headingUp)
        {
            float angle = -_aircraftTransform.eulerAngles.y * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            horizontalPos = new Vector2(
                horizontalPos.x * cos - horizontalPos.y * sin,
                horizontalPos.x * sin + horizontalPos.y * cos
            );
        }

        //【修正箇所】: _currentSettings.Rangeが0以下の場合、0除算を避けるために0を返す
        if (_currentSettings.Range <= 0)
        {
            return Vector2.zero;
        }

        // レンジに基づいて正規化（画面の範囲に収める）
        float normalizedX = horizontalPos.x / _currentSettings.Range;
        float normalizedY = horizontalPos.y / _currentSettings.Range;

        // レーダー画面の大きさに合わせてスケーリング
        float screenRadius = _radarScreen.rect.width * 0.5f;
        return new Vector2(normalizedX * screenRadius, normalizedY * screenRadius);
    }

    private void UpdateMarkerAppearance(GameObject marker, TrackedObjectDTO obj)
    {
        // 種類に応じたマーカーの外観設定
        var markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null)
        {
            // 所属(Allegiance)に応じた色設定
            switch (obj.Allegiance)
            {
                case "Friendly":
                    markerRenderer.material.color = Color.blue;
                    break;
                case "Hostile":
                    markerRenderer.material.color = Color.red;
                    break;
                case "Neutral":
                    markerRenderer.material.color = Color.green;
                    break;
                default:
                    markerRenderer.material.color = Color.white;
                    break;
            }
        }

        // 高度差インジケーター（上/下矢印）
        var altitudeIndicator = marker.transform.Find("AltitudeIndicator");
        if (altitudeIndicator != null)
        {
            float ownAltitude = _aircraftTransform.position.y;
            float targetAltitude = obj.Position.y;
            float altitudeDiff = targetAltitude - ownAltitude;

            var arrowUp = altitudeIndicator.Find("ArrowUp");
            var arrowDown = altitudeIndicator.Find("ArrowDown");

            if (arrowUp != null && arrowDown != null)
            {
                // 高度差に基づいて矢印表示
                if (altitudeDiff > 500)
                {
                    arrowUp.gameObject.SetActive(true);
                    arrowDown.gameObject.SetActive(false);
                }
                else if (altitudeDiff < -500)
                {
                    arrowUp.gameObject.SetActive(false);
                    arrowDown.gameObject.SetActive(true);
                }
                else
                {
                    arrowUp.gameObject.SetActive(false);
                    arrowDown.gameObject.SetActive(false);
                }
            }
        }

        // 速度ベクトル表示（目標の進行方向を示す線）
        var velocityVector = marker.transform.Find("VelocityVector");
        if (velocityVector != null && obj.Velocity.magnitude > 0.1f)
        {
            velocityVector.gameObject.SetActive(true);

            // 速度に応じた方向と長さ
            Vector3 normalizedVelocity = obj.Velocity.normalized;
            float speed = obj.Velocity.magnitude;

            // 2D平面での表示角度計算（水平面のみ）
            float angle = Mathf.Atan2(normalizedVelocity.z, normalizedVelocity.x) * Mathf.Rad2Deg;
            velocityVector.localRotation = Quaternion.Euler(0, 0, angle);

            // 速度に応じた長さ（スケーリング）
            float vectorLength = Mathf.Clamp(speed / 50f, 0.5f, 3f);
            velocityVector.localScale = new Vector3(vectorLength, 1f, 1f);
        }
        else if (velocityVector != null)
        {
            velocityVector.gameObject.SetActive(false);
        }

        // ターゲット情報ラベル更新
        var infoLabelTransform = marker.transform.Find("InfoLabel");
        if (infoLabelTransform != null)
        {
            var infoLabel = infoLabelTransform.GetComponent<Text>();
            if (infoLabel != null)
            {
                infoLabel.text = obj.Callsign;
            }
        }
    }

    private void UpdatePrimaryTargetDisplay()
    {
        var primaryTarget = _targetTrackingService.GetPrimaryTarget();
        if (primaryTarget != null && _trackMarkers.TryGetValue(primaryTarget.Id.Value, out var targetMarker))
        {
            // ロックマーカーの表示
            if (_targetLockBox != null)
            {
                _targetLockBox.SetActive(true);
                _targetLockBox.transform.position = targetMarker.transform.position;
            }

            // ロック状態を視覚的に強調
            var markerRenderer = targetMarker.GetComponent<Renderer>();
            if (markerRenderer != null)
            {
                markerRenderer.material.color = Color.yellow; // ロック状態を黄色で強調
            }
        }
        else
        {
            if (_targetLockBox != null)
            {
                _targetLockBox.SetActive(false);
            }
        }
    }

    private void ClearRangeCircles()
    {
        foreach (var circle in _rangeCircles)
        {
            Destroy(circle);
        }
        _rangeCircles.Clear();
    }

    private void CreateRangeCircles(float range)
    {
        // レーダーの最大範囲に合わせて同心円を生成
        int circleCount = 4; // 例: 4つの距離同心円

        for (int i = 1; i <= circleCount; i++)
        {
            float ratio = (float)i / circleCount;
            var circle = Instantiate(_rangeCirclePrefab, _radarScreen);

            // 同心円のサイズを設定
            RectTransform circleRect = circle.GetComponent<RectTransform>();
            float diameter = _radarScreen.rect.width * ratio;
            circleRect.sizeDelta = new Vector2(diameter, diameter);

            // スクリーン中央に配置
            circleRect.anchoredPosition = Vector2.zero;

            _rangeCircles.Add(circle);
        }
    }

    // マーカーのタップ/クリック処理（ターゲットロック）
    public void OnMarkerSelected(string targetId)
    {
        //【修正箇所】: _lockTargetUseCaseのnullチェックを追加
        if (_lockTargetUseCase == null)
        {
            Debug.LogWarning("_lockTargetUseCase is not assigned.");
            return;
        }

        var request = new TargetLockRequestDTO
        {
            TargetId = targetId
        };

        var result = _lockTargetUseCase.Execute(request);

        if (result.Success)
        {
            PlayLockSound();
            UpdatePrimaryTargetDisplay();
        }
    }

    #endregion

    #region Radar Settings Accessors

    protected override float GetCurrentRange()
    {
        return _currentSettings.Range;
    }

    protected override bool GetCurrentHeadingUp()
    {
        return _currentSettings.HeadingUp;
    }

    protected override bool GetCurrentECMActive()
    {
        return _currentSettings.ECMActive;
    }

    protected override string GetCurrentMode()
    {
        return _currentSettings.Mode.ToString();
    }

    #endregion
}


// AWACSレーダーコントローラー
public class AWACSRadarController : RadarController
{
    [SerializeField]
    private Transform _aircraftTransform;
    [SerializeField]
    private float _maxRange = 400f; // 非常に広い範囲
    [SerializeField]
    private float _detectionCone = 360f; // 全方位
    
    // レンダリング要素
    [SerializeField]
    private RectTransform _radarScreen;
    [SerializeField]
    private GameObject _trackMarkerPrefab;
    [SerializeField]
    private GameObject _rangeCirclePrefab;
    [SerializeField]
    private GameObject _sectorDividerPrefab;
    [SerializeField]
    private GameObject _dataLinkLinePrefab;
    
    private Dictionary<string, GameObject> _trackMarkers = new Dictionary<string, GameObject>();
    private List<GameObject> _rangeCircles = new List<GameObject>();
    private List<GameObject> _sectorDividers = new List<GameObject>();
    private Dictionary<string, GameObject> _dataLinkLines = new Dictionary<string, GameObject>();
    
    // 参照サービス
    private ITargetTrackingService _targetTrackingService;
    private IRadarSettingsRepository _settingsRepository;
    private IThreatAssessmentService _threatAssessmentService;
    
    // 現在の設定状態
    private RadarSettings _currentSettings;
    
    // 高度フィルター設定
    [SerializeField]
    private float _minAltitudeFilter = 0f;
    [SerializeField]
    private float _maxAltitudeFilter = 40000f;
    
    // データリンク設定
    [SerializeField]
    private bool _dataLinkActive = true;
    [SerializeField]
    private List<string> _dataLinkReceivers = new List<string>(); // データリンク先IDリスト
    
    protected override void Start()
    {
        base.Start();
        
        _targetTrackingService = FindObjectOfType<UdonTargetTrackingService>();
        _settingsRepository = FindObjectOfType<UdonRadarSettingsRepository>();
        _threatAssessmentService = FindObjectOfType<UdonThreatAssessmentService>();
        
        _currentSettings = _settingsRepository.GetCurrentSettings();
        
        InitializeRadarDisplay();
    }
    
    private void InitializeRadarDisplay()
    {
        // AWACSレーダー表示の初期化
        ClearRangeCircles();
        ClearSectorDividers();
        
        // 現在のレンジに基づいて新しいサークルを生成
        CreateRangeCircles(_currentSettings.Range);
        CreateSectorDividers();
    }
    
    protected override float GetDetectionCone()
    {
        return _detectionCone; // AWACS は360度全方位
    }
    
    protected override void UpdateRadarDisplay(RadarScanResultDTO scanResult)
    {
        // 既存のマーカーを更新し、不要なものをクリア
        List<string> activeIds = scanResult.DetectedObjects.Select(obj => obj.Id).ToList();
        
        foreach (var trackId in _trackMarkers.Keys.ToList())
        {
            if (!activeIds.Contains(trackId))
            {
                Destroy(_trackMarkers[trackId]);
                _trackMarkers.Remove(trackId);
            }
        }
        
        // 高度フィルターに基づいてフィルタリング
        var filteredObjects = scanResult.DetectedObjects.Where(obj => 
            obj.Altitude >= _minAltitudeFilter && obj.Altitude <= _maxAltitudeFilter).ToList();
        
        // マーカー更新
        foreach (var obj in filteredObjects)
        {
            Vector2 screenPosition = CalculateScreenPosition(obj, _aircraftTransform.position, _currentSettings.HeadingUp);
            
            if (_trackMarkers.TryGetValue(obj.Id, out var marker))
            {
                // 既存マーカーを更新
                marker.transform.localPosition = new Vector3(screenPosition.x, screenPosition.y, 0);
                UpdateMarkerAppearance(marker, obj);
            }
            else
            {
                // 新しいマーカーを作成
                var newMarker = Instantiate(_trackMarkerPrefab, _radarScreen);
                newMarker.transform.localPosition = new Vector3(screenPosition.x, screenPosition.y, 0);
                
                UpdateMarkerAppearance(newMarker, obj);
                
                _trackMarkers[obj.Id] = newMarker;
            }
        }
        
        // データリンク線の更新
        if (_dataLinkActive)
        {
            UpdateDataLinkLines(filteredObjects);
        }
        else
        {
            ClearDataLinkLines();
        }
        
        // 脅威評価表示の更新
        UpdateThreatDisplay();
    }
    
    private void UpdateMarkerAppearance(GameObject marker, TrackedObjectDTO obj)
    {
        // AWACS特有のマーカー表示ロジック
        var markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null)
        {
            // 所属(Allegiance)に応じた色設定 - AWACS用強調カラー
            switch (obj.Allegiance)
            {
                case "Friendly":
                    markerRenderer.material.color = new Color(0.2f, 0.5f, 1.0f); // 濃い青
                    break;
                case "Hostile":
                    markerRenderer.material.color = new Color(1.0f, 0.2f, 0.2f); // 濃い赤
                    break;
                case "Neutral":
                    markerRenderer.material.color = new Color(0.2f, 0.8f, 0.2f); // 濃い緑
                    break;
                default:
                    markerRenderer.material.color = new Color(0.8f, 0.8f, 0.2f); // 黄色 (不明)
                    break;
            }
        }
        
        // 高度情報ラベル
        var altitudeLabel = marker.transform.Find("AltitudeLabel")?.GetComponent<Text>();
        if (altitudeLabel != null)
        {
            // フィート表示
            float altitudeInFeet = obj.Altitude * 3.28084f;
            altitudeLabel.text = $"{Mathf.RoundToInt(altitudeInFeet / 100) * 100}";
        }
        
        // トラックタイプに応じたアイコン変更
        var typeIcon = marker.transform.Find("TypeIcon");
        if (typeIcon != null)
        {
            // 各種アイコンの表示/非表示を切り替え
            typeIcon.Find("Aircraft")?.gameObject.SetActive(obj.Type == "Aircraft");
            typeIcon.Find("Ship")?.gameObject.SetActive(obj.Type == "Ship");
            typeIcon.Find("Missile")?.gameObject.SetActive(obj.Type == "Missile");
        }
        
        // 速度ベクトル表示
        var velocityVector = marker.transform.Find("VelocityVector");
        if (velocityVector != null && obj.Velocity.magnitude > 0.1f)
        {
            velocityVector.gameObject.SetActive(true);
            
            // 速度に応じた方向と長さ
            Vector3 normalizedVelocity = obj.Velocity.normalized;
            float speed = obj.Velocity.magnitude;
            
            // 2D平面での表示角度計算
            float angle = Mathf.Atan2(normalizedVelocity.z, normalizedVelocity.x) * Mathf.Rad2Deg;
            velocityVector.localRotation = Quaternion.Euler(0, 0, angle);
            
            // 速度に応じた長さ（AWACSは速度表示を強調）
            float vectorLength = Mathf.Clamp(speed / 100f, 0.5f, 5f);
            velocityVector.localScale = new Vector3(vectorLength, 1f, 1f);
        }
        else if (velocityVector != null)
        {
            velocityVector.gameObject.SetActive(false);
        }
        
        // 詳細情報ラベル (AWACS特有の詳細表示)
        var infoLabel = marker.transform.Find("InfoLabel")?.GetComponent<Text>();
        if (infoLabel != null)
        {
            float speedInKnots = obj.Velocity.magnitude * 1.94384f; // m/s から knots へ変換
            infoLabel.text = $"{obj.Callsign}\n{Mathf.RoundToInt(speedInKnots)}kts";
        }
    }
    
    private void UpdateThreatDisplay()
    {
        // 脅威レベルに応じたハイライト表示
        var threats = _threatAssessmentService.GetThreatsByPriority(_aircraftTransform.position);
        
        foreach (var threat in threats)
        {
            if (_trackMarkers.TryGetValue(threat.Id.Value, out var marker))
            {
                var threatLevel = _threatAssessmentService.AssessThreat(threat, _aircraftTransform.position);
                
                // 脅威レベルインジケーターの更新
                var threatIndicator = marker.transform.Find("ThreatIndicator");
                if (threatIndicator != null)
                {
                    // 脅威レベルに応じた表示
                    threatIndicator.gameObject.SetActive(threatLevel >= ThreatLevel.Medium);
                    
                    // 脅威レベルに応じた色
                    var indicatorRenderer = threatIndicator.GetComponent<Renderer>();
                    if (indicatorRenderer != null)
                    {
                        switch (threatLevel)
                        {
                            case ThreatLevel.Critical:
                                indicatorRenderer.material.color = Color.red;
                                break;
                            case ThreatLevel.High:
                                indicatorRenderer.material.color = new Color(1.0f, 0.5f, 0f); // オレンジ
                                break;
                            case ThreatLevel.Medium:
                                indicatorRenderer.material.color = Color.yellow;
                                break;
                        }
                    }
                }
            }
        }
    }
    
    private void UpdateDataLinkLines(List<TrackedObjectDTO> targets)
    {
        // データリンク線の更新 (味方機へのデータ共有表示)
        ClearDataLinkLines();
        
        // データリンク先がある場合のみ表示
        if (_dataLinkReceivers.Count == 0)
            return;
            
        foreach (var receiverId in _dataLinkReceivers)
        {
            if (_trackMarkers.TryGetValue(receiverId, out var receiverMarker))
            {
                // プライマリターゲットへのデータリンク
                var primaryTarget = _targetTrackingService.GetPrimaryTarget();
                if (primaryTarget != null && _trackMarkers.TryGetValue(primaryTarget.Id.Value, out var targetMarker))
                {
                    // データリンク線の作成
                    var linkLine = Instantiate(_dataLinkLinePrefab, _radarScreen);
                    
                    // 線の位置とサイズを設定
                    SetupDataLinkLine(linkLine, receiverMarker.transform.localPosition, targetMarker.transform.localPosition);
                    
                    // 保存
                    _dataLinkLines[receiverId + "_" + primaryTarget.Id.Value] = linkLine;
                }
                
                // 脅威ターゲットへのデータリンク表示 (上位3つまで)
                var threats = _threatAssessmentService.GetThreatsByPriority(_aircraftTransform.position)
                    .Take(3)
                    .ToList();
                    
                foreach (var threat in threats)
                {
                    if (_trackMarkers.TryGetValue(threat.Id.Value, out var threatMarker))
                    {
                        // データリンク線の作成
                        var linkLine = Instantiate(_dataLinkLinePrefab, _radarScreen);
                        
                        // 線の位置とサイズを設定
                        SetupDataLinkLine(linkLine, receiverMarker.transform.localPosition, threatMarker.transform.localPosition, true);
                        
                        // 保存
                        _dataLinkLines[receiverId + "_" + threat.Id.Value] = linkLine;
                    }
                }
            }
        }
    }
    
    private void SetupDataLinkLine(GameObject linkLine, Vector3 start, Vector3 end, bool isThreat = false)
    {
        // 線の描画パラメータを設定
        var lineRenderer = linkLine.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            
            // 脅威ターゲットへのリンクか通常リンクかで色を変える
            if (isThreat)
            {
                lineRenderer.startColor = new Color(1f, 0.5f, 0f, 0.7f); // オレンジ (半透明)
                lineRenderer.endColor = new Color(1f, 0.5f, 0f, 0.7f);
            }
            else
            {
                lineRenderer.startColor = new Color(0f, 0.8f, 1f, 0.7f); // 水色 (半透明)
                lineRenderer.endColor = new Color(0f, 0.8f, 1f, 0.7f);
            }
        }
    }
    
    private void ClearDataLinkLines()
    {
        foreach (var line in _dataLinkLines.Values)
        {
            Destroy(line);
        }
        _dataLinkLines.Clear();
    }
    
    private Vector2 CalculateScreenPosition(TrackedObjectDTO obj, Vector3 radarPos, bool headingUp)
    {
        // 対象の相対位置（2D水平面）
        Vector3 relativePos = obj.Position - radarPos;
        
        // 水平面上の位置のみを使用
        Vector2 horizontalPos = new Vector2(relativePos.x, relativePos.z);
        
        // ヘディングアップの場合、自機の向きに対して回転
        if (headingUp)
        {
            float angle = -_aircraftTransform.eulerAngles.y * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            horizontalPos = new Vector2(
                horizontalPos.x * cos - horizontalPos.y * sin,
                horizontalPos.x * sin + horizontalPos.y * cos
            );
        }
        
        // レンジに基づいて正規化
        float normalizedX = horizontalPos.x / _currentSettings.Range;
        float normalizedY = horizontalPos.y / _currentSettings.Range;
        
        // レーダー画面の大きさに合わせてスケーリング
        float screenRadius = _radarScreen.rect.width * 0.5f;
        return new Vector2(normalizedX * screenRadius, normalizedY * screenRadius);
    }
    
    private void ClearRangeCircles()
    {
        foreach (var circle in _rangeCircles)
        {
            Destroy(circle);
        }
        _rangeCircles.Clear();
    }
    
    private void CreateRangeCircles(float range)
    {
        // AWACSレーダーは複数の距離リング表示
        int circleCount = 5; // AWACS用に5つの距離同心円
        
        for (int i = 1; i <= circleCount; i++)
        {
            float ratio = (float)i / circleCount;
            var circle = Instantiate(_rangeCirclePrefab, _radarScreen);
            
            // 同心円のサイズを設定
            RectTransform circleRect = circle.GetComponent<RectTransform>();
            float diameter = _radarScreen.rect.width * ratio;
            circleRect.sizeDelta = new Vector2(diameter, diameter);
            
            // スクリーン中央に配置
            circleRect.anchoredPosition = Vector2.zero;
            
            // 距離ラベル追加 (AWACSは距離表示が重要)
            var distanceLabel = circle.transform.Find("DistanceLabel")?.GetComponent<Text>();
            if (distanceLabel != null)
            {
                float distanceKm = range * ratio;
                distanceLabel.text = $"{distanceKm:F0}km";
            }
            
            _rangeCircles.Add(circle);
        }
    }
    
    private void ClearSectorDividers()
    {
        foreach (var divider in _sectorDividers)
        {
            Destroy(divider);
        }
        _sectorDividers.Clear();
    }
    
    private void CreateSectorDividers()
    {
        // AWACS特有のセクター区分線の描画
        int sectorCount = 8; // 8セクターに分割
        
        for (int i = 0; i < sectorCount; i++)
        {
            float angle = 360f / sectorCount * i;
            var divider = Instantiate(_sectorDividerPrefab, _radarScreen);
            
            // 線の回転角度設定
            divider.transform.localRotation = Quaternion.Euler(0, 0, angle);
            
            // 線の長さ設定（画面いっぱい）
            var rect = divider.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(_radarScreen.rect.width, rect.sizeDelta.y);
            }
            
            _sectorDividers.Add(divider);
        }
    }
    
    // 高度フィルター設定UI用
    public void SetMinAltitudeFilter(float minAltitude)
    {
        _minAltitudeFilter = minAltitude;
    }
    
    public void SetMaxAltitudeFilter(float maxAltitude)
    {
        _maxAltitudeFilter = maxAltitude;
    }
    
    // データリンク制御UI用
    public void ToggleDataLink()
    {
        _dataLinkActive = !_dataLinkActive;
    }
    
    public void AddDataLinkReceiver(string receiverId)
    {
        if (!_dataLinkReceivers.Contains(receiverId))
        {
            _dataLinkReceivers.Add(receiverId);
        }
    }
    
    public void RemoveDataLinkReceiver(string receiverId)
    {
        _dataLinkReceivers.Remove(receiverId);
    }
    
    protected override float GetCurrentRange()
    {
        return _currentSettings.Range;
    }
    
    protected override bool GetCurrentHeadingUp()
    {
        return _currentSettings.HeadingUp;
    }
    
    protected override bool GetCurrentECMActive()
    {
        return _currentSettings.ECMActive;
    }
    
    protected override string GetCurrentMode()
    {
        return _currentSettings.Mode.ToString();
    }
}

// 艦船レーダーコントローラーベース (空母・護衛艦共通部分)
public abstract class ShipRadarController : RadarController
{
    [SerializeField]
    protected Transform _shipTransform;
    
    // レンダリング要素
    [SerializeField]
    protected RectTransform _radarScreen;
    [SerializeField]
    protected GameObject _trackMarkerPrefab;
    [SerializeField]
    protected GameObject _rangeCirclePrefab;
    [SerializeField]
    protected GameObject _bearingLinePrefab;
    
    protected Dictionary<string, GameObject> _trackMarkers = new Dictionary<string, GameObject>();
    protected List<GameObject> _rangeCircles = new List<GameObject>();
    protected List<GameObject> _bearingLines = new List<GameObject>();
    
    // 参照サービス
    protected ITargetTrackingService _targetTrackingService;
    protected IRadarSettingsRepository _settingsRepository;
    
    // 現在の設定状態
    protected RadarSettings _currentSettings;
    
    // 全方位スキャン
    protected override float GetDetectionCone()
    {
        return 360f; // 艦船は全方位
    }
    
    protected virtual void InitializeShipRadarDisplay()
    {
        ClearRangeCircles();
        ClearBearingLines();
        
        CreateRangeCircles(_currentSettings.Range);
        CreateBearingLines();
    }
    
    protected virtual void ClearRangeCircles()
    {
        foreach (var circle in _rangeCircles)
        {
            Destroy(circle);
        }
        _rangeCircles.Clear();
    }
    
    protected virtual void CreateRangeCircles(float range)
    {
        // レーダーの最大範囲に合わせて同心円を生成
        int circleCount = 4;
        
        for (int i = 1; i <= circleCount; i++)
        {
            float ratio = (float)i / circleCount;
            var circle = Instantiate(_rangeCirclePrefab, _radarScreen);
            
            RectTransform circleRect = circle.GetComponent<RectTransform>();
            float diameter = _radarScreen.rect.width * ratio;
            circleRect.sizeDelta = new Vector2(diameter, diameter);
            circleRect.anchoredPosition = Vector2.zero;
            
            _rangeCircles.Add(circle);
        }
    }
    
    protected virtual void ClearBearingLines()
    {
        foreach (var line in _bearingLines)
        {
            Destroy(line);
        }
        _bearingLines.Clear();
    }
    
    protected virtual void CreateBearingLines()
    {
        // 方位線の描画 (艦船レーダーは方位表示が重要)
        int bearingCount = 8; // 8方位
        
        for (int i = 0; i < bearingCount; i++)
        {
            float angle = 360f / bearingCount * i;
            var line = Instantiate(_bearingLinePrefab, _radarScreen);
            
            line.transform.localRotation = Quaternion.Euler(0, 0, angle);
            
            var rect = line.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(_radarScreen.rect.width, rect.sizeDelta.y);
            }
            
            // 方位ラベル配置
            var bearingLabel = line.transform.Find("BearingLabel")?.GetComponent<Text>();
            if (bearingLabel != null)
            {
                // 方位角をフォーマット (000, 045, 090, etc.)
                bearingLabel.text = angle.ToString("000");
                
                // ラベルの位置調整
                float labelRadius = _radarScreen.rect.width * 0.45f; // 画面端に近い位置
                float labelX = Mathf.Sin(angle * Mathf.Deg2Rad) * labelRadius;
                float labelY = Mathf.Cos(angle * Mathf.Deg2Rad) * labelRadius;
                bearingLabel.transform.localPosition = new Vector3(labelX, labelY, 0);
            }
            
            _bearingLines.Add(line);
        }
    }
    
    protected Vector2 CalculateScreenPosition(TrackedObjectDTO obj, Vector3 radarPos, bool headingUp)
    {
        // 対象の相対位置（2D水平面）
        Vector3 relativePos = obj.Position - radarPos;
        
        // 水平面上の位置のみを使用
        Vector2 horizontalPos = new Vector2(relativePos.x, relativePos.z);
        
        // ヘディングアップの場合、艦の向きに対して回転
        if (headingUp)
        {
            float angle = -_shipTransform.eulerAngles.y * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            horizontalPos = new Vector2(
                horizontalPos.x * cos - horizontalPos.y * sin,
                horizontalPos.x * sin + horizontalPos.y * cos
            );
        }
        
        // レンジに基づいて正規化
        float normalizedX = horizontalPos.x / _currentSettings.Range;
        float normalizedY = horizontalPos.y / _currentSettings.Range;
        
        // レーダー画面の大きさに合わせてスケーリング
        float screenRadius = _radarScreen.rect.width * 0.5f;
        return new Vector2(normalizedX * screenRadius, normalizedY * screenRadius);
    }
}
