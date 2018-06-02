using System;
using System.IO;
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

		void CopyButton_Click(object sender, RoutedEventArgs e)
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

			bmp.Render(dv);

			Clipboard.SetImage(bmp);
		}

		void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			var viewModel = (TestViewModel)DataContext;
			var fileName  = $"{viewModel.Platform}.{viewModel.Name}.bmp";

			App.Root.ViewModel.Status = $"Saving {fileName}...";

			var width  = ActualWidth;
			var height = ActualHeight;

			var bmp = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Default);
			var dv  = new DrawingVisual();

			using (var dc = dv.RenderOpen())
				dc.DrawRectangle(new VisualBrush(this), null, new Rect(new Point(), new Size(width, height)));

			bmp.Render(dv);

			var frame   = BitmapFrame.Create(bmp);
			var encoder = new BmpBitmapEncoder();

			encoder.Frames.Add(frame);

			using (var stream = File.Create(fileName))
				encoder.Save(stream);

			App.Root.ViewModel.Status = "";
		}
	}
}
