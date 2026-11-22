# 計算結果ライフサイクル仕様

## 1. 目的と適用範囲

本書は、ShikigamiAppのGuiにおける計算結果の生成・保持・破棄、Dirty / Clean状態、結果詳細表示、およびCalculationSnapshot保存可否を定義する。

計算処理は`MainForm`から`CalculationFunction`へ、結果・Dirty状態の管理は`CalculationResultFunction`へ分離されている。対象は、これらのFunction、関連Operator、MainFormのイベント処理、およびSaveDataとの接点である。計算式そのもの、SaveDataのファイル形式、一般的な画面配置は対象外とする。

**照合基準：** 2026-11-21時点のmaster `8e494fe76974ba4ae6d3b65d7a70dc393e8c76ec`。

## 2. 基本方針

- 計算は計算ボタン押下時に実行する。入力変更時に自動再計算は行わない。
- 直近の計算結果とDirty / Clean状態は`CalculationResultFunction`が保持する。
- 通常の入力変更では、前回結果と表示済みの結果を破棄しない。Dirty化によりSnapshot保存可否のみを変更する。
- 新しい計算が成功すると保持結果を置き換える。式神選択時の計算成功でClean化する。
- 式神未選択でも御魂のみの計算は可能であり、成功時にもDirty / Clean状態は変更しない。
- CLEAR確定時は入力・結果表示・保持結果を消去し、Dirty状態にする。
- SaveData読込の適用失敗でロールバックが成功した場合は前回結果を保持する。ロールバック自体が失敗した場合は保持結果を無効化する。

## 3. 管理状態

### 3.1 計算結果

`CalculationResultFunction.mLastCalculationResult`で直近の計算結果を保持し、`LastCalculationResult`で参照する。

| 値 | 意味 |
|---|---|
| `null` | 保持中の計算結果が存在しない |
| `null`以外 | 直近の計算結果が存在する |

MainFormは保持結果を直接フィールドとして管理しない。`btnResultView_Click()`は`LastCalculationResult`を参照し、結果の有無だけで詳細表示可否を判定する。

DEBUGビルドの`MainForm.mLastCalculationTestSource`はテストソース出力用の別状態であり、通常の計算結果保持状態とは区別する。

### 3.2 Dirty / Clean

`CalculationResultFunction.mIsCalculationResultDirty`で管理する。

| 状態 | 値 | 意味 |
|---|---|---|
| Dirty | `true` | 保持結果をCalculationSnapshot保存に使用できない |
| Clean | `false` | 保持結果が存在すればCalculationSnapshot保存が可能 |

Dirtyは単純に「表示結果が古い」ことを意味しない。御魂のみの計算では結果が更新されてもClean化されない。Dirty状態で入力を元へ戻しても自動的にCleanに復帰しない。

`CalculationResultFunction.MarkDirty(source)`は、抑止条件が成立した場合は何も行わない。抑止されなければSaveボタンの有効状態を更新し、CleanからDirtyへ変わるときだけ状態を変更して`Calculation.Result.DirtyChanged`を記録する。すでにDirtyなら状態変更ログは重複出力しない。

`MarkClean()`はDirtyフラグを`false`にする。`Invalidate()`はDirty化・保持結果破棄・表示消去をまとめて実行する。

### 3.3 初期状態

| 項目 | 状態 |
|---|---|
| `CalculationResultFunction.LastCalculationResult` | `null` |
| Dirty状態 | `true` |
| 計算結果表示 | 空 |
| 計算結果詳細表示 | 不可 |
| CalculationSnapshot保存 | 不可 |

## 4. 計算実行

### 4.1 責務と入力検証

`MainForm.btnCalc_Click()`は操作の開始・終了ログ、`CalculationFunction.TryExecute()`の呼び出し、計算結果の反映、例外通知を担当する。

`CalculationFunction.TryExecute()`は、`MitamaInputOperator.CreateInputModel()`、`CalculationInputValidator.Validate()`、`CalculationInputValidationErrorHandler.Handle()`、DTOへの変換、`CalculationGateway.Calculate()`を順に実行する。式神未選択時の基礎ステータスには新規`StatusDto`を使用する。DEBUGビルドでは`CalculationTestSourceConverter`でテストソースも作成する。

