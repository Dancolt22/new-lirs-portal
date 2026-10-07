CREATE DATABASE LirsPortal;
GO
USE LirsPortal;
GO

CREATE TABLE Taxpayers (
    TaxpayerId INT PRIMARY KEY,
    TIN        VARCHAR(10)  NOT NULL UNIQUE,
    Name       VARCHAR(150) NOT NULL,
    Type       VARCHAR(20)  NOT NULL,
    State      VARCHAR(50)  NOT NULL,
    Phone      VARCHAR(20)
);

CREATE TABLE TaxReturns (
    ReturnId       INT PRIMARY KEY,
    TaxpayerId     INT NOT NULL REFERENCES Taxpayers (TaxpayerId),
    TaxYear        INT NOT NULL,
    DeclaredIncome DECIMAL(18,2) NOT NULL,
    TaxDue         DECIMAL(18,2) NOT NULL,
    Status         VARCHAR(20) NOT NULL
);

CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    ReturnId  INT NOT NULL REFERENCES TaxReturns (ReturnId),
    Amount    DECIMAL(18,2) NOT NULL,
    PaidOn    DATETIME2 NOT NULL,
    Channel   VARCHAR(20) NOT NULL
);

CREATE TABLE ComplianceLogs (
    LogId       INT IDENTITY(1,1) PRIMARY KEY,
    TaxpayerId  INT NOT NULL REFERENCES Taxpayers (TaxpayerId),
    Event       VARCHAR(200) NOT NULL,
    OfficerName VARCHAR(100),
    LoggedAt    DATETIME2 NOT NULL
);

CREATE INDEX IX_TaxReturns_TaxpayerId ON TaxReturns (TaxpayerId);
CREATE INDEX IX_Payments_ReturnId     ON Payments (ReturnId);
GO
