# Scenario Runner Performance Optimization

## 1. 概要

本資料は、ShikigamiStatusCalculator の Scenario Runner に対して実施した性能改善について、最終的な設計、主要な改善内容、性能評価、および検討したが採用しなかった方式を整理する。

対象となる主な変更範囲は以下とする。

- 対象コミット：`e586b7c099ee0ef7e4ea539a2d6b0c3baa197fb6` ～ `98f0492d7f4a40ae02e3c901d64644b8ef416fbe`
- 実施期間：2026-09-22 ～ 2026-09-29

この期間では、単純に待機時間を短くするのではなく、Scenario Runner が行っていた不要なファイル I/O、UI Automation 探索、同一要素の再探索、広すぎる Window 探索、同期的な UI 操作による待機などを段階的に削減した。

一方、UI 操作完了を保証するために必要な待機や Fallback は、性能上のコストが存在しても意図的に維持している。

最終的な目的は、以下の両立である。

- GitHub Actions 上での Scenario 実行時間短縮
- Runner 個体差や UI 応答速度の変動に耐えられる安定性の維持

---

## 2. 性能改善の基本方針

### 2.1 不要な処理そのものを減らす

今回の性能改善では、単純に timeout や polling interval を短くするのではなく、繰り返し実行されていた不要な処理そのものを減らすことを基本方針とした。

対象となった処理には、例えば以下がある。

- ログ出力ごとのファイル open / close
- Desktop 全体を対象とした Window 探索
- 同一 Window に対する UI Automation tree の再探索
- 取得済み AutomationElement の再取得
- 同じ状態に対する重複した検証
- 同期的な UI 操作によって呼び出し側が待機していた時間

つまり、

> 「処理を無理に急がせる」のではなく、「毎回行っていた不要な仕事をなくす」

ことを基本方針とした。

### 2.2 待機時間そのものを安易に削らない

Scenario Runner は GUI E2E テストであるため、固定待機や polling interval だけを短縮すると race condition を発生させやすい。

そのため、UI Automation 周辺では以下を優先した。

1. 不要な UI Automation 探索を行わない。
2. 一度取得した Window / AutomationElement を再利用する。
3. 探索範囲を Desktop 全体から Process、Owner、Direct child へ狭める。
4. UI 操作と後続の待機を安全に重ねられる場合のみ non-blocking な操作を使用する。
5. 必要な同期処理、Fallback、完了確認は削除しない。

### 2.3 Fast path と Fallback を分離する

通常ケースを高速化するために Fallback 自体を削除すると、GitHub Actions Runner の個体差や UI Automation tree の構築タイミングによって不安定になる。

そのため、

```text
Fast path
    ↓ 見つからない
Fallback
```

という構造を基本とする。

正常系では狭い探索範囲から高速に取得し、通常経路で取得できない場合のみ従来相当の広い探索を行う。

### 2.4 変更は実測して判断する

コード上で高速に見える変更であっても、実際の GitHub Actions 上で効果が確認できるとは限らない。

そのため、変更後は原則として Scenario Runner Batch を実行し、

- Scenario が正常に PASS すること
- 対象 Command の実行時間
- 変更と無関係な Command の実行時間
- Runner 個体差
- 必要に応じて複数回実行した結果

を確認した上で採否を判断した。

ただし、`e586b7c` のログ出力改善のように、明確に不要な処理を削減しているものの、変更単体での性能測定を実施していない改善も存在する。

この場合は具体的な性能向上量を断定せず、設計上削減された処理と期待できる効果を区別して記録する。

---

## 3. 性能測定方針

### 3.1 GitHub Actions Runner の個体差

GitHub Actions の Windows Runner では、同一コミットでも UI Automation の処理時間に大きな差が発生した。

そのため、単一 Run の Batch 全体時間だけを根拠として変更の採否を決定しない。

評価時には主に以下を使用した。

- 同一コミットの複数回実行
- 変更対象 Command 自身の所要時間
- 変更と無関係な Command を control group とした相対比較
- 30 Scenario 全件 PASS の確認
- 必要に応じたログ、キャプチャ、録画による race condition の確認

後半の微細な最適化では、対象 Command の変化を control group の変化量で正規化する方法も使用した。

### 3.2 複数回実行による確認

Runner 個体差の影響が疑われる場合は、同一コミットを 5 ～ 6 回程度実行して傾向を確認した。

これにより、

- 本当に変更による改善なのか
- 単に速い Runner を引いただけなのか
- race condition が低確率で残っていないか

を確認した。

### 3.3 常設の詳細計測を行わない

Scenario Runner 本体へ詳細な性能計測コードを常設すると、計測処理そのものが UI Automation のタイミングへ影響する可能性がある。

実際、一時的に詳細ログを追加した際には実行時間や安定性への悪影響が確認された。

そのため、性能評価は原則として既存ログの Step 開始・終了時刻と GitHub Actions の実行結果を利用する。

調査用ログを追加した場合も、原因特定後は削除する。

---

## 4. ログ出力の効率化

高速化対応の開始時点では、Scenario Runner のログ出力に `File.AppendAllText()` を使用していた。

