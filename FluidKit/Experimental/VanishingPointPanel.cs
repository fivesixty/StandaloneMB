using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FluidKit.Experimental
{
	public class VanishingPointPanel : Panel
	{
		private const double ItemHeight = 30;
		private int _selectedIndex = -1;


		private static readonly DependencyProperty IsVanishedProperty = DependencyProperty.RegisterAttached(
			"IsVanished", typeof (bool), typeof (VanishingPointPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentArrange));

		public static readonly DependencyProperty XFactorProperty = DependencyProperty.Register(
			"XFactor", typeof(double), typeof(VanishingPointPanel), new FrameworkPropertyMetadata(0.5D, FrameworkPropertyMetadataOptions.AffectsArrange));

		public static readonly DependencyProperty ZFactorProperty = DependencyProperty.Register(
			"ZFactor", typeof(double), typeof(VanishingPointPanel), new FrameworkPropertyMetadata(0.8D, FrameworkPropertyMetadataOptions.AffectsArrange));

		public double ZFactor
		{
			get { return (double)GetValue(ZFactorProperty); }
			set { SetValue(ZFactorProperty, value); }
		}

		public double XFactor
		{
			get { return (double)GetValue(XFactorProperty); }
			set { SetValue(XFactorProperty, value); }
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			if (InternalChildren.Count <= 0) return;
			if (e.Delta > 0)
			{
				_selectedIndex++;
				if (_selectedIndex >= InternalChildren.Count)
					_selectedIndex = InternalChildren.Count - 1;
			}
			else
			{
				_selectedIndex--;
				if (_selectedIndex < 0) _selectedIndex = -1;
			}
			if (_selectedIndex < 0) return;

			// Animate items
			SelectItem();
		}

		private void SelectItem()
		{
			int numCount = InternalChildren.Count;
			// Vanished
			for (int i = _selectedIndex + 1; i < numCount; i++)
			{
				InternalChildren[i].SetValue(IsVanishedProperty, true);
			}
			// Visible
			for (int i = 0; i <= _selectedIndex; i++)
			{
				InternalChildren[i].SetValue(IsVanishedProperty, false);
			}
		}

		protected override void OnInitialized(EventArgs e)
		{
			_selectedIndex = InternalChildren.Count - 1;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			for (int index = 0; index < InternalChildren.Count; index++)
			{
				Size childSize = new Size(availableSize.Width, ItemHeight);
				InternalChildren[index].Measure(childSize);
			}

			return availableSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			int currentIndex = 0;

			for (int index = InternalChildren.Count-1; index >= 0; index--)
			{
				FrameworkElement elt = InternalChildren[index] as FrameworkElement;

				bool isVanished = (bool) elt.GetValue(IsVanishedProperty);
				if (isVanished)
				{
					elt.Arrange(new Rect(0,0,0,0));
					continue;
				}

				Rect rect = CalculateRect(finalSize, currentIndex);
				elt.Arrange(rect);

				currentIndex++;
			}
			return finalSize;
		}

		protected override void OnRender(DrawingContext dc)
		{
			dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
		}

		private Rect CalculateRect(Size panelSize, int index)
		{
			double zFactor = Math.Pow(ZFactor, index);
			Size itemSize = new Size(panelSize.Width * zFactor, ItemHeight * zFactor);

			double degAngle = 90*XFactor;
			double left = (panelSize.Width - itemSize.Width)*XFactor;
			double top = panelSize.Height;
			for (int i = 0; i < index + 1; i++)
			{
				top -= Math.Pow(ZFactor, i) * ItemHeight;
				if (i != 0) top += (1 - ZFactor) * ItemHeight;
			}

			Rect rect = new Rect(itemSize);
			rect.Location = new Point(left, top);
			return rect;
		}
	}
}