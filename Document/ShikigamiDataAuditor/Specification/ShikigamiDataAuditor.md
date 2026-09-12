# Shikigami Data Auditor 設計仕様書

## 1. 概要

Shikigami Data Auditorは、ShikigamiAppが保持する式神ステータスと、公式変更履歴および外部参照データを照合するための保守用コンソールツールである。

本ツールの目的は、式神追加やバランス調整による `Gui/Data/ShikigamiData.csv` の更新漏れ、転記誤り、外部参照データとの不一致を検出し、人間が確認可能な監査結果を出力することである。

本ツールは監査のみを行い、`ShikigamiData.csv` を自動更新しない。

## 2. 設計方針

- 日本版公式サイトの告知を最も信頼度の高い根拠として扱う。
- 外部サイトは、公式情報で把握できていない差分を発見するための補助情報として扱う。
- 外部サイトとの不一致だけを根拠に、アプリデータの誤りを確定しない。
- 初期値、覚醒後、最大レベル、覚醒ボーナスを区別する。
- 比較対象の範囲が一致しない値を、単純な不一致として扱わない。
- ネットワーク障害や外部サイトの変更によって、ローカルCSVの監査全体が実行不能にならないようにする。
- 取得した外部データと監査結果には、取得日時および出典を残す。
- 元データは読み取り専用として扱い、自動修正は将来も既定動作にしない。

## 3. 対象範囲

### 3.1 対象

- `Gui/Data/ShikigamiData.csv` に登録されている式神
- SP、SSR、SRの式神
- 攻撃力
- HP
- 防御力
- 素早さ
- 会心率
- 会心DMG
- 効果命中
- 効果抵抗
- 日本版公式サイトで確認した式神ステータス変更履歴
- 外部サイトに掲載された式神の静的ステータス

### 3.2 対象外

- スキル倍率
- 回復量、バリア量、付与確率
- 戦闘中のみ発生するバフおよびデバフ
- 召喚物やダンジョン敵のステータス
- 陰陽師および御霊のステータス
- 御魂セット効果および固有効果
- 外部サイトの情報を根拠とした `ShikigamiData.csv` の自動更新
- ShikigamiApp本体の実行時処理への組み込み

## 4. 情報源と優先順位

情報源の優先順位を以下とする。

| 優先順位 | 情報源 | 用途 | 扱い |
|---:|---|---|---|
| 1 | 日本版公式サイトの告知 | 変更内容と実装日の確定 | 公式根拠 |
| 2 | ShikigamiApp公式変更履歴CSV | 公式告知を比較可能な形式へ正規化 | 公式根拠の索引 |
| 3 | 外部参照サイト | 未把握の差分検知、現在値の補助確認 | 非公式参考情報 |
| 4 | ShikigamiAppのCSV | 監査対象となる現在のアプリデータ | 比較対象 |

外部参照サイトと公式変更履歴が矛盾する場合は、公式変更履歴を優先する。

公式変更履歴は完全な現行ステータス一覧ではない。そのため、公式変更履歴に記録がない項目について外部参照サイトと不一致になった場合は、誤りを確定せず手動確認対象とする。

## 5. 入力データ

### 5.1 ShikigamiData.csv

監査対象となる既存ファイルは以下とする。

`Gui/Data/ShikigamiData.csv`

列構成は既存仕様を変更しない。

| 列 | 内容 | 単位 |
|---|---|---|
| レア度 | SP、SSR、SR | - |
| 式神名 | 日本版の式神名 | - |
| 攻撃力 | 基礎攻撃力 | ポイント |
| HP | 基礎HP | ポイント |
| 防御力 | 基礎防御力 | ポイント |
| 素早さ | 基礎素早さ | ポイント |
| 会心率 | 会心率 | % |
| 会心DMG | 会心ダメージ | % |
| 効果命中 | 効果命中 | % |
| 効果抵抗 | 効果抵抗 | % |

本設計では、アプリCSVが保持する値を「覚醒後・レベル40で、アプリの計算に使用する完成状態の基礎ステータス」とし、内部では `AWAKENED_LEVEL_40` として扱う。公式告知や外部サイトから取得した値が初期値、未覚醒値、覚醒ボーナスのいずれかである場合は、同一範囲であることを確認できない限り直接比較しない。

### 5.2 OfficialChangeHistory.csv

日本版公式サイトで確認した変更履歴を、監査用の入力データとして保持する。

