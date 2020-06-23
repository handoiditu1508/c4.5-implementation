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
			string filePath = @"..\..\..\data\";//visual studio
			//string filePath = @".\data\";//visual studio code (project scope)
			//string filePath = @".\C45\data\";//visual studio code (solution scope)
			string fileName = "connect-4.csv";
			IList<IList<string>> data = new List<IList<string>>();
			IList<Column> columns = new List<Column>();
			IList<string> columnNames = null;
			var distinctValues = new List<HashSet<string>>();

			#region Read File
			Console.WriteLine("Start Reading File");
			bool isFirstLine = true;

			foreach (string line in File.ReadLines(Path.Combine(filePath, fileName)))
			{
				if (isFirstLine)
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
			Console.WriteLine("End Reading File");
			Console.WriteLine("--------------------------");
			#endregion

			#region Split Data
			Console.WriteLine("Start Splitting Data");
			IList<IList<string>> trainingData = new List<IList<string>>();
			IList<IList<string>> testData = new List<IList<string>>();
			int seed = 13;
			IList<IList<string>> shuffledData = ShuffleListOfLists(data, seed);
			float testSize = 0.3f;
			int testLength = (int)(data.Count * testSize);

			for (int i = 0; i < testLength; i++)
				testData.Add(shuffledData[i]);
			for (int i = testLength; i < shuffledData.Count; i++)
				trainingData.Add(shuffledData[i]);
			Console.WriteLine($"Test set: {testLength} records");
			Console.WriteLine($"Traing set: {shuffledData.Count - testLength} records");
			Console.WriteLine("End Splitting Data");
			Console.WriteLine("--------------------------");
			#endregion

			#region Process Data
			Console.WriteLine("Start Processing Data");
			for (int i = 0; i < trainingData.Count; i++)
			{
				for (int j = 0; j < trainingData[i].Count; j++)
				{
					if (trainingData[i][j] != "?")
						distinctValues[j].Add(trainingData[i][j]);
				}
			}

			for (int i = 0; i < columnNames.Count; i++)
			{
				bool isNominal = false;
				foreach (string val in distinctValues[i])
				{
					if (!double.TryParse(val, out double val2))
					{
						isNominal = true;
						break;
					}
				}
				Column col = new Column(columnNames[i], i, isNominal);
				col.Values = distinctValues[i].ToList();
				columns.Add(col);
			}
			Console.WriteLine("End Processing Data");
			Console.WriteLine("--------------------------");
			#endregion

			#region Create Tree
			Console.WriteLine("Start Creating Tree");
			Node tree = TreeCreator.CreateTree(trainingData, columns);
			Console.WriteLine("End Creating Tree");
			Console.WriteLine("--------------------------");
			#endregion

			#region Predict
			/*Console.WriteLine("Deciding");
			int rightDecisions = 0;
			int missingRows = 0;
			foreach (var row in data)
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
			Console.WriteLine("--------------------------");*/
			Console.WriteLine("Start Predicting");
			int rightPredictions = 0;
			foreach (var row in testData)
			{
				string decision = tree.Decide(row);
				if (decision == row[row.Count - 1])
					rightPredictions++;
			}
			Console.WriteLine($"{rightPredictions}/{testData.Count} right predictions");
			Console.WriteLine("End Predicting");
			Console.WriteLine("--------------------------");
			#endregion

			Console.WriteLine("Done!");
		}

		static IList<IList<string>> ShuffleListOfLists(IList<IList<string>> list, int? seed)
		{
			IList<IList<string>> resultList = list.Select(x => x).ToList();
			Random rnd = seed.HasValue ? new Random(seed.Value) : new Random();
			for (int i = resultList.Count - 1; i > 0; i--)
			{
				int randomIndex = rnd.Next(0, i + 1);

				IList<string> temp = resultList[i];
				resultList[i] = resultList[randomIndex];
				resultList[randomIndex] = temp;
			}
			return resultList;
		}
	}
}
