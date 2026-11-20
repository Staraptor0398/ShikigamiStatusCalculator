# ShikigamiApp ログイベント定義

## 1. 目的

本書は、Guiの診断ログとして記録するイベントの識別子、重要度、発生条件および補足項目を定義する。

ログ書式、SessionId、保存先、Logger / LogWriterの責務、書き込み失敗時の扱いについては`LoggerSpecification.md`に従う。

## 2. 記録対象の判断基準

次の条件に該当する処理を記録対象とする。

- ユーザーが明示的に実行した主要操作の開始と完了
- 障害解析で進行地点を特定するために必要な完了地点
- 保存データや式神マスターデータへの永続的な変更
- 検証による操作の拒否、例外による失敗、失敗からの復旧
- 計算結果の保存可能性に影響する主要状態変化
- アプリケーションの起動と終了

内部Controlの個別イベント、繰り返し発生する値の再設定、単なる画面描画、および計測専用の細粒度処理は原則として記録しない。

## 3. イベント命名・記録規則

- `Operation`は機能単位の識別子を使用する（例：`Calculation`、`SaveData.Load`）。ユーザー向けの日本語ダイアログ名称とログ識別子は区別する。
- `Event`は`Started`、`Completed`、`Failed`、`ValidationRejected`など、発生した事実を表す識別子を使用する。
- 失敗結果を表す列挙値が存在する場合は`Outcome`に記録する。
- 操作種別等の追加情報は`Key=Value`形式とし、該当イベント定義に従う。
- 同じ意味のイベントに複数の命名を使用しない。
- `Started`と正常完了を示す`Completed`は対象処理の実際の開始・成功地点で出力する。入力検証で拒否された操作や途中で失敗した操作を正常完了扱いにしない。
- `Failed`は操作としての最終的な失敗を示す。途中の障害から正常に復旧する経路は、復旧可能な事象と最終結果を区別する。
- 例外を記録するときは例外専用のLogger APIを利用する。すでにErrorとして記録した例外の全文を、別の層で重複して記録しない。
- 状態・結果の変化だけで処理完了を示せない場合、対象操作の境界で成否を確認してから記録する。

## 4. アプリケーション

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `Application` | `Started` | INFO | アプリケーションの初期化を開始するとき | なし |
| `Application` | `Initialized` | INFO | 必要な初期化が正常終了し、MainFormが操作可能になったとき | なし |
| `Application` | `InitializationFailed` | ERROR | 起動に必要な初期化が失敗したとき | `Exception`または`Outcome` |
| `Application` | `Closing` | INFO | キャンセルされない正常終了処理へ進むとき | なし |
| `Application` | `ShutdownCheckpoint` | INFO | 終了過程の主要な通過地点に到達したとき | `Phase` |
| `Application` | `Closed` | INFO | `Application.Run`の正常な終了地点に到達したとき | なし |
| `PerformanceLog` | `Failed` | ERROR | PerformanceLoggerによる性能ログ出力が失敗したとき | `Exception` |

`ShutdownCheckpoint`は、終了処理での到達地点を診断するための補助イベントとする。`Phase`は以下の既存チェックポイント名を使用する。

```text
OnFormClosing.Begin
OnFormClosing.End
OnFormClosed.Begin
PerformanceLogger.Write.Begin
PerformanceLogger.Write.End
Base.OnFormClosed.Begin
Base.OnFormClosed.End
```

`ShutdownCheckpoint`は終了処理の正常完了を意味しない。`Application.Closing`および`Application.Closed`に置き換えてはならない。終了がキャンセルされた場合は`Closed`を記録しない。

`PerformanceLog.Failed`は性能計測結果そのものの出力ではなく、Guiの診断イベントである。PerformanceTrackerの集計値は診断ログに複製しない。

## 5. MainForm操作状態

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `MainForm.State` | `Changed` | INFO | 操作状態が実際に変更されたとき | `Previous`、`Current` |

同一状態への設定要求は記録しない。状態変更だけでは保存・読み込み・CLEAR等の操作完了を示せないため、各操作の終端イベントと組み合わせて調査する。