この実装では、ログを 1 行出力するたびにファイルへの追記処理が発生していた。

`e586b7c` では `LogFileWriter` が `StreamWriter` を保持する方式へ変更し、Scenario 実行中は同一 Writer を再利用するようにした。

変更前は概ね以下の処理となっていた。

```csharp
File.AppendAllText(
    mLogFilePath,
    message + Environment.NewLine,
    Encoding.UTF8);
```

変更後は `StreamWriter` を保持し、

```csharp
mWriter.WriteLine(message);
```

によってログを出力する。

これにより、ログ出力ごとに発生していたファイル open / close を削減した。

`AutoFlush = true` とすることでログの即時反映は維持している。

### 4.1 Writer の lifetime 管理

`StreamWriter` を保持する方式へ変更したため、Writer の lifetime も明示的に管理する必要がある。

そのため、

- `LogFileWriter`
- `ScenarioLogger`

へ `IDisposable` を導入した。

新しい Scenario の Validation を開始するときには以前の `LogFileWriter` を破棄し、Runner 終了時にも `ScenarioLogger.Dispose()` を通して Writer を破棄する。

CLI 側でも `ScenarioLogger` を `using` で管理するよう変更した。

### 4.2 性能効果

この変更単体での性能測定は実施していないため、具体的な短縮時間は算出していない。

ただし、各ログ出力で発生していたファイル open / close を除去しているため、従来実装よりファイル I/O のオーバーヘッドが小さくなる設計となっている。

この変更は、今回の高速化全体で採用した、

> 繰り返し発生する不要な処理を削減し、再利用可能なものは保持する

という方針の最初の適用例でもある。

後続の UI Automation 高速化でも、同じ考え方を Window、AutomationElement、UI tree の再利用へ適用している。

---

## 5. UI Automation で確認された主な問題

### 5.1 Desktop 起点の広範囲な UI Automation 探索

Window や Dialog を取得するために Desktop 全体を対象とした探索が頻繁に行われていた。

`FindAllDescendants()` のような探索は、Windows の UI Automation tree 全体を走査する場合があり、GitHub Actions Runner 上では非常に高コストとなる。

### 5.2 同じ UI tree の再探索

Window を検出した後、Button、ComboBox、TextBox などを取得するために同じ Window 配下を繰り返し探索していた。

特に FileDialog では、候補判定時に取得した descendants を後続処理で再び取得するケースが存在した。

### 5.3 MainWindow の再探索

MainWindow は Scenario Runner のほぼすべての処理で利用されるにもかかわらず、必要になるたびに UI Automation tree から探索する箇所が存在した。

MainWindow は Gui.exe の起動中ほぼ不変であるため、GuiSession 単位で保持できる。

### 5.4 FileDialog 探索

FileDialog は今回の性能改善で最も大きなボトルネックの一つだった。

調査の結果、Save / Load 系 Scenario では FileDialog の検出と内部要素探索が実行時間の大部分を占めるケースが確認された。

そのため、今回の高速化では FileDialog の探索方法を重点的に見直した。

---

## 6. 最終的な Window 探索設計

### 6.1 ProcessWindowWaiter の探索範囲を段階化する

`ProcessWindowWaiter` による一般的な Process Window 探索では、最初から Desktop 全体を検索しない。

Owner が既知の場合、概念的には以下の順序で探索する。

```text
Native owned window
        ↓ not found
Owner direct child
        ↓ not found
Owner descendant
        ↓ not found
Process top-level window
        ↓ not found
Process descendant
```

現在の `ProcessWindowWaiter` では、Owner 指定時に以下の順序となる。

1. Win32 の owner relationship を利用した Native owned window 探索
2. Owner 配下の UI Automation 探索
3. Process 全体を対象とした Fallback

これにより通常ケースでは探索範囲を大幅に縮小しつつ、特殊な Window 構造にも対応できる。

FileDialog は `FileDialogWaiter` が専用の探索処理を担当しており、`ProcessWindowWaiter` と完全に同一の探索順序を使用するわけではない。

### 6.2 Native owner fast path

WinForms の modal window は Win32 上では owner relationship を持つ。

そこで、

- `EnumWindows()`
- `GetWindowThreadProcessId()`
- `GetWindow(..., GW_OWNER)`

を利用し、

- 対象 Process
- 指定 Owner を持つ Window

だけを先に列挙する。

UI Automation tree 全体を探索する前に候補を絞り込めるため、通常ケースでは高速である。

### 6.3 UIA readiness の確認

Native handle が取得できたことと、その Window の AutomationElement が完全に利用可能であることは同一ではない。

Window 生成直後には、

- HWND は存在する
- `Automation.FromHandle()` も成功する
- しかし descendants の取得にはまだ失敗する

という状態が存在する。

そのため Native owned window を取得した場合でも、UI Automation tree が利用可能な状態であることを確認する。

また、

- `PropertyNotSupportedException`
- `ElementNotAvailableException`
- `COMException`

などの stale / unavailable 状態も考慮する。

### 6.4 Fallback は削除しない

Native owner fast path が存在するからといって、UI Automation による Fallback は削除しない。

開発中には、

