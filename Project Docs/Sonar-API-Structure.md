# 📦 SonarCloud API Documentation 
## API Request & Response Structure For Table Schema and Correct API end-Points

---

## 🔍 1. Search Projects

### 📌 Endpoint

```
GET https://sonarcloud.io/api/projects/search?organization=bpcq
```

### 🏷 Description

This API is used to **retrieve all projects** under a specific SonarCloud organization.

In this case, it fetches all projects under the organization:

```
organization = bpcq
```

---

## 📥 Query Parameters

| Parameter    | Type   | Required | Description                 |
| ------------ | ------ | -------- | --------------------------- |
| organization | string | ✅ Yes    | SonarCloud organization key |

---

## 📤 Sample Response

```json
{
  "paging": {
    "pageIndex": 1,
    "pageSize": 100,
    "total": 6
  },
  "components": [
    {
      "organization": "bpcq",
      "key": "BPCQ_axios-sonar",
      "name": "axios-sonar",
      "qualifier": "TRK",
      "visibility": "public",
      "lastAnalysisDate": "2026-02-24T11:55:19+0000",
      "revision": "31b1865ede1a735f774605e145b2d1d236e33ddf"
    },
    {
      "organization": "bpcq",
      "key": "BPCQ_ClawWork-sonarqube",
      "name": "ClawWork-sonarqube",
      "qualifier": "TRK",
      "visibility": "public",
      "lastAnalysisDate": "2026-02-24T10:02:52+0000",
      "revision": "a30512aae69bbfba67966e3e0c268c7bd8fc9a52"
    }
  ]
}
```

---

## 📊 Response Structure Explanation

### 🗂 Paging Object

| Field     | Type | Description                        |
| --------- | ---- | ---------------------------------- |
| pageIndex | int  | Current page number                |
| pageSize  | int  | Number of records per page         |
| total     | int  | Total number of projects available |

---

### 🧩 Components Array

Each object inside `components` represents **one Sonar project**.

| Field            | Type   | Description                             |
| ---------------- | ------ | --------------------------------------- |
| organization     | string | Organization key                        |
| key              | string | Unique project key (Used in other APIs) |
| name             | string | Project display name                    |
| qualifier        | string | Project type (TRK = Project)            |
| visibility       | string | public / private                        |
| lastAnalysisDate | string | Last scan execution date                |
| revision         | string | Git commit SHA of last analysis         |

---

## 🧠 Backend Usage Logic (Important)

From this API response, you should:

* Store `key` → Used for calling other Sonar APIs
* Store `name` → For frontend display
* Store `lastAnalysisDate` → For tracking freshness
* Store `revision` → For commit-based tracking
* Use `total` → For pagination logic (if needed)

---

## 🔍 2. List Project Branches

### 📌 Endpoint

```http
GET https://sonarcloud.io/api/project_branches/list?project=BPCQ_axios-sonar
```

### 🏷 Description

This API retrieves **all branches** of a specific SonarCloud project along with:

* Quality Gate status
* Bug, Vulnerability, Code Smell counts
* Latest analysis details
* Commit information

In this example, branches are fetched for:

```
project = BPCQ_axios-sonar
```

---

## 📥 Query Parameters

| Parameter | Type   | Required | Description                                      |
| --------- | ------ | -------- | ------------------------------------------------ |
| project   | string | ✅ Yes    | Project key obtained from `/api/projects/search` |

---

## 📤 Sample Response

```json
{
  "branches": [
    {
      "name": "v1.x",
      "isMain": true,
      "type": "LONG",
      "status": {
        "qualityGateStatus": "ERROR",
        "bugs": 29,
        "vulnerabilities": 11,
        "codeSmells": 713
      },
      "analysisDate": "2026-02-24T11:55:19+0000",
      "commit": {
        "sha": "31b1865ede1a735f774605e145b2d1d236e33ddf",
        "author": {
          "name": "Aditya Shinde",
          "login": "adityanshinde-kOCVq@github",
          "avatar": "6240bfdcf1753959a56cb548eeec570a"
        },
        "date": "2026-02-24T10:22:22+0000",
        "message": "Update sonar-project.properties"
      },
      "branchId": "f44acfd0-bd32-42b4-bbfe-1d398f3eda15",
      "branchUuidV1": "AZyPGYDjnEax4LApFCOF"
    }
  ]
}
```

---

## 📊 Response Structure Explanation

---

### 🌿 Branch Object

| Field        | Type    | Description                           |
| ------------ | ------- | ------------------------------------- |
| name         | string  | Branch name (e.g., `v1.x`)            |
| isMain       | boolean | Indicates if this is the main branch  |
| type         | string  | `LONG` (long-lived branch) or `SHORT` |
| analysisDate | string  | Last analysis timestamp               |
| branchId     | string  | Unique branch identifier              |
| branchUuidV1 | string  | Internal Sonar branch UUID            |

---

### 🚦 Status Object (Quality Summary)

| Field             | Type   | Description           |
| ----------------- | ------ | --------------------- |
| qualityGateStatus | string | `OK` or `ERROR`       |
| bugs              | int    | Total number of bugs  |
| vulnerabilities   | int    | Total vulnerabilities |
| codeSmells        | int    | Total code smells     |

