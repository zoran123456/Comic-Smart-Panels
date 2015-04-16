using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComicPanelReader
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public string ComicFilename { get; set; }
		public string CPDFile { get; set; }

		public POCO.CPDFileFormat PanelsInfo { get; set; }
		public List<string> ComicPages { get; set; }

		public int CurrentPageIndex { get; set; }
		public int CurrentPanelIndex { get; set; }

		public bool DisplayWholePageOnPageExit { get; set; }

		public MainWindow()
		{
			InitializeComponent();
		}


		private void About_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Comic Panel Reader. For testing purposes only.\n\nDo not resale this software.\n\n\nCopyright Zoran Bosnjak");
		}

		private void Bye_Click(object sender, RoutedEventArgs e)
		{

			App.Current.Shutdown();

		}


		async void StartReadComic()
		{
			DisplayWholePageOnPageExit = miDisplayWholePageOnPageExit.IsChecked;

			Cursor = Cursors.Wait;

			string json = File.ReadAllText(CPDFile, Encoding.UTF8);

			if (ComicPages == null)
				ComicPages = new List<string>();

			ComicPages.Clear();
			CurrentPageIndex = 0;
			CurrentPanelIndex = 0;

			await Task.Run(() =>
			{
				PanelsInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<POCO.CPDFileFormat>(json);

				var comicFileFormat = FileFormatReader.GetComicBookFormat(ComicFilename);
				FileFormatReader rdr = new FileFormatReader(ComicFilename, comicFileFormat);

				rdr.ExtractToFolder("Temp");
				foreach (string page in rdr.ExtractedFiles)
					ComicPages.Add(page);

			});

			Cursor = Cursors.Arrow;

			DisplayNextPanel();

		}

		private WriteableBitmap GetCroppedPart(WriteableBitmap source, RectangleF relativeRect)
		{
			double width = source.Width;
			double height = source.Height;

			double rleft = relativeRect.Left * width / 100;
			double rtop = relativeRect.Top * height / 100;
			double rwidth = relativeRect.Width * width / 100;
			double rheight = relativeRect.Height * height / 100;

			Rect r = new Rect(rleft, rtop, rwidth, rheight);

			return source.Crop(r);
		}

		public static BitmapImage BitmapFromUri(Uri source)
		{
			var bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = source;
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.EndInit();
			return bitmap;
		}

		private TimeSpan panelFadeOut = TimeSpan.FromSeconds(0.5);
		private TimeSpan panelFadeIn = TimeSpan.FromSeconds(0.5);

		private void DisplayNextPanel()
		{

			if (PanelsInfo.PanelDefinitions[CurrentPageIndex].PanelCount > 0)
			{

				byte[] bts = File.ReadAllBytes(ComicPages[CurrentPageIndex]);

				WriteableBitmap bmp = new WriteableBitmap(BitmapFromUri(new Uri(ComicPages[CurrentPageIndex])));

				bmp = GetCroppedPart(bmp, PanelsInfo.PanelDefinitions[CurrentPageIndex].Panels[CurrentPanelIndex].RelativePosition);

				ComicPresenter.ChangeSource(bmp, panelFadeOut, panelFadeIn);

			}
			else
			{

				var bmp = BitmapFromUri(new Uri(ComicPages[CurrentPageIndex]));
				ComicPresenter.ChangeSource(bmp, panelFadeOut, panelFadeIn);

			}
		}

		private bool currentPageDisplayed = false;
		private void DisplayWholeCurrentPage()
		{
			currentPageDisplayed = true;

			var bmp = BitmapFromUri(new Uri(ComicPages[CurrentPageIndex]));
			ComicPresenter.ChangeSource(bmp, panelFadeOut, panelFadeIn);
		}

		private void OpenComic_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "CBR files (*.cbr)|*.cbr|CBZ files (*.cbz)|*.cbz";
			ofd.Title = "Open Comic Book File";

			bool? ok = ofd.ShowDialog();
			if (ok.HasValue && ok.Value)
			{
				ComicFilename = ofd.FileName;

				OpenFileDialog ofdPanel = new OpenFileDialog();
				ofdPanel.Filter = "CPD files (*.cpd)|*.cpd";
				ofdPanel.Title = "Open Comic Panel Definitions";

				bool? ok2 = ofdPanel.ShowDialog();
				if (ok2.HasValue && ok.Value)
				{
					CPDFile = ofdPanel.FileName;

					StartReadComic();
				}

			}

		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(ComicFilename) ||
				string.IsNullOrWhiteSpace(CPDFile) ||
				ComicPages == null ||
				ComicPages.Count == 0)
			{
				return;
			}

			if (e.Key == Key.Left)
			{
				currentPageDisplayed = false;

				CurrentPanelIndex -= 1;
				if (CurrentPanelIndex < 0)
				{
					CurrentPanelIndex = PanelsInfo.PanelDefinitions[CurrentPageIndex - 1].PanelCount - 1;
					if (CurrentPanelIndex < 0) CurrentPanelIndex = 0;

					CurrentPageIndex -= 1;
					if (CurrentPageIndex < 0) CurrentPageIndex = 0;
				}


				DisplayNextPanel();
			}
			else if (e.Key == Key.Right)
			{
				CurrentPanelIndex += 1;
				if (CurrentPanelIndex > PanelsInfo.PanelDefinitions[CurrentPageIndex].PanelCount - 1)
				{
					if (DisplayWholePageOnPageExit &&
						!currentPageDisplayed &&
						PanelsInfo.PanelDefinitions[CurrentPageIndex].PanelCount > 0)
					{
						DisplayWholeCurrentPage();
						return;
					}

					currentPageDisplayed = false;

					CurrentPanelIndex = 0;

					CurrentPageIndex += 1;
					if (CurrentPageIndex > ComicPages.Count - 1) CurrentPageIndex = ComicPages.Count - 1;
				}


				DisplayNextPanel();
			}
		}

		private void PageExitCheck_Click(object sender, RoutedEventArgs e)
		{

			DisplayWholePageOnPageExit = ((MenuItem)sender).IsChecked;

		}


		private void LeftArrow_Click(object sender, RoutedEventArgs e)
		{
			KeyEventArgs kea = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(this), 0, Key.Left);

			Window_KeyDown(null, kea);
		}

		private void RightArrow_Click(object sender, RoutedEventArgs e)
		{
			KeyEventArgs kea = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(this), 0, Key.Right);

			Window_KeyDown(null, kea);
		}

	}
}