- Owner から取得できない Window
- UIA tree 上で予想と異なる位置に存在する Window
- MessageBox
- FileDialog
- 起動直後の Dialog
- stale AutomationElement

などが実際に確認された。

したがって Fallback は性能上の無駄ではなく、

> Fast path が成立しない環境やタイミングに対する安全装置

として扱う。

---

## 7. Waiter の責務分離

高速化前は Window 関連処理が一つの Waiter に集中していた。

改善後は主に以下へ責務を分離した。

- `WindowWaiter`
- `ProcessWindowWaiter`
- `FileDialogWaiter`

### 7.1 WindowWaiter

既知の Window の close など、一般的な Window 状態待機を担当する。

### 7.2 ProcessWindowWaiter

Gui.exe の Process に属する Window の検索と待機を担当する。

Owner が利用できる場合は owner-scope を優先し、必要な場合のみ Process 全体へ Fallback する。

### 7.3 FileDialogWaiter

Windows FileDialog 固有の探索を担当する。

FileDialog は通常の WinForms Form と UI Automation tree の構造が異なるため、専用処理として分離した。

この分離により、

> 一般 Window の都合で FileDialog 探索を複雑化する

またはその逆の状態を避けている。

---

## 8. FileDialog の最適化

### 8.1 Owner-scope の利用

FileDialog を開いた Dialog が既知の場合、その Window を owner として探索する。

Desktop 全体の探索は Fallback とする。

これにより、無関係な Window を候補として調査する回数を削減した。

### 8.2 Native owner fast path

FileDialog についても Native owner relationship を利用する。

Native owner candidate が見つかった直後は UI Automation tree が完全に構築されていない場合がある。

そのため Native candidate が存在する場合は短い grace period を設け、native-owner path を優先して再試行する。

現在は最大 500 ms の範囲で native candidate の readiness を待ち、その後 Fallback へ移行する。

これは単なる固定待機ではなく、

> 高速な探索経路が UIA tree の初期化より少しだけ先行した場合に、即座に高コストな Fallback へ移行しない

ための処理である。

### 8.3 Direct path と Fallback の分離

FileDialog 探索では、通常経路と Fallback を明確に分離した。

通常ケースでは Native owner や Owner scope から取得し、見つからない場合のみ Process で絞り込んだ Desktop 側の探索へ進む。

これにより正常系で不要な Desktop 探索を実行しない。

### 8.4 候補調査結果の再利用

FileDialog 候補を調査するときに取得した以下の情報を `FileDialogElements` として保持する。

- Dialog
- descendants
- File name ComboBox candidates
- File name Edit controls

後続の `FileDialogOperator` はこれらを再利用する。

これにより、

```text
Dialog を探す
    ↓
descendants を取得
    ↓
FileDialog と判定
    ↓
もう一度 descendants を取得
    ↓
入力欄や Button を探す
```

という重複探索を避けた。

FileDialog 周辺の探索方法を見直した一連の変更では、約 93 秒の Batch 短縮を確認した Run も存在する。

この Run では FileDialog candidate search の最適化によって、Direct path で既に調査した Window を Fallback candidate から除外し、候補調査を早期終了できる場合は探索を打ち切るようにした。

ただし GitHub Actions Runner の個体差が存在するため、この約 93 秒を FileDialog 最適化全体の固定的な短縮量として扱わない。

FileDialog 周辺の探索削減が今回の高速化で特に大きな効果を持ったことを示す実測例として扱う。

### 8.5 Win32 と UI Automation の併用

FileDialog 操作をすべて FlaUI / UI Automation に統一するのではなく、操作内容に応じて Win32 API と UI Automation を使い分ける。

FileDialog の入力・状態更新・確定操作では、主に以下の Win32 message を使用する。

- `WM_SETTEXT`
- `WM_GETTEXT`
- `WM_GETTEXTLENGTH`
- `WM_COMMAND`
- `EN_CHANGE`
- `BM_CLICK`

特に Save Dialog では、テキストだけを変更しても Dialog 側が変更を認識しないケースがあった。

そのため `EN_CHANGE` を送出し、FileDialog 内部の状態更新を発生させる。

また、Load 側の Open 操作では同期的な `BM_CLICK` を利用する。

一方、Save 側の Save 操作など UI Automation の方が適切な箇所では FlaUI の Invoke を維持する。

すべてを Win32 化すれば高速になるわけではなく、操作対象と必要な同期特性に応じて使い分ける。

---

## 9. AutomationElement の再利用

### 9.1 AutomationElementMap

同一 Window 内で複数の AutomationElement を利用する場合、要素ごとに `FindFirstDescendant()` を実行するのではなく、一度取得した要素群から `AutomationId` ベースの Map を構築する。

`AutomationElementMap` は概念的に以下の対応を保持する。

```text
AutomationId
    ↓
AutomationElement[]
```

同一 Window に対する UI Automation tree の再探索を減らすことが目的である。

### 9.2 MainForm Map を GuiSession 単位で共有

MainForm はほぼすべての Scenario Command から利用される。

そのため `GuiSession` に以下を保持する。

```text
MainWindow
MainElementMap
```

これにより各 Feature Operator が MainForm を個別に探索する必要がなくなった。

