/**
 * ==============================================================================
 * Currency Formatter Component (src/components/Naira.jsx) - Day 3
 * ==============================================================================
 * Architecture Purpose:
 * A reusable presentation component that consistently formats currency amounts
 * according to Nigerian Naira standards (symbol ₦, comma separators, 2 decimal places).
 *
 * Example:
 * <Naira value={500000} /> renders: ₦500,000.00
 * ==============================================================================
 */

/**
 * Naira Component
 *
 * @param {object} props
 * @param {number|string} props.value - Numeric or string monetary amount
 */
export default function Naira({ value }) {
  // Convert incoming value to a number safely, defaulting to 0 if invalid/null
  const amount = Number(value) || 0;

  // Format with standard Nigerian locale number formatting
  const formatted = amount.toLocaleString("en-NG", { 
    minimumFractionDigits: 2, 
    maximumFractionDigits: 2 
  });

  return <>₦{formatted}</>;
}
