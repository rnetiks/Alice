using System;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

/**
	<summary>	
		Namespace for extended mathematical calculations.
	</summary>	
*/
namespace HyperMath {

	/**
		<summary>	
			Datatype to calculate with numbers of arbitrary precision.
		</summary>	
	*/
	public partial struct EDecimal: IEquatable<EDecimal> {

		// internal
		public byte[] Value {
			get; private set;
		}

		public int FPos;
		public int VLen {
			get {
				return Value.Length;
			}
		}

		// flags
		public bool IsNegative;

		public static char DecimalSeparator = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator[0];

		// required for "ToDouble()"
		public static char LocalDecimalSeparator = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator[0];

		public static char MinusSign = '-';
		private static byte MinusSignByte = (byte)'-';
		public static char[] TrimCharZero = { '0' };
		public static char[] TrimCharComma = { '.', ',' };
		private static byte[] EmptyValue = {};

		public static EDecimal Zero = new EDecimal("0");
		public static EDecimal One  = new EDecimal("1");
		public static EDecimal Two  = new EDecimal("2");
		public static EDecimal Ten  = new EDecimal("10");

		public const int FixedPrecision   = -2;
		public const int FullPrecision    = -1;
		public const int DefaultPrecision = -1;

		public static int Precision = 50;

		public int CurrentPrecision {
			get {
				return Math.Min(0,-FPos);
			}
		}

		public EDecimal(string number) {
			IsNegative = false;
			FPos = 0;
			Value = EmptyValue;

			Process(number);
        }
		public EDecimal(int number) {
			IsNegative = false;
			FPos = 0;
			Value = EmptyValue;

			Process(number.ToString());
		}
		public EDecimal(double number) {
			IsNegative = false;
			FPos = 0;
			Value = EmptyValue;

			Process(number.ToString());
		}
		public EDecimal(decimal number) {
			IsNegative = false;
			FPos = 0;
			Value = EmptyValue;

			Process(number.ToString());
		}
		
		private void Process(string number) {

			// check for compact (e.g.: -1.3456E-5)
			if ((number.IndexOf('e') > -1) || (number.IndexOf('E') > -1)) {
				number = EDMath.FromCompact(number);
			}

			// simple "numbers"
			if ((number == "0") || (number == "") || (number == null)) {
				// IsZero = true;
				Value = new byte[1];
				return;
			}
			else if ((number == "1") || (number == "-1")) {
				// IsOne = true;
				IsNegative = number.StartsWith(""+MinusSign);
				Value = new byte[1];
				Value[0] = 1;
				return;
			}

			int len = number.Length;
			int sub = 0;

			// get minus
			if (number.StartsWith(""+MinusSign)) {
				IsNegative = true;
				sub++;
			}

			int commaPos = number.IndexOf('.');
			if (commaPos > -1) {
				FPos = -(len-commaPos-1);
				sub++;
			}
			// local comma
			else {
				commaPos = number.IndexOf(',');
				if (commaPos > -1) {
					FPos = -(len-commaPos-1);
					sub++;
				}
			}

			// init value
			Value = new byte[len-sub];

			int pos = 0;
			byte c;
			for (int i = 0; i<len; i++) {
				if (i == commaPos) {
					continue;
				}
				c = (byte)number[i];

				// - and ,
				if (c == MinusSignByte) continue;
				Value[pos] = (byte)(c-48);
				pos++;
			}

		}

	}

	// convertion methods
	public partial struct EDecimal {

		/**
			<summary>
				Get the full string representation of the number.
			</summary>
			<returns>String representation.</returns>
		*/
		new public string ToString() {
			return EDMath.ToString(this);
		}

		/**
			<summary>
				Get the string representation of the number with given precision.
			</summary>
			<param name="precision">Left operand</param>
			<returns>String representation.</returns>
		*/
		public string ToString(int precision) {
			return EDMath.ToString(this, precision);
		}

		/**
			<summary>
				Get the double representation if possible.
			</summary>
			<returns></returns>
		*/
		public double ToDouble() {
			return Convert.ToDouble(EDMath.ToString(this).Replace(DecimalSeparator, LocalDecimalSeparator));
		}

		/**
			<summary>
				Get the decimal representation if possible.
			</summary>
			<returns></returns>
		*/
		public decimal ToDecimal() {
			return Convert.ToDecimal(EDMath.ToString(this).Replace(DecimalSeparator, LocalDecimalSeparator));
		}

		/**
			<summary>
				Get the int32 representation if possible.
			</summary>
			<returns></returns>
		*/
		public int ToInt32() {
			return Convert.ToInt32(EDMath.ToString(this).Replace(DecimalSeparator, LocalDecimalSeparator));
		}

		/**
			<summary>
				Get the int64 representation if possible.
			</summary>
			<returns></returns>
		*/
		public long ToInt64() {
			return Convert.ToInt64(EDMath.ToString(this).Replace(DecimalSeparator, LocalDecimalSeparator));
		}


	}

	// calculations
	public partial struct EDecimal {

		/**
			<summary>
				Negate the number.
			</summary>
			<returns></returns>
		*/
		public EDecimal Negate() {
			EDecimal r = this;
			r.IsNegative = !r.IsNegative;
			return r;
		}

		/**
			<summary>
				Move the comma to the left by n places.
			</summary>
			<returns></returns>
		*/
		public EDecimal CommaLeft(int n = 1) {
			EDecimal r = this;
			r.FPos -= n;
			return r;
		}

		/**
			<summary>
				Move the comma to the right by n places.
			</summary>
			<returns></returns>
		*/
		public EDecimal CommaRight(int n = 1) {
			EDecimal r = this;
			r.FPos += n;
			return r;
		}

	}

	// calculation shortcuts
	public partial struct EDecimal {

		/**
			<summary>
				Compare the number against an other. 1 if the number is greater, -1 if ""b is greater, 0 if they are equal.
			</summary>
			<param name="b">The number to compare against.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns></returns>
		*/
		public int CompareTo(EDecimal b, int precision = DefaultPrecision) {
			return EDMath.CompareTo(this, b, precision);
		}

		/**
			<summary>
				Add "b" to the number.
			</summary>
			<param name="b">The number to add.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The sum of the number and "b".</returns>
		*/
		public EDecimal Add(EDecimal b, int precision = DefaultPrecision) {
			return EDMath.Add(this, b, precision);
		}

		/**
			<summary>
				Subtract "b" from the number.
			</summary>
			<param name="b">The number to subtract.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The difference of the number and "b".</returns>
		*/
		public EDecimal Sub(EDecimal b, int precision = DefaultPrecision) {
			return EDMath.Sub(this, b, precision);
		}

		/**
			<summary>
				Multiply the number with "b".
			</summary>
			<param name="b">The number to multiply with.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The product of the number and "b".</returns>
		*/
		public EDecimal Mul(EDecimal b, int precision = DefaultPrecision) {
			return EDMath.Mul(this, b, precision);
		}

		/**
			<summary>
				Divide the number by "b".
			</summary>
			<param name="b">The divisor.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The division of the number by "b".</returns>
		*/
		public EDecimal Div(EDecimal b, int precision = DefaultPrecision) {
			return EDMath.Div(this, b, precision);
		}
		
		/**
			<summary>
				Integer divide the number by "b".
			</summary>
			<param name="b">The divisor.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The integer division of the number by "b".</returns>
		*/
		public EDecimal IDiv(EDecimal b) {
			return EDMath.Div(this, b, 0);
		}

		/**
			<summary>
				Create the power with integer exponent.
			</summary>
			<param name="b">The exponent.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The power of the number.</returns>
		*/
		public EDecimal Pow(int b, int precision = DefaultPrecision) {
			return EDMath.Pow(this, b, precision);
		}
		
