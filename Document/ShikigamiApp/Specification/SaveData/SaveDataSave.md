# SaveData保存仕様

## 1. 目的・対象範囲

本書は、ShikigamiAppのGuiにおけるSaveData保存処理について、保存可能条件、保存ダイアログ、保存実行フロー、保存種別ごとのデータ生成、JSON保存、および失敗時の扱いを現行実装に基づいて整理する。

対象は、`MainForm`の保存操作、`SaveDataSaveFunction`、`SaveDataSaveDialog`、保存前入力取得に関わる`MitamaInputOperator` / Converter群、`SaveDataAccess` / `JsonDataAccess` / `JsonSerializer`、および関連するSaveDataモデル・Version定義である。

本書の対象外は、SaveData読込・UI適用・ロールバックの詳細、計算実行や計算結果ライフサイクルの一般仕様、保存ファイル選択UIの見た目上の詳細、および未実装の将来仕様である。これらとの関係は、必要に応じて関連資料を参照する。

保存可能レベルの判定と保存実行時の再検証、計算結果の保持状態とCalculationSnapshot保存可否、保存ボタンの有効化条件と保存処理自体の前提条件は、それぞれ別の判定である。本書ではこれらを区別して記載する。

## 2. 関連クラスと責務

| クラス / 型 | 主な責務（本仕様に関係する範囲） |
|---|---|
| `Gui.Form.MainForm` | 保存ダイアログの表示、保存確定後の`MainFormOperationState`遷移、`SaveData.Save Started/Completed/Failed`ログ出力、`SaveDataSaveFunction.Save()`呼び出し、失敗時メッセージ表示、`finally`での`IDLE`復帰 |
| `Gui.Function.SaveDataSaveFunction` | 保存可能レベル判定、保存実行時の前提条件検証、入力モデル取得、保存種別ごとのSaveData生成、`SaveDataAccess`呼び出し、CalculationSnapshotの`SnapshotName`生成 |
| `Gui.Dialog.SaveDataSaveDialog` | 保存種別候補と初期選択の構成、既定ファイル名/既定パス/拡張子/フィルタの決定、参照ダイアログ表示、保存先未指定時の警告、OK/Cancelの確定 |
| `Gui.Dialog.SaveDataSaveType` | 保存種別。`Build`、`MitamaSet`、`CalculationSnapshot` |
| `Gui.Dialog.SaveDataSaveLevel` | 保存可能レベル。`MITAMA_SET_ONLY`、`BUILD_AVAILABLE`、`SNAPSHOT_AVAILABLE` |
| `Gui.Definition.SaveDataFileDefinition` | 保存種別ごとのディレクトリ名、拡張子、ファイルフィルタ定義 |
| `Gui.Function.SaveDataButtonFunction` | `IDLE`時の読込/保存ボタン有効化制御。保存ボタンは御魂入力モデルの`CalculationInputValidator`結果に基づいて有効化されるが、特定保存種別の保存可否そのものは保証しない |
| `Gui.Function.CalculationResultFunction` | `LastCalculationResult`保持、`CanSaveCalculationSnapshot()`判定、Dirty/Clean状態管理。CalculationSnapshot保存可否の一部条件を提供する |
| `Gui.Operator.MitamaInputOperator` | `CreateInputModel()`による現在の御魂入力モデル生成 |
| `Gui.Operator.ShikigamiSelectionOperator` | 選択中式神名・選択中式神情報の取得。保存可能レベル判定とBuild/Snapshot保存時の式神名参照に使用される |
| `Gui.Converter.MitamaSetConverter` | `MitamaSetInputModel`から`MitamaSetSaveData` / `MitamaSetDto`相当データへの変換（保存では`ToSaveData(MitamaSetInputModel)`を使用） |
| `Gui.Converter.StatusConverter` | `StatusDto`から`StatusSaveData`への変換（CalculationSnapshotの`MitamaStatus`/`FinalStatus`生成に使用） |
| `SaveData.Access.SaveDataAccess` | 種別別の`SaveDataFile<T>`生成（`Version` + `Data`）と`JsonDataAccess.Save()`呼び出し |
| `SaveData.Access.JsonDataAccess` | `JsonSerializer.Write()`結果のUTF-8（BOMなし）ファイル書き込み |
| `SaveData.Serialization.JsonSerializer` | オブジェクトからJSON文字列を生成。ネスト上限や数値/文字列制約違反時は例外を送出し得る |
| `SaveData.Definition.SaveDataVersionDefinition` | 種別別の現行保存Version（MitamaSet / Build / CalculationSnapshotはいずれもVersion 2） |
| `SaveData.Model.File.SaveDataFile<T>` | 保存ファイルのルート構造。`Version`と`Data`を保持する |
| `SaveData.Model.*` | `BuildSaveData`、`MitamaSetSaveData`、`CalculationSnapshotSaveData`など保存データ本体 |

