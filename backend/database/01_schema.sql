-- ==============================================================================
-- LIRS TAXPAYER MINI-PORTAL: DATABASE SCHEMA DEFINITION (01_schema.sql)
-- ==============================================================================
-- Database: LirsPortal
-- Engine:   Microsoft SQL Server (SQLEXPRESS or Developer Edition)
-- Purpose:  Establishes relational tables, primary keys, foreign keys, and indexes.
-- ==============================================================================

-- Create the state portal database
CREATE DATABASE LirsPortal;
GO

USE LirsPortal;
GO

-- ------------------------------------------------------------------------------
-- Table 1: Taxpayers
-- Represents registered citizens and corporate entities in the state tax system.
-- ------------------------------------------------------------------------------
CREATE TABLE Taxpayers (
    TaxpayerId INT PRIMARY KEY,               -- Unique integer identifier (e.g. 101, 102)
    TIN        VARCHAR(10)  NOT NULL UNIQUE,  -- 10-digit Taxpayer Identification Number
    Name       VARCHAR(150) NOT NULL,         -- Full legal citizen or business name
    Type       VARCHAR(20)  NOT NULL,         -- 'Individual' or 'Business'
    State      VARCHAR(50)  NOT NULL,         -- Tax jurisdiction (e.g. 'Lagos', 'Ogun')
    Phone      VARCHAR(20)                    -- Contact phone number
);

-- ------------------------------------------------------------------------------
-- Table 2: TaxReturns
-- Represents annual tax assessments submitted by a taxpayer.
-- Linked to Taxpayers via a Foreign Key constraint.
-- ------------------------------------------------------------------------------
CREATE TABLE TaxReturns (
    ReturnId       INT PRIMARY KEY,
    TaxpayerId     INT NOT NULL REFERENCES Taxpayers (TaxpayerId),
    TaxYear        INT NOT NULL,              -- Assessment year (e.g. 2025, 2026)
    DeclaredIncome DECIMAL(18,2) NOT NULL,    -- Gross income declared by the taxpayer
    TaxDue         DECIMAL(18,2) NOT NULL,    -- Statutory tax legally assessed
    Status         VARCHAR(20) NOT NULL       -- Lifecycle: 'Submitted', 'Approved', 'Draft'
);

-- ------------------------------------------------------------------------------
-- Table 3: Payments
-- Records citizen tax payments made against filed returns.
-- Uses an auto-incrementing IDENTITY column for atomic sequential payment IDs.
-- ------------------------------------------------------------------------------
CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,  -- Sequential payment receipt identifier
    ReturnId  INT NOT NULL REFERENCES TaxReturns (ReturnId),
    Amount    DECIMAL(18,2) NOT NULL,         -- Payment amount in Nigerian Naira
    PaidOn    DATETIME2 NOT NULL,             -- Server timestamp when payment occurred
    Channel   VARCHAR(20) NOT NULL            -- Channel: 'Bank', 'Card', 'USSD'
);

-- ------------------------------------------------------------------------------
-- Table 4: ComplianceLogs
-- Append-only audit trail recording regulatory and operational state events.
-- Supports non-repudiation and government compliance audits (NDPR).
-- ------------------------------------------------------------------------------
CREATE TABLE ComplianceLogs (
    LogId       INT IDENTITY(1,1) PRIMARY KEY,
    TaxpayerId  INT NOT NULL REFERENCES Taxpayers (TaxpayerId),
    Event       VARCHAR(200) NOT NULL,        -- Description of the audit event
    OfficerName VARCHAR(100),                 -- Officer responsible for the action
    LoggedAt    DATETIME2 NOT NULL            -- Timestamp of the event
);

-- ------------------------------------------------------------------------------
-- Performance Optimization Indexes:
-- Foreign keys are not automatically indexed by SQL Server. Adding these explicit
-- non-clustered indexes accelerates JOINs and foreign key lookups under load.
-- ------------------------------------------------------------------------------
CREATE INDEX IX_TaxReturns_TaxpayerId ON TaxReturns (TaxpayerId);
CREATE INDEX IX_Payments_ReturnId     ON Payments (ReturnId);
GO
