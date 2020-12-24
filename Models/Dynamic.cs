using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Threading.Tasks;

namespace DangerZoneHackerTracker
{
	/// <summary>
	/// acts as a strongly typed dynamic primitive, can implicitly convert between floats, ints, bools, doubles, Colors, and strings.
	/// Underlying data structure is a string
	/// </summary>
	public class Dynamic : IConvertible, IComparable
	{
		public double UpperBounds = double.MaxValue;
		public double LowerBounds = double.MinValue;

		public string Name { get; set; }

		private readonly object defaultValue;
		public string DefaultValue { get => defaultValue.ToString(); }

		private string _value;
		public string Value
		{
			get => _value;
			set
			{
				if(value != _value)
				{
					var temp = value;
					_value = value;
					ConvarChanged?.Invoke(this, temp, _value);
				}
			}
		}

		public bool BoolValue
		{
			get
			{
				double.TryParse(_value, out double result);
				result = Math.Clamp(result, LowerBounds, UpperBounds);
				return result != 0;
			}
			set
			{
				value = this.DoubleValue != 0;
				if (BoolValue != value)
				{
					var temp = _value;
					_value = value.ToString();
					ConvarChanged?.Invoke(this, temp, _value);
				}
			}
		}

		public int IntValue
		{
			get
			{
				int.TryParse(_value, out int result);
				result = (int)Math.Clamp(result, LowerBounds, UpperBounds);
				return result;
			}
			set
			{
				value = (int)Math.Clamp(value, LowerBounds, UpperBounds);
				if (IntValue != value)
				{
					var temp = _value;
					_value = value.ToString();
					ConvarChanged?.Invoke(this, temp, _value);
				}
			}
		}
		public long LongValue
		{
			get
			{
				long.TryParse(_value, out long result);
				result = (long)Math.Clamp(result, LowerBounds, UpperBounds);
				return result;
			}
			set
			{
				value = (long)Math.Clamp(value, LowerBounds, UpperBounds);
				if (LongValue != value)
				{
					var temp = _value;
					_value = value.ToString();
					ConvarChanged?.Invoke(this, temp, _value);
				}
			}
		}

		public ulong ULongValue
		{
			get
			{
				ulong.TryParse(_value, out ulong result);
				result = (ulong)Math.Clamp(result, LowerBounds, UpperBounds);
				return result;
			}
			set
			{
				value = (ulong)Math.Clamp(value, LowerBounds, UpperBounds);
				if (ULongValue != value)
				{
					var temp = _value;
					_value = value.ToString();
					ConvarChanged?.Invoke(this, temp, _value);
				}
			}
		}

		public float FloatValue
		{
			get
			{
				float.TryParse(_value, out float result);
				result = (float)Math.Clamp(result, LowerBounds, UpperBounds);

				return result;
			}
			set
			{
				value = (float)Math.Clamp(value, LowerBounds, UpperBounds);
				if (FloatValue != value)
				{
					var temp = _value;
					_value = value.ToString();
					ConvarChanged?.Invoke(this, temp, _value);
				}
			}
		}

		public double DoubleValue
		{
			get
			{
				double.TryParse(_value, out double result);
				result = Math.Clamp(result, LowerBounds, UpperBounds);

				return result;
			}
			set
			{
				value = Math.Clamp(value, LowerBounds, UpperBounds);
				if(DoubleValue != value)
				{
					var temp = _value;
					_value = value.ToString();
					ConvarChanged?.Invoke(this, temp, _value);
				}
			}
		}

		public Dynamic(int value, string description = "",
				double min = double.MinValue, double max = double.MaxValue)
		{
			this.defaultValue = _value = value.ToString();
			this.LowerBounds = min;
			this.UpperBounds = max;
		}

		public Dynamic(float value, string description = "",
				double min = double.MinValue, double max = double.MaxValue)
		{
			this.defaultValue = _value = value.ToString();
			this.LowerBounds = min;
			this.UpperBounds = max;
		}

