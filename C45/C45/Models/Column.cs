using System.Collections.Generic;

namespace C45.Models
{
	public class Column
	{
		public string Name { get; set; }
		public int Index { get; set; }
		public bool IsNominal { get; set; }
		public IList<string> Values { get; set; }

		public Column(string name, int index, bool isNominal)
		{
			Name = name;
			Index = index;
			IsNominal = isNominal;
			Values = new List<string>();
		}

		public Column Clone() => (Column)MemberwiseClone();

		public Column DeepClone()
		{
			Column result = Clone();
			result.Values = new List<string>(Values);
			return result;
		}
	}
}