入力検証で拒否された場合、`TryExecute()`は`false`を返す。`MainForm`はそのまま終了し、保持中の前回結果を置き換えない。`Calculation.ValidationRejected`が記録され、`Calculation.Completed`は記録されない。

### 4.2 計算成功時

`TryExecute()`が成功した場合、`MainForm`は次の順に実行する。

1. `CalculationResultFunction.SetResult()`で計算結果を保持する。
2. DEBUGビルドでは`mLastCalculationTestSource`を更新する。
3. `updateSaveButtonEnabled("Calculation")`でSaveボタンを再評価する。
4. 式神が選択されていた場合に限り`CalculationResultFunction.MarkClean()`を呼び出す。
5. `CalculationResultFunction.ShowResult()`で計算結果を画面へ反映する。
6. `Calculation.Completed`を記録する。

`ShowResult()`は`CalculationResultOperator`を通じて御魂のみの集計欄を更新し、式神選択中は最終ステータスも表示する。式神未選択時の最終ステータス欄は空にする。

**実装上の順序に注意：** Saveボタンの有効状態更新は`MarkClean()`より前に呼び出される。SaveボタンのEnabled状態とSnapshot保存可否を同一の判定として扱わない。

### 4.3 御魂のみの計算

式神未選択でも、有効な御魂入力があれば計算結果を生成・保持・表示できる。結果詳細表示も可能である。`MarkClean()`は実行されないため、Dirty状態は変更されず、通常の初期Dirty状態からの操作ではSnapshot保存不可のままである。

### 4.4 例外と保持結果

現行`btnCalc_Click()`では、`TryExecute()`だけでなく、その後の`SetResult()`、Saveボタン更新、Clean化、結果表示も同じ`try`内で実行し、例外は`Calculation.Failed`として記録してエラーダイアログを表示する。

- `TryExecute()`の途中で例外が発生した場合、`SetResult()`へ到達しないため、通常は前回結果を保持する。
- `SetResult()`実行後にSaveボタン更新や結果表示などで例外が発生した場合、すでに保持結果が更新されている可能性がある。
- 後続処理を含めた一括ロールバックは実装していない。例外発生時に前回結果を必ず保持すると解釈してはならない。

## 5. 入力変更と状態遷移

### 5.1 Dirty化の契機

入力変更は`MitamaInputFunction`や`ShikigamiSelectionFunction`からMainFormの`markCalculationResultDirty(source)`を経由し、`CalculationResultFunction.MarkDirty(source)`に通知される。

主な契機：

- 式神の選択変更・選択解除
- 御魂メイン／サブステータスとセット効果・固有効果の変更
- 式神データの再読み込み・登録／編集後の選択反映
- Build／MitamaSetの読込成功
- CLEAR確定

入力変更だけで自動再計算、保持結果破棄、表示消去、結果詳細表示禁止は行わない。

### 5.2 操作中のDirty化抑止

MainFormは`MainFormOperationState`で`IDLE`、`LOADING`、`SAVING`、`CLEARING`等の操作状態を扱う。`CalculationResultFunction`へ渡す抑止条件は`LOADING`または`CLEARING`である。

これによりLoad／CLEARのUI適用中に発火する入力変更イベントではDirty化を抑え、**操作終了後**にMainFormから明示的にDirty化する。Saveボタンの更新も別途操作状態によって制御する。

### 5.3 状態遷移の一覧