📌 `qualityGateStatus = ERROR` means Quality Gate failed.

---

### 🧾 Commit Object

| Field   | Type   | Description      |
| ------- | ------ | ---------------- |
| sha     | string | Git commit SHA   |
| date    | string | Commit timestamp |
| message | string | Commit message   |

#### 👤 Author Object

| Field  | Type   | Description        |
| ------ | ------ | ------------------ |
| name   | string | Commit author name |
| login  | string | GitHub login       |
| avatar | string | Avatar hash        |

---

## 🧠 Backend Usage Logic (Important)

From this API, you should:

* Store `branch name`
* Identify `isMain` branch (for dashboard default)
* Store `qualityGateStatus`
* Store bug / vulnerability / code smell counts
* Store commit SHA for traceability
* Store analysisDate for freshness tracking

---

## 🔗 API Dependency Flow

This API depends on:

```
/api/projects/search → gives project key
```

---

## 🔍 3. Get Project Quality Gate Status

### 📌 Endpoint

```http
GET https://sonarcloud.io/api/qualitygates/project_status?projectKey=BPCQ_axios-sonar
```

### 🏷 Description

This API returns the **Quality Gate evaluation result** of a specific project.

It provides:

* Overall Quality Gate status
* Individual metric conditions
* Threshold comparison
* Evaluation period details

In this example, Quality Gate status is fetched for:

```
projectKey = BPCQ_axios-sonar
```

---

## 📥 Query Parameters

| Parameter  | Type   | Required | Description                                    |
| ---------- | ------ | -------- | ---------------------------------------------- |
| projectKey | string | ✅ Yes    | Unique project key from `/api/projects/search` |

---

## 📤 Sample Response

```json
{
  "projectStatus": {
    "status": "ERROR",
    "conditions": [
      {
        "status": "OK",
        "metricKey": "new_reliability_rating",
        "comparator": "GT",
        "periodIndex": 1,
        "errorThreshold": "1",
        "actualValue": "1"
      },
      {
        "status": "OK",
        "metricKey": "new_security_rating",
        "comparator": "GT",
        "periodIndex": 1,
        "errorThreshold": "1",
        "actualValue": "1"
      },
      {
        "status": "OK",
        "metricKey": "new_maintainability_rating",
        "comparator": "GT",
        "periodIndex": 1,
        "errorThreshold": "1",
        "actualValue": "1"
      },
      {
        "status": "OK",
        "metricKey": "new_duplicated_lines_density",
        "comparator": "GT",
        "periodIndex": 1,
        "errorThreshold": "3",
        "actualValue": "0.0"
      },
      {
        "status": "ERROR",
        "metricKey": "new_security_hotspots_reviewed",
        "comparator": "LT",
        "periodIndex": 1,
        "errorThreshold": "100",
        "actualValue": "0.0"
      }
    ],
    "periods": [
      {
        "index": 1,
        "mode": "days",
        "date": "2026-02-24T10:02:32+0000",
        "parameter": "1"
      }
    ],
    "ignoredConditions": false
  }
}
```

---

## 📊 Response Structure Explanation

---

### 🚦 ProjectStatus Object

| Field             | Type    | Description                                  |
| ----------------- | ------- | -------------------------------------------- |
| status            | string  | Overall Quality Gate result (`OK` / `ERROR`) |
| conditions        | array   | List of metric-based evaluations             |
| periods           | array   | Evaluation period details                    |
| ignoredConditions | boolean | Indicates if any conditions were ignored     |

📌 In this case:
`status = ERROR` → Quality Gate has failed.

---

### 📈 Conditions Array

Each condition represents one Quality Gate rule.

| Field          | Type   | Description                      |
| -------------- | ------ | -------------------------------- |
| status         | string | `OK` or `ERROR`                  |
| metricKey      | string | Metric being evaluated           |
| comparator     | string | Comparison operator (`GT`, `LT`) |
| errorThreshold | string | Threshold value                  |
| actualValue    | string | Current metric value             |
| periodIndex    | int    | Reference to period object       |

---

### 📌 Important Metric Keys

| Metric                         | Meaning                         |
| ------------------------------ | ------------------------------- |
| new_reliability_rating         | Reliability grade of new code   |
| new_security_rating            | Security grade of new code      |
| new_maintainability_rating     | Maintainability rating          |
| new_duplicated_lines_density   | % of duplicated lines           |
| new_security_hotspots_reviewed | % of security hotspots reviewed |

📌 The failure occurred because:

```
new_security_hotspots_reviewed = 0.0
Threshold = 100
Comparator = LT
Status = ERROR
```

Meaning → Required 100% review but actual is 0%.

---

### 📅 Periods Object

| Field     | Description                     |
| --------- | ------------------------------- |
| mode      | Evaluation mode (e.g., days)    |
| date      | Start date of evaluation period |
| parameter | Period length                   |

---

## 🧠 Backend Usage Logic (Important)

From this API, you should:

* Store overall `status`
* Store each condition separately (for analytics)
* Identify which metric failed
* Save `actualValue` vs `errorThreshold`
* Link period information for trend comparison
* Trigger alert if `status = ERROR`

---

## 🔗 API Dependency Flow

This API depends on:

