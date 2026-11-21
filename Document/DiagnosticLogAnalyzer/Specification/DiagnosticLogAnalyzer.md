# DiagnosticLogAnalyzer 仕様・運用資料

確認基準：2026-11-21 / master `71171ba85fe292416d1d79997ca6a2852ce379cf`

## 1. 目的と適用範囲

DiagnosticLogAnalyzerは、Scenario RunnerのE2Eシナリオ実行ログ、Guiの診断ログ、PerformanceLoggerのセッションログを突き合わせ、**Gui内部の診断イベントとE2E操作の対応関係・イベントのライフサイクル整合性**を分析するオフラインツールである。

- シナリオのPASS/FAIL自体はScenarioRunnerCliが判定する。Analyzerの`Pass`は診断イベントの整合性を表し、GUI操作そのものの正しさを独立に保証しない。
- 相関できないイベントを即座にアプリの異常とはみなさない。確定相関と参考候補、直接の判定と追加根拠による解決を区別する。
- ログの記録内容自体は `Document/ShikigamiApp/Specification/Logger/LoggerSpecification.md` と `LogEventDefinition.md` を参照する。
- 対象プロジェクトは `DiagnosticLogAnalyzer`（.NET Framework 4.8、x64）、`DiagnosticLogAnalyzerTest`、`Script/Analysis`、分析用PowerShellスクリプト、およびReroll Workflowである。

## 2. 入力と処理フロー

```text
ScenarioRunnerCli実行ログ（ScenarioRunner_*.log）
Gui診断ログ（ShikigamiApp_yyyyMMdd.log）
Performanceログ（ShikigamiApp_Performance_yyyyMMdd.log）
      │
      ▼
Script/PrepareDiagnosticAnalysis.ps1
  ├─ Scenario / Command / GuiWindowの解析
  ├─ Performance sessionの解析
  └─ GuiWindow ↔ Performance sessionの照合
      │
      ▼
Run/DiagnosticAnalysisInput.json（schemaVersion=1）
      │
      ▼
DiagnosticLogAnalyzer.exe
  ├─ DiagnosticLogParser
  ├─ DiagnosticSessionCorrelator
  ├─ DiagnosticCommandCorrelator
  ├─ DiagnosticCommandContextAnalyzer
  ├─ DiagnosticEventLifecycleValidator
  └─ SnapshotCompareModalContinuationValidator
      │
      ├─ コンソール出力 → DiagnosticAnalysis.txt
      └─ DiagnosticAnalysisResult.json（schemaVersion=1）
                    │
                    ▼
          Script/WriteDiagnosticSummary.ps1
                    │
                    ├─ DiagnosticSummary.md
                    └─ GitHub Actions Job Summary
```

`DiagnosticAnalysisInput.json`は**解析入力**、`DiagnosticAnalysisResult.json`は**解析結果**である。両者は用途もスキーマも異なる。結果JSONはC#側で判定済みのオブジェクトから直接構築し、Console出力テキストを再解析して生成しない。

## 3. 相関と判定の意味

### 3.1 Session相関

`DiagnosticSessionCorrelator`は診断イベントの`SessionId`を用いて、GuiWindowおよびPerformanceセッションと照合する。判定状態は`WindowMatched`、`PerformanceOnly`、`Unmatched`、`Ambiguous`。未確定イベントについては時間帯から参考となるWindow候補を示すが、候補は**確定一致ではない**。

### 3.2 Command相関

`DiagnosticCommandCorrelator`はSession相関結果とScenarioのCommand実行区間から、`Matched`、`NoConfirmedWindow`、`NoMatchingCommand`、`Ambiguous`、`NotMappedOperation`に分類する。`NoMatchingCommand`は必ずしも異常ではない。`DiagnosticCommandContextAnalyzer`で、その時点に実行中だったコマンドがあるかを追加調査する。

### 3.3 ライフサイクル検証

`DiagnosticEventLifecycleValidator`の現行対象は`Calculation`、`SaveData.Load`、`SaveData.Save`、`MainForm.Clear`、`Snapshot.Compare`。`Pass`、`Fail`、`Inconclusive`に分類する。

- `Pass`：対象の診断イベント列が定義されたライフサイクルに整合する。
- `Fail`：定義に反するイベント列を確認した。
- `Inconclusive`：開始・終端イベントなどの証拠が不足し、正常・異常を確定できない。

`SnapshotCompareModalContinuationValidator`は、比較結果ダイアログをまたいで`Completed`が記録されるケースについて`Resolved`、`Unresolved`、`Ambiguous`の**追加証拠**を付ける。元の直接判定とCommand相関を書き換えない。結果JSONの各項目では`directVerdict`と`effectiveStatus`を分け、`modalStatus`と`modalReason`を保持する。

**留意点：** `Inconclusive`、`Unmatched`、`NoMatchingCommand`を単純にCI失敗へ変換しない。現行Analyzerの終了コードは、主として引数・実行例外・診断ログ構文解析問題を示し、全ライフサイクル結果のQuality Gateではない。

## 4. コマンドライン

```text
DiagnosticLogAnalyzer.exe <DiagnosticAnalysisInput.json> [DiagnosticAnalysisResult.json]
```

- 引数1個：従来のコンソール解析出力。
- 引数2個：コンソール解析に加え、指定先に構造化JSONを出力。
- 現行終了コード：`0`=構文解析問題なし、`1`=診断ログの構文解析問題あり、`2`=引数不正または実行例外。
- 相関未確定や`Inconclusive`は、それだけでは終了コードを非0にしない。

