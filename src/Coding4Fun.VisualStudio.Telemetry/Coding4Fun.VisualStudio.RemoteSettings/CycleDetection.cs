using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class CycleDetection : ICycleDetection
	{
		internal class Node
		{
			private string value;

			private HashSet<Node> edges = new HashSet<Node>();

			public List<Node> Edges => edges.ToList();

			public Node(string value)
			{
				this.value = value;
			}

			public void AddDirected(Node node)
			{
				if (!edges.Contains(node))
				{
					edges.Add(node);
				}
			}

			public override string ToString()
			{
				return value;
			}

			public override bool Equals(object obj)
			{
				if (!(obj is Node))
				{
					return false;
				}
				Node node = (Node)obj;
				return value.Equals(node.value);
			}

			public override int GetHashCode()
			{
				return value.GetHashCode();
			}
		}

		internal class Graph
		{
			private Dictionary<string, Node> nodes = new Dictionary<string, Node>();

			public List<Node> Nodes => nodes.Select((KeyValuePair<string, Node> keyValue) => keyValue.Value).ToList();

			public Node AddNode(string value)
			{
				if (nodes.ContainsKey(value))
				{
					return nodes[value];
				}
				Node node = new Node(value);
				nodes[value] = node;
				return node;
			}

			/// <summary>
			/// Determines if there is a cycle in the graph.
			/// </summary>
			/// <returns>True if there is a cycle, returns as soon as one is found</returns>
			public bool HasCycles()
			{
				Dictionary<Node, Color> dictionary = new Dictionary<Node, Color>();
				foreach (Node node in Nodes)
				{
					if (!dictionary.ContainsKey(node) && ColorToFindCycles(node, dictionary))
					{
						return true;
					}
				}
				return false;
			}

			/// <summary>
			/// Colors nodes to determine if there is a cycle in the graph.
			/// </summary>
			/// <param name="node"></param>
			/// <param name="colors"></param>
			/// <returns>True if there is a cycle</returns>
			private bool ColorToFindCycles(Node node, IDictionary<Node, Color> colors)
			{
				colors[node] = Color.Gray;
				foreach (Node edge in node.Edges)
				{
					if (!colors.ContainsKey(edge) && ColorToFindCycles(edge, colors))
					{
						return true;
					}
					if (colors[edge] == Color.Gray)
					{
						return true;
					}
				}
				colors[node] = Color.Black;
				return false;
			}
		}

		internal enum Color
		{
			Gray,
			Black
		}

		private static Regex scopeRegex = new Regex("Scope\\.[a-zA-Z_0-9]+");

		public bool HasCycles(IEnumerable<Scope> scopes)
		{
			Graph graph = new Graph();
			foreach (Scope scope in scopes)
			{
				Node node = graph.AddNode(scope.Name);
				foreach (object scopeMatch in GetScopeMatches(scope.ScopeString))
				{
					string[] array = scopeMatch.ToString().Split('.');
					Node node2 = graph.AddNode(array[1]);
					node.AddDirected(node2);
				}
			}
			return graph.HasCycles();
		}

		internal static MatchCollection GetScopeMatches(string scopeString)
		{
			return scopeRegex.Matches(scopeString);
		}
	}
}
