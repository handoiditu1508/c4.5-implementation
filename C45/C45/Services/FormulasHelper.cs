using C45.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C45.Services
{
	public class FormulasHelper
	{
		private IEnumerable<IList<string>> data;
		public IEnumerable<IList<string>> Data
		{
			get => data;
			private set {
				if(value == null){
					data = value;
				}
				else if(data == null || data != value){
					data = value;
					recalculateDecisionRates();
					Threshold = null;
					Column = null;
					columnRates.Clear();
					columnRatesForVals.Clear();
				}
			}
		}
		public Column DecisionColumn { get; private set; }
		private IDictionary<string, double> decisionRates;

		private IDictionary<string, double> columnRates;
		private IDictionary<string, IDictionary<string, double>> columnRatesForVals;
		private string threshold;
		public string Threshold
		{
			get => threshold;
			set
			{
				if(value == null){
					threshold = value;
				}
				else if (threshold == null || threshold != value)
				{
					threshold = value;
					if(!Column.IsNominal)
						recalculateColumnRates();
				}

			}
		}
		private Column column;
		public Column Column
		{
			get => column;
			set {
				if (value == null)
				{
					column = value;
				}
				else if (column == null || column.Index != value.Index)
				{
					column = value;
					recalculateColumnRates();
				}
			}
		}

		private void setColumnAndThreshold(Column newColumn, string newThreshold){
			bool recalculate = false;
			if(newColumn != null){
				if (newColumn != Column)
					recalculate = true;
			}
			else if(newThreshold != null){
				if (newThreshold != Threshold)
					recalculate = true;
			}

			column = newColumn;
			threshold = newThreshold;
			if (recalculate)
				recalculateColumnRates();
		}

		private void recalculateDecisionRates()
		{
			decisionRates.Clear();

			//initialize decision count
			foreach (string val in DecisionColumn.Values)
			{
				decisionRates.Add(val, 0);
			}

			//counts decisions
			int dataLength = 0;
			foreach (IList<string> row in Data)
			{
				if(row[DecisionColumn.Index] != "?")
					decisionRates[row[DecisionColumn.Index]]++;
				dataLength++;
			}

			//calculates rates of decisions
			foreach (string val in DecisionColumn.Values)
			{
				decisionRates[val] = decisionRates[val] / dataLength;
			}
		}

		private void recalculateColumnRates()
		{
			columnRates.Clear();
			columnRatesForVals.Clear();

			if (Column == null)
				return;

			if (Column.IsNominal)
			{
				#region columnRates
				//initialize decision count
				foreach (string val in Column.Values)
				{
					columnRates.Add(val, 0);
				}

				//counts decisions
				int dataLength = 0;
				foreach (IList<string> row in Data)
				{
					if(row[Column.Index] != "?")
						columnRates[row[Column.Index]]++;
					dataLength++;
				}

				//calculates rates of decisions
				foreach (string val in Column.Values)
				{
					columnRates[val] = columnRates[val] / dataLength;
				}
				#endregion

				#region columnRatesForVals
				foreach (string val in Column.Values)
				{
					//initialize decision count
					IDictionary<string, double> dRates = new Dictionary<string, double>();
					foreach (string dVal in DecisionColumn.Values){
						dRates.Add(dVal, 0);
					}

					//counts decisions
					dataLength = 0;
					foreach (IList<string> row in Data){
						if (row[Column.Index] == val){
							dRates[row[DecisionColumn.Index]]++;
							dataLength++;
						}
					}

					//calculates rates of decisions
					foreach (string dVal in DecisionColumn.Values){
						dRates[dVal] = dRates[dVal] / dataLength;
					}

					columnRatesForVals.Add(val, dRates);
				}
				#endregion
			}
			else if(threshold != null){
				double parsedThreshold = double.Parse(Threshold);

				#region columnRates
				//initialize decision count
				columnRates.Add(true.ToString(), 0);
				columnRates.Add(false.ToString(), 0);

				//counts decisions
				int dataLength = 0;
				foreach (IList<string> row in Data)
				{
					if (row[Column.Index] != "?")
					{
						if (double.Parse(row[Column.Index]) > parsedThreshold)
							columnRates[true.ToString()]++;
						else columnRates[false.ToString()]++;
					}

					dataLength++;
				}

				//calculates rates of decisions
				columnRates[true.ToString()] = columnRates[true.ToString()] / dataLength;
				columnRates[false.ToString()] = columnRates[false.ToString()] / dataLength;
				#endregion

				#region columnRatesForVals
				//initialize decision count
				IDictionary<string, double> gt_dRates = new Dictionary<string, double>();
				IDictionary<string, double> loet_dRates = new Dictionary<string, double>();
				foreach (string dVal in DecisionColumn.Values)
				{
					gt_dRates.Add(dVal, 0);
					loet_dRates.Add(dVal, 0);
				}

				//counts decisions
				int gt_dataLength = 0;
				int loet_dataLength = 0;
				foreach (IList<string> row in Data)
				{
					if (double.Parse(row[Column.Index]) > parsedThreshold){
						if(row[DecisionColumn.Index] != "?")
							gt_dRates[row[DecisionColumn.Index]]++;
						gt_dataLength++;
					}
					else{
						if (row[DecisionColumn.Index] != "?")
							loet_dRates[row[DecisionColumn.Index]]++;
						loet_dataLength++;
					}
				}

				//calculates rates of decisions
				foreach (string dVal in DecisionColumn.Values)
				{
					gt_dRates[dVal] = gt_dRates[dVal] / gt_dataLength;
					loet_dRates[dVal] = loet_dRates[dVal] / loet_dataLength;
				}

				columnRatesForVals.Add(true.ToString(), gt_dRates);
				columnRatesForVals.Add(false.ToString(), loet_dRates);
				#endregion
			}
		}

		public FormulasHelper(IEnumerable<IList<string>> data, Column decisionColumn)
		{
			DecisionColumn = decisionColumn;
			decisionRates = new Dictionary<string, double>();
			columnRates = new Dictionary<string, double>();
			columnRatesForVals = new Dictionary<string, IDictionary<string, double>>();
			Data = data;
		}

		public double GetEntropy(){
			double result = 0d;
			foreach(string val in DecisionColumn.Values){
				double p = decisionRates[val];
				if (p != 0 && p != 1)
					result += -p * Math.Log2(p);
			}
			return result;
		}

		public double GetEntropy(Column column, string value){
			Column = column;
			double result = 0d;
			foreach (string val in DecisionColumn.Values)
			{
				double p = columnRatesForVals[value][val];
				if (p != 0 && p != 1)
					result += -p * Math.Log2(p);
			}
			return result;
		}

		public double GetEntropy(Column column, string thresholdValue, bool isGreaterThan)//continuous attribute
		{
			setColumnAndThreshold(column, thresholdValue);
			double result = 0d;
			foreach (string val in DecisionColumn.Values)
			{
				double p = columnRatesForVals[isGreaterThan.ToString()][val];
				if (p != 0 && p != 1)
					result += -p * Math.Log2(p);
			}
			return result;
		}

		public double GetInfoGain(Column column)
		{
			Column = column;
			double result = GetEntropy();
			foreach (string val in column.Values)
			{
				double p = columnRates[val];
				result += -p * GetEntropy(column, val);
			}
			return result;
		}

		public double GetInfoGain(Column column, string thresholdValue)//continuous attribute
		{
			setColumnAndThreshold(column, thresholdValue);

			double result = GetEntropy();

			double gt_p = columnRates[true.ToString()];
			double loet_p = columnRates[false.ToString()];
			result += -gt_p * GetEntropy(column, thresholdValue, true);
			result += -loet_p * GetEntropy(column, thresholdValue, false);

			return result;
		}

		public double GetSplitInfo(Column column)
		{
			Column = column;
			double result = 0d;

			foreach(string val in column.Values){
				double p = columnRates[val];
				if (p != 0 && p != 1)
					result += -p * Math.Log2(p);
			}

			return result;
		}

		public double GetSplitInfo(Column column, string thresholdValue)//continuous attribute
		{
			setColumnAndThreshold(column, thresholdValue);
			double result = 0d;

			double gt_p = columnRates[true.ToString()];
			double loet_p = columnRates[false.ToString()];

			if (gt_p != 0 && gt_p != 1)
				result += -gt_p * Math.Log2(gt_p);
			if (loet_p != 0 && loet_p != 1)
				result += -loet_p * Math.Log2(loet_p);

			return result;
		}

		public double GetGainRatio(Column column)
		{
			//Column = column;
			return GetInfoGain(column) / GetSplitInfo(column);
		}

		public double GetGainRatio(Column column, string thresholdValue)//continuous attribute
		{
			//setColumnAndThreshold(column, thresholdValue);
			return GetInfoGain(column, thresholdValue) / GetSplitInfo(column, thresholdValue);
		}
	}
}
