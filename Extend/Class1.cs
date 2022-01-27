using System;
using System.Linq;

namespace Extend {
	public static class Extend {
		public static bool Contains(this string input, string[] filter) {
			return filter.All(input.Contains);
		}

		public static bool Contains(this string input, char[] filter) {
			return filter.All(input.Contains);
		}

		public static string Repeat(this string input, int count) {
			var v = string.Empty;
			for (var i = 0; i < count; i++) {
				v += input;
			}

			return v;
		}
	}
}