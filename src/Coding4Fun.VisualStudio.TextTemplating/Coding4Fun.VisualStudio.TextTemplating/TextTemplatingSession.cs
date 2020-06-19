using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Trivial implementation of text transformation session interface
	/// </summary>
	[Serializable]
	public sealed class TextTemplatingSession : Dictionary<string, object>, ITextTemplatingSession, IEquatable<ITextTemplatingSession>, IEquatable<Guid>, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, ISerializable
	{
		private readonly Guid id;

		private const string idName = "__TextTransformationSession_Id";

		/// <summary>
		/// The identity of the session
		/// </summary>
		public Guid Id => id;

		/// <summary>
		/// Basic constructor that creates a unique Id.
		/// </summary>
		public TextTemplatingSession()
		{
			id = Guid.NewGuid();
		}

		/// <summary>
		/// Constructor to allow a specific Id to be used
		/// </summary>
		/// <remarks>
		/// Potential use for other serialization schemes
		/// </remarks>
		public TextTemplatingSession(Guid id)
		{
			this.id = id;
		}

		public bool Equals(ITextTemplatingSession other)
		{
			if (other != null)
			{
				return Equals(other.Id);
			}
			return false;
		}

		public bool Equals(Guid other)
		{
			return Id.CompareTo(other) == 0;
		}

		public override bool Equals(object obj)
		{
			ITextTemplatingSession textTemplatingSession = obj as ITextTemplatingSession;
			if (textTemplatingSession != null)
			{
				return Equals(textTemplatingSession);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		/// <summary>
		/// Serialization constructor
		/// </summary>
		private TextTemplatingSession(SerializationInfo information, StreamingContext context)
			: base(information, context)
		{
			id = (Guid)information.GetValue("__TextTransformationSession_Id", typeof(Guid));
		}

		/// <summary>
		/// Serialize the object
		/// </summary>
		void ISerializable.GetObjectData(SerializationInfo information, StreamingContext context)
		{
			information.AddValue("__TextTransformationSession_Id", id, typeof(Guid));
			base.GetObjectData(information, context);
		}
	}
}