## 6. ステータス計算

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `Calculation` | `Started` | INFO | 計算ボタンから計算処理を開始したとき | `ShikigamiSelected` |
| `Calculation` | `ValidationRejected` | WARNING | 計算入力の検証に失敗し、計算を実行しなかったとき | `Outcome` |
| `Calculation` | `Completed` | INFO | 計算結果の保持・必要な状態変更・画面表示が完了したとき | `ShikigamiSelected` |
| `Calculation` | `Failed` | ERROR | 計算実行または結果反映中に予期しない例外が発生したとき | `Exception` |
| `Calculation.Result` | `DirtyChanged` | INFO | 計算結果の状態がCleanからDirtyへ変更されたとき | `Current=Dirty` |
| `Calculation.Result` | `Invalidated` | WARNING | ロールバック失敗等により、既存の計算結果を強制的に無効化したとき | `Reason` |
| `Calculation.TestSource` | `Failed` | ERROR | DEBUG用計算テストソースの出力に失敗したとき | `Exception` |

`Calculation.Completed`はGatewayの計算完了だけで記録せず、Guiへの結果反映が完了した後に記録する。御魂のみの計算でも正常完了時は記録し、`ShikigamiSelected=False`で区別する。

`DirtyChanged`はCleanからDirtyに遷移した場合のみ記録する。すでにDirtyの状態で入力を変更しても新たには記録しない。

`Invalidated`は単なるDirty化とは異なり、計算結果が強制破棄されたことを記録する。`Calculation.TestSource`はDEBUG環境における補助的な出力操作であり、通常計算の結果とは区別する。

## 7. SaveData保存

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `SaveData.Save` | `Started` | INFO | 保存ダイアログで確定し、実際の保存処理を開始するとき | `SaveType` |
| `SaveData.Save` | `Completed` | INFO | 指定された形式の保存が正常終了したとき | `SaveType`、`FileName` |
| `SaveData.Save` | `Failed` | ERROR | 保存前提条件、変換、ファイル書き込み等により保存が失敗したとき | `SaveType`、`Exception`または`Outcome` |

`SaveType`は`Build`、`MitamaSet`、`CalculationSnapshot`のいずれかとする。

保存ダイアログをキャンセルした場合は保存操作として記録しない。保存ボタンの有効状態を評価しただけで`Started`を出力しない。`Completed`はファイル保存の成功後に記録する。

## 8. SaveData読み込み・適用・ロールバック

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `SaveData.Load` | `Started` | INFO | 読み込みダイアログで確定し、実際の読み込みを開始するとき | `LoadType` |
| `SaveData.Load` | `Validated` | INFO | ファイル読み取り、変換、入力検証がすべて完了したとき | `LoadType` |
| `SaveData.Load` | `Completed` | INFO | 検証済みデータのGui適用と後続の状態・計算結果更新が正常終了したとき | `LoadType` |
| `SaveData.Load` | `Failed` | ERROR | 読み込み・検証・適用が最終的に失敗したとき | `LoadType`、`Exception`または`Outcome` |
| `SaveData.Rollback` | `Started` | WARNING | 適用失敗後、以前のGui状態への復元を開始するとき | なし |
| `SaveData.Rollback` | `Completed` | INFO | 以前のGui状態への復元を完了したとき | なし |
| `SaveData.Rollback` | `Failed` | ERROR | 以前のGui状態への復元が失敗したとき | `Exception` |

`LoadType`は`Build`または`MitamaSet`とする。読み込みダイアログのキャンセル時には`Started`を記録しない。

`Validated`は`SaveDataLoadFunction.Load()`による検証完了後、Guiへの適用前に記録する。`Completed`は読み取り成功のみでは記録せず、Guiへの適用と必要な状態更新が正常終了してから記録する。

ロールバックが成功しても、元の`SaveData.Load`は`Failed`として記録する。ロールバック失敗時は`SaveData.Rollback.Failed`と`SaveData.Load.Failed`を区別する。ただし、同じ例外全文を複数のErrorレコードに重複して書かず、読み込み側には失敗結果、ロールバック側には固有の例外を記録するなど、原因と結果を識別できる形にする。

