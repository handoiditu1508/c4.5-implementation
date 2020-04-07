using System;
using System.Collections.Generic;

namespace C45.Models
{
	public class Node
	{
		public IDictionary<string, Node> Childs { get; private set; }
		public Column Column { get; set; }
		public double? Threshold { get; set; }
		public Func<IList<string>, string> DecideFunc;

		public Node()
		{
			Childs = new Dictionary<string, Node>();
		}

		public void AddChild(string name, Node child) => Childs.Add(name, child);

		public string Decide(IList<string> row) => DecideFunc(row);
	}
}
