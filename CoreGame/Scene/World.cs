﻿using System;
using System.Collections.Generic;
using System.Linq;
using CoreGame.Collision;
using CoreGame.Engine;
using Microsoft.Xna.Framework;
using Humper;
using Humper.Base;
using Humper.Responses;

namespace CoreGame.Scene
{
	//TODO create system that load and unload world. generally changing scene. World is scene
	/// <summary>
	/// World, yes world. It also handle all collision.
	/// </summary>
	public class World : IGameCycle, IWorld
	{
		public Camera Camera;
		protected List<Layer> Layers = new List<Layer>();
		private LayerName _defaultLayer = LayerName.Default;

		public World(int width, int height, float cellSize = 64)
		{
			Camera = new Camera();

			foreach (LayerName name in (LayerName[]) Enum.GetValues(typeof(LayerName)))
			{
				Layers.Add(new Layer(this, name));
			}
			grid = new Grid((int) Math.Ceiling(width / cellSize), 
				(int) Math.Ceiling(height / cellSize), cellSize);
		}
		
		#region GameCycle

		public Layer GetLayer(LayerName layerName)
		{
			//return Layers.Count < (int) layerName ? Layers[(int) layerName] : Layers[(int) layerName];
			return Layers[(int) layerName];
		}

		public T CreateActor<T>(string name) where T : Actor
		{
			return Layers[(int)_defaultLayer].AddActor<T>(name);
		}
		
		public virtual void Initialize()
		{
			foreach (Layer layer in Layers)
			{
				layer.Initialize();
			}
			Camera.Initialize();
		}

		public virtual void Start()
		{
			foreach (Layer layer in Layers)
			{
				layer.Start();
			}
			Camera.Start();
		}

		public virtual void Update(GameTime gameTime)
		{
			foreach (var layer in Layers)
			{
				layer.Update(gameTime);
			}
			Camera.Update(gameTime);
		}

		public virtual void LateUpdate(GameTime gameTime)
		{
			foreach (var layer in Layers)
			{
				layer.LateUpdate(gameTime);
			}
			Camera.LateUpdate(gameTime);
		}

		/// <summary>
		/// Drawing
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="layerZDepth">Not important, for Layer / Actor / Component only</param>
		public virtual void Draw(GameTime gameTime, float layerZDepth = 0)
		{
			foreach (var layer in Layers)
			{
				layer.Draw(gameTime, layerZDepth);
			}
			Camera.Draw(gameTime);
		}
		
		#endregion

		#region Collisions
		
		public RectangleF Bounds => new RectangleF(0, 0, this.grid.Width , this.grid.Height);
		private Grid grid;

		/// <summary>
		/// Create a collision body
		/// </summary>
		/// <param name="transform2D">Transform</param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="origin"></param>
		/// <returns></returns>
		public Body Create(Transform2D transform2D, float width, float height, Vector2 origin)
		{
			Body b = new Body(this, transform2D, origin, width, height);
			grid.Add(b);
			return b;
		}

		IBox IWorld.Create(float x, float y, float width, float height)
		{
			throw new NotImplementedException();
		}

		public bool Remove(IBox box)
		{
			return this.grid.Remove(box);
		}

		public void Update(IBox box, RectangleF @from)
		{
			this.grid.Update(box, from);
		}

		public IEnumerable<IBox> Find(float x, float y, float w, float h)
		{
			x = Math.Max(0, Math.Min(x, this.Bounds.Right - w));
			y = Math.Max(0, Math.Min(y, this.Bounds.Bottom - h));

			return this.grid.QueryBoxes(x, y, w, h);
		}

		public IEnumerable<IBox> Find(RectangleF area)
		{
			return this.Find(area.X, area.Y, area.Width, area.Height);
		}

