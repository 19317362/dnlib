﻿using dot10.IO;

namespace dot10.DotNet.MD {
	/// <summary>
	/// Represents the #Strings stream
	/// </summary>
	public sealed class StringsStream : DotNetStream {
		/// <inheritdoc/>
		public StringsStream() {
		}

		/// <inheritdoc/>
		public StringsStream(IImageStream imageStream, StreamHeader streamHeader)
			: base(imageStream, streamHeader) {
		}

		/// <summary>
		/// Reads a <see cref="UTF8String"/>
		/// </summary>
		/// <param name="offset">Offset of string</param>
		/// <returns>A <see cref="UTF8String"/> instance or <c>null</c> if invalid offset</returns>
		public UTF8String Read(uint offset) {
			if (offset >= imageStream.Length)
				return null;
			imageStream.Position = offset;
			var data = imageStream.ReadBytesUntilByte(0);
			if (data == null)
				return null;
			return new UTF8String(data);
		}

		/// <summary>
		/// Reads a <see cref="UTF8String"/>. The empty string is returned if <paramref name="offset"/>
		/// is invalid.
		/// </summary>
		/// <param name="offset">Offset of string</param>
		/// <returns>A <see cref="UTF8String"/> instance</returns>
		public UTF8String ReadNoNull(uint offset) {
			return Read(offset) ?? UTF8String.Empty;
		}
	}
}
