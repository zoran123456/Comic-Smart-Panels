using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Archive;
using System.Windows.Forms;

namespace ComicSmartPanels
{
	public class FileFormatReader
	{
		public enum ComicBookFormat
		{
			Unknown = 0,
			CBZ,
			CBR
		}

		public string PathToComicFile { get; set; }

		public ComicBookFormat ComicFileFormat { get; set; }

		public string[] ExtractedFiles
		{
			get;
			private set;
		}

		public FileFormatReader(string pathToComicFile, ComicBookFormat comicFileFormat)
		{
			this.PathToComicFile = pathToComicFile;
			this.ComicFileFormat = comicFileFormat;
		}

		public static ComicBookFormat GetComicBookFormat(string filename)
		{
			string ext = System.IO.Path.GetExtension(filename).ToLower();

			FileFormatReader.ComicBookFormat fileFormat = ComicBookFormat.Unknown;

			switch (ext)
			{
				case ".cbz":
				case ".zip":
					fileFormat = FileFormatReader.ComicBookFormat.CBZ;
					break;

				case ".cbr":
				case ".rar":
					fileFormat = FileFormatReader.ComicBookFormat.CBR;
					break;
			}

			return fileFormat;
		}

		private void emptyDirectory(string path)
		{
			var dinfo = new DirectoryInfo(path);

			foreach (System.IO.FileInfo file in dinfo.GetFiles()) file.Delete();
			foreach (System.IO.DirectoryInfo subDirectory in dinfo.GetDirectories()) subDirectory.Delete(true);
		}

		public void ExtractToFolder(string folder)
		{
			string appLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path = Path.Combine(appLocation, folder);

			// Remove all files and folders in destination folder
			try
			{
				emptyDirectory(path);
			}
			catch (System.IO.IOException ex)
			{
				string msg = "Cannot delete all files in Temp directory ('{0}').\n\n Application error message:\n{1}";
				msg = string.Format(msg, path, ex.Message);

				MessageBox.Show(msg, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				return;
			}

			// Extract files from Archive
			switch (ComicFileFormat)
			{
				case ComicBookFormat.Unknown:
					break;

				case ComicBookFormat.CBZ:
				case ComicBookFormat.CBR:
					using (var compressed = ArchiveFactory.Open(PathToComicFile))
					{
						foreach (var entry in compressed.Entries)
						{
							if (!entry.IsDirectory)
							{
								entry.WriteToDirectory(folder, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
							}
						}
					}
					break;
			}

			// Read extracted files
			string[] files;

			files = Directory.GetFiles(path);
			if (files.Length == 0)
			{
				// No files in root, must be Archive with folder (in which files are)
				string[] dirs = Directory.GetDirectories(path);

				if (dirs.Length == 0)
				{
					// Empty Archive. Now what?
				}
				else
				{
					files = Directory.GetFiles(dirs[0]);

					if (files.Length == 0)
					{
						// Empty directory. Now what?
					}
				}
			}

			// Hopefully this would be good enough
			Array.Sort(files);

			ExtractedFiles = files;

		}

	}
}
