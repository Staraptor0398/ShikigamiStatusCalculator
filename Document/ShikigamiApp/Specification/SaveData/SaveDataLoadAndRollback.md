# SaveData読込・UI適用・ロールバック仕様

## 1. 目的・対象範囲

本書は、ShikigamiAppのGuiにおけるSaveData読込処理について、読込・変換・検証、UI適用、適用失敗時のロールバック、および計算結果・操作状態との関係を現行実装に基づいて整理する。

対象は、`MainForm`の読込操作、`SaveDataLoadFunction`、`SaveDataApplyFunction`、`SaveDataAccess` / `JsonDataAccess` / `SaveDataMigrator`、および`MitamaInputOperator`・`ShikigamiSelectionFunction`を中心としたUI適用経路である。SaveData保存処理、ファイル選択UIの詳細な見た目、計算ロジックそのもの、および未実装の将来仕様は対象外とする。

## 2. 関連クラスと責務

| クラス / 型 | 主な責務（本仕様に関係する範囲） |
|---|---|
| `Gui.Form.MainForm` | 読込ダイアログ起動、`MainFormOperationState`遷移、読込開始/検証/完了/失敗ログ、`SaveDataLoadFunction.Load()`と`SaveDataApplyFunction.Apply()`の呼び出し、成功時のDirty化、失敗時の例外種別に応じた計算結果無効化・エラー通知 |
| `Gui.Dialog.SaveDataLoadDialog` | 読込対象種別（Build / MitamaSet）の選択、ファイルパス指定、OK時に`SelectedLoadType`と`FilePath`を確定 |
| `Gui.Dialog.SaveDataLoadType` | 読込対象種別。`Build`、`MitamaSet`の2値 |
| `Gui.Function.SaveDataLoadFunction` | 種別に応じたファイル読込、SaveDataから`MitamaSetInputModel`への変換、適用前検証（Build時は式神名検証を含む） |
| `Gui.Function.SaveDataApplyFunction` | UI適用前の状態退避、種別別のUI適用、適用例外時の復元試行、復元成功時の元例外再送出、復元失敗時の`SaveDataRollbackException`送出 |
| `Gui.Function.SaveDataRollbackException` | ロールバック失敗を表す例外。`applyException`と`rollbackException`を`AggregateException`として内包する |
| `Gui.Function.ShikigamiSelectionFunction` | Build読込時の式神名存在検証（`ValidateBuildShikigami`）と式神選択適用（`ApplyShikigami`） |
| `Gui.Operator.MitamaInputOperator` | 現在入力からの`MitamaSetInputModel`生成、`SaveDataInputModelValidator`による検証、`MitamaSetInputModelApplicator`によるUI適用、入力クリア |
| `Gui.Validation.SaveDataInputModelValidator` | 読込後InputModelと現行UIコントロール構成の整合性検証（件数、構造、選択可能なステータス種別など） |
| `Gui.Converter.MitamaSetConverter` | `BuildSaveData.MitamaSet` / `MitamaSetSaveData`と`MitamaSetInputModel`間の変換 |
| `Gui.Form.Applicator.MitamaSetInputModelApplicator` | `MitamaSetInputModel`の内容を御魂・セット効果・固有効果のUIコントロールへ反映 |
| `Gui.Function.CalculationResultFunction` | Dirty / Clean管理、計算結果保持、`Invalidate()`による結果無効化と表示消去 |
| `Gui.Function.SaveDataButtonFunction` | `MainFormOperationState`が`IDLE`かどうかに基づく読込/保存ボタン有効化制御 |
| `SaveData.Access.SaveDataAccess` | 種別別の読込/保存。読込時はテキスト読込、Version確認、必要時マイグレーション、デシリアライズを行う |
| `SaveData.Access.JsonDataAccess` | JSONファイル読込、`Version`取得、`SaveDataFile<T>`へのデシリアライズ |
| `SaveData.Migration.SaveDataMigrator` | 保存Versionが現行Versionより古い場合のマイグレーション |
| `SaveData.Definition.SaveDataVersionDefinition` | 現行読込対象Version（MitamaSet / Build / CalculationSnapshotはいずれもVersion 2） |

## 3. 読込対象と基本フロー

### 3.1 読込対象

