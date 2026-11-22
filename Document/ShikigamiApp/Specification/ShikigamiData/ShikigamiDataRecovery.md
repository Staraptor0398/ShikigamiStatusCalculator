# 式神データ手動復元仕様

## 1. 目的・対象範囲

本書は、ShikigamiApp の Gui から実行する式神データCSV（`Data\ShikigamiData.csv`）の手動復元機能を、現行実装に基づいて整理する。

対象は、復元元CSVの選択、復元候補の取得と表示、復元対象の選択、復元実行前の存在確認とバックアップ作成、Coreでの復元対象再判定・マージ・保存、復元後のGUI再読込・Dirty化・最終成否判定、および関連するOutcome/例外/ログ/ユーザー通知である。

対象外は、アプリ起動時の既定CSV復元や式神一覧読込失敗時の自動復旧そのもの（`ShikigamiSelectionFunction.InitializeShikigamiComboBox` 内で発生し得る処理を含む詳細）、式神登録・編集・保存処理、ユーザー操作による復元以外のCSV操作、および未実装の将来仕様である。自動復旧の詳細は [式神データ自動復旧仕様](ShikigamiDataAutoRepair.md) を参照する。

## 2. 手動復元と自動復旧の違い

| 観点 | 手動復元 | 自動復旧 |
|---|---|---|
| 起動契機 | `MainForm.btnRecoveryShikigami_Click` からユーザーが復元操作を開始する。 | アプリ起動時のCSV欠落対応、または `InitializeShikigamiComboBox` 実行時の初回読込失敗が条件を満たした場合に実行される。 |
| 復元元データ | ユーザーが `OpenFileDialog` で選択したCSVファイルを復元元として候補取得する。 | 埋め込みリソース `Gui.Data.ShikigamiData.csv` を既定データとして使用する。 |
| 現行データの扱い | 現行CSVに存在しない式神を候補として抽出し、選択された対象を現行データへマージする（Core実装）。 | 読込失敗時は `MoveBrokenFile` により現行CSVをBroken領域へ退避し、必要に応じて既定CSVを復元して再読込する。 |
| 事前処理 | 復元実行前に現行CSVの存在確認を行い、`ShikigamiDataFileManager.CreateBackup()` でBackup領域へコピーを作成する。 | 既定復元（`RestoreDefaultIfMissing`）やBroken退避（`MoveBrokenFile`）を行うが、手動復元と同じBackup作成フローは使用しない。 |
| 保存の有無 | Coreで復元対象が0件の場合、保存を行わず `SUCCESS` を返す場合がある。復元対象がある場合はマージ結果を保存する。 | 既定ファイル作成/復元や再読込が中心であり、手動復元のマージ保存とは処理内容が異なる。 |
| 成功の扱い | `ShikigamiGateway.RecoveryShikigami` のOutcomeが `SUCCESS` でも、手動復元フロー全体の成功とは限らない。GUI再読込成功かつ `autoRestored == false` で成功通知・完了ログとなる。 | 読込/復旧の成否は `InitializeShikigamiComboBox` の戻り値やログ・UI反映で判定される（詳細は自動復旧仕様を参照）。 |

## 3. 関連クラスと責務

