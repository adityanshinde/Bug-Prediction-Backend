# FRONTEND API GUIDE

⚠️ Angular should NEVER care about SonarCloud structure.
Angular only talks to your backend.

---

# 🏗 Architecture Now

```id="arch01"
Angular
   ↓
.NET API (Dashboard APIs)
   ↓
SQL Database
```

No Sonar calls here. Only DB reads.

---

# 🎯 API Design Based On Your UI Pages

I’ll design this page-wise.

---

# 1️⃣ HEADER SECTION API

### UI Needs:

* Project Name
* Branch
* Last Scan
* Quality Gate Status

## Endpoint

```id="hdr01"
GET /api/projects/{projectId}/header
```

## Response DTO

```json
{
  "projectName": "CoreAppAPI",
  "branch": "main",
  "lastScanDate": "2026-02-23T12:30:00",
  "qualityGateStatus": "PASS"
}
```

---

# 2️⃣ DASHBOARD PAGE API

### UI Needs:

* KPIs
* Donut distribution
* Overall Risk
* Recent Scan table

## Recommended: Single Aggregated API

```id="dash01"
GET /api/projects/{projectId}/dashboard
```

## Response

```json
{
  "kpis": {
    "bugs": 15,
    "vulnerabilities": 3,
    "riskScore": 62,
    "qualityGate": "PASS"
  },
  "issueDistribution": {
    "bugs": 15,
    "vulnerabilities": 3,
    "codeSmells": 120
  },
  "severityDistribution": {
    "blocker": 1,
    "critical": 2,
    "major": 5,
    "minor": 10,
    "info": 20
  },
  "overallRisk": "MEDIUM",
  "recentScans": [
    {
      "scanDate": "2026-02-23",
      "branch": "main",
      "commit": "a8f9c12",
      "qualityGate": "PASS"
    }
  ]
}
```

⚡ Why single API?
Reduces 5 HTTP calls from Angular → better performance.

---

# 3️⃣ PROJECTS PAGE API

## Projects List

```id="proj01"
GET /api/projects
```

Response:

```json
[
  {
    "projectId": 1,
    "name": "CoreAppAPI",
    "lastScanDate": "2026-02-23"
  }
]
```

---

## Scan History (Project Wise)

```id="proj02"
GET /api/projects/{projectId}/scan-history
```

Response:

```json
[
  {
    "scanDate": "2026-02-23",
    "branch": "main",
    "commit": "a8f9c12",
    "gateStatus": "PASS"
  }
]
```

---

# 4️⃣ RISK ANALYSIS PAGE

## Endpoint

```id="risk01"
GET /api/projects/{projectId}/risk-analysis
```

Response:

```json
{
  "overallRiskScore": 62,
  "riskLevel": "MEDIUM",
  "moduleDistribution": [
    {
      "moduleName": "AuthModule",
      "riskScore": 80
    }
  ],
  "highRiskModules": [
    {
      "moduleName": "PaymentModule",
      "bugs": 5,
      "vulnerabilities": 2,
      "coverage": 45,
      "duplication": 20,
      "riskScore": 88,
      "riskLevel": "HIGH"
    }
  ]
}
```

---

# 5️⃣ METRICS PAGE

## Endpoint

```id="met01"
GET /api/projects/{projectId}/metrics
```

Response:

```json
{
  "kpis": {
    "bugs": 15,
    "codeSmells": 120,
    "coverage": 78,
    "duplication": 6
  },
  "coverageTrend": [
    { "date": "2026-02-01", "coverage": 72 },
    { "date": "2026-02-15", "coverage": 75 },
    { "date": "2026-02-23", "coverage": 78 }
  ],
  "bugsVsVulnerabilities": [
    { "date": "2026-02-23", "bugs": 15, "vulnerabilities": 3 }
  ],
  "moduleMetrics": [
    {
      "moduleName": "AuthModule",
      "bugs": 3,
      "codeSmells": 12,
      "coverage": 80,
      "duplication": 5,
      "complexity": 120,
      "linesOfCode": 1500
    }
  ]
}
```

---

# 6️⃣ QUALITY GATES PAGE

```id="qg01"
GET /api/projects/{projectId}/quality-gates
```

Response:

```json
{
  "currentStatus": "PASS",
  "gateConditions": [
    {
      "metric": "coverage",
      "condition": "> 70%",
      "status": "PASS"
    }
  ],
  "history": [
    {
      "date": "2026-02-23",
      "branch": "main",
      "status": "PASS",
      "failedRule": null
    }
  ]
}
```

---

# 🧠 Professional API Design Advice

### ❌ Don’t expose DB tables directly

### ❌ Don’t return raw snapshot rows

### ✅ Always create page-specific DTOs

### ✅ Aggregate and shape data in backend

Backend = Data shaping layer.

---

# 🏛 Suggested Folder Structure (.NET)

```id="fs01"
Controllers/
    ProjectsController.cs
    DashboardController.cs
    RiskController.cs
    MetricsController.cs

Services/
    DashboardService.cs
    RiskService.cs
    MetricsService.cs

DTOs/
    DashboardDto.cs
    RiskDto.cs
    MetricsDto.cs
```

---

# ⚡ Performance Optimization

For dashboard API:

* Fetch latest snapshot only
* Use proper indexes on:

  * ProjectId
  * ScanDate
  * SnapshotId

---

# 🎯 Final Result

Angular will only call:

```id="final01"
/api/projects
/api/projects/{id}/dashboard
/api/projects/{id}/metrics
/api/projects/{id}/risk-analysis
/api/projects/{id}/quality-gates
/api/projects/{id}/scan-history
/api/sync/{projectKey}
```

Clean. Maintainable. Enterprise-level design 💼🔥