`GuiSession` の終了時に cache も破棄するため、Gui.exe の再起動を跨いで古い AutomationElement を使用しない。

### 9.3 MainForm の Direct child Map

MainForm の Scenario Runner 操作対象を Designer と照合した結果、対象となる Control がすべて MainForm の direct child であることを確認した。

そのため MainForm の Map 構築を、

```csharp
new AutomationElementMap(mainWindow)
```

による全 descendants 取得から、

```csharp
new AutomationElementMap(mainWindow.FindAllChildren())
```

へ変更した。

この変更では Run #148 → #149 で、

| Command | Before | After | 変化 |
| --- | ---: | ---: | ---: |
| `EQUIP MITAMA MAIN 1` | 1.8083 sec | 0.9909 sec | -45.2% |
| `CHECK CLEARED` | 1.6649 sec | 0.9510 sec | -42.9% |

を確認した。

Control group を考慮しても大幅な改善が残っており、MainForm の `FindAllDescendants()` が大きな初期 Map 構築コストになっていたことが確認できた。

### 9.4 Children / Descendants の使い分け

`FindAllChildren()` を無条件に使用するわけではない。

以下の条件を満たす場合のみ direct child に限定する。

- 対象 UI の Designer 構造を確認できる。
- Scenario Runner が必要とする Control がすべて direct child に存在する。
- UI 階層への依存を許容できる。

現在、この条件を満たす MainForm および一部 Dialog では `FindAllChildren()` を利用する。

一方、`ResultViewForm` のように GroupBox 配下の Control を取得する必要がある画面では descendants 探索を維持する。

小規模 Dialog については `FindAllChildren()` 化による明確な性能差は確認できなかった。

ただし、

> 必要な範囲だけ探索する

という設計方針の統一、およびコード複雑度が増加しないことから変更は維持している。

---

## 10. Child-first lookup

MainForm の単一 Element を Map 構築前に取得する場合も、最初から descendants を検索しない。

現在の `GuiOperator` は、

1. `FindFirstChild()`
2. 見つからなければ `FindFirstDescendant()`

の順で探索する。

Fallback を残しているため、将来 UI 階層が変化した場合でも従来の探索能力を維持できる。

この変更後、Runner 全体が遅い個体であったにもかかわらず `SEL SHIKIGAMI` は、

- 0.946 sec
- 0.836 sec

へ約 11.6% 短縮したケースが確認された。

---

## 11. Click と PostClick

### 11.1 Click

通常の `Click()` は FlaUI の Invoke を使用する。

Invoke は対象イベント処理と同期する性質があるため、操作完了との同期が必要な箇所では安全性が高い。

### 11.2 PostClick

`PostClick()` は native handle に対して `PostMessage(BM_CLICK)` を送信する。

呼び出し側はイベント処理完了を待たずに後続処理へ進める。

以下のように、

```text
PostClick
    ↓
WaitForWindow
```

または、

```text
PostClick
    ↓
WaitForWindowClosed
```

という明確な同期処理が直後に存在し、Button event と後続の待機を実際に重ねられる場合に有効となる。

SaveData、Snapshot、Clear、各種 modal dialog 起動などで利用している。

### 11.3 PostClick は無条件な高速化ではない

`Click()` の直後に Wait があるからといって、必ず `PostClick()` が有効とは限らない。

Button event 自体が、

```text
Validation
    ↓
File update
    ↓
State update
    ↓
Form.Close
```

のような処理を行う場合、PostClick にしても後続の `WaitForWindowClosed()` が同じ処理時間を待つだけである。

式神登録処理では PostClick 化を実験し、30/30 Scenario は PASS したものの、明確な性能改善は確認できなかった。

Invoke 内で待機していた時間が `WaitForWindowClosed()` 側へ移動しただけで、critical path 自体は短縮されなかったため、この変更は最終的に採用しなかった。

このように PostClick の採用可否は、

> 後続処理に、Button event と並行して進められる独立した待機が存在するか

で判断する。

---

## 12. 完了待機は意図的に残す

高速化の途中では、操作自体を高速化したことで、それまで偶然隠れていた race condition も発生した。

そのため以下の待機は意図的に残している。

### 12.1 Load 完了待機

SaveData Load Dialog が閉じた時点では、MainForm 側へのデータ反映が完了しているとは限らない。

そのため Load Dialog close 後に MainForm の Load Button 状態を監視する。

単に `Enabled == true` を一度確認するだけでは、disable 状態を観測する前に確認してしまう可能性がある。

現在は、

- disabled を観測した場合は、その後 enabled へ戻るまで待つ。
- disabled を観測できなかった場合は、enabled が連続して安定していることを確認する。

という処理を行う。

### 12.2 Clear 完了待機

Clear 確認 MessageBox を閉じた直後に次の Command を開始すると、MainForm の Clear 処理と競合する可能性がある。

そのため `WaitWhileBusy()` を維持する。

### 12.3 Shikigami update 完了待機

ShikigamiRegisterForm が閉じた後も、MainForm 側では式神データ再読み込みや ComboBox 更新が行われる。

そのため Form close のみを完了条件とせず、MainForm 側の処理完了も待つ。