| クラス / 型 | 主な責務（本仕様に関係する範囲） |
|---|---|
| `Gui.Form.MainForm` (`btnRecoveryShikigami_Click`) | 手動復元のUI起点。復元元ファイル選択、候補取得、候補0件時の通知、復元ダイアログ表示、復元実行、復元後の再読込・Dirty化・最終成否判定、成功通知とログ出力を行う。 |
| `Gui.Function.ShikigamiRecoveryFunction` | 現行CSVパスとバックアップ作成デリゲートを保持し、候補取得（`GetRecoveryCandidates`）と復元実行（`Recover`）を提供する。`Recover` では現行CSV存在確認、バックアップ作成、`Shikigami.Recovery` `BackupCompleted` ログ出力後にGateway呼び出しを行う。 |
| `Gui.Dialog.ShikigamiRecoveryDialog` | 復元候補一覧の表示と選択UIを提供する。全選択/全解除、選択件数の収集、未選択時の案内表示、OK/Cancelの確定を行う。 |
| `Gui.ShikigamiData.ShikigamiDataFileManager` | `CreateBackup()` により現行CSVを `Data\Backup` に時刻付きファイル名でコピーし、古いバックアップを整理する。`RestoreDefaultIfMissing()` / `MoveBrokenFile()` は復元後再読込時の自動復旧経路で関与し得る。 |
| `Gui.ShikigamiData.ShikigamiDataErrorHandler` | 候補取得・復元実行・復元後再読込関連のOutcome失敗および例外に対して、ログ出力とメッセージ表示を行う。`logOperation` が `Shikigami.Recovery` のため、Outcome失敗は本フローでは `Failed` ログ分岐となる。 |
| `Gui.Function.ShikigamiSelectionFunction` | `InitializeShikigamiComboBox(out autoRestored)` により式神一覧を再読込し、必要時に自動復旧処理（Broken退避・既定復元・再読込）へ移行する。`autoRestored == true` は自動復旧処理への移行を示し、Broken退避・既定CSV復元・再読込の成功を保証しない。復元後のGUI再読込と `autoRestored` 判定に使用される。 |
| `Gateway.Service.ShikigamiGateway` | 候補取得（`GetRecoveryCandidateShikigamiList`）と復元実行（`RecoveryShikigami`）をCoreへ橋渡しする。`RecoveryShikigami` は選択式神リストのネイティブ変換時に発生する `ArgumentNullException` を捕捉して `INVALID_ARGUMENT` を返す。`GetRecoveryCandidateShikigamiList` には同様の例外捕捉・Outcome変換処理はない。 |
| `Gateway.Mapper.Outcome.ShikigamiDataOutcomeMapper` | Coreの `ShikigamiDataOutcome` を `ShikigamiDataOutcomeDto` に変換する。 |
| `Core.Service.ShikigamiService` | 候補取得・復元実行を `ShikigamiRepository` に委譲する。 |
| `Core.Shikigami.ShikigamiRepository` | 候補抽出（`getRecoveryCandidateShikigamiList`）と復元実行（`recoverShikigamiList`）を実装する。復元元パスを `mRecoverySourceFilePath` に保持し、復元対象再判定、マージ、保存を行う。 |
| `Core.IO.FileAccess` | 現行データ読込（`loadShikigami`）、復元元データ読込（`loadValidShikigami`）、保存（`saveShikigami`）を提供する。 |
| `Core.Mapper.Outcome.FileAccessOutcomeMapper` | `FileAccessOutcome` を `ShikigamiDataOutcome` に変換する。 |
| `Core.Outcome.*` / `Gateway.Dto.Outcome.ShikigamiDataOutcomeDto` | 処理結果を表すOutcome/Dto。`ShikigamiDataOutcomeDto` には `INVALID_ARGUMENT` が含まれる。 |

## 4. 復元元CSVの選択と候補取得

### 4.1 ファイル選択

`MainForm.btnRecoveryShikigami_Click` は `OpenFileDialog` を表示し、次の設定で復元元CSVを選択させる。

- タイトル: `復旧元の式神データを選択してください。`
- フィルタ: `式神データ (*.csv)|*.csv`
- 初期ディレクトリ: `AppPath.DataBackupDirectoryPath`

ダイアログ結果が `DialogResult.OK` でない場合は処理を終了する。

### 4.2 候補取得

ファイル選択後、`ShikigamiRecoveryFunction.GetRecoveryCandidates(dialog.FileName, out recoveryShikigamiList)` を呼び出す。

- 呼び出し経路: `ShikigamiRecoveryFunction` → `ShikigamiGateway.GetRecoveryCandidateShikigamiList(currentFilePath, sourceFilePath, ...)` → `ShikigamiService.getRecoveryCandidateShikigamiList(...)` → `ShikigamiRepository.getRecoveryCandidateShikigamiList(...)`
- Outcomeが `SUCCESS` 以外の場合、`ShikigamiDataErrorHandler.Handle(outcome, "式神復旧候補読み込み", "Shikigami.Recovery")` がエラーログとダイアログを表示し、手動復元処理を終了する。
- 候補取得中に例外が発生した場合、`ShikigamiDataErrorHandler.Handle(ex, "式神復旧候補読み込み", "Shikigami.Recovery")` によりエラーログとダイアログを表示して終了する。

