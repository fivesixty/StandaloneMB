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
using System;
using System.Timers;
using System.ComponentModel;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace FluidKit.Controls
{
	public partial class ElementFlow : Panel
	{
		// Fields
		private ContainerUIElement3D _modelContainer;
		private Viewport3D _viewport;

        #region Events

        public event EventHandler SelectedIndexChanged;
        #endregion

		#region Properties

		public int SelectedIndex
		{
			get { return (int) GetValue(SelectedIndexProperty); }
			set {SetValue(SelectedIndexProperty, value);}
		}

		public double TiltAngle
		{
			get { return (double) GetValue(TiltAngleProperty); }
			set { SetValue(TiltAngleProperty, value); }
		}

		public double ItemGap
		{
			get { return (double) GetValue(ItemGapProperty); }
			set { SetValue(ItemGapProperty, value); }
		}

		public double FrontItemGap
		{
			get { return (double) GetValue(FrontItemGapProperty); }
			set { SetValue(FrontItemGapProperty, value); }
		}

		public double PopoutDistance
		{
			get { return (double) GetValue(PopoutDistanceProperty); }
			set { SetValue(PopoutDistanceProperty, value); }
		}

		public ViewStateBase CurrentView
		{
			get { return (ViewStateBase) GetValue(CurrentViewProperty); }
			set { SetValue(CurrentViewProperty, value); }
		}

		public double ElementWidth
		{
			get { return (double) GetValue(ElementWidthProperty); }
			set { SetValue(ElementWidthProperty, value); }
		}

		public double ElementHeight
		{
			get { return (double) GetValue(ElementHeightProperty); }
			set { SetValue(ElementHeightProperty, value); }
		}

		public PerspectiveCamera Camera
		{
			get { return (PerspectiveCamera) GetValue(CameraProperty); }
			set { SetValue(CameraProperty, value); }
		}

		private ResourceDictionary InternalResources { get; set; }

		/* This gives an accurate count of the number of visible children. 
		 * Panel.Children is not always accurate and is generally off-by-one.
         */

		internal int VisibleChildrenCount
		{
			get { return _modelContainer.Children.Count; }
		}

		public bool HasReflection { get; set; }

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty CameraProperty = DependencyProperty.Register(
			"Camera", typeof (PerspectiveCamera), typeof (ElementFlow), new PropertyMetadata(null, OnCameraChanged));

		public static readonly DependencyProperty CurrentViewProperty =
			DependencyProperty.Register("CurrentView", typeof (ViewStateBase), typeof (ElementFlow),
			                            new FrameworkPropertyMetadata(null, OnCurrentViewChanged));

		public static readonly DependencyProperty ElementHeightProperty =
			DependencyProperty.Register("ElementHeight", typeof (double), typeof (ElementFlow),
			                            new FrameworkPropertyMetadata(300.0));

		public static readonly DependencyProperty ElementWidthProperty =
			DependencyProperty.Register("ElementWidth", typeof (double), typeof (ElementFlow),
			                            new FrameworkPropertyMetadata(400.0));

		public static readonly DependencyProperty FrontItemGapProperty =
			DependencyProperty.Register("FrontItemGap", typeof (double), typeof (ElementFlow),
			                            new PropertyMetadata(0.65, OnFrontItemGapChanged));

		public static readonly DependencyProperty HasReflectionProperty =
			DependencyProperty.Register("HasReflection", typeof (bool), typeof (ElementFlow),
			                            new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty ItemGapProperty =
			DependencyProperty.Register("ItemGap", typeof (double), typeof (ElementFlow),
			                            new PropertyMetadata(0.25, OnItemGapChanged));

		private static readonly DependencyProperty LinkedElementProperty =
			DependencyProperty.Register("LinkedElement", typeof (UIElement), typeof (ElementFlow));

		private static readonly DependencyProperty LinkedModelProperty =
			DependencyProperty.Register("LinkedModel", typeof (ModelUIElement3D), typeof (ElementFlow));

		public static readonly DependencyProperty PopoutDistanceProperty =
			DependencyProperty.Register("PopoutDistance", typeof (double), typeof (ElementFlow),
			                            new PropertyMetadata(1.0, OnPopoutDistanceChanged));

        /*
		public static readonly DependencyProperty SelectedIndexProperty =
			DependencyProperty.Register("SelectedIndex", typeof (int), typeof (ElementFlow),
			                            new PropertyMetadata(-1, OnSelectedIndexChanged)); */

        public static readonly DependencyProperty SelectedIndexProperty =
            Selector.SelectedIndexProperty.AddOwner(typeof(ElementFlow),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedIndexChanged));

		public static readonly DependencyProperty TiltAngleProperty =
			DependencyProperty.Register("TiltAngle", typeof (double), typeof (ElementFlow),
			                            new PropertyMetadata(45.0, OnTiltAngleChanged));

		#endregion

		#region Initialization

		public ElementFlow()
		{
			LoadViewport();
			SetupEventHandlers();

			CurrentView = new CoverFlow();
		}

		private void SetupEventHandlers()
		{
			_modelContainer.MouseLeftButtonDown += OnContainerLeftButtonDown;
            _modelContainer.MouseMove += OnContainerMouseMove;
			Loaded += ElementFlow_Loaded;
		}

		private void LoadViewport()
		{
			_viewport =
				Application.LoadComponent(new Uri("/FluidKit;component/Controls/ElementFlow/Viewport.xaml", UriKind.Relative)) as
				Viewport3D;
			InternalResources = _viewport.Resources;

			// Container for containing the mesh models of elements
			_modelContainer = _viewport.FindName("ModelContainer") as ContainerUIElement3D;
		}

		#endregion

		#region DependencyProperty PropertyChange Callbacks

		private static void OnTiltAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ElementFlow cf = d as ElementFlow;
			cf.ReflowItems();
		}

		private static void OnItemGapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ElementFlow ef = d as ElementFlow;
			ef.ReflowItems();
		}

		private static void OnFrontItemGapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ElementFlow ef = d as ElementFlow;
			ef.ReflowItems();
		}

		private static void OnPopoutDistanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ElementFlow ef = d as ElementFlow;
			ef.ReflowItems();
		}

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ElementFlow ef = d as ElementFlow;
            if (ef.IsLoaded == false)
            {
                return;
            }

            int oldIndex = (int)e.OldValue;
            int newIndex = (int)e.NewValue;

            ef.SelectItemCore(newIndex);
            if (oldIndex != newIndex && ef.SelectedIndexChanged != null)
            {
                ef.SelectedIndexChanged(ef, new EventArgs());
            }

        }

		private static void OnCurrentViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ElementFlow ef = d as ElementFlow;
			ViewStateBase newView = e.NewValue as ViewStateBase;
			if (newView == null)
			{
				throw new ArgumentNullException("e", "The CurrentView cannot be null");
			}
			newView.SetOwner(ef);
			ef.ReflowItems();
		}

		private static void OnCameraChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ElementFlow ef = d as ElementFlow;

			PerspectiveCamera camera = e.NewValue as PerspectiveCamera;
			if (camera == null)
			{
				throw new ArgumentNullException("e", "The Camera cannot be null");
			}

			ef._viewport.Camera = camera;
		}

		#endregion

		#region Event Handlers

		protected override void OnInitialized(EventArgs e)
		{
			AddVisualChild(_viewport);
		}

		private void ElementFlow_Loaded(object sender, RoutedEventArgs e)
		{
			SelectItemCore(SelectedIndex);
		}

		private void OnContainerLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Focus();

			ModelUIElement3D model = e.Source as ModelUIElement3D;
			if (model != null)
			{
				SelectedIndex = _modelContainer.Children.IndexOf(model);
			}
		}

        private void OnContainerMouseMove(object sender, MouseEventArgs e)
        {
            ModelUIElement3D model = e.Source as ModelUIElement3D;
            if (model != null)
            {
                SelectedIndex = _modelContainer.Children.IndexOf(model);
            }
        }

		#endregion

		#region Item Selection

		private void SelectItemCore(int index)
		{
			if (index >= 0 && index < VisibleChildrenCount)
			{
				CurrentView.SelectElement(index);
			}
		}

		internal Storyboard PrepareTemplateStoryboard(int index)
		{
			// Initialize storyboard
			Storyboard sb = (InternalResources["ElementAnimator"] as Storyboard).Clone();
			Rotation3DAnimation rotAnim = sb.Children[0] as Rotation3DAnimation;
			Storyboard.SetTargetProperty(rotAnim, BuildTargetPropertyPath(index, "Rotation"));

			DoubleAnimation xAnim = sb.Children[1] as DoubleAnimation;
			Storyboard.SetTargetProperty(xAnim, BuildTargetPropertyPath(index, "Translation-X"));

			DoubleAnimation yAnim = sb.Children[2] as DoubleAnimation;
			Storyboard.SetTargetProperty(yAnim, BuildTargetPropertyPath(index, "Translation-Y"));

			DoubleAnimation zAnim = sb.Children[3] as DoubleAnimation;
			Storyboard.SetTargetProperty(zAnim, BuildTargetPropertyPath(index, "Translation-Z"));

			return sb;
		}

		private PropertyPath BuildTargetPropertyPath(int index, string animType)
		{
			PropertyDescriptor childDesc = TypeDescriptor.GetProperties(_modelContainer).Find("Children", true);
			string pathString = string.Empty;
			if (animType == "Rotation")
			{
				pathString = "(0)[0].(1)[" + index + "].(2).(3).(4)[0].(5)";
			}
			else if (animType == "Translation-X")
			{
				pathString = "(0)[0].(1)[" + index + "].(2).(3).(4)[1].(6)";
			}
			else if (animType == "Translation-Y")
			{
				pathString = "(0)[0].(1)[" + index + "].(2).(3).(4)[1].(7)";
			}
			else if (animType == "Translation-Z")
			{
				pathString = "(0)[0].(1)[" + index + "].(2).(3).(4)[1].(8)";
			}

			return new PropertyPath(pathString,
			                        Viewport3D.ChildrenProperty,
			                        childDesc,
			                        ModelUIElement3D.ModelProperty,
			                        GeometryModel3D.TransformProperty,
			                        Transform3DGroup.ChildrenProperty,
			                        RotateTransform3D.RotationProperty,
			                        TranslateTransform3D.OffsetXProperty,
			                        TranslateTransform3D.OffsetYProperty,
			                        TranslateTransform3D.OffsetZProperty);
		}

		internal void AnimateElement(Storyboard sb)
		{
			sb.Begin(_viewport);
		}

		#endregion

		#region Layout overrides

		protected override int VisualChildrenCount
		{
			get
			{
				int count = base.VisualChildrenCount;
				count = (count == 0) ? 0 : 1;
				return count;
			}
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			Size eltSize = new Size(ElementWidth, ElementHeight);
			// Arrange children so that their visualbrush has some width/height
			foreach (UIElement child in Children)
			{
				child.Arrange(new Rect(new Point(), eltSize));
			}

			_viewport.Arrange(new Rect(new Point(), finalSize));

			return finalSize;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			Size eltSize = new Size(ElementWidth, ElementHeight);
			foreach (UIElement child in Children)
			{
				child.Measure(eltSize);
			}

			_viewport.Measure(availableSize);

			return availableSize;
		}

		protected override Visual GetVisualChild(int index)
		{
			if (index == 0)
			{
				return _viewport;
			}

			return null;
		}

		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);

			if (visualAdded != null)
			{
				OnVisualAdded(visualAdded as UIElement);
			}

			if (visualRemoved != null)
			{
				OnVisualRemoved(visualRemoved as UIElement);
			}
		}

		#endregion

		#region Utility functions

		private void ReflowItems()
		{
			SelectItemCore(SelectedIndex);
		}

		private void SelectAdjacentItem(bool isNext)
		{
			int index = -1;
			if (isNext == false) // Select previous
			{
				index = Math.Max(-1, SelectedIndex - 1);
			}
			else // Select next
			{
				index = Math.Min(VisibleChildrenCount - 1, SelectedIndex + 1);
			}

			if (index != -1)
			{
				SelectedIndex = index;
			}

		}

        public void ResortAlpha()
        {

            return;

            int index = SelectedIndex;

            for (int i = _modelContainer.Children.Count - 1; i > index; i--)
            {
                Visual3D t = _modelContainer.Children[i];
                _modelContainer.Children.Remove(t);
                _modelContainer.Children.Add(t);
            }

        }

        private void OnVisualRemoved(UIElement elt)
		{
			ModelUIElement3D model = elt.GetValue(LinkedModelProperty) as ModelUIElement3D;
			_modelContainer.Children.Remove(model);

			model.ClearValue(LinkedElementProperty);
			elt.ClearValue(LinkedModelProperty);

			// Update SelectedIndex if needed
			if (SelectedIndex >= 0 && SelectedIndex < VisibleChildrenCount)
			{
				ReflowItems();
			}
			else
			{
				SelectedIndex = Math.Max(0, Math.Min(SelectedIndex, VisibleChildrenCount - 1));
			}
		}

		private void OnVisualAdded(UIElement elt)
		{
			if (elt is Viewport3D) return;

			int index = Children.IndexOf(elt);

			ModelUIElement3D model = CreateMeshModel(elt);
			_modelContainer.Children.Insert(index, model);

			model.SetValue(LinkedElementProperty, elt);
			elt.SetValue(LinkedModelProperty, model);

			if (IsLoaded)
			{
				ReflowItems();
			}
		}

        public int IndexOf(ModelUIElement3D mu)
        {
            return _modelContainer.Children.IndexOf(mu);
        }

		#endregion
	}
}