namespace PropertiesDotNet.Utils
{
	/// <summary>
	/// Provides functions for generating hash codes.
	/// </summary>
	internal static class HashCodeHelper
	{
		/// <summary>
		/// Uses a custom hashing algorithm to generate the hash code of the given objects.
		/// </summary>
		/// <param name="args">The objects to get the hash code of.</param>
		/// <returns>The hash code.</returns>
		public static int GenerateHashCode(params object?[]? args) =>  GenerateHashCode<object?>(args);

		/// <summary>
		/// Uses a custom hashing algorithm to generate the hash code of the given objects.
		/// </summary>
		/// <param name="args">The objects to get the hash code of.</param>
		/// <returns>The hash code.</returns>
		public static int GenerateHashCode<T>(params T[]? args) 
		{
			if (args is null || args.Length <= 0)
				return 0;
				
			int hash = 0;

			for (int i = 0; i < args.Length; i++)
			{
				int item2 = args[i] is null ? 0 : args[i].GetHashCode();
                hash = (hash << 5) + hash ^ item2;
			}

			return hash;
		}
	}
}
