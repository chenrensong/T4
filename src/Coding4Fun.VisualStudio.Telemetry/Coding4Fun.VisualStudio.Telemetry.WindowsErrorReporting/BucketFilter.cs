using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	/// <summary>
	/// Used to affect the behavior of one or more fault events based on matching bucketer paremeter filters.
	/// For example, a fault event whose buckete parameters match the filter values can have its report to
	/// Watson disabled, or can have a process dump added.
	/// </summary>
	public sealed class BucketFilter
	{
		private static string[] bucketParameterNames = new string[10]
		{
			"P1",
			"P2",
			"P3",
			"P4",
			"P5",
			"P6",
			"P7",
			"P8",
			"P9",
			"P10"
		};

		public Dictionary<string, string> AdditionalProperties = new Dictionary<string, string>();

		/// <summary>
		///  Gets or sets the ID of the bucket filter.
		/// </summary>
		public Guid Id
		{
			get;
			set;
		}

		/// <summary>
		///  Gets or sets the Watson event type associated with the bucket filter (ex. VisualStudioNonFatalErrors2).
		/// </summary>
		public string WatsonEventType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the bucket parameter filters used to match against a fault event's bucket parameters.
		/// The filter values are treated as regular expressions.
		/// </summary>
		public string[] BucketParameterFilters
		{
			get;
			set;
		} = new string[10];


		/// <summary>
		/// Gets the index of the specified bucket parameter name (ex. the index of "P1" would be 0).
		/// </summary>
		/// <param name="bucketParameterName">The name of the bucket parameter whose index is to be returned.</param>
		/// <returns>The index of the bucket parameter if the name is valid, or -1 if not.</returns>
		public static int IndexOfBucketParameter(string bucketParameterName)
		{
			return Array.IndexOf(bucketParameterNames, bucketParameterName);
		}

		/// <summary>
		/// Constructs a BucketFilter object.
		/// </summary>
		/// <param name="id">The ID (a guid) of the bucket filter.</param>
		/// <param name="watsonEventType">The Watson event type of the bucket filter. Ex. VisualStudioNonFatalErrors2.</param>
		public BucketFilter(Guid id, string watsonEventType)
		{
			Id = id;
			WatsonEventType = watsonEventType;
		}
	}
}
