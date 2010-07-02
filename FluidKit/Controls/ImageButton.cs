// -------------------------------------------------------------------------------
// 
// This file is part of the FluidKit project: http://www.codeplex.com/fluidkit
// 
// Copyright (c) 2008, The FluidKit community 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this 
// list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice, this 
// list of conditions and the following disclaimer in the documentation and/or 
// other materials provided with the distribution.
// 
// * Neither the name of FluidKit nor the names of its contributors may be used to 
// endorse or promote products derived from this software without specific prior 
// written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR 
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON 
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
// -------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace FluidKit.Controls
{
	public class ImageButton : Button
	{
		static ImageButton()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			DefaultStyleKeyProperty.OverrideMetadata(typeof (ImageButton), new FrameworkPropertyMetadata(typeof (ImageButton)));
		}

		#region properties

		public string ImageHover
		{
			get { return (string) GetValue(ImageHoverProperty); }
			set { SetValue(ImageHoverProperty, value); }
		}

		public string ImageNormal
		{
			get { return (string) GetValue(ImageNormalProperty); }
			set { SetValue(ImageNormalProperty, value); }
		}

		public string ImagePressed
		{
			get { return (string) GetValue(ImagePressedProperty); }
			set { SetValue(ImagePressedProperty, value); }
		}

		public string ImageDisabled
		{
			get { return (string) GetValue(ImageDisabledProperty); }
			set { SetValue(ImageDisabledProperty, value); }
		}

		#endregion

		#region dependency properties

		public static readonly DependencyProperty ImageDisabledProperty =
			DependencyProperty.Register(
				"ImageDisabled", typeof (string), typeof (ImageButton));

		public static readonly DependencyProperty ImageHoverProperty =
			DependencyProperty.Register(
				"ImageHover", typeof (string), typeof (ImageButton));

		public static readonly DependencyProperty ImageNormalProperty =
			DependencyProperty.Register(
				"ImageNormal", typeof (string), typeof (ImageButton));

		public static readonly DependencyProperty ImagePressedProperty =
			DependencyProperty.Register(
				"ImagePressed", typeof (string), typeof (ImageButton));

		#endregion
	}
}