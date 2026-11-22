# 式神データ自動復旧仕様

## 1. 目的・対象範囲

本書は、ShikigamiApp の Gui における式神データCSV（`Data\ShikigamiData.csv`）の欠落時、および式神データ読込失敗時に実行される自動復旧機能を、現行実装に基づいて整理する。

対象は、アプリ起動時の既定データ復元（`Program` / `ApplicationDirectoryInitializer` / `ShikigamiDataFileManager`）と、式神一覧読込時の自動復旧（`ShikigamiSelectionFunction` / `ShikigamiDataErrorHandler` / `ShikigamiDataFileManager`）、ならびに読込に関わる `Gateway` / `Core` の結果伝播である。

対象外は、ユーザー操作による式神データ復元機能（復旧元ファイル選択、復旧候補抽出、選択、マージ、バックアップ作成を含む）、式神登録・編集・保存処理、`loadValidShikigami` を用いた手動復旧向け読込、および未実装の将来仕様である。

## 2. 関連クラスと責務

| クラス / 型 | 主な責務（本仕様に関係する範囲） |
|---|---|
| `Gui.Program` | アプリ起動時の初期化順序制御。`ApplicationDirectoryInitializer.Initialize()`、`ShikigamiDataFileManager.RestoreDefaultIfMissing()`、`MainForm`生成を実行する。初期化中の例外を捕捉してログ出力後に再送出する。 |
| `Gui.ApplicationDirectoryInitializer` | `Data`、`Data\Backup`、`Data\Broken`、`SaveData`配下、`Log` 等のディレクトリ作成を行う。 |
| `Gui.Definition.AppPath` | 式神データCSV、Broken保存先、Backup保存先などのパスを提供する。 |
| `Gui.ShikigamiData.ShikigamiDataFileManager` | 起動時/復旧時の既定CSV復元（`RestoreDefaultIfMissing`）、読込失敗時の破損ファイル退避（`MoveBrokenFile`）、Brokenファイルの世代管理を行う。埋め込みリソース `Gui.Data.ShikigamiData.csv` を使用する。 |
| `Gui.Function.ShikigamiSelectionFunction` | 式神一覧読込（`InitializeShikigamiComboBox`）を実行し、初回読込が非成功Outcomeの場合に自動復旧（退避→既定復元→再読込）を行う。読込結果に基づいて内部リスト・ComboBox表示・選択状態・基礎ステータス表示を更新する。 |
| `Gui.Operator.ShikigamiSelectionOperator` | レアリティフィルタ初期化、式神一覧のUI反映、選択解除、基礎ステータス表示更新を行う。 |
| `Gui.ShikigamiData.ShikigamiDataErrorHandler` | 初回読込失敗（`HandleInitialLoadRejected`）と再読込/通常失敗（`Handle(Outcome, ...)`）、例外（`Handle(Exception, ...)`）に応じたログ出力とメッセージ表示を行う。 |
| `Gui.Form.MainForm` | 起動時初期化、再読込ボタン、式神登録/編集後の再読込などから `ShikigamiSelectionFunction.InitializeShikigamiComboBox` を呼び出す。 |
| `Gateway.Service.ShikigamiGateway` | GuiからCoreへの式神一覧取得要求を橋渡しし、`ShikigamiDataOutcome` を `ShikigamiDataOutcomeDto` に変換して返す。 |
| `Gateway.Mapper.Outcome.ShikigamiDataOutcomeMapper` | Coreの `ShikigamiDataOutcome` をDtoへ変換する（`INVALID_ARGUMENT` への変換はこのMapperにはない）。 |
| `Core.Service.ShikigamiService` | 式神一覧取得を `ShikigamiRepository` に委譲する。 |
| `Core.Shikigami.ShikigamiRepository` | `FileAccess::loadShikigami` の結果を `FileAccessOutcomeMapper` で `ShikigamiDataOutcome` に変換して返す。 |
| `Core.IO.FileAccess` | CSV読込を行う。`loadShikigami` はヘッダー検証・行解析・ストリーム状態を確認し、`FileAccessOutcome` を返す。 |
| `Core.Mapper.Outcome.FileAccessOutcomeMapper` | `FileAccessOutcome` を `ShikigamiDataOutcome` に変換する。 |
| `Core.Outcome.*` / `Gateway.Dto.Outcome.ShikigamiDataOutcomeDto` | 読込結果を表すOutcome/Dto。 |