### 4.3 候補0件時

候補取得が `SUCCESS` かつ `recoveryShikigamiList.Count == 0` の場合、次を行って終了する。

- 情報ダイアログ表示: タイトル `式神復旧候補読み込み`、本文 `復旧できる式神はありませんでした。`（`MessageBoxButtons.OK`, `MessageBoxIcon.Information`）
- ログ出力: `Logger.Info("Shikigami.Recovery", "CandidatesEmpty")`

この場合、復元ダイアログ表示やバックアップ作成、`Recover` 実行は行わない。

## 5. 復元候補の抽出条件

`ShikigamiRepository.getRecoveryCandidateShikigamiList` は次の順序で候補を抽出する。

1. 現行CSV（`currentFilePath`）を `FileAccess::loadShikigami` で読み込む。読込Outcomeが `SUCCESS` 以外の場合は、`FileAccessOutcomeMapper` により変換した `ShikigamiDataOutcome` を返す。
2. 復元元CSV（`sourceFilePath`）を `FileAccess::loadValidShikigami` で読み込む。読込Outcomeが `SUCCESS` 以外の場合は、変換した `ShikigamiDataOutcome` を返す。
3. `mRecoverySourceFilePath = sourceFilePath` を設定する。
4. `extractUnknownShikigamiList(currentShikigamis, sourceShikigamis)` の結果を候補として返し、`ShikigamiDataOutcome::SUCCESS` を返す。

抽出条件と関連する読込特性は次のとおりである。

- 現行CSV読込（`loadShikigami`）はヘッダー行の一致を要求し、行解析失敗（`invalid_argument` / `out_of_range`）やストリーム異常を検出した場合は失敗Outcomeを返す。空行は読み飛ばす。
- 復元元CSV読込（`loadValidShikigami`）は各行を順に解析し、空行および解析に失敗した行（`invalid_argument` / `out_of_range`）を読み飛ばして継続する。`catch (...)` 時は出力をクリアして `UNKNOWN_ERROR` を返し、`file.bad()` 時は `FILE_READ_FAILED` を返す。ヘッダー行を特別扱いする実装はなく、ヘッダー相当行は解析結果に応じて読み飛ばされるか、解析可能であれば通常行として扱われ得る（現行実装の動作）。
- 候補抽出は、復元元データの各要素について現行データに同一式神（`Rarity` と `Name` が一致）が存在しないものを、復元元データの並び順で抽出する。

## 6. 復元候補ダイアログの操作と確定条件

`ShikigamiRecoveryDialog` は候補取得結果を受け取り、次の表示・操作を提供する。

- コンストラクタで候補リストを保持し、`loadRecoveryCandidates()` と `updateRecoveryCandidateCount()` を実行する。
- `dgvRecoveryCandidates` に候補を1行ずつ追加し、復旧チェック列の初期値を `true`、表示列にレアリティ/式神名/攻撃/HP/防御/素早さを設定する。各行の `Tag` に対応する `ShikigamiDto` を保持する。
- `lblRecoveryCandidateCount` に `復旧候補：{候補件数}件` を表示する。
- `全選択` ボタンは全行の復旧チェック列を `true` にする。
- `全解除` ボタンは全行の復旧チェック列を `false` にする。
- `復旧` ボタン押下時、チェックされた行の `Tag` を `SelectedRecoveryCandidates` に追加する。
  - チェック件数が0件の場合、`MessageBox.Show("復旧する式神を選択してください。")` を表示し、ダイアログは閉じない（`DialogResult` を設定しない）。
  - 1件以上選択されている場合、`DialogResult.OK` を設定してダイアログを閉じる。
- `キャンセル` ボタン押下時、`DialogResult.Cancel` を設定してダイアログを閉じる。

`MainForm` はダイアログ表示後、`DialogResult != DialogResult.OK` の場合は復元処理を終了する。`DialogResult.OK` の場合のみ、選択件数をログ用に取得して復元実行へ進む。

## 7. 復元実行前の存在確認・バックアップ

