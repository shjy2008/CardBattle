//#file：./tab/level.xlsx, sheet：level，output：level.cs

using System.Collections.Generic;

public class Table_level
{
	public struct Data
	{
		public string id;
		public string difficulty1;
		public string difficulty2;
		public string difficulty3;
		public string difficulty4;
		public string difficulty5;

		public Data(string _id = "", string _difficulty1 = "", string _difficulty2 = "", string _difficulty3 = "", string _difficulty4 = "", string _difficulty5 = "")
		{
			id = _id;
			difficulty1 = _difficulty1;
			difficulty2 = _difficulty2;
			difficulty3 = _difficulty3;
			difficulty4 = _difficulty4;
			difficulty5 = _difficulty5;
		}
	}
	public static Dictionary<string, Data> data = new Dictionary<string, Data> {
		{"map1", new Data(_id: "map1", _difficulty1: "[[2,3,6,4,5],13]", _difficulty2: "[[3,4,7,5,5],14]", _difficulty3: "[[3,4,7,5,5],14]", _difficulty4: "[[3,4,7,5,5],14]", _difficulty5: "[[3,4,7,5,5],15]")},
		{"map2", new Data(_id: "map2", _difficulty1: "[[3,4,7,5,5],14]", _difficulty2: "[[4,4,8,6,5],15]", _difficulty3: "[[4,4,8,6,5],15]", _difficulty4: "[[4,5,9,7,7],16]", _difficulty5: "[[5,5,10,7,8],17]")},
		{"map3", new Data(_id: "map3", _difficulty1: "[[4,4,8,6,5],15]", _difficulty2: "[[5,5,9,7,7],16]", _difficulty3: "[[5,5,9,7,7],17]", _difficulty4: "[[6,6,10,7,8],17]", _difficulty5: "[[6,7,10,7,8],18]")},
		{"map4", new Data(_id: "map4", _difficulty1: "[[5,5,9,7,7],16]", _difficulty2: "[[6,6,10,7,8],17]", _difficulty3: "[[6,6,10,7,8],18]", _difficulty4: "[[7,8,10,9,8],19]", _difficulty5: "[[8,9,10,9,8],21]")},
		{"map5", new Data(_id: "map5", _difficulty1: "[[6,6,10,7,8],18]", _difficulty2: "[[7,7,10,9,8],19]", _difficulty3: "[[7,8,10,9,8],20]", _difficulty4: "[[9,9,10,9,8],22]", _difficulty5: "[[9,10,10,10,8],24]")},
	};
}
