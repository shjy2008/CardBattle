//#file：./tab/tip_info.xlsx, sheet：tip_info，output：tip_info.cs

using System.Collections.Generic;

public class Table_tip_info
{
	public struct Data
	{
		public string id;
		public string title_id;
		public string description_id;

		public Data(string _id = "", string _title_id = "", string _description_id = "")
		{
			id = _id;
			title_id = _title_id;
			description_id = _description_id;
		}
	}
	public static Dictionary<string, Data> data = new Dictionary<string, Data> {
		{"tip_info_001", new Data(_id: "tip_info_001", _title_id: "text_001", _description_id: "text_002")},
	};
}
