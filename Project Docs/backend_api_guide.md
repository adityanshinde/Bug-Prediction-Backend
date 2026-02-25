# BACKEND API GUIDE
# 🔐 Authentication (Common for All APIs)

All calls:

```
https://sonarcloud.io/api/*
```

Use:

```
Authorization: Bearer {SONAR_TOKEN}
```

---

# 1️⃣ HEADER SECTION

**Project Name, Branch, Last Scan, Status**

## Required APIs

### 🟢 Get Projects

```
GET /api/projects/search?organization=your-org
```

Returns:

* key
* name
* visibility
* lastAnalysisDate

---

### 🟢 Get Branch List

```
GET /api/project_branches/list?project=project_key
```

Returns:

* branch name
* isMain
* analysisDate

---

### 🟢 Get Quality Gate Status

```
GET /api/qualitygates/project_status?projectKey=project_key
```

Returns:

* status: OK / ERROR

Map:

* OK → Pass
* ERROR → Fail

---

# 2️⃣ DASHBOARD KPIs

You need aggregated metrics.

## 🟢 Core Metrics API

```
GET /api/measures/component
```

Example:

```
/api/measures/component
?component=project_key
&metricKeys=bugs,vulnerabilities,code_smells,coverage,duplicated_lines_density,security_rating,reliability_rating,sqale_rating
```

This gives:

| Metric                   | Use                   |
| ------------------------ | --------------------- |
| bugs                     | Total Bugs            |
| vulnerabilities          | Total Vulnerabilities |
| code_smells              | For donut             |
| coverage                 | Coverage %            |
| duplicated_lines_density | Duplication %         |
| security_rating          | Risk scoring          |
| reliability_rating       | Stability             |

---

## 🟢 Severity Distribution (Donut Chart)

```
GET /api/issues/search
?componentKeys=project_key
&severities=BLOCKER,CRITICAL,MAJOR,MINOR,INFO
&ps=1
&facets=severities,types
```

Use:

* facets.severities → distribution
* facets.types → BUG / VULNERABILITY / CODE_SMELL

---

## 🟢 Recent Scans Table

Use:

```
GET /api/project_analyses/search?project=project_key
```

Returns:

* date
* revision (commit id)
* events
* quality gate status

---

# 3️⃣ PROJECTS PAGE

Same APIs reused:

### Projects List

```
/api/projects/search
```

### Scan History (Project Wise)

```
/api/project_analyses/search?project=project_key
```

This is your full scan history.

---

# 4️⃣ RISK ANALYSIS PAGE

Now we get deeper.

## 🟢 Module-wise Metrics

```
GET /api/measures/component_tree
```

Example:

```
/api/measures/component_tree
?component=project_key
&metricKeys=bugs,vulnerabilities,coverage,duplicated_lines_density,complexity,ncloc
```

This returns module/file-level breakdown.

You use this for:

* Module Risk Distribution (Bar Chart)
* High Risk Modules Table

---

## 🧠 Risk Score Logic (You Must Calculate)

SonarCloud does NOT give “Risk Score 1–100”.
You calculate it.

Example formula:

```
RiskScore =
(bugs * 5)
+ (vulnerabilities * 8)
+ (100 - coverage)
+ (duplication * 2)
```

Then classify:

| Score | Risk |
| ----- | ---- |
| 0–30  | LOW  |
| 31–70 | MED  |
| 71+   | HIGH |

This logic lives in .NET backend.

---

# 5️⃣ METRICS PAGE

## Coverage Trend (Line Chart)

```
GET /api/measures/search_history
```

Example:

```
/api/measures/search_history
?component=project_key
&metrics=coverage
```

Returns historical data.

---

## Bugs vs Vulnerabilities (Bar Chart)

```
/api/measures/search_history
?component=project_key
&metrics=bugs,vulnerabilities
```

---

## Module Metrics Table

Use again:

```
/api/measures/component_tree
```

---

# 6️⃣ QUALITY GATES PAGE

## Gate Status

```
/api/qualitygates/project_status
```

## Gate History

Combine:

```
/api/project_analyses/search
```

Look at:

* events
* quality gate change events

---

# 🏗 Recommended Backend Design (.NET)

### Service Layer

```
ISonarService
    - GetProjects()
    - GetProjectMetrics(projectKey)
    - GetIssues(projectKey)
    - GetScanHistory(projectKey)
    - GetModuleMetrics(projectKey)
```

---

### Background Sync Job

Use:

* Hangfire OR
* IHostedService (Scheduled Task)

Flow:

1. Fetch all projects
2. Loop projects
3. Call APIs
4. Save to DB
5. Calculate RiskScore
6. Store Snapshot

---

# 🗄 Recommended DB Structure

## Projects

* Id
* ProjectKey
* Name
* LastScanDate

## Branches

* Id
* ProjectId
* BranchName

## Snapshots

* Id
* ProjectId
* BranchId
* ScanDate
* CommitId
* Bugs
* Vulnerabilities
* CodeSmells
* Coverage
* Duplication
* RiskScore
* RiskLevel
* QualityGateStatus

## ModuleMetrics

* SnapshotId
* ModuleName
* Bugs
* Vulnerabilities
* Coverage
* Duplication
* Complexity
* Ncloc
* RiskScore

---

# 🔥 Important Architecture Advice

Do NOT:

* Call SonarCloud every time frontend loads ❌
* Expose token to Angular ❌

Do:

* Sync periodically (every 6 hours or daily)
* Cache DB data
* Let Angular call only your APIs

---

# 📊 API Summary Table

| Feature             | Sonar API                   |
| ------------------- | --------------------------- |
| Projects            | projects/search             |
| Metrics             | measures/component          |
| Module Metrics      | measures/component_tree     |
| Issues Distribution | issues/search               |
| Scan History        | project_analyses/search     |
| Coverage Trend      | measures/search_history     |
| Gate Status         | qualitygates/project_status |

---

You are essentially building a **custom SonarCloud analytics platform** 💥
This is production-level architecture.

---

Now tell me:

1. Single organization or multiple org support?
2. Multi-user login required?
3. Real-time sync button or scheduled only?
4. SQL Server or PostgreSQL?

Next step I can:

* Design complete API contract
* Or write sample .NET controller
* Or draw architecture mapping table

This is turning into a serious product 🚀