`MainForm` が復元ダイアログでOKを受け取ると、`Logger.Info("Shikigami.Recovery", "Started", $"SelectedCount={selectedCount}")` を記録してから `ShikigamiRecoveryFunction.Recover(selectedList)` を呼び出す。

`ShikigamiRecoveryFunction.Recover` の実行順序は次のとおりである。

1. 現行CSV（コンストラクタで受け取った `currentFilePath`、MainFormでは `AppPath.ShikigamiDataCsvPath`）の存在を `File.Exists` で確認する。存在しない場合は `FileNotFoundException("Current Shikigami data file was not found.", mCurrentFilePath)` を送出する。
2. `mCreateBackup()` を実行する（MainFormからは `ShikigamiDataFileManager.CreateBackup` が渡される）。
3. `Logger.Info("Shikigami.Recovery", "BackupCompleted")` を記録する。
4. `ShikigamiGateway.RecoveryShikigami(mCurrentFilePath, selectedShikigamiList)` を実行し、Outcomeを返す。

`ShikigamiDataFileManager.CreateBackup()` は次を行う。

- 現行CSVが存在しない場合、`FileNotFoundException` を送出する。
- `yyyyMMdd_HHmmss` 形式のタイムスタンプで `ShikigamiData_{timestamp}.csv` を作成し、`AppPath.DataBackupDirectoryPath` へ `File.Copy(..., overwrite: true)` する。
- `Data\Backup` 内の `ShikigamiData_*.csv` を `File.GetCreationTime` 降順で列挙し、先頭10件を除いて削除する（`MAX_BACKUP_COUNT = 10`）。

バックアップ作成後に `Recover` 内後続処理やGateway/Core処理で失敗した場合でも、作成済みバックアップを自動で元に戻す処理（ロールバック）は実装上確認できない。

## 8. Coreでの復元対象再判定・マージ・保存

`ShikigamiRepository.recoverShikigamiList(currentFilePath, selectedShikigamis)` は次の順序で処理する。

1. 現行CSVを `FileAccess::loadShikigami` で読み込む。失敗時はマップした `ShikigamiDataOutcome` を返す。
2. 復元元CSVを `FileAccess::loadValidShikigami(mRecoverySourceFilePath, sourceShikigamis)` で読み込む。失敗時はマップした `ShikigamiDataOutcome` を返す。
3. `recoveryTargetShikigamis = extractUnknownShikigamiList(currentShikigamis, selectedShikigamis)` により、選択された式神のうち復元実行時点の現行データに存在しないものを再判定する。
4. `recoveryTargetShikigamis` が空の場合、保存を行わず `ShikigamiDataOutcome::SUCCESS` を返す（詳細は第9章）。
5. `mergeShikigamiList(currentShikigamis, sourceShikigamis, recoveryTargetShikigamis)` でマージ結果を作成する。
6. `FileAccess::saveShikigami(currentFilePath, mergedShikigamis)` で保存する。保存成功時は `mRecoverySourceFilePath` をクリアし、保存Outcomeをマップして返す。

マージ処理の要点は次のとおりである。

- `mergeShikigamiList` は現行データのコピーを基点とし、復元元データ（`sourceShikigamis`）を先頭から走査する。
- 復元元要素が `recoveryTargetShikigamis` に存在する場合のみ、その復元元要素を挿入対象とする（挿入データは復元元読込結果の要素）。
- 挿入位置は `findMergeInsertIndex` により決定する。復元元内で対象より前方にあり、かつマージ結果に存在する最寄りの式神の直後を優先し、前方アンカーがない場合は後方にある最寄りの式神の直前を使用する。アンカーが見つからない場合はレアリティ順に基づく `findInsertIndex` にフォールバックする。

`FileAccess::saveShikigami` は既存CSVを直接上書きせず、一時ファイルへ全データを書き込んだ後に `ReplaceFileW` で置換する。置換失敗時は、置換先が存在する場合に一時ファイルを削除し、存在しない場合は一時ファイルを残す実装である。

## 9. 保存なしSUCCESSと保存ありSUCCESSの違い

Coreの `recoverShikigamiList` が返す `ShikigamiDataOutcome::SUCCESS` には、少なくとも次の2パターンがある。

