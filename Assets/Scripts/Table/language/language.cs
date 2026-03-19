//#file：./tab/language.xlsx, sheet：language，output：language.cs

using System.Collections.Generic;

public class Table_language
{
	public struct Data
	{
		public string text_id;
		public string text_cns;
		public string text_cnt;
		public string text_en;
		public string text_fr;

		public Data(string _text_id = "", string _text_cns = "", string _text_cnt = "", string _text_en = "", string _text_fr = "")
		{
			text_id = _text_id;
			text_cns = _text_cns;
			text_cnt = _text_cnt;
			text_en = _text_en;
			text_fr = _text_fr;
		}
	}
	public static Dictionary<string, Data> data = new Dictionary<string, Data> {
		{"text_001", new Data(_text_id: "text_001", _text_cns: "简中测试标题", _text_cnt: "繁體測試標題", _text_en: "Title of test", _text_fr: "Titre du test")},
		{"text_002", new Data(_text_id: "text_002", _text_cns: "简中测试描述", _text_cnt: "繁體測試描述", _text_en: "Description of test", _text_fr: "Description du test")},
	};
}