```
/api/projects/search → provides projectKey
```
---
---

## 🔍 4. Get Project Measures (Key Metrics)

### 📌 Endpoint

```http
GET https://sonarcloud.io/api/measures/component?component=BPCQ_axios-sonar&metricKeys=bugs,vulnerabilities,coverage,duplicated_lines_density,reliability_rating,security_rating,sqale_rating
```

### 🏷 Description

This API retrieves **current metric values** of a specific SonarCloud project.

It is mainly used for:

* Dashboard statistics
* Risk evaluation
* Bug prediction inputs
* Quality scoring display

In this example, metrics are fetched for:

```
component = BPCQ_axios-sonar
```

---

## 📥 Query Parameters

| Parameter  | Type   | Required | Description                              |
| ---------- | ------ | -------- | ---------------------------------------- |
| component  | string | ✅ Yes    | Project key                              |
| metricKeys | string | ✅ Yes    | Comma-separated list of required metrics |

---

## 📤 Sample Response

```json
{
  "component": {
    "id": "AZyPGYDjnEax4LApFCOF",
    "key": "BPCQ_axios-sonar",
    "name": "axios-sonar",
    "qualifier": "TRK",
    "measures": [
      {
        "metric": "bugs",
        "value": "29",
        "bestValue": false
      },
      {
        "metric": "reliability_rating",
        "value": "5.0",
        "bestValue": false
      },
      {
        "metric": "duplicated_lines_density",
        "value": "3.7",
        "bestValue": false
      },
      {
        "metric": "security_rating",
        "value": "5.0",
        "bestValue": false
      },
      {
        "metric": "vulnerabilities",
        "value": "11",
        "bestValue": false
      },
      {
        "metric": "sqale_rating",
        "value": "1.0",
        "bestValue": true
      }
    ]
  }
}
```

---

## 📊 Response Structure Explanation

---

### 🧩 Component Object

| Field     | Type   | Description                     |
| --------- | ------ | ------------------------------- |
| id        | string | Internal Sonar project ID       |
| key       | string | Project key                     |
| name      | string | Project name                    |
| qualifier | string | Project type (`TRK`)            |
| measures  | array  | List of requested metric values |

---

### 📈 Measures Array

Each object represents one metric.

| Field     | Type    | Description                             |
| --------- | ------- | --------------------------------------- |
| metric    | string  | Metric name                             |
| value     | string  | Current metric value                    |
| bestValue | boolean | Indicates if metric is at optimal value |

---

## 📌 Requested Metrics Meaning

| Metric                   | Description                                  |
| ------------------------ | -------------------------------------------- |
| bugs                     | Total number of bugs                         |
| vulnerabilities          | Total security vulnerabilities               |
| coverage                 | Code coverage percentage (not returned here) |
| duplicated_lines_density | % of duplicated code                         |
| reliability_rating       | Reliability grade (1–5 scale)                |
| security_rating          | Security grade (1–5 scale)                   |
| sqale_rating             | Maintainability rating                       |

---

## 📊 Current Project Snapshot (From Response)

| Metric                  | Value | Status Insight |
| ----------------------- | ----- | -------------- |
| Bugs                    | 29    | High           |
| Vulnerabilities         | 11    | Medium         |
| Reliability Rating      | 5.0   | Worst grade    |
| Security Rating         | 5.0   | Worst grade    |
| Duplicated Lines        | 3.7%  | Moderate       |
| Maintainability (sqale) | 1.0   | Best (A grade) |

📌 Rating Scale (SonarCloud):

* 1.0 → A (Best)
* 2.0 → B
* 3.0 → C
* 4.0 → D
* 5.0 → E (Worst)

---

## 🧠 Backend Usage Logic (Important)

From this API, you should:

* Convert measures array into key-value mapping
* Store metric values for analytics
* Identify risky metrics (rating ≥ 4)
* Use data for:

  * Risk scoring
  * ML bug prediction model
  * Dashboard display
  * Alert triggering

If `bestValue = false` → metric is not optimal.

---

## 🔗 API Dependency Flow

This API depends on:

```
/api/projects/search → provides project key
```

---
---

## 🔍 5. Search Project Issues (With Severity & Type Facets)

### 📌 Endpoint

```http
GET https://sonarcloud.io/api/issues/search?componentKeys=project_key&severities=BLOCKER,CRITICAL,MAJOR,MINOR,INFO&ps=1&facets=severities,types
```

### 🏷 Description

This API retrieves **issues of a project** along with aggregated statistics (facets).

It provides:

* Total issues count
* Severity-wise distribution
* Type-wise distribution (Bug, Vulnerability, Code Smell)
* Technical debt & effort estimates

In this example:

```
componentKeys = project_key
severities = BLOCKER, CRITICAL, MAJOR, MINOR, INFO
ps = 1 (page size)
facets = severities, types
```

⚠ `ps=1` is used to minimize returned issue data while still getting facet counts.

---

## 📥 Query Parameters

| Parameter     | Type   | Required | Description               |
| ------------- | ------ | -------- | ------------------------- |
| componentKeys | string | ✅ Yes    | Project key               |
| severities    | string | Optional | Filter by severity levels |
| ps            | int    | Optional | Page size                 |
| facets        | string | Optional | Aggregation fields        |