| パターン | 条件 | 実装上の挙動 |
|---|---|---|
| 保存なしSUCCESS | 現行データと選択データの再判定結果 `recoveryTargetShikigamis` が空 | `saveShikigami` を呼び出さず `SUCCESS` を返す。`mRecoverySourceFilePath` はクリアされない。 |
| 保存ありSUCCESS | `recoveryTargetShikigamis` が1件以上あり、`saveShikigami` が `SUCCESS` | マージ結果を保存し、`mRecoverySourceFilePath` をクリアして `SUCCESS` を返す。 |

補足:

- 候補取得結果が0件の場合、`MainForm` は情報表示と `CandidatesEmpty` ログ後に処理を終了するため、`Recover` は実行されない。
- `ShikigamiRecoveryDialog` は未選択のままOK確定できないため、通常のGUI操作では「0件選択でRecover実行」は発生しない。
- ただしCore実装上は、`selectedShikigamis` の全件が復元実行時点で現行データに存在する等により再判定結果が空になった場合、保存なしで `SUCCESS` を返す。
- いずれの `SUCCESS` もCore復元処理の結果であり、GUI再読込や `autoRestored` 判定を含む手動復元フロー全体の成功を意味しない。

## 10. GUI再読込とDirty化

`MainForm` は `mShikigamiRecoveryFunction.Recover(...)` のOutcomeが `ShikigamiDataErrorHandler.Handle(...)` によりエラー扱いされなかった場合（通常は `SUCCESS`）、次の順序で処理する。

1. `bool reloadSucceeded = mShikigamiSelectionFunction.InitializeShikigamiComboBox(out autoRestored);`
2. `markCalculationResultDirty();`

`InitializeShikigamiComboBox(out autoRestored)` は、初回読込が非成功Outcomeかつ `HandleInitialLoadRejected` が `true` を返した場合に自動復旧処理（Broken退避・既定復元・再読込）へ移行する。このとき `autoRestored == true` となるが、これは自動復旧処理への移行を示すものであり、Broken退避・既定CSV復元・再読込の成功を保証しない。したがって、復元後のGUI再読込時にも `autoRestored == true` となる可能性がある。再読込処理の詳細な成功/失敗時挙動は [式神データ自動復旧仕様](ShikigamiDataAutoRepair.md) を参照する。

`markCalculationResultDirty()` は `mCalculationResultFunction.MarkDirty(source)` を呼び出す（`MainForm` 実装）。復元後フローでは再読込結果の成否判定や成功通知の前に実行される。

## 11. `reloadSucceeded` / `autoRestored` による最終成功判定

復元Outcomeが成功扱いでGUI再読込とDirty化を実行した後、`MainForm` は次の順序で最終判定する。

1. `reloadSucceeded == false` の場合
   - `Logger.Error("Shikigami.Recovery", "Failed", "Outcome=GuiReloadFailed")` を記録する。
   - 成功通知および `Completed` ログは行わず、手動復元処理を終了する。
2. `autoRestored == true` の場合
   - `Logger.Error("Shikigami.Recovery", "Failed", "Outcome=RecoveryDataReplaced")` を記録する（ログ名は実装どおり）。
   - 再読込時に自動復旧処理へ移行したことを示し、自動復旧処理の成功やデータ置換の完了を意味しない。
   - 成功通知および `Completed` ログは行わず、手動復元処理を終了する。
3. 上記のいずれでもない場合
   - 情報ダイアログを表示する（タイトル `式神復旧`、本文 `式神の復旧に成功しました。`、`MessageBoxButtons.OK`, `MessageBoxIcon.Information`）。
   - `Logger.Info("Shikigami.Recovery", "Completed", $"SelectedCount={selectedCount}")` を記録する。

このため、Core復元Outcomeが `SUCCESS` であっても、再読込失敗または再読込時に自動復旧処理へ移行した場合（`autoRestored == true`）は、手動復元フロー全体としては失敗扱いとなる。

## 12. Outcome失敗と例外処理

### 12.1 候補取得時

