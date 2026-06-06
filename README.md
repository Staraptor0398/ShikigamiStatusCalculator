# ShikigamiStatusCalculator

## Overview

ShikigamiStatusCalculator is a desktop application for calculating the final status of shikigami in Onmyoji RPG.

The application allows users to:

- Select a shikigami
- Configure mitama information
- Calculate final status values
- Save and load calculation data
- Compare calculation snapshots
- Add and edit shikigami master data

---

## Requirements

- Windows 10 or later
- .NET Framework 4.8

---

## How to Start

1. Extract the release package.
2. Open the extracted folder.
3. Run `Gui.exe`.

---

## Features

### Shikigami Selection

- Select a shikigami from master data.
- Filter shikigami by rarity.

### Status Calculation

- Select mitama main stats.
- Select and enter mitama sub stats.
- Select set effects.
- Select unique effects.
- Calculate final status values.

### Save and Load

The application supports the following save data types.

#### Build

Stores:

- Selected shikigami
- Mitama information
- Calculation settings

#### MitamaSet

Stores:

- Mitama information only

#### CalculationSnapshot

Stores:

- Calculation result snapshots

### Snapshot Comparison

- Save calculation results as snapshots.
- Load two snapshots and compare their status values.
- Display differences between calculation results.

### Shikigami Data Management

- Add new shikigami data.
- Edit existing shikigami data.

---

## Directory Structure

```text
ShikigamiStatusCalculator/
├─ Gui.exe
├─ Gateway.dll
├─ Newtonsoft.Json.dll
│
├─ Data/
│  ├─ AppVersion.txt
│  ├─ ShikigamiData.csv
│  │
│  ├─ Backup/
│  │  └─ ShikigamiData_yyyyMMdd_HHmmss.csv
│  │
│  └─ Broken/
│     └─ ShikigamiData_broken_yyyyMMdd_HHmmss.csv
│
├─ SaveData/
│  ├─ Build/
│  │  └─ *.build.json
│  │
│  ├─ MitamaSet/
│  │  └─ *.mitama.json
│  │
│  └─ Snapshot/
│     └─ *.snapshot.json
│
└─ Log/
   └─ ShikigamiApp_yyyyMMdd.log

```

### Data

Stores application data files.

#### AppVersion.txt

Stores the application version.

#### ShikigamiData.csv

Stores shikigami master data.

### Data/Backup

Stores backup copies of shikigami master data.

### Data/Broken

Stores damaged shikigami master data files detected during automatic recovery.

### SaveData/Build

Stores Build save data files.

### SaveData/MitamaSet

Stores MitamaSet save data files.

### SaveData/Snapshot

Stores CalculationSnapshot save data files.

### Log

Stores application log files.

---

## Automatic Recovery

If `ShikigamiData.csv` is missing or damaged:

1. The damaged file is moved to the `Data/Broken` folder.
2. Default master data is restored automatically.
3. The application reloads the restored data.

---

## Backup Management

Before updating shikigami master data:

- A backup file is created automatically.
- Old backup files are removed automatically.
- Old broken files are removed automatically.

---

## Notes

- This application is designed for Windows environments.
- Manual editing of save data files is not recommended.

---

## Version

Current Release:

V00.01.00