`SaveDataLoadDialog`は読込種別として`Build`（ビルド保存データ）と`MitamaSet`（御魂セット保存データ）を選択できる。OK時に`SelectedLoadType`と`FilePath`が`MainForm`へ渡される。キャンセル時、`MainForm`は読込処理を開始しない。

### 3.2 基本フロー（正常系）

`MainForm.btnLoad_Click()`における正常系の実行順序は次のとおりである。

1. 読込ダイアログを表示する。`DialogResult.OK`以外の場合は終了する。
2. `setOperationState(MainFormOperationState.LOADING)`を実行する。
3. `Logger.Info("SaveData.Load", "Started", ...)`を記録し、`SaveData.Load`計測区間内で以下を実行する。
   - `SaveDataLoadFunction.Load(loadType, filePath, out shikigamiName)`でファイル読込・変換・適用前検証を行う。
   - `Logger.Info("SaveData.Load", "Validated", ...)`を記録する。
   - `SaveDataApplyFunction.Apply(loadType, shikigamiName, inputModel)`でUI適用する。
4. 内側`try`の`finally`で`setOperationState(MainFormOperationState.IDLE)`を実行する（正常終了・例外終了の両方）。
5. 例外が外側`catch`へ伝播していない場合、`markCalculationResultDirty()`を実行する。
6. `Logger.Info("SaveData.Load", "Completed", ...)`を記録する。

読込・適用中に例外が発生した場合、手順5以降の正常終了処理には到達しない。

### 3.3 BuildとMitamaSetの違い

| 観点 | Build (`SaveDataLoadType.Build`) | MitamaSet (`SaveDataLoadType.MitamaSet`) |
|---|---|---|
| ファイル読込 | `SaveDataAccess.LoadBuild(path, track)`で`BuildSaveData`を取得 | `SaveDataAccess.LoadMitamaSet(path, track)`で`MitamaSetSaveData`を取得 |
| InputModel生成 | `MitamaSetConverter.ToInputModel(data.MitamaSet)` | `MitamaSetConverter.ToInputModel(data)` |
| 読込データの追加確認 | `data == null`および`inputModel == null`で`InvalidOperationException` | `inputModel == null`で`InvalidOperationException` |
| 適用前の式神検証 | `ShikigamiSelectionFunction.ValidateBuildShikigami(data.ShikigamiName)`を実行 | なし |
| `shikigamiName`出力 | `data.ShikigamiName`を設定 | `null`のまま |
| UI適用 | `applyBuild`：`ApplyShikigami(shikigamiName)`後に御魂入力を適用 | `applyMitamaSet`：御魂入力のみ適用 |
| ロールバック退避対象 | 適用前のInputModel・式神名・レア度を退避 | 同左（MitamaSet読込でも退避・復元処理自体は同じ） |

`SaveDataApplyFunction.Apply()`は未対応種別に対して`InvalidOperationException`を送出する。`SaveDataLoadFunction.Load()`も未対応種別に対して`InvalidOperationException`を送出する。

## 4. 読込データの検証

### 4.1 ファイル読込・Version確認・マイグレーション

`SaveDataAccess.LoadBuild` / `LoadMitamaSet`は、いずれも`SaveDataVersionDefinition`の現行Version（Build / MitamaSetともVersion 2）を用いて共通の読込処理を行う。

1. `JsonDataAccess.ReadText(path, track)`でUTF-8テキストを取得する。
2. `JsonDataAccess.GetVersion(json)`で`sourceVersion`を取得する。
   - ルートがJSONオブジェクトでない場合は`FormatException`となる。
   - `Version`プロパティが存在しない場合は`1`を返す。
3. `sourceVersion > currentVersion.Version`の場合、`NotSupportedException`（`Unsupported save data version: ...`）を送出する。
4. `sourceVersion < currentVersion.Version`の場合、`SaveDataMigrator.Migrate(json, currentVersion, sourceVersion)`で移行後JSONを得る。
5. `JsonDataAccess.Deserialize<SaveDataFile<T>>(json, track)`で`SaveDataFile<T>`を復元し、`file.Data`を返す（`file == null`の場合は`default`）。

