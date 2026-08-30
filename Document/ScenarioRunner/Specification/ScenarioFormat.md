# ShikigamiApp Scenario Format Specification

## 1. 概要
Scenarioファイルは、ShikigamiApp Scenario Runnerで実行する試験手順を記述するためのテキストファイルである。
Scenarioファイルは、単なる自動操作スクリプトではなく、人間がそのまま試験仕様書として読めることを重視する。
そのため、FlaUI、Win32 API、AutomationId、キー入力などのUI Automation上の実装詳細はScenarioファイルに記述しない。
Scenario RunnerはScenarioファイルを上から順番に解釈し、記述された試験手順に従ってShikigamiAppおよび関連ファイルを操作する。
Scenarioファイルの拡張子は以下とする。
`.scenario`

## 2. 基本構造
1つのScenarioファイルには、1つの試験を記述する。
試験範囲は必ず `START` から `END` までとする。
例：
   # 基本計算試験
   START
   OPEN GUI
   SEL SHIKIGAMI "願紡縁結神"
   CALC
   CHECK CALC
   CLOSE GUI
   END
`START` および `END` はGui.exeの起動・終了を意味しない。
`START` は試験そのものの開始、`END` は試験そのものの終了を表す予約語とする。
そのため、Gui.exe起動前に必要となる事前条件の作成も `START` から `END` の範囲内に記述する。
例：
   START
   BREAK SHIKIGAMI HEADER
   OPEN GUI
   CHECK SHIKIGAMI
   CLOSE GUI
   END

## 3. START / END
`START` および `END` はScenarioファイルに必須とする。
以下の規則に従う。
- `START` は1回のみ記述できる。
- `END` は1回のみ記述できる。
- `START` は `END` より前に存在しなければならない。
- 実行コマンドは `START` から `END` の間にのみ記述できる。
- `START` より前に実行コマンドを記述してはならない。
- `END` より後に実行コマンドを記述してはならない。
- `START` / `END` のネストは認めない。
- コメント行および空行は `START` / `END` の外側にも記述できる。

## 4. コメント
先頭の空白を除いた最初の文字が `#` である行はコメント行として扱う。
コメント行はScenario Runnerによる実行対象にはならない。
例：
   # 基本計算試験
       # この行もコメントとして扱う
コメントは試験目的、事前条件、操作内容、期待結果などを人間向けに記述するために使用する。
Version 1では行末コメントをサポートしない。
以下の記述は認めない。
   CALC # 計算を実行する
コメントを記述する場合は独立した行とする。
   # 計算を実行する
   CALC

## 5. 空行
空行および空白文字のみで構成された行は無視する。
Scenarioファイルを試験仕様書として読みやすくするため、空行は自由に使用できる。

## 6. キーワード
Scenarioで使用するキーワードは英大文字を基本とする。
キーワードは可能な限り短くしつつ、記述内容から意味を推測できる名称とする。
長い複合キーワードは原則として使用しない。
例：
   SELECT_SHIKIGAMI
ではなく、
   SEL SHIKIGAMI
とする。
また、
   ASSERT_CALCULATION_SUCCEEDED
ではなく、
   CHECK CALC
とする。
キーワードは大文字小文字を区別する。
以下は有効とする。
   CALC
以下は構文エラーとする。
   calc
   Calc

## 7. コマンドの基本形式
コマンドは原則として以下の形式で記述する。
  COMMAND
または、
  COMMAND TARGET
または、
  COMMAND TARGET ARGUMENT
または、
  COMMAND TARGET ARGUMENT ...
コマンドによっては、複数のTARGETまたはARGUMENTを持つ。
例：
  CALC
  OPEN GUI
  SEL SHIKIGAMI "願紡縁結神"
  EQUIP MITAMA MAIN 2 "追加攻撃力"
  EQUIP MITAMA SUB 2 1 "追加攻撃力" "5"
1つのコマンドは1行に記述する。

## 8. 文字列
文字列引数はダブルクォーテーション `"` で囲む。
例：
   SEL SHIKIGAMI "願紡縁結神"