		public Dynamic(long value, string description = "",
			double min = double.MinValue, double max = double.MaxValue)
		{
			this.defaultValue = _value = value.ToString();
			this.LowerBounds = min;
			this.UpperBounds = max;
		}

		public Dynamic(double value, string description = "",
				double min = double.MinValue, double max = double.MaxValue)
		{
			this.defaultValue = _value = value.ToString();
			this.LowerBounds = min;
			this.UpperBounds = max;
		}

		public Dynamic(string value, string description = "",
				double min = double.MinValue, double max = double.MaxValue)
		{
			this.defaultValue = _value = value;
			this.LowerBounds = min;
			this.UpperBounds = max;
		}
		public Dynamic(bool value, string description = "",
				double min = double.MinValue, double max = double.MaxValue)
		{
			this.defaultValue = _value = (value? 1: 0).ToString();
			this.LowerBounds = min;
			this.UpperBounds = max;
		}

		public void RestoreDefault()
		{
			this.Value = this.defaultValue.ToString();
		}

		public static implicit operator int(Dynamic Convar) => Convar.IntValue;
		public static implicit operator float(Dynamic Convar) => Convar.FloatValue;
		public static implicit operator double(Dynamic Convar) => Convar.DoubleValue;
		public static implicit operator string(Dynamic Convar) => Convar.Value;
		public static implicit operator bool(Dynamic Convar) => Convar.BoolValue;
		public static implicit operator long(Dynamic Convar) => Convar.LongValue;

		public static implicit operator Dynamic(int value) => new Dynamic(value);
		public static implicit operator Dynamic(float value) => new Dynamic(value);
		public static implicit operator Dynamic(double value) => new Dynamic(value);
		public static implicit operator Dynamic(string value) => new Dynamic(value);
		public static implicit operator Dynamic(bool value) => new Dynamic(value);
		public static implicit operator Dynamic(long value) => new Dynamic(value);


		public static bool operator true(Dynamic x) => x.BoolValue;
		public static bool operator false(Dynamic x) => !x.BoolValue;


		public delegate void ConvarChangedHandler(Dynamic sender, string oldValue, string newValue);
		public static ConvarChangedHandler ConvarChanged;

		public override string ToString()
		{
			return _value;
		}

		public TypeCode GetTypeCode() => TypeCode.Object;


		public bool ToBoolean(IFormatProvider provider) => BoolValue;


		public byte ToByte(IFormatProvider provider) => (byte)IntValue;

		public char ToChar(IFormatProvider provider) => Value[0];

		public DateTime ToDateTime(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public decimal ToDecimal(IFormatProvider provider)
		{
			decimal.TryParse(Value, out decimal ret);
			return ret;
		}

		public double ToDouble(IFormatProvider provider) => DoubleValue;

		public short ToInt16(IFormatProvider provider) => (short)IntValue;

		public int ToInt32(IFormatProvider provider) => IntValue;

		public long ToInt64(IFormatProvider provider) => LongValue;

		public sbyte ToSByte(IFormatProvider provider) => (sbyte)IntValue;

		public float ToSingle(IFormatProvider provider) => FloatValue;

		public string ToString(IFormatProvider provider) => Value;

		public object ToType(Type toType, IFormatProvider provider)
		{
			var method = this.GetType().GetMethods().Where(m => m.Name == $"To{toType.Name}");
			if(method == null || method.Count() == 0)
			{
				throw new NotImplementedException();
			}

			return method.First().Invoke(this, null);
		}

		public ushort ToUInt16(IFormatProvider provider) => (ushort)IntValue;

		public uint ToUInt32(IFormatProvider provider) => (uint)ULongValue;

		public ulong ToUInt64(IFormatProvider provider) => ULongValue;

		public int CompareTo(object obj)
		{
			return Value.CompareTo(obj);
		}
	}
}