ロールバック失敗により既存の計算結果を破棄した場合は、別途`Calculation.Result.Invalidated`を記録する。

## 9. Snapshot比較

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `Snapshot.Compare` | `Started` | INFO | 2つのSnapshotを選択し、比較処理を開始したとき | なし |
| `Snapshot.Compare` | `Completed` | INFO | 両Snapshotの読み込み・比較・比較結果フォームの表示が正常終了したとき | なし |
| `Snapshot.Compare` | `Failed` | ERROR | 読込異常、データ不正、比較処理または結果表示中の例外が発生したとき | `Outcome`または`Exception` |

`SnapshotComparisonOutcome.SNAPSHOT_NULL`と`SnapshotComparisonOutcome.STATUS_NULL`はいずれも比較の正常完了ではないため、`Failed`に結果コードを記録する。比較の数値やSnapshotのJSON全文は出力しない。

## 10. 式神データの登録・編集

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `Shikigami.Register` | `Started` | INFO | 登録内容の検証を通過し、データ更新を開始するとき | なし |
| `Shikigami.Register` | `Completed` | INFO | 登録データの更新処理が正常終了したとき | なし |
| `Shikigami.Register` | `ValidationRejected` | WARNING | 登録内容の入力検証または重複検証に失敗したとき | `Outcome`または`Reason` |
| `Shikigami.Register` | `Failed` | ERROR | 登録中に例外またはエラーOutcomeが発生したとき | `Outcome`または`Exception` |
| `Shikigami.Edit` | `Started` | INFO | 編集内容の検証を通過し、データ更新を開始するとき | なし |
| `Shikigami.Edit` | `Completed` | INFO | 編集データの更新処理が正常終了したとき | なし |
| `Shikigami.Edit` | `ValidationRejected` | WARNING | 編集内容の入力検証または重複検証に失敗したとき | `Outcome`または`Reason` |
| `Shikigami.Edit` | `Failed` | ERROR | 編集中に例外またはエラーOutcomeが発生したとき | `Outcome`または`Exception` |

確定操作に必要な入力検証を通過しなかった場合は`Started`を出さない。重複検証による拒否は`ValidationRejected`とする。単なる入力欄の変更やフォーカス移動は記録しない。

`Completed`はマスターCSVへの更新が成功したことを示し、親フォームの候補一覧の再読み込み成功までを意味しない。候補一覧の更新は`Shikigami.Reload`で区別する。

## 11. 式神データの読み込み・自動復元・手動復旧

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `Shikigami.Reload` | `Started` | INFO | 起動時・手動操作・更新後の式神データ読み込みを開始したとき | なし |
| `Shikigami.Reload` | `InitialLoadRejected` | WARNING | 最初の読み込みが失敗し、自動復元へ移行するとき | `Outcome`または`Exception` |
| `Shikigami.Reload` | `Completed` | INFO | 必要なら自動復元を経て、読み込みと候補一覧更新が正常終了したとき | なし |
| `Shikigami.Reload` | `Failed` | ERROR | 自動復元・再試行を含めて、読み込みまたはGui更新が最終的に失敗したとき | `Outcome`または`Exception` |
| `Shikigami.AutoRestore` | `Completed` | INFO | 埋め込みデータから既定CSVの復元が完了したとき | なし |
| `Shikigami.AutoRestore` | `Failed` | ERROR | 既定CSVの復元に失敗したとき | `Outcome`または`Exception` |
| `Shikigami.BrokenFile` | `Moved` | INFO | 破損CSVをBrokenディレクトリへ退避したとき | `FileName` |
| `Shikigami.Recovery` | `CandidatesEmpty` | INFO | 復旧候補を正常に読み込んだ結果、候補が0件だったとき | なし |
| `Shikigami.Recovery` | `Started` | INFO | 候補を選択して確定し、手動復旧を開始するとき | `SelectedCount` |
| `Shikigami.Recovery` | `BackupCompleted` | INFO | 復旧前データのバックアップ作成が完了したとき | なし |
| `Shikigami.Recovery` | `Completed` | INFO | 復旧データの反映と後続のGui候補一覧更新が完了したとき | `SelectedCount` |
| `Shikigami.Recovery` | `Failed` | ERROR | 候補の取得・バックアップ作成・復旧・Gui更新が失敗したとき | `Outcome`または`Exception` |