ファイルパスについても同様とする。
例：
   LOAD MITAMA "TestData/Valid.mitama.json"
特に空白を含む可能性がある値は必ずダブルクォーテーションで囲む。

## 9. Version 1 コマンド
Version 1で使用する予約語およびコマンドを以下に定義する。

### Scenario制御
#### START
試験開始位置を表す。

形式：
   START
Gui.exeの起動を意味しない。

#### END
試験終了位置を表す。

形式：
   END
Gui.exeの終了を意味しない。

### Gui操作
#### LAUNCH GUI
Gui.exeのプロセスを起動する。
形式：
   LAUNCH GUI
Gui.exeのプロセスが起動した時点でコマンドを完了する。
Gui.exeのメインウィンドウがScenario Runnerから操作可能な状態になるまでの待機は行わない。
Gui.exeの起動処理中にモーダルダイアログが表示され、
メインウィンドウが操作可能になる前にユーザー操作を必要とする試験で使用する。
起動後に表示されたダイアログを確認する場合は `CHECK DIALOG` を使用し、
ダイアログを閉じる場合は `CLOSE DIALOG` を使用する。

#### OPEN GUI
Gui.exeを起動する。
形式：
   OPEN GUI
Gui.exeが正常に起動し、Scenario Runnerから操作可能な状態になるまで待機する。
Gui.exeの起動途中に表示されるモーダルダイアログを
メインウィンドウ表示前に操作する必要がある場合は、
`OPEN GUI` ではなく `LAUNCH GUI` を使用する。

#### CLOSE GUI
起動中のGui.exeを終了する。
形式：
   CLOSE GUI

#### CLOSE DIALOG
現在表示されているモーダルダイアログを閉じる。
形式：
   CLOSE DIALOG
`CHECK DIALOG` などでダイアログの表示内容を確認した後、
同一のGui.exe上で後続の試験手順を継続する場合に使用する。
表示中のモーダルダイアログが存在しない場合は、
コマンドの実行失敗として扱う。
本コマンドはダイアログの内容を確認しない。
ダイアログの内容を確認する場合は `CHECK DIALOG` を使用する。

### 入力・データ操作
#### SEL SHIKIGAMI
指定した式神を選択する。

形式：
   SEL SHIKIGAMI "<式神名>"
例：
   SEL SHIKIGAMI "願紡縁結神"
Scenarioファイルでは、ComboBoxのAutomationId、Item index、キー操作などの実装詳細を指定しない。
指定された式神を実際に選択する方法はScenario Runner側で決定する。

#### EQUIP MITAMA
Gui.exe上で御魂のステータスまたは効果を入力する。
御魂の入力対象として `MAIN`、`SUB`、`SET`、`UNIQUE` を指定する。
Scenarioファイルでは、ComboBoxやTextBoxのAutomationId、Item index、キー操作などの実装詳細を指定しない。
指定された内容をGui.exeへ入力する方法はScenario Runner側で決定する。

##### MAIN
指定した御魂スロットのメインステータスを選択する。
形式：
  EQUIP MITAMA MAIN <御魂スロット番号> "<ステータス種類>"
例：
  EQUIP MITAMA MAIN 2 "追加攻撃力"
御魂スロット番号には1から6を指定する。
メインステータスの値はGui.exe上でステータス種類の選択に応じて設定されるため、
Scenarioファイルでは指定しない。
1番、3番、5番の御魂についてもステータス種類を省略せず指定する。
指定した御魂スロットで選択できないステータス種類が指定された場合は、
コマンドの実行失敗として扱う。

##### SUB
指定した御魂スロットのサブステータスを入力する。
形式：
  EQUIP MITAMA SUB <御魂スロット番号> <サブスロット番号> ["<ステータス種類>"] ["<値>"]
例：
  EQUIP MITAMA SUB 2 1 "追加攻撃力" "5"
御魂スロット番号には1から6、
サブスロット番号には1から4を指定する。
ステータス種類と値はそれぞれ独立して省略できる。
例：
  EQUIP MITAMA SUB 2 1 "追加攻撃力"
  EQUIP MITAMA SUB 2 1 "" "5"