## 3. 自動復旧の起動条件

現行実装で確認できる自動復旧の起動条件は次の2系統である。

| 系統 | 起動箇所 | 条件 | 実行内容 |
|---|---|---|---|
| アプリ起動時のCSV欠落対応 | `Program.Main` | `ApplicationDirectoryInitializer.Initialize()` 完了後に `RestoreDefaultIfMissing()` を呼び出す。`AppPath.ShikigamiDataCsvPath` が存在しない場合のみ復元処理を試行する。 | 埋め込みCSVから既定ファイルを作成する（既存ファイルがある場合は何もしない）。この時点ではCSV内容の読込検証は行わない。 |
| 式神データ読込失敗時の自動復旧 | `ShikigamiSelectionFunction.InitializeShikigamiComboBox(out bool autoRestored)` | `ShikigamiGateway.GetShikigamiList` の初回結果が `ShikigamiDataOutcomeDto.SUCCESS` 以外であり、`ShikigamiDataErrorHandler.HandleInitialLoadRejected(...)` が `true` を返した場合。 | 破損/読込不可ファイルの退避（`MoveBrokenFile`）、既定CSV復元（`RestoreDefaultIfMissing`）、同一パスでの再読込を1回試行する。 |

補足:

- `InitializeShikigamiComboBox()` は `InitializeShikigamiComboBox(out autoRestored)` を呼び出すオーバーロードである。呼び出し元が `autoRestored` を参照しない場合でも、自動復旧判定と復旧処理の実行有無は同一である。
- `InitializeShikigamiComboBox` は `MainForm` の初期化、再読込ボタン、式神登録/編集後の再読込など複数経路から呼ばれる。したがって読込失敗時の自動復旧は起動直後の初回読込に限定されない。
- ユーザー操作による式神データ復元（復旧元CSV選択・候補抽出・マージ）とは別機能である。本書は埋め込み既定データへの自動復旧のみを扱う。

## 4. アプリ起動時のCSV欠落対応

### 4.1 実行順序

`Program.Main` は次の順序で初期化を行う（すべて同一 `try` ブロック内）。

1. `Application.EnableVisualStyles()` / `SetCompatibleTextRenderingDefault(false)`
2. `ApplicationDirectoryInitializer.Initialize()`
3. `ShikigamiDataFileManager.RestoreDefaultIfMissing()`
4. `MainForm` 生成

初期化中に例外が発生した場合、`Logger.Error("Application", "InitializationFailed", ex)` を記録して例外を再送出する。`Application.Run(mainForm)` には到達しない。

### 4.2 `RestoreDefaultIfMissing` の挙動

- 対象パスは `AppPath.ShikigamiDataCsvPath`（`<BaseDirectory>\Data\ShikigamiData.csv`）である。
- ファイルが存在する場合は復元処理を行わずに戻る。
- ファイルが存在しない場合、アセンブリのマニフェストリソース `Gui.Data.ShikigamiData.csv` を取得する。
  - ストリーム取得に成功した場合、UTF-8（BOMなし）で読み取り、同エンコードで `File.WriteAllText` により書き込む。成功時は `Logger.Info("Shikigami.AutoRestore", "Completed")` を記録する。
  - ストリームが `null` の場合、`Logger.Error("Shikigami.AutoRestore", "Failed", "Outcome=ResourceNotFound Resource=Gui.Data.ShikigamiData.csv")` を記録して戻る。例外は送出しないため、CSVが存在しない状態のまま後続処理へ進む可能性がある。
  - 読み取り/書き込み等で例外が発生した場合、`Logger.Error("Shikigami.AutoRestore", "Failed", "Outcome=UnexpectedException ExceptionType=...")` を記録して例外を再送出する。

