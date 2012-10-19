﻿using System;
using dot10.DotNet.MD;

namespace dot10.DotNet {
	/// <summary>
	/// A high-level representation of a row in the File table
	/// </summary>
	public abstract class FileDef : IHasCustomAttribute, IImplementation {
		/// <summary>
		/// The row id in its table
		/// </summary>
		protected uint rid;

		/// <inheritdoc/>
		public MDToken MDToken {
			get { return new MDToken(Table.File, rid); }
		}

		/// <inheritdoc/>
		public uint Rid {
			get { return rid; }
			set { rid = value; }
		}

		/// <inheritdoc/>
		public int HasCustomAttributeTag {
			get { return 16; }
		}

		/// <inheritdoc/>
		public int ImplementationTag {
			get { return 0; }
		}

		/// <summary>
		/// From column File.Flags
		/// </summary>
		public abstract FileAttributes Flags { get; set; }

		/// <summary>
		/// From column File.Name
		/// </summary>
		public abstract UTF8String Name { get; set; }

		/// <summary>
		/// From column File.HashValue
		/// </summary>
		public abstract byte[] HashValue { get; set; }

		/// <summary>
		/// Gets all custom attributes
		/// </summary>
		public abstract CustomAttributeCollection CustomAttributes { get; }

		/// <summary>
		/// Gets/sets the <see cref="FileAttributes.ContainsMetaData"/> bit
		/// </summary>
		public bool ContainsMetaData {
			get { return (Flags & FileAttributes.ContainsNoMetaData) == 0; }
			set {
				if (value)
					Flags &= ~FileAttributes.ContainsNoMetaData;
				else
					Flags |= FileAttributes.ContainsNoMetaData;
			}
		}

		/// <summary>
		/// Gets/sets the <see cref="FileAttributes.ContainsNoMetaData"/> bit
		/// </summary>
		public bool ContainsNoMetaData {
			get { return (Flags & FileAttributes.ContainsNoMetaData) != 0; }
			set {
				if (value)
					Flags |= FileAttributes.ContainsNoMetaData;
				else
					Flags &= ~FileAttributes.ContainsNoMetaData;
			}
		}

		/// <inheritdoc/>
		public string FullName {
			get { return UTF8String.ToSystemStringOrEmpty(Name); }
		}

		/// <inheritdoc/>
		public override string ToString() {
			return FullName;
		}
	}

	/// <summary>
	/// A File row created by the user and not present in the original .NET file
	/// </summary>
	public class FileDefUser : FileDef {
		FileAttributes flags;
		UTF8String name;
		byte[] hashValue;
		CustomAttributeCollection customAttributeCollection = new CustomAttributeCollection();

		/// <inheritdoc/>
		public override FileAttributes Flags {
			get { return flags; }
			set { flags = value; }
		}

		/// <inheritdoc/>
		public override UTF8String Name {
			get { return name; }
			set { name = value; }
		}

		/// <inheritdoc/>
		public override byte[] HashValue {
			get { return hashValue; }
			set { hashValue = value; }
		}

		/// <inheritdoc/>
		public override CustomAttributeCollection CustomAttributes {
			get { return customAttributeCollection; }
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public FileDefUser() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of file</param>
		/// <param name="flags">Flags</param>
		/// <param name="hashValue">File hash</param>
		public FileDefUser(string name, FileAttributes flags, byte[] hashValue)
			: this(new UTF8String(name), flags, hashValue) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of file</param>
		/// <param name="flags">Flags</param>
		/// <param name="hashValue">File hash</param>
		public FileDefUser(UTF8String name, FileAttributes flags, byte[] hashValue) {
			this.name = name;
			this.flags = flags;
			this.hashValue = hashValue;
		}
	}

	/// <summary>
	/// Created from a row in the File table
	/// </summary>
	sealed class FileDefMD : FileDef {
		/// <summary>The module where this instance is located</summary>
		ModuleDefMD readerModule;
		/// <summary>The raw table row. It's <c>null</c> until <see cref="InitializeRawRow"/> is called</summary>
		RawFileRow rawRow;

		UserValue<FileAttributes> flags;
		UserValue<UTF8String> name;
		UserValue<byte[]> hashValue;
		CustomAttributeCollection customAttributeCollection;

		/// <inheritdoc/>
		public override FileAttributes Flags {
			get { return flags.Value; }
			set { flags.Value = value; }
		}

		/// <inheritdoc/>
		public override UTF8String Name {
			get { return name.Value; }
			set { name.Value = value; }
		}

		/// <inheritdoc/>
		public override byte[] HashValue {
			get { return hashValue.Value; }
			set { hashValue.Value = value; }
		}

		/// <inheritdoc/>
		public override CustomAttributeCollection CustomAttributes {
			get {
				if (customAttributeCollection == null) {
					var list = readerModule.MetaData.GetCustomAttributeRidList(Table.File, rid);
					customAttributeCollection = new CustomAttributeCollection((int)list.Length, list, (list2, index) => readerModule.ReadCustomAttribute(((RidList)list2)[index]));
				}
				return customAttributeCollection;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="readerModule">The module which contains this <c>File</c> row</param>
		/// <param name="rid">Row ID</param>
		/// <exception cref="ArgumentNullException">If <paramref name="readerModule"/> is <c>null</c></exception>
		/// <exception cref="ArgumentException">If <paramref name="rid"/> is invalid</exception>
		public FileDefMD(ModuleDefMD readerModule, uint rid) {
#if DEBUG
			if (readerModule == null)
				throw new ArgumentNullException("readerModule");
			if (readerModule.TablesStream.Get(Table.File).IsInvalidRID(rid))
				throw new BadImageFormatException(string.Format("File rid {0} does not exist", rid));
#endif
			this.rid = rid;
			this.readerModule = readerModule;
			Initialize();
		}

		void Initialize() {
			flags.ReadOriginalValue = () => {
				InitializeRawRow();
				return (FileAttributes)rawRow.Flags;
			};
			name.ReadOriginalValue = () => {
				InitializeRawRow();
				return readerModule.StringsStream.ReadNoNull(rawRow.Name);
			};
			hashValue.ReadOriginalValue = () => {
				InitializeRawRow();
				return readerModule.BlobStream.Read(rawRow.HashValue);
			};
		}

		void InitializeRawRow() {
			if (rawRow != null)
				return;
			rawRow = readerModule.TablesStream.ReadFileRow(rid);
		}
	}
}