式神データの初回読み込みが失敗しても、自動復元により再読み込みが成功した場合は`Shikigami.Reload.Completed`を記録する。初回失敗は`InitialLoadRejected`で区別し、最終失敗の`Failed`と混同しない。

`Shikigami.AutoRestore.Completed`および`Shikigami.BrokenFile.Moved`は、それぞれのファイル操作が実際に完了した後に記録する。復旧候補一覧の個々の要素を全件ログへ出力しない。

`Shikigami.Recovery.CandidatesEmpty`は復旧操作開始前の結果であり、`Started`を伴わない。手動復旧の`Completed`は、CSV更新だけではなく後続のGui候補一覧更新の成功確認後に出力する。

`Shikigami.Recovery.Completed`の`SelectedCount`は復旧対象として選択された件数を示し、実際にCSVへ追加された件数を保証しない。復旧実行時に既存データとの重複が再判定されるため、両者は一致しない場合がある。

## 12. CLEAR

| Operation | Event | Level | 発生条件 | 追加項目 |
|---|---|---|---|---|
| `MainForm.Clear` | `Started` | INFO | CLEAR確認ダイアログで確定し、処理を開始するとき | なし |
| `MainForm.Clear` | `Completed` | INFO | 入力初期化・計算結果破棄・必要な状態更新が完了したとき | なし |
| `MainForm.Clear` | `Failed` | ERROR | CLEAR処理中に予期しない例外が発生したとき | `Exception` |

CLEARの確認をキャンセルした場合は`Started`を出力しない。

## 13. 出力位置の責務

- 正常完了イベントは、成功条件を判断できる処理境界で記録する。
- SaveDataの変換処理やControl設定処理など、内部の小さなメソッドごとに正常系ログを追加しない。
- エラーイベントは原則として利用者へ失敗を報告する境界で出力し、同じ障害を複数の層で繰り返し記録しない。
- 途中で例外が発生した場合、それより後の正常完了イベントを記録しない。
- 記録したイベントを状態変更や例外処理の実行条件としない。
- 内部で失敗を捕捉して処理を継続するFunctionについて、呼び出し元は正常に返ったという事実だけで成功判定しない。必要なら結果を伝える設計とし、成功を確認してから`Completed`を記録する。

## 14. テスト観点

- 主要操作の正常系で`Started`と`Completed`が記録されること。
- 検証失敗時は適切な`ValidationRejected`が記録され、`Completed`が出ないこと。
- 例外発生時は`Failed`が記録され、同一操作が正常完了扱いにならないこと。
- SaveData読み込みの`Validated`と`Completed`が、それぞれ検証完了・Gui適用完了に対応すること。
- ロールバック成功と失敗が区別され、同じ例外全文が不要に重複しないこと。
- `Calculation.Result.DirtyChanged`がCleanからDirtyへの変更時だけ記録されること。
- 式神データの初回読み込み失敗からの自動復元成功が、最終失敗の`Failed`と区別できること。
- Snapshot比較の`SNAPSHOT_NULL`・`STATUS_NULL`が成功扱いにならないこと。
- ダイアログのキャンセルや単純な入力変更によって大量の操作ログが生成されないこと。
- Loggerの書き込み失敗時でも、ユーザー通知および操作状態の復帰が実行されること。

## 15. 関連資料

- `LoggerSpecification.md`：Logger共通仕様
- `../Calculation/CalculationResultLifecycle.md`：計算結果ライフサイクル
- `../../Diagram/SaveData/save_sequence.wsd`：SaveData保存のシーケンス図
- `../../Diagram/SaveData/load_sequence.wsd`：SaveData読み込みのシーケンス図
- `../../Diagram/ShikigamiData/shikigami_recovery_sequence.wsd`：式神復旧のシーケンス図
