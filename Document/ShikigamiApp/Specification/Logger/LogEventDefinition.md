# ShikigamiApp ログイベント定義

## 1. 目的

本書は、Guiの診断ログとして記録するイベントの識別子、重要度、発生条件および補足項目を定義する。

ログの書式、保存先、書き込み失敗時の扱いについては`LoggerSpecification.md`に従う。

## 2. 記録対象の判断基準

次の条件に該当する処理を記録対象とする。

- ユーザーが明示的に実行した主要操作の開始と完了
- 障害解析で進行地点を特定するために必要な完了地点
- 保存データや式神マスターデータへの永続的な変更
- 検証による操作の拒否、例外による失敗、失敗からの復旧
- 計算結果の保存可能性に影響する主要状態変化
- アプリケーションの起動と終了

内部Controlの個別イベント、繰り返し発生する値の再設定、単なる画面描画、および計測専用の細粒度処理は原則として記録しない。

## 3. イベント命名規則

- `Operation`は機能単位の識別子を使用する（例：`Calculation`、`SaveData.Load`）。
- `Event`は`Started`、`Completed`、`Failed`、`ValidationRejected`など、発生した事実を表す識別子を使用する。
- 失敗結果を表す列挙値が存在する場合は`Outcome`に記録する。
- 操作種別等の追加情報は`Key=Value`形式とする。
- 同じ意味のイベントに複数の命名を使用しない。

## 4. アプリケーション

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `Application` | `Started` | INFO | 初期化処理を開始するとき | なし |
| `Application` | `Initialized` | INFO | 必要な初期化が正常終了し、MainFormが操作可能になったとき | なし |
| `Application` | `InitializationFailed` | ERROR | 起動に必要な初期化が失敗したとき | `Exception`または`Outcome` |
| `Application` | `Closing` | INFO | アプリケーションの正常終了処理を開始するとき | なし |
| `Application` | `Closed` | INFO | 終了処理の完了地点に到達したとき | なし |

## 5. MainForm操作状態

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `MainForm.State` | `Changed` | INFO | 操作状態が実際に変更されたとき | `Previous`、`Current` |

同一状態への設定要求は記録しない。状態の値のみでなく、保存・読み込み・CLEAR処理の完了イベントと組み合わせて調査する。

## 6. ステータス計算

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `Calculation` | `Started` | INFO | 計算ボタンから計算処理を開始したとき | `ShikigamiSelected` |
| `Calculation` | `ValidationRejected` | WARNING | 計算入力の検証に失敗し、計算を実行しなかったとき | `Outcome` |
| `Calculation` | `Completed` | INFO | 計算結果の保持・必要な状態変更・画面表示が完了したとき | `ShikigamiSelected` |
| `Calculation` | `Failed` | ERROR | 計算処理中に予期しない例外が発生したとき | `Exception` |
| `Calculation.Result` | `DirtyChanged` | INFO | 計算結果の状態がCleanからDirtyへ変更されたとき | `Current=Dirty` |
| `Calculation.Result` | `Invalidated` | WARNING | ロールバック失敗等により、既存の計算結果を強制的に無効化したとき | `Reason` |

`Calculation.Completed`はGatewayの計算完了だけで記録せず、Guiでの結果反映が完了した後に記録する。御魂のみの計算でも正常完了時は記録し、`ShikigamiSelected=False`によって区別する。

`DirtyChanged`は毎回の入力変更を記録するイベントではない。すでにDirtyの状態で入力を変更しても新たには記録しない。

## 7. SaveData保存

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `SaveData.Save` | `Started` | INFO | 保存ダイアログで確定し、実際の保存処理を開始するとき | `SaveType` |
| `SaveData.Save` | `Completed` | INFO | 指定された形式の保存処理が正常に終了したとき | `SaveType`、`FileName` |
| `SaveData.Save` | `Failed` | ERROR | 保存前提条件、変換、ファイル書き込みなどにより保存が失敗したとき | `SaveType`、`Exception`または`Outcome` |

`SaveType`は`Build`、`MitamaSet`、`CalculationSnapshot`のいずれかとする。

保存ダイアログをキャンセルした場合は保存操作として記録しない。保存ボタンの有効状態を評価しただけで`Started`を出力しない。`Completed`はファイル保存の成功後に記録する。

## 8. SaveData読み込み・適用

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `SaveData.Load` | `Started` | INFO | 読み込みダイアログで確定し、実際の読み込みを開始するとき | `LoadType` |
| `SaveData.Load` | `Validated` | INFO | ファイル読み取り、変換、入力検証がすべて完了したとき | `LoadType` |
| `SaveData.Load` | `Completed` | INFO | 検証済みデータのGuiへの適用が正常終了したとき | `LoadType` |
| `SaveData.Load` | `Failed` | ERROR | 読み込み・検証・適用が失敗したとき | `LoadType`、`Exception`または`Outcome` |
| `SaveData.Rollback` | `Started` | WARNING | 適用失敗後、以前のGui状態への復元を開始するとき | なし |
| `SaveData.Rollback` | `Completed` | INFO | 以前のGui状態への復元を完了したとき | なし |
| `SaveData.Rollback` | `Failed` | ERROR | 以前のGui状態への復元が失敗したとき | `Exception` |

`LoadType`は`Build`または`MitamaSet`とする。読み込みダイアログのキャンセル時には`Started`を記録しない。

`SaveData.Load.Completed`はファイル読み取りだけの成功ではなく、Guiへの適用が完了したことを示す。

ロールバック失敗では、ロールバック固有の失敗と読み込み操作全体の失敗を区別できるよう記録する。ただし、同じ例外の全文を複数箇所で重複して出力しない。

