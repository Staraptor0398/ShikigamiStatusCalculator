# 計算結果ライフサイクル仕様

## 1. 目的と適用範囲

本書は、ShikigamiAppのGuiにおけるステータス計算結果の生成、保持、破棄、Dirty / Clean状態、計算結果詳細表示、およびCalculationSnapshot保存可否を定義する。

対象は主に`MainForm`の計算・入力変更・CLEAR・結果表示・保存機能である。計算アルゴリズム自体やSaveDataのファイル形式は対象外とする。

## 2. 基本方針

- 計算は計算ボタンの押下時に実行する。入力変更による自動再計算は行わない。
- MainFormは直近に正常終了した計算結果を保持する。
- 入力変更時には、保持している計算結果と画面に表示済みの結果を消去しない。
- 次の計算が正常終了すると、保持している結果を新しい結果で置き換える。
- CLEARを確定した場合は、入力内容、画面上の計算結果、保持している計算結果を消去する。
- Dirty / Clean状態は、計算結果の存在有無とは独立して管理する。

## 3. 管理状態

### 3.1 計算結果の保持

`MainForm.mLastCalculationResult`で直近の計算結果を管理する。

| 値 | 意味 |
|---|---|
| `null` | 保持している計算結果が存在しない |
| `null`以外 | 直近の計算結果が存在する |

結果詳細表示の可否は、この値の有無で判定する。

### 3.2 Dirty / Clean状態

`MainForm.mIsCalculationResultDirty`で管理する。

| 状態 | 値 | 意味 |
|---|---|---|
| Dirty | `true` | CalculationSnapshot保存に使用できない状態 |
| Clean | `false` | 計算結果が存在する場合にCalculationSnapshot保存へ使用できる状態 |

Dirtyは単純に「計算結果が古い」ことを意味しない。御魂のみの計算では、新しい計算結果を生成してもClean化しないためである。

入力変更後に値を元へ戻しても、自動的にCleanへ復帰しない。Clean化するのは、式神が選択された状態で計算が正常終了した場合である。

### 3.3 初期状態

| 項目 | 状態 |
|---|---|
| `mLastCalculationResult` | `null` |
| `mIsCalculationResultDirty` | `true` |
| 計算結果表示 | 空 |
| 計算結果詳細表示 | 不可 |
| CalculationSnapshot保存 | 不可 |

## 4. 計算実行

### 4.1 入力検証

計算ボタン押下時、御魂入力からInputModelを作成して検証する。検証エラーの場合は計算せず、エラー処理を行う。保持している前回の計算結果は破棄しない。

### 4.2 計算成功時

入力検証を通過した場合、選択中の式神の基礎ステータスと御魂セットを使用して計算する。式神未選択時は空の`StatusDto`を基礎ステータスとして使用する。

計算が正常終了した場合は次の処理を行う。

1. `mLastCalculationResult`を新しい計算結果に更新する。
2. Saveボタンの有効状態を更新する。
3. 式神が選択されている場合のみ、`markCalculationResultClean()`でClean化する。
4. 計算結果をMainFormに表示する。

### 4.3 御魂のみの計算

式神未選択でも、入力が有効なら計算を実行できる。計算結果は保持・表示され、結果詳細表示にも使用できる。ただし、Dirty状態は変更せず、CalculationSnapshot保存は可能にしない。

### 4.4 計算中の例外

計算中に例外が発生した場合、エラーをログ出力してメッセージを表示し、後続の通常完了処理は実行しない。

現行実装は、計算結果の代入とDEBUG用テストデータ生成を同じ`try`ブロックで行っている。したがって、例外発生時の完全なロールバックまでは保証しない。

## 5. 入力変更

### 5.1 基本動作

計算に関係する入力が変更された場合、`markCalculationResultDirty()`を呼び出す。入力変更だけを理由に、次の処理は行わない。

- 自動再計算
- 保持中の計算結果の破棄
- MainFormの前回計算結果表示の消去
- 結果詳細表示の禁止

### 5.2 主なDirty化契機

- 式神の選択変更・選択解除
- 御魂のメイン・サブステータス変更
- 2セット効果・固有効果の変更
- 式神データの再読み込みや編集など、入力へ影響する操作
- Build / MitamaSetの読み込み処理
- CLEAR

Dirty化は、計算時と現在の入力値を比較する方式ではなく、入力変更イベントや各操作から明示的に呼び出す方式である。

すでにDirtyの場合は状態を維持する。CleanからDirtyへ遷移した場合は状態変更ログを出力する。

## 6. 計算結果詳細表示

`btnResultView_Click()`は`mLastCalculationResult`の有無のみで表示可否を判定する。Dirty / Clean状態は参照しない。

- 結果がある場合：保持している計算結果を使用して`ResultViewForm`を開く。
- 結果がない場合：`先に計算を実行してください。`と表示し、結果詳細画面は開かない。

入力変更後でも、前回の計算結果を詳細表示できる。

## 7. CLEAR

### 7.1 実行確認