`SaveDataMigrator.Migrate`の現行実装で確認できる移行は、`sourceVersion == 1`かつ`targetVersion.Version`未満の間に行う処理のみである。

- MitamaSet：ルート内の`SetEffects` / `UniqueEffects`を変換する。
- Build / CalculationSnapshot：ルート内`MitamaSet`を同様に変換する。
- 上記変換後、`{ "Version": 2, "Data": <変換後ルート> }`形式へラップする。
- `sourceVersion`が1以外で未対応の場合は`NotSupportedException`を送出する。

### 4.2 変換と適用前検証

読込成功後、`SaveDataLoadFunction`は種別に応じて`MitamaSetInputModel`へ変換し、UI適用前に検証を行う。

- **Build**：`data`および変換結果`inputModel`の非`null`を確認後、`ValidateBuildShikigami(data.ShikigamiName)`と`MitamaInputOperator.ValidateInputModel(inputModel)`を実行する。
- **MitamaSet**：変換結果`inputModel`の非`null`を確認後、`MitamaInputOperator.ValidateInputModel(inputModel)`を実行する。

`ValidateBuildShikigami`は、式神名が空でない場合に、現在保持している式神リストに同名の式神が存在しなければ`InvalidOperationException`を送出する。

`SaveDataInputModelValidator.Validate`で確認できる主な条件は以下のとおりである。

- InputModel、各コレクション、UIコントロール配列が不足していないこと。
- 御魂数・セット効果数・固有効果数がUIスロット数を超えないこと。
- 各御魂・サブステータス・効果の構造が不正でないこと。
- 各ステータス値について、型名・値テキスト・ComboBox参照が`null`でないこと。
- ステータス型名が空文字列でない場合、その型名が対応するComboBoxの`Items`に含まれること。

上記検証で拒否された場合、`SaveDataLoadFunction.Load()`は例外を送出し、`MainForm`のUI適用処理には進まない。

## 5. UI適用と状態退避

### 5.1 適用前の状態退避

`SaveDataApplyFunction.applyWithRollback`は、UI適用前に以下を退避する。

- `MitamaInputOperator.CreateInputModel()`で取得した現在の御魂入力モデル
- `ShikigamiSelectionOperator.GetSelectedShikigami()?.Name`で取得した現在の選択式神名
- `ShikigamiSelectionOperator.GetSelectedRarity()`で取得した現在のレア度選択状態

退避は`SaveData.RollbackSnapshot`計測区間内で行われ、Build / MitamaSetのいずれの読込でも実施される。

### 5.2 種別別のUI適用

| 種別 | 適用内容 | 到達順序（例外がない場合） |
|---|---|---|
| Build | `ShikigamiSelectionFunction.ApplyShikigami(shikigamiName)`を実行し、続けて`MitamaInputOperator.ApplyInputModel(inputModel)`を実行する | 式神選択適用 → `AfterShikigamiSelection`チェックポイント → 御魂入力適用 → `AfterApply`チェックポイント |
| MitamaSet | `MitamaInputOperator.ApplyInputModel(inputModel)`のみ実行する | 御魂入力適用 → `AfterApply`チェックポイント |

`ApplyShikigami`は、式神名が空の場合は何もしない。指定名を現在のフィルタ状態で選択できない場合はレア度を`DisplayText.RARITY_ALL`に変更して再試行し、それでも選択できなければ`InvalidOperationException`を送出する。

`MitamaSetInputModelApplicator.Apply`は、InputModel内の御魂・セット効果・固有効果を順にUIへ反映する。御魂の主ステータス種別が空の場合は選択解除と表示テキストクリアを行い、空でない場合は対応項目を選択する。サブステータスや効果の種別が空の場合は選択解除する。

`SaveDataApplyFunction`はチェックポイント呼び出し用コールバックを保持するが、`MainForm`の現行初期化ではコールバックを渡していない。したがって通常の読込処理では、チェックポイントに紐づく追加処理は構成されない。

## 6. ロールバック処理

### 6.1 失敗検知と復元手順

UI適用中に例外が発生すると、`applyWithRollback`はその例外を捕捉し、以降の正常系処理（未実行の適用処理や`AfterApply`チェックポイント）を行わない。捕捉後は`SaveData.Rollback`の開始ログを出力し、以下の順で復元を試行する。