値はGui.exeの入力欄へ入力する文字列として扱う。
そのため、数値として解釈できない文字列や負数など、
Gui.exe側のValidation対象となる値についてもScenario Runnerでは入力を許可する。
Scenario Runnerは入力内容の妥当性を判定せず、
指定された内容をGui.exeへ入力する。
入力内容に対するValidation結果の確認は、
`CALC` および `CHECK DIALOG` などの後続コマンドによって行う。

##### SET
指定した2セット効果を選択する。
形式：
  EQUIP MITAMA SET <番号> "<ステータス種類>"
例：
  EQUIP MITAMA SET 1 "追加攻撃力"
番号には1から3を指定する。

##### UNIQUE
指定した固有効果を選択する。
形式：
  EQUIP MITAMA UNIQUE <番号> "<ステータス種類>"
例：
  EQUIP MITAMA UNIQUE 1 "追加攻撃力"
番号には1から6を指定する。

#### LOAD MITAMA
指定された御魂セットファイルをGui.exeへ読み込む。
形式：
   LOAD MITAMA "<ファイルパス>"
例：
   LOAD MITAMA "TestData/Valid.mitama.json"
ファイルパスには相対パスまたは絶対パスを指定できる。
相対パスを指定した場合、Scenarioファイル自身が存在するディレクトリを基準として解決する。
Scenarioファイルおよび関連するテストデータを一式として移動可能にするため、
原則として相対パスの使用を推奨する。
例：
   Scenario/
   ├─ BasicCalculation.scenario
   └─ TestData/
      └─ Valid.mitama.json
上記構成の場合、Scenarioファイルからは以下のように指定する。
   LOAD MITAMA "TestData/Valid.mitama.json"
絶対パスの指定も許可する。

#### CLEAR
Gui.exeの入力内容をクリアする。
形式：
   CLEAR

#### RELOAD SHIKIGAMI
Gui.exeで式神データの再読み込みを実行する。
形式：
   RELOAD SHIKIGAMI
式神データの破損および復旧に関する試験で使用する。

#### CREATE SHIKIGAMI BACKUP
Gui.exeの式神データ編集機能を使用してBackupデータを生成する。
形式：
   CREATE SHIKIGAMI BACKUP
Scenario Runnerは、Gui.exe上で利用可能な式神を選択し、
式神データを変更せずに保存する。
これにより、Gui.exeの通常の式神データ更新処理を経由して
ShikigamiData.csvのBackupデータを生成する。
本コマンドはBackupファイルを直接作成またはコピーするものではない。
生成されたBackupデータは、同一Scenario内で
`RECOVER SHIKIGAMI BACKUP` の復旧元として使用できる。
Backupデータの生成に失敗した場合は、
コマンドの実行失敗として扱う。

#### RECOVER SHIKIGAMI
Gui.exeの式神データ復旧機能を使用して、
欠損した式神データを復旧する。
復旧元の種別として `BROKEN` または `BACKUP` を指定する。
形式：
   RECOVER SHIKIGAMI BROKEN
または、
   RECOVER SHIKIGAMI BACKUP
`BROKEN` を指定した場合、
同一Scenario内で式神データの破損および自動修復によって生成された
Brokenデータを復旧元として使用する。
`BACKUP` を指定した場合、
同一Scenario内で `CREATE SHIKIGAMI BACKUP` によって生成された
Backupデータを復旧元として使用する。
Scenarioファイルには、復旧元ファイルの具体的なファイル名または
ファイルパスを記述しない。
復旧元として使用するファイルは、
指定された復旧元種別とScenario実行中に生成された状態を基に
Scenario Runnerが決定する。
本コマンドは復旧結果の確認を行わない。
復旧成功時に表示されるダイアログを確認する場合は
`CHECK DIALOG` を使用する。

#### WAIT SHIKIGAMI AUTO REPAIR
Gui.exeによる式神データの自動修復が完了するまで待機する。
形式：
  WAIT SHIKIGAMI AUTO REPAIR
