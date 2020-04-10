using C45.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C45.Services
{
	public class NodeCreator
	{
		public IList<IList<string>> Data { get; private set; }
		public IList<Column> Columns { get; private set; }
		private Node node;
		private FormulasHelper formulas;

		public NodeCreator(IList<IList<string>> data, IList<Column> columns) : this(data, columns, new Node())
		{}

		public NodeCreator(IList<IList<string>> data, IList<Column> columns, Node node)
		{
			this.Data = data;
			this.Columns = columns;
			this.node = node;
			formulas = new FormulasHelper(data, columns[columns.Count - 1]);
		}

		private ColumnWrapper getBranchingColumn()
		{
			if (formulas.GetEntropy() == 0)
				return new ColumnWrapper(null);

			double gainRatio = double.MinValue;
			double? threshold = null;
			Column result = null;

			IList<Column> newCols = new List<Column>(Columns);
			for(int j = 0; j<Columns.Count-1; j++)
			{
				//remove columns have 1 value
				Column col = Columns[j];
				if(col.Values.Count < 2)
				{
					newCols.Remove(col);
				}
				//calculate gain ratio
				else if(col.IsNominal)
				{
					double gR = formulas.GetGainRatio(col);
					if(gR > gainRatio)
					{
						gainRatio = gR;
						threshold = null;
						result = col;
					}
				}
				else
				{
					col.Values = col.Values.OrderBy(v => v).ToList();
					for (int i = 0; i < col.Values.Count - 1; i++)
					{
						double th = double.Parse(col.Values[i]);
						double gR = formulas.GetGainRatio(col, col.Values[i]);
						if(gR > gainRatio)
						{
							gainRatio = gR;
							threshold = th;
							result = col;
						}
					}
				}
			}
			Columns = newCols;

			return new ColumnWrapper(result, threshold);
		}

		public Node GetNode()
		{
			if (node == null)
				node = new Node();

			ColumnWrapper cw = getBranchingColumn();

			node.Column = cw.Column;
			node.Threshold = cw.Threshold;

			if(node.Column != null)
			{
				if (node.Column.IsNominal)
				{
					node.DecideFunc = row => node.Childs[row[node.Column.Index]].Decide(row);
				}
				else
				{
					node.DecideFunc = delegate (IList<string> row)
					{
						if (double.Parse(row[node.Column.Index]) > node.Threshold)
						{
							return node.Childs[true.ToString()].Decide(row);
						}
						else return node.Childs[false.ToString()].Decide(row);
					};
				}
			}
			else
			{
				IDictionary<string, int> count = new Dictionary<string, int>();
				Column decisionColumn = Columns[Columns.Count - 1];
				foreach (string val in decisionColumn.Values)
				{
					count.Add(val, 0);
				}

				foreach(IList<string> row in Data)
				{
					count[row[decisionColumn.Index]]++;
				}

				string mostAppearResult = count.First().Key;

				foreach (string val in decisionColumn.Values)
				{
					if (count[val] > count[mostAppearResult])
						mostAppearResult = val;
				}

				node.DecideFunc = row => mostAppearResult;
			}

			return node;
		}

		private class ColumnWrapper
		{
			public Column Column { get; set; }
			public double? Threshold { get; set; }

			public ColumnWrapper(Column column)
			{
				Column = column;
			}

			public ColumnWrapper(Column column, double? threshold) : this(column)
			{
				Threshold = threshold;
			}
		}
	}
}
