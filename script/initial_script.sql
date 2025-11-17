-- ============================================================
-- Ruleset Evaluation Engine - Database Schema
-- ============================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'RulesetEngine')
BEGIN
    CREATE DATABASE [RulesetEngine];
    PRINT 'Database [RulesetEngine] created successfully.';
END
GO

-- Switch to the newly created database
USE [RulesetEngine];
GO

-- Drop tables if they exist (for clean setup)
IF OBJECT_ID('EvaluationLogs', 'U') IS NOT NULL DROP TABLE EvaluationLogs;
IF OBJECT_ID('Conditions', 'U') IS NOT NULL DROP TABLE Conditions;
IF OBJECT_ID('Rules', 'U') IS NOT NULL DROP TABLE Rules;
IF OBJECT_ID('Rulesets', 'U') IS NOT NULL DROP TABLE Rulesets;
GO

-- ============================================================
-- Table: Rulesets
-- ============================================================
CREATE TABLE Rulesets (
    RulesetId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Priority INT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Ruleset_Name UNIQUE (Name)
);

CREATE INDEX IX_Rulesets_IsActive_Priority ON Rulesets(IsActive, Priority);
GO

-- ============================================================
-- Table: Rules
-- ============================================================
CREATE TABLE Rules (
    RuleId INT PRIMARY KEY IDENTITY(1,1),
    RulesetId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    ResultProductionPlant NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    SequenceOrder INT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Rules_Rulesets FOREIGN KEY (RulesetId) 
        REFERENCES Rulesets(RulesetId) ON DELETE CASCADE
);

CREATE INDEX IX_Rules_RulesetId_SequenceOrder ON Rules(RulesetId, SequenceOrder);
GO

-- ============================================================
-- Table: Conditions
-- ============================================================
CREATE TABLE Conditions (
    ConditionId INT PRIMARY KEY IDENTITY(1,1),
    RulesetId INT NULL,
    RuleId INT NULL,
    Field NVARCHAR(100) NOT NULL,
    Operator NVARCHAR(50) NOT NULL,
    Value NVARCHAR(500) NOT NULL,
    SequenceOrder INT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Conditions_Rulesets FOREIGN KEY (RulesetId) 
        REFERENCES Rulesets(RulesetId) ON DELETE CASCADE,
    CONSTRAINT FK_Conditions_Rules FOREIGN KEY (RuleId) 
        REFERENCES Rules(RuleId) ON DELETE NO ACTION,
    CONSTRAINT CK_Conditions_Parent CHECK (
        (RulesetId IS NOT NULL AND RuleId IS NULL) OR 
        (RulesetId IS NULL AND RuleId IS NOT NULL)
    )
);

CREATE INDEX IX_Conditions_RulesetId ON Conditions(RulesetId) WHERE RulesetId IS NOT NULL;
CREATE INDEX IX_Conditions_RuleId ON Conditions(RuleId) WHERE RuleId IS NOT NULL;
GO

-- ============================================================
-- Table: EvaluationLogs
-- ============================================================
CREATE TABLE EvaluationLogs (
    LogId BIGINT PRIMARY KEY IDENTITY(1,1),
    OrderId NVARCHAR(50) NOT NULL,
    PublisherNumber NVARCHAR(50) NULL,
    OrderMethod NVARCHAR(50) NULL,
    EvaluationDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    MatchedRulesetId INT NULL,
    MatchedRulesetName NVARCHAR(200) NULL,
    MatchedRuleId INT NULL,
    MatchedRuleName NVARCHAR(200) NULL,
    ProductionPlant NVARCHAR(50) NULL,
    IsMatched BIT NOT NULL,
    Reason NVARCHAR(MAX) NULL,
    OrderJson NVARCHAR(MAX) NULL,
    EvaluationTimeMs INT NULL,
    CONSTRAINT FK_EvaluationLogs_Rulesets FOREIGN KEY (MatchedRulesetId) 
        REFERENCES Rulesets(RulesetId) ON DELETE SET NULL,
    CONSTRAINT FK_EvaluationLogs_Rules FOREIGN KEY (MatchedRuleId) 
        REFERENCES Rules(RuleId) ON DELETE NO ACTION
);