1. `BeforeRollbackRestore`チェックポイントを呼び出す（設定されている場合）。
2. 退避していたレア度を`SetSelectedRarity(previousRarity)`で復元する。
3. 退避していた式神名が空の場合は`ClearShikigamiSelection()`を実行する。空でない場合は`TrySelectShikigami(previousShikigamiName)`を試行し、失敗したら`InvalidOperationException`を送出する。
4. 退避していたInputModelを`ApplyInputModel(previousInputModel)`で復元する。

### 6.2 ロールバック結果の扱い

| 結果 | `SaveDataApplyFunction`の動作 | `MainForm`から見た到達可能性 |
|---|---|---|
| 復元成功 | `SaveData.Rollback`完了ログを出力し、元の適用例外を`throw;`で再送出する | `SaveDataRollbackException`ではない例外として外側`catch`へ到達する。成功時のDirty化・完了ログには到達しない |
| 復元失敗 | `SaveData.Rollback`失敗ログを出力し、`SaveDataRollbackException(applyEx, rollbackEx)`を送出する | `SaveDataRollbackException`として外側`catch`へ到達し、専用の無効化・失敗処理が実行される |

復元成功時はUI状態の復元を試みたことを示すが、読込操作自体は失敗として扱われる（元の適用例外が再送出されるため）。

## 7. 例外処理と計算結果の扱い

### 7.1 `MainForm`の例外分岐

`MainForm.btnLoad_Click()`は、読込・検証・適用で発生した例外を外側`catch`で受け、以下のように分岐する。

- **`SaveDataRollbackException`の場合**
  - 例外発生前に保持していた計算結果の有無を確認する。
  - `CalculationResultFunction.Invalidate()`を実行する。
  - 事前に計算結果が存在していた場合、`Logger.Warning("Calculation.Result", "Invalidated", "Reason=SaveDataRollbackFailed")`を記録する。
  - DEBUGビルドでは`mLastCalculationTestSource = null`を実行する。
  - 内部`AggregateException`の先頭要素があればそれを原因として`SaveData.Load Failed (Outcome=RollbackFailed)`を記録し、なければ原因例外なしで同Outcomeの失敗ログを記録する。
- **その他の例外の場合**
  - 計算結果の無効化は行わず、`Logger.Error("SaveData.Load", "Failed", ex, ...)`を記録する。
  - ロールバックが成功して元の適用例外が再送出されたケースもこの分岐に含まれる。

いずれの分岐でも最後にエラーメッセージボックスを表示する。

### 7.2 計算結果・Dirty状態とUI入力状態

`MainForm.btnLoad_Click()`の外側`catch`に到達する例外には、UI適用前、UI適用中、UI適用が例外なく完了した後の経路がある。`SaveDataRollbackException`以外の例外であることだけを根拠に、UI入力状態が必ず復元される、または必ず読込前のままであると断定できない。以下では、計算結果の保持・Dirty状態とUI入力状態を区別する。

| 状況 | 計算結果保持 / Dirty | UI入力状態（実装から確認できる範囲） |
|---|---|---|
| 読込成功 | 直前に保持していた結果を保持する。`IDLE`復帰後に`markCalculationResultDirty()`を実行する（`SetResult()` / `Invalidate()`は行わない） | `SaveDataApplyFunction.Apply()`が例外なく完了した場合、読込データがUIへ適用された状態となる |
| UI適用前の失敗（`SaveDataRollbackException`以外） | `Invalidate()`は行わない。成功時の`markCalculationResultDirty()`にも到達しないため、読込前の計算結果保持状態とDirty状態を維持する | `SaveDataApplyFunction.Apply()`へ到達していないため、`applyWithRollback`によるUI適用・ロールバックは開始されない |
| UI適用中の失敗・ロールバック成功（`SaveDataRollbackException`以外） | `Invalidate()`は行わない。成功時の`markCalculationResultDirty()`にも到達しないため、読込前の計算結果保持状態とDirty状態を維持する | `applyWithRollback`が退避状態（レア度・式神選択・InputModel）の復元処理を例外なく完了し、元の適用例外を再送出する。復元処理中に例外が発生した場合は本区分ではなくロールバック失敗となる |
| UI適用正常終了後の失敗（`SaveDataRollbackException`以外） | `Invalidate()`は行わない。`IDLE`復帰後の`markCalculationResultDirty()`や完了ログの実行有無は例外発生位置による（例: 完了ログ出力中の例外ではDirty化は既に実行されている）。計算結果の`Invalidate()`が行われないことと、Dirty化が実行済みかどうかは区別する | `Apply()`が例外なく完了した後（例: `IDLE`復帰後の`markCalculationResultDirty()`や`SaveData.Load`完了ログ出力）に例外が発生した場合、`SaveDataApplyFunction`によるロールバックは実行されない。UIは適用後の状態となっている可能性がある |
| ロールバック失敗（`SaveDataRollbackException`） | `Invalidate()`により計算結果を破棄しDirty状態とする。事前に結果が存在した場合は無効化ログを出力する | 復元処理中に例外が発生したため、UI入力状態が完全に復元されていることは保証されない |