---

## 📤 Sample Response

```json
{
  "total": 0,
  "p": 1,
  "ps": 1,
  "paging": {
    "pageIndex": 1,
    "pageSize": 1,
    "total": 0
  },
  "effortTotal": 0,
  "debtTotal": 0,
  "issues": [],
  "components": [],
  "organizations": [],
  "facets": [
    {
      "property": "severities",
      "values": [
        { "val": "INFO", "count": 0 },
        { "val": "MINOR", "count": 0 },
        { "val": "MAJOR", "count": 0 },
        { "val": "CRITICAL", "count": 0 },
        { "val": "BLOCKER", "count": 0 }
      ]
    },
    {
      "property": "types",
      "values": [
        { "val": "CODE_SMELL", "count": 0 },
        { "val": "BUG", "count": 0 },
        { "val": "VULNERABILITY", "count": 0 }
      ]
    }
  ]
}
```

---

## 📊 Response Structure Explanation

---

### 📌 Root Level Fields

| Field       | Type  | Description                        |
| ----------- | ----- | ---------------------------------- |
| total       | int   | Total number of issues             |
| effortTotal | int   | Estimated effort to fix issues     |
| debtTotal   | int   | Technical debt value               |
| issues      | array | List of issue objects (empty here) |
| facets      | array | Aggregated issue counts            |

📌 In this case:

```
total = 0
```

→ No issues found for given filters.

---

### 📈 Facets – Severity Distribution

| Severity | Count |
| -------- | ----- |
| BLOCKER  | 0     |
| CRITICAL | 0     |
| MAJOR    | 0     |
| MINOR    | 0     |
| INFO     | 0     |

---

### 🧩 Facets – Issue Type Distribution

| Type          | Count |
| ------------- | ----- |
| BUG           | 0     |
| VULNERABILITY | 0     |
| CODE_SMELL    | 0     |

---

## 🧠 Backend Usage Logic (Important)

From this API, you should:

* Store `total` issue count
* Extract severity-wise counts from facets
* Extract type-wise counts from facets
* Ignore `issues` array if only counts required
* Use for:

  * Risk scoring
  * Severity heatmaps
  * Alert thresholds
  * Bug prediction inputs

⚡ Optimization Tip:

If you only need aggregated counts:

```
ps=1
```

prevents loading full issue data → improves performance.

---

## 🔗 API Dependency Flow

Depends on:

```
/api/projects/search → provides project key
```
---
---

## 🔍 6. Get Project Analysis History

### 📌 Endpoint

```http
GET https://sonarcloud.io/api/project_analyses/search?project=BPCQ_axios-sonar
```

### 🏷 Description

This API retrieves the **analysis history** of a project.

It provides:

* All past scan executions
* Quality Gate change events
* Version events
* Associated Git revisions
* Timeline for trend analysis

In this example:

```
project = BPCQ_axios-sonar
```

---

## 📥 Query Parameters

| Parameter | Type   | Required | Description |
| --------- | ------ | -------- | ----------- |
| project   | string | ✅ Yes    | Project key |

---

## 📤 Sample Response

```json
{
  "paging": {
    "pageIndex": 1,
    "pageSize": 100,
    "total": 5
  },
  "analyses": [
    {
      "key": "0fe5683a-99f1-4bc9-8327-4915e09ee604",
      "date": "2026-02-24T11:55:19+0000",
      "events": [
        {
          "key": "AZyPgdF2TZXaewIJMK3v",
          "category": "VERSION",
          "name": "not provided"
        }
      ],
      "projectVersion": "not provided",
      "manualNewCodePeriodBaseline": false,
      "revision": "31b1865ede1a735f774605e145b2d1d236e33ddf"
    },
    {
      "key": "be605756-6a4d-4525-88ea-4854ab19fa32",
      "date": "2026-02-24T10:22:29+0000",
      "events": [],
      "projectVersion": "not provided",
      "manualNewCodePeriodBaseline": false,
      "revision": "31b1865ede1a735f774605e145b2d1d236e33ddf"
    },
    {
      "key": "a765b091-6458-4657-a9d0-a907756c5047",
      "date": "2026-02-24T10:21:20+0000",
      "events": [
        {
          "key": "AZyPK4DyRJN5UoZEevlD",
          "category": "QUALITY_GATE",
          "name": "Red (was Green)",
          "description": "Security Hotspots Reviewed on New Code < 100"
        }
      ],
      "projectVersion": "not provided",
      "manualNewCodePeriodBaseline": false,
      "revision": "5ef312afe3e0195f56b9e5b34c9bfab9a7e419f5"
    },
    {
      "key": "6da32029-0b2a-4722-9e51-5583a413baa7",
      "date": "2026-02-24T10:08:11+0000",
      "events": [],
      "projectVersion": "not provided",
      "manualNewCodePeriodBaseline": false,
      "revision": "78c98891e97dd044675071abcc909419953107f0"
    },
    {
      "key": "650046e1-e1c7-4d24-b0bf-8805dde5089b",
      "date": "2026-02-24T10:02:32+0000",
      "events": [],
      "projectVersion": "not provided",
      "manualNewCodePeriodBaseline": false,
      "revision": "1ebb21e8481f730bba7289e0402f43c30f969f60"
    }
  ]
}
```

