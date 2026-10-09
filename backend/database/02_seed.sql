-- ==============================================================================
-- LIRS TAXPAYER MINI-PORTAL: SEED DATA SCRIPT (02_seed.sql)
-- ==============================================================================
-- Purpose:
-- Populates the LirsPortal database with representative test records:
-- - 12 Fictional Taxpayers (Adewale, Chioma, Bello, Ngozi, Emeka, Fatima, Tunde, Amaka, Ibrahim, Kemi, Olumide, Zainab)
-- - 13 Annual Tax Returns across multiple years
-- - 12 Historic Payments spanning various payment channels
-- - Initial compliance audit entries
-- ==============================================================================

USE LirsPortal;
GO

-- 0. Clean up existing records to ensure idempotent execution
-- (Prevents duplicate payment insertions and negative balance bugs when re-run)
DELETE FROM ComplianceLogs;
DELETE FROM Payments;
DELETE FROM TaxReturns;
DELETE FROM Taxpayers;
GO

-- 1. Insert Core Taxpayer Entities
INSERT INTO Taxpayers (TaxpayerId, TIN, Name, Type, State, Phone) VALUES
(101, '1000000001', 'Adewale Ventures Ltd',    'Business',   'Lagos', '08030000001'),
(102, '1000000002', 'Chioma Okafor',           'Individual', 'Lagos', '08030000002'),
(103, '1000000003', 'Bello Logistics',         'Business',   'Lagos', '08030000003'),
(104, '1000000004', 'Ngozi Textiles',          'Business',   'Ogun',  '08030000004'),
(105, '1000000005', 'Emeka Eze Consulting',    'Individual', 'Lagos', '08030000005'),
(106, '1000000006', 'Fatima Aliyu Foods',      'Individual', 'Lagos', '08030000006'),
(107, '1000000007', 'Tunde Bakare Enterprises','Business',   'Lagos', '08030000007'),
(108, '1000000008', 'Amaka Johnson Creative',  'Individual', 'Lagos', '08030000008'),
(109, '1000000009', 'Ibrahim Musa Haulage',    'Business',   'Lagos', '08030000009'),
(110, '1000000010', 'Kemi Adeyemi & Partners', 'Business',   'Lagos', '08030000010'),
(111, '1000000011', 'Olumide Solar Systems Ltd','Business',  'Lagos', '08030000011'),
(112, '1000000012', 'Zainab Farouk Pharmacy',  'Individual', 'Lagos', '08030000012');

-- 2. Insert Filed Returns
INSERT INTO TaxReturns (ReturnId, TaxpayerId, TaxYear, DeclaredIncome, TaxDue, Status) VALUES
(1,  101, 2025,  5000000.00,  500000.00, 'Approved'),
(2,  101, 2026,  6000000.00,  600000.00, 'Submitted'),
(3,  102, 2025,  2400000.00,  240000.00, 'Approved'),
(4,  103, 2025, 10000000.00, 1000000.00, 'Approved'),
(5,  104, 2025,  3000000.00,  300000.00, 'Submitted'),
(6,  105, 2025,  4500000.00,  450000.00, 'Approved'),
(7,  106, 2025,  1800000.00,  180000.00, 'Approved'),
(8,  107, 2025,  8500000.00,  850000.00, 'Approved'),
(9,  108, 2025,  3200000.00,  320000.00, 'Approved'),
(10, 109, 2025, 12000000.00, 1200000.00, 'Approved'),
(11, 110, 2025,  5500000.00,  550000.00, 'Approved'),
(12, 111, 2025,  9000000.00,  900000.00, 'Submitted'),
(13, 112, 2025,  2100000.00,  210000.00, 'Approved');

-- 3. Insert Historical Payments
-- Return 1 (500k due):  300k + 100k paid -> 100k remaining
-- Return 2 (600k due):  0 paid -> 600k remaining (Total 101 liability: ₦700,000.00)
-- Return 3 (240k due):  240k paid -> ₦0.00 remaining (Chioma is fully settled!)
-- Return 4 (1M due):    400k paid -> 600k remaining
-- Return 6 (450k due):  150k paid -> 300k remaining
-- Return 7 (180k due):  180k paid -> ₦0.00 remaining (Fatima settled!)
-- Return 8 (850k due):  350k paid -> 500k remaining
-- Return 9 (320k due):  120k paid -> 200k remaining
-- Return 10 (1.2M due): 450k paid -> 750k remaining
-- Return 11 (550k due): 250k paid -> 300k remaining
-- Return 13 (210k due): 210k paid -> ₦0.00 remaining (Zainab settled!)
INSERT INTO Payments (ReturnId, Amount, PaidOn, Channel) VALUES
(1,  300000.00, '2026-03-10', 'Bank'),
(1,  100000.00, '2026-05-02', 'Card'),
(3,  240000.00, '2026-04-15', 'USSD'),
(4,  400000.00, '2026-06-01', 'Bank'),
(6,  150000.00, '2026-04-10', 'Bank'),
(7,  180000.00, '2026-05-14', 'Card'),
(8,  350000.00, '2026-03-22', 'Bank'),
(9,  120000.00, '2026-04-18', 'USSD'),
(10, 450000.00, '2026-05-05', 'Bank'),
(11, 250000.00, '2026-04-20', 'Card'),
(13, 210000.00, '2026-06-11', 'USSD');

-- 4. Insert Initial Compliance Audit Logs
INSERT INTO ComplianceLogs (TaxpayerId, Event, OfficerName, LoggedAt) VALUES
(101, 'Return 1 approved', 'Officer Bisi', '2026-02-20'),
(103, 'Return 4 approved', 'Officer Bisi', '2026-03-05'),
(105, 'Return 6 approved', 'Officer Folake', '2026-03-12'),
(107, 'Return 8 approved', 'Officer Bisi', '2026-03-25');
GO

