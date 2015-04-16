using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;


public static class Extensions
{

	public static void ChangeSource(this Image image, ImageSource source, TimeSpan fadeOutTime, TimeSpan fadeInTime)
	{
		var fadeInAnimation = new DoubleAnimation(1d, fadeInTime);

		if (image.Source != null)
		{
			var fadeOutAnimation = new DoubleAnimation(0d, fadeOutTime);

			fadeOutAnimation.Completed += (o, e) =>
			{
				image.Source = source;
				image.BeginAnimation(Image.OpacityProperty, fadeInAnimation);
			};

			image.BeginAnimation(Image.OpacityProperty, fadeOutAnimation);
		}
		else
		{
			image.Opacity = 0d;
			image.Source = source;
			image.BeginAnimation(Image.OpacityProperty, fadeInAnimation);
		}
	}

}
