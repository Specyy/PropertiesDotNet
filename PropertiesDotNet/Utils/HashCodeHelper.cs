namespace PropertiesDotNet.Utils
{
	/// <summary>
	/// Provides functions for generating hash codes.
	/// </summary>
	internal static class HashCodeHelper
	{
		private static void CreateHashCode(ref int item1, ref int item2)
		{
			item1 = (item1 << 5) + item1 ^ item2; // multiplies by 31 and adds item2 ?
		}

		/// <summary>
		/// Uses a custom hashing algorithm to generate the hash code of the given objects.
		/// </summary>
		/// <param name="args">The objects to get the hash code of.</param>
		/// <returns>The hash code.</returns>
		public static int GenerateHashCode(params object?[]? args)
		{
			return GenerateHashCode<object?>(args);
		}

		/// <summary>
		/// Uses a custom hashing algorithm to generate the hash code of the given objects.
		/// </summary>
		/// <param name="args">The objects to get the hash code of.</param>
		/// <returns>The hash code.</returns>
		public static int GenerateHashCode<T>(params T[] args) 
		{
			if (args == null || args.Length <= 0)
				return 0;

			var hash = args[0] == null ? 0 : args[0].GetHashCode();

			for (var i = 1; i < args.Length; i++)
			{
				var item2 = args[i] == null ? 0 : args[i].GetHashCode();
				CreateHashCode(ref hash, ref item2);
			}

			return hash;
		}
	}
}
