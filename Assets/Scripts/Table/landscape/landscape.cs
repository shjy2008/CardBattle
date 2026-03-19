//#file：./tab/landscape.xlsx, sheet：landscape，output：landscape.cs

using System.Collections.Generic;

public class Table_landscape
{
	public struct Data
	{
		public string id;
		public bool ifWeather;
		public string name;
		public string faceimage;
		public string backImage;
		public string element;
		public List<string> modifier;
		public string description;
		public string icon;

		public Data(string _id = "", bool _ifWeather = false, string _name = "", string _faceimage = "", string _backImage = "", string _element = "", List<string> _modifier = null, string _description = "", string _icon = "")
		{
			id = _id;
			ifWeather = _ifWeather;
			name = _name;
			faceimage = _faceimage;
			backImage = _backImage;
			element = _element;
			modifier = _modifier;
			description = _description;
			icon = _icon;
		}
	}
	public static Dictionary<string, Data> data = new Dictionary<string, Data> {
		{"landscape_0001", new Data(_id: "landscape_0001", _ifWeather: false, _name: "HILL", _faceimage: "landscape_0001", _element: "EARTH", _modifier: new List<string>(){ @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""priotizedAttack"", ""modifiedValue"":""-0.15""}", @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""projectileAttack"", ""modifiedValue"":""0.15""}" }, _icon: "landscape_0001")},
		{"landscape_0002", new Data(_id: "landscape_0002", _ifWeather: false, _name: "MONTAIN", _faceimage: "landscape_0002", _element: "METAL", _modifier: new List<string>(){ @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""priotizedAttack"", ""modifiedValue"":""-0.3""}" }, _icon: "landscape_0002")},
		{"landscape_0003", new Data(_id: "landscape_0003", _ifWeather: false, _name: "PLAIN", _faceimage: "landscape_0003", _element: "EARTH", _modifier: new List<string>(){ @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""priotizedAttack"", ""modifiedValue"":""+0.2""}" }, _icon: "landscape_0003")},
		{"landscape_0004", new Data(_id: "landscape_0004", _ifWeather: false, _name: "DESERT", _faceimage: "landscape_0004", _element: "FIRE", _modifier: new List<string>(){ @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""priotizedAttack"", ""modifiedValue"":""0.15""}", @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""projectileAttack"", ""modifiedValue"":""-0.15""}" }, _icon: "landscape_0004")},
		{"landscape_0005", new Data(_id: "landscape_0005", _ifWeather: false, _name: "FOREST", _faceimage: "landscape_0005", _element: "WOOD", _modifier: new List<string>(){ @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""priotizedAttack"", ""modifiedValue"":""-0.15""}", @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""projectileAttack"", ""modifiedValue"":""-0.15""}" }, _icon: "landscape_0005")},
		{"landscape_0006", new Data(_id: "landscape_0006", _ifWeather: false, _name: "BUSH FIELD", _faceimage: "landscape_0006", _element: "WOOD", _modifier: new List<string>(){ @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""meleeAttack"", ""modifiedValue"":""+0.2""}" }, _icon: "landscape_0006")},
		{"landscape_1001", new Data(_id: "landscape_1001", _ifWeather: true, _name: "RAINY WEATHER", _faceimage: "landscape_1001", _element: "WATER", _modifier: new List<string>(){ @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""armorPiercingAttack"", ""modifiedValue"":""-0.2""}", @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""siegeAttack"", ""modifiedValue"":""-0.2""}" }, _icon: "landscape_1001")},
		{"landscape_1002", new Data(_id: "landscape_1002", _ifWeather: true, _name: "MISTY WEATHER", _faceimage: "landscape_1002", _element: "WATER", _modifier: new List<string>(){ @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""priotizedAttack"", ""modifiedValue"":""-0.15""}", @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""projectileAttack"", ""modifiedValue"":""0.15""}" }, _icon: "landscape_1002")},
		{"landscape_1003", new Data(_id: "landscape_1003", _ifWeather: true, _name: "SUNNY WEATHER", _faceimage: "landscape_1003", _element: "METAL", _icon: "landscape_1003")},
		{"landscape_1004", new Data(_id: "landscape_1004", _ifWeather: true, _name: "THUNDERSTORM WEATHER", _faceimage: "landscape_1004", _element: "FIRE", _modifier: new List<string>(){ @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""armorPiercingAttack"", ""modifiedValue"":""-0.2""}", @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""siegeAttack"", ""modifiedValue"":""-0.2""}", @"{""from"":""NONE"", ""target"":""ALL"",""effect"":""projectileAttack"", ""modifiedValue"":""0.2""}" }, _icon: "landscape_1004")},
	};
}