---

## 📊 Response Structure Explanation

---

### 📄 Paging Object

| Field     | Description              |
| --------- | ------------------------ |
| pageIndex | Current page             |
| pageSize  | Records per page         |
| total     | Total number of analyses |

---

### 🧩 Analysis Object

Each object represents one project scan execution.

| Field                       | Type    | Description                      |
| --------------------------- | ------- | -------------------------------- |
| key                         | string  | Unique analysis ID               |
| date                        | string  | Analysis execution date          |
| revision                    | string  | Git commit SHA                   |
| projectVersion              | string  | Version label                    |
| manualNewCodePeriodBaseline | boolean | Baseline override indicator      |
| events                      | array   | Events triggered during analysis |

---

### 📌 Events Object

Events indicate important state changes.

| Category     | Meaning                        |
| ------------ | ------------------------------ |
| VERSION      | Version tagged during analysis |
| QUALITY_GATE | Quality Gate status change     |

Example from response:

```
Red (was Green)
Description: Security Hotspots Reviewed on New Code < 100
```

Meaning:

* Quality Gate changed from PASS to FAIL
* Due to hotspot review threshold failure

---

## 📈 Analysis Timeline Insight

Based on response:

* 5 total analyses
* Latest analysis at: `2026-02-24T11:55:19`
* One Quality Gate transition detected
* Multiple revisions scanned in short time span

This is useful for:

* Trend graphs
* Regression detection
* Risk spike detection
* Commit-to-quality correlation

---

## 🧠 Backend Usage Logic (Important)

From this API, you should:

* Store analysis timeline
* Map revision → analysis date
* Detect Quality Gate transitions
* Track failure reasons from events
* Use for:

  * Historical trend dashboard
  * Bug prediction timeline model
  * Risk spike alerts
  * Dev performance analytics

---

## 🔗 API Dependency Flow

Depends on:

```
/api/projects/search → provides project key
```
---
---

## 🔍 7. Get Component Tree Metrics (Directory & File Level)

### 📌 Endpoint

```http
GET https://sonarcloud.io/api/measures/component_tree?metricKeys=bugs,vulnerabilities,coverage,duplicated_lines_density,reliability_rating,security_rating,sqale_rating&component=BPCQ_axios-sonar
```

### 🏷 Description

This API retrieves **metrics at directory and file level** for a project.

Unlike `/api/measures/component` (which returns only project-level summary), this API provides:

* Project summary metrics
* Folder-level metrics
* File-level metrics
* Language information
* Code hierarchy structure

This is useful for:

* Hotspot detection
* Risky file identification
* Folder-wise risk scoring
* Detailed bug prediction modeling

---

## 📥 Query Parameters

| Parameter  | Type   | Required | Description                 |
| ---------- | ------ | -------- | --------------------------- |
| component  | string | ✅ Yes    | Project key                 |
| metricKeys | string | ✅ Yes    | Comma-separated metric list |

---

## 📤 Sample Response (Trimmed Structure)

```json
{
  "paging": {
    "pageIndex": 1,
    "pageSize": 100,
    "total": 261
  },
  "baseComponent": {
    "key": "BPCQ_axios-sonar",
    "qualifier": "TRK",
    "measures": [...]
  },
  "components": [
    {
      "key": "BPCQ_axios-sonar:.github/workflows",
      "qualifier": "DIR",
      "path": ".github/workflows",
      "measures": [...]
    },
    {
      "key": "BPCQ_axios-sonar:test/specs/__helpers.js",
      "qualifier": "FIL",
      "path": "test/specs/__helpers.js",
      "language": "js",
      "measures": [...]
    }
  ]
}
```

---

## 📊 Response Structure Explanation

---

### 📄 Paging Object

| Field     | Description                         |
| --------- | ----------------------------------- |
| pageIndex | Current page                        |
| pageSize  | Records per page                    |
| total     | Total components (261 in this case) |

---

### 🏗 BaseComponent (Project Summary)

Contains overall project-level metrics.

Example:

| Metric             | Value |
| ------------------ | ----- |
| Bugs               | 29    |
| Vulnerabilities    | 11    |
| Reliability Rating | 5.0   |
| Security Rating    | 5.0   |
| Duplicated Lines   | 3.7%  |
| Maintainability    | 1.0   |

---

### 📁 Components Array

Each item represents either:

| Qualifier | Meaning   |
| --------- | --------- |
| TRK       | Project   |
| DIR       | Directory |
| FIL       | File      |

---

### 📌 Component Fields

| Field     | Description                      |
| --------- | -------------------------------- |
| key       | Unique component identifier      |
| name      | File/Folder name                 |
| qualifier | DIR or FIL                       |
| path      | Relative project path            |
| language  | Programming language (for files) |
| measures  | Metrics for that component       |

---

## 📈 Key Insights From Response

### 🔴 High Risk Areas Detected

Example risky components from response:

* `test/unit/adapters`

  * Vulnerabilities: 3
  * Security Rating: 5.0
  * Duplicated Lines: 20.7%

* `examples`

  * Bugs: 8
  * Reliability Rating: 3.0

