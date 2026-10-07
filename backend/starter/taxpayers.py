# Fictional data only. Real taxpayer data is never used in this project.
TAXPAYERS = [
    {"tin": "1000000001", "name": "Adewale Ventures Ltd", "type": "Business", "state": "Lagos"},
    {"tin": "1000000002", "name": "Chioma Okafor", "type": "Individual", "state": "Lagos"},
    {"tin": "1000000003", "name": "Bello Logistics", "type": "Business", "state": "Lagos"},
    {"tin": "1000000004", "name": "Ngozi Textiles", "type": "Business", "state": "Ogun"},
]

PENALTY_RATE = 0.10
DAILY_LATE_FEE_NAIRA = 100


def find_taxpayer_by_tin(tin):
    """Return the taxpayer with this TIN, or None if there is no match."""
    for taxpayer in TAXPAYERS:
        if taxpayer["tin"] == tin:
            return taxpayer
    return None


def calculate_balance(tax_due_naira, payments_naira):
    """Outstanding balance: total tax due minus everything paid so far."""
    return tax_due_naira - sum(payments_naira)


def calculate_late_penalty(amount_due_naira, days_late, is_waived=False):
    # Late fee set by the current finance act, confirm with the policy unit before changing
    if is_waived or days_late <= 0 or amount_due_naira <= 0:
        return 0

    percentage_penalty = amount_due_naira * PENALTY_RATE
    daily_penalty = days_late * DAILY_LATE_FEE_NAIRA
    return percentage_penalty + daily_penalty


if __name__ == "__main__":
    taxpayer = find_taxpayer_by_tin("1000000001")
    if taxpayer:
        print(f"Taxpayer: {taxpayer['name']}")
    balance = calculate_balance(500000, [150000, 100000])
    print(f"Calculated Balance: ₦{balance:,.2f}")  # ₦250,000.00
    penalty = calculate_late_penalty(100000, 5, False)
    print(f"Calculated Penalty: ₦{penalty:,.2f}")  # ₦10,500.00
