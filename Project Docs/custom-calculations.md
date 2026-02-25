# 📘 CUSTOM CALCULATION DESIGN

---

# 1️⃣ Unified Project Risk Score (Core Metric)

## 🎯 Purpose

Convert multiple Sonar metrics into a single normalized risk score (0–100).

## 📥 Input Metrics (From DB Snapshot)

| Metric          | Source             |
| --------------- | ------------------ |
| Bugs            | measures/component |
| Vulnerabilities | measures/component |
| Code Smells     | measures/component |
| Coverage (%)    | measures/component |
| Duplication (%) | measures/component |

---

## 🧮 Formula (Weighted Model)

```id="formula01"
RiskScore =
(Bugs × 4)
+ (Vulnerabilities × 7)
+ (CodeSmells × 1)
+ (100 − Coverage)
+ (Duplication% × 2)
```

---

## 🔒 Normalization

```csharp id="norm01"
riskScore = Math.Min(riskScore, 100);
```

---

## 📊 Risk Classification

| Score Range | Risk Level |
| ----------- | ---------- |
| 0 – 30      | LOW        |
| 31 – 70     | MEDIUM     |
| 71 – 100    | HIGH       |

---

## 💡 Why This Works

* Vulnerabilities weighted higher (security risk)
* Coverage inversely affects risk
* Duplication impacts maintainability

---

# 2️⃣ Module Risk Score (Granular Risk)

## 🎯 Purpose

Identify high-risk modules inside a project.

## 📥 Input (Per Module)

* Bugs
* Vulnerabilities
* Coverage
* Duplication%
* Complexity (optional advanced)

---

## 🧮 Formula

```id="formula02"
ModuleRisk =
(Bugs × 5)
+ (Vulnerabilities × 8)
+ (100 − Coverage)
+ (Duplication% × 3)
```

Optional add:

```id="formula03"
+ (Complexity / 10)
```

---

## 🎯 Usage

* High Risk Modules Table
* Risk Distribution Bar Chart
* Sorting modules by severity

---

# 3️⃣ Severity Weighted Issue Score

## 🎯 Purpose

Make issue severity meaningful (not just counts).

## 📥 Input

From:
`/api/issues/search?facets=severities`

| Severity | Weight |
| -------- | ------ |
| BLOCKER  | 10     |
| CRITICAL | 7      |
| MAJOR    | 5      |
| MINOR    | 2      |
| INFO     | 1      |

---

## 🧮 Formula

```id="formula04"
SeverityScore =
(Blocker × 10)
+ (Critical × 7)
+ (Major × 5)
+ (Minor × 2)
+ (Info × 1)
```

---

## 🎯 Benefit

Gives more accurate risk representation than total issue count.

---

# 4️⃣ Coverage Trend Delta

## 🎯 Purpose

Show improvement or degradation between scans.

## 📥 Input

Previous snapshot coverage
Current snapshot coverage

---

## 🧮 Formula

```id="formula05"
CoverageDelta = CurrentCoverage − PreviousCoverage
```

---

## 📊 Interpretation

| Result | Meaning   |
| ------ | --------- |
| > 0    | Improving |
| < 0    | Declining |
| = 0    | Stable    |

---

# 5️⃣ Risk Trend Indicator

## 🎯 Purpose

Project stability measurement.

## 📥 Input

RiskScore (Last 3 scans)

---

## 🧮 Formula

```id="formula06"
RiskDelta = CurrentRisk − PreviousRisk
```

---

## 📊 Interpretation

| Result | Status    |
| ------ | --------- |
| < 0    | Improving |
| > 0    | Degrading |
| Stable | No Change |

---

# 6️⃣ Issue Density (Professional Metric)

## 🎯 Purpose

Normalize issue count by project size.

## 📥 Input

* Bugs
* Lines of Code (ncloc)

---

## 🧮 Formula

```id="formula07"
BugDensity = Bugs / (LinesOfCode / 1000)
```

Example:
20 bugs, 10,000 LOC

```id="formula08"
20 / 10 = 2 bugs per 1K LOC
```

---

# 7️⃣ Quality Gate Failure Extraction

## 🎯 Purpose

Show exact reason for failure.

## 📥 Input

From:
`qualitygates/project_status`

---

## 🧮 Logic

```csharp id="logic01"
var failedConditions = conditions
    .Where(c => c.Status == "ERROR")
    .Select(c => c.MetricKey);
```

---

# 8️⃣ Risk Contribution Percentage

## 🎯 Purpose

Show which metric contributes most to risk.

## 🧮 Formula

```id="formula09"
MetricContribution% =
(MetricScore / TotalRiskScore) × 100
```

---

# 9️⃣ Project Health Score (Optional Branding Metric)

## 🎯 Purpose

Positive scoring representation.

## 🧮 Formula

```id="formula10"
HealthScore = 100 − RiskScore
```

---

## 📊 Classification

| Health | Meaning   |
| ------ | --------- |
| 80+    | Excellent |
| 60–79  | Good      |
| 40–59  | Moderate  |
| < 40   | Critical  |

---

# 🔟 Risk Stability Index (Advanced)

## 🎯 Purpose

Measure volatility over last N scans.

## 🧮 Formula

Standard deviation of last 5 RiskScores.

Lower deviation = Stable project
Higher deviation = Unstable codebase

---

# 🏗 Implementation Structure (.NET)

Create dedicated class:

```csharp id="impl01"
public class RiskCalculator
{
    public int CalculateProjectRisk(ProjectMetrics m) { }
    public int CalculateModuleRisk(ModuleMetrics m) { }
    public int CalculateSeverityScore(SeverityMetrics s) { }
    public int CalculateTrend(int current, int previous) { }
}
```

Keep business logic isolated.

---

# 🧠 Enterprise Recommendation

## Mandatory

* Unified Risk Score
* Module Risk
* Severity Weighted Score
* Trend Delta
* Failure Extraction

## Advanced (Product-Level Analytics)

* Issue Density
* Risk Contribution
* Stability Index
* Health Score

---

# 🎯 Final Perspective

SonarCloud gives:
📦 Raw technical metrics

Your backend provides:
🧠 Business intelligence
📊 Risk interpretation
📈 Trend awareness
🚨 Actionable insights

That transformation layer is what differentiates a dashboard from an analytics platform.