		public IHit Hit(Vector2 point, IEnumerable<IBox> ignoring = null)
		{
			var boxes = this.Find(point.X, point.Y, 0, 0);

			if (ignoring != null)
			{
				boxes = boxes.Except(ignoring);
			}

			foreach (var other in boxes)
			{
				var hit = Humper.Hit.Resolve(point, other);

				if (hit != null)
				{
					return hit;
				}
			}

			return null;
		}

		public IHit Hit(Vector2 origin, Vector2 destination, IEnumerable<IBox> ignoring = null)
		{
			var min = Vector2.Min(origin, destination);
			var max = Vector2.Max(origin, destination);

			var wrap = new RectangleF(min, max - min);
			var boxes = this.Find(wrap.X, wrap.Y, wrap.Width, wrap.Height);

			if (ignoring != null)
			{
				boxes = boxes.Except(ignoring);
			}

			IHit nearest = null;

			foreach (var other in boxes)
			{
				var hit = Humper.Hit.Resolve(origin, destination, other);

				if (hit != null && (nearest == null || hit.IsNearest(nearest,origin)))
				{
					nearest = hit;
				}
			}

			return nearest;
		}

		public IHit Hit(RectangleF origin, RectangleF destination, IEnumerable<IBox> ignoring = null)
		{
			var wrap = new RectangleF(origin, destination);
			var boxes = this.Find(wrap.X, wrap.Y, wrap.Width, wrap.Height);

			if (ignoring != null)
			{
				boxes = boxes.Except(ignoring);
			}

			IHit nearest = null;

			foreach (var other in boxes)
			{
				var hit = Humper.Hit.Resolve(origin, destination, other);

				if (hit != null && (nearest == null || hit.IsNearest(nearest, origin.Location)))
				{
					nearest = hit;
				}
			}

			return nearest;
		}

		public IMovement Simulate(IBox box, float x, float y, Func<ICollision, ICollisionResponse> filter)
		{
			var origin = box.Bounds;
			var destination = new RectangleF(x, y, box.Width, box.Height);

			var hits = new List<IHit>();

			var result = new Movement()
			{
				Origin = origin,
				Goal = destination,
				Destination = this.Simulate(hits, new List<IBox>() { box }, (Body)box, origin, destination, filter),
				Hits = hits,
			};

			return result;
		}
		private RectangleF Simulate(List<IHit> hits, List<IBox> ignoring, Body box, RectangleF origin, RectangleF destination, Func<ICollision, ICollisionResponse> filter)
		{
			var nearest = this.Hit(origin, destination, ignoring);
				
			if (nearest != null)
			{
				hits.Add(nearest);

				var impact = new RectangleF(nearest.Position, origin.Size);
				var collision = new Humper.Collision() { Box = box, Hit = nearest, Goal = destination, Origin = origin };
				var response = filter(collision);

				if (response != null && destination != response.Destination)
				{
					ignoring.Add(nearest.Box);
					return this.Simulate(hits, ignoring, box, impact, response.Destination, filter);
				}
			}

			return destination;
		}
		
		public bool IsValidLocation(Vector2 v)
		{
			return (v.X >= 0) && (v.Y >= 0) && (v.X <= grid.Width) && (v.X <= grid.Height);
		}

		public void DrawDebug(int x, int y, int w, int h, Action<int, int, int, int, float> drawCell, Action<IBox> drawBox, Action<string, int, int, float> drawString)
		{
			var boxes = this.grid.QueryBoxes(x, y, w, h);
			foreach (var box in boxes)
			{
				drawBox(box);
			}

			// Drawing cells
			var cells = this.grid.QueryCells(x, y, w, h);
			foreach (var cell in cells)
			{
				var count = cell.Count();
				var alpha = count > 0 ? 1f : 0.4f;
				drawCell((int)cell.Bounds.X, (int)cell.Bounds.Y, (int)cell.Bounds.Width, (int)cell.Bounds.Height, alpha);
				drawString(count.ToString(), (int)cell.Bounds.Center.X, (int)cell.Bounds.Center.Y,alpha);
			}
		}
		
		#endregion
	}
}