| 操作・状況 | 保持結果 | Dirty / Clean | 詳細表示 | Snapshot保存 |
|---|---|---|---|---|
| 起動直後 | なし | Dirty | 不可 | 不可 |
| 式神ありで計算成功 | 新結果あり | Clean | 可 | 可 |
| 御魂のみで計算成功（通常状態） | 新結果あり | Dirty維持 | 可 | 不可 |
| Cleanから入力変更 | 前回結果を保持 | Dirty | 可 | 不可 |
| Dirtyから入力を元に戻す | 前回結果を保持 | Dirty維持 | 可 | 不可 |
| 式神ありで再計算成功 | 新結果あり | Clean | 可 | 可 |
| SaveData読込成功 | 前回結果を保持 | Dirty | 結果があれば可 | 不可 |
| SaveData適用失敗・ロールバック成功 | 前回結果を保持 | 直前の状態を維持 | 結果があれば可 | 直前の状態による |
| SaveData適用失敗・ロールバック失敗 | 破棄 | Dirty | 不可 | 不可 |
| CLEAR確定 | 破棄 | Dirty | 不可 | 不可 |

※ 例外発生時の一部状態が更新済みとなる可能性は4.4節に従う。上表はそれぞれの記載条件における通常の制御経路を表す。

## 6. 計算結果詳細表示

`MainForm.btnResultView_Click()`は`CalculationResultFunction.LastCalculationResult`を取得する。

- 結果あり：`ResultViewForm`へ渡してモーダル表示する。
- 結果なし：`先に計算を実行してください。`と通知して開かない。

Dirty / Cleanの状態は詳細表示の判定には使わない。入力変更後でも、前回結果を閲覧できる。

## 7. CLEAR

### 7.1 キャンセル

CLEARボタン押下時に`入力内容と計算結果をクリアします。よろしいですか？`と確認する。`Yes`以外では状態を変更しない。

### 7.2 確定

`MainForm.btnClear_Click()`は`MainForm.Clear.Started`を記録し、操作状態を`CLEARING`に変更して以下を実行する。

1. `clearInputs()`で式神選択、御魂・効果入力、結果表示欄を初期化する。内部では`ShikigamiSelectionOperator.ClearShikigamiSelection()`、`MitamaInputFunction.Clear()`、`CalculationResultFunction.ClearTexts()`を呼び出す。
2. `deleteLastCalculationResult()`から`CalculationResultFunction.DeleteResult()`を呼び出し、保持結果を破棄する。DEBUGビルドではテストソースも破棄する。
3. 操作状態を`IDLE`に戻した後、`markCalculationResultDirty()`でDirty化する。
4. `MainForm.Clear.Completed`を記録する。

失敗時は`MainForm.Clear.Failed`を記録し、例外を再送出する。CLEAR全工程を元に戻すロールバックはない。

## 8. SaveDataとの関係

### 8.1 CalculationSnapshot保存条件

`CalculationResultFunction.CanSaveCalculationSnapshot()`は次の条件を判定する。

```csharp
return mLastCalculationResult != null && !mIsCalculationResultDirty;
```

ただし、実際の保存可否には式神選択と保存種別の前提条件も必要である。`SaveDataSaveFunction.Save()`はSnapshot保存時に式神が選択されていることと`CanSaveCalculationSnapshot()`を再検証する。

### 8.2 保存レベル

`SaveDataSaveFunction.GetSaveLevel()`は**最初に式神選択状態**を確認する。

| 条件 | 保存レベル | 選択可能な対象 |
|---|---|---|
| 式神未選択 | `MITAMA_SET_ONLY` | MitamaSet |
| 式神選択済みかつSnapshot保存可能 | `SNAPSHOT_AVAILABLE` | MitamaSet、Build、CalculationSnapshot |
| 式神選択済み・上記以外 | `BUILD_AVAILABLE` | MitamaSet、Build |

この順序により、式神未選択時にSnapshot保存レベルを返さない。

### 8.3 SaveボタンとLoadボタンの有効状態

`SaveDataButtonFunction.Update()`が操作状態と入力検証結果に基づいてボタンを制御する。

- `IDLE`以外：LoadボタンとSaveボタンを無効化する。
- `IDLE`：Loadボタンを有効化し、Saveボタンは`CalculationInputValidator.Validate()`の結果が`NO_EQUIPPED_MITAMA`でなければ有効化する。

SaveボタンのEnabled状態は、特定の保存形式（特にCalculationSnapshot）が保存可能であることを保証しない。

### 8.4 SaveData読込成功・失敗時の計算結果