起動時のこの処理は「欠落時の既定ファイル作成」であり、既存ファイルの内容検証や破損判定は行わない。

## 5. 式神データ読込失敗時の自動復旧

### 5.1 読込と初回失敗判定

`ShikigamiSelectionFunction.InitializeShikigamiComboBox(out bool autoRestored)` の主な流れは次のとおりである。

1. `autoRestored = false`、`loadSucceeded = true` として開始し、`Logger.Info("Shikigami.Reload", "Started")` を記録する。
2. `ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out shikigamiList)` を実行する。
3. 戻り値Outcomeが `SUCCESS` 以外の場合、`ShikigamiDataErrorHandler.HandleInitialLoadRejected(outcome, "式神データ読み込み")` を呼ぶ。
   - このハンドラは `Logger.Warning("Shikigami.Reload", "InitialLoadRejected", $"Outcome={outcome}")` を記録し、操作名「式神データ読み込み」でエラーダイアログを表示して `true` を返す。
4. 初回失敗と判定された場合、`autoRestored = true` を設定した上で自動復旧処理（5.2）を実行する。
5. 初回読込が成功した場合、自動復旧は行わない。`loadSucceeded` が `true` かつ `shikigamiList == null` の場合は `InvalidOperationException("Shikigami list is null.")` を送出する。

### 5.2 自動復旧シーケンス

初回失敗時に実行される処理は、次の順序である。

1. `ShikigamiDataFileManager.MoveBrokenFile()`：現行CSVが存在する場合にBroken領域へ退避し、古いBrokenファイルを整理する（詳細は第6章）。
2. `ShikigamiDataFileManager.RestoreDefaultIfMissing()`：CSVが存在しない場合に埋め込み既定CSVを復元する（詳細は第4章）。
3. `ShikigamiGateway.GetShikigamiList(...)` を再実行する。
4. 再読込Outcomeを `ShikigamiDataErrorHandler.Handle(outcome, "式神データ復元後読み込み", "Shikigami.Reload")` で判定する。
   - 非成功の場合、ログ記録とメッセージ表示を行い、`shikigamiList = new List<ShikigamiDto>()`、`loadSucceeded = false` とする。
   - 成功の場合、`loadSucceeded` は `true` のまま再読込結果を使用する。

`autoRestored` は手順1の前に `true` に設定される。したがって、退避・復元・再読込の途中で例外が発生した場合や再読込が失敗した場合でも、呼び出し元が `out` 値を参照できる状況では `true` となり得る。

### 5.3 読込経路（Gateway / Core）

`GetShikigamiList` のOutcomeは次の経路で伝播する。

1. `ShikigamiGateway` がパスをネイティブ文字列へ変換し `ShikigamiService::getShikigamiList` を呼ぶ。Outcomeが非成功の場合はMapperでDtoへ変換して返し、成功時はリスト変換後に `SUCCESS` を返す。
2. `ShikigamiService` は `ShikigamiRepository::getShikigamiList` に委譲する。
3. `ShikigamiRepository` は `FileAccess::loadShikigami` を呼び、`FileAccessOutcomeMapper::toShikigamiDataOutcome` で変換する。
4. `FileAccess::loadShikigami` はファイルオープン失敗時に `FILE_OPEN_FAILED`、ヘッダー未読取/ヘッダー不一致/行解析失敗（`invalid_argument`・`out_of_range`）時に `INVALID_FORMAT`、`file.bad()` 時に `FILE_READ_FAILED`、その他例外時に `UNKNOWN_ERROR` を返す。行解析失敗時は出力リストをクリアする。

## 6. Brokenファイルの退避・保持規則

`MoveBrokenFile()` の挙動は次のとおりである。

