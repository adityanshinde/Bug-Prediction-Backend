# ?? Frontend API – How To Use Guide

> **For:** Angular developers and AI agents building the frontend dashboard.
> **Base URL (dev):** `http://localhost:5234`
> **All responses:** `application/json`
> **Auth required:** ? No token needed from frontend — backend handles SonarCloud auth internally.

---

## ?? Table of Contents

1. [All Endpoints at a Glance](#1-all-endpoints-at-a-glance)
2. [GET /api/projects](#2-get-apiprojects)
3. [GET /api/projects/{id}/header](#3-get-apiprojectsidheader)
4. [GET /api/projects/{id}/dashboard](#4-get-apiprojectsiddashboard)
5. [GET /api/projects/{id}/metrics](#5-get-apiprojectsidmetrics)
6. [GET /api/projects/{id}/risk-analysis](#6-get-apiprojectsidrisk-analysis)
7. [GET /api/projects/{id}/quality-gates](#7-get-apiprojectsidquality-gates)
8. [GET /api/projects/{id}/scan-history](#8-get-apiprojectsidscan-history)
9. [POST /api/sync/{projectKey}](#9-post-apisyncprojectkey)
10. [POST /api/sync/all](#10-post-apisyncall)
11. [HTTP Status Codes](#11-http-status-codes)
12. [TypeScript Interface Definitions](#12-typescript-interface-definitions)
13. [Angular Service – Ready to Use](#13-angular-service--ready-to-use)
14. [Field Reference – All DTOs](#14-field-reference--all-dtos)

---

## 1. All Endpoints at a Glance

| Method | Endpoint | Used On Page | Returns |
|--------|----------|-------------|---------|
| `GET` | `/api/projects` | Projects List page | `ProjectListDto[]` |
| `GET` | `/api/projects/{id}/header` | Every page – top header bar | `HeaderDto` |
| `GET` | `/api/projects/{id}/dashboard` | Dashboard page | `DashboardDto` |
| `GET` | `/api/projects/{id}/metrics` | Metrics page | `MetricsDto` |
| `GET` | `/api/projects/{id}/risk-analysis` | Risk Analysis page | `RiskAnalysisDto` |
| `GET` | `/api/projects/{id}/quality-gates` | Quality Gates page | `QualityGateDto` |
| `GET` | `/api/projects/{id}/scan-history` | Scan History page | `ScanHistoryDto[]` |
| `POST` | `/api/sync/{projectKey}` | Sync button (optional) | `{ message: string }` |
| `POST` | `/api/sync/all` | Admin sync all (optional) | `{ message: string }` |

> `{id}` = integer project ID (from `/api/projects` response)
> `{projectKey}` = string project key e.g. `BPCQ_axios-sonar`

---

## 2. GET /api/projects

**Page:** Projects List
**Purpose:** Get all synced projects to show in a project selector or list page.

### Request
```
GET /api/projects
```
No parameters. No body.

### Response – `ProjectListDto[]`

```json
[
  {
    "projectId": 1,
    "projectKey": "BPCQ_axios-sonar",
    "name": "axios-sonar",
    "organization": "bpcq",
    "visibility": "public",
    "lastScanDate": "2026-02-24T11:55:19"
  },
  {
    "projectId": 2,
    "projectKey": "BPCQ_ClawWork-sonarqube",
    "name": "ClawWork-sonarqube",
    "organization": "bpcq",
    "visibility": "public",
    "lastScanDate": "2026-02-24T10:02:52"
  }
]
```

### Field Reference

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `projectId` | `number` | ? | Primary key – use this as `{id}` in all other API calls |
| `projectKey` | `string` | ? | SonarCloud project key – use this for `/api/sync/{projectKey}` |
| `name` | `string` | ? | Human-readable project name for display |
| `organization` | `string` | ? | SonarCloud organization key |
| `visibility` | `string` | ? | `"public"` or `"private"` |
| `lastScanDate` | `string (ISO datetime)` | ? | Last time SonarCloud scanned this project |

---

## 3. GET /api/projects/{id}/header

**Page:** Shown at the top of every project page (header bar)
**Purpose:** Display project name, active branch, last scan time, and quality gate badge.

### Request
```
GET /api/projects/1/header
```

### Response – `HeaderDto`

```json
{
  "projectName": "axios-sonar",
  "branch": "v1.x",
  "lastScanDate": "2026-02-24T11:55:19",
  "qualityGateStatus": "FAIL",
  "commitId": "31b1865ede1a735f774605e145b2d1d236e33ddf"
}
```

### Field Reference

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `projectName` | `string` | ? | Display name of the project |
| `branch` | `string` | ? | Name of the main/latest scanned branch |
| `lastScanDate` | `string (ISO datetime)` | ? | Timestamp of the most recent scan |
| `qualityGateStatus` | `string` | ? | `"PASS"` or `"FAIL"` — use for badge colour |
| `commitId` | `string` | ? | Full Git commit SHA of the last scan |

> **UI Tip:** Show `qualityGateStatus` as a green badge for `"PASS"` and red for `"FAIL"`.

---

## 4. GET /api/projects/{id}/dashboard

**Page:** Dashboard
**Purpose:** Single aggregated call that powers the entire dashboard page — KPI cards, donut charts, severity chart, and recent scans table.

### Request
```
GET /api/projects/1/dashboard
```

### Response – `DashboardDto`

```json
{
  "kpis": {
    "bugs": 29,
    "vulnerabilities": 11,
    "codeSmells": 713,
    "coverage": 45.00,
    "duplication": 3.70,
    "securityRating": "5.0",
    "reliabilityRating": "5.0",
    "maintainabilityRating": "1.0",
    "qualityGate": "FAIL"
  },
  "issueDistribution": {
    "bugs": 29,
    "vulnerabilities": 11,
    "codeSmells": 713
  },
  "severityDistribution": {
    "blocker": 11,
    "critical": 153,
    "major": 290,
    "minor": 295,
    "info": 4
  },
  "recentScans": [
    {
      "scanDate": "2026-02-24T11:55:19",
      "branch": "v1.x",
      "commit": "31b1865ede1a735f774605e145b2d1d236e33ddf",
      "qualityGate": "FAIL"
    }
  ]
}
```

### Field Reference – `kpis`

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `bugs` | `number` | ? | Total bug count |
| `vulnerabilities` | `number` | ? | Total vulnerability count |
| `codeSmells` | `number` | ? | Total code smell count |
| `coverage` | `number (decimal)` | ? | Code coverage percentage e.g. `45.00` = 45% |
| `duplication` | `number (decimal)` | ? | Duplicated lines density percentage |
| `securityRating` | `string` | ? | SonarCloud rating `"1.0"`=A `"2.0"`=B `"3.0"`=C `"4.0"`=D `"5.0"`=E |
| `reliabilityRating` | `string` | ? | Same 1–5 scale as securityRating |
| `maintainabilityRating` | `string` | ? | Same 1–5 scale as securityRating |
| `qualityGate` | `string` | ? | `"PASS"` or `"FAIL"` |

### Field Reference – `issueDistribution`

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `bugs` | `number` | ? | Used for donut chart – Bug slice |
| `vulnerabilities` | `number` | ? | Used for donut chart – Vulnerability slice |
| `codeSmells` | `number` | ? | Used for donut chart – Code Smell slice |

### Field Reference – `severityDistribution`

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `blocker` | `number` | ? | BLOCKER severity issue count |
| `critical` | `number` | ? | CRITICAL severity issue count |
| `major` | `number` | ? | MAJOR severity issue count |
| `minor` | `number` | ? | MINOR severity issue count |
| `info` | `number` | ? | INFO severity issue count |

### Field Reference – `recentScans[]`

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `scanDate` | `string (ISO datetime)` | ? | When the scan ran |
| `branch` | `string` | ? | Branch that was scanned |
| `commit` | `string` | ? | Git commit SHA |
| `qualityGate` | `string` | ? | `"PASS"` or `"FAIL"` for that scan |

> **Rating Scale Reference:**
> `"1.0"` = A (Best) · `"2.0"` = B · `"3.0"` = C · `"4.0"` = D · `"5.0"` = E (Worst)

---

## 5. GET /api/projects/{id}/metrics

**Page:** Metrics
**Purpose:** KPI summary cards, coverage trend line chart, bugs vs vulnerabilities bar chart, and module metrics table.

### Request
```
GET /api/projects/1/metrics
```

### Response – `MetricsDto`

```json
{
  "kpis": {
    "bugs": 29,
    "codeSmells": 713,
    "coverage": 45.00,
    "duplication": 3.70
  },
  "coverageTrend": [
    { "date": "2026-02-24T10:02:32", "coverage": 45.00 },
    { "date": "2026-02-24T10:08:11", "coverage": 45.00 },
    { "date": "2026-02-24T11:55:19", "coverage": 45.00 }
  ],
  "bugsVsVulnerabilities": [
    { "date": "2026-02-24T10:02:32", "bugs": 29, "vulnerabilities": 11 },
    { "date": "2026-02-24T11:55:19", "bugs": 29, "vulnerabilities": 11 }
  ],
  "moduleMetrics": [
    {
      "moduleName": "test/unit/adapters",
      "qualifier": "DIR",
      "language": null,
      "bugs": 1,
      "vulnerabilities": 3,
      "codeSmells": 0,
      "coverage": 0.00,
      "duplication": 20.70,
      "complexity": 0,
      "linesOfCode": 0
    }
  ]
}
```

### Field Reference – `kpis`

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `bugs` | `number` | ? | Total bugs from latest snapshot |
| `codeSmells` | `number` | ? | Total code smells |
| `coverage` | `number (decimal)` | ? | Coverage % from latest snapshot |
| `duplication` | `number (decimal)` | ? | Duplication % from latest snapshot |

### Field Reference – `coverageTrend[]`

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `date` | `string (ISO datetime)` | ? | Scan date – use as X-axis |
| `coverage` | `number (decimal)` | ? | Coverage % at that scan – use as Y-axis |

### Field Reference – `bugsVsVulnerabilities[]`

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `date` | `string (ISO datetime)` | ? | Scan date – use as X-axis |
| `bugs` | `number` | ? | Bug count at that scan |
| `vulnerabilities` | `number` | ? | Vulnerability count at that scan |

### Field Reference – `moduleMetrics[]`

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `moduleName` | `string` | ? | File or directory path e.g. `"test/unit/adapters"` |
| `qualifier` | `string` | ? | `"DIR"` = directory · `"FIL"` = individual file |
| `language` | `string` | ? | Programming language e.g. `"js"`, `"ts"`, `"py"` — null for directories |
| `bugs` | `number` | ? | Bug count for this module |
| `vulnerabilities` | `number` | ? | Vulnerability count for this module |
| `codeSmells` | `number` | ? | Code smell count |
| `coverage` | `number (decimal)` | ? | Coverage % for this module |
| `duplication` | `number (decimal)` | ? | Duplication % for this module |
| `complexity` | `number` | ? | Cyclomatic complexity — `0` if not available |
| `linesOfCode` | `number` | ? | Lines of code (ncloc) — `0` if not available |

---

## 6. GET /api/projects/{id}/risk-analysis

**Page:** Risk Analysis
**Purpose:** Module-level risk distribution bar chart and high-risk modules table. Backend provides raw metrics — **risk score calculation is done in Angular**.

### Request
```
GET /api/projects/1/risk-analysis
```

### Response – `RiskAnalysisDto`

```json
{
  "moduleDistribution": [
    {
      "moduleName": "test/unit/adapters",
      "bugs": 1,
      "vulnerabilities": 3,
      "coverage": 0.00,
      "duplication": 20.70,
      "complexity": 0
    }
  ],
  "highRiskModules": [
    {
      "moduleName": "examples",
      "language": null,
      "bugs": 8,
      "vulnerabilities": 0,
      "coverage": 0.00,
      "duplication": 0.00,
      "complexity": 0,
      "linesOfCode": 0
    }
  ]
}
```

### Field Reference – `moduleDistribution[]`

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `moduleName` | `string` | ? | Module/directory path |
| `bugs` | `number` | ? | Bug count – use for risk bar chart |
| `vulnerabilities` | `number` | ? | Vulnerability count |
| `coverage` | `number (decimal)` | ? | Coverage % |
| `duplication` | `number (decimal)` | ? | Duplication % |
| `complexity` | `number` | ? | Cyclomatic complexity |

### Field Reference – `highRiskModules[]`

> These are modules pre-filtered by the backend where `bugs > 0 OR vulnerabilities > 0`, sorted by highest issue count first.

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `moduleName` | `string` | ? | Module path |
| `language` | `string` | ? | Programming language — null for directories |
| `bugs` | `number` | ? | Bug count |
| `vulnerabilities` | `number` | ? | Vulnerability count |
| `coverage` | `number (decimal)` | ? | Coverage % |
| `duplication` | `number (decimal)` | ? | Duplication % |
| `complexity` | `number` | ? | Cyclomatic complexity |
| `linesOfCode` | `number` | ? | Lines of code |

> **Angular calculates:**
> Risk Score formula (from `custom-calculations.md`):
> `ModuleRisk = (bugs × 5) + (vulnerabilities × 8) + (100 - coverage) + (duplication × 3)`
> Risk Level: `0–30` = LOW · `31–70` = MEDIUM · `71+` = HIGH

---

## 7. GET /api/projects/{id}/quality-gates

**Page:** Quality Gates
**Purpose:** Current gate pass/fail status, condition breakdown table, and historical gate change timeline.

### Request
```
GET /api/projects/1/quality-gates
```

### Response – `QualityGateDto`

```json
{
  "currentStatus": "FAIL",
  "gateConditions": [],
  "history": [
    {
      "date": "2026-02-24T11:55:19",
      "branch": "v1.x",
      "status": "FAIL",
      "commitId": "31b1865ede1a735f774605e145b2d1d236e33ddf"
    },
    {
      "date": "2026-02-24T10:02:32",
      "branch": "v1.x",
      "status": "FAIL",
      "commitId": "1ebb21e8481f730bba7289e0402f43c30f969f60"
    }
  ]
}
```

### Field Reference – root

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `currentStatus` | `string` | ? | `"PASS"` or `"FAIL"` — current gate result from latest snapshot |
| `gateConditions` | `array` | ? | Condition-level breakdown — currently empty, reserved for future |
| `history` | `array` | ? | Gate status per scan — sorted latest first |

### Field Reference – `gateConditions[]`

> Currently returns `[]`. Reserved for future condition-level detail.

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `metric` | `string` | ? | Metric key e.g. `"coverage"`, `"new_security_rating"` |
| `condition` | `string` | ? | Human-readable threshold e.g. `"> 70%"` |
| `actualValue` | `string` | ? | Actual current value |
| `status` | `string` | ? | `"PASS"` or `"FAIL"` for this condition |

### Field Reference – `history[]`

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `date` | `string (ISO datetime)` | ? | Scan date — use as timeline X-axis |
| `branch` | `string` | ? | Branch name |
| `status` | `string` | ? | `"PASS"` or `"FAIL"` for that scan |
| `commitId` | `string` | ? | Git commit SHA |

---

## 8. GET /api/projects/{id}/scan-history

**Page:** Scan History / Projects Page
**Purpose:** Full list of all historical scans for a project with metrics per scan.

### Request
```
GET /api/projects/1/scan-history
```

### Response – `ScanHistoryDto[]`

```json
[
  {
    "scanDate": "2026-02-24T11:55:19",
    "branch": "v1.x",
    "commitId": "31b1865ede1a735f774605e145b2d1d236e33ddf",
    "qualityGateStatus": "FAIL",
    "bugs": 29,
    "vulnerabilities": 11,
    "codeSmells": 713,
    "coverage": 45.00,
    "duplication": 3.70
  },
  {
    "scanDate": "2026-02-24T10:22:29",
    "branch": "v1.x",
    "commitId": "31b1865ede1a735f774605e145b2d1d236e33ddf",
    "qualityGateStatus": "FAIL",
    "bugs": 29,
    "vulnerabilities": 11,
    "codeSmells": 713,
    "coverage": 45.00,
    "duplication": 3.70
  }
]
```

### Field Reference

| JSON Field | Type | Nullable | Description |
|------------|------|----------|-------------|
| `scanDate` | `string (ISO datetime)` | ? | When the scan ran — sorted latest first |
| `branch` | `string` | ? | Branch that was scanned |
| `commitId` | `string` | ? | Full Git SHA of the commit at scan time |
| `qualityGateStatus` | `string` | ? | `"PASS"` or `"FAIL"` |
| `bugs` | `number` | ? | Total bugs at that scan |
| `vulnerabilities` | `number` | ? | Total vulnerabilities at that scan |
| `codeSmells` | `number` | ? | Total code smells at that scan |
| `coverage` | `number (decimal)` | ? | Coverage % at that scan |
| `duplication` | `number (decimal)` | ? | Duplication % at that scan |

---

## 9. POST /api/sync/{projectKey}

**Purpose:** Manually trigger a sync for one specific project from SonarCloud into the database.

### Request
```
POST /api/sync/BPCQ_axios-sonar
```
No body. No parameters.

### Response
```json
{
  "message": "Sync completed for project: BPCQ_axios-sonar"
}
```

> Use `projectKey` from the `/api/projects` response — the `projectKey` field.

---

## 10. POST /api/sync/all

**Purpose:** Manually trigger a full sync for all projects in the organization.

### Request
```
POST /api/sync/all
```
No body. No parameters.

### Response
```json
{
  "message": "Full sync completed."
}
```

> ?? This call can take 30–60 seconds depending on the number of projects. Show a loading indicator.

---

## 11. HTTP Status Codes

| Code | When it happens | What to do in Angular |
|------|-----------------|-----------------------|
| `200 OK` | Data found and returned | Bind to component |
| `404 Not Found` | Project ID doesn't exist, or no snapshot yet | Show empty state / "No data yet" message |

### 404 Response Body
```json
{
  "message": "Project 99 not found."
}
```
or
```json
{
  "message": "No snapshot data found."
}
```

---

## 12. TypeScript Interface Definitions

Copy-paste ready interfaces for Angular. All field names match the JSON exactly.

```typescript
// ?? /api/projects ?????????????????????????????????????????????
export interface ProjectListDto {
  projectId: number;
  projectKey: string;
  name: string;
  organization: string | null;
  visibility: string | null;
  lastScanDate: string | null;   // ISO datetime string
}

// ?? /api/projects/{id}/header ?????????????????????????????????
export interface HeaderDto {
  projectName: string;
  branch: string;
  lastScanDate: string | null;   // ISO datetime string
  qualityGateStatus: string;     // "PASS" | "FAIL"
  commitId: string | null;
}

// ?? /api/projects/{id}/dashboard ??????????????????????????????
export interface DashboardDto {
  kpis: DashboardKpisDto;
  issueDistribution: IssueDistributionDto;
  severityDistribution: SeverityDistributionDto;
  recentScans: RecentScanDto[];
}

export interface DashboardKpisDto {
  bugs: number;
  vulnerabilities: number;
  codeSmells: number;
  coverage: number;
  duplication: number;
  securityRating: string | null;        // "1.0"–"5.0"
  reliabilityRating: string | null;     // "1.0"–"5.0"
  maintainabilityRating: string | null; // "1.0"–"5.0"
  qualityGate: string;                  // "PASS" | "FAIL"
}

export interface IssueDistributionDto {
  bugs: number;
  vulnerabilities: number;
  codeSmells: number;
}

export interface SeverityDistributionDto {
  blocker: number;
  critical: number;
  major: number;
  minor: number;
  info: number;
}

export interface RecentScanDto {
  scanDate: string;        // ISO datetime string
  branch: string | null;
  commit: string | null;
  qualityGate: string | null;  // "PASS" | "FAIL"
}

// ?? /api/projects/{id}/metrics ????????????????????????????????
export interface MetricsDto {
  kpis: MetricsKpisDto;
  coverageTrend: CoverageTrendPointDto[];
  bugsVsVulnerabilities: BugsVsVulnerabilitiesPointDto[];
  moduleMetrics: ModuleMetricDto[];
}

export interface MetricsKpisDto {
  bugs: number;
  codeSmells: number;
  coverage: number;
  duplication: number;
}

export interface CoverageTrendPointDto {
  date: string;      // ISO datetime string – X axis
  coverage: number;  // decimal – Y axis
}

export interface BugsVsVulnerabilitiesPointDto {
  date: string;            // ISO datetime string – X axis
  bugs: number;
  vulnerabilities: number;
}

export interface ModuleMetricDto {
  moduleName: string;
  qualifier: string | null;   // "DIR" | "FIL"
  language: string | null;    // "js" | "ts" | "py" | null
  bugs: number;
  vulnerabilities: number;
  codeSmells: number;
  coverage: number;
  duplication: number;
  complexity: number;
  linesOfCode: number;
}

// ?? /api/projects/{id}/risk-analysis ??????????????????????????
export interface RiskAnalysisDto {
  moduleDistribution: ModuleRiskDto[];
  highRiskModules: HighRiskModuleDto[];
}

export interface ModuleRiskDto {
  moduleName: string;
  bugs: number;
  vulnerabilities: number;
  coverage: number;
  duplication: number;
  complexity: number;
}

export interface HighRiskModuleDto {
  moduleName: string;
  language: string | null;
  bugs: number;
  vulnerabilities: number;
  coverage: number;
  duplication: number;
  complexity: number;
  linesOfCode: number;
}

// ?? /api/projects/{id}/quality-gates ??????????????????????????
export interface QualityGateDto {
  currentStatus: string;               // "PASS" | "FAIL"
  gateConditions: QualityGateConditionDto[];
  history: QualityGateHistoryDto[];
}

export interface QualityGateConditionDto {
  metric: string;
  condition: string | null;
  actualValue: string | null;
  status: string;   // "PASS" | "FAIL"
}

export interface QualityGateHistoryDto {
  date: string;          // ISO datetime string
  branch: string | null;
  status: string | null; // "PASS" | "FAIL"
  commitId: string | null;
}

// ?? /api/projects/{id}/scan-history ???????????????????????????
export interface ScanHistoryDto {
  scanDate: string;              // ISO datetime string
  branch: string | null;
  commitId: string | null;
  qualityGateStatus: string | null;  // "PASS" | "FAIL"
  bugs: number;
  vulnerabilities: number;
  codeSmells: number;
  coverage: number;
  duplication: number;
}
```

---

## 13. Angular Service – Ready to Use

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ProjectListDto, HeaderDto, DashboardDto,
  MetricsDto, RiskAnalysisDto, QualityGateDto, ScanHistoryDto
} from './models/api.models'; // adjust import path as needed

@Injectable({ providedIn: 'root' })
export class ApiService {

  private readonly base = 'http://localhost:5234';

  constructor(private http: HttpClient) {}

  // ?? Projects ????????????????????????????????????????????????
  getProjects(): Observable<ProjectListDto[]> {
    return this.http.get<ProjectListDto[]>(`${this.base}/api/projects`);
  }

  // ?? Header ??????????????????????????????????????????????????
  getHeader(projectId: number): Observable<HeaderDto> {
    return this.http.get<HeaderDto>(`${this.base}/api/projects/${projectId}/header`);
  }

  // ?? Dashboard ???????????????????????????????????????????????
  getDashboard(projectId: number): Observable<DashboardDto> {
    return this.http.get<DashboardDto>(`${this.base}/api/projects/${projectId}/dashboard`);
  }

  // ?? Metrics ?????????????????????????????????????????????????
  getMetrics(projectId: number): Observable<MetricsDto> {
    return this.http.get<MetricsDto>(`${this.base}/api/projects/${projectId}/metrics`);
  }

  // ?? Risk Analysis ????????????????????????????????????????????
  getRiskAnalysis(projectId: number): Observable<RiskAnalysisDto> {
    return this.http.get<RiskAnalysisDto>(`${this.base}/api/projects/${projectId}/risk-analysis`);
  }

  // ?? Quality Gates ????????????????????????????????????????????
  getQualityGates(projectId: number): Observable<QualityGateDto> {
    return this.http.get<QualityGateDto>(`${this.base}/api/projects/${projectId}/quality-gates`);
  }

  // ?? Scan History ?????????????????????????????????????????????
  getScanHistory(projectId: number): Observable<ScanHistoryDto[]> {
    return this.http.get<ScanHistoryDto[]>(`${this.base}/api/projects/${projectId}/scan-history`);
  }

  // ?? Manual Sync (optional) ???????????????????????????????????
  syncProject(projectKey: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/api/sync/${projectKey}`, {});
  }

  syncAll(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/api/sync/all`, {});
  }
}
```

---

## 14. Field Reference – All DTOs

Quick lookup table for every field across every DTO.

### Rating Values (SonarCloud Scale)
Used in `securityRating`, `reliabilityRating`, `maintainabilityRating`:

| Value | Grade | Meaning |
|-------|-------|---------|
| `"1.0"` | A | Best |
| `"2.0"` | B | Good |
| `"3.0"` | C | Moderate |
| `"4.0"` | D | Poor |
| `"5.0"` | E | Worst |

### Quality Gate Values
Used in `qualityGate`, `qualityGateStatus`, `status` fields:

| Value | Meaning |
|-------|---------|
| `"PASS"` | Gate passed — all conditions met |
| `"FAIL"` | Gate failed — one or more conditions not met |

### Qualifier Values
Used in `qualifier` field of module-level responses:

| Value | Meaning |
|-------|---------|
| `"DIR"` | A directory / folder |
| `"FIL"` | An individual source file |

### Decimal Fields
`coverage` and `duplication` are returned as decimals:
- `45.00` means **45%**
- `3.70` means **3.7%**
- `0.00` means **0%** (not available or zero)

### Null Fields
Fields marked ? Nullable in field references can be `null` in the JSON response.
Always guard against null in Angular templates:
```html
{{ item.branch ?? 'N/A' }}
{{ item.lastScanDate | date:'mediumDate' }}
```
