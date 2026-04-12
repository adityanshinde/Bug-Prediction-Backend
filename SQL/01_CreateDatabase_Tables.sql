-- =========================
-- PROJECTS
-- =========================
CREATE TABLE dbo.projects (
    id SERIAL PRIMARY KEY,
    projectkey VARCHAR(200) UNIQUE NOT NULL,
    name VARCHAR(300) NOT NULL,
    organization VARCHAR(200),
    visibility VARCHAR(50),
    lastanalysisdate TIMESTAMP,
    createddate TIMESTAMP DEFAULT NOW() NOT NULL
);

-- =========================
-- BRANCHES
-- =========================
CREATE TABLE dbo.branches (
    id SERIAL PRIMARY KEY,
    projectid INT NOT NULL,
    branchname VARCHAR(200) NOT NULL,
    ismain BOOLEAN DEFAULT FALSE NOT NULL,
    analysisdate TIMESTAMP,
    CONSTRAINT fk_branches_projects FOREIGN KEY (projectid)
        REFERENCES dbo.projects(id)
);

CREATE INDEX ix_branches_projectid ON dbo.branches(projectid);

-- =========================
-- SNAPSHOTS
-- =========================
CREATE TABLE dbo.snapshots (
    id SERIAL PRIMARY KEY,
    projectid INT NOT NULL,
    branchid INT,
    scandate TIMESTAMP NOT NULL,
    commitid VARCHAR(200),
    bugs INT DEFAULT 0,
    vulnerabilities INT DEFAULT 0,
    codesmells INT DEFAULT 0,
    coverage NUMERIC(5,2) DEFAULT 0,
    duplication NUMERIC(5,2) DEFAULT 0,
    securityrating VARCHAR(10),
    reliabilityrating VARCHAR(10),
    maintainabilityrating VARCHAR(10),
    qualitygatestatus VARCHAR(50),
    createddate TIMESTAMP DEFAULT NOW() NOT NULL,
    CONSTRAINT fk_snapshots_projects FOREIGN KEY (projectid)
        REFERENCES dbo.projects(id),
    CONSTRAINT fk_snapshots_branches FOREIGN KEY (branchid)
        REFERENCES dbo.branches(id)
);

CREATE INDEX ix_snapshots_projectid ON dbo.snapshots(projectid);
CREATE INDEX ix_snapshots_branchid ON dbo.snapshots(branchid);
CREATE INDEX ix_snapshots_scandate ON dbo.snapshots(scandate);

-- =========================
-- MODULE METRICS
-- =========================
CREATE TABLE dbo.modulemetrics (
    id SERIAL PRIMARY KEY,
    snapshotid INT NOT NULL,
    modulename VARCHAR(500),
    qualifier VARCHAR(10),
    path VARCHAR(1000),
    language VARCHAR(50),
    bugs INT DEFAULT 0,
    vulnerabilities INT DEFAULT 0,
    codesmells INT DEFAULT 0,
    coverage NUMERIC(5,2) DEFAULT 0,
    duplication NUMERIC(5,2) DEFAULT 0,
    complexity INT DEFAULT 0,
    ncloc INT DEFAULT 0,
    CONSTRAINT fk_modulemetrics_snapshots FOREIGN KEY (snapshotid)
        REFERENCES dbo.snapshots(id)
);

CREATE INDEX ix_modulemetrics_snapshotid ON dbo.modulemetrics(snapshotid);

-- =========================
-- QUALITY GATE CONDITIONS
-- =========================
CREATE TABLE dbo.qualitygateconditions (
    id SERIAL PRIMARY KEY,
    snapshotid INT NOT NULL,
    metrickey VARCHAR(100) NOT NULL,
    comparator VARCHAR(10),
    errorthreshold VARCHAR(50),
    actualvalue VARCHAR(50),
    status VARCHAR(10) NOT NULL,
    CONSTRAINT fk_qgconditions_snapshots FOREIGN KEY (snapshotid)
        REFERENCES dbo.snapshots(id)
);

CREATE INDEX ix_qgconditions_snapshotid ON dbo.qualitygateconditions(snapshotid);

-- =========================
-- SEVERITY DISTRIBUTION
-- =========================
CREATE TABLE dbo.severitydistribution (
    id SERIAL PRIMARY KEY,
    snapshotid INT NOT NULL,
    blocker INT DEFAULT 0,
    critical INT DEFAULT 0,
    major INT DEFAULT 0,
    minor INT DEFAULT 0,
    info INT DEFAULT 0,
    CONSTRAINT fk_severity_snapshots FOREIGN KEY (snapshotid)
        REFERENCES dbo.snapshots(id)
);

CREATE INDEX ix_severity_snapshotid ON dbo.severitydistribution(snapshotid);

-- =========================
-- MANUAL QA ENTRIES
-- =========================
CREATE TABLE dbo.manualqaentries (
    id SERIAL PRIMARY KEY,
    projectid INT NOT NULL,
    modulename VARCHAR(500) NOT NULL,
    issuetype VARCHAR(100) NOT NULL,
    severity VARCHAR(50) NOT NULL,
    description VARCHAR(2000),
    reportedby VARCHAR(200),
    entrydate TIMESTAMP DEFAULT NOW() NOT NULL,
    CONSTRAINT fk_manualqa_projects FOREIGN KEY (projectid)
        REFERENCES dbo.projects(id)
);

CREATE INDEX ix_manualqa_projectid ON dbo.manualqaentries(projectid);