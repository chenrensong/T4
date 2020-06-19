namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// Generates batches of random number using Xorshift algorithm
	/// Note: the base code is from http://www.codeproject.com/Articles/9187/A-fast-equivalent-for-System-Random.
	/// </summary>
	internal class XorshiftRandomBatchGenerator : IRandomNumberBatchGenerator
	{
		private const ulong Y = 4477743899113974427uL;

		private const ulong Z = 2994213561913849757uL;

		private const ulong W = 9123831478480964153uL;

		private ulong lastX;

		private ulong lastY;

		private ulong lastZ;

		private ulong lastW;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.XorshiftRandomBatchGenerator" /> class.
		/// </summary>
		/// <param name="seed">Random generator seed value.</param>
		public XorshiftRandomBatchGenerator(ulong seed)
		{
			lastX = seed * 5073061188973594169L + seed * 8760132611124384359L + seed * 8900702462021224483L + seed * 6807056130438027397L;
			lastY = 4477743899113974427uL;
			lastZ = 2994213561913849757uL;
			lastW = 9123831478480964153uL;
		}

		/// <summary>
		/// Generates a batch of random numbers.
		/// </summary>
		/// <param name="buffer">Buffer to put numbers in.</param>
		/// <param name="index">Start index in the buffer.</param>
		/// <param name="count">Count of random numbers to generate.</param>
		public void NextBatch(ulong[] buffer, int index, int count)
		{
			ulong num = lastX;
			ulong num2 = lastY;
			ulong num3 = lastZ;
			ulong num4 = lastW;
			for (int i = 0; i < count; i++)
			{
				ulong num5 = num ^ (num << 11);
				num = num2;
				num2 = num3;
				num3 = num4;
				num4 = (buffer[index + i] = (num4 ^ (num4 >> 19) ^ (num5 ^ (num5 >> 8))));
			}
			lastX = num;
			lastY = num2;
			lastZ = num3;
			lastW = num4;
		}
	}
}