- 対象ファイル（`AppPath.ShikigamiDataCsvPath`）が存在しない場合は何もせず戻る。
- 存在する場合、`DateTime.Now.ToString("yyyyMMdd_HHmmss")` を用いて `ShikigamiData_broken_{timestamp}.csv` を作成し、`AppPath.DataBrokenDirectoryPath`（`Data\Broken`）へ `File.Move` する。
- 移動成功後、`Logger.Info("Shikigami.BrokenFile", "Moved", $"FileName={Path.GetFileName(brokenFilePath)}")` を記録する。
- 続けて `Data\Broken` 内の `ShikigamiData_broken_*.csv` を `File.GetCreationTime` の降順で列挙し、先頭30件を除くファイルを削除する（`MAX_BROKEN_COUNT = 30`）。

補足:

- `ApplicationDirectoryInitializer.Initialize()` は起動時に `Data\Broken` ディレクトリを作成する。`MoveBrokenFile` 自体はディレクトリ存在確認や例外捕捉を行わない。
- 同一秒に複数回退避が発生する場合のファイル名衝突回避や、移動/削除の原子性は実装上保証されない（`File.Move` / `File.Delete` の挙動に依存する）。

## 7. 復旧後の再読込とUI反映

### 7.1 `ShikigamiSelectionFunction` 内の反映

読込（および必要に応じた自動復旧・再読込）の `try` ブロック内では、最終的に次を実行する。

1. `mShikigamiList = shikigamiList` を設定する。
2. 式神ComboBoxの `DisplayMember` を `"Name"` に設定する。
3. 現在のレアリティフィルタに基づく一覧（`getFilteredShikigamiList()`）をComboBoxへ設定する。
4. 式神選択を解除し、基礎ステータス表示テキストを空文字にする。

再読込が失敗したOutcomeパスでは `shikigamiList` を空リストに置き換えてから上記反映を行うため、式神一覧は空、選択なし、基礎ステータス空の状態になる。フィルタ条件自体は `ShikigamiSelectionOperator` が保持するレアリティComboBoxの状態に依存する。

`loadSucceeded` が `true` の場合のみ `Logger.Info("Shikigami.Reload", "Completed")` を記録し、戻り値は `loadSucceeded` となる。

### 7.2 例外発生時

`try` ブロック内で例外が発生した場合、`catch` で `ShikigamiDataErrorHandler.Handle(ex, "式神データ読み込み", "Shikigami.Reload")` を実行する。その後、`mShikigamiList` が `null` なら空リストを設定し、`false` を返す。この例外パスでは、`try` ブロック末尾のUI反映処理（表示メンバー設定、一覧反映、選択解除、基礎ステータスクリア）を再実行しない。

## 8. 正常系・異常系と状態保証

### 8.1 正常系

| ケース | 確認できる挙動 |
|---|---|
| 起動時にCSVが存在する | `RestoreDefaultIfMissing` は何も行わない。 |
| 起動時にCSVが欠落し、埋め込み取得と書込みに成功する | 既定CSVを生成し `Shikigami.AutoRestore` Completed を記録する。 |
| 初回読込が `SUCCESS` | 自動復旧を行わず、読込結果をUIへ反映し `Shikigami.Reload` Completed を記録する。 |
| 初回読込が非成功Outcomeだが、退避・復元・再読込に成功する | 初回失敗通知後に自動復旧を実行し、再読込結果をUIへ反映して `Completed` を記録する。戻り値は `true`、`autoRestored` は `true` となる。 |

### 8.2 異常系

| ケース | 確認できる挙動 |
|---|---|
| 起動時、埋め込みリソースが見つからない | `Shikigami.AutoRestore` Failed（`Outcome=ResourceNotFound`）を記録して戻る。`RestoreDefaultIfMissing` は例外を送出しない。 |
| 起動時、既定復元中に例外が発生する | `Shikigami.AutoRestore` Failed（`Outcome=UnexpectedException`）を記録して例外を再送出する。`Program.Main` が捕捉して初期化失敗ログ後に再送出するため、起動処理は継続しない。 |
| 初回読込が非成功Outcome | `HandleInitialLoadRejected` により警告ログとエラーダイアログを表示し、自動復旧へ進む。 |
| 自動復旧後の再読込が非成功Outcome | `Handle` によりログとエラーダイアログを表示し、内部リストを空にしてUIへ反映する。`Completed` は記録されず、戻り値は `false`。`autoRestored` は `true`。 |
| 読込/復旧処理中に例外が発生する | `Handle(ex, "式神データ読み込み", "Shikigami.Reload")` によりエラーログとダイアログを表示する。`mShikigamiList` が `null` の場合は空リストを設定し、`false` を返す。 |