ShikigamiData.csvの破損を検出した際に実行される自動修復処理について、
Brokenデータへの退避およびShikigamiData.csvの再生成が完了するまで待機する。
本コマンドは固定時間の待機を行うものではなく、
自動修復の完了条件が満たされるまでScenario Runnerが待機する。
式神データ自動修復後の状態を利用する後続コマンドを、
自動修復処理と競合させずに実行するために使用する。
例：
  BREAK SHIKIGAMI HEADER
  LAUNCH GUI
  CHECK DIALOG "式神データファイルの形式が正しくありません。"
  CLOSE DIALOG
  WAIT SHIKIGAMI AUTO REPAIR
  REMOVE SHIKIGAMI "願紡縁結神"

### 実行操作
#### CALC
Gui.exeで計算を実行する。
形式：
   CALC

### テストデータ操作
#### BREAK SHIKIGAMI HEADER
ShikigamiData.csvのヘッダーを意図的に破損させる。
形式：
   BREAK SHIKIGAMI HEADER
Gui.exe起動前、または実行中の式神データ破損状態を作成するために使用する。
`BREAK` は試験用の事前条件または異常状態を作成するためのコマンドとして扱う。
将来的に破損方法が追加された場合は、以下のように対象を追加できる形式とする。
   BREAK SHIKIGAMI HEADER
   BREAK SHIKIGAMI ROW
Version 1では `HEADER` のみを実装対象とする。

#### REMOVE SHIKIGAMI
`ShikigamiData.csv` から指定した式神のデータを削除する。
式神データ復旧機能の試験などで、
意図的に式神データが欠損した状態を作成するために使用する。

形式：
   REMOVE SHIKIGAMI "<式神名>"
例：
   REMOVE SHIKIGAMI "願紡縁結神"
指定した式神名に一致するデータを `ShikigamiData.csv` から削除する。
本コマンドはGui.exe上の操作を再現するものではなく、
Scenario Runnerが試験用の状態を作成するためにテスト対象データを直接変更するコマンドである。
指定した式神が `ShikigamiData.csv` に存在しない場合は、
コマンドの実行失敗として扱う。

### 確認
試験結果の確認には原則として `CHECK` コマンドを使用する。
形式：
   CHECK TARGET
確認内容ごとに巨大な専用キーワードを作成せず、
`CHECK` と確認対象の組み合わせで表現する。

#### CHECK CALC
計算が正常に完了し、計算結果がGui.exe上に表示されていることを確認する。
形式：
   CHECK CALC

#### CHECK SHIKIGAMI
式神データが正常な状態であることを確認する。
形式：
   CHECK SHIKIGAMI
式神データ破損・復旧Scenarioなどで使用する。

#### CHECK DIALOG
指定したメッセージを持つダイアログが表示されていることを確認する。
形式：
   CHECK DIALOG "<メッセージ>"
例：
   CHECK DIALOG "御魂が1つも装備されていません。"
Validationエラーなど、
Gui.exeが期待したダイアログを表示すること自体を試験結果として確認する場合に使用する。
`CHECK DIALOG` はダイアログを閉じる操作を行わない。
ダイアログを閉じる場合は `CLOSE DIALOG` を使用する。

## 10. 実行順序
Scenario Runnerは `START` から `END` までのコマンドを上から順番に実行する。
例：
   START
   OPEN GUI
   SEL SHIKIGAMI "願紡縁結神"
   CALC
   CHECK CALC
   CLOSE GUI
   END
各コマンドは、原則として直前のコマンドが正常に完了した後に実行する。

## 11. 実行失敗
コマンドの実行に失敗した場合、その時点でScenarioを失敗とする。
`CHECK` の条件を満たさなかった場合もScenarioを失敗とする。
失敗時には少なくとも以下の情報を実行ログへ出力する。
- Scenarioファイル名
- 行番号
- 実行コマンド
- エラー内容
例：
   [05] OPEN GUI                         PASS
   [08] SEL SHIKIGAMI "願紡縁結神"      PASS
   [10] CALC                             PASS
   [11] CHECK CALC                       FAIL
   Scenario failed at line 11.
   Calculation result was not displayed.