CREATE INDEX IX_EvaluationLogs_OrderId ON EvaluationLogs(OrderId);
CREATE INDEX IX_EvaluationLogs_EvaluationDate ON EvaluationLogs(EvaluationDate DESC);
CREATE INDEX IX_EvaluationLogs_PublisherNumber ON EvaluationLogs(PublisherNumber);
GO

-- ============================================================
-- Seed Data
-- ============================================================

-- Insert Ruleset One
INSERT INTO Rulesets (Name, Description, IsActive, Priority)
VALUES ('Ruleset One', 'Rules for Publisher 99990', 1, 1);

DECLARE @RulesetOne INT = SCOPE_IDENTITY();

-- Ruleset One Conditions
INSERT INTO Conditions (RulesetId, Field, Operator, Value, SequenceOrder)
VALUES 
    (@RulesetOne, 'PublisherNumber', 'Equals', '99990', 1),
    (@RulesetOne, 'OrderMethod', 'Equals', 'POD', 2);

-- Ruleset One - Rule 1
INSERT INTO Rules (RulesetId, Name, ResultProductionPlant, IsActive, SequenceOrder)
VALUES (@RulesetOne, 'Rule 1', 'US', 1, 1);

DECLARE @Rule1 INT = SCOPE_IDENTITY();

INSERT INTO Conditions (RuleId, Field, Operator, Value, SequenceOrder)
VALUES 
    (@Rule1, 'BindTypeCode', 'Equals', 'PB', 1),
    (@Rule1, 'IsCountry', 'Equals', 'US', 2),
    (@Rule1, 'PrintQuantity', 'LessThanOrEqual', '20', 3);

-- Insert Ruleset Two
INSERT INTO Rulesets (Name, Description, IsActive, Priority)
VALUES ('Ruleset Two', 'Rules for Publisher 99999', 1, 2);

DECLARE @RulesetTwo INT = SCOPE_IDENTITY();

-- Ruleset Two Conditions
INSERT INTO Conditions (RulesetId, Field, Operator, Value, SequenceOrder)
VALUES 
    (@RulesetTwo, 'PublisherNumber', 'Equals', '99999', 1),
    (@RulesetTwo, 'OrderMethod', 'Equals', 'POD', 2);

-- Ruleset Two - Rule 2
INSERT INTO Rules (RulesetId, Name, ResultProductionPlant, IsActive, SequenceOrder)
VALUES (@RulesetTwo, 'Rule 2', 'UK', 1, 1);

DECLARE @Rule2 INT = SCOPE_IDENTITY();

INSERT INTO Conditions (RuleId, Field, Operator, Value, SequenceOrder)
VALUES 
    (@Rule2, 'BindTypeCode', 'Equals', 'CV', 1),
    (@Rule2, 'IsCountry', 'Equals', 'UK', 2),
    (@Rule2, 'PrintQuantity', 'LessThanOrEqual', '20', 3);

-- Ruleset Two - Rule 3
INSERT INTO Rules (RulesetId, Name, ResultProductionPlant, IsActive, SequenceOrder)
VALUES (@RulesetTwo, 'Rule 3', 'KGL', 1, 2);

DECLARE @Rule3 INT = SCOPE_IDENTITY();

INSERT INTO Conditions (RuleId, Field, Operator, Value, SequenceOrder)
VALUES 
    (@Rule3, 'BindTypeCode', 'Equals', 'PB', 1),
    (@Rule3, 'IsCountry', 'Equals', 'US', 2),
    (@Rule3, 'PrintQuantity', 'GreaterThanOrEqual', '20', 3);
GO

-- ============================================================
-- Verification Queries
-- ============================================================

-- View all rulesets with their conditions
SELECT 
    r.Name AS RulesetName,
    c.Field,
    c.Operator,
    c.Value
FROM Rulesets r
LEFT JOIN Conditions c ON r.RulesetId = c.RulesetId
ORDER BY r.Priority, c.SequenceOrder;

-- View all rules with their conditions
SELECT 
    rs.Name AS RulesetName,
    r.Name AS RuleName,
    r.ResultProductionPlant,
    c.Field,
    c.Operator,
    c.Value
FROM Rules r
INNER JOIN Rulesets rs ON r.RulesetId = rs.RulesetId
LEFT JOIN Conditions c ON r.RuleId = c.RuleId
ORDER BY rs.Priority, r.SequenceOrder, c.SequenceOrder;