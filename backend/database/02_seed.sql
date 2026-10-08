-- ==============================================================================
-- LIRS TAXPAYER MINI-PORTAL: SEED DATA SCRIPT (02_seed.sql)
-- ==============================================================================
-- Purpose:
-- Populates the LirsPortal database with representative test records:
-- - 4 Fictional Taxpayers (Adewale, Chioma, Bello, Ngozi)
-- - 5 Annual Tax Returns across multiple years
-- - 4 Historic Payments spanning various payment channels
-- - Initial compliance audit entries
-- ==============================================================================

USE LirsPortal;
GO

-- 1. Insert Core Taxpayer Entities
INSERT INTO Taxpayers (TaxpayerId, TIN, Name, Type, State, Phone) VALUES
(101, '1000000001', 'Adewale Ventures Ltd', 'Business',   'Lagos', '08030000001'),
(102, '1000000002', 'Chioma Okafor',        'Individual', 'Lagos', '08030000002'),
(103, '1000000003', 'Bello Logistics',      'Business',   'Lagos', '08030000003'),
(104, '1000000004', 'Ngozi Textiles',       'Business',   'Ogun',  '08030000004');

-- 2. Insert Filed Returns
-- Note: Taxpayer 101 has two returns (2025 and 2026).
-- Taxpayer 102 has one return with ₦240,000 tax due.
INSERT INTO TaxReturns (ReturnId, TaxpayerId, TaxYear, DeclaredIncome, TaxDue, Status) VALUES
(1, 101, 2025,  5000000.00,  500000.00, 'Approved'),
(2, 101, 2026,  6000000.00,  600000.00, 'Submitted'),
(3, 102, 2025,  2400000.00,  240000.00, 'Approved'),
(4, 103, 2025, 10000000.00, 1000000.00, 'Approved'),
(5, 104, 2025,  3000000.00,  300000.00, 'Submitted');

-- 3. Insert Historical Payments
-- Return 1 (500k due): 300k + 100k paid -> 100k remaining
-- Return 2 (600k due): 0 paid -> 600k remaining (Total 101 liability: ₦700,000.00)
-- Return 3 (240k due): 240k paid -> ₦0.00 remaining (Chioma is fully settled!)
-- Return 4 (1M due):   400k paid -> 600k remaining
INSERT INTO Payments (ReturnId, Amount, PaidOn, Channel) VALUES
(1, 300000.00, '2026-03-10', 'Bank'),
(1, 100000.00, '2026-05-02', 'Card'),
(3, 240000.00, '2026-04-15', 'USSD'),
(4, 400000.00, '2026-06-01', 'Bank');

-- 4. Insert Initial Compliance Audit Logs
INSERT INTO ComplianceLogs (TaxpayerId, Event, OfficerName, LoggedAt) VALUES
(101, 'Return 1 approved', 'Officer Bisi', '2026-02-20'),
(103, 'Return 4 approved', 'Officer Bisi', '2026-03-05');
GO