## 9. Snapshot比較

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `Snapshot.Compare` | `Started` | INFO | 2つのSnapshotを選択し、比較処理を開始したとき | なし |
| `Snapshot.Compare` | `Completed` | INFO | 両Snapshotの読み込み・比較・結果表示が正常終了したとき | なし |
| `Snapshot.Compare` | `Failed` | ERROR | Snapshotの読込異常、データ不正、比較処理中の例外が発生したとき | `Outcome`または`Exception` |

比較の数値やSnapshotのJSON全文は出力しない。

## 10. 式神データの登録・編集

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `Shikigami.Register` | `Started` | INFO | 登録操作を確定し、データ更新を開始するとき | なし |
| `Shikigami.Register` | `Completed` | INFO | 登録データの更新処理が正常終了したとき | なし |
| `Shikigami.Register` | `ValidationRejected` | WARNING | 登録内容の入力検証に失敗したとき | `Outcome`または`Reason` |
| `Shikigami.Register` | `Failed` | ERROR | 登録中に例外またはエラーOutcomeが発生したとき | `Outcome`または`Exception` |
| `Shikigami.Edit` | `Started` | INFO | 編集操作を確定し、データ更新を開始するとき | なし |
| `Shikigami.Edit` | `Completed` | INFO | 編集データの更新処理が正常終了したとき | なし |
| `Shikigami.Edit` | `ValidationRejected` | WARNING | 編集内容の入力検証に失敗したとき | `Outcome`または`Reason` |
| `Shikigami.Edit` | `Failed` | ERROR | 編集中に例外またはエラーOutcomeが発生したとき | `Outcome`または`Exception` |

入力検証で確定操作を実行していない場合は`Started`を出さない。単なる入力欄の変更やフォーカス移動は記録しない。

## 11. 式神データの再読み込み・自動復元・手動復旧

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `Shikigami.Reload` | `Started` | INFO | 式神データの再読み込みを開始したとき | なし |
| `Shikigami.Reload` | `Completed` | INFO | 読み込みとGuiの候補一覧更新が完了したとき | なし |
| `Shikigami.Reload` | `Failed` | ERROR | 読み込みまたはGui更新に失敗したとき | `Outcome`または`Exception` |
| `Shikigami.AutoRestore` | `Completed` | INFO | 埋め込みデータからの既定CSV復元が完了したとき | なし |
| `Shikigami.AutoRestore` | `Failed` | ERROR | 既定CSVの復元に失敗したとき | `Outcome`または`Exception` |
| `Shikigami.BrokenFile` | `Moved` | INFO | 破損CSVをBrokenディレクトリへ退避したとき | `FileName` |
| `Shikigami.Recovery` | `Started` | INFO | 選択した式神データの手動復旧を開始したとき | `SelectedCount` |
| `Shikigami.Recovery` | `BackupCompleted` | INFO | 復旧前データのバックアップ作成が完了したとき | なし |
| `Shikigami.Recovery` | `Completed` | INFO | 復旧データの反映と後続のGui更新が完了したとき | `RecoveredCount` |
| `Shikigami.Recovery` | `Failed` | ERROR | 候補の取得・バックアップ作成・復旧処理・Gui更新に失敗したとき | `Outcome`または`Exception` |

正常完了イベントは、対象とするファイル操作やGui更新処理の完了後に記録する。

復旧候補一覧の取得自体はデータ更新ではないため、候補の個別要素を全件ログに記録しない。

## 12. CLEAR

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `MainForm.Clear` | `Started` | INFO | CLEAR確認ダイアログで確定したとき | なし |
| `MainForm.Clear` | `Completed` | INFO | 入力初期化と計算結果破棄が完了したとき | なし |
| `MainForm.Clear` | `Failed` | ERROR | CLEAR処理中に予期しない例外が発生したとき | `Exception` |

CLEARの確認をキャンセルした場合は`Started`を出力しない。

## 13. 出力位置の責務

- 正常完了イベントは、成功条件を判断できる処理境界で記録する。
- 保存データの変換処理やControl設定処理など、内部の小さなメソッドごとに正常系ログを追加しない。
- エラーイベントは原則として利用者へ失敗を報告する境界で出力し、同じ障害を複数の層で繰り返し記録しない。
- 途中で例外が発生した場合、それより後の正常完了イベントを記録しない。
- 記録したイベントが状態変更や例外処理の実行条件となってはならない。

## 14. テスト観点

- 主要操作の正常系で`Started`と`Completed`が記録されること。
- 検証失敗時は適切な`ValidationRejected`が記録され、`Completed`が出ないこと。
- 例外発生時は`Failed`が記録され、同一操作が正常完了扱いにならないこと。
- SaveData読み込みの`Validated`と`Completed`が、それぞれ検証完了・Gui適用完了に対応すること。
- ロールバック成功と失敗が区別されること。
- `Calculation.Result.DirtyChanged`がCleanからDirtyへの変更時だけ記録されること。
- ダイアログのキャンセルや単純な入力変更によって大量の操作ログが生成されないこと。
- Loggerの書き込み失敗時でも、ユーザー通知および操作状態の復帰が実行されること。

## 15. 関連資料

- `LoggerSpecification.md`：Logger共通仕様
- `../Calculation/CalculationResultLifecycle.md`：計算結果ライフサイクル
- `../../Diagram/SaveData/save_sequence.wsd`：SaveData保存のシーケンス図
- `../../Diagram/SaveData/load_sequence.wsd`：SaveData読み込みのシーケンス図
- `../../Diagram/ShikigamiData/shikigami_recovery_sequence.wsd`：式神復旧のシーケンス図