想定配置先：

`ShikigamiDataAuditor/Data/OfficialChangeHistory.csv`

列構成は以下とする。

| 列 | 内容 |
|---|---|
| effective_date | ゲームへ反映された日 |
| announcement_date | 告知日 |
| rarity | レア度 |
| shikigami | 日本版の式神名 |
| stat_scope | 公式記載値の範囲 |
| stat | 対象ステータス |
| old_value | 変更前の値。公式記載がなければ空欄 |
| new_value | 変更後の値 |
| unit | pointまたはpercent |
| old_value_known | 変更前の値が公式に記載されているか |
| current_app_scope | 現在のアプリ対象か |
| review_priority | 確認優先度 |
| app_action | アプリ側で必要と考えられる対応 |
| source_url | 日本版公式サイトの告知URL |
| notes | 補足 |

`stat_scope` は最低限、以下を区別する。

| 値 | 意味 |
|---|---|
| initial | 初期状態の値 |
| base | 公式告知上の基礎値 |
| max_level_base | 最大レベル時の基礎値 |
| awakened_base | 覚醒後の基礎値 |
| awakening_bonus | 覚醒による常時加算値 |
| unknown | 範囲を特定できない値 |

### 5.3 ExternalNameMap.csv

日本版の式神名と外部サイトのページ名を対応付ける。

想定配置先：

`ShikigamiDataAuditor/Data/ExternalNameMap.csv`

列構成は以下とする。

| 列 | 内容 |
|---|---|
| rarity | レア度 |
| shikigami | 日本版の式神名 |
| source | 外部情報源を識別する名前 |
| page_title | 外部サイトのページタイトル |
| enabled | 取得対象か |
| notes | 表記差などの補足 |

レア度と日本版式神名を複合キーとする。同名式神や派生式神をページタイトルの部分一致だけで自動決定しない。

## 6. 外部データ取得

### 6.1 初期対応情報源

初期実装では、萌娘百科の「陰陽師手游」式神ページを外部参照先とする。

萌娘百科は公式情報源ではなく、更新が遅れている場合がある。実際に公式変更後の値が反映されていないページが存在するため、取得値は監査の補助情報としてのみ使用する。

### 6.2 取得方式

MediaWiki Action APIを使用し、ページのWikitextまたは解析済みHTMLを取得する。

基本要求は以下を想定する。

```text
GET https://zh.moegirl.org.cn/api.php
    ?action=parse
    &page=<page_title>
    &prop=wikitext
    &format=json
    &formatversion=2
```

Wikitextから安定して値を抽出できないページについては、`prop=text` で取得したHTMLの解析を代替手段とする。ただし、Wikitext解析とHTML解析を同一クラスへ混在させない。

MediaWiki Action APIは `api.php` をエンドポイントとして提供され、Parse APIではページを指定してWikitextまたはHTMLを取得できる。

参考：

