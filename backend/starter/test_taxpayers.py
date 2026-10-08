import pytest
from taxpayers import calculate_late_penalty, find_taxpayer_by_tin, calculate_balance

def test_penalty_when_on_time():
    """Validates that filing on time incurs zero statutory penalty."""
    assert calculate_late_penalty(100000, 0) == 0

def test_penalty_when_late():
    """Validates 10% principal plus ₦100 per day statutory late fee formula."""
    # 10% of ₦100,000 = ₦10,000; 5 days at ₦100/day = ₦500 -> ₦10,500
    assert calculate_late_penalty(100000, 5) == 10500

def test_penalty_when_negative_days():
    """Validates that early filing (negative days late) incurs zero penalty."""
    assert calculate_late_penalty(50000, -2) == 0

def test_penalty_when_waived():
    """Validates that an executive waiver results in zero penalty even if overdue."""
    assert calculate_late_penalty(100000, 15, is_waived=True) == 0

def test_find_taxpayer_found():
    """Validates TIN lookup returns the registered business record."""
    result = find_taxpayer_by_tin("1000000001")
    assert result is not None
    assert result["name"] == "Adewale Ventures Ltd"

def test_find_taxpayer_not_found():
    """Validates searching an unknown TIN safely returns None."""
    assert find_taxpayer_by_tin("9999999999") is None

def test_balance_calculation():
    """Validates balance arithmetic (total statutory due minus payments made)."""
    # ₦500,000 due minus payments of ₦150,000 and ₦100,000 = ₦250,000
    assert calculate_balance(500000, [150000, 100000]) == 250000