- `GetRecoveryCandidates` のOutcomeが `SUCCESS` 以外: `ShikigamiDataErrorHandler.Handle(outcome, "式神復旧候補読み込み", "Shikigami.Recovery")` がログとエラーダイアログを表示し、`MainForm` は復元処理を終了する。
- 例外発生: `ShikigamiDataErrorHandler.Handle(ex, "式神復旧候補読み込み", "Shikigami.Recovery")` がエラーログとダイアログを表示し、終了する。

`ShikigamiDataErrorHandler.Handle(Outcome, ..., "Shikigami.Recovery")` は、`Shikigami.Recovery` がデータ編集系ログ操作名（`Shikigami.Register` / `Shikigami.Edit`）ではないため、`DUPLICATE` / `INVALID_ARGUMENT` を含む失敗Outcomeを `Failed` として記録する。

### 12.2 復元実行時

- `Recover` のOutcomeが `SUCCESS` 以外: `ShikigamiDataErrorHandler.Handle(outcome, "式神復旧", "Shikigami.Recovery")` がログとエラーダイアログを表示し、`MainForm` は処理を終了する（再読込・Dirty化・最終判定へ進まない）。
- 例外発生: `ShikigamiDataErrorHandler.Handle(ex, "式神復旧", "Shikigami.Recovery")` がエラーログとダイアログを表示する。呼び出し箇所は `btnRecoveryShikigami_Click` 内の `try` ブロックであり、`MainForm` 側で復旧処理の成功通知や `Completed` ログは出力しない。

### 12.3 実装から確認できる主な失敗要因

| 段階 | 主な要因（実装上確認できる範囲） |
|---|---|
| 候補取得 | 現行CSV読込失敗（`loadShikigami` の `FILE_OPEN_FAILED` / `INVALID_FORMAT` / `FILE_READ_FAILED` / `UNKNOWN_ERROR`）、復元元CSV読込失敗（`loadValidShikigami` の `FILE_OPEN_FAILED` / `FILE_READ_FAILED` / `UNKNOWN_ERROR`）等 |
| 復元実行前 | 現行CSV不存在（`Recover` 内 `FileNotFoundException`）、バックアップ作成失敗（`CreateBackup` 内 `FileNotFoundException` や `File.Copy` 等の例外） |
| Core復元処理 | `ShikigamiGateway.RecoveryShikigami` における選択式神リストのネイティブ変換時 `ArgumentNullException` 捕捉による `INVALID_ARGUMENT`、現行CSV/復元元CSVの再読込失敗、保存失敗（`FILE_OPEN_FAILED` / `FILE_WRITE_FAILED` / `UNKNOWN_ERROR`）等 |
| 復元後GUI反映 | 再読込失敗（`reloadSucceeded == false`）、再読込時の自動復旧処理への移行（`autoRestored == true`）、再読込処理中の例外（`InitializeShikigamiComboBox` 内で捕捉・処理される） |

## 13. 復元途中の失敗時に保証されること・されないこと

### 13.1 実装から確認できること

- `Recover` はCore復元呼び出し前に現行CSVの存在確認とバックアップ作成を試行し、バックアップ作成成功時には `BackupCompleted` を記録する。
- Coreの `saveShikigami` は一時ファイルへの書き込み完了後に `ReplaceFileW` で置換する実装であり、直接の逐次上書きとは異なる。
- GUIの手動復元フローは、復元Outcome成功後でも再読込失敗または再読込時に自動復旧処理へ移行した場合（`autoRestored == true`）に成功通知・`Completed` ログを出さず失敗扱いとする。`autoRestored == true` は自動復旧処理への移行を示し、その成否を意味しない。

### 13.2 実装上保証されないこと（確認できる範囲）

- バックアップ作成後にCore復元・保存・GUI再読込等で失敗しても、バックアップからの自動ロールバックは行われない。
- ファイル存在確認、バックアップ作成、Coreでの再読込/マージ/保存、GUI再読込、Dirty化、最終通知の各段階は一連の原子的操作として保証されない。失敗発生位置により、バックアップ有無、現行CSV内容、GUI表示状態、Dirty状態、ログ/通知状態が異なり得る。
- `saveShikigami` の置換失敗時は状況により一時ファイルを削除または残置するが、手動復元フロー全体としてのファイル整合性復旧は保証されない。
- 保存なしSUCCESSの場合、Coreは `mRecoverySourceFilePath` をクリアしない。復元元パスの保持状態に関する追加の保証は実装上確認できない。
- `loadValidShikigami` は不正行を読み飛ばして継続するため、復元元CSV内のすべての行が候補抽出やマージ元として有効に扱われることは保証されない。

