﻿using System;
using System.Collections.Generic;
using System.Reflection;
using CoreGame.Engine;
using Microsoft.Xna.Framework;

namespace CoreGame.Scene
{
	/// <summary>
	/// Layer is container for Actors. It also work for physics world.
	/// IMPORTANT
	/// 1. Physics can only work non-negative position! (Humper limitation)
	/// 2. Layer is NOT PHYSIC LAYER!! LayerName used for rendering priority!
	/// </summary>
	public class Layer : IGameCycle
	{
		/// <summary>
		/// Initialize layer
		/// </summary>
		/// <param name="gameWorld"></param>
		/// <param name="layerName"></param>
		/// <param name="useCollision"></param>
		public Layer(World gameWorld, LayerName layerName)
		{
			GameWorld = gameWorld;
			LayerName = layerName;
		}

		public World GameWorld { get; private set; }
		public LayerName LayerName { get; private set; }

		/// <summary>
		/// Auto Y Sort only work when Actor Position is positive.
		/// </summary>
		public bool AutoYSort = false;
		
		protected List<Actor> Actors = new List<Actor>();
		
		/// <summary>
		/// Actors that created in runtime (After Initialize function)
		/// This will activate Start function to be called
		/// </summary>
		private List<Actor> _delayedActor = new List<Actor>();
		
		/// <summary>
		/// Add Actor to the current layer. When adding at runtime, Actor will not available in the world directly.
		/// It need to wait to the next frame.
		/// </summary>
		/// <param name="name"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T AddActor<T>(string name) where T : Actor
		{
			T actor = (T) Activator.CreateInstance(typeof(T), name, this);
			
			AddActor(actor, false);
			return actor;
		}
		// TODO AddActor Recursively
		/// <summary>
		/// Adding new actor to world
		/// </summary>
		/// <param name="actor">Actor</param>
		/// <param name="recursive">add all child actor</param>
		public void AddActor(Actor actor, bool recursive = true)
		{
			_delayedActor.Add(actor);
			if (recursive)
			{
				foreach (var child in actor.GetChilds)
				{
					AddActor(child, recursive);
				}
			}
		}

		public virtual void Initialize()
		{
			foreach (Actor actor in Actors)
			{
				actor.Initialize();
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="isThisAfterInitialize">true when only created at Initialize cycle</param>
		public virtual void Start()
		{
			for (int i = _delayedActor.Count - 1; i >= 0; i--)
			{
				_delayedActor[i].Start();
				_delayedActor.RemoveAt(i);
			}
		}
		
		public virtual void Update(GameTime gameTime)
		{
			foreach (Actor actor in Actors)
			{
				actor.Update(gameTime);
			}
		}

		public virtual void LateUpdate(GameTime gameTime)
		{
			foreach (Actor actor in Actors)
			{
				actor.LateUpdate(gameTime);
			}

			ForceAddNewestActor();
		}

		/// <summary>
		/// Add all newest actor
		/// </summary>
		public void ForceAddNewestActor()
		{
			foreach (var actor in _delayedActor)
			{
				Actors.Add(actor);
			}
		}

		public virtual void Draw(GameTime gameTime, float layerZDepth)
		{
			foreach (Actor actor in Actors)
			{
				actor.Draw(gameTime, AutoYSort ?  actor.Transform.Position.Y * float.Epsilon : (float) LayerName);
			}
		}
		
	}

	//List enum edit here
	public enum LayerName
	{
		Background = 0,
		Default = 1,
		Foreground = 2,
		UI = 16
	}
}