## 3. 保存可能レベル

### 3.1 判定条件

保存可能レベルは`SaveDataSaveFunction.GetSaveLevel()`で判定される。判定順序は次のとおりである。

1. `ShikigamiSelectionOperator.GetSelectedShikigami()`が`null`の場合、`MITAMA_SET_ONLY`を返す。
2. 式神が選択されており、`CalculationResultFunction.CanSaveCalculationSnapshot()`が`true`の場合、`SNAPSHOT_AVAILABLE`を返す。
3. 上記以外（式神選択済みだがCalculationSnapshot保存不可）の場合、`BUILD_AVAILABLE`を返す。

`CanSaveCalculationSnapshot()`は`LastCalculationResult != null && !mIsCalculationResultDirty`で判定される。保持計算結果の有無とDirty状態は別個に管理されるため、結果が存在してもDirtyであればSnapshot保存可能レベルにはならない。逆に、式神未選択時は`GetSaveLevel()`が最初に`MITAMA_SET_ONLY`を返すため、Snapshot保存レベルにはならない。

### 3.2 保存可能レベルと選択可能な保存種別

| 保存可能レベル | 判定条件（要約） | ダイアログで選択可能な保存種別 | 初期選択 |
|---|---|---|---|
| `MITAMA_SET_ONLY` | 式神未選択 | `MitamaSet`のみ | `MitamaSet` |
| `BUILD_AVAILABLE` | 式神選択済み、かつSnapshot保存不可 | `MitamaSet`、`Build` | `MitamaSet` |
| `SNAPSHOT_AVAILABLE` | 式神選択済み、かつ`CanSaveCalculationSnapshot()`が`true` | `MitamaSet`、`Build`、`CalculationSnapshot` | `MitamaSet` |

ダイアログはレベルに応じて候補を追加するが、`SelectedIndex = 0`を設定するため、候補が複数ある場合でも初期選択は常に御魂セット保存データ（`MitamaSet`）となる。

## 4. 保存ダイアログ

`SaveDataSaveDialog`は、`MainForm.btnSave_Click()`から選択中式神名（`GetShikigamiText()`）と保存可能レベルを受け取って生成される。

### 4.1 保存種別の候補と初期選択

- `initializeSaveTypeComboBox()`は候補をクリアし、まず`MitamaSet`を追加する。
- `SaveDataSaveLevel.BUILD_AVAILABLE`以上の場合に`Build`を追加する。
- `SaveDataSaveLevel.SNAPSHOT_AVAILABLE`以上の場合に`CalculationSnapshot`を追加する。
- 候補追加後、`cmbSaveType.SelectedIndex = 0`とする。

表示名は`Build`が「ビルド保存データ」、`MitamaSet`が「御魂セット保存データ」、`CalculationSnapshot`が「計算結果スナップショット」である。

### 4.2 種別変更時の既定ファイル名・パス

保存種別変更時（`cmbSaveType_SelectedIndexChanged`）、`txtFilePath`に選択種別の既定ファイルパスを設定する。既定パスは`getSaveTypeDirectoryPath(saveType)`と`createDefaultFileName(saveType)`の組み合わせである。

| 保存種別 | 既定ディレクトリ | 拡張子 | 既定ファイル名 |
|---|---|---|---|
| `Build` | `AppPath.BuildSaveDataDirectoryPath` | `.build.json` | 式神名が空白なら`Build`、それ以外は選択中式神名 |
| `MitamaSet` | `AppPath.MitamaSetSaveDataDirectoryPath` | `.mitama.json` | `MitamaSet` |
| `CalculationSnapshot` | `AppPath.SnapshotSaveDataDirectoryPath` | `.snapshot.json` | 式神名が空白なら`yyyyMMdd_HHmmss`、それ以外は`<式神名>_yyyyMMdd_HHmmss` |

拡張子は`SaveDataFileDefinition`の定数（`BUILD_EXTENSION` / `MITAMA_SET_EXTENSION` / `SNAPSHOT_EXTENSION`）に基づく。CalculationSnapshotの既定ファイル名に含まれるタイムスタンプは、既定パス生成時点の`DateTime.Now`である。

