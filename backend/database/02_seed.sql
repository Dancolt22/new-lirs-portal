USE LirsPortal;
GO

INSERT INTO Taxpayers (TaxpayerId, TIN, Name, Type, State, Phone) VALUES
(101, '1000000001', 'Adewale Ventures Ltd', 'Business',   'Lagos', '08030000001'),
(102, '1000000002', 'Chioma Okafor',        'Individual', 'Lagos', '08030000002'),
(103, '1000000003', 'Bello Logistics',      'Business',   'Lagos', '08030000003'),
(104, '1000000004', 'Ngozi Textiles',       'Business',   'Ogun',  '08030000004');

INSERT INTO TaxReturns (ReturnId, TaxpayerId, TaxYear, DeclaredIncome, TaxDue, Status) VALUES
(1, 101, 2025,  5000000.00,  500000.00, 'Approved'),
(2, 101, 2026,  6000000.00,  600000.00, 'Submitted'),
(3, 102, 2025,  2400000.00,  240000.00, 'Approved'),
(4, 103, 2025, 10000000.00, 1000000.00, 'Approved'),
(5, 104, 2025,  3000000.00,  300000.00, 'Submitted');

INSERT INTO Payments (ReturnId, Amount, PaidOn, Channel) VALUES
(1, 300000.00, '2026-03-10', 'Bank'),
(1, 100000.00, '2026-05-02', 'Card'),
(3, 240000.00, '2026-04-15', 'USSD'),
(4, 400000.00, '2026-06-01', 'Bank');

INSERT INTO ComplianceLogs (TaxpayerId, Event, OfficerName, LoggedAt) VALUES
(101, 'Return 1 approved', 'Officer Bisi', '2026-02-20'),
(103, 'Return 4 approved', 'Officer Bisi', '2026-03-05');
GO
