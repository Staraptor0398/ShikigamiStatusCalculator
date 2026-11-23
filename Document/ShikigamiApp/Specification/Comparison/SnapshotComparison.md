# Snapshot比較仕様

## 1. 目的・対象範囲

本書は、ShikigamiAppのGuiにおけるSnapshot比較機能について、比較元・比較先Snapshotファイルの選択、読込・検証、ステータス差分計算、比較結果表示、および失敗時の通知を現行実装に基づいて整理する。

対象は、`MainForm`の比較操作、`SnapshotCompareFileSelectDialog`、`SnapshotComparisonFunction`、`SaveDataAccess` / `JsonDataAccess` / `SaveDataMigrator`によるSnapshot読込、Gui側およびGateway側のステータス変換、`ComparisonGateway` / `ComparisonService` / `StatusComparator`による差分計算、`StatusComparisonResultForm`による結果表示である。

本書の対象外は、SaveData保存処理、計算処理そのもの、Snapshotファイルの生成内容の詳細仕様、および計算結果ライフサイクルの一般仕様である。これらとの関係は[関連資料](#11-関連資料)を参照する。

## 2. 関連クラスと責務

| クラス / 型 | 主な責務（本仕様に関係する範囲） |
|---|---|
| `Gui.Form.MainForm` | 比較ダイアログ表示、比較開始/完了/失敗ログ出力、`SnapshotComparisonFunction.TryCompare()`呼び出し、`SnapshotComparisonOutcome`別処理、結果フォーム表示、例外時の汎用エラー通知 |
| `Gui.Dialog.SnapshotCompareFileSelectDialog` | 比較元・比較先ファイルパスの入力/参照、`OpenFileDialog`設定、比較ボタン有効条件、同一パス指定時の警告、OK/Cancel確定 |
| `Gui.Function.SnapshotComparisonFunction` | outパラメータ初期化、比較元/比較先Snapshot読込、両Snapshotおよび両`FinalStatus` DTOの判定、Gateway比較呼び出し、Snapshot名と比較結果の返却 |
| `Gui.Function.SnapshotComparisonOutcome` | 比較結果区分。`SUCCESS`、`SNAPSHOT_NULL`、`STATUS_NULL` |
| `SaveData.Access.SaveDataAccess` | `LoadSnapshot()`によるSnapshot読込。JSON読込、Version確認、必要時マイグレーション、`SaveDataFile<CalculationSnapshotSaveData>`のデシリアライズと`Data`取得 |
| `SaveData.Access.JsonDataAccess` | UTF-8テキスト読込、`Version`取得、JSONデシリアライズ |
| `SaveData.Migration.SaveDataMigrator` | 保存Versionが現行Versionより古い場合のマイグレーション（現行実装で確認できるのはVersion 1からの移行） |
| `SaveData.Definition.SaveDataVersionDefinition` | `CalculationSnapshot`の現行Version（Version 2） |
| `SaveData.Model.CalculationSnapshotSaveData` | 読込対象Snapshotデータ。`SnapshotName`、`CreatedAt`、`ShikigamiName`、`MitamaSet`、`MitamaStatus`、`FinalStatus`を保持する |
| `SaveData.Model.StatusSaveData` | Snapshot内ステータスデータ。`Attack`、`HP`、`Defense`、`Speed`、`CritRate`、`CritDamage`、`EffectHit`、`EffectResist`を保持する |
| `Gui.Converter.StatusConverter` | Gui側変換。`ToDto(StatusSaveData)`で`FinalStatus`を`StatusDto`へ変換し、`null`入力時は`null`を返す |
| `StatusDto` / `StatusComparisonResultDto`（Gateway側で定義） | GuiとGateway間で受け渡す比較入力・比較結果データ |
| `ComparisonGateway` | Gui DTOをネイティブ`Status`へ変換し、`ComparisonService`で比較後、`StatusComparisonResultMapper`でDTOへ変換して返す |
| `Gateway.Converter.Model.StatusConverter` | Gateway側変換。`StatusDto`とネイティブ`Status`を相互変換する。`toNative`は`dto == nullptr`の場合に`ArgumentNullException`を送出する |
| `Gateway.Mapper.Model.StatusComparisonResultMapper` | ネイティブ`StatusComparisonResult`から`StatusComparisonResultDto`への片方向変換 |
| `Core.Service.ComparisonService` | `StatusComparator::compare()`を呼び出して差分結果を返す |
| `Core.Status.StatusComparator` | 比較先 − 比較元で8項目の差分を計算する |
| `Gui.Form.StatusComparisonResultForm` | 比較元/比較先Snapshot名と8項目差分の表示、閉じる操作による`ShowDialog`復帰 |

本仕様では、`Gui.Converter.StatusConverter`と`Gateway.Converter.Model.StatusConverter`を同名のため区別して記載する。

## 3. 比較元・比較先ファイルの選択

`SnapshotCompareFileSelectDialog`は比較元・比較先のSnapshotファイルパスを指定するダイアログである。

| 項目 | 実装内容 |
|---|---|
| 比較元/比較先の指定 | `btnBrowseBaseSnapshot_Click` / `btnBrowseTargetSnapshot_Click`で`selectSnapshotFile()`を呼び出し、取得したパスが`null`・空文字・空白のみでなければ対応するテキストボックスへ設定する |
| ファイル選択ダイアログ | `OpenFileDialog`を使用する。`Filter`は`SaveDataFileDefinition.SnapshotFilter`、`InitialDirectory`は`AppPath.SnapshotSaveDataDirectoryPath`とする。`ShowDialog(this)`が`OK`以外の場合は空文字を返し、テキストボックスは変更しない |
| 比較ボタンの有効条件 | `updateCompareButtonEnabled()`により、比較元・比較先の両テキストが`string.IsNullOrWhiteSpace(...)`でない場合に`btnCompare.Enabled`を`true`とする。`Load`時および参照ボタンでパス設定後に更新する |
| 同一パス指定時の警告 | 比較ボタン押下時、`string.Equals(txtBaseSnapshotFilePath.Text, txtTargetSnapshotFilePath.Text)`が`true`の場合に警告メッセージを表示する。文言は「同じスナップショットファイルは比較できません。」、タイトルは「スナップショット比較」、種別は`MessageBoxButtons.OK` / `MessageBoxIcon.Warning`である |
| 同一パス判定の性質 | 判定はテキストボックス文字列の`string.Equals`比較である。ファイルの実体、正規化パス、内容が同一かどうかまで確認する実装ではない |
| 同一パス時の継続 | 警告表示後は`return`し、`BaseSnapshotFilePath` / `TargetSnapshotFilePath`の確定、`DialogResult`設定、ダイアログ終了は行わない。選択ダイアログは継続する |
| OK確定条件 | 比較ボタン押下時に比較元・比較先のパス未指定を拒否する処理はないが、`btnCompare`は両パスが空白でない場合のみ有効化される。同一パスでない場合、`BaseSnapshotFilePath` / `TargetSnapshotFilePath`にテキストボックス値を設定し、`DialogResult.OK`を設定して閉じる |
| Cancel | `BaseSnapshotFilePath` / `TargetSnapshotFilePath`を空文字に設定し、`DialogResult.Cancel`を設定して閉じる |

`MainForm.btnCompareResult_Click()`は`ShowDialog(this)`が`DialogResult.OK`でない場合に終了する。したがって、キャンセル時および同一パス警告によりダイアログが継続したままOK確定しない場合、`Logger.Info("Snapshot.Compare", "Started")`には到達しない。

## 4. 比較実行フロー

`MainForm.btnCompareResult_Click(object sender, EventArgs e)`における処理順序は次のとおりである。

1. `SnapshotCompareFileSelectDialog`を`using`で生成し、`ShowDialog(this)`で表示する。
2. ダイアログ結果が`DialogResult.OK`でない場合は`return`する。
3. `try`内で`Logger.Info("Snapshot.Compare", "Started")`を記録する。
4. `SnapshotComparisonFunction.TryCompare(dialog.BaseSnapshotFilePath, dialog.TargetSnapshotFilePath, out baseSnapshotName, out targetSnapshotName, out comparisonResult)`を呼び出す。
5. 戻り値`outcome`に応じて分岐する。
   - `SNAPSHOT_NULL`：`Logger.Error("Snapshot.Compare", "Failed", $"Outcome={outcome}")`を記録し、エラーメッセージ「スナップショットデータの読み込みに失敗しました。」（タイトル「計算結果比較」、`MessageBoxButtons.OK` / `MessageBoxIcon.Error`）を表示して`return`する。
   - `STATUS_NULL`：`Logger.Error("Snapshot.Compare", "Failed", $"Outcome={outcome}")`を記録して`return`する。`MessageBox`は表示しない。
6. `SUCCESS`の場合、`StatusComparisonResultForm(baseSnapshotName, targetSnapshotName, comparisonResult)`を`using`で生成し、`ShowDialog(this)`で表示する。
7. 結果フォームの`ShowDialog(this)`が正常に戻った後、`Logger.Info("Snapshot.Compare", "Completed")`を記録する。
8. `try`内で例外が発生した場合、`catch (Exception ex)`で`Logger.Error("Snapshot.Compare", "Failed", ex)`を記録し、汎用エラーメッセージ「計算結果比較中に予期しないエラーが発生しました。」（タイトル「計算結果比較」、`MessageBoxButtons.OK` / `MessageBoxIcon.Error`）を表示する。

`Completed`ログは比較計算成功直後や結果フォーム表示直後ではなく、結果フォームの`ShowDialog(this)`が正常に戻った後に記録される。`SNAPSHOT_NULL` / `STATUS_NULL`で`return`した場合、および例外発生時は`Completed`ログに到達しない。

## 5. Snapshot読込と検証

### 5.1 `SnapshotComparisonFunction.TryCompare()`の実装順序

`TryCompare(string baseFilePath, string targetFilePath, out string baseSnapshotName, out string targetSnapshotName, out StatusComparisonResultDto comparisonResult)`は次の順に処理する。

1. `baseSnapshotName`、`targetSnapshotName`、`comparisonResult`を`null`に初期化する。
2. `SaveDataAccess.LoadSnapshot(baseFilePath)`で比較元Snapshotを取得する。
3. `SaveDataAccess.LoadSnapshot(targetFilePath)`で比較先Snapshotを取得する。
4. `baseSnapshot == null || targetSnapshot == null`の場合、`SnapshotComparisonOutcome.SNAPSHOT_NULL`を返す。
5. `Gui.Converter.StatusConverter.ToDto(baseSnapshot.FinalStatus)`および`ToDto(targetSnapshot.FinalStatus)`で両`FinalStatus`をDTOへ変換する。
6. `baseStatus == null || targetStatus == null`の場合、`SnapshotComparisonOutcome.STATUS_NULL`を返す。
7. `ComparisonGateway.CompareStatus(baseStatus, targetStatus)`で差分を計算する。
8. `baseSnapshotName`に`baseSnapshot.SnapshotName`、`targetSnapshotName`に`targetSnapshot.SnapshotName`、`comparisonResult`に比較結果を設定する。
9. `SnapshotComparisonOutcome.SUCCESS`を返す。

比較元Snapshotを読み込んだ直後には`null`判定しない。比較元と比較先の読込が両方戻った後に`SNAPSHOT_NULL`を判定する。ただし、比較元読込で例外が発生した場合は`TryCompare()`内に例外捕捉がないため、比較先読込には進まない。

### 5.2 `SaveDataAccess.LoadSnapshot()`と読込結果

`SaveDataAccess.LoadSnapshot(string path)`は`load<CalculationSnapshotSaveData>(path, SaveDataVersionDefinition.CalculationSnapshot)`を呼び出す。`track`引数は渡されない。

読込処理の順序は次のとおりである。

1. `JsonDataAccess.ReadText(path, track)`でUTF-8テキストを取得する。
2. `JsonDataAccess.GetVersion(json)`で`sourceVersion`を取得する。ルートがJSONオブジェクトでない場合は`FormatException`となる。`Version`プロパティが存在しない場合は`1`を返す。
3. `sourceVersion > currentVersion.Version`の場合、`NotSupportedException`（`Unsupported save data version: ...`）を送出する。
4. `sourceVersion < currentVersion.Version`の場合、`SaveDataMigrator.Migrate(json, currentVersion, sourceVersion)`で移行後JSONを得る。
5. `JsonDataAccess.Deserialize<SaveDataFile<CalculationSnapshotSaveData>>(json, track)`で`SaveDataFile`を復元し、`file == null ? default : file.Data`を返す。

`CalculationSnapshot`の現行Versionは2である（`SaveDataVersionDefinition.CalculationSnapshot`）。`SaveDataMigrator`の現行実装で確認できる移行は、`sourceVersion == 1`の場合に`MitamaSet`配下の`SetEffects` / `UniqueEffects`を変換し、`{ "Version": 2, "Data": <変換後ルート> }`形式へラップする処理である。`sourceVersion`が1以外で未対応の場合は`NotSupportedException`を送出する。

読込結果が`null`になる経路として、デシリアライズ結果の`file`が`null`の場合に`load`が`default`を返すことが確認できる。一方、ファイル読込失敗、JSON形式不正、未対応Version、マイグレーション失敗、デシリアライズ失敗などは例外として伝播し得る。`null`戻り値と例外伝播を混同しない。

SaveData読込共通処理の詳細は[SaveData読込・UI適用・ロールバック仕様](../SaveData/SaveDataLoadAndRollback.md)を参照する。

## 6. ステータス変換と差分計算

### 6.1 Gui側の`FinalStatus`変換

`SnapshotComparisonFunction`は、読込した`CalculationSnapshotSaveData`の`FinalStatus`を`Gui.Converter.StatusConverter.ToDto(StatusSaveData)`で`StatusDto`へ変換する。入力が`null`の場合は`null`を返す。変換対象は`Attack`、`HP`、`Defense`、`Speed`、`CritRate`、`CritDamage`、`EffectHit`、`EffectResist`の8項目である。

`CalculationSnapshotSaveData`には`MitamaStatus`と`FinalStatus`が存在するが、本比較処理が差分計算の入力として使用するのは`FinalStatus`である。保存データに存在する全フィールドを比較対象とみなさない。

### 6.2 Gateway / Coreでの比較

`SUCCESS`判定前の比較処理は次のとおりである。

1. `ComparisonGateway.CompareStatus(StatusDto^ baseStatus, StatusDto^ targetStatus)`が呼び出される。
2. Gateway側`Gateway.Converter.Model.StatusConverter::toNative(StatusDto^)`で比較元・比較先DTOをネイティブ`Status`へ変換する。`dto == nullptr`の場合は`ArgumentNullException`を送出する。
3. `ComparisonService::CompareStatus(const Status& baseStatus, const Status& targetStatus)`が`StatusComparator::compare(baseStatus, targetStatus)`を呼び出す。
4. `StatusComparisonResultMapper::toDto(const StatusComparisonResult&)`で差分結果を`StatusComparisonResultDto`へ変換する。

Gateway側`StatusConverter::toNative`は、8項目に加えて`AdditionalAttackRate`、`AdditionalHpRate`、`AdditionalDefenseRate`もネイティブ`Status`へコピーする。一方、`StatusComparator::compare`が差分計算するのは8項目のみであり、`StatusComparisonResult` / `StatusComparisonResultDto` / `StatusComparisonResultMapper`にも`Additional*`系の差分フィールドは含まれない。ネイティブ変換に含まれる項目と実際の差分計算対象項目は区別する。

### 6.3 差分の定義

差分の定義は常に「比較先 − 比較元」である。

| 項目 | 対応する差分フィールド |
|---|---|
| 攻撃力 | `AttackDifference` |
| HP | `HpDifference` |
| 防御力 | `DefenseDifference` |
| 素早さ | `SpeedDifference` |
| 会心率 | `CriticalRateDifference` |
| 会心DMG | `CriticalDamageDifference` |
| 効果命中 | `EffectHitDifference` |
| 効果抵抗 | `EffectResistDifference` |

`Additional*`系のフィールドは比較差分の算出対象ではない。

## 7. 比較結果表示

`StatusComparisonResultForm`は、比較元・比較先のSnapshot名と差分結果をモーダル表示する。

| 項目 | 実装内容 |
|---|---|
| Snapshot名表示 | `Load`時に`lblBaseSnapshotName.Text = mBaseSnapshotName`、`lblTargetSnapshotName.Text = mTargetSnapshotName`を設定する |
| 表示項目 | 8項目を固定順で表示する。順序は「攻撃力」「HP」「防御力」「素早さ」「会心率」「会心DMG」「効果命中」「効果抵抗」である |
| DataGridView列構成 | `initializeComparisonResultGrid()`で列をクリア後、`"ステータス"`（列名`StatusName`）と`"差分"`（列名`Difference`）の2列を追加する。`AllowUserToAddRows=false`、`AllowUserToDeleteRows=false`、`ReadOnly=true`、`RowHeadersVisible=false`、`SelectionMode=FullRowSelect`、`AutoSizeColumnsMode=Fill`とする |
| 初期化処理 | `Load`時に列/行初期化と表示データ設定を行い、`dgvComparisonResult.ClearSelection()`を実行する |
| 差分の表示形式 | `formatDifference(double difference)`で、`difference > 0`の場合は`"+" + difference.ToString()`を返す。0および負数は`difference.ToString()`を返す |
| 終了 | 閉じるボタン押下時に`this.Close()`を実行する。`MainForm`は`ShowDialog(this)`の復帰後に制御を受け、正常復帰時のみ`Completed`ログを記録する |

数値の丸めや固定小数点桁数など、現行実装で確認できない書式仕様は本書の保証事項としない。

## 8. Outcome別の処理

| Outcome | `MainForm`の処理 | ログ | ユーザー通知 | 結果フォーム |
|---|---|---|---|---|
| `SUCCESS` | `baseSnapshotName`、`targetSnapshotName`、`comparisonResult`を用いて`StatusComparisonResultForm`を`ShowDialog(this)`で表示する。正常復帰後に後続処理へ進む | `Snapshot.Compare Started`（開始時）、`Snapshot.Compare Completed`（フォーム正常終了後） | なし | 表示する |
| `SNAPSHOT_NULL` | `TryCompare()`から戻った直後に失敗処理を行い`return`する | `Snapshot.Compare Started`（開始時）、`Snapshot.Compare Failed`（`Outcome=SNAPSHOT_NULL`） | 「スナップショットデータの読み込みに失敗しました。」（タイトル「計算結果比較」、エラー）を表示する | 表示しない |
| `STATUS_NULL` | `TryCompare()`から戻った直後に失敗処理を行い`return`する | `Snapshot.Compare Started`（開始時）、`Snapshot.Compare Failed`（`Outcome=STATUS_NULL`） | `MessageBox`表示なし | 表示しない |

`SNAPSHOT_NULL`と`STATUS_NULL`は、失敗ログの記録有無や結果フォーム非表示は共通するが、ユーザー通知の有無が異なる。同一の通知経路として扱わない。

## 9. 例外処理と失敗時の扱い

`SnapshotComparisonFunction.TryCompare()`内には例外捕捉処理がない。ファイル読込、Version確認、マイグレーション、デシリアライズ、DTO/native変換、Gateway/Core比較処理、および`TryCompare()`内の各呼び出しで発生した例外は、捕捉されず呼び出し元へ伝播する。

`MainForm.btnCompareResult_Click()`の`try`範囲には、`Started`ログ出力、`TryCompare()`呼び出し、Outcome分岐後の処理、結果フォーム生成・表示、`Completed`ログ出力が含まれる。`try`内で例外が発生した場合は、その地点以降の正常処理を中断し、`catch (Exception ex)`で`Logger.Error("Snapshot.Compare", "Failed", ex)`を記録した後、汎用エラーメッセージ「計算結果比較中に予期しないエラーが発生しました。」を表示する。

現行実装から、次のような失敗経路が例外として`MainForm`の`catch`へ到達し得る（例外の具体的な型や発生条件は読込/変換/比較対象の実装に依存する）。

- ファイル読込失敗（例: `File.ReadAllText`起因）
- JSON形式不正（例: ルートオブジェクト不正時の`FormatException`）
- 未対応Version（`SaveDataAccess` / `SaveDataMigrator`の`NotSupportedException`）
- マイグレーション失敗（`SaveDataMigrator`の例外）
- デシリアライズ失敗（`JsonParser` / `JsonObjectConverter`等の例外）
- DTO/native変換時の例外（例: Gateway側`StatusConverter::toNative`の`ArgumentNullException`）
- Gateway/Core比較処理中の例外
- 比較結果フォーム生成・表示中の例外（フォーム生成、`ShowDialog`、フォーム表示中の例外）

例外と`SnapshotComparisonOutcome.SNAPSHOT_NULL` / `SnapshotComparisonOutcome.STATUS_NULL`は異なる失敗経路である。Outcome失敗は`TryCompare()`が正常に戻り値を返した場合の分岐であり、例外伝播とは区別する。

## 10. ログ・ユーザー通知

[ログイベント定義](../Logger/LogEventDefinition.md)の`Snapshot.Compare`と現行実装を照合した結果は次のとおりである。

| イベント / 通知 | 発生条件 | ログ | ユーザー通知 |
|---|---|---|---|
| 比較開始 | ダイアログで`OK`確定後、`MainForm`が比較処理を開始するとき | `Logger.Info("Snapshot.Compare", "Started")` | なし |
| 比較完了 | 両Snapshotの読込・比較・結果フォーム表示が正常に終了し、結果フォームの`ShowDialog(this)`が正常に戻ったとき | `Logger.Info("Snapshot.Compare", "Completed")` | なし |
| 比較失敗（`SNAPSHOT_NULL`） | `TryCompare()`が`SNAPSHOT_NULL`を返したとき | `Logger.Error("Snapshot.Compare", "Failed", $"Outcome=SNAPSHOT_NULL")` | 「スナップショットデータの読み込みに失敗しました。」（タイトル「計算結果比較」、エラー） |
| 比較失敗（`STATUS_NULL`） | `TryCompare()`が`STATUS_NULL`を返したとき | `Logger.Error("Snapshot.Compare", "Failed", $"Outcome=STATUS_NULL")` | なし（`MessageBox`表示なし） |
| 比較失敗（例外） | `MainForm`の`try`範囲で例外が発生したとき | `Logger.Error("Snapshot.Compare", "Failed", ex)` | 「計算結果比較中に予期しないエラーが発生しました。」（タイトル「計算結果比較」、エラー） |
| 同一パス指定時の警告 | 比較ダイアログで比較元・比較先パスが`string.Equals`で同一のまま比較ボタンを押下したとき | なし（`Snapshot.Compare`ログは出力されない） | 「同じスナップショットファイルは比較できません。」（タイトル「スナップショット比較」、警告） |

## 11. 関連資料

- [Snapshot比較シーケンス図（WSD）](../../Diagram/Comparison/snapshot_comparison_sequence.wsd)
- [Snapshot比較シーケンス図（PDF）](../../Diagram/Comparison/snapshot_comparison_sequence.pdf)
- [SaveData保存仕様](../SaveData/SaveDataSave.md)
- [SaveData読込・UI適用・ロールバック仕様](../SaveData/SaveDataLoadAndRollback.md)
- [計算結果ライフサイクル仕様](../Calculation/CalculationResultLifecycle.md)
- [ログイベント定義](../Logger/LogEventDefinition.md)
