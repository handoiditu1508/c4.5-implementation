using C45.Models;
using C45.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace C45
{
	class Program
	{
		static void Main(string[] args)
		{
			string filePath = @"..\..\..\data\";
			string fileName = "tic_tac_toe.csv";
			IList<IList<string>> data = new List<IList<string>>();
			IList<Column> columns = new List<Column>();
			IList<string> columnNames = null;
			var distinctValues = new List<HashSet<string>>();

			Console.WriteLine("Reading File");
			bool isFirstLine = true;
			foreach (string line in File.ReadLines(Path.Combine(filePath, fileName)))
			{
				if(isFirstLine)
				{
					isFirstLine = false;
					columnNames = line.Split(',');
					for (int i = 0; i < columnNames.Count; i++)
					{
						distinctValues.Add(new HashSet<string>());
					}
					continue;
				}
				data.Add(line.Split(','));
			}
			Console.WriteLine("File Readed");
			Console.WriteLine("--------------------------");

			Console.WriteLine("Processing Data");
			for (int i = 0; i < data.Count; i++)
			{
				for (int j = 0; j < data[i].Count; j++)
				{
					if (data[i][j] != "?")
						distinctValues[j].Add(data[i][j]);
				}
			}

			for (int i = 0; i < columnNames.Count; i++)
			{
				bool isNominal = false;
				foreach(string val in distinctValues[i])
				{
					if(!double.TryParse(val, out double val2))
					{
						isNominal = true;
						break;
					}
				}
				Column col = new Column(columnNames[i], i, isNominal);
				col.Values = distinctValues[i].ToList();
				columns.Add(col);
			}
			Console.WriteLine("Data Processed");
			Console.WriteLine("--------------------------");

			foreach (Column col in columns)
			{
				foreach (var val in col.Values)
					Console.Write(val + ",");
				Console.WriteLine("");
			}

			Console.WriteLine("Creating Tree");
			Node tree = TreeCreator.CreateTree(data, columns);
			Console.WriteLine("Tree Created");
			Console.WriteLine("--------------------------");

			Console.WriteLine("Deciding");
			int rightDecisions = 0;
			int missingRows = 0;
			foreach(var row in data)
			{
				if (row.Any(s => s == "?"))
				{
					missingRows++;
					continue;
				}
				string decision = tree.Decide(row);
				if (decision == row[row.Count - 1])
					rightDecisions++;
				//Console.WriteLine($"[{decision}] | [{row[row.Count - 1]}]");
			}
			Console.WriteLine($"{rightDecisions}/{data.Count - missingRows} right decisions");
			Console.WriteLine("Decided");
			Console.WriteLine("--------------------------");

			Console.WriteLine("Done!");
			Console.ReadKey();
		}
	}
}