## 12. 行番号
Scenario Runnerは元のScenarioファイル上の行番号を保持する。
コメント行および空行も行番号の計算対象とする。
これにより、実行ログに表示された行番号からScenarioファイル上の該当箇所を直接確認できる。

## 13. 構文チェック
Scenario RunnerはScenarioの実行開始前にScenarioファイル全体を解析し、構文チェックを行う。
構文エラーが存在する場合、Scenarioを実行してはならない。
少なくとも以下を構文エラーとして検出する。
- `START` が存在しない。
- `END` が存在しない。
- `START` が複数存在する。
- `END` が複数存在する。
- `START` より前に実行コマンドが存在する。
- `END` より後に実行コマンドが存在する。
- `END` が `START` より前に存在する。
- 未定義のコマンドが存在する。
- 必須引数が不足している。
- 不要な引数が指定されている。
- ダブルクォーテーションが閉じられていない。
- 定義されていないTARGETが指定されている。
構文エラーにはScenarioファイル上の行番号を付与する。
例：
   Syntax Error
   BasicCalculation.scenario:8
   Unknown command: CALCULATE

## 14. ScenarioとRunner設定の分離
Scenarioファイルには試験手順そのものだけを記述する。
人間が実行状況を確認するための待機時間や、デバッグ目的のRunner動作はScenarioの試験内容には含めない。
そのため、Version 1では以下のようなコマンドをScenario Formatには定義しない。
   WAIT 500
   WATCH
   KEEP_OPEN_ON_FAILURE
これらに相当する機能はScenario Runnerアプリ側の実行設定として提供する。
例：
   Execution Speed
       Fast
       Watch
   Keep Gui.exe open on failure
       On / Off
これにより、Scenarioファイルに記述された試験内容と、Scenarioをどのように実行・観察するかを分離する。

## 15. 実行ログ
Scenario RunnerはScenario実行中の状態をアプリ上のログ表示領域へ出力する。
各実行コマンドについて、Scenarioファイル上の行番号、コマンド、実行結果を表示する。
例：
   Scenario: BasicCalculation.scenario
   ========== START ==========
   [05] OPEN GUI                         PASS
   [08] SEL SHIKIGAMI "願紡縁結神"      PASS
   [11] LOAD MITAMA "Valid.mitama.json" PASS
   [13] CALC                             PASS
   [14] CHECK CALC                       PASS
   [16] CLOSE GUI                        PASS
   =========== END ===========
   Scenario Result : PASS
   Passed          : 6
   Failed          : 0
   Elapsed         : 4.21 sec
実行失敗時は失敗したコマンドおよび理由を表示する。

## 16. Version 1 実装対象一覧
Version 1では以下の予約語およびコマンドを実装対象とする。
   START
   END
   LAUNCH GUI
   OPEN GUI
   CLOSE GUI
   CLOSE DIALOG
   SEL SHIKIGAMI "<式神名>"
   EQUIP MITAMA MAIN <御魂スロット番号> "<ステータス種類>"
   EQUIP MITAMA SUB <御魂スロット番号> <サブスロット番号> ["<ステータス種類>"] ["<値>"]
   EQUIP MITAMA SET <番号> "<ステータス種類>"
   EQUIP MITAMA UNIQUE <番号> "<ステータス種類>"
   LOAD MITAMA "<ファイルパス>"
   CALC
   CLEAR
   RELOAD SHIKIGAMI
   BREAK SHIKIGAMI HEADER
   REMOVE SHIKIGAMI "<式神名>"
   CREATE SHIKIGAMI BACKUP
   RECOVER SHIKIGAMI BROKEN
   RECOVER SHIKIGAMI BACKUP
   WAIT SHIKIGAMI AUTO REPAIR
   CHECK CALC
   CHECK SHIKIGAMI
   CHECK DIALOG "<メッセージ>"
   
新しいコマンドは、実際のScenarioを作成する上で必要になった時点で追加する。
Version 1の段階では、条件分岐、ループ、変数、関数、ジャンプなどの汎用プログラミング機能は実装しない。