CIでは`Script/RunDiagnosticAnalysis.ps1`がAnalyzerをビルドし、ログを集め、入力JSONを生成し、AnalyzerおよびSummary生成を呼び出す。

## 5. GitHub ActionsとArtifact

統合先は `.github/workflows/scenario-runner-batch-reroll.yml`。選択されたRunnerにおいて、**Scenario Batchの実行・録画が終わった後**、`Analyze diagnostic logs`と`Upload diagnostic analysis`を実行する。Analyzerビルドを候補Runner選定・録画開始前の時間計測へ混入させない。

通常時のArtifact名は`scenario-runner-diagnostic-analysis`。代表的な構成：

```text
DiagnosticAnalysis/
├─ DiagnosticAnalysis.txt          # 従来の全文テキスト結果
├─ DiagnosticAnalysisResult.json   # 構造化解析結果
├─ DiagnosticSummary.md            # 人間向けMarkdown
├─ DiagnosticTiming.log            # Stage別時間
├─ PreparationTiming.log           # Scenario別・処理区間別時間
├─ AnalyzerBuild.stdout.log
├─ AnalyzerBuild.stderr.log
├─ Analyzer.stdout.txt
├─ Analyzer.stderr.txt
└─ Run/
   ├─ DiagnosticAnalysisInput.json
   ├─ ScenarioRunnerCli/bin/x64/Release/Log/ScenarioRunner_*.log
   └─ Gui/bin/x64/Release/Log/ShikigamiApp_*.log
```

ログ欠落や失敗時には一部ファイルが生成されない場合がある。詳細なログは再解析・障害調査用として保持する。`DiagnosticSummary.md`はGitHub Actionsの`GITHUB_STEP_SUMMARY`にも追記するため、Artifactをダウンロードせずに概要と対象Scenario別判定を確認できる。

診断処理は**現状、Workflow全体の成否を上書きしない設計**。診断失敗は警告と`DiagnosticAnalysisError.txt`等で調査できるようにするが、CIの品質ゲートとして導入されたわけではない。

### ビルド待機について

`RunDiagnosticAnalysis.ps1`では`Start-Process -Wait`をdotnetビルド待機に使わず、起動した`dotnet`プロセスの`WaitForExit()`で終了を待つ。`Start-Process -Wait`による子孫プロセス待機で約15分の余分な時間が発生したため（Run `44875934585`）、Run `44878045606`で改善を確認した。将来ここを変更するときは**実際のビルド時間と待機時間を分けて評価**する。

## 6. 2026-11-21時点のE2E検証記録

| Run | 観測・結果 | 備考 |
|---|---|---|
| [44873905281](https://github.com/Staraptor0398/ShikigamiStatusCalculator/actions/runs/44873905281) | Scenario 50/50 PASS。解析成功。 | 初回CI統合。解析段階は約15分。 |
| [44875934585](https://github.com/Staraptor0398/ShikigamiStatusCalculator/actions/runs/44875934585) | Build 905,955.92ms、入力準備822.72ms、Analyzer107.01ms。 | Timing導入。ボトルネックはビルド待機。 |
| [44878045606](https://github.com/Staraptor0398/ShikigamiStatusCalculator/actions/runs/44878045606) | 50/50 PASS。診断解析全体17,185.19ms。 | ビルド待機修正後。前回約906,998msから約52.8倍短縮。 |
| [44880857343](https://github.com/Staraptor0398/ShikigamiStatusCalculator/actions/runs/44880857343) | 50/50 PASS、Analyzer終了コード0、Preparation issues 0。直接PASS 79、モーダル継続Resolved 7、未解決0。診断解析全体10,553.98ms。 | 構造化JSON、Markdown、Job Summary、Artifactアップロード成功。 |

上記時間は**各Run固有の実測**であり、Runner個体・キュー・その他の条件が異なる。特定修正の効果として把握する際も、異なるサンプルを一般的な定常性能と混同しない。

## 7. 運用手順と未実装事項

- 日常確認はGitHub ActionsのJob Summaryから行い、問題がある場合は`DiagnosticAnalysisResult.json`でScenario・Window・Command行番号・Operation・`effectiveStatus`を特定し、必要な元ログへ進む。
- 再調査はArtifactの`Run/`と入力JSONを使う。`schemaVersion`やAnalyzerのバージョンが変わる場合、結果を無条件で互換扱いしない。
- 解析結果だけでScenarioのPASSを代替しない。両方を確認する。
- **今回の機能は2026-11-21時点で一旦完結扱い。** 将来の任意候補として、Quality Gate導入、解析対象Operationの追加、DiagnosticLogAnalyzerTestのCI実行、他Workflowへの展開などは挙げられるが、いずれも着手済み・必須残件として扱わない。

## 8. 関連実装・資料

- `DiagnosticLogAnalyzer/Program.cs`、`Correlation/`、`Parser/`、`Reader/`、`Validation/`、`Model/`
- `DiagnosticLogAnalyzerTest/TestCase/`
- `Script/PrepareDiagnosticAnalysis.ps1`、`Script/RunDiagnosticAnalysis.ps1`、`Script/WriteDiagnosticSummary.ps1`
- `Script/Analysis/ScenarioSessionParser.ps1`、`ScenarioCommandParser.ps1`、`PerformanceSessionParser.ps1`、`SessionMatcher.ps1`
- `.github/workflows/scenario-runner-batch-reroll.yml`
- `Document/ShikigamiApp/Specification/Logger/LoggerSpecification.md`
- `Document/ShikigamiApp/Specification/Logger/LogEventDefinition.md`