### 12.4 Snapshot Compare Button

FileDialog が閉じたことと、外側の WinForms Dialog がファイルパスを反映して Compare Button を有効化したことは同一ではない。

そのため `waitForEnabled()` を維持する。

これらは削除可能な「余分な待機」ではなく、非同期化や高速化によって露呈した race condition に対する同期ポイントである。

---

## 13. Dialog 検出の最適化

`CHECK SHIKIGAMI` は以下の二つを確認する。

1. エラー等の modal dialog が表示されていない。
2. 式神 ComboBox に利用可能なデータが存在する。

以前の `DialogOperator.Exists()` は、Dialog が存在しない正常ケースでも `ProcessWindowWaiter` の複数の Fallback 探索を最後まで実行していた。

しかし `Exists()` に必要なのは、

> 現在 Gui.exe に visible な modal dialog が存在するか

という限定された判定である。

そこで `Exists()` のみ専用 Fast path を設けた。

まず Win32 の `EnumWindows()` を利用し、

- ProcessId
- Visible
- MainWindow を除外

という条件で候補 Window を絞り込む。

その後、候補を AutomationElement として取得し、UI Automation で Button / Text の存在を確認する。

つまり、

```text
Win32 で候補 Window を安く限定
        ↓
UI Automation で Dialog として必要な構造を確認
```

という hybrid な探索となっている。

共有の `ProcessWindowWaiter` や `GetActiveDialog()` の挙動は変更していない。

この変更では `CHECK SHIKIGAMI` が、

- 6.137 sec / call
- 1.131 sec / call

へ短縮し、約 81.6% の改善を確認した。

これは「汎用 Waiter を無理に高速化する」のではなく、

> 必要な意味が限定されている呼び出し側だけ専用 Fast path を持つ

という方針の代表例である。

---

## 14. Feature 単位の主な改善

### 14.1 Snapshot Comparison

Snapshot Comparison では以下を実施した。

- Window 参照の再利用
- AutomationElement の再利用
- DataGridView row の cache
- FileDialog 探索の owner-scope 化
- modal 起動の PostClick 化
- Compare Button の enable 待機追加

DataGridView の各行を `CHECK` ごとに再取得するのではなく、比較結果を一度取得して再利用する。

これにより、同一 Snapshot Comparison Result に対する複数の検証で DataGridView 全体を繰り返し探索しない。

### 14.2 SaveData

SaveData では以下を実施した。

- Dialog 内 AutomationElement の一括取得
- FileDialogElements の再利用
- owner-scope FileDialog 探索
- Load / Save Button の PostClick 化
- SaveData type 選択後の冗長な UIA GetValue 検証削除
- Load FileDialog 完了後の冗長な file path UIA polling 削除
- Load 完了同期の追加

ComboBox の選択は Win32 の、

- `CB_FINDSTRINGEXACT`
- `CB_SETCURSEL`
- `WM_COMMAND`
- `CBN_SELCHANGE`

を利用する。

これらは同期的に処理されるため、その直後に UI Automation で同じ選択値を再確認する処理は削除した。

ただし Save 側の file path wait は Load と同一条件ではないため、単純に削除していない。

### 14.3 Calculation

`CHECK CALC` では、式神が選択されているかを判定するために ComboBox の表示文字列を UI Automation 経由で取得していた。

必要なのは文字列そのものではなく、

> selected item が存在するか

である。

そのため Win32 の `CB_GETCURSEL` を利用する `HasSelectedItem()` を追加した。

検証目的に必要な最小情報だけを取得する方針としている。

### 14.4 Dialog

Dialog 処理では以下を実施した。

- Dialog / Button の探索結果再利用
- Process scope の限定
- Owner scope の利用
- `CHECK SHIKIGAMI` 用 `Exists()` の専用 Fast path
- Dialog close 後の明示的な Window close 待機

Dialog の検出結果や Button 探索結果を後続処理で再利用し、不要な再探索を削減した。

---

## 15. 採用しなかった、または効果が限定的だった方式

性能改善では、成功した変更だけでなく複数の方式を試験し、効果や安定性が不足したものは採用しなかった。

Git 履歴から削除された実験も含め、再検討を防ぐためここに記録する。

### 15.1 CacheRequest による CHECK CLEARED 一括取得

`CHECK CLEARED` では多数の ComboBox / TextBox を確認するため、UI Automation の CacheRequest による一括取得を試験した。

しかし既存の `AutomationElementMap` がすでに tree traversal の結果を保持している状態で、CacheRequest のために新しい `FindAllDescendants()` を実行したことで逆に遅くなった。

試験 Run では、

- 従来：約 1.5 sec
- CacheRequest：約 4.91 sec

となり、大幅に悪化した。

この方式は採用しなかった。

#### 教訓

> Cache API 自体が高速でも、そのために新しい tree traversal を追加すれば全体として遅くなる。

---

### 15.2 TextBox.GetText の WM_GETTEXT 化

TextBox の値取得を UI Automation から Win32 の、

- `WM_GETTEXTLENGTH`
- `WM_GETTEXT`

へ変更する実験を行った。

30/30 Scenario は安定して PASS したものの、6 Run の `CHECK CLEARED` 平均では raw な差が約 1% に留まった。