### 8.3 状態保証（確認できる範囲）

- 自動復旧の再読込が `SUCCESS` で完了した場合に限り、`InitializeShikigamiComboBox` は復旧後データに基づく一覧反映と `Shikigami.Reload` Completed を行う。
- 再読込が非成功Outcomeの場合、例外を伴わなければ空リスト・選択解除・基礎ステータス空のUI状態を設定する。
- 例外パスでは `mShikigamiList` の `null` 防止は行うが、UIコントロール状態の再同期やファイル状態の復元（退避/復元のロールバック）は実装上確認できない。
- `MoveBrokenFile` の移動、`RestoreDefaultIfMissing` の書込み、`MoveBrokenFile` 内の古いBroken削除、再読込の各段階は個別のファイル操作/処理であり、全体としての原子性は保証されない。例外発生時は、どの段階まで完了したかによりファイル配置・内部状態・UI状態が異なり得る。

## 9. ログとユーザー通知

| タイミング | 種別 | 内容 |
|---|---|---|
| 起動時既定復元 成功 | ログ（Info） | `Operation=Shikigami.AutoRestore`, `Result=Completed` |
| 起動時既定復元 埋め込み未検出 | ログ（Error） | `Operation=Shikigami.AutoRestore`, `Result=Failed`, `Details=Outcome=ResourceNotFound Resource=Gui.Data.ShikigamiData.csv` |
| 起動時既定復元 例外 | ログ（Error） | `Operation=Shikigami.AutoRestore`, `Result=Failed`, `Details=Outcome=UnexpectedException ExceptionType=...`（例外再送出あり） |
| 起動初期化失敗 | ログ（Error） | `Operation=Application`, `Result=InitializationFailed`（`Program.Main` で捕捉） |
| 式神一覧読込開始 | ログ（Info） | `Operation=Shikigami.Reload`, `Result=Started` |
| 初回読込失敗（自動復旧判定） | ログ（Warning）+ ダイアログ | `Operation=Shikigami.Reload`, `Result=InitialLoadRejected`, `Outcome=...`。MessageBoxはタイトル/本文に「式神データ読み込み」を用い、Outcomeに応じた文言を表示する。 |
| Broken退避成功 | ログ（Info） | `Operation=Shikigami.BrokenFile`, `Result=Moved`, `FileName=...` |
| 復旧後再読込失敗（Outcome） | ログ（Error/Warning相当は`Handle`の条件による）+ ダイアログ | 通常は `Operation=Shikigami.Reload`, `Result=Failed`, `Outcome=...`（タイトル「式神データ復元後読み込み」）。`DUPLICATE`/`INVALID_ARGUMENT` かつデータ編集系ログ操作名の場合に限りValidationRejected警告となる分岐が実装にあるが、本経路では `logOperation` が `Shikigami.Reload` のため該当しない。 |
| 読込/復旧中の例外 | ログ（Error）+ ダイアログ | `Operation=Shikigami.Reload`, `Result=Failed` と例外情報。MessageBoxは「式神データ読み込み中に予期しないエラーが発生しました。」 |
| 読込成功 | ログ（Info） | `Operation=Shikigami.Reload`, `Result=Completed` |

## 10. 関連資料

- [式神データ自動復旧シーケンス図](../../Diagram/ShikigamiData/shikigami_auto_repair_sequence.wsd)
- [式神データ自動復旧シーケンス図（PDF）](../../Diagram/ShikigamiData/shikigami_auto_repair_sequence.pdf)
- [SaveData読込・UI適用・ロールバック仕様](../SaveData/SaveDataLoadAndRollback.md)（別機能。読込失敗時のUI/状態扱いの記述様式参照先）