* `test/specs/helpers/combineURLs.spec.js`

  * Duplicated Lines: 79.2%

* `test/helpers`

  * Reliability Rating: 4.0

---

## 🧠 Backend Usage Logic (Very Important)

This API is powerful for your **BugPredictionBackend**.

You should:

1. Loop through `components`
2. Convert `measures` array into dictionary format
3. Categorize:

   * High bug files
   * High vulnerability files
   * High duplication files
4. Assign risk score per file
5. Store:

   * path
   * language
   * bugs
   * vulnerabilities
   * reliability_rating
   * security_rating
   * duplicated_lines_density

This becomes your:

* ML training dataset
* Risk heatmap source
* Module-level prediction input
* Refactoring priority list

---

## 📊 Why This API Is Critical

| API                 | Level               |
| ------------------- | ------------------- |
| /measures/component | Project-level       |
| /component_tree     | Folder + File-level |

This enables:

* Granular analysis
* Micro-level prediction
* Developer performance tracking
* Hotspot identification

---

## 🔗 API Dependency Flow

Depends on:

```
/api/projects/search → provides project key
```
---
---

## 🔍 8. Get Metric History (Trend Data)

### 📌 Endpoint

```http
GET https://sonarcloud.io/api/measures/search_history?component=BPCQ_axios-sonar&metrics=coverage
```

### 🏷 Description

This API retrieves **historical trend data** for a specific metric of a project.

It is used for:

* Trend visualization (line charts)
* Regression detection
* Quality evolution tracking
* ML time-series modeling

In this example:

```
component = BPCQ_axios-sonar
metrics = coverage
```

---

## 📥 Query Parameters

| Parameter | Type   | Required | Description                               |
| --------- | ------ | -------- | ----------------------------------------- |
| component | string | ✅ Yes    | Project key                               |
| metrics   | string | ✅ Yes    | Metric name (comma-separated if multiple) |

---

## 📤 Sample Response

```json
{
  "paging": {
    "pageIndex": 1,
    "pageSize": 100,
    "total": 5
  },
  "measures": [
    {
      "metric": "coverage",
      "history": [
        {
          "date": "2026-02-24T10:02:32+0000",
          "value": "45"
        },
        {
          "date": "2026-02-24T10:08:11+0000",
          "value": "45"
        },
        {
          "date": "2026-02-24T10:21:20+0000",
          "value": "45"
        },
        {
          "date": "2026-02-24T10:22:29+0000",
          "value": "45"
        },
        {
          "date": "2026-02-24T11:55:19+0000",
          "value": "45"
        }
      ]
    }
  ]
}
```

---

## 📊 Response Structure Explanation

---

### 📄 Paging Object

| Field     | Description              |
| --------- | ------------------------ |
| pageIndex | Current page             |
| pageSize  | Records per page         |
| total     | Total historical entries |

---

### 📈 Measures Object

| Field   | Description                |
| ------- | -------------------------- |
| metric  | Metric name                |
| history | Array of historical values |

---

### 🕒 History Object

| Field | Description               |
| ----- | ------------------------- |
| date  | Analysis timestamp        |
| value | Metric value at that time |

---

## 📊 Trend Insight From Response

Metric: **coverage**

| Date  | Coverage |
| ----- | -------- |
| 10:02 | 45%      |
| 10:08 | 45%      |
| 10:21 | 45%      |
| 10:22 | 45%      |
| 11:55 | 45%      |

📌 Observation:
Coverage remained constant at **45%** across all 5 analyses.

---

## 🧠 Backend Usage Logic (Very Important for BugPredictionBackend)

This API is extremely important for:

### ✅ 1. Trend Graphs

* Plot date vs coverage
* Show increase/decrease

### ✅ 2. Regression Detection

If coverage drops suddenly → mark risk

### ✅ 3. Time-Series ML Input

Use:

* Previous N coverage values
* Previous bug counts
* Previous vulnerability counts
  To predict future defects.

### ✅ 4. Stability Indicator

If metric constant → stable
If fluctuating → unstable module

---

## 📌 Advanced Usage

You can request multiple metrics:

```
metrics=coverage,bugs,vulnerabilities,reliability_rating
```

This will give multi-metric time history in one call.

---

## 🔗 API Dependency Flow

Depends on:

```
/api/projects/search → provides project key
```
---
---

## 🔍 9. Get Bugs vs Vulnerabilities History (For Bar Chart)

### 📌 Endpoint

```http
GET https://sonarcloud.io/api/measures/search_history?component=BPCQ_axios-sonar&metrics=bugs,vulnerabilities
```

### 🏷 Description

This API retrieves **historical values** for:

* Total Bugs
* Total Vulnerabilities

It is mainly used for:

* Bugs vs Vulnerabilities comparison chart
* Risk growth tracking
* ML time-series analysis
* Dashboard bar/line chart visualization

In this example:

```
component = BPCQ_axios-sonar
metrics = bugs,vulnerabilities
```

---

## 📥 Query Parameters

| Parameter | Type   | Required | Description                  |
| --------- | ------ | -------- | ---------------------------- |
| component | string | ✅ Yes    | Project key                  |
| metrics   | string | ✅ Yes    | Comma-separated metric names |

---