CLEARボタン押下時、`入力内容と計算結果をクリアします。よろしいですか？`と確認する。「いいえ」の場合は何も変更しない。

### 7.2 確定時の処理

1. `clearInputs()`で式神選択・御魂・セット効果・固有効果・結果表示欄を初期化する。
2. `deleteLastCalculationResult()`で`mLastCalculationResult`を`null`にする。DEBUGビルドでは`mLastCalculationTestSource`も`null`にする。
3. `markCalculationResultDirty()`でDirty状態にする。

入力初期化と保持中の計算結果の破棄は責務を分離し、CLEARイベントハンドラからそれぞれ明示的に実行する。

### 7.3 CLEAR後の状態

| 項目 | 状態 |
|---|---|
| 式神選択 | なし |
| 御魂・効果入力 | 初期状態 |
| 計算結果表示 | 空 |
| `mLastCalculationResult` | `null` |
| `mIsCalculationResultDirty` | `true` |
| 計算結果詳細表示 | 不可 |
| CalculationSnapshot保存 | 不可 |

## 8. SaveData保存との関係

### 8.1 CalculationSnapshot保存条件

`canSaveCalculationSnapshot()`は次の条件を返す。

```csharp
return mLastCalculationResult != null && !mIsCalculationResultDirty;
```

計算結果が存在し、かつCleanである場合に限りSnapshot保存を選択可能とする。

### 8.2 Saveボタンの有効状態

Saveボタンの有効状態はSnapshot保存可否とは別に判定する。現行実装では、`CalculationInputValidator.Validate(inputModel)`の結果が`NO_EQUIPPED_MITAMA`でなければSaveボタンを有効にする。

### 8.3 保存可能レベル

| 保存レベル | 選択可能な保存対象 |
|---|---|
| `MITAMA_SET_ONLY` | MitamaSet |
| `BUILD_AVAILABLE` | MitamaSet、Build |
| `SNAPSHOT_AVAILABLE` | MitamaSet、Build、CalculationSnapshot |

`getSaveDataSaveLevel()`は以下の優先順で判定する。

1. `canSaveCalculationSnapshot()`が真なら`SNAPSHOT_AVAILABLE`。
2. それ以外で式神が選択されていれば`BUILD_AVAILABLE`。
3. それ以外は`MITAMA_SET_ONLY`。

## 9. 状態遷移

| 操作・状況 | 計算結果 | Dirty / Clean | 詳細表示 | Snapshot保存 |
|---|---|---|---|---|
| 起動直後 | なし | Dirty | 不可 | 不可 |
| 式神ありで計算成功 | 新結果あり | Clean | 可 | 可 |
| 御魂のみで計算成功 | 新結果あり | Dirty維持 | 可 | 不可 |
| Clean状態から入力変更 | 前回結果を保持 | Dirty | 可 | 不可 |
| Dirty状態から入力を元に戻す | 前回結果を保持 | Dirty維持 | 可 | 不可 |
| 入力変更後に式神ありで再計算成功 | 新結果あり | Clean | 可 | 可 |
| CLEAR確定 | なし | Dirty | 不可 | 不可 |

※「御魂のみで計算成功」のDirty維持は、通常の画面操作による状態を前提とする。実装上、御魂のみの計算処理自体はDirty状態を変更しない。

## 10. 関連実装

| 機能 | 主な実装 |
|---|---|
| 計算実行 | `MainForm.btnCalc_Click()` |
| Clean化 | `MainForm.markCalculationResultClean()` |
| Dirty化 | `MainForm.markCalculationResultDirty()` |
| 結果破棄 | `MainForm.deleteLastCalculationResult()` |
| CLEAR | `MainForm.btnClear_Click()` |
| 入力初期化 | `MainForm.clearInputs()` |
| 結果詳細表示 | `MainForm.btnResultView_Click()` |
| Snapshot保存可否 | `MainForm.canSaveCalculationSnapshot()` |
| 保存レベル判定 | `MainForm.getSaveDataSaveLevel()` |
| Saveボタン有効判定 | `MainForm.updateSaveButtonEnabled()` |
| 保存種別の表示 | `SaveDataSaveDialog.initializeSaveTypeComboBox()` |

## 11. テスト観点

- 計算成功後に結果詳細を表示できること。
- 入力変更後も前回の計算結果を詳細表示できること。
- 入力変更後はSnapshot保存対象から除外されること。
- 入力を元へ戻しただけではClean化されないこと。
- 式神ありの再計算成功後にSnapshot保存が可能になること。
- 御魂のみの計算成功後に結果詳細を表示でき、Snapshot保存はできないこと。
- CLEAR確定後に結果詳細を開こうとすると`先に計算を実行してください。`が表示されること。
- CLEARキャンセル時は入力・結果が維持されること。

関連するScenario Runnerのシナリオとして、`10_CalculationDetailAfterClear.scenario`および`CHECK SAVEDATA LEVEL`を利用するテストがある。その他のケースは既存シナリオの確認または追加が必要である。
