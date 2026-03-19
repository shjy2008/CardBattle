//#file：./tab/constant.xlsx, sheet：constant，output：constant.cs

using System.Collections.Generic;

public class Table_constant
{
	public struct Data
	{
		public string id;
		public int param_int;
		public List<int> param_int_list;
		public float param_float;
		public List<float> param_float_list;
		public string param_string;
		public List<string> param_string_list;

		public Data(string _id = "", int _param_int = 0, List<int> _param_int_list = null, float _param_float = 0.0f, List<float> _param_float_list = null, string _param_string = "", List<string> _param_string_list = null)
		{
			id = _id;
			param_int = _param_int;
			param_int_list = _param_int_list;
			param_float = _param_float;
			param_float_list = _param_float_list;
			param_string = _param_string;
			param_string_list = _param_string_list;
		}
	}
	public static Dictionary<string, Data> data = new Dictionary<string, Data> {
		{"constant_0001", new Data(_id: "constant_0001", _param_float: 15.0f)},
		{"constant_0002", new Data(_id: "constant_0002", _param_float_list: new List<float>(){ 5.0f, 7.5f })},
		{"constant_0003", new Data(_id: "constant_0003", _param_float_list: new List<float>(){ 0.0f, 9.0f })},
		{"constant_0004", new Data(_id: "constant_0004", _param_float: 0.4f)},
		{"constant_0005", new Data(_id: "constant_0005", _param_float: 0.05f)},
		{"constant_0006", new Data(_id: "constant_0006", _param_float: 0.05f)},
		{"constant_0007", new Data(_id: "constant_0007", _param_float: 0.8f)},
		{"constant_0008", new Data(_id: "constant_0008", _param_float: 0.4f)},
		{"constant_0009", new Data(_id: "constant_0009", _param_float: 0.025f)},
		{"constant_0010", new Data(_id: "constant_0010", _param_float: 0.125f)},
		{"constant_0011", new Data(_id: "constant_0011", _param_float: 0.1f)},
		{"constant_0012", new Data(_id: "constant_0012", _param_float: 0.5f)},
		{"constant_0013", new Data(_id: "constant_0013", _param_float_list: new List<float>(){ 9.0f, 3.0f })},
		{"constant_0014", new Data(_id: "constant_0014", _param_float_list: new List<float>(){ 6.0f, 2.0f })},
		{"constant_0015", new Data(_id: "constant_0015", _param_float: 2.0f)},
		{"constant_0016", new Data(_id: "constant_0016", _param_float: 0.5f)},
		{"constant_0017", new Data(_id: "constant_0017", _param_float: 0.3f)},
		{"constant_0018", new Data(_id: "constant_0018", _param_float: 0.6f)},
		{"constant_0019", new Data(_id: "constant_0019", _param_int: 6)},
		{"constant_0020", new Data(_id: "constant_0020", _param_float: 0.05f)},
		{"constant_0021", new Data(_id: "constant_0021", _param_float: 0.02f)},
	};
}