## 📤 Sample Response

```json
{
  "paging": {
    "pageIndex": 1,
    "pageSize": 100,
    "total": 5
  },
  "measures": [
    {
      "metric": "bugs",
      "history": [
        { "date": "2026-02-24T10:02:32+0000", "value": "29" },
        { "date": "2026-02-24T10:08:11+0000", "value": "29" },
        { "date": "2026-02-24T10:21:20+0000", "value": "29" },
        { "date": "2026-02-24T10:22:29+0000", "value": "29" },
        { "date": "2026-02-24T11:55:19+0000", "value": "29" }
      ]
    },
    {
      "metric": "vulnerabilities",
      "history": [
        { "date": "2026-02-24T10:02:32+0000", "value": "11" },
        { "date": "2026-02-24T10:08:11+0000", "value": "11" },
        { "date": "2026-02-24T10:21:20+0000", "value": "11" },
        { "date": "2026-02-24T10:22:29+0000", "value": "11" },
        { "date": "2026-02-24T11:55:19+0000", "value": "11" }
      ]
    }
  ]
}
```

---

## 📊 Response Structure Explanation

---

### 📈 Measures Array

Each object contains:

| Field   | Description                               |
| ------- | ----------------------------------------- |
| metric  | Metric name (`bugs` or `vulnerabilities`) |
| history | Array of date-value pairs                 |

---

### 🕒 History Object

| Field | Description               |
| ----- | ------------------------- |
| date  | Analysis timestamp        |
| value | Metric value at that time |

---

## 📊 Trend Analysis

### 🐞 Bugs History

| Date  | Bugs |
| ----- | ---- |
| 10:02 | 29   |
| 10:08 | 29   |
| 10:21 | 29   |
| 10:22 | 29   |
| 11:55 | 29   |

### 🔐 Vulnerabilities History

| Date  | Vulnerabilities |
| ----- | --------------- |
| 10:02 | 11              |
| 10:08 | 11              |
| 10:21 | 11              |
| 10:22 | 11              |
| 11:55 | 11              |

📌 Observation:

* Bugs constant at **29**
* Vulnerabilities constant at **11**
* No increase or decrease across analyses
* Indicates stable but high technical risk

---

## 📊 Suggested Bar Chart Logic (Frontend)

For simple comparison (latest value):

```
Bugs = 29
Vulnerabilities = 11
```

Bar Chart:

* X-Axis → Metric Type
* Y-Axis → Count
* Bars → Bugs vs Vulnerabilities

---

## 🧠 Backend Usage Logic (For BugPredictionBackend)

You should:

1. Extract latest value from each metric
2. Store time-series data
3. Calculate:

   * Delta change between analyses
   * Growth rate
4. Use as ML features:

   * bugs_t0, bugs_t1, bugs_t2
   * vulnerabilities_t0, vulnerabilities_t1
5. Flag project if:

   * bugs > threshold
   * vulnerabilities > threshold
   * sudden spike detected

---

## 🔗 API Dependency Flow

Depends on:

```
/api/projects/search → provides project key
```
---
Perfect 👍
Ab main tumhe **Module Metrics Table** ke liye proper Markdown format de raha hoon — bilkul waise hi jaise tumne pehle APIs ke liye banaya tha.

---

# 🔍 10.Module Metrics Table API

## 🔹 Endpoint

```http
GET https://sonarcloud.io/api/measures/component_tree
```

---

## 🔹 Query Parameters

| Parameter    | Description         | Example                                                                                                  |
| ------------ | ------------------- | -------------------------------------------------------------------------------------------------------- |
| `component`  | Project Key         | `BPCQ_axios-sonar`                                                                                       |
| `metricKeys` | Metrics to retrieve | `bugs,vulnerabilities,coverage,duplicated_lines_density,reliability_rating,security_rating,sqale_rating` |

---

## 🔹 Full API Call

```http
GET https://sonarcloud.io/api/measures/component_tree?component=BPCQ_axios-sonar&metricKeys=bugs,vulnerabilities,coverage,duplicated_lines_density,reliability_rating,security_rating,sqale_rating
```

---

# 📌 Purpose

This API retrieves:

* Project level metrics (baseComponent)
* Module / Directory level metrics
* File level metrics

Used for:

✅ Module Metrics Table
✅ Detailed Quality Breakdown
✅ Directory-wise Issue Distribution

---

# 📌 Important Response Fields

## 🔹 Project Level (baseComponent)

| Metric                                | Value |
| ------------------------------------- | ----- |
| Bugs                                  | 29    |
| Vulnerabilities                       | 11    |
| Duplicated Lines Density              | 3.7%  |
| Reliability Rating                    | 5.0   |
| Security Rating                       | 5.0   |
| Maintainability Rating (sqale_rating) | 1.0   |

---

## 🔹 Module Level (Example Entries)

### 📁 examples (Directory)

| Metric             | Value |
| ------------------ | ----- |
| Bugs               | 8     |
| Vulnerabilities    | 0     |
| Reliability Rating | 3.0   |
| Security Rating    | 1.0   |

---

### 📁 test/unit/adapters (Directory)

