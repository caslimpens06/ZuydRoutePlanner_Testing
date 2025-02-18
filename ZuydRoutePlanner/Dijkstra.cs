﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZuydRoutePlanner
{
    public class Dijkstra
    {
		public static (List<Node> path, double totalDistance) FindShortestPath(Graph graph, Node start, Node end, bool requireAccessible, bool emergencyMode, bool startByCar)
		{
			var distances = new Dictionary<Node, double>();
			var previous = new Dictionary<Node, Node>();
			var priorityQueue = new SortedSet<(Node node, double distance)>(Comparer<(Node node, double distance)>.Create((x, y) =>
				x.distance == y.distance ? x.node.Name.CompareTo(y.node.Name) : x.distance.CompareTo(y.distance)));

			foreach (var node in graph.Nodes)
			{
				distances[node] = double.PositiveInfinity;
				previous[node] = null;
			}

			distances[start] = 0;
			priorityQueue.Add((start, 0));

			bool isCarMode = startByCar;

			while (priorityQueue.Count > 0)
			{
				var (currentNode, currentDistance) = priorityQueue.Min;
				priorityQueue.Remove(priorityQueue.Min);

				if (currentNode == end)
					break;

				foreach (var edge in currentNode.Edges)
				{
					if ((emergencyMode && edge.IsEmergencyBlocked) ||
						(!emergencyMode && edge.Destination.IsEmegencyOnly) ||
						(requireAccessible && !edge.IsAccessible) ||
						(!requireAccessible && edge.Destination.RequireAccessible) ||
						(isCarMode && !edge.IsAccessibleByCar && !currentNode.IsParking))
						continue;

					double newDist = currentDistance + edge.Distance;
					if (newDist < distances[edge.Destination])
					{
						priorityQueue.Remove((edge.Destination, distances[edge.Destination]));
						distances[edge.Destination] = newDist;
						previous[edge.Destination] = currentNode;
						priorityQueue.Add((edge.Destination, newDist));
					}
				}

				if (isCarMode && currentNode.IsParking && (!requireAccessible || currentNode.RequireAccessible))
				{
					isCarMode = false;
				}
			}

			var path = new List<Node>();
			for (var at = end; at != null; at = previous[at])
			{
				path.Insert(0, at);
			}

			return (path, distances[end]);
		}


		public static (List<Node> path, double totalDistance) FindShortestPathRec(Graph graph, Node start, Node end, bool requireAccessible, bool emergencyMode, bool startByCar)
        {
            var distances = new Dictionary<Node, double>();
            var previous = new Dictionary<Node, Node>();

            foreach (var node in graph.Nodes)
            {
                distances[node] = double.PositiveInfinity;
                previous[node] = null;
            }

            distances[start] = 0;

            FindShortestPathRecursive(graph, start, end, requireAccessible, emergencyMode, startByCar, distances, previous);

            // Reconstruct the path
            var path = new List<Node>();
            for (var at = end; at != null; at = previous[at])
            {
                path.Insert(0, at);
            }

            return (path, distances[end]);
        }

        private static void FindShortestPathRecursive(Graph graph, Node currentNode, Node end, bool requireAccessible, bool emergencyMode, bool isCarMode, Dictionary<Node, double> distances, Dictionary<Node, Node> previous)
        {
            if (currentNode == end)
                return;

            foreach (var edge in currentNode.Edges)
            {
                // Skip edges that are blocked during emergency
                if ((emergencyMode && edge.IsEmergencyBlocked) ||
                    // Skip doors that are emergency only when not in emergency mode
                    (!emergencyMode && edge.Destination.IsEmegencyOnly) ||
                    // Skip edges that are not accessible when accessibility is required
                    (requireAccessible && !edge.IsAccessible) ||
                    // Skip edges that need accessibility required (such as the elevator)
                    (!requireAccessible && edge.Destination.RequireAccessible) ||
                    // Skip edges that are not accessible by car when in car mode
                    (isCarMode && !edge.IsAccessibleByCar && !currentNode.IsParking))
                    continue;

                double newDist = distances[currentNode] + edge.Distance;
                if (newDist < distances[edge.Destination])
                {
                    distances[edge.Destination] = newDist;
                    previous[edge.Destination] = currentNode;

                    // Switch to foot mode if a parking node is reached and it is accessible if required
                    bool nextIsCarMode = isCarMode;
                    if (isCarMode && currentNode.IsParking && (!requireAccessible || currentNode.RequireAccessible))
                    {
                        nextIsCarMode = false;
                    }

                    FindShortestPathRecursive(graph, edge.Destination, end, requireAccessible, emergencyMode, nextIsCarMode, distances, previous);
                }
            }
        }
    }
}