### 4.3 ファイル参照ダイアログ

参照ボタン押下時、現在選択中の保存種別に応じて`SaveFileDialog`を構成して表示する。

- `Filter`：`SaveDataFileDefinition`の種別別フィルタ（`BuildFilter` / `MitamaSetFilter` / `SnapshotFilter`）
- `InitialDirectory`：種別別の既定ディレクトリ
- `FileName`：種別別の既定ファイル名

`ShowDialog(this)`が`OK`以外の場合は`txtFilePath`を変更しない。`OK`の場合は選択された`FileName`を`txtFilePath`へ設定する。

### 4.4 保存先未指定時の警告

保存ボタン押下時、`txtFilePath.Text`が`null`・空文字・空白のみの場合、次のメッセージを表示して処理を中断する。

- 文言：`保存先ファイルを指定してください。`
- タイトル：`SaveData保存`
- 種別：`MessageBoxButtons.OK`, `MessageBoxIcon.Warning`

この場合、`SelectedSaveType` / `FilePath`の確定、`DialogResult`設定、ダイアログ終了は行わないため、OK確定には至らない。

### 4.5 OK / Cancelの挙動

| 操作 | 条件 | ダイアログの挙動 | `MainForm`側の後続 |
|---|---|---|---|
| Cancel | ユーザーがキャンセル | `DialogResult.Cancel`を設定して閉じる | `ShowDialog`が`OK`以外のため保存処理を開始しない |
| 保存（OK相当） | 保存先が未指定 | 警告表示後に`return`。ダイアログは開いたまま | 保存処理は開始しない |
| 保存（OK相当） | 保存先が指定済み | `SelectedSaveType`と`FilePath`を確定し、`DialogResult.OK`を設定して閉じる | 保存実行フローへ進む |

`MainForm`は`DialogResult.OK`の場合のみ保存を試行する。ダイアログをキャンセルした場合、状態変更・開始/完了/失敗ログ・ファイル保存は行わない。

## 5. 保存実行フロー

`MainForm.btnSave_Click()`における保存確定後の処理順序は次のとおりである。

1. `SaveDataSaveDialog`を生成して表示する（コンストラクタ引数は選択中式神名と`GetSaveLevel()`の結果）。
2. `ShowDialog(this)`が`DialogResult.OK`でない場合は終了する。
3. `try`内で`setOperationState(MainFormOperationState.SAVING)`を実行する。
4. `Logger.Info("SaveData.Save", "Started", $"SaveType={dialog.SelectedSaveType}")`を記録する。
5. `mSaveDataSaveFunction.Save(dialog.SelectedSaveType, dialog.FilePath)`を実行する。
6. `Save()`が例外なく戻った場合、`Logger.Info("SaveData.Save", "Completed", $"SaveType={dialog.SelectedSaveType} FileName={Path.GetFileName(dialog.FilePath)}")`を記録する。
7. `try`内で例外が発生した場合、`catch (Exception ex)`で`Logger.Error("SaveData.Save", "Failed", ex, $"SaveType={dialog.SelectedSaveType}")`を記録し、エラーメッセージボックスを表示する。
8. `finally`で`setOperationState(MainFormOperationState.IDLE)`を実行する。

### 5.1 ログ名・メッセージ文言・補足項目

| 区分 | 内容 |
|---|---|
| 開始ログ | `Logger.Info("SaveData.Save", "Started", $"SaveType={dialog.SelectedSaveType}")` |
| 完了ログ | `Logger.Info("SaveData.Save", "Completed", $"SaveType={dialog.SelectedSaveType} FileName={Path.GetFileName(dialog.FilePath)}")` |
| 失敗ログ | `Logger.Error("SaveData.Save", "Failed", ex, $"SaveType={dialog.SelectedSaveType}")` |
| 失敗時メッセージ | `MessageBox.Show("SaveDataの保存中にエラーが発生しました。", "SaveData保存", MessageBoxButtons.OK, MessageBoxIcon.Error)` |

`Completed`ログは`Save()`の正常復帰後にのみ到達する。`FileName`は`Path.GetFileName(dialog.FilePath)`で取得される。

### 5.2 `try / catch / finally`の範囲