計算結果の保持・破棄・Dirty状態の一般仕様については、[計算結果ライフサイクル仕様](../Calculation/CalculationResultLifecycle.md)も参照する。

## 8. 状態遷移・処理結果の整理

### 8.1 `MainFormOperationState`とボタン制御

`MainFormOperationState`は`IDLE`、`SAVING`、`LOADING`、`CLEARING`を持つ。読込処理では`LOADING`と`IDLE`を使用する。

- `setOperationState`は状態変更時に`updateSaveButtonEnabled("OperationState")`を実行し、状態変更ログを記録する。
- `SaveDataButtonFunction.Update()`は、`IDLE`でない場合に読込・保存ボタンを無効化する。`IDLE`の場合は読込ボタンを有効化し、入力検証結果に応じて保存ボタンを制御する。
- 読込中にUIイベント経由で発生するDirty化は、`CalculationResultFunction`の抑止条件（`LOADING`または`CLEARING`）により抑止される。`MitamaInputFunction`は読込中判定として`LOADING`状態を参照する。

### 8.2 処理結果の到達可能性

| 区分 | 主な到達条件 | 状態 / 結果（実装から確認できる範囲） |
|---|---|---|
| 読込開始前中断 | ダイアログがOK以外 | `LOADING`へ遷移しない。入力・計算結果・Dirty状態は変更されない |
| 読込成功 | 読込・検証・UI適用が例外なく完了し、`IDLE`復帰後のDirty化・完了ログまで到達 | `LOADING` → `IDLE`遷移後にDirty化し、完了ログを記録する。UIは適用後の状態となる |
| 読込失敗（UI適用前） | `SaveDataApplyFunction.Apply()`到達前の処理（読込、Version確認/マイグレーション、変換、適用前検証、`LOADING`/ログ等）で例外 | `applyWithRollback`によるUI適用・ロールバックは開始されない。`IDLE`復帰後に`SaveDataRollbackException`以外の失敗分岐へ進み、計算結果の`Invalidate()`は行わない |
| 読込失敗（UI適用中・ロールバック成功） | UI適用中に例外が発生し、退避状態の復元処理が完了 | 元の適用例外が再送出され、`SaveDataRollbackException`以外の失敗分岐へ進む。計算結果の`Invalidate()`は行わない |
| 読込失敗（UI適用中・ロールバック失敗） | UI適用中の例外に加え、復元処理でも例外が発生 | `SaveDataRollbackException`として失敗分岐へ進み、計算結果を`Invalidate()`する |
| 読込失敗（UI適用正常終了後） | `Apply()`が例外なく完了した後、`IDLE`復帰後のDirty化または完了ログ出力などで例外 | ロールバックは実行されない。`SaveDataRollbackException`以外の失敗分岐へ進み、計算結果の`Invalidate()`は行わない。例外発生位置によりDirty化が実行済みの場合がある（例: 完了ログ出力中の例外）。UIは適用後の状態となっている可能性がある |

## 9. 関連資料

- [SaveData読込シーケンス図](../../Diagram/SaveData/load_sequence.wsd)
- [計算結果ライフサイクル仕様](../Calculation/CalculationResultLifecycle.md)
- [ログイベント定義](../Logger/LogEventDefinition.md)
- [Logger仕様](../Logger/LoggerSpecification.md)
