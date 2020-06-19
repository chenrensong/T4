using System;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal class WeakConcurrentRandom
	{
		/// <summary>
		/// Generator singleton.
		/// </summary>
		private static WeakConcurrentRandom random;

		/// <summary>
		/// Index of the last used random number within pre-generated array.
		/// </summary>
		private int index;

		/// <summary>
		/// Count of segments of random numbers.
		/// </summary>
		private int segmentCount;

		/// <summary>
		/// Number of random numbers per segment.
		/// </summary>
		private int segmentSize;

		/// <summary>
		/// Number of bits used to store index of the random number within segment.
		/// </summary>
		private int bitsToStoreRandomIndexWithinSegment;

		/// <summary>
		/// Bit mask to get segment index bits.
		/// </summary>
		private int segmentIndexMask;

		/// <summary>
		/// Bit mask to get index of the random number within segment.
		/// </summary>
		private int randomIndexWithinSegmentMask;

		/// <summary>
		/// Bit mask to get index of the random number in the pre-generated array.
		/// </summary>
		private int randomArrayIndexMask;

		/// <summary>
		/// Array of random number batch generators (one per each segment).
		/// </summary>
		private IRandomNumberBatchGenerator[] randomGemerators;

		/// <summary>
		/// Array of pre-generated random numbers.
		/// </summary>
		private ulong[] randomNumbers;

		public static WeakConcurrentRandom Instance
		{
			get
			{
				if (random != null)
				{
					return random;
				}
				Interlocked.CompareExchange(ref random, new WeakConcurrentRandom(), null);
				return random;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.WeakConcurrentRandom" /> class.
		/// </summary>
		public WeakConcurrentRandom()
		{
			Initialize();
		}

		/// <summary>
		/// Initializes generator with a set of random numbers.
		/// </summary>
		public void Initialize()
		{
			Initialize((ulong seed) => new XorshiftRandomBatchGenerator(seed), 3, 10);
		}

		/// <summary>
		/// Initializes generator with a set of random numbers.
		/// </summary>
		/// <param name="randomGeneratorFactory">Factory used to create random number batch generators.</param>
		/// <param name="segmentIndexBits">Number of significant bits in segment index, i.e. value of 3 means 8 segments of random numbers - 0..7.</param>
		/// <param name="segmentBits">Number of significant bits in random number index within segment, i.e. value of 10 means 1024 random numbers per segment.</param>
		public void Initialize(Func<ulong, IRandomNumberBatchGenerator> randomGeneratorFactory, int segmentIndexBits, int segmentBits)
		{
			int num = segmentIndexBits;
			if (segmentIndexBits < 1 || segmentIndexBits > 4)
			{
				num = 3;
			}
			int num2 = segmentBits;
			if (segmentBits < 7 || segmentBits > 15)
			{
				num2 = 9;
			}
			bitsToStoreRandomIndexWithinSegment = num2;
			segmentCount = 1 << num;
			segmentSize = 1 << num2;
			segmentIndexMask = segmentCount - 1 << bitsToStoreRandomIndexWithinSegment;
			randomIndexWithinSegmentMask = segmentSize - 1;
			randomArrayIndexMask = (segmentIndexMask | randomIndexWithinSegmentMask);
			int num3 = segmentCount * segmentSize;
			randomGemerators = new IRandomNumberBatchGenerator[segmentCount];
			XorshiftRandomBatchGenerator xorshiftRandomBatchGenerator = new XorshiftRandomBatchGenerator((ulong)Environment.TickCount);
			ulong[] array = new ulong[segmentCount];
			xorshiftRandomBatchGenerator.NextBatch(array, 0, segmentCount);
			for (int i = 0; i < segmentCount; i++)
			{
				Func<ulong, IRandomNumberBatchGenerator> func = (ulong seed) => new XorshiftRandomBatchGenerator(seed);
				IRandomNumberBatchGenerator randomNumberBatchGenerator = (randomGeneratorFactory == null) ? func(array[i]) : (randomGeneratorFactory(array[i]) ?? func(array[i]));
				randomGemerators[i] = randomNumberBatchGenerator;
			}
			randomNumbers = new ulong[num3];
			xorshiftRandomBatchGenerator.NextBatch(randomNumbers, 0, num3);
		}

		/// <summary>
		/// Weakly thread safe next (random) operation id generator
		/// where 'weakly' indicates that it is unlikely we'll get into
		/// collision state.
		/// </summary>
		/// <returns>Next operation id.</returns>
		public ulong Next()
		{
			int num = Interlocked.Increment(ref index);
			if ((num & randomIndexWithinSegmentMask) == 0)
			{
				RegenerateSegment(num);
			}
			return randomNumbers[num & randomArrayIndexMask];
		}

		/// <summary>
		/// Generates random number batch for segment which just exhausted
		/// according to value of the new index.
		/// </summary>
		/// <param name="newIndex">Index in random number array of the random number we're about to return.</param>
		private void RegenerateSegment(int newIndex)
		{
			int num = ((newIndex & segmentIndexMask) != 0) ? (((newIndex & segmentIndexMask) >> bitsToStoreRandomIndexWithinSegment) - 1) : (segmentCount - 1);
			randomGemerators[num].NextBatch(randomNumbers, num * segmentSize, segmentSize);
		}
	}
}
