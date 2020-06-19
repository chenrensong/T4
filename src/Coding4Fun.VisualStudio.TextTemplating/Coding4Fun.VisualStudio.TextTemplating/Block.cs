namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Represents a block in the template file
	/// </summary>
	internal sealed class Block
	{
		/// <summary>
		/// The type of the block
		/// </summary>
		public BlockType Type
		{
			get;
			set;
		}

		/// <summary>
		/// The text contained within the block, not
		/// including the opening and closing tags
		/// </summary>
		public string Text
		{
			get;
			set;
		}

		/// <summary>
		/// The line number on which the block text starts
		/// </summary>
		public int StartLineNumber
		{
			get;
			set;
		}

		/// <summary>
		/// The column number on which the block text starts
		/// </summary>
		public int StartColumnNumber
		{
			get;
			set;
		}

		/// <summary>
		/// The line number on which the block text ends
		/// </summary>
		public int EndLineNumber
		{
			get;
			set;
		}

		/// <summary>
		/// The column number on which the block text ends
		/// </summary>
		public int EndColumnNumber
		{
			get;
			set;
		}

		/// <summary>
		/// The file name that this block comes from (in the case that
		/// it comes from an included file). Empty string if the
		/// block doesn't come from an include file.
		/// </summary>
		public string FileName
		{
			get;
			set;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="type"></param>
		/// <param name="text"></param>
		public Block(BlockType type, string text)
		{
			FileName = "";
			Type = type;
			Text = text;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public Block()
		{
			FileName = "";
			Text = "";
		}
	}
}
