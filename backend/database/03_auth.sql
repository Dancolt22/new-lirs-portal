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

-- 2. Seed training users with PBKDF2 HMAC-SHA256 hashed passwords
-- Password for all training accounts: "pass123"
-- Format: {iterations}.{base64-salt}:{base64-subKey}

DELETE FROM Users;

INSERT INTO Users (Username, PasswordHash, Role, TaxpayerId, FailedAttempts, LockedUntil) VALUES
-- Original Core Accounts
('adewale', '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 101, 0, NULL),
('chioma',  '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 102, 0, NULL),
('bisi',    '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Officer',  NULL, 0, NULL),

-- 10+ Additional Taxpayer Accounts
('bello',   '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 103, 0, NULL),
('ngozi',   '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 104, 0, NULL),
('emeka',   '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 105, 0, NULL),
('fatima',  '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 106, 0, NULL),
('tunde',   '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 107, 0, NULL),
('amaka',   '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 108, 0, NULL),
('ibrahim', '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 109, 0, NULL),
('kemi',    '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 110, 0, NULL),
('olumide', '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 111, 0, NULL),
('zainab',  '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Taxpayer', 112, 0, NULL),

-- Additional Officer Account
('folake',  '10000.ga13/Me02+MX/Hd9gpHJkQ==:kFGvTNPaHY3scQap24ahQsfLdUraTtVfVRhIbQ9BJFc=', 'Officer',  NULL, 0, NULL);
GO

-- 3. Verification query to inspect our secure user records
SELECT UserId, Username, Role, TaxpayerId, FailedAttempts, LockedUntil 
FROM Users;
GO