`MainForm.btnLoad_Click()`は、`SaveDataLoadFunction.Load()`による読込・検証後、`SaveDataApplyFunction.Apply()`で入力を画面へ適用する。適用中は`LOADING`でDirty化を抑止し、成功後に`IDLE`へ戻して明示的にDirty化する。通常の成功経路では前回の計算結果を保持したまま、Snapshot保存不可となる。

適用失敗時、`SaveDataApplyFunction`は直前の入力モデル・式神選択・レアリティ条件の復元を試みる。

- **ロールバック成功：** 読込全体は失敗として報告するが、保持計算結果を`Invalidate()`しない。通常、操作前の計算結果状態を保つ。
- **ロールバック失敗：** `SaveDataRollbackException`を受けたMainFormが`CalculationResultFunction.Invalidate()`を呼び、Dirty化、保持結果破棄、表示消去を行う。結果が存在した場合には`Calculation.Result.Invalidated`を記録する。DEBUGテストソースも破棄する。

## 9. 関連実装と責務

| 対象 | 責務・API |
|---|---|
| `Gui/Form/MainForm.cs` | `btnCalc_Click()`、`btnResultView_Click()`、`btnClear_Click()`、SaveData操作境界、ログ・エラー通知 |
| `Gui/Function/CalculationFunction.cs` | `TryExecute()`：入力モデル作成・検証・変換・計算 |
| `Gui/Function/CalculationResultFunction.cs` | `LastCalculationResult`、`SetResult()`、`MarkDirty()`、`MarkClean()`、`CanSaveCalculationSnapshot()`、`ShowResult()`、`ClearTexts()`、`DeleteResult()`、`Invalidate()` |
| `Gui/Operator/CalculationResultOperator.cs` | 計算結果のTextBox表示・クリア |
| `Gui/Function/MitamaInputFunction.cs` | 御魂入力イベントのDirty通知と入力クリア |
| `Gui/Function/ShikigamiSelectionFunction.cs` | 式神選択・再読み込みとDirty通知 |
| `Gui/Function/SaveDataButtonFunction.cs` | Save / LoadボタンのEnabled制御 |
| `Gui/Function/SaveDataSaveFunction.cs` | 保存レベル判定・保存種別前提条件の検証 |
| `Gui/Function/SaveDataApplyFunction.cs` | 読込入力のGui適用、失敗時のロールバック |

## 10. テスト観点

- 計算成功後に結果を保持・表示し、詳細画面を開けること。
- 入力変更後も保持結果と画面表示が維持され、Snapshot保存だけが不可になること。
- 入力を元に戻すだけではCleanに復帰しないこと。
- 式神選択時の計算成功でClean化し、Snapshot保存可能となること。
- 式神未選択時の御魂のみ計算ではDirtyを変更せず、結果詳細表示は可能であること。
- 入力検証拒否時に前回結果を破棄せず、`ValidationRejected`として記録すること。
- CLEARキャンセル時に入力・結果を維持し、確定時は入力・結果表示・保持結果・DEBUGテストソースを破棄すること。
- SaveData読込成功時に前回結果を保持したままDirty化すること。
- SaveData適用失敗時のロールバック成功では計算結果を維持すること。
- SaveDataロールバック失敗では計算結果を破棄し、結果詳細表示・Snapshot保存が不可になること。
- SaveボタンのEnabled状態とSnapshot保存条件が別々に判定されること。

上記は現行実装の契約から抽出したテスト観点であり、すべてのケースについて個別テストの存在やPASSを保証するものではない。

## 11. 関連資料

- `../Logger/LoggerSpecification.md`：Gui診断ログの共通仕様
- `../Logger/LogEventDefinition.md`：Calculation、Calculation.Result、SaveData、MainForm.Clearのイベント定義
- `../../Diagram/Calculation/calculation_sequence.wsd`：計算処理シーケンス図（図の実装同期は別途確認）
- `../../Diagram/SaveData/load_sequence.wsd`：SaveData読込シーケンス図
- `../../Diagram/SaveData/save_sequence.wsd`：SaveData保存シーケンス図
