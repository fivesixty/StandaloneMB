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
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace FluidKit.Controls
{
	public abstract class ViewStateBase
	{
		protected ElementFlow Owner { get; private set; }

		internal void SetOwner(ElementFlow owner)
		{
			Owner = owner;
		}

		public void SelectElement(int index)
		{
			Storyboard sb = null;
			for (int leftItem = 0; leftItem < index; leftItem++)
			{
				sb = Owner.PrepareTemplateStoryboard(leftItem);
				PrepareStoryboard(sb, GetPreviousMotion(leftItem));
				Owner.AnimateElement(sb);
			}

			sb = Owner.PrepareTemplateStoryboard(index);
			PrepareStoryboard(sb, GetSelectionMotion(index));
			Owner.AnimateElement(sb);

			for (int rightItem = index + 1; rightItem < Owner.VisibleChildrenCount; rightItem++)
			{
				sb = Owner.PrepareTemplateStoryboard(rightItem);
				PrepareStoryboard(sb, GetNextMotion(rightItem));
				Owner.AnimateElement(sb);
			}
		}

		private void PrepareStoryboard(Storyboard sb, Motion motion)
		{
			// Child animations
			AxisAngleRotation3D rotation = (sb.Children[0] as Rotation3DAnimation).To as AxisAngleRotation3D;
			DoubleAnimation xAnim = sb.Children[1] as DoubleAnimation;
			DoubleAnimation yAnim = sb.Children[2] as DoubleAnimation;
			DoubleAnimation zAnim = sb.Children[3] as DoubleAnimation;

			rotation.Angle = motion.Angle;
			rotation.Axis = motion.Axis;
			xAnim.To = motion.X;
			yAnim.To = motion.Y;
			zAnim.To = motion.Z;
		}

		protected abstract Motion GetPreviousMotion(int index);
		protected abstract Motion GetSelectionMotion(int index);
		protected abstract Motion GetNextMotion(int index);
	}
}