using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	[CLSCompliant(true)]
	public interface ITextTemplatingSession : IEquatable<ITextTemplatingSession>, IEquatable<Guid>, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, ISerializable
	{
		Guid Id
		{
			get;
		}
	}
}