## 14. ログとユーザー通知

| タイミング | 種別 | 内容 |
|---|---|---|
| 候補取得成功かつ候補0件 | ログ（Info）+ 情報ダイアログ | `Logger.Info("Shikigami.Recovery", "CandidatesEmpty")`。タイトル `式神復旧候補読み込み`、本文 `復旧できる式神はありませんでした。` |
| 候補取得失敗（Outcome） | ログ（Error）+ エラーダイアログ | `ShikigamiDataErrorHandler.Handle(outcome, "式神復旧候補読み込み", "Shikigami.Recovery")` により `Result=Failed`, `Outcome=...` を記録し、Outcomeに応じたメッセージを表示する。 |
| 候補取得失敗（例外） | ログ（Error）+ エラーダイアログ | `Result=Failed` と例外情報を記録し、`式神復旧候補読み込み中に予期しないエラーが発生しました。` を表示する。 |
| ダイアログで復旧対象未選択のまま復旧押下 | ダイアログ内メッセージ | `MessageBox.Show("復旧する式神を選択してください。")`。ダイアログは開いたまま。 |
| 復元開始 | ログ（Info） | `Logger.Info("Shikigami.Recovery", "Started", $"SelectedCount={selectedCount}")` |
| バックアップ作成完了 | ログ（Info） | `ShikigamiRecoveryFunction.Recover` 内で `Logger.Info("Shikigami.Recovery", "BackupCompleted")` |
| 復元失敗（Outcome） | ログ（Error）+ エラーダイアログ | `ShikigamiDataErrorHandler.Handle(outcome, "式神復旧", "Shikigami.Recovery")` により `Result=Failed`, `Outcome=...` を記録し、Outcomeに応じたメッセージを表示する。 |
| 復元失敗（例外） | ログ（Error）+ エラーダイアログ | `Result=Failed` と例外情報を記録し、`式神復旧中に予期しないエラーが発生しました。` を表示する。 |
| 復元後再読込失敗 | ログ（Error） | `Logger.Error("Shikigami.Recovery", "Failed", "Outcome=GuiReloadFailed")`。成功通知なし。 |
| 復元後再読込で自動復旧処理へ移行（`autoRestored == true`） | ログ（Error） | `Logger.Error("Shikigami.Recovery", "Failed", "Outcome=RecoveryDataReplaced")`。自動復旧処理への移行を示し、その成否やデータ置換の完了を意味しない。成功通知なし。 |
| 手動復元フロー成功 | 情報ダイアログ+ ログ（Info） | タイトル `式神復旧`、本文 `式神の復旧に成功しました。`。`Logger.Info("Shikigami.Recovery", "Completed", $"SelectedCount={selectedCount}")` |

補足:

- 候補取得失敗や復元失敗のOutcome/例外ログは `logOperation=Shikigami.Recovery` で記録される。`ShikigamiDataErrorHandler` 実装上のValidationRejected分岐（`Shikigami.Register` / `Shikigami.Edit` かつ `DUPLICATE` / `INVALID_ARGUMENT`）には該当しない。
- 復元後のGUI再読込処理自体は `ShikigamiSelectionFunction` / `ShikigamiDataErrorHandler` により `Shikigami.Reload` 系ログや初回失敗通知を出力する場合がある（詳細は自動復旧仕様を参照）。

## 15. 関連資料

- [式神データ手動復元シーケンス図](../../Diagram/ShikigamiData/shikigami_recovery_sequence.wsd)
- [式神データ手動復元シーケンス図（PDF）](../../Diagram/ShikigamiData/shikigami_recovery_sequence.pdf)
- [式神データ自動復旧仕様](ShikigamiDataAutoRepair.md)
- [式神データ自動復旧シーケンス図](../../Diagram/ShikigamiData/shikigami_auto_repair_sequence.wsd)
- [式神データ自動復旧シーケンス図（PDF）](../../Diagram/ShikigamiData/shikigami_auto_repair_sequence.pdf)
