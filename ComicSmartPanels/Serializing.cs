using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace ComicSmartPanels
{
	class Serializing
	{
		public class CPDFileFormat
		{
			public int CPDVersion { get { return 1; } }

			public string ComicBookFile { get; private set; }
			public string ComicMD5 { get; private set; }

			public string Author { get; set; }
			public string Comments { get; set; }
			public DateTime FileDate { get; set; }

			public List<SerializedComicPage> PanelDefinitions { get; set; }

			public CPDFileFormat(string pathToComicFile, string author)
			{
				using (var md5 = MD5.Create())
				{
					using (var stream = File.OpenRead(pathToComicFile))
					{
						var bytes = md5.ComputeHash(stream);

						StringBuilder result = new StringBuilder(bytes.Length * 2);

						for (int i = 0; i < bytes.Length; i++)
							result.Append(bytes[i].ToString("x2"));

						ComicMD5 = result.ToString();
					}
				}

				this.ComicBookFile = Path.GetFileName(pathToComicFile);
				this.Author = author;
				this.Comments = string.Empty;
				this.FileDate = DateTime.Now;

				this.PanelDefinitions = new List<SerializedComicPage>();
			}
		}

		public class SerializedComicPage
		{
			public int ImageIndex { get; private set; }
			public string ImageName { get; private set; }
			public List<SerializedComicPanel> Panels { get; private set; }
			public int PanelCount { get { return Panels.Count; } }

			public SerializedComicPage(int imageIndex, string imageName)
			{
				this.ImageIndex = imageIndex;
				this.ImageName = imageName;
				this.Panels = new List<SerializedComicPanel>();
			}

		}

		public class SerializedComicPanel
		{
			public bool IsRectangle { get; set; }
			public RectangleF RelativePosition { get; set; }
			public int Page { get; set; }
			public int PanelNum { get; set; }

			public SerializedComicPanel(bool isRectangle, RectangleF relativePos, int page, int panelNum)
			{
				this.IsRectangle = isRectangle;
				this.RelativePosition = relativePos;
				this.Page = page;
				this.PanelNum = panelNum;
			}

		}


		public class CPRFileFormat
		{
			public int CPRVersion { get { return 1; } }

			public string Author { get; set; }
			public string Comments { get; set; }

			public string RootFolder { get; set; }
			public string ComicFile { get; set; }

			public int LastWorkingPage { get; set; }
			public bool UseGrid { get; set; }
			public int GridSize { get; set; }

			public List<ComicPage> Pages { get; set; }

		}

	}

}
