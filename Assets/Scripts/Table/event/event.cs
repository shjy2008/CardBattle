//#file：./tab/event.xlsx, sheet：event，output：event.cs

using System.Collections.Generic;

public class Table_event
{
	public struct Data
	{
		public string eventID;
		public string eventTitle;
		public List<string> eventType;
		public List<string> sceneSound;
		public List<string> sceneImage;
		public List<string> sceneText;
		public List<string> dialogue;
		public List<string> option;

		public Data(string _eventID = "", string _eventTitle = "", List<string> _eventType = null, List<string> _sceneSound = null, List<string> _sceneImage = null, List<string> _sceneText = null, List<string> _dialogue = null, List<string> _option = null)
		{
			eventID = _eventID;
			eventTitle = _eventTitle;
			eventType = _eventType;
			sceneSound = _sceneSound;
			sceneImage = _sceneImage;
			sceneText = _sceneText;
			dialogue = _dialogue;
			option = _option;
		}
	}
	public static Dictionary<string, Data> data = new Dictionary<string, Data> {
		{"event_00001", new Data(_eventID: "event_00001", _eventTitle: "Bandit", _eventType: new List<string>(){ "FIGHT", "RECRUIT", "TRADE" }, _sceneText: new List<string>(){ "test description1", "test description2" }, _dialogue: new List<string>(){ @"{""dialoguerImage"":"""", ""dialoguerName"":""tester1"", ""dialogue"":[""dialogue1"",""dialogue2""]}", @"{""dialoguerImage"":"""", ""dialoguerName"":""tester2"", ""dialogue"":[""dialogue3"",""dialogue4""]}", @"{""dialoguerImage"":"""", ""dialoguerName"":""tester2"", ""dialogue"":[""dialogue5"",""dialogue6""]}", @"{""dialoguerImage"":"""", ""dialoguerName"":""tester2"", ""dialogue"":[""dialogue7"",""dialogue8""]}" }, _option: new List<string>(){ @"{""title"":""fight!"", ""cost"":""money"",""""costValue"":""0"",""action"":""startBattle"", ""actionValue"":""""}", @"{""title"":""trade!"", ""cost"":""money"",""""costValue"":""500"",""action"":""trade"", ""actionValue"":""""}", @"{""title"":""recruit!"", ""cost"":""money"",""""costValue"":""500"",""action"":""startRecruit"", ""actionValue"":""""}", @"{""title"":""leave!"", ""cost"":""money"",""""costValue"":""200"",""action"":""leaveEvent"", ""actionValue"":""""}", @"{""title"":""chance!"", ""cost"":""succesRate"",""""costValue"":""50"",""action"":""multipleAction"", ""actionValue"":""[{""action"":""leaveEvent"", ""actionValue"":""""},{""action"":""leaveEvent"", ""actionValue"":""""}]""}" })},
		{"event_00002", new Data(_eventID: "event_00002", _eventTitle: "Event2", _eventType: new List<string>(){ "FIGHT", "RECRUIT", "TRADE" }, _sceneText: new List<string>(){ "test description1", "test description2" }, _dialogue: new List<string>(){ @"""""", @"""""" }, _option: new List<string>(){ @"{""title"":""fight!"", ""cost"":""money"",""""costValue"":""0"",""action"":""startBattle"", ""actionValue"":""""}", @"{""title"":""fight!"", ""cost"":""money"",""""costValue"":""500"",""action"":""trade"", ""actionValue"":""""}", @"{""title"":""fight!"", ""cost"":""money"",""""costValue"":""500"",""action"":""startRecruit"", ""actionValue"":""""}", @"{""title"":""fight!"", ""cost"":""money"",""""costValue"":""200"",""action"":""leaveEvent"", ""actionValue"":""""}", @"{""title"":""fight!"", ""cost"":""succesRate"",""""costValue"":""50"",""action"":""multipleAction"", ""actionValue"":""[{""action"":""leaveEvent"", ""actionValue"":""""},{""action"":""leaveEvent"", ""actionValue"":""""}]""}" })},
	};
}