- `setOperationState(MainFormOperationState.SAVING)`、Startedログ、`Save()`呼び出し、Completedログは同一`try`内にある。これらのいずれかで例外が発生した場合、その時点以降の正常処理は中断される。
- `catch`は`Exception`を捕捉し、Failedログとエラーメッセージ表示を行う。例外の再送出は行わない。
- `finally`は正常・異常を問わず実行され、`setOperationState(MainFormOperationState.IDLE)`を試行する。`finally`自体の処理が例外なく完了することを本書で保証するものではない。

## 6. 保存前の条件検証

### 6.1 `SaveDataSaveFunction.Save()`の検証順序

`Save()`は次の順に前提条件を検証する。

1. `filePath`が`null`、空文字、または空白のみの場合、`ArgumentException("SaveData file path is empty.", nameof(filePath))`を送出する。
2. `saveType`が`Build` / `MitamaSet` / `CalculationSnapshot`のいずれでもない場合、`InvalidOperationException($"Unsupported SaveData save type: {saveType}")`を送出する。
3. `ShikigamiSelectionOperator.GetSelectedShikigami()`で選択中式神を取得する。
4. `saveType != SaveDataSaveType.MitamaSet`かつ選択中式神が`null`の場合、`InvalidOperationException("Shikigami is not selected.")`を送出する。
5. `saveType == SaveDataSaveType.CalculationSnapshot`かつ`CalculationResultFunction.CanSaveCalculationSnapshot()`が`false`の場合、`InvalidOperationException("Calculation snapshot is not available.")`を送出する。
6. 上記を通過した後、`MitamaInputOperator.CreateInputModel()`を呼び出す。

### 6.2 保存可能レベル判定との違い

| 観点 | `GetSaveLevel()` | `Save()` |
|---|---|---|
| 主目的 | ダイアログ表示用の保存可能レベル決定 | 実保存前の前提条件検証 |
| ファイルパス | 判定しない | 空/空白を拒否する |
| 保存種別 | レベルに応じた候補提示に影響 | 未対応種別を拒否する |
| 式神選択 | レベル判定の分岐条件 | `Build`/`CalculationSnapshot`で未選択を拒否する |
| Snapshot保存可否 | レベル判定の分岐条件 | `CalculationSnapshot`で再検証する |
| 入力モデル取得 | 行わない | 検証通過後に`CreateInputModel()`を実行する |

ダイアログで選択可能な種別が`GetSaveLevel()`時点の状態に基づくため、ダイアログ表示後から保存実行までの間に状態が変化した場合は、`Save()`側の再検証で拒否される可能性がある。

### 6.3 計算入力バリデーションとの違い

`SaveDataSaveFunction.Save()`は`CalculationInputValidator.Validate()`を実行しない。保存処理は`MitamaInputOperator.CreateInputModel()`で取得した入力モデルを変換・保存対象として使用する。

一方、`SaveDataButtonFunction.Update()`は`IDLE`時に入力モデルを生成し、`CalculationInputValidator.Validate()`結果が`NO_EQUIPPED_MITAMA`でない場合に保存ボタンを有効化する。これは保存ボタンの有効化条件であり、保存処理自体の成功や、保存実行時に計算入力バリデーションを再実行することを意味しない。両者を同一視しない。

## 7. 保存種別ごとのデータ生成

### 7.1 MitamaSet

- 対象種別：`SaveDataSaveType.MitamaSet`
- 式神選択：不要。式神未選択時でも`GetSaveLevel()`が`MITAMA_SET_ONLY`となり得るため、MitamaSet保存は可能である。
- 入力モデル取得：`MitamaInputOperator.CreateInputModel()`で`MitamaSetInputModel`を取得する。
- データ生成：`MitamaSetConverter.ToSaveData(inputModel)`により`MitamaSetSaveData`を生成する。
- 保存呼び出し：`SaveDataAccess.SaveMitamaSet(filePath, data)`を実行する。

### 7.2 Build

- 対象種別：`SaveDataSaveType.Build`
- 式神選択：必須。選択中式神が`null`の場合は`Save()`が`InvalidOperationException`を送出する。
- 入力モデル取得：`MitamaInputOperator.CreateInputModel()`で`MitamaSetInputModel`を取得する。
- データ生成：
  - `MitamaSetConverter.ToSaveData(inputModel)`で`MitamaSetSaveData`を生成する。
  - `BuildSaveData`を生成し、`MitamaSet`に上記データ、`ShikigamiName`に選択中式神名（`selectedShikigami.Name`）を設定する。
