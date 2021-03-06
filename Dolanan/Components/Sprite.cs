﻿using System;
using System.Runtime.Serialization;
using Dolanan.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dolanan.Components
{
	// TODO Create Frame feature
	public class Sprite : Renderer
	{
		// TODO: Complete setter and getter event, such changing frame will automatically change the sprite image
		public int Frame
		{
			get => _frame;
			set
			{
				_frame = value;
				Point rectPosition = new Point();
				rectPosition.Y = (int) MathF.Floor(_frame / (Texture2D.Width / FrameSize.X)) * FrameSize.Y; 
				rectPosition.X = (int) MathF.Floor(_frame % (Texture2D.Width / FrameSize.X)) * FrameSize.X;
				SrcLocation = rectPosition;
			}
		}

		private int _frame = 0;

		public Point FrameSize
		{
			get => SrcSize;
			set => SrcSize = value;
		}

		public Sprite(Actor owner) : base(owner)
		{
		}

		public override void Start()
		{
			base.Start();
			FrameSize = SrcRectangle.Size;
		}
	}
}