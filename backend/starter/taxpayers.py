# ==============================================================================
# LIRS TAXPAYER STARTER PROTOTYPE (taxpayers.py) - Day 2 Morning Exercise
# ==============================================================================
# Pedagogical Purpose:
# Before writing C# APIs or connecting to SQL Server, students write this simple
# in-memory Python script to conceptualize dictionary data structures, business
# rule arithmetic (tax due minus payments), and statutory penalty calculations.
# ==============================================================================

# Fictional seed data in simple dictionary format (representing our future database table)
TAXPAYERS = [
    {"tin": "1000000001", "name": "Adewale Ventures Ltd", "type": "Business", "state": "Lagos"},
    {"tin": "1000000002", "name": "Chioma Okafor", "type": "Individual", "state": "Lagos"},
    {"tin": "1000000003", "name": "Bello Logistics", "type": "Business", "state": "Lagos"},
    {"tin": "1000000004", "name": "Ngozi Textiles", "type": "Business", "state": "Ogun"},
]

# Statutory rates from the state finance regulations
PENALTY_RATE = 0.10          # 10% statutory late filing penalty
DAILY_LATE_FEE_NAIRA = 100   # ₦100 daily recurring late fee


def find_taxpayer_by_tin(tin):
    """
    Simulates a database index lookup.
    Searches through the taxpayer records and returns the matching dictionary,
    or None if the TIN is unregistered.
    """
    for taxpayer in TAXPAYERS:
        if taxpayer["tin"] == tin:
            return taxpayer
    return None


def calculate_balance(tax_due_naira, payments_naira):
    """
    Calculates the citizen's remaining tax debt.
    Formula: Total statutory tax assessed minus all payments recorded to date.
    """
    return tax_due_naira - sum(payments_naira)


def calculate_late_penalty(amount_due_naira, days_late, is_waived=False):
    """
    Calculates statutory penalty for late tax filing.
    
    Business Rules:
    1. If the penalty has been officially waived by the Executive Chairman, return 0.
    2. If the return was submitted on or before the due date (days_late <= 0), return 0.
    3. If there is no tax liability owed (amount_due_naira <= 0), return 0.
    4. Otherwise: 10% of the principal tax due + ₦100 for each overdue day.
    """
    if is_waived or days_late <= 0 or amount_due_naira <= 0:
        return 0

    percentage_penalty = amount_due_naira * PENALTY_RATE
    daily_penalty = days_late * DAILY_LATE_FEE_NAIRA
    return percentage_penalty + daily_penalty


# Simple verification script when executed directly from the terminal
if __name__ == "__main__":
    print("--- LIRS Morning Prototype Verification ---")
    
    # Test 1: Search by TIN
    taxpayer = find_taxpayer_by_tin("1000000001")
    if taxpayer:
        print(f"Found Taxpayer: {taxpayer['name']} ({taxpayer['type']}, {taxpayer['state']})")
    
    # Test 2: Calculate balance for ₦500,000 due with payments of ₦150,000 and ₦100,000
    balance = calculate_balance(500000, [150000, 100000])
    print(f"Calculated Balance: ₦{balance:,.2f}")  # Expected: ₦250,000.00
    
    # Test 3: Calculate penalty for ₦100,000 due, 5 days late
    # 10% of 100,000 = 10,000; 5 * 100 = 500; Total = 10,500
    penalty = calculate_late_penalty(100000, 5, False)
    print(f"Calculated Penalty: ₦{penalty:,.2f}")  # Expected: ₦10,500.00