## 17. 記述例
### 17.1 基本計算試験
   # 基本計算試験
   # 式神を選択し、御魂セットを読み込んだ状態で
   # 正常に計算できることを確認する。
   START
   OPEN GUI
   # 対象式神を選択
   SEL SHIKIGAMI "願紡縁結神"
   # 正常な御魂セットを読み込む
   LOAD MITAMA "TestData/Valid.mitama.json"
   # 計算を実行
   CALC
   # 計算結果が正常に表示されること
   CHECK CALC
   CLOSE GUI
   END

### 17.2 式神データ自動修復試験
  # 式神データ自動修復試験
  # ShikigamiData.csvのヘッダーが破損している状態から
  # 正常な式神データへ自動修復できることを確認する。

  START

  # Gui.exe起動前に式神データを破損させる
  BREAK SHIKIGAMI HEADER

  LAUNCH GUI

  # 破損検出時のダイアログを確認
  CHECK DIALOG "式神データファイルの形式が正しくありません。"
  CLOSE DIALOG

  # Brokenへの退避とShikigamiData.csvの再生成完了を待つ
  WAIT SHIKIGAMI AUTO REPAIR

  # 自動修復後の式神データが正常であること
  RELOAD SHIKIGAMI
  CHECK SHIKIGAMI

  CLOSE GUI

  END

### 17.3 御魂入力による計算試験
  # 御魂をGui.exe上で入力し、
  # 正常に計算できることを確認する。
  START
  OPEN GUI

  # 対象式神を選択
  SEL SHIKIGAMI "願紡縁結神"

  # 1番御魂を入力
  EQUIP MITAMA MAIN 1 "攻撃力"
  EQUIP MITAMA SUB 1 1 "追加攻撃力" "5"
  EQUIP MITAMA SUB 1 2 "追加HP" "3"
  EQUIP MITAMA SUB 1 3 "会心率" "3"

  # 計算を実行
  CALC

  # 計算結果が正常に表示されること
  CHECK CALC

  CLOSE GUI
  END

## 18. 改訂履歴
| Version | Date | 内容 |
|---|---|---|
| 1.0 | 2026-08-17 | 初版作成。Scenarioファイルの基本構造、コメント、キーワード、Version 1実装コマンド、構文チェック、実行ログ等の仕様を定義。 |
| 1.1 | 2026-08-22 | Scenario内で指定する外部テストデータのパス解決規則を追加。相対パスはScenarioファイルの配置ディレクトリを基準とし、絶対パスも許可する仕様を定義。`CHECK DIALOG` コマンドを追加。 |
| 1.2 | 2026-08-22 | 式神データ復旧試験用に、`ShikigamiData.csv` から指定した式神データを削除する `REMOVE SHIKIGAMI` コマンドを追加。 |
| 1.3 | 2026-08-23 | コマンド定義をカテゴリ別に再構成し、`CHECK` コマンドをVersion 1コマンドへ統合。モーダルダイアログを閉じる `CLOSE DIALOG` コマンドを追加。 |
| 1.4 | 2026-08-23 | 式神データ復旧試験用に、Gui.exeの通常の式神データ更新処理を経由してBackupデータを生成する `CREATE SHIKIGAMI BACKUP` コマンド、およびScenario内で生成されたBrokenまたはBackupデータから式神データを復旧する `RECOVER SHIKIGAMI` コマンドを追加。 |
| 1.5 | 2026-08-28 | 式神データ自動修復処理との同期用に、Brokenデータへの退避およびShikigamiData.csvの再生成が完了するまで待機する `WAIT SHIKIGAMI AUTO REPAIR` コマンドを追加。 |
| 1.6 | 2026-08-29 | Gui.exeのメインウィンドウが操作可能になるまで待機せず、プロセス起動のみを行う `LAUNCH GUI` コマンドを追加。起動途中に表示されるモーダルダイアログを確認する試験で使用する。 |
| 1.7 | 2026-08-30 | Gui.exe上で御魂のメインステータス、サブステータス、2セット効果、固有効果を入力する `EQUIP MITAMA` コマンドを追加。サブステータスではGui.exe側のValidation試験を可能とするため、ステータス種類と値をそれぞれ独立して省略可能とした。 |