- 保存呼び出し：`SaveDataAccess.SaveBuild(filePath, data)`を実行する。

### 7.3 CalculationSnapshot

- 対象種別：`SaveDataSaveType.CalculationSnapshot`
- 式神選択：必須。選択中式神が`null`の場合は`Save()`が`InvalidOperationException`を送出する。
- 保存可否：`CalculationResultFunction.CanSaveCalculationSnapshot()`が`true`であることが必要である。`false`の場合、`Save()`は`InvalidOperationException("Calculation snapshot is not available.")`を送出する。
- 保持計算結果の利用：`CalculationResultFunction.LastCalculationResult`（`CalculationResultDto`）を取得し、`MitamaOnlyStatus`および`FinalStatus`を保存データ生成に使用する。
- 入力モデル取得：`MitamaInputOperator.CreateInputModel()`で`MitamaSetInputModel`を取得する。
- データ生成：
  - `SnapshotName`：`createSnapshotNameFromFilePath(filePath)`で決定する。ファイル名が`SaveDataFileDefinition.SNAPSHOT_EXTENSION`（`.snapshot.json`）で終わる場合はその拡張子を除いた文字列、そうでない場合は`Path.GetFileNameWithoutExtension(filePath)`を使用する。
  - `CreatedAt`：`DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")`で保存時点の日時文字列を設定する。
  - `ShikigamiName`：選択中式神名（`selectedShikigami.Name`）を設定する。
  - `MitamaSet`：`MitamaSetConverter.ToSaveData(inputModel)`で生成する。
  - `MitamaStatus`：`StatusConverter.ToSaveData(calculationResult.MitamaOnlyStatus)`で生成する。
  - `FinalStatus`：`StatusConverter.ToSaveData(calculationResult.FinalStatus)`で生成する。
- 保存呼び出し：`SaveDataAccess.SaveSnapshot(filePath, data)`を実行する。

CalculationSnapshot保存可否は保持計算結果とDirty状態に依存する。Dirty状態や保持結果の破棄/置換条件、詳細表示可否との関係は[計算結果ライフサイクル仕様](../Calculation/CalculationResultLifecycle.md)に従う。`CanSaveCalculationSnapshot()`が`true`であることは、保存時点で保持結果が存在しDirtyでないことを示すが、計算結果そのものの正当性や現在入力との一致を本書で追加に保証するものではない。

## 8. JSON保存

### 8.1 `SaveDataAccess`による`SaveDataFile<T>`生成

`SaveDataAccess`は保存種別ごとに、対応する`SaveDataVersionDefinition`を用いて`SaveDataFile<T>`を生成する。

| 保存メソッド | データ型 | Version定義 | Version値 |
|---|---|---|---|
| `SaveMitamaSet(path, data)` | `MitamaSetSaveData` | `SaveDataVersionDefinition.MitamaSet` | 2 |
| `SaveBuild(path, data)` | `BuildSaveData` | `SaveDataVersionDefinition.Build` | 2 |
| `SaveSnapshot(path, data)` | `CalculationSnapshotSaveData` | `SaveDataVersionDefinition.CalculationSnapshot` | 2 |

生成されるファイル構造は`SaveData.Model.File.SaveDataFile<T>`であり、`Version`（`int`）と`Data`（種別ごとの保存データ）を保持する。`SaveDataAccess`はこのオブジェクトを`JsonDataAccess.Save(path, file)`へ渡す。

### 8.2 シリアライズとファイル書き込み

- `JsonDataAccess.Save<T>(path, data)`は`JsonSerializer.Write(data)`でJSON文字列を生成し、その結果を`File.WriteAllText(path, json, new UTF8Encoding(false))`で書き込む。
- 文字エンコードはUTF-8（BOMなし）である。
- `JsonSerializer.Write()`は、ネストが128段を超える場合、`NaN`/`Infinity`を含む数値、未ペアのサロゲート文字列などを検出した場合に例外を送出し得る。

### 8.3 上書き時の動作と例外伝播

保存先に同名ファイルが存在する場合、`File.WriteAllText`により上書き書き込みが行われる。`JsonSerializer.Write()`、`JsonDataAccess.Save()`、`File.WriteAllText`で発生した例外は`SaveDataAccess`内で捕捉・変換されず、そのまま呼び出し元（`SaveDataSaveFunction`、`MainForm`）へ伝播する。

## 9. 保存失敗時の扱い

### 9.1 失敗要因の分類