さらに control group で Runner 性能差を正規化すると、従来方式より遅い傾向となった。

原因として、

- NativeWindowHandle 自体を UI Automation から取得する。
- その後さらに複数の `SendMessage()` を行う。

ため、UI Automation を完全に回避できていなかったことが考えられる。

この方式は採用しなかった。

#### 教訓

> Win32 API に置き換えること自体を高速化と考えない。UIA → HWND 取得 → Win32 call まで含めた総コストで判断する。

---

### 15.3 式神登録 Button の PostClick 化

ShikigamiRegisterForm の登録 Button を `Click()` から `PostClick()` へ変更する実験を行った。

30/30 Scenario は PASS したが、明確な性能改善は確認できなかった。

登録処理では Button event 内で、

- Validation
- Backup
- CSV update
- Form close

などが行われる。

そのため Invoke の待機時間を `WaitForWindowClosed()` 側へ移動させただけで、critical path 自体は短縮されなかった。

この方式は最終的に採用せず、登録 Button は `Click()` を使用する実装へ戻した。

この実験から、

> `Click()` の直後に Wait があることだけを理由に PostClick 化してはいけない

ことが確認できた。

---

### 15.4 詳細性能ログ

FileDialog や Button Invoke の詳細時間を Runner 内部から記録する方式を一時的に導入した。

原因調査には有効だったが、ログ追加後に性能低下や失敗増加が確認された。

必要な調査終了後に詳細ログは削除した。

#### 教訓

> GUI E2E の性能計測では、観測処理そのものがタイミングへ影響する。

---

### 15.5 小規模 Dialog の FindAllChildren 化

MainForm で大きな効果があったため、

- SaveDataLoadDialog
- SaveDataSaveDialog
- SnapshotCompareFileSelectDialog

についても direct child のみから `AutomationElementMap` を構築するよう変更した。

対象 Control がすべて direct child であることは Designer 上で確認した。

30/30 Scenario は PASS した。

ただし MainForm と異なり元々 Control 数が少ないため、Runner 個体差を正規化すると明確な性能改善は確認できなかった。

性能上の効果は限定的だが、

> 必要な範囲だけ探索する

という設計方針の統一、およびコード複雑度が増加しないことから変更は維持している。

---

## 16. 主要な性能改善ポイント

今回の改善では、単一の「高速化機能」によって性能が改善したわけではない。

主要な改善点は以下である。

| 改善 | 主な効果 |
| --- | --- |
| Log Writer の再利用 | ログ出力ごとの file open / close 削減 |
| FileDialog 探索範囲縮小 | Desktop / descendant 全探索削減 |
| FileDialog candidate 情報再利用 | 同一 tree の再探索削減 |
| Owner-scope | 無関係 Window の探索削減 |
| Native owner fast path | UIA tree 探索前に候補を限定 |
| MainWindow cache | MainWindow 再探索削減 |
| MainElementMap | MainForm Control 再探索削減 |
| Direct child Map | MainForm 全 descendants 走査削減 |
| DataGridView row cache | CHECK ごとの Grid 全走査削減 |
| Dialog Exists 専用 Fast path | no-dialog 正常系の高コスト Fallback 削減 |
| PostClick | 適用可能な modal operation と Window wait の重畳 |
| 不要な再検証削除 | UIA property read / polling 削減 |

一連の改善によって Batch 全体の実行時間は大幅に短縮した。

ただし GitHub Actions Runner の個体差が大きいため、単一 Run の Before / After を Scenario Runner 全体の固定的な性能倍率として扱わない。

重要なのは、主要な改善について対象 Command 単位でも短縮が確認され、30/30 Scenario PASS を維持したことである。

なお、ログ出力改善のように変更単体で性能測定を実施していないものについては、具体的な短縮量を性能結果として扱わない。

---

## 17. 性能改善によって得られた設計原則

今回の作業を通して、Scenario Runner では以下を基本原則とする。

### 17.1 繰り返し利用するリソースを毎回作り直さない

ログ Writer、MainWindow、AutomationElement、UI tree など、適切な lifetime を設定できるものは再利用する。

ただし cache の lifetime が実際の対象より長くならないよう注意する。

### 17.2 最も狭い探索範囲から開始する

```text
Known element
    ↓
Direct child
    ↓
Owner
    ↓
Process
    ↓
Desktop
```

必要になるまで探索範囲を広げない。

### 17.3 一度取得した UI tree を再利用する

同じ Window に対して何度も `FindAllDescendants()` を実行しない。

Window、AutomationElement、descendants、DataGridView row など、再利用可能な情報は適切な lifetime で cache する。

### 17.4 Cache の lifetime を対象 UI と合わせる

MainForm の Element は `GuiSession` 単位で保持する。

一時的な Dialog の Element は、その Dialog が存在する期間だけ保持する。

Window が閉じた後や Gui.exe 再起動後に古い AutomationElement を再利用しない。

### 17.5 Fallback を消すことと高速化を混同しない

Fallback は正常系では通らなくても、環境差や UI timing に対する安全装置である。

Fast path を追加して通常ケースを高速化し、Fallback は維持する。