| Metric             | Value |
| ------------------ | ----- |
| Bugs               | 1     |
| Vulnerabilities    | 3     |
| Duplicated Lines   | 20.7% |
| Reliability Rating | 4.0   |
| Security Rating    | 5.0   |

---

### 📁 test/specs/cancel (Directory)

| Metric             | Value |
| ------------------ | ----- |
| Bugs               | 2     |
| Reliability Rating | 3.0   |
| Vulnerabilities    | 0     |

---

Bilkul 👍 samajh gaya.
Ab main **sirf API documentation format** me de raha hoon — bilkul same style me jaise 9th wale API ke liye diya tha.
No Angular. No extra code. Only API doc.

---

# 🔍 11.QUALITY GATES PAGE

## 🔹 Endpoint

```
GET https://sonarcloud.io/api/qualitygates/project_status
```

---

## 🔹 Query Parameters

| Parameter  | Description        | Example          |
| ---------- | ------------------ | ---------------- |
| projectKey | Unique project key | BPCQ_axios-sonar |

---

## 🔹 Full API Call

```
GET https://sonarcloud.io/api/qualitygates/project_status?projectKey=BPCQ_axios-sonar
```

---

# 📌 Purpose

This API returns the Quality Gate evaluation result for a project.

It provides:

* Overall Quality Gate status (OK / ERROR)
* Condition-wise evaluation
* New code metrics validation
* Period information

Used for:

* Quality Gates Page
* Build Validation
* Release Approval Decision
* New Code Health Monitoring

---

# 📊 Response Summary

### 🔹 Overall Project Status

| Field             | Value |
| ----------------- | ----- |
| status            | ERROR |
| ignoredConditions | false |

👉 `status = ERROR` means Quality Gate Failed

---

# 📌 Conditions Breakdown

| Metric                         | Status | Threshold | Actual Value | Comparator |
| ------------------------------ | ------ | --------- | ------------ | ---------- |
| new_reliability_rating         | OK     | 1         | 1            | GT         |
| new_security_rating            | OK     | 1         | 1            | GT         |
| new_maintainability_rating     | OK     | 1         | 1            | GT         |
| new_duplicated_lines_density   | OK     | 3         | 0.0          | GT         |
| new_security_hotspots_reviewed | ERROR  | 100       | 0.0          | LT         |

---

# 🚨 Failure Reason

Quality Gate failed because:

* new_security_hotspots_reviewed = 0.0%
* Required Threshold = 100%
* Condition not satisfied

---

# 📅 Evaluation Period

| Field     | Value                    |
| --------- | ------------------------ |
| mode      | days                     |
| parameter | 1                        |
| date      | 2026-02-24T10:02:32+0000 |

Meaning: Evaluation based on New Code (last 1 day).

---

Perfect 👍
Same format me de raha hoon — **sirf API documentation**, no extra code.

---

# 12. GATE HISTORY

## 🔹 Endpoint

```
GET https://sonarcloud.io/api/project_analyses/search
```

---

## 🔹 Query Parameters

| Parameter | Description        | Example          |
| --------- | ------------------ | ---------------- |
| project   | Unique project key | BPCQ_axios-sonar |

---

## 🔹 Full API Call

```
GET https://sonarcloud.io/api/project_analyses/search?project=BPCQ_axios-sonar
```

---

# 📌 Purpose

This API returns the analysis history of a project.

It provides:

* Past analysis runs
* Quality Gate change events
* Version events
* Commit revision details
* Analysis timestamps

Used for:

* Gate History Page
* Quality Gate trend tracking
* CI/CD audit history
* Version tracking

---

# 📊 Response Summary

### 🔹 Paging Information

| Field     | Value |
| --------- | ----- |
| pageIndex | 1     |
| pageSize  | 100   |
| total     | 5     |

---

# 📌 Analysis History Table

| Date                | Analysis Key  | Revision | Events       |
| ------------------- | ------------- | -------- | ------------ |
| 2026-02-24T11:55:19 | 0fe5683a-99f1 | 31b1865  | VERSION      |
| 2026-02-24T10:22:29 | be605756-6a4d | 31b1865  | None         |
| 2026-02-24T10:21:20 | a765b091-6458 | 5ef312a  | QUALITY_GATE |
| 2026-02-24T10:08:11 | 6da32029-0b2a | 78c9889  | None         |
| 2026-02-24T10:02:32 | 650046e1-e1c7 | 1ebb21e  | None         |

---

# 📌 Event Types Found

### 🔹 VERSION Event

Indicates a version was recorded during analysis.

Example:

```
category: VERSION
name: not provided
```

---

### 🔹 QUALITY_GATE Event

Indicates a Quality Gate status change.

Example:

```
category: QUALITY_GATE
name: Red (was Green)
description: Security Hotspots Reviewed on New Code < 100
```

Meaning:

* Quality Gate changed from Green → Red
* Failure due to Security Hotspots not reviewed

---

# 📌 Important Fields

| Field                       | Meaning                             |
| --------------------------- | ----------------------------------- |
| key                         | Unique analysis ID                  |
| date                        | Analysis timestamp                  |
| revision                    | Git commit hash                     |
| projectVersion              | Project version at analysis         |
| manualNewCodePeriodBaseline | Whether new code baseline is manual |
| events                      | Gate / Version change events        |

---
