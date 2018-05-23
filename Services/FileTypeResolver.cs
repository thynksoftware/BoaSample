
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Boa.Sample.Services
{
    public static class FileTypeResolver
    {
        #region Constants

		// file headers are taken from here:
		//http://www.garykessler.net/library/file_sigs.html
		//mime types are taken from here:
		//http://www.freeformatter.com/mime-types-list.html
		// MS Office files
		public static readonly FileType Word = new FileType(new byte?[] { 0xEC, 0xA5, 0xC1, 0x00 }, 512, "doc", "application/msword");
		public static readonly FileType Excel = new FileType(new byte?[] { 0x09, 0x08, 0x10, 0x00, 0x00, 0x06, 0x05, 0x00 }, 512, "xls", "application/excel");
		public static readonly FileType Ppt = new FileType(new byte?[] { 0xFD, 0xFF, 0xFF, 0xFF, null, 0x00, 0x00, 0x00 }, 512, "ppt", "application/mspowerpoint");

		public static readonly FileType Wordx = new FileType(new byte?[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00 }, "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
		public static readonly FileType Excelx = new FileType(new byte?[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00 }, "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
		public static readonly FileType Pptx = new FileType(new byte?[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00 }, "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");

		// common documents
		public static readonly FileType Rtf = new FileType(new byte?[] { 0x7B, 0x5C, 0x72, 0x74, 0x66, 0x31 }, "rtf", "application/rtf");
		public static readonly FileType Pdf = new FileType(new byte?[] { 0x25, 0x50, 0x44, 0x46 }, "pdf", "application/pdf");

		// graphics
		public static readonly FileType Jpg = new FileType(new byte?[] { 0xFF, 0xD8, 0xFF }, "jpg", "image/jpeg");
		public static readonly FileType Jpeg = new FileType(new byte?[] { 0xFF, 0xD8, 0xFF }, "jpeg", "image/jpeg");
		public static readonly FileType Png = new FileType(new byte?[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, "png", "image/png");
		public static readonly FileType Gif = new FileType(new byte?[] { 0x47, 0x49, 0x46, 0x38, null, 0x61 }, "gif", "image/gif");
		public static readonly FileType Bmp = new FileType(new byte?[] { 0x42, 0x4D }, "bmp", "image/bmp");
		public static readonly FileType Tif = new FileType(new byte?[] { 0x49, 0x49, 0x2A, 0x00 }, "tif", "image/tiff");


		public static readonly FileType Zip = new FileType(new byte?[] { 0x50, 0x4B, 0x03, 0x04 }, "zip", "application/x-compressed");
		public static readonly FileType Rar = new FileType(new byte?[] { 0x52, 0x61, 0x72, 0x21 }, "rar", "application/x-compressed");
		public static readonly FileType Exe = new FileType(new byte?[] { 0x4D, 0x5A }, "exe", "application/octet-stream");
		public static readonly FileType Msdoc = new FileType(new byte?[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, "", "application/octet-stream");

		// all the file types to be put into one list
		private static readonly List<FileType> Types = new List<FileType> { 
            Pdf, Word, Excel, Wordx, Excelx, Pptx, Jpeg, Zip, Rar, Rtf, Png, Ppt, Gif, Exe, Msdoc, Bmp, Tif, Jpg};

		// number of bytes we read from a file
		private const int MaxHeaderSize = 560;  // some file formats have headers offset to 512 bytes

		#endregion

		#region Main Methods

		/// <summary>
		/// Read header of a file and depending on the information in the header
		/// return object FileType.
		/// Return null in case when the file type is not identified. 
		/// Throws Application exception if the file can not be read or does not exist
		/// </summary>
		/// <param name="file">The FileInfo object.</param>
		/// <returns>FileType or null not identified</returns>
		public static FileType GetFileType(this FileInfo file)
		{
			// read first n-bytes from the file
			var fileHeader = ReadFileHeader(file, MaxHeaderSize);
			return GetFileTypeForHeader(fileHeader);
		}

		public static FileType GetFileTypeFromStream(this Stream fileStream)
		{
			// read first n-bytes from the stream
			var fileHeader = ReadFileStreamHeader(fileStream);
			return GetFileTypeForHeader(fileHeader);
		}

		public static FileType GetFileTypeForHeader(IReadOnlyList<byte> fileHeader)
		{
			// compare the file header to the stored file headers
			foreach (FileType type in Types)
			{
				int matchingCount = 0;
				for (int i = 0; i < type.Header.Length; i++)
				{
					// if file offset is not set to zero, we need to take this into account when comparing.
					// if byte in type.header is set to null, means this byte is variable, ignore it
					if (type.Header[i] != null && type.Header[i] != fileHeader[i + type.HeaderOffset])
					{
						// if one of the bytes does not match, move on to the next type
						matchingCount = 0;
						break;
					}
					matchingCount++;
				}
				if (matchingCount == type.Header.Length)
				{
					// if all the bytes match, return the type
					return type;
				}
			}
			// if none of the types match, return null
			return null;
		}

	    /// <summary>
	    /// Reads the file header - first (16) bytes from the file
	    /// </summary>
	    /// <param name="file">The file to work with</param>
	    /// <param name="maxHeaderSize"></param>
	    /// <returns>Array of bytes</returns>
	    private static byte[] ReadFileHeader(FileSystemInfo file, int maxHeaderSize)
		{
			var header = new byte[maxHeaderSize];
			try  // read file
			{
				using (var fsSource = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
				{
					// read first symbols from file into array of bytes.
					fsSource.Read(header, 0, maxHeaderSize);
				}   // close the file stream

			}
			catch (Exception e) // file could not be found/read
			{
				throw new Exception("Could not read file : " + e.Message);
			}

			return header;
		}

		private static byte[] ReadFileStreamHeader(Stream fileStream)
		{
			var header = new byte[MaxHeaderSize];
			fileStream.Read(header, 0, MaxHeaderSize);
			return header;
		}

		/// <summary>
		/// Determines whether provided file belongs to one of the provided list of files
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="requiredTypes">The required types.</param>
		/// <returns>
		///   <c>true</c> if file of the one of the provided types; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsFileOfTypes(this FileInfo file, List<FileType> requiredTypes)
		{
			var currentType = file.GetFileType();

			if (null == currentType)
			{
				return false;
			}

			return requiredTypes.Contains(currentType);
		}

		public static bool IsFileOfTypes(this Stream file, List<FileType> requiredTypes)
		{
			var currentType = file.GetFileTypeFromStream();

			if (null == currentType)
			{
				return false;
			}

			return requiredTypes.Contains(currentType);
		}

		public static bool IsFileOfType(this Stream file, FileType requiredType)
		{
			var fileHeader = ReadFileStreamHeader(file);
			return !requiredType.Header.Where((t, i) => fileHeader[i + requiredType.HeaderOffset] != t).Any();
		}

	    /// <summary>
	    /// Determines whether provided file belongs to one of the provided list of files,
	    /// where list of files provided by string with Comma-Separated-Values of extensions
	    /// </summary>
	    /// <param name="file">The file.</param>
	    /// <param name="csv"></param>
	    /// <returns>
	    ///   <c>true</c> if file of the one of the provided types; otherwise, <c>false</c>.
	    /// </returns>
	    public static bool IsFileOfTypes(this FileInfo file, string csv)
		{
			var providedTypes = GetFileTypesByExtensions(csv);

			return file.IsFileOfTypes(providedTypes);
		}


		public static bool IsFileOfType(this Stream file, string extension)
		{
			var providedType = GetFileTypesByExtensions(extension).FirstOrDefault();

			return providedType != null && file.IsFileOfType(providedType);
		}
		/// <summary>
		/// Gets the list of FileTypes based on list of extensions in Comma-Separated-Values string
		/// </summary>
		/// <param name="csv">The CSV String with extensions</param>
		/// <returns>List of FileTypes</returns>
		private static List<FileType> GetFileTypesByExtensions(string csv)
		{
			var extensions = csv.ToUpper().Replace(" ", "").Split(',');

			var result = new List<FileType>();

			foreach (var type in Types)
			{
				if (extensions.Contains(type.Extension.ToUpper()))
				{
					result.Add(type);
				}
			}
			return result;
		}

		#endregion

		#region isType functions


		/// <summary>
		/// Determines whether the specified file is of provided type
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="type">The FileType</param>
		/// <returns>
		///   <c>true</c> if the specified file is type; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsType(this FileInfo file, FileType type)
		{
			var actualType = GetFileType(file);

			if (null == actualType)
				return false;

			return (actualType.Equals(type));
		}

		/// <summary>
		/// Determines whether the specified file is PDF.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns>
		///   <c>true</c> if the specified file is PDF; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsPdf(this FileInfo file)
		{
			return file.IsType(Pdf);
		}


		/// <summary>
		/// Determines whether the specified file info is ms-word document file
		/// </summary>
		/// <param name="fileInfo">The file info.</param>
		/// <returns>
		///   <c>true</c> if the specified file info is doc; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsWord(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Word);
		}


		/// <summary>
		/// Determines whether the specified file is zip archive
		/// </summary>
		/// <param name="fileInfo">The file info.</param>
		/// <returns>
		///   <c>true</c> if the specified file info is zip; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsZip(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Zip);
		}

		/// <summary>
		/// Determines whether the specified file is MS Excel spreadsheet
		/// </summary>
		/// <param name="fileInfo">The FileInfo</param>
		/// <returns>
		///   <c>true</c> if the specified file info is excel; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsExcel(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Excel);
		}

		/// <summary>
		/// Determines whether the specified file is JPEG image
		/// </summary>
		/// <param name="fileInfo">The FileInfo.</param>
		/// <returns>
		///   <c>true</c> if the specified file info is JPEG; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsJpeg(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Jpeg);
		}

		/// <summary>
		/// Determines whether the specified file is RAR-archive.
		/// </summary>
		/// <param name="fileInfo">The FileInfo.</param>
		/// <returns>
		///   <c>true</c> if the specified file info is RAR; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsRar(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Rar);
		}

		/// <summary>
		/// Determines whether the specified file is RTF document.
		/// </summary>
		/// <param name="fileInfo">The FileInfo.</param>
		/// <returns>
		///   <c>true</c> if the specified file is RTF; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsRtf(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Rtf);
		}

		/// <summary>
		/// Determines whether the specified file is PNG.
		/// </summary>
		/// <param name="fileInfo">The FileInfo object</param>
		/// <returns>
		///   <c>true</c> if the specified file info is PNG; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsPng(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Png);
		}

		/// <summary>
		/// Determines whether the specified file is Microsoft PowerPoint Presentation
		/// </summary>
		/// <param name="fileInfo">The FileInfo object.</param>
		/// <returns>
		///   <c>true</c> if the specified file info is PPT; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsPpt(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Ppt);
		}

		/// <summary>
		/// Determines whether the specified file is GIF image
		/// </summary>
		/// <param name="fileInfo">The FileInfo object</param>
		/// <returns>
		///   <c>true</c> if the specified file info is GIF; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsGif(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Gif);
		}


		/// <summary>
		/// Checks if the file is executable
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static bool IsExe(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Exe);
		}


		/// <summary>
		/// Check if the file is Microsoft Installer.
		/// Beware, many Microsoft file types are starting with the same header. 
		/// So use this one with caution. If you think the file is MSI, just need to confirm, use this method. 
		/// But it could be MSWord or MSExcel, or Powerpoint... 
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static bool IsMsi(this FileInfo fileInfo)
		{
			// MSI has a generic DOCFILE header. Also it matches PPT files
			return fileInfo.IsType(Ppt) || fileInfo.IsType(Msdoc);
		}


		/// <summary>
		/// Determines whether the specified file is DOCX.
		/// </summary>
		/// <param name="fileInfo">The FileInfo object</param>
		/// <returns>
		///   <c>true</c> if the specified file info is DOCX; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsDocx(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Wordx);
		}

		/// <summary>
		/// Determines whether the specified file is XLSX.
		/// </summary>
		/// <param name="fileInfo">The FileInfo object</param>
		/// <returns>
		///   <c>true</c> if the specified file info is XLSX; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsXlsx(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Excelx);
		}

		/// <summary>
		/// Determines whether the specified file is BMP.
		/// </summary>
		/// <param name="fileInfo">The FileInfo object</param>
		/// <returns>
		///   <c>true</c> if the specified file info is BMP; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsBmp(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Bmp);
		}

		/// <summary>
		/// Determines whether the specified file is TIF/TIFF.
		/// </summary>
		/// <param name="fileInfo">The FileInfo object</param>
		/// <returns>
		///   <c>true</c> if the specified file info is TIF/TIFF; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsTiff(this FileInfo fileInfo)
		{
			return fileInfo.IsType(Tif);
		}
		#endregion
    }
}