### 17.6 検証目的に必要な情報だけ取得する

例えば、

```text
「選択されている式神名は何か？」
```

が不要で、

```text
「式神が選択されているか？」
```

だけが必要なら、文字列取得ではなく selection index を確認する。

### 17.7 UIA と Win32 を適材適所で使う

Win32 が常に UIA より高速とは限らない。

Windows 標準 Control の同期メッセージが有効な箇所では Win32 を利用し、UI Automation の方が自然な箇所では FlaUI を使用する。

Generic Operator や Waiter の内部でも、必要に応じて UIA と Win32 の両方を利用する。

### 17.8 PostClick の後には完了条件が必要

PostClick は同期処理を消すための API ではない。

```text
PostClick
    ↓
明確な状態変化を待つ
```

という組み合わせで使用する。

また、明確な完了条件が存在しても、それだけで PostClick が高速になるとは限らない。

Button event と後続待機を実際に重ねられるかを確認し、実測して採否を判断する。

### 17.9 Window が閉じたことと処理完了を同一視しない

Dialog close 後に MainForm が処理を継続するケースが存在する。

必要に応じて、

- enabled state
- busy state
- file existence
- Window existence

など、実際の完了条件を待つ。

### 17.10 GitHub Actions の単一 Run を信用しすぎない

絶対時間ではなく、

- 複数 Run
- 対象 Command
- control group
- PASS / FAIL
- ログ
- キャプチャ
- 録画

を組み合わせて判断する。

---

## 18. 今後の変更時に注意する箇所

以下は現在意図的に安全性を優先している。

性能改善目的だけで削除・変更しないこと。

### 18.1 ProcessWindowWaiter の Fallback

Native owner path だけに限定しない。

Owner / Native relationship だけでは取得できない Window が存在する可能性がある。

### 18.2 FileDialogWaiter の Fallback

FileDialog は Windows / UI Automation 上の表現差が大きいため、単一路線に固定しない。

### 18.3 `waitForLoadCompleted()`

Load Dialog close だけでは MainForm 反映完了を保証できない。

### 18.4 Snapshot Comparison の `waitForEnabled()`

FileDialog close と Compare Button enable は別イベントである。

### 18.5 Clear / Shikigami update の `WaitWhileBusy()`

MainForm 側の後処理との race 防止に使用している。

### 18.6 Native-owned Window の UIA readiness 確認

HWND が存在することと AutomationElement が安全に利用できることは同一ではない。

### 18.7 FileDialog の EN_CHANGE

Save Dialog の File name input は、文字列を書き換えるだけでは内部状態が更新されない場合がある。

`EN_CHANGE` は単なる冗長通知として削除しない。

---

## 19. 代表的な関連コミット

今回の改善は多数の小さな変更を段階的に適用している。

代表的なコミットを以下に示す。

| Commit | 内容 |
| --- | --- |
| `e586b7c` | Scenario log writing の効率化 |
| `b8b85a1` | FileDialog detection の最適化 |
| `ecfe5e2` | Dialog Button lookup の再利用 |
| `e428609` | SaveData Dialog Element lookup の再利用 |
| `70011ee` | MainForm Element Map の GuiSession 共有 |
| `3d18dd6` | Clear 完了同期 |
| `0a88fc2` | FileDialog search scope の縮小 |
| `c4f71bf` | Save Dialog の owner-scope 対応 |
| `4dc4cc5` | modal dialog 起動の non-blocking click |
| `7bee8b4` | Native-owner FileDialog Fast path |
| `62fd6fb` | Native-owner candidate の再試行 |
| `5a39bf1` | Window Waiter の責務分離 |
| `fffbff3` | ProcessWindow の native-owner Fast path |
| `0cb96c2` | stale UIA2 Element 対応 |
| `76784a5` | FileDialog candidate search の最適化 |
| `52d95b9` | Dialog existence check の専用高速化 |
| `aceca30` | Main Element lookup の child-first 化 |
| `b0cb04f` | Calculation result check の最小化 |
| `1ec5139` | Load file path の冗長 wait 削除 |
| `8d528b0` | Main Element Map の direct-child 化 |
| `08034cd` | Dialog Element Map の direct-child 化 |
| `98f0492` | 効果が確認できなかった Shikigami Register Button の PostClick 化を取り消し |

コミット単位では一時的な診断、修正、Revert、再設計も存在する。

また、一部の実験は効果確認後に hard reset 等で Git 履歴から削除されている。

そのため、この表は変更履歴そのものではなく、最終設計を理解するための代表点として扱う。

---

## 20. 設計全体像

Scenario Runner の UI 操作は、Feature Operator から Generic Operator や各種 Waiter を利用し、操作内容に応じて UI Automation と Win32 API を使い分ける。

概念的には以下の構成となる。

```text
Scenario
   |
   v
ScenarioExecutor
   |
   v
Feature Operator
   |
   +-----------------------+
   |                       |
   v                       v
Generic Operator          Waiter
   |                       |
   |                 +-----+----------------+
   |                 |                      |
   |                 v                      v
   |          ProcessWindowWaiter      FileDialogWaiter
   |                 |                      |
   +-----------------+----------+-----------+
                              |
                    +---------+---------+
                    |                   |
                    v                   v
                FlaUI / UIA          Win32 API
                    |                   |
                    +---------+---------+
                              |
                              v
                           Gui.exe
```

