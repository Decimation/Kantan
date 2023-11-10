using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.Utilities;

public static class HashHelper
{
	public static class Sha256
	{
		public static string ToString(byte[] h, bool lower = true)
		{
			var stringBuilder = new StringBuilder();

			foreach (byte b in h)
				stringBuilder.Append($"{b:X2}");

			string hashString = stringBuilder.ToString();

			if (lower) {
				hashString = hashString.ToLower();
			}

			return hashString;
		}

		public static byte[] ToBytes(string inputString)
		{
			using HashAlgorithm algorithm = SHA256.Create();

			return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}
	}
}