| 分類 | 主な発生箇所 | 例（現行実装から確認できる範囲） |
|---|---|---|
| 事前条件エラー | `SaveDataSaveFunction.Save()` | ファイルパス空/空白、未対応保存種別、Build/Snapshotで式神未選択、Snapshotで保存不可 |
| 入力モデル生成・変換時の例外 | `MitamaInputOperator.CreateInputModel()`、各Converter、CalculationSnapshotのデータ組み立て | 入力モデル生成や変換、保存データ生成に伴う例外。`LastCalculationResult`が`null`の状態でSnapshotデータ生成に到達した場合なども含まれ得るが、通常は事前検証で拒否される |
| JSONシリアライズ時の例外 | `JsonSerializer.Write()` | ネスト超過、数値表現不可、文字列の不正サロゲートなど |
| ファイル書き込み時の例外 | `JsonDataAccess.Save()` / `File.WriteAllText` | パス不正、アクセス権限不足、I/Oエラーなど |

上記は発生箇所の整理であり、すべての例外型や原因を網羅するものではない。

### 9.2 `MainForm`の`catch / finally`

`try`範囲内（`setOperationState(SAVING)`、Startedログ、`Save()`、Completedログ）で例外が発生した場合、`MainForm`は`catch`でFailedログを記録し、エラーメッセージを表示する。保存処理の再試行や入力・計算結果の復元、保存ファイルの修復は行わない。

`finally`では`setOperationState(MainFormOperationState.IDLE)`を実行するため、正常終了時も例外終了時もIDLE復帰を試行する。

### 9.3 保存ファイル状態の保証範囲

JSON保存は`JsonSerializer.Write()`の結果を`File.WriteAllText`で直接書き込む方式である。一時ファイルへの書き込み後の置換、`ReplaceFileW`、明示的なバックアップ復元、明示的なロールバックは実装されていない。

このため、書き込み失敗時に旧ファイル内容が保持されること、保存先ファイルが必ず破損すること、または必ず復元可能であることは、現行実装から保証できない。失敗時の保存先ファイル状態は、失敗箇所・ファイルシステム・既存ファイル有無等に依存し得るため、本書では断定しない。

## 10. ログ・ユーザー通知

### 10.1 診断ログ

| Operation | Event | Level | 出力条件 | 追加項目 |
|---|---|---|---|---|
| `SaveData.Save` | `Started` | INFO | ダイアログで保存先が確定し、`SAVING`遷移後に実際の保存処理を開始するとき | `SaveType` |
| `SaveData.Save` | `Completed` | INFO | `SaveDataSaveFunction.Save()`が正常復帰した後 | `SaveType`、`FileName` |
| `SaveData.Save` | `Failed` | ERROR | 保存処理を含む`try`範囲内で例外が発生し、`catch`に到達した場合 | `SaveType`、`Exception` |

`SaveType`はダイアログで確定した`SelectedSaveType`が記録される。`FileName`は保存先パスから`Path.GetFileName`で取得した値である。

### 10.2 ユーザー通知

| 発生箇所 | 条件 | 文言 / タイトル | 備考 |
|---|---|---|---|
| `SaveDataSaveDialog` | 保存先ファイル未指定で保存を押下 | `保存先ファイルを指定してください。` / `SaveData保存` | 警告表示。ダイアログは閉じず、保存処理は開始されない |
| `MainForm` | 保存失敗（`catch`） | `SaveDataの保存中にエラーが発生しました。` / `SaveData保存` | エラーメッセージ表示。例外内容の詳細は診断ログ側に記録される |

### 10.3 操作状態変更に伴うログ

保存実行時は`MainForm.setOperationState()`により`SAVING`遷移と`IDLE`復帰が行われる。`setOperationState()`は状態が実際に変更された場合に保存/読込ボタン更新と`MainForm.State Changed`ログを記録する。操作状態ログの詳細は[ログイベント定義](../Logger/LogEventDefinition.md)を参照する。

## 11. 関連資料

- [SaveData保存シーケンス図（.wsd）](../../Diagram/SaveData/save_sequence.wsd)
- [SaveData保存シーケンス図（.pdf）](../../Diagram/SaveData/save_sequence.pdf)
- [SaveData読込・UI適用・ロールバック仕様](./SaveDataLoadAndRollback.md)
- [計算結果ライフサイクル仕様](../Calculation/CalculationResultLifecycle.md)
- [ログイベント定義](../Logger/LogEventDefinition.md)
