using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PerformanceTest.Views.TestControl
{
	public partial class TestControl : UserControl
	{
		public TestControl()
		{
			InitializeComponent();
		}

		RenderTargetBitmap CreateBitmap()
		{
			var width  = ActualWidth;
			var height = ActualHeight;

			var bmp = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Default);
			var dv  = new DrawingVisual();

			using (var dc = dv.RenderOpen())
			{
				var vb = new VisualBrush(this);
				dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
			}

			ButtonPanel.Visibility = Visibility.Hidden;
			bmp.Render(dv);
			ButtonPanel.Visibility = Visibility.Visible;

			return bmp;
		}

		void CopyButton_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetImage(CreateBitmap());
		}

		internal void SaveButton_Click(object sender, RoutedEventArgs? e)
		{
			var basePath = Path.GetDirectoryName(GetType().Assembly.Location)!;

			while (!Directory.Exists(Path.Combine(basePath, "Result")))
				basePath = Path.GetDirectoryName(basePath)!;

			var viewModel = (TestViewModel)DataContext;
			var fileName  = Path.Combine(basePath, "Result", $"{viewModel.Platform}.{viewModel.Name}.png");

			App.Root.ViewModel.Status = $"Saving {fileName}...";

			var frame   = BitmapFrame.Create(CreateBitmap());
			var encoder = new BmpBitmapEncoder();

			encoder.Frames.Add(frame);

			using (var stream = File.Create(fileName))
				encoder.Save(stream);

			App.Root.ViewModel.Status = "";
		}
	}
}