		/**
			<summary>
				Create the general power.
			</summary>
			<param name="b">The exponent.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The power of the number.</returns>
		*/
		public EDecimal Pow(EDecimal b, int precision = DefaultPrecision) {
			return EDMath.Pow(this, b, precision);
		}

		/**
			<summary>
				Create the sqare root.
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The sqare root of the number.</returns>
		*/
		public EDecimal Sqrt(int precision = DefaultPrecision) {
			return EDMath.Sqrt(this, precision);
		}

		/**
			<summary>
				Create the integer root.
			</summary>
			<param name="b">The root level.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The integer root of the number.</returns>
		*/
		public EDecimal Root(int b, int precision = DefaultPrecision) {
			return EDMath.Root(this, b, precision);
		}

		/**
			<summary>
				Create the general root of the number.
			</summary>
			<param name="b">The root level.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The root of the number.</returns>
		*/
		public EDecimal Root(EDecimal b, int precision = DefaultPrecision) {
			return EDMath.Root(this, b, precision);
		}

		/**
			<summary>
				Create the exponent of the number (e^x).
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The exponential value of e^number.</returns>
		*/
		public EDecimal Exp(int precision = DefaultPrecision) {
			return EDMath.Exp(this,precision);
		}

		/**
			<summary>
				Create the natural logarithm of the number.
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The logarithm of the number.</returns>
		*/
		public EDecimal Log(int precision = DefaultPrecision) {
			return EDMath.Log(this, precision);
		}

		/**
			<summary>
				Create the general logarithm of the number.
			</summary>
			<param name="Base">The base to use.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The logarithm of the number.</returns>
		*/
		public EDecimal Log(EDecimal Base, int precision = DefaultPrecision) {
			return EDMath.Log(this, Base, precision);
		}

		/**
			<summary>
				Create the logarithm of the number with base 10.
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The logarithm of the number.</returns>
		*/
		public EDecimal Log10(int precision = DefaultPrecision) {
			return EDMath.Log(this, EDecimal.Ten, precision);
		}

		/**
			<summary>
				Create the sinus of the number.
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The sinus of the number.</returns>
		*/
		public EDecimal Sin(int precision = DefaultPrecision) {
			return EDMath.Sin(this, precision);
		}

		/**
			<summary>
				Create the cosinus of the number.
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The cosinus of the number.</returns>
		*/
		public EDecimal Cos(int precision = DefaultPrecision) {
			return EDMath.Cos(this, precision);
		}

		/**
			<summary>
				Create the tangens of the number.
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The tangens of the number.</returns>
		*/
		public EDecimal Tan(int precision = DefaultPrecision) {
			return EDMath.Tan(this, precision);
		}

		/**
			<summary>
				Create the cotangens of the number.
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The cotangens of the number.</returns>
		*/
		public EDecimal Cot(int precision = DefaultPrecision) {
			return EDMath.Cot(this, precision);
		}

		/**
			<summary>
				Get the sign of the number. -1 if lower than 0, 1 if greater than 0, 0 if 0.
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>-1 if lower than 0, 1 if greater than 0, 0 if 0.</returns>
		*/
		public EDecimal Sign() {
			return EDMath.Sign(this);
		}

		/**
			<summary>
				Truncate the fractional part.
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>Integer part of the number.</returns>
		*/
		public EDecimal Trunc(int precision = 0) {
			return EDMath.Trunc(this, precision);
		}

		/**
			<summary>
				Get the fractional part.
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The fractional part of the number.</returns>
		*/
		public EDecimal Frac(int precision = DefaultPrecision) {
			return EDMath.Frac(this, precision);
		}

		/**
			<summary>
				Get the floor of the number.
			</summary>
			<returns>The floor of the number.</returns>
		*/
		public EDecimal Floor() {
			return EDMath.Floor(this);
		}

		/**
			<summary>
				Get the ceiling of the number.
			</summary>
			<returns>The ceiling of the number.</returns>
		*/
		public EDecimal Ceiling() {
			return EDMath.Ceiling(this);
		}

		/**
			<summary>
				Round the number.
			</summary>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The rounded number.</returns>
		*/
		public EDecimal Round(int precision = 0) {
			return EDMath.Round(this, precision);
		}

		/**
			<summary>
				Get the absolute value of the number.
			</summary>
			<returns>The absolute value of the number.</returns>
		*/
		public EDecimal Abs() {
			return EDMath.Abs(this);
		}

		/**
			<summary>
				Get the minimum of the number and "b".
			</summary>
			<param name="b">The number to compate to.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The minimum of the number and "b".</returns>
		*/
		public EDecimal Min(EDecimal b, int precision = DefaultPrecision) {
			return EDMath.Min(this, b, precision);
		}

		/**
			<summary>
				Get the maximum of the number and "b".
			</summary>
			<param name="b">The number to compate to.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The maximum of the number and "b".</returns>
		*/
		public EDecimal Max(EDecimal b, int precision = DefaultPrecision) {
			return EDMath.Max(this, b, precision);
		}

		/**
			<summary>
				Generates the faculty of the number. The number must be a positive integer.
			</summary>
			<returns>The faculty of the number.</returns>
		*/
		public EDecimal Faculty() {
			return EDMath.Faculty(this);
		}

		/**
			<summary>
				Get the modulo of the number and "b".
			</summary>
			<param name="b">The number to devide by.</param>
			<param name="precision">Optional: the precision to use.</param>
			<returns>The modulo of the number and "b".</returns>
		*/
		public EDecimal Mod(EDecimal b, int precision = DefaultPrecision) {
			return EDMath.Mod(this,b,precision);
		}


	}

	// queries
	public partial struct EDecimal {

		/**
			<summary>
				Check if the number is an integer.
			</summary>
			<returns></returns>
		*/
		public bool IsInteger() {
			return EDMath.IsInteger(this);
		}

		/**
			<summary>
				Check if the number is a float.
			</summary>
			<returns></returns>
		*/
		public bool IsFloat() {
			return !EDMath.IsInteger(this);
		}

		/**
			<summary>
				Check if the number is even (integer only).
			</summary>
			<returns></returns>
		*/
		public bool IsOdd() {
			return EDMath.IsOdd(this);
		}

		/**
			<summary>
				Check if the number is odd (integer only).
			</summary>
			<returns></returns>
		*/
		public bool IsEven() {
			return !EDMath.IsOdd(this);
		}

		/**
			<summary>
				Check if the number is 0.
			</summary>
			<returns></returns>
		*/
		public bool IsZero() {
			return EDMath.IsZero(this);
		}

		/**
			<summary>
				Check if the number is 1 or -1.
			</summary>
			<returns></returns>
		*/
		public bool IsOne() {
			return EDMath.IsOne(this);
		}

	}

	// basic operators
	public partial struct EDecimal {

		public static EDecimal operator -(EDecimal a) {
			a.IsNegative = !a.IsNegative;
			return a;
		}

		public static EDecimal operator +(EDecimal a, EDecimal b) {
			// optimization: check for Negative numbers here
			return EDMath.Add(a, b);
		}

		public static EDecimal operator -(EDecimal a, EDecimal b) {
			// optimization: check for Negative numbers here
			return EDMath.Sub(a, b);
		}

		public static EDecimal operator *(EDecimal a, EDecimal b) {
			return EDMath.Mul(a, b);
		}

		public static EDecimal operator /(EDecimal a, EDecimal b) {
			return EDMath.Div(a, b);
		}

		public static EDecimal operator ++(EDecimal a) {
			return EDMath.Add(a, One);
		}

		public static EDecimal operator --(EDecimal a) {
			return EDMath.Sub(a, One);
		}

		public static EDecimal operator %(EDecimal a, EDecimal b) {
			return EDMath.Mod(a, b);
		}

	}