- [MediaWiki Action API](https://www.mediawiki.org/wiki/API:Action_API)
- [Parsing wikitext](https://www.mediawiki.org/wiki/API:Parsing_wikitext)
- [Get the contents of a page](https://www.mediawiki.org/wiki/API:Get_the_contents_of_a_page)
- [萌娘百科：姑獲鳥](https://zh.moegirl.org.cn/阴阳师手游:姑获鸟)
- [萌娘百科：不見岳](https://zh.moegirl.org.cn/阴阳师手游:不见岳)

### 6.3 通信条件

- HTTPタイムアウトを設定する。
- 連続要求の間隔を設定可能にする。
- User-Agentにツール名およびバージョンを設定する。
- 同一ページを1回の監査中に複数回取得しない。
- HTTPエラー、タイムアウト、JSON形式不正を区別して記録する。
- 取得失敗を理由に、公式変更履歴との照合まで中断しない。

### 6.4 キャッシュ

取得した外部データはローカルキャッシュへ保存する。

想定配置先：

`ShikigamiDataAuditor/Cache/<source>/<rarity>_<shikigami>.json`

キャッシュには以下を保持する。

- 情報源
- 日本版式神名
- 外部ページタイトル
- 取得日時
- ページのリビジョンID
- 取得元URL
- 取得した原文
- 解析結果

ネットワーク取得に失敗し、有効なキャッシュが存在する場合はキャッシュを使用できる。ただし、監査結果にはキャッシュ使用と取得日時を明記する。

キャッシュの有効期限は設定可能とする。期限切れキャッシュしか存在しない場合は、値を確定情報として扱わず手動確認対象とする。

`Cache` 配下は原則としてバージョン管理対象外とする。

## 7. 正規化

### 7.1 ステータス種類

入力元ごとの表記を内部の `StatType` へ変換する。

| 内部値 | ShikigamiData.csv | 外部サイトで想定される表記 |
|---|---|---|
| ATTACK | 攻撃力 | 攻击、攻擊 |
| HP | HP | 生命 |
| DEFENSE | 防御力 | 防御、防禦 |
| SPEED | 素早さ | 速度 |
| CRITICAL_RATE | 会心率 | 暴击、暴擊 |
| CRITICAL_DAMAGE | 会心DMG | 暴击伤害、暴擊傷害 |
| EFFECT_HIT | 効果命中 | 效果命中 |
| EFFECT_RESIST | 効果抵抗 | 效果抵抗 |

### 7.2 数値

- アプリCSVの小数値は `double` として読み込む。
- %記号は除去し、百分率の数値として保持する。
- `150%` は内部値 `150` として扱う。
- 全角数字および全角小数点は半角へ正規化する。
- 比較時は既定では完全一致とし、許容誤差を導入する場合は設定値として明示する。
- ランク表記と数値が併記されている場合は、数値のみを比較対象とする。

### 7.3 ステータス範囲

外部サイトから以下の範囲を識別する。

| 内部値 | 意味 |
|---|---|
| INITIAL | 未覚醒・レベル1 |
| AWAKENED | 覚醒後。レベル不明 |
| MAX_LEVEL | 最大レベル。覚醒状態不明 |
| AWAKENED_LEVEL_40 | 覚醒後・レベル40 |
| AWAKENING_BONUS | 覚醒効果による常時加算 |
| UNKNOWN | 判定不能 |

`ShikigamiData.csv` と直接比較できる範囲は `AWAKENED_LEVEL_40`、または同等であることを仕様上確認できた値に限る。

### 7.4 式神名

- アプリCSVでは日本版の名称を正規名とする。
- 外部ページ名は `ExternalNameMap.csv` で解決する。
- 簡体字、繁体字、日本の字体の変換だけで式神を確定しない。
- マッピングが存在しない式神は `NAME_UNRESOLVED` として報告する。

## 8. 監査処理

### 8.1 複合キー

式神の識別には以下を使用する。

```text
rarity + shikigami
```

監査対象項目の識別には以下を使用する。

```text
rarity + shikigami + stat
```

### 8.2 公式変更履歴の選択

同じ式神・ステータスに複数の公式変更が存在する場合は、`effective_date` の昇順で履歴として保持し、現在値との比較には最新の適用済み変更を使用する。

ただし、最新履歴の `stat_scope` がアプリCSVと直接比較できない場合は、過去の直接比較可能な値へ戻って比較せず、`SCOPE_MISMATCH` とする。

### 8.3 判定結果

`AuditOutcome` は以下を想定する。

| 値 | 意味 |
|---|---|
| MATCH | 比較可能な値が一致した |
| CONFIRMED_MISMATCH | 同一範囲の公式値とアプリ値が不一致 |
| REFERENCE_MISMATCH | 外部参照値とアプリ値が不一致。公式根拠未確認 |
| SOURCE_STALE | 外部参照値が、より新しい公式変更と矛盾する |
| SCOPE_MISMATCH | 初期値と覚醒後など、比較範囲が一致しない |
| MANUAL_REVIEW | 自動判定に必要な情報が不足している |
| SHIKIGAMI_NOT_FOUND | 公式履歴または外部マッピングの式神がアプリCSVに存在しない |
| REFERENCE_NOT_FOUND | 外部ページまたはステータス値を取得できない |
| NAME_UNRESOLVED | 日本版式神名と外部ページ名を解決できない |
| INVALID_DATA | CSVまたは取得データの形式が不正 |

### 8.4 判定規則

| 公式値 | 外部値 | アプリ値との関係 | 判定 |
|---|---|---|---|
| 同一範囲で存在 | 任意 | 公式値と一致 | MATCH |
| 同一範囲で存在 | 任意 | 公式値と不一致 | CONFIRMED_MISMATCH |
| 異なる範囲で存在 | 任意 | 単純比較不能 | SCOPE_MISMATCH |
| なし | 同一範囲で存在 | 外部値と一致 | MATCH |
| なし | 同一範囲で存在 | 外部値と不一致 | REFERENCE_MISMATCH |
| 公式変更後の値が存在 | 公式変更前の値 | 外部値のみ古い | SOURCE_STALE |
| なし | 取得不能 | 判定不能 | REFERENCE_NOT_FOUND |

公式値と外部値が矛盾する場合は、アプリ値の一致状況とは別に外部情報源を `SOURCE_STALE` として報告する。同じ式神・ステータスについて、アプリデータの判定と外部情報源の判定を別々の `AuditResult` として出力してよい。

例：

```text
公式変更後: 不見岳 会心DMG 150
外部参照値: 不見岳 会心DMG 130
アプリ値:   不見岳 会心DMG 130
```

この場合、アプリデータは `CONFIRMED_MISMATCH`、外部参照データは `SOURCE_STALE` とする。

## 9. 出力

### 9.1 Markdownレポート

想定ファイル：

`Audit/ShikigamiDataAudit_yyyyMMdd_HHmmss.md`

以下を出力する。

- 実行日時
- 入力ファイル
- 外部データ取得日時
- 判定件数の集計
- 確定不一致
- 外部参照のみの不一致
- 手動確認対象
- 外部情報源の未取得または陳腐化
- 一致項目
- 各項目の根拠URL

### 9.2 CSVレポート

想定ファイル：

`Audit/ShikigamiDataAudit_yyyyMMdd_HHmmss.csv`

列構成は以下を想定する。

| 列 | 内容 |
|---|---|
| outcome | 判定結果 |
| rarity | レア度 |
| shikigami | 式神名 |
| stat | ステータス種類 |
| app_value | アプリCSVの値 |
| official_value | 公式変更後の値 |
| reference_value | 外部参照値 |
| stat_scope | 比較した値の範囲 |
| evidence | 判定根拠 |
| source_url | 根拠URL |
| fetched_at | 外部データ取得日時 |
| notes | 補足 |

## 10. コマンドライン

基本実行例：

```text
ShikigamiDataAuditor.exe
    --app-data "../Gui/Data/ShikigamiData.csv"
    --official-history "Data/OfficialChangeHistory.csv"
    --external-name-map "Data/ExternalNameMap.csv"
    --output "Audit"
```

想定オプション：

| オプション | 内容 |
|---|---|
| --app-data | 監査対象CSV |
| --official-history | 公式変更履歴CSV |
| --external-name-map | 外部ページ対応CSV |
| --output | レポート出力先 |
| --no-network | 外部通信を行わずキャッシュのみ使用 |
| --cache-max-age | キャッシュ有効期間 |
| --request-interval | 外部要求間隔 |

## 11. 終了コード

| コード | 意味 |
|---:|---|
| 0 | 確定不一致なし。手動確認項目が存在する場合を含む |
| 1 | `CONFIRMED_MISMATCH` を1件以上検出 |
| 2 | 入力ファイル不正など、監査を開始または完了できないエラー |
| 3 | 外部データ取得または解析が一部失敗。公式履歴の監査結果は出力済み |

外部サイトとの不一致だけでは終了コード1にしない。

## 12. クラス責務

### 12.1 Access

| クラス | 責務 |
|---|---|
| ShikigamiDataCsvReader | アプリCSVを読み込み、式神ステータスへ変換する |
| OfficialChangeHistoryCsvReader | 公式変更履歴CSVを読み込む |
| ExternalNameMapCsvReader | 外部ページ対応CSVを読み込む |
| AuditReportFileWriter | 生成されたレポートをファイルへ保存する |

### 12.2 External

| クラス | 責務 |
|---|---|
| IShikigamiStatusSource | 外部情報源から式神ステータスを取得するための境界 |
| MoegirlShikigamiStatusSource | 萌娘百科向けの取得処理を構成する |
| MediaWikiClient | MediaWiki Action APIとのHTTP通信を担当する |
| MoegirlWikitextParser | Wikitextからステータスを解析する |
| MoegirlHtmlParser | HTMLからステータスを解析する代替手段 |
| ExternalStatusCache | 外部取得データの保存と読み込みを担当する |

### 12.3 Audit

| クラス | 責務 |
|---|---|
| ShikigamiDataAuditor | 監査処理全体を制御する |
| OfficialChangeResolver | 現在値と比較すべき最新の公式変更を選択する |
| StatScopeMatcher | 値の範囲が直接比較可能か判定する |
| AuditOutcomeResolver | 公式値、外部値、アプリ値から判定結果を決定する |

### 12.4 Report

| クラス | 責務 |
|---|---|
| MarkdownAuditReportWriter | 人間が確認するMarkdownレポートを生成する |
| CsvAuditReportWriter | 機械処理可能なCSVレポートを生成する |

## 13. 依存関係

- Shikigami Data Auditorは独立した.NET Framework 4.8コンソールプロジェクトとする。
- Gui、Gateway、Coreへプロジェクト参照しない。
- `ShikigamiData.csv` はファイルとして読み込む。
- ShikigamiApp本体のソリューションへ含めず、専用の `ShikigamiDataAuditor.slnx` を用意する。
- 外部情報源固有の処理を `IShikigamiStatusSource` の背後へ隔離する。
- 外部サイト追加時にAudit層を変更しない構造を目標とする。

## 14. エラー処理

- アプリCSVのヘッダー不正、列数不正、数値変換失敗は `INVALID_DATA` とする。
- アプリCSVそのものを読み込めない場合は監査を中止する。
- 公式変更履歴の形式不正は監査を中止する。
- 外部ページ1件の取得失敗で監査全体を中止しない。
- 外部解析に失敗した原文はキャッシュへ残し、解析処理の修正後に再利用できるようにする。
- 例外の詳細は標準エラー出力へ出し、通常レポートには利用者向けの要約を出す。

## 15. テスト方針

最低限、以下を自動テスト対象とする。

- 正常なアプリCSVの読み込み
- ヘッダー不正
- 列数不正
- 数値不正
- 公式履歴の最新変更選択
- 同一範囲の一致および不一致
- 初期値と覚醒後値の `SCOPE_MISMATCH`
- 外部値だけが異なる `REFERENCE_MISMATCH`
- 公式値より外部値が古い `SOURCE_STALE`
- 外部ページ名未解決
- Wikitextの正常解析
- Wikitext解析失敗時のHTML解析
- ネットワーク失敗時のキャッシュ利用
- MarkdownおよびCSVレポートの主要項目

外部サイトへ実通信するテストと、保存したレスポンスを使用するパーサーテストを分離する。通常のテスト実行では外部通信を必須にしない。

## 16. セキュリティおよび運用上の注意

- 外部サイトから取得した文字列をファイルパスとして直接使用しない。
- 外部ページ名はURLエンコードする。
- レポート出力時にCSVおよびMarkdownの制御文字を適切に処理する。
- 外部レスポンスを信頼せず、サイズ上限を設定する。
- 認証情報は使用しない。
- 外部サイトの利用条件およびアクセス負荷へ配慮する。
- 監査結果の確定不一致は、公式告知本文を再確認してからアプリCSVへ反映する。

## 17. 初期実装の完了条件

- 日本版公式変更履歴を読み込める。
- 萌娘百科から、対応表に登録された式神のステータスを取得できる。
- 外部取得失敗時も公式変更履歴との監査を継続できる。
- 不見岳の会心DMGについて、アプリ値130、公式値150、外部値130の状態を正しく分類できる。
- 時曜滝夜叉姫、姑獲鳥、絡新婦など、同一範囲で比較可能な既知差分を検出できる。
- 大嶽丸の初期素早さなど、範囲が一致しない値を確定不一致にしない。
- MarkdownおよびCSVの監査結果を出力できる。
- `ShikigamiData.csv` を変更しない。

## 18. 将来拡張

- 外部情報源の追加
- 新式神の未登録検出
- 御魂データの監査
- 公式告知から変更履歴候補を抽出する補助機能
- CIでの定期監査
- 前回監査結果との差分レポート
- 人間が承認した変更だけを反映する更新候補CSVの生成

自動更新を追加する場合も、監査結果の確認および明示的な承認を必須とする。

## 19. 関連図

| ファイル | 内容 |
|---|---|
| `Diagram/data_flow.wsd` | 入力、外部取得、キャッシュ、監査、レポート出力のデータフロー |
| `Diagram/class_diagram.wsd` | アプリケーション、アクセス、外部取得、監査、レポートの処理構成 |
| `Diagram/model_class_diagram.wsd` | 監査で使用するモデルの属性と関連 |
