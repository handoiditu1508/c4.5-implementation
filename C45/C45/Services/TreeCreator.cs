using C45.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C45.Services
{
	public static class TreeCreator
	{
		public static Node CreateTree(IList<IList<string>> data, IList<Column> columns)
		{
			//sort values if column’s type is continuous
			foreach (Column col in columns)
			{
				if(!col.IsNominal)
				{
					col.Values.OrderBy(v => v);
				}
			}

			//deep first search
			Stack<NodeCreator> stack = new Stack<NodeCreator>();

			Node tree = new Node();
			stack.Push(new NodeCreator(data, columns, tree));

			while(stack.Any())
			{
				NodeCreator nc = stack.Pop();
				Node node = nc.GetNode();

				if (node.Column == null)
					continue;

				if(node.Column.IsNominal)
				{
					foreach(string value in node.Column.Values)
					{
						Node child = new Node();
						node.AddChild(value, child);

						var filteredDataFrom_nc = nc.Data.Where(row => row[node.Column.Index] == value/* || row[node.Column.Index] == "?"*/).ToList();
						var copiedColumns = new List<Column>(nc.Columns);
						copiedColumns.Remove(node.Column);
						var filteredColumnsFrom_nc = new List<Column>();

						for(int i = 0; i<copiedColumns.Count;i++)
						{
							var col = copiedColumns[i];
							var values = filteredDataFrom_nc.GroupBy(row => row[col.Index]).Select(c => c.Key).ToList();
							values.Remove("?");
							if (values.Count > 1 || i == copiedColumns.Count - 1)
							{
								Column newCol = col.Clone();
								newCol.Values = values;
								filteredColumnsFrom_nc.Add(newCol);
							}
						}

						stack.Push(new NodeCreator(filteredDataFrom_nc, filteredColumnsFrom_nc, child));
					}
				}
				else
				{
					/*greater than threshold*/
					Node child = new Node();
					node.AddChild(true.ToString(), child);

					var filteredDataFrom_nc = nc.Data.Where(row => double.Parse(row[node.Column.Index]) > node.Threshold/* || row[node.Column.Index] == "?"*/).ToList();
					var copiedColumns = new List<Column>(nc.Columns);
					copiedColumns.Remove(node.Column);
					var filteredColumnsFrom_nc = new List<Column>();

					for (int i = 0; i < copiedColumns.Count; i++)
					{
						var col = copiedColumns[i];
						var values = filteredDataFrom_nc.GroupBy(row => row[col.Index]).Select(c => c.Key).ToList();
						values.Remove("?");
						values.OrderBy(v => v);
						if (values.Count > 1 || i == copiedColumns.Count - 1)
						{
							Column newCol = col.Clone();
							newCol.Values = values;
							filteredColumnsFrom_nc.Add(newCol);
						}
					}

					stack.Push(new NodeCreator(filteredDataFrom_nc, filteredColumnsFrom_nc, child));

					/*lesser or equal to threshold*/
					child = new Node();
					node.AddChild(false.ToString(), child);

					filteredDataFrom_nc = nc.Data.Where(row => double.Parse(row[node.Column.Index]) <= node.Threshold/* || row[node.Column.Index] == "?"*/).ToList();
					filteredColumnsFrom_nc = new List<Column>();

					for (int i = 0; i < copiedColumns.Count; i++)
					{
						var col = copiedColumns[i];
						var values = filteredDataFrom_nc.GroupBy(row => row[col.Index]).Select(c => c.Key).ToList();
						values.Remove("?");
						values.OrderBy(v => v);
						if (values.Count > 1 || i == copiedColumns.Count - 1)
						{
							Column newCol = col.Clone();
							newCol.Values = values;
							filteredColumnsFrom_nc.Add(newCol);
						}
					}

					stack.Push(new NodeCreator(filteredDataFrom_nc, filteredColumnsFrom_nc, child));
				}
			}

			return tree;
		}
	}
}