	// additional operators (string, int, double, decimal)
	public partial struct EDecimal {

		public static EDecimal operator +(EDecimal a, string b) {
			return EDMath.Add(a,new EDecimal(b));
		}
		public static EDecimal operator +(EDecimal a, int b) {
			return EDMath.Add(a,new EDecimal(b.ToString()));
		}
		public static EDecimal operator +(EDecimal a, double b) {
			return EDMath.Add(a,new EDecimal(b.ToString()));
		}
		public static EDecimal operator +(EDecimal a, decimal b) {
			return EDMath.Add(a,new EDecimal(b.ToString()));
		}
		public static EDecimal operator +(string a, EDecimal b) {
			return EDMath.Add(new EDecimal(a),b);
		}
		public static EDecimal operator +(int a, EDecimal b) {
			return EDMath.Add(new EDecimal(a.ToString()),b);
		}
		public static EDecimal operator +(double a, EDecimal b) {
			return EDMath.Add(new EDecimal(a.ToString()),b);
		}
		public static EDecimal operator +(decimal a, EDecimal b) {
			return EDMath.Add(new EDecimal(a.ToString()),b);
		}

		public static EDecimal operator -(EDecimal a, string b) {
			return EDMath.Sub(a,new EDecimal(b));
		}
		public static EDecimal operator -(EDecimal a, int b) {
			return EDMath.Sub(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator -(EDecimal a, double b) {
			return EDMath.Sub(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator -(EDecimal a, decimal b) {
			return EDMath.Sub(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator -(string a, EDecimal b) {
			return EDMath.Sub(new EDecimal(a),b);
		}
		public static EDecimal operator -(int a, EDecimal b) {
			return EDMath.Sub(new EDecimal(a.ToString()),b);
		}
		public static EDecimal operator -(double a, EDecimal b) {
			return EDMath.Sub(new EDecimal(a.ToString()), b);
		}
		public static EDecimal operator -(decimal a, EDecimal b) {
			return EDMath.Sub(new EDecimal(a.ToString()), b);
		}

		public static EDecimal operator *(EDecimal a, string b) {
			return EDMath.Mul(a, new EDecimal(b));
		}
		public static EDecimal operator *(EDecimal a, int b) {
			return EDMath.Mul(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator *(EDecimal a, double b) {
			return EDMath.Mul(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator *(EDecimal a, decimal b) {
			return EDMath.Mul(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator *(string a, EDecimal b) {
			return EDMath.Mul(new EDecimal(a), b);
		}
		public static EDecimal operator *(int a, EDecimal b) {
			return EDMath.Mul(new EDecimal(a.ToString()), b);
		}
		public static EDecimal operator *(double a, EDecimal b) {
			return EDMath.Mul(new EDecimal(a.ToString()), b);
		}
		public static EDecimal operator *(decimal a, EDecimal b) {
			return EDMath.Mul(new EDecimal(a.ToString()), b);
		}

		public static EDecimal operator /(EDecimal a, string b) {
			return EDMath.Div(a, new EDecimal(b));
		}
		public static EDecimal operator /(EDecimal a, int b) {
			return EDMath.Div(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator /(EDecimal a, double b) {
			return EDMath.Div(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator /(EDecimal a, decimal b) {
			return EDMath.Div(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator /(string a, EDecimal b) {
			return EDMath.Div(new EDecimal(a), b);
		}
		public static EDecimal operator /(int a, EDecimal b) {
			return EDMath.Div(new EDecimal(a.ToString()), b);
		}
		public static EDecimal operator /(double a, EDecimal b) {
			return EDMath.Div(new EDecimal(a.ToString()), b);
		}
		public static EDecimal operator /(decimal a, EDecimal b) {
			return EDMath.Div(new EDecimal(a.ToString()), b);
		}

		public static EDecimal operator %(EDecimal a, string b) {
			return EDMath.Mod(a,new EDecimal(b));
		}
		public static EDecimal operator %(EDecimal a, int b) {
			return EDMath.Mod(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator %(EDecimal a, double b) {
			return EDMath.Mod(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator %(EDecimal a, decimal b) {
			return EDMath.Mod(a, new EDecimal(b.ToString()));
		}
		public static EDecimal operator %(string a, EDecimal b) {
			return EDMath.Mod(new EDecimal(a),b);
		}
		public static EDecimal operator %(int a, EDecimal b) {
			return EDMath.Mod(new EDecimal(a.ToString()), b);
		}
		public static EDecimal operator %(double a, EDecimal b) {
			return EDMath.Mod(new EDecimal(a.ToString()), b);
		}
		public static EDecimal operator %(decimal a, EDecimal b) {
			return EDMath.Mod(new EDecimal(a.ToString()), b);
		}

	}

	// basic equatables
	public partial struct EDecimal {

		// ==
		public static bool operator ==(EDecimal a, EDecimal b) {
			return EDMath.CompareTo(a, b) == 0;
		}

		// !=
		public static bool operator !=(EDecimal a, EDecimal b) {
			return EDMath.CompareTo(a, b) != 0;
		}

		// <
		public static bool operator <(EDecimal a, EDecimal b) {
			return EDMath.CompareTo(a,b) == -1;
		}

		// >
		public static bool operator >(EDecimal a, EDecimal b) {
			return EDMath.CompareTo(a, b) == 1;
		}

		// <=
		public static bool operator <=(EDecimal a, EDecimal b) {
			return EDMath.CompareTo(a, b) <= 0;
		}

		// >=
		public static bool operator >=(EDecimal a, EDecimal b) {
			return EDMath.CompareTo(a, b) >= 0;
		}

		// equals
		public bool Equals(EDecimal b) {
			return EDMath.CompareTo(this, b) == 0;
		}
		public bool Equals(EDecimal b, int precision) {
			return EDMath.CompareTo(this, b, precision) == 0;
		}

		public override bool Equals(object obj) {
			return obj is EDecimal && (EDMath.CompareTo(this, (EDecimal)obj) == 0);
		}

		// TOCHECK!!!
		public override int GetHashCode() {
			unchecked {
				return (Value.GetHashCode() * 397) ^ IsNegative.GetHashCode();
			}
		}

	}

	// additional equatables (string, int, double, decimal)
	public partial struct EDecimal {

		// ==
		public static bool operator ==(EDecimal a, string b) {
			return EDMath.CompareTo(a,new EDecimal(b)) == 0;
		}
		public static bool operator ==(EDecimal a, int b) {
			return EDMath.CompareTo(a, new EDecimal(b)) == 0;
		}
		public static bool operator ==(EDecimal a, double b) {
			return EDMath.CompareTo(a, new EDecimal(b)) == 0;
		}
		public static bool operator ==(EDecimal a, decimal b) {
			return EDMath.CompareTo(a, new EDecimal(b)) == 0;
		}

		public static bool operator ==(string a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a),b) == 0;
		}
		public static bool operator ==(int a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) == 0;
		}
		public static bool operator ==(double a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) == 0;
		}
		public static bool operator ==(decimal a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) == 0;
		}

		// !=
		public static bool operator !=(EDecimal a, string b) {
			return EDMath.CompareTo(a, new EDecimal(b)) != 0;
		}
		public static bool operator !=(EDecimal a, int b) {
			return EDMath.CompareTo(a, new EDecimal(b)) != 0;
		}
		public static bool operator !=(EDecimal a, double b) {
			return EDMath.CompareTo(a, new EDecimal(b)) != 0;
		}
		public static bool operator !=(EDecimal a, decimal b) {
			return EDMath.CompareTo(a, new EDecimal(b)) != 0;
		}

		public static bool operator !=(string a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) != 0;
		}
		public static bool operator !=(int a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) != 0;
		}
		public static bool operator !=(double a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) != 0;
		}
		public static bool operator !=(decimal a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) != 0;
		}

		// <
		public static bool operator <(EDecimal a, string b) {
			return EDMath.CompareTo(a, new EDecimal(b)) < 0;
		}
		public static bool operator <(EDecimal a, int b) {
			return EDMath.CompareTo(a, new EDecimal(b)) < 0;
		}
		public static bool operator <(EDecimal a, double b) {
			return EDMath.CompareTo(a, new EDecimal(b)) < 0;
		}
		public static bool operator <(EDecimal a, decimal b) {
			return EDMath.CompareTo(a, new EDecimal(b)) < 0;
		}

		public static bool operator <(string a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) < 0;
		}
		public static bool operator <(int a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) < 0;
		}
		public static bool operator <(double a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) < 0;
		}
		public static bool operator <(decimal a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) < 0;
		}

		// >
		public static bool operator >(EDecimal a, string b) {
			return EDMath.CompareTo(a, new EDecimal(b)) > 0;
		}
		public static bool operator >(EDecimal a, int b) {
			return EDMath.CompareTo(a, new EDecimal(b)) > 0;
		}
		public static bool operator >(EDecimal a, double b) {
			return EDMath.CompareTo(a, new EDecimal(b)) > 0;
		}
		public static bool operator >(EDecimal a, decimal b) {
			return EDMath.CompareTo(a, new EDecimal(b)) > 0;
		}

		public static bool operator >(string a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) > 0;
		}
		public static bool operator >(int a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) > 0;
		}
		public static bool operator >(double a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) > 0;
		}
		public static bool operator >(decimal a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) > 0;
		}

		// <=
		public static bool operator <=(EDecimal a, string b) {
			return EDMath.CompareTo(a, new EDecimal(b)) <= 0;
		}
		public static bool operator <=(EDecimal a, int b) {
			return EDMath.CompareTo(a, new EDecimal(b)) <= 0;
		}
		public static bool operator <=(EDecimal a, double b) {
			return EDMath.CompareTo(a, new EDecimal(b)) <= 0;
		}
		public static bool operator <=(EDecimal a, decimal b) {
			return EDMath.CompareTo(a, new EDecimal(b)) <= 0;
		}

		public static bool operator <=(string a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) <= 0;
		}
		public static bool operator <=(int a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) <= 0;
		}
		public static bool operator <=(double a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) <= 0;
		}
		public static bool operator <=(decimal a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) <= 0;
		}

		// >=
		public static bool operator >=(EDecimal a, string b) {
			return EDMath.CompareTo(a, new EDecimal(b)) >= 0;
		}
		public static bool operator >=(EDecimal a, int b) {
			return EDMath.CompareTo(a, new EDecimal(b)) >= 0;
		}
		public static bool operator >=(EDecimal a, double b) {
			return EDMath.CompareTo(a, new EDecimal(b)) >= 0;
		}
		public static bool operator >=(EDecimal a, decimal b) {
			return EDMath.CompareTo(a, new EDecimal(b)) >= 0;
		}

		public static bool operator >=(string a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) >= 0;
		}
		public static bool operator >=(int a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) >= 0;
		}
		public static bool operator >=(double a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) >= 0;
		}
		public static bool operator >=(decimal a, EDecimal b) {
			return EDMath.CompareTo(new EDecimal(a), b) >= 0;
		}

		// equals
		public bool Equals(string b, int precision = EDecimal.DefaultPrecision) {
			return EDMath.CompareTo(this, new EDecimal(b), precision) >= 0;
		}
		public bool Equals(int b) {
			return EDMath.CompareTo(this, new EDecimal(b)) >= 0;
		}
		public bool Equals(double b, int precision = EDecimal.DefaultPrecision) {
			return EDMath.CompareTo(this, new EDecimal(b), precision) >= 0;
		}
		public bool Equals(decimal b, int precision = EDecimal.DefaultPrecision) {
			return EDMath.CompareTo(this, new EDecimal(b), precision) >= 0;
		}

	}

	// main calculations
	public static class EDMath {
		public static EDecimal Zero = new EDecimal("0");
		public static EDecimal One = new EDecimal("1");
		public static EDecimal Two = new EDecimal("2");
		public static EDecimal Ten = new EDecimal("10");

		// conversions
		/**
			<summary>
				Get the full string representation of the number from format "1.234E12".
			</summary>
			<returns></returns>
		*/
		public static string FromCompact(string Number) {
			string pattern = @"^([\+\-]{0,1})(\d+)(([\.\,]{0,1})(\d*))e([\+\-]{1})(\d+)$";
			Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
			MatchCollection matches = rgx.Matches(Number);
			if (matches.Count == 0) {
				return Number;
			}

			Match m = matches[0];

			string sign = m.Groups[1].Value;
			string num1 = m.Groups[2].Value;
			// string comma = m.Groups[4].Value;
			string num2 = m.Groups[5].Value;
			string signE = m.Groups[6].Value;
			int e = Convert.ToInt32(m.Groups[7].Value);

			string result = sign;

			if (signE == "+") {
				if (e >= num2.Length) {
					result += num1+num2.PadRight(e, '0');
				}
				else {
					result += num1+num2.Substring(0, e)+EDecimal.DecimalSeparator+num2.Substring(e+1);
				}
			}
			else {
				if (e >= num1.Length) {
					result += "0"+EDecimal.DecimalSeparator+num1.PadLeft(e, '0')+num2;
				}
				else {
					result += num1.Substring(0, e)+EDecimal.DecimalSeparator+num1.Substring(e+1)+num2;
				}
			}

			return result;
		}

		// formatting
		public static string ToString(EDecimal a, int precision = EDecimal.DefaultPrecision) {
			if (precision == EDecimal.FixedPrecision) {
				precision = EDecimal.Precision;
			}
			else if (precision < -1) {
				precision = EDecimal.FullPrecision;
			}

			string result = "";

			// check for left comma
			if (a.VLen+a.FPos <= 0) {
				result += "0";
			}

			// build string
			int min = Math.Min(0, a.VLen+a.FPos);
			int max = Math.Max(a.VLen, a.VLen+a.FPos);
			int p = 0;
			bool f = false;
			for (int i = min; i<max; i++) {
				if (f && (precision >= 0) && (p >= precision)) {
					break;
				}
				if (i == (a.VLen+a.FPos)) {
					if (precision == 0) {
						break;
					}
					result += EDecimal.DecimalSeparator;
					f = true;
				}
				if (f) {
					p++;
				}
				if ((i >= 0) && (i < a.VLen)) {
					result += (Char)(a.Value[i]+48);
				}
				else {
					result += "0";
				}
			}

			// trim
			result = result.TrimStart(EDecimal.TrimCharZero);
			if (result.StartsWith(""+EDecimal.DecimalSeparator)) {
				result = "0"+result;
			}

			if (a.FPos < 0) {
				result = result.TrimEnd(EDecimal.TrimCharZero);
				result = result.TrimEnd(EDecimal.TrimCharComma);
			}

			// check empty
			if (result == "") {
				result = "0";
			}

			// sign
			if (a.IsNegative && (result != "0")) {
				result = "-"+result;
			}

			return result;
		}

		// comparison
		public static int CompareTo(EDecimal a, EDecimal b, int precision = EDecimal.DefaultPrecision) {
			if (precision == EDecimal.FixedPrecision) {
				precision = EDecimal.Precision;
			}
			else if (precision < -1) {
				precision = EDecimal.FullPrecision;
			}

			// check for Negative numbers
			int n = 1;
			if (a.IsNegative && !b.IsNegative) {
				return -1;
			}
			else if (!a.IsNegative && b.IsNegative) {
				return 1;
			}
			else if (a.IsNegative && b.IsNegative) {
				n = -1;
			}

			// if (comma is left to number) then just 1 digit: "0" --> 0.x
			int ILenA = (-a.FPos > a.VLen) ? 1 : a.VLen+a.FPos;
			int FLenA = (a.FPos < 0) ? -a.FPos : 0;

			int ILenB = (-b.FPos > b.VLen) ? 1 : b.VLen+b.FPos;
			int FLenB = (b.FPos < 0) ? -b.FPos : 0;

			int maxILen = Math.Max(ILenA, ILenB);
			int maxFLen = Math.Max(FLenA, FLenB);

			int StartA = maxILen+(-a.FPos-a.VLen);
			int StartB = maxILen+(-b.FPos-b.VLen);

			// init result
			int RLen = maxILen+maxFLen;

			bool f = false;
			int p = 0;

			int v1, v2, t;
			for (int i = 0; i <= RLen-1; i++) {
				if (f && (precision >= 0) && (p >= precision)) {
					break;
				}

				if (i == maxILen) {
					if (precision == 0) {
						break;
					}
					f = true;
				}
				if (f) {
					p++;
				}

				// get values
				t = i-StartA;
				v1 = ((t < a.VLen) && (t >= 0)) ? a.Value[t] : 0;
				t = i-StartB;
				v2 = ((t < b.VLen) && (t >= 0)) ? b.Value[t] : 0;

				// check value
				if (v1 > v2) {
					return n*1;
				}
				else if (v1 < v2) {
					return n*-1;
				}
			}

			return 0;
		}

		// helper functions
		public static EDecimal CommaLeft(EDecimal a, int n = 1) {
			a.FPos -= n;
			return a;
		}

		public static EDecimal CommaRight(EDecimal a, int n = 1) {
			a.FPos += n;
			return a;
		}

		// basic operations
		public static EDecimal Negate(EDecimal a) {
			a.IsNegative = !a.IsNegative;
			return a;
		}

		public static EDecimal Add(EDecimal a, EDecimal b, int precision = EDecimal.DefaultPrecision) {
			if (precision == EDecimal.FixedPrecision) {
				precision = EDecimal.Precision;
			}
			else if (precision < -1) {
				precision = EDecimal.FullPrecision;
			}

			// OPTIMIZATION
			// check simple
			if (IsZero(a)) {
				return b;
			}
			else if (IsZero(b)) {
				return a;
			}

			// check for Negative numbers
			bool neg = false;
			if (a.IsNegative && b.IsNegative) {
				a.IsNegative = false;
				b.IsNegative = false;

				neg = true;
			}
			else if (a.IsNegative && !b.IsNegative) {
				a.IsNegative = false;
				return Sub(b, a, precision);
			}
			else if (!a.IsNegative && b.IsNegative) {
				b.IsNegative = false;
				return Sub(a, b, precision);
			}

			// if (comma is left to number) then just 1 digit: "0" --> 0.x
			int ILenA = (-a.FPos >= a.VLen) ? 1 : a.VLen+a.FPos;
			int FLenA = (a.FPos < 0) ? -a.FPos : 0;

			int ILenB = (-b.FPos >= b.VLen) ? 1 : b.VLen+b.FPos;
			int FLenB = (b.FPos < 0) ? -b.FPos : 0;


			// set precision
			if (precision >= 0) {
				FLenA = Math.Min(FLenA, precision);
				FLenB = Math.Min(FLenB, precision);
			}


			int maxILen = Math.Max(ILenA, ILenB);
			int maxFLen = Math.Max(FLenA, FLenB);

			int carryPlace = 1;

			int StartA = maxILen+carryPlace+(-a.FPos-a.VLen);
			int StartB = maxILen+carryPlace+(-b.FPos-b.VLen);

			// init result
			int RLen = maxILen+maxFLen+carryPlace;

			string result = "";

			int r = 0, c = 0, v1, v2, t;
			for (int i = RLen-1; i>0; i--) {

				// set comma
				if ((i == maxILen) && (i != RLen-1)) {
					result = "."+result;
				}

				// get values
				t = i-StartA;
				v1 = ((t < a.VLen) && (t >= 0)) ? a.Value[t] : 0;
				t = i-StartB;
				v2 = ((t < b.VLen) && (t >= 0)) ? b.Value[t] : 0;

				// calculate result and carry
				r = v1+v2+c;
				if (r >= 10) {
					r = r-10;
					c = 1;
				}
				else {
					c = 0;
				}

				// add digit
				result = r+result;
			}
			// add carry
			if (c == 1) {
				result = "1"+result;
			}

			// both operands are negative
			if (neg) {
				result = '-'+result;
			}

			return new EDecimal(result);
		}

		public static EDecimal Sub(EDecimal a, EDecimal b, int precision = EDecimal.DefaultPrecision) {
			if (precision == EDecimal.FixedPrecision) {
				precision = EDecimal.Precision;
			}
			else if (precision < -1) {
				precision = EDecimal.FullPrecision;
			}

			// OPTIMIZATION
			// check simple
			if (IsZero(a)) {
				b.IsNegative = !b.IsNegative;
				return b;
			}
			else if (IsZero(b)) {
				return a;
			}

			// check for Negative numbers
			if (a.IsNegative && b.IsNegative) {
				a.IsNegative = false;
				b.IsNegative = false;
				return Sub(b, a, precision);
			}
			else if (a.IsNegative && !b.IsNegative) {
				a.IsNegative = false;
				EDecimal ncr = Add(a, b, precision);
				ncr.IsNegative = true;
				return ncr;
				// return Add(a.Negate(), b, precision).Negate();
			}
			else if (!a.IsNegative && b.IsNegative) {
				b.IsNegative = false;
				return Add(a, b, precision);
				// return Add(a, b.Negate(), precision);
			}

			// check order
			int comp = CompareTo(a, b, precision);
			if (comp == -1) {
				return Sub(b, a, precision).Negate();
			}
			else if (comp == 0) {
				return EDecimal.Zero;
			}

			// if (comma is left to number) then just 1 digit: "0" --> 0.x
			int ILenA = (-a.FPos >= a.VLen) ? 1 : a.VLen+a.FPos;
			int FLenA = (a.FPos < 0) ? -a.FPos : 0;

			int ILenB = (-b.FPos >= b.VLen) ? 1 : b.VLen+b.FPos;
			int FLenB = (b.FPos < 0) ? -b.FPos : 0;


			// set precision
			if (precision >= 0) {
				FLenA = Math.Min(FLenA, precision);
				FLenB = Math.Min(FLenB, precision);
			}


			int maxILen = Math.Max(ILenA, ILenB);
			int maxFLen = Math.Max(FLenA, FLenB);

			// TODO: get carry place
			int carryPlace = 1;

			int StartA = maxILen+carryPlace+(-a.FPos-a.VLen);
			int StartB = maxILen+carryPlace+(-b.FPos-b.VLen);

			// init result
			int RLen = maxILen+maxFLen+carryPlace;

			/* v2
			byte[] result = new byte[RLen];
			EDecimal res = new EDecimal();
			res.FPos = -maxFLen;
			res.VLen = RLen;
			/**/

			string result = ""; // v1

			int r = 0, c = 0, v1, v2, t;
			for (int i = RLen-1; i>0; i--) {

				// set comma
				// v1
				if (i == maxILen) {
					result = "."+result;
				}

				// get values
				t = i-StartA;
				v1 = ((t < a.VLen) && (t >= 0)) ? a.Value[t] : 0;
				t = i-StartB;
				v2 = ((t < b.VLen) && (t >= 0)) ? b.Value[t] : 0;

				// do calculation
				r = v1-v2-c;
				if (r < 0) {
					c = 1;
					r += 10;
				}
				else {
					c = 0;
				}

				// add digit
				// v2: result[i] = (byte) r; // v2
				result = r+result; // v1
			}

			// build final result
			// res.Value = result; // v2
			// return res; // v2

			return new EDecimal(result); // v1
		}

		// 0 <= b <= 9
		private static EDecimal Mul(EDecimal a, int b, int precision = EDecimal.DefaultPrecision) {
			if (precision == EDecimal.FixedPrecision) {
				precision = EDecimal.Precision;
			}
			else if (precision < -1) {
				precision = EDecimal.FullPrecision;
			}

			// OPTIMIZATION
			// check simple
			if (b == 0) {
				return EDecimal.Zero;
			}
			else if (b == 10) {
				a.FPos++;
				return a;
			}
			else if (b == 1) {
				return a;
			}
			else if (b == -1) {
				a.IsNegative = !a.IsNegative;
				return a;
			}

			// init result
			string result = "";

			// set precision
			int p = 0;
			if ((precision >= 0) && (a.FPos < 0)) {
				p = -a.FPos-precision;
			}

			int len = a.VLen+a.FPos;
			int cmove = (a.FPos < 0) ? a.FPos : 0;

			// build string
			int min = Math.Min(0, len);
			int max = Math.Max(a.VLen, len)-p-1;

			// do calculation
			int v1 = 0, c = 0, t;
			for (int i = max; i>=min; i--) {
				if ((i > max) && (i+1) == len) {
					result = "."+result;
				}

				// get value
				v1 = ((i >= 0) && (i < a.VLen)) ? a.Value[i] : 0;
				v1 = t = v1*b+c;

				// calculation
				c = 0;
				if (v1 >= 10) {
					v1 = v1%10;
					c = (t-v1)/10;
				}

				// add to result
				result = v1+result;
			}

			// check for carry
			if (c > 0) {
				result = c+result;
			}

			// operand is negative
			if (a.IsNegative) {
				result = '-'+result;
			}

			// buid result
			EDecimal fresult = new EDecimal(result);

			// restore comma position
			fresult.FPos += cmove;

			return fresult;
		}

		// TODO: precision
		public static EDecimal Mul(EDecimal a, EDecimal b, int precision = EDecimal.DefaultPrecision) {
			if (precision == EDecimal.FixedPrecision) {
				precision = EDecimal.Precision;
			}
			else if (precision < -1) {
				precision = EDecimal.FullPrecision;
			}

			// OPTIMIZATION
			// check simple
			if (IsZero(a) || IsZero(b)) {
				return EDecimal.Zero;
			}

			if (IsOne(a)) {
				if (a.IsNegative) {
					b.IsNegative = !b.IsNegative;
				}
				return b;
				// return (a.IsNegative) ? Negate(b) : b;
			}
			if (IsOne(b)) {
				if (b.IsNegative) {
					a.IsNegative = !a.IsNegative;
				}
				return a;
				// return (b.IsNegative) ? Negate(a) : a;
			}


			bool neg = false;

			// check for negative numbers
			if (a.IsNegative && b.IsNegative) {
				a.IsNegative = false;
				b.IsNegative = false;
				// return Mul(a.Negate(), b.Negate(), precision);
			}
			else if (a.IsNegative || b.IsNegative) {
				a.IsNegative = false;
				b.IsNegative = false;
				neg = true;
			}

			// init result
			EDecimal result = EDecimal.Zero;

			// build string
			int len = b.VLen+b.FPos;
			int min = Math.Min(0, len);
			int max = Math.Max(b.VLen, len);

			// move comma
			// int cmove = ((a.FPos < 0) ? a.FPos : 0) + ((b.FPos < 0) ? b.FPos : 0);
			int cmove = a.FPos+b.FPos;

			a.FPos = ((b.VLen<-b.FPos) ? -b.FPos : b.VLen)-1;
			b.FPos = 0;

			// do calculation
			int v2 = 0;
			for (int i = min; i<max; i++) {

				// get value
				v2 = ((i >= 0) && (i < b.VLen)) ? b.Value[i] : 0;

				// optimizations
				switch (v2) {
					// nothing to add
					case 0: {
						break;
					}
					// just addition
					case 1: {
						result = Add(result, a);
						break;
					}
					// multiply and add
					default: {
						result = Add(result, Mul(a, v2));
						break;
					}
				}

				// next pos
				a.FPos--;
			}

			// move comma back
			result.FPos += cmove;

			// negative?
			if (neg) {
				result.IsNegative = true;
			}

			return result;
		}

		public static EDecimal Div(EDecimal a, EDecimal b, int precision = EDecimal.DefaultPrecision, bool round = true) {

			// no full precision!
			if (precision < 0) {
				precision = EDecimal.Precision;
			}

			// EDecimal oa = a, ob = b;

			// OPTIMIZATION
			// check simple
			if (IsZero(b)) {
				throw new DivideByZeroException();
			}
			if (IsZero(a)) {
				return EDecimal.Zero;
			}

			if (IsOne(b)) {
				return (b.IsNegative) ? Negate(a) : a;
			}


			if (round) {
				precision++;
			}

			bool neg = false;

			// check for negative numbers
			if (a.IsNegative && b.IsNegative) {
				a.IsNegative = false;
				b.IsNegative = false;
				// return Div(a.Negate(), b.Negate(), precision);
			}
			else if (a.IsNegative || b.IsNegative) {
				a.IsNegative = false;
				b.IsNegative = false;
				neg = true;
			}


			// init result
			string result = (neg) ? "-" : "";

			// for precision check
			int ILenA = (-a.FPos >= a.VLen) ? 1 : a.VLen+a.FPos;


			// move comma ("b")
			int cmove = b.FPos;
			b.FPos = 0;

			// start at the very left (with one digit)
			// int aFPos = a.FPos;
			if (-a.FPos >= a.VLen) {
				cmove += (-a.FPos-a.VLen+1);
			}
			a.FPos = -a.VLen+1;
			

			EDecimal sum, tmp;

			byte t;
			int p = cmove, loop = 0;
			bool f = false;
			int comp = 0;

			while ((IsZero(a) == false) || (loop < ILenA)) {

				// check precision
				if (f == true) {
					if (p >= precision) {
						break;
					}
					p++;
				}

				comp = EDMath.CompareTo(a, b);
				// if (a >= b)
				if (comp >= 0) {

					// at least one
					t = 1;
					sum = b;

					// while (a >= sum)
					while ((t < 10) && (EDMath.CompareTo(a, tmp=EDMath.Add(sum, b,0),0) >= 0)) {
						sum = tmp;
						t++;
					}

					// check
					if (t >= 10) {
						throw new ArgumentOutOfRangeException();
					}

					result += t;
					a = EDMath.Sub(a, sum);

				}
				else {
					result += "0";
				}

				if (loop+1 == ILenA) {
					result += '.';
					f = true;
				}

				// next digit
				a.FPos++;

				loop++;
			}

			// build result
			EDecimal r = new EDecimal(result);

			// move comma
			r.FPos -= cmove;

			if (round) {
				return Round(r, precision-1);
			}
			return r;
		}
		public static EDecimal IDiv(EDecimal a, EDecimal b) {
			return Div(a, b, 0, false);
		}

		// checks
		public static bool IsOdd(EDecimal a) {
			// fractional number cannot be odd or even
			if (IsFloat(a)) {
				throw new ApplicationException("A fractional number cannot be odd or even");
			}

			// always even
			if (a.FPos > 0) {
				return false;
			}

			return ((a.FPos <= 0) && (-a.FPos<a.VLen) && (a.Value[a.VLen+a.FPos-1]%2 == 1));
		}
		public static bool IsEven(EDecimal a) {
			return !IsOdd(a);
		}

		public static bool IsInteger(EDecimal a) {
			if (a.FPos >= 0) {
				return true;
			}

			// check digits behind comma; must be zero
			for (int i = a.VLen+a.FPos; i<a.VLen; i++) {
				if ((i>=0) && (a.Value[i] != 0)) {
					return false;
				}
			}

			return true;
		}
		public static bool IsFloat(EDecimal a) {
			return !IsInteger(a);
		}

		// check if number is 0
		public static bool IsZero(EDecimal a) {
			foreach (int v in a.Value) {
				if (v != 0) {
					return false;
				}
			}
			return true;
		}

		// check if number is 1 or -1
		public static bool IsOne(EDecimal a) {
			int cnt = 0;
			foreach (int v in a.Value) {
				if (v == 1) {
					cnt++;
				}
				// no other values than 1 allowed
				// not more than one 1 allowed
				if ((cnt > 1) || ((v != 1) && (v != 0))) {
					return false;
				}
			}

			// c == 1
			int len = a.Value.Count();
			return ((a.FPos <= 0) && (-a.FPos < len) && (a.Value[len+a.FPos-1] == 1));
		}

		// extended functions
		public static EDecimal Sign(EDecimal a) {
			return (IsZero(a)) ? EDecimal.Zero : ((a.IsNegative) ? EDecimal.One.Negate() : EDecimal.One);
		}

		public static EDecimal Trunc(EDecimal a, int precision = 0) {
			return new EDecimal(ToString(a, precision));
		}

		public static EDecimal Floor(EDecimal a) {
			if (IsInteger(a)) {
				return a;
			}
			if (a.IsNegative == false) {
				return Trunc(a, 0);
			}
			return Sub(Trunc(a, 0), One);
		}
		public static EDecimal Ceiling(EDecimal a) {
			if (IsInteger(a)) {
				return a;
			}
			if (a.IsNegative == true) {
				return Trunc(a, 0);
			}
			return Add(Trunc(a, 0), One);
		}

		public static EDecimal Frac(EDecimal a, int precision = EDecimal.DefaultPrecision) {
			return new EDecimal(ToString(Sub(a,Trunc(a),precision), precision));
		}

		public static EDecimal Abs(EDecimal a) {
			a.IsNegative = false;
			return a;
		}

		public static EDecimal Round(EDecimal a, int precision = EDecimal.DefaultPrecision) {
			if (precision < 0) {
				return a;
			}

			// nothing to do
			if (a.FPos >= 0) {
				return a;
			}

			string result = "";

			// check for left comma
			if (a.VLen+a.FPos <= 0) {
				result += "0";
			}

			// build string
			int min = Math.Min(0, a.VLen+a.FPos-1);
			int max = Math.Max(a.VLen, a.VLen+a.FPos);
			int p = -a.FPos;
			byte c = 0, v = 0;

			for (int i = max-1; i>=min; i--) {

				// set decimal point
				if (p <= precision) {
					if (p == 0) {
						result = EDecimal.DecimalSeparator+result;
					}

					// extended round: while value+carry > 10
					if ((i >= 0) && (i < a.VLen) && (a.Value[i]+c < 10)) {
						result = (char)(a.Value[i]+c+48)+result;
						c = 0;
					}
					else {
						result = '0'+result;
						// c = 1;
					}
				}
				// basic round
				else if ((i >= 0) && (i < a.VLen)) {
					v = (byte) (a.Value[i]+c);
					if (v >= 5) {
						c = 1;
					}
					else {
						c = 0;
					}
				}
				else {
					result = "0" + result;
				}

				p--;
			}

			
			if (c == 1) {
				result = '1'+result;
			}

			// sign
			if (a.IsNegative && (result != "0")) {
				result = "-"+result;
			}

			return new EDecimal(result);
		}

		public static EDecimal Min(EDecimal a, EDecimal b, int precision = EDecimal.DefaultPrecision) {
			return (CompareTo(a, b, precision) <= 0) ? a : b;
		}

		public static EDecimal Max(EDecimal a, EDecimal b, int precision = EDecimal.DefaultPrecision) {
			return (CompareTo(a, b, precision) >= 0) ? a : b;
		}

		// fast integer exponent power
		// adapted from: http://stackoverflow.com/questions/101439/the-most-efficient-way-to-implement-an-integer-based-power-function-powint-int
		// TODO: do in loop
		public static EDecimal Pow(EDecimal a, int Exponent, int precision = EDecimal.DefaultPrecision, bool round = true) {
			if (precision == EDecimal.FixedPrecision) {
				precision = EDecimal.Precision;
			}
			else if (precision < -1) {
				precision = EDecimal.FullPrecision;
			}

			// make exponent positive
			if (Exponent < 0) {
				return Pow(Div(EDecimal.One, a, precision), -Exponent, precision, round);
			}

			if (round == false) {
				precision++;
			}

			// OPTIMIZATION
			// check simple
			if (Exponent == 0) {
				return EDecimal.One;
			}
			if (Exponent == 1) {
				return a;
			}


			EDecimal result = One;

			while (Exponent > 0) {
				if ((Exponent & 1) == 1) {
					result = Mul(result,a,precision);
				}
				Exponent >>= 1;
				a = Mul(a, a, precision);
			}

			if (round == false) {
				return result;
			}
			return Round(result,precision-1);
		}

		/*
		public static EDecimal Pow(EDecimal X, int a, int b, int precision = EDecimal.DefaultPrecision) {
			return Root(Pow(X, a, precision), b,precision);
		}
		/**/
		// sqrt with heron algorithm
		// adapted from: https://www.html.de/threads/php-c-heron-verfahren-mit-diversen-programmiersprachen.11474/
		public static EDecimal Sqrt(EDecimal X, int precision = EDecimal.DefaultPrecision) {
			
			// no full precision!
			if (precision < 0) {
				precision = EDecimal.Precision;
			}

			if (IsZero(X)) {
				return X;
			}
			if (X.IsNegative) {
				throw new ArithmeticException("Sqrt with negative radikand is not allowed.");
			}
			if (IsOne(X)) {
				return X;
			}



			// optimization
			if ((Abs(X) < new EDecimal(double.MaxValue)) && (precision <= 15)) {
				return Round(new EDecimal(Math.Sqrt(X.ToDouble())), precision);
			}



			EDecimal t = One, result = One, f = new EDecimal("0.5");
			precision++;
			do {
				t = result;
				// result = 0.5*(t+a/t)
				result = Mul(Add(t, Div(X, t, precision,false), precision), f, precision);
			} while (CompareTo(t, result, precision) != 0);

			return Round(result,precision-1);
		}

		// integer root
		// newton algorithm
		public static EDecimal Root(EDecimal X, int n, int precision = EDecimal.DefaultPrecision) {
			
			// no full precision!
			if (precision < 0) {
				precision = EDecimal.Precision;
			}

			if (n == 0) {
				return Zero;
			}
			else if (n == 1) {
				return X;
			}
			else if (n < 0) {
				return Pow(Div(One, X, precision), -n);
			}


			
			// optimization
			if ((Abs(X) < new EDecimal(double.MaxValue)) && (precision <= 15)) {
				return Round(new EDecimal(Math.Pow(X.ToDouble(), 1d/n)), precision);
			}
			


			precision++;

			EDecimal _n = new EDecimal(n);
			// EDecimal result = Div(X, new EDecimal(n), precision, false), t = X;
			EDecimal result = Div(X, Two, precision, false), t = Zero;
			EDecimal f = Div(One, _n, precision, false), g = Sub(_n, One);
			Console.WriteLine(result.ToString());
			Console.WriteLine(precision);
			do {
				t = result;
				result = Mul(f,Add(Mul(g,t, precision), Div(X,Pow(t, n-1,precision,false),precision,false), precision), precision);
				Console.WriteLine(result.ToString());
			} while (CompareTo(t,result,precision) != 0);

			Console.WriteLine(result.ToString());
			Console.WriteLine(Round(result, precision-1).ToString());

			return Round(result,precision-1);
		}


		// taylor series of e^x
		public static EDecimal Exp(EDecimal X, int precision = EDecimal.DefaultPrecision) {
			
			// no full precision!
			if (precision < 0) {
				precision = EDecimal.Precision;
			}

			if (IsZero(X)) {
				return One;
			}

			precision++;

			EDecimal k = One;
			EDecimal f = One;
			EDecimal _x = X;
			EDecimal l = Zero;

			EDecimal result = Add(X,k,precision);
			do {
				l = result;
				k = Add(k,One, precision);
				f = Mul(f,k,precision);
				_x = Mul(_x,X,precision);
				result = Add(result,Div(_x,f,precision,false),precision);
			} while (CompareTo(l,result, precision) != 0);

			return Round(result,precision-1);
		}

		// taylor series of log(x)
		public static EDecimal Log(EDecimal X, int precision = EDecimal.DefaultPrecision) {
			
			// no full precision!
			if (precision < 0) {
				precision = EDecimal.Precision;
			}

			if (X.IsNegative) {
				throw new ArithmeticException("Negative logarithm is not allowed.");
			}
			else if (IsZero(X)) {
				throw new ArithmeticException("Logarithm argument must be greater than zero.");
			}

			precision++;

			EDecimal k = One;
			EDecimal l = Zero;
			EDecimal t = Div(Sub(X, One, precision),Add(X,One, precision), precision);
			EDecimal t2 = Mul(t,t, precision);
			EDecimal tn = t;

			EDecimal result = t;
			do {
				l = result;

				k = Add(k,Two,precision);
				tn = Mul(tn,t2,precision);
				result = Add(  result,   Mul(  Div(One,k, precision,false), tn, precision), precision);
				Console.WriteLine(result.ToString());
			} while (CompareTo(l,result, precision) != 0);

			return Round(Mul(result, Two, precision),precision-1);
		}

		public static EDecimal Log(EDecimal X, EDecimal Base, int precision = EDecimal.DefaultPrecision) {
			return Div(Log(X,precision),Log(Base,precision),precision);
		}


		// taylor series of sin(x)
		public static EDecimal Sin(EDecimal X, int precision = EDecimal.DefaultPrecision) {
			
			// no full precision!
			if (precision < 0) {
				precision = EDecimal.Precision;
			}

			if (IsZero(X)) {
				return Zero;
			}

			precision++;

			EDecimal k = Zero;
			EDecimal result = X;
			EDecimal l = Zero;

			EDecimal t2 = Mul(X,X,precision);
			EDecimal tf = X;
			EDecimal f = One;

			do {
				l = result;
				k = Add(k,Two,precision); // k = k+2
				tf = Mul(tf,t2,precision); // x^(2k+1)
				f = Mul(f,Mul(k,Add(k,One,precision),precision),precision); // (2k+1)!
				f.IsNegative = !f.IsNegative;
                result = Add(result,Div(tf,f,precision,false), precision);
			} while (CompareTo(l, result, precision) != 0);

			return Round(result, precision-1);
		}

		// taylor series of cos(x)
		public static EDecimal Cos(EDecimal X, int precision = EDecimal.DefaultPrecision) {

			// no full precision!
			if (precision < 0) {
				precision = EDecimal.Precision;
			}

			if (IsZero(X)) {
				return One;
			}

			precision++;

			EDecimal k = One;
			k.IsNegative = true;

			EDecimal result = One;
			EDecimal l = Zero;

			EDecimal t2 = Mul(X, X, precision);
			EDecimal tf = One;
			EDecimal f = One;

			do {
				l = result;
				k = Add(k,Two,precision); // k = k+2
				tf = Mul(tf,t2,precision); // x^(2k)
				f = Mul(f,Mul(k,Add(k,One,precision),precision),precision); // (2k)!
				f.IsNegative = !f.IsNegative;
				result = Add(result, Div(tf, f, precision, false), precision);
			} while (CompareTo(l, result, precision) != 0);

			return Round(result, precision-1);
		}

		public static EDecimal Tan(EDecimal X, int precision = EDecimal.DefaultPrecision) {
			return Div(Sin(X, precision), Cos(X, precision), precision);
		}

		public static EDecimal Cot(EDecimal X, int precision = EDecimal.DefaultPrecision) {
			return Div(Cos(X, precision), Sin(X, precision), precision);
		}


		public static EDecimal E(int precision = EDecimal.DefaultPrecision) {
			return Exp(One, precision);
		}

		public static EDecimal Faculty(EDecimal X) {

			if (X.IsNegative) {
				throw new ArithmeticException("Faculty cannot be built with negative numbers.");
			}
			else if (X.IsFloat()) {
				throw new ArithmeticException("Faculty cannot be built with fractional numbers.");
			}
			else if (IsZero(X)) {
				return One;
			}

			EDecimal result = One;
			while (X.IsZero() == false) {
				result = Mul(result,X);
				X = Sub(X,One);
			}

			return result;
		}

		public static EDecimal Mod(EDecimal X, EDecimal Y, int precision = EDecimal.DefaultPrecision) {
			// return X - (int)(X / Y) * Y;
			return Sub(X,Mul(Div(X,Y,precision).Trunc(), Y,precision),precision);
		}



		// ####### incomplete implementations ################################################################################


		// general power

		public static EDecimal Pow(EDecimal X, EDecimal Exponent, int precision = EDecimal.DefaultPrecision) {
			if (precision < 0) {
				precision = EDecimal.Precision;
			}

			// base must not be negative if exponent if rational
			if (IsFloat(Exponent) && X.IsNegative) {
				throw new ArithmeticException("Exponent must be a positive integer or zero for negative base.");
			}

			// make exponent positive
			if (Exponent.IsNegative) {
				return Pow(Div(EDecimal.One, X, precision), Exponent.Negate(), precision);
			}


			// OPTIMIZATION
			// check simple
			if (IsZero(Exponent)) {
				return EDecimal.One;
			}
			if (IsOne(Exponent)) {
				return X;
			}


			if (IsZero(X)) {
				return X;
			}
			if (IsOne(X)) {
				return X;
			}

			if (IsInteger(Exponent)) {
				return Pow(X, Exponent.ToInt32(), precision);
			}


			// block for non integer exponent
			throw new ApplicationException("Power with float exponent is not yet implemented.");

			// return Exp(Mul(Log(X,precision), Exponent, precision),precision);
		}
		// general root
		public static EDecimal Root(EDecimal a, EDecimal b, int precision = EDecimal.DefaultPrecision) {
			if (precision < 0) {
				precision = EDecimal.Precision;
			}

			// OPTIMIZATION
			// check simple
			if (IsZero(a)) {
				return a;
			}
			if (IsZero(b)) {
				return EDecimal.One;
			}

			if (a.IsNegative) {
				throw new ApplicationException("Root exponent must be a positive integer for negative base.");
			}
			// make exponent positive
			if (b.IsNegative) {
				return Root(Div(EDecimal.One, a, precision), b.Negate(), precision);
			}

			if (IsOne(a) || IsOne(b)) {
				return a;
			}


			// root for integer exponents
			if (IsInteger(b)) {
				return Root(a, b.ToInt32(), precision);
			}


			// block for non integer exponent
			throw new ApplicationException("Root with float exponent is not yet implemented.");

			// return Pow(a,Div(EDecimal.One, b, precision),precision);
		}

	}

}