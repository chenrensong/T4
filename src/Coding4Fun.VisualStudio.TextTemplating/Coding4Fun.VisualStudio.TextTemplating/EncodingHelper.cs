using System;
using System.IO;
using System.Text;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Helper class to get the encoding of a file from its BOM
	/// </summary>
	public static class EncodingHelper
	{
		/// <summary>
		/// Helper method to get the encoding of a file from its BOM
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static Encoding GetEncoding(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}
			Encoding encoding = Encoding.Default;
			if (!File.Exists(filePath))
			{
				return encoding;
			}
			try
			{
				using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					if (fileStream.Length > 0)
					{
						using (StreamReader streamReader = new StreamReader(fileStream, true))
						{
							char[] buffer = new char[1];
							streamReader.Read(buffer, 0, 1);
							encoding = streamReader.CurrentEncoding;
							streamReader.BaseStream.Position = 0L;
							if (encoding == Encoding.UTF8)
							{
								byte[] preamble = encoding.GetPreamble();
								if (fileStream.Length >= preamble.Length)
								{
									byte[] array = new byte[preamble.Length];
									fileStream.Read(array, 0, array.Length);
									for (int i = 0; i < array.Length; i++)
									{
										if (array[i] != preamble[i])
										{
											encoding = Encoding.Default;
											break;
										}
									}
								}
								else
								{
									encoding = Encoding.Default;
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				if (Engine.IsCriticalException(e))
				{
					throw;
				}
			}
			return encoding ?? Encoding.UTF8;
		}
	}
}
