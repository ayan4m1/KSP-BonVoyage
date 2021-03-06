﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BonVoyage
{
	// A* pathfinding
	// https://blogs.msdn.microsoft.com/ericlippert/tag/astar/

	class Path<Node> : IEnumerable<Node>
	{
		public Node LastStep { get; private set; }
		public Path<Node> PreviousSteps { get; private set; }
		public double TotalCost { get; private set; }
		private Path(Node lastStep, Path<Node> previousSteps, double totalCost)
		{
			LastStep = lastStep;
			PreviousSteps = previousSteps;
			TotalCost = totalCost;
		}
		public Path(Node start) : this(start, null, 0) {}
		public Path<Node> AddStep(Node step, double stepCost)
		{
			return new Path<Node>(step, this, TotalCost + stepCost);
		}
		public IEnumerator<Node> GetEnumerator()
		{
			for (Path<Node> p = this; p != null; p = p.PreviousSteps)
				yield return p.LastStep;
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		static public Path<Node> FindPath<Node>(
			Node start,
			Node destination,
			Func<Node, Node, double> distance,
			Func<Node, double> estimate)
			where Node : IHasNeighbours<Node>
		{
			DateTime startedAt = DateTime.Now;
			var closed = new HashSet<Node>();
			var queue = new PriorityQueue<double, Path<Node>>();
			queue.Enqueue(0, new Path<Node>(start));
			while (!queue.IsEmpty)
			{
				var path = queue.Dequeue();
				if (closed.Contains(path.LastStep))
					continue;
				if (path.LastStep.Equals (destination)) {
//					path.Reverse (); // Don't ask me...
					return path;
				}
				closed.Add(path.LastStep);
				foreach(Node n in path.LastStep.Neighbours)
				{
					double d = distance(path.LastStep, n);
					var newPath = path.AddStep(n, d);
					queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
				}
				// If pathfinding takes too much time then cancel
				if (startedAt.AddSeconds (10) < DateTime.Now) {
					ScreenMessages.PostScreenMessage ("Ten seconds passed", 5);
					ScreenMessages.PostScreenMessage ("Route calculation stopped", 6);
					ScreenMessages.PostScreenMessage ("Try some closer location", 7);
					return null;
				}
			}
			return null;
		}
	}

	class PriorityQueue<P, V>
	{
		private SortedDictionary<P, Queue<V>> list = new SortedDictionary<P, Queue<V>>();
		public void Enqueue(P priority, V value)
		{
			Queue<V> q;
			if (!list.TryGetValue(priority, out q))
			{
				q = new Queue<V>();
				list.Add(priority, q);
			}
			q.Enqueue(value);
		}
		public V Dequeue()
		{
			// will throw if there isn’t any first element!
			var pair = list.First();
			var v = pair.Value.Dequeue();
			if (pair.Value.Count == 0) // nothing left of the top priority.
				list.Remove(pair.Key);
			return v;
		}
		public bool IsEmpty
		{
			get { return !list.Any(); }
		}
	}

	interface IHasNeighbours<N> 
	{
		IEnumerable<N> Neighbours { get; }
	}
}