UI Automation と Win32 API は上下に固定されたレイヤーではなく、Scenario Runner が Windows UI を操作・探索するための手段である。

例えば、

- `ButtonOperator` は FlaUI Invoke と `PostMessage(BM_CLICK)` を使い分ける。
- `ComboBoxOperator` は Win32 の `CB_*` message を利用する。
- `FileDialogOperator` は UI Automation と Win32 message を併用する。
- `ProcessWindowWaiter` / `FileDialogWaiter` は UI Automation 探索と Win32 の Window relationship を併用する。

したがって、

> UI Automation か Win32 のどちらかへ統一する

ことを目的とせず、対象 Control、必要な同期特性、探索コストに応じて適切な手段を選択する。

---

## 21. ProcessWindowWaiter の Window 探索戦略

Owner が既知の Process Window を取得する場合、`ProcessWindowWaiter` は概念的に以下の順序で探索する。

```text
             Window が必要
                   |
                   v
           Owner は既知か？
              /         \
            Yes          No
             |            |
             v            v
      Native owner     Process scope
             |
        found? ---- Yes ----> Return
             |
             No
             v
       Owner child
             |
        found? ---- Yes ----> Return
             |
             No
             v
     Owner descendant
             |
        found? ---- Yes ----> Return
             |
             No
             v
      Process fallback
             |
             v
           Return
```

通常ケースを Fast path で高速化しながら、特殊ケースを Fallback で救済する構成となっている。

FileDialog については `FileDialogWaiter` が専用の探索戦略を持つため、この図をそのまま適用するものではない。

---

## 22. 今回の高速化で重要だった判断

今回の高速化では、単純に「速くなった変更を残す」だけではなく、以下のような判断を繰り返した。

### 22.1 速くても不安定なら採用しない

UI Automation では数百 ms の削減より Scenario の再現性を優先する。

race condition が発生した場合は、原因となる同期処理を復元または再設計した。

### 22.2 効果が測定できなければ過度に複雑化しない

理論上高速であっても、実測で効果が確認できずコードだけが複雑になる場合は採用しない。

### 22.3 未測定と効果なしを区別する

性能測定を実施していない変更について、

> 効果がなかった

とは判断しない。

例えば `e586b7c` のログ出力改善では、変更単体での性能測定は実施していない。

しかし、ログ出力ごとの file open / close を削減していることは実装上明確であり、従来実装より処理コストを削減する設計となっている。

このような変更については、

- 実測で改善を確認
- 未測定だが設計上のコスト削減あり
- 実測したが明確な改善なし

を区別して記録する。

### 22.4 効果が小さくても設計が単純になる場合は維持できる

小規模 Dialog の direct-child Map のように性能差が測定誤差範囲でも、

- 探索範囲が明確になる
- MainForm と設計原則を統一できる
- 実装が複雑化しない

場合は設計改善として維持する。

### 22.5 Shared infrastructure は慎重に変更する

`ProcessWindowWaiter` や `FileDialogWaiter` のように多数の Scenario が依存する共有処理は、特定 Command の高速化だけを理由に挙動を狭めない。

必要であれば `DialogOperator.Exists()` のように呼び出し側へ専用 Fast path を設ける。

---

## 23. 結論

今回の性能改善で最も重要だったのは、polling interval や timeout の数字を削ることではなかった。

Scenario Runner が行っていた、

- ログ出力ごとのファイル open / close
- 必要以上に広い UI tree の探索
- 同じ tree の繰り返し探索
- 取得済み Window / Element の再取得
- 正常系でも実行される高コストな Fallback
- 重複した状態検証
- modal operation の同期時間

といった処理を一つずつ見直した。

これらに対して、

```text
繰り返し利用できるものを再利用する
        +
探索範囲を狭める
        +
取得結果を再利用する
        +
通常系に Fast path を設ける
        +
異常系の Fallback は残す
        +
非同期化した箇所には明確な完了条件を設ける
```

という設計へ変更した。

高速化対応の開始点である `e586b7c` では、ログ Writer を再利用することでログ出力ごとのファイル操作を削減した。

その後の UI Automation 高速化でも同じ考え方を適用し、MainWindow、AutomationElement、UI tree、FileDialog の探索結果などを再利用する方向へ設計を変更した。

また、

- CacheRequest
- TextBox の Win32 化
- 無条件な PostClick 化
- 過剰な詳細性能ログ

など、理論上高速に見える方式でも実測では改善しない、または別のコストを発生させるケースが確認された。

したがって今後も、

> 性能改善は API 単体の速さではなく、処理全体の critical path と、安定性を含めて評価する。

ことを基本方針とする。

性能改善のために必要な大規模な探索・同期構造の見直しは、本資料作成時点で完了とする。

今後は新しい Scenario や UI を追加する際も、本資料で整理した、

- resource reuse
- search scope
- cache lifetime
- Fast path / Fallback
- UI synchronization

の設計原則を維持する。
