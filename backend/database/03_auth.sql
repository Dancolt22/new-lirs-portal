-- =========================================================================================
-- LIRS TAXPAYER MINI-PORTAL: DATABASE SECURITY & AUTHENTICATION MIGRATION (DAY 4)
-- Purpose: Introduces the Users table for cryptographic authentication, salted password
--          hashes (PBKDF2), role-based access control (RBAC), and brute-force lockout defenses.
-- =========================================================================================

USE LirsPortal;
GO

-- 1. Create the Users table
-- Senior Architect Note:
-- Never, under any circumstances, store plain-text passwords in a government database.
-- We store a cryptographic hash string containing the iteration count, salt, and subkey.
-- We also track FailedAttempts and LockedUntil so automated bots cannot guess passwords.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId         INT IDENTITY(1,1) PRIMARY KEY,
        Username       VARCHAR(50) NOT NULL UNIQUE,
        PasswordHash   VARCHAR(255) NOT NULL,
        Role           VARCHAR(20) NOT NULL,       -- 'Taxpayer' for citizens, 'Officer' for revenue staff
        TaxpayerId     INT NULL REFERENCES Taxpayers(TaxpayerId), -- Links citizen user directly to their taxpayer record
        FailedAttempts INT NOT NULL DEFAULT 0,     -- Incremented on bad password; triggers account lockout at 5 attempts
        LockedUntil    DATETIME2 NULL              -- Timestamp until which login attempts are hard-blocked (HTTP 423)
    );

    -- Index on Username guarantees sub-millisecond lookup times during high-concurrency login surges
    CREATE INDEX IX_Users_Username ON Users (Username);
END
GO

-- 2. Seed initial training users with PBKDF2 HMAC-SHA256 hashed passwords
-- Password for all three training accounts: "pass123"
-- Format: {iterations}.{base64-salt}:{base64-subKey}
--
-- Fictional credentials:
-- 1. adewale / pass123 -> Taxpayer for Adewale Ventures Ltd (TaxpayerId: 101)
-- 2. chioma  / pass123 -> Taxpayer for Chioma Okafor (TaxpayerId: 102)
-- 3. bisi    / pass123 -> LIRS Revenue Officer (TaxpayerId: NULL, Role: 'Officer')

DELETE FROM Users;

INSERT INTO Users (Username, PasswordHash, Role, TaxpayerId, FailedAttempts, LockedUntil) VALUES
('adewale', '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 101, 0, NULL),
('chioma',  '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 102, 0, NULL),
('bisi',    '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Officer',  NULL, 0, NULL);
GO

-- 3. Verification query to inspect our secure user records
SELECT UserId, Username, Role, TaxpayerId, FailedAttempts, LockedUntil 
FROM Users;
GO
