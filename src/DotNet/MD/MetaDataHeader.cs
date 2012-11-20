﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using dot10.IO;

namespace dot10.DotNet.MD {
	/// <summary>
	/// Represents the .NET metadata header
	/// </summary>
	/// <remarks><c>IMAGE_COR20_HEADER.MetaData</c> points to this header</remarks>
	public sealed class MetaDataHeader : FileSection {
		uint signature;
		ushort majorVersion;
		ushort minorVersion;
		uint reserved1;
		uint stringLength;
		string versionString;
		uint offset2ndPart;
		StorageFlags flags;
		byte reserved2;
		ushort streams;
		IList<StreamHeader> streamHeaders;

		/// <summary>
		/// Returns the signature (should be 0x424A5342)
		/// </summary>
		public uint Signature {
			get { return signature; }
		}

		/// <summary>
		/// Returns the major version
		/// </summary>
		public ushort MajorVersion {
			get { return majorVersion; }
		}

		/// <summary>
		/// Returns the minor version
		/// </summary>
		public ushort MinorVersion {
			get { return minorVersion; }
		}

		/// <summary>
		/// Returns the reserved dword (pointer to extra header data)
		/// </summary>
		public uint Reserved1 {
			get { return reserved1; }
		}

		/// <summary>
		/// Returns the version string length value
		/// </summary>
		public uint StringLength {
			get { return stringLength; }
		}

		/// <summary>
		/// Returns the version string
		/// </summary>
		public string VersionString {
			get { return versionString; }
		}

		/// <summary>
		/// Returns the flags (reserved)
		/// </summary>
		public StorageFlags Flags {
			get { return flags; }
		}

		/// <summary>
		/// Returns the reserved byte (padding)
		/// </summary>
		public byte Reserved2 {
			get { return reserved2; }
		}

		/// <summary>
		/// Returns the number of streams
		/// </summary>
		public ushort Streams {
			get { return streams; }
		}

		/// <summary>
		/// Returns all stream headers
		/// </summary>
		public IList<StreamHeader> StreamHeaders {
			get { return streamHeaders; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="reader">PE file reader pointing to the start of this section</param>
		/// <param name="verify">Verify section</param>
		/// <exception cref="BadImageFormatException">Thrown if verification fails</exception>
		public MetaDataHeader(IImageStream reader, bool verify) {
			SetStartOffset(reader);
			this.signature = reader.ReadUInt32();
			if (verify && this.signature != 0x424A5342)
				throw new BadImageFormatException("Invalid MetaData header signature");
			this.majorVersion = reader.ReadUInt16();
			this.minorVersion = reader.ReadUInt16();
			if (verify && !((majorVersion == 1 && minorVersion == 1) || (majorVersion == 0 && minorVersion >= 19)))
				throw new BadImageFormatException(string.Format("Unknown MetaData header version: {0}.{1}", majorVersion, minorVersion));
			this.reserved1 = reader.ReadUInt32();
			this.stringLength = reader.ReadUInt32();
			this.versionString = ReadString(reader, stringLength);
			this.offset2ndPart = (uint)(reader.Position - startOffset);
			this.flags = (StorageFlags)reader.ReadByte();
			if (verify && this.flags != 0)
				throw new BadImageFormatException(string.Format("Storage flags != 0 ({0})", this.flags));
			this.reserved2 = reader.ReadByte();
			this.streams = reader.ReadUInt16();
			this.streamHeaders = new StreamHeader[streams];
			for (int i = 0; i < streamHeaders.Count; i++)
				streamHeaders[i] = new StreamHeader(reader, verify);
			SetEndoffset(reader);
		}

		static string ReadString(IImageStream reader, uint maxLength) {
			long endPos = reader.Position + maxLength;
			if (endPos < reader.Position || endPos > reader.Length)
				throw new BadImageFormatException("Invalid MD version string");
			byte[] utf8Bytes = new byte[maxLength];
			uint i;
			for (i = 0; i < maxLength; i++) {
				byte b = reader.ReadByte();
				if (b == 0)
					break;
				utf8Bytes[i] = b;
			}
			reader.Position = endPos;
			return Encoding.UTF8.GetString(utf8Bytes, 0, (int)i);
		}
	}
}
