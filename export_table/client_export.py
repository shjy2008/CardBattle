# -*- coding: utf-8 -*-

import os
import traceback

import sys
reload(sys)
sys.setdefaultencoding('utf8')

from_path = "./tab"
to_path = "../Assets/Scripts/Table/"
#to_path = "./Table"

STRUCT_NAME = "Data"

hasError = False

# import relation_tab_check
import re
import print_color
import tab_filter
import platform
import json
patternDot = re.compile('\.')
def getIntAndFraction(strFloat):
	g_dot = patternDot.search(strFloat)

	if not g_dot:
		# print('strFloat', strFloat)
		return strFloat, "0"

	start_g_dot = g_dot.start()
	end_g_dot = g_dot.end()

	strInteger = str(strFloat[:start_g_dot])
	strFraction = str(strFloat[end_g_dot:])
	strFraction = strFraction[:4]
	while len(strFraction) > 1 and strFraction[-1] == '0':
		strFraction = strFraction[:-1]

	return strInteger, strFraction

# 根据数据和类型，获得导出时的字符串
def data2Str(data, t):
	if t == "int":
		if data == "":
			return 0
		return str(int(data))
	elif t == "float":
		if data == "":
			return 0
		strInteger, strFraction = getIntAndFraction(str(data))
		return "%s.%sf" % (strInteger, strFraction)
	elif t == "double":
		if data == "":
			return 0
		strInteger, strFraction = getIntAndFraction(str(data))
		return "%s.%s" % (strInteger, strFraction)
	elif t == "bool":
		if data == "" or int(data) == 0:
			return "false"
		return "true"
	elif t == "str" or t == "string":
		if isinstance(data, float):
			if int(data) == data:
				data = int(data)
		data = str(data)
		# 把一些特殊字符前面加\
		tmpList = list(data)
		i = 0
		while i < len(tmpList):
			c = tmpList[i]
			if c == '\'' or c == '\"':
				tmpList.insert(i, '\\')
				i += 2
			elif c == '\n':
				del tmpList[i]
				tmpList.insert(i, "\\n")
				i += 1
			else:
				i += 1
		data = "".join(tmpList)
		return '\"%s\"' % data
	elif t == "jsonStr" or t == "jsonString":
		data = data.replace('"', '""') # double the '"' character and then using @ in C#.
		data = '@"' + data + '"'
		return data
	else:
		if t.endswith("_list"):
			itemT = t[0:-len("_list")]
			typeStr = type2TypeStr(itemT)
			items = data.split('|') if ('|' in str(data)) else [data]
			valuesStr = ""
			for item in items:
				item = item.strip()
				s = data2Str(item, itemT)
				if s != "":
					valuesStr += "%s, " % s
			if valuesStr:
				valuesStr = valuesStr[:-2]
			ret = "new List<%s>(){ %s }" % (typeStr, valuesStr)
			return ret
	return data

# 获得某个目录下所有文件（非递归）
def getAllFiles(path):
	dirList = []
	allfilelist=os.listdir(path)
	for file in allfilelist:
		if file.startswith("~"):
			continue
		filepath=os.path.join(path,file)
		filepath = filepath.replace("\\", "/")
		if os.path.isfile(filepath):
			if tab_filter.CheckNeedExportTab(filepath):  # 增量导表
				dirList.append(filepath)
	return dirList

def writeToFile(d, sheet_name, text):
	if not os.path.exists(d):
		os.makedirs(d)
	f = open(os.path.join(d, '%s.cs' % sheet_name), "w")
	f.write(text.encode('utf-8'))
	f.close()

def checkName(s):
	for c in s.decode("utf-8"):
		ok = 'a' <= c <= 'z' or 'A' <= c <= 'Z' or c == '_' or '0' <= c <= '9'
		if not ok:
			return False
	return True

def type2TypeStr(type):
	if type == "str" or type == "jsonStr" or type == "jsonString":
		type = "string"
	ret = type
	if type.endswith("_list"):
		type = type[:-len("_list")]
		ret = "List<%s>" % (type2TypeStr(type))
	return ret

def getTypeDefaultStr(type):
	if type == "int":
		return "0"
	elif type == "float":
		return "0.0f"
	elif type == "double":
		return "0.0"
	elif type == "bool":
		return "false"
	elif type == "str" or type == "string" or type == "jsonString" or type == "jsonStr":
		return "\"\""
	elif type.endswith("_list"):
		return "null"
	return ""

import xlrd
import shutil
def export(need_export_tab):
	# 删除原目录
	if os.path.exists(to_path):
		shutil.rmtree(to_path)

	# 创建一个空目录
	if not os.path.exists(to_path):
		os.makedirs(to_path)

	def createDatas(tabs):
		datas = {}
		for path in tabs:
			workbook = None
			try:
				workbook = xlrd.open_workbook(path)
			except Exception, e:
				print "Invalid file: %s" % path, e.message
			if not workbook:
				continue
			sheet_names = workbook.sheet_names()
			tabName = path.split('/')[-1].split('.')[0]
			if platform.system() == 'Windows':
				tabName = tabName.decode("gb2312")
			for sheet_name in sheet_names:
				datas[sheet_name] = {"path":path, "workbook":workbook, "tabName":tabName}
		return datas
	trunkDatas = createDatas(need_export_tab)

	# 导表
	# paths = getAllFiles("./tab/")
	for sheet_name, data in trunkDatas.iteritems():
		path = data["path"]
		workbook = data["workbook"]
		tabName = data["tabName"]
		if checkName(sheet_name) == False:
			continue

		text = ""

		path = str(path)
		if platform.system() == 'Windows':
			path_desc = path.decode("gbk")
		else:
			path_desc = path
		exportDesc = "//#file：%s, sheet：%s，output：%s" % (path_desc, sheet_name, sheet_name + ".cs")
		text += exportDesc + '\n\n'
		
		if platform.system() == 'Windows':
			exportDesc = exportDesc
		else:
			exportDesc = exportDesc.encode('utf-8')
		print "start: " + exportDesc

		text += "using System.Collections.Generic;\n\n"

		sheet = workbook.sheet_by_name(sheet_name)
		keys = sheet.row_values(1)
		types = sheet.row_values(2)
		datasList = [] # [[id1, id2, id3, ...], [name1, name2, name3, ...], [price1, price2, price3, ...]]
		for i in xrange(len(keys)):
			datas = []
			for j in xrange(3, sheet.nrows):
				data = sheet.row_values(j)[i]
				datas.append(data)
			datasList.append(datas)

		text += "public class Table_%s\n{\n" % sheet_name
		# 写入struct，如果第一行第一列标记NO_STRUCT，则不写入，datastruct链接到冒号后面指定的表结构
		firstGrid = sheet.row_values(0)[0]
		structName = STRUCT_NAME
		if firstGrid.startswith("NO_STRUCT"):
			originName = firstGrid.split(":")[-1]
			structName = "Table_%s.%s" % (originName, STRUCT_NAME) #datastruct链接到冒号后面指定的表结构
		else:
			text += "\tpublic struct %s\n\t{\n" % STRUCT_NAME

			# 先处理一下
			newkeys = []
			newTypes = []
			newDatasList = []
			for i in xrange(len(keys)):
				key = keys[i]
				# if key == "id": # C#不允许用id，改为uid
				# 	key = "uid"
				# 	keys[i] = "uid"
				type = types[i]
				if key != "" and type != "":
					newkeys.append(key)
					newTypes.append(type)
					newDatasList.append(datasList[i])
			keys = newkeys
			types = newTypes
			datasList = newDatasList

			for i in xrange(len(keys)):
				key = keys[i]
				type = types[i]
				text += "\t\tpublic %s %s;\n" % (type2TypeStr(type), key)
			text += "\n"
			text += "\t\tpublic %s(" % STRUCT_NAME
			for i in xrange(len(keys)):
				key = keys[i]
				type = types[i]
				text += "%s _%s = %s, " % (type2TypeStr(type), key, getTypeDefaultStr(type))
			if len(keys):
				text = text[0:-2]
			text += ")\n"
			text += "\t\t{\n"
			for key in keys:
				text += "\t\t\t%s = _%s;\n" % (key, key)
			text += "\t\t}\n"
			text += "\t}\n"

		# 写入数据dict，如果第一行第一列标记NO_DATA，则不写入
		if sheet.row_values(0)[0] != "NO_DATA":
			primaryKeyTypeStr = type2TypeStr(types[0])

			text += "\tpublic static Dictionary<%s, %s> data = new Dictionary<%s, %s> {\n" % (primaryKeyTypeStr, structName, primaryKeyTypeStr, structName)

			existKeyList = []
			def getLineStr(_datas):
				try:
					if len(str(_datas[0])) == 0:
						return
					elif str(_datas[0]).startswith('#'):
						# if not is_server:
						# print_color.PrintYellowText(
						# 	'[warning] "{}" 表中主键 ID = {} 已忽略.'.format(tabName, datas[0]).encode('gb2312')
						# )
						return
					datas = _datas
					dataValueStr = ""
					dataKeyStr = data2Str(datas[0], types[0])
					#line += dataKeyStr
					_keys = keys
					_types = types

					#line += ":{"
					for j in xrange(len(_keys)):
						if len(str(_keys[j])) > 0 and len(str(_types[j])) > 0 and len(datas) > j and len(str(datas[j])) > 0:
							# line += "\'%s\':%s, " % (
							# _keys[j], data2Str(datas[j], _types[j]))
							dataValueStr += "_%s: " % str(_keys[j])
							dataValueStr += data2Str(datas[j], _types[j])
							dataValueStr += ", "
					#line += "},\n"
					dataValueStr = dataValueStr[:-2] # 删掉最后的逗号

					if datas[0] not in existKeyList:
						existKeyList.append(datas[0])
					else:
						print_color.PrintRedText(
							'[error] "{}" 表中主键 ID = {} 冲突.'.format(tabName, datas[0]).encode('gb2312')
						)
						raise print_color.CustomExpection()
					line = "\t\t{%s, new %s(%s)},\n" % (dataKeyStr, structName, dataValueStr)
					return line
				except Exception, e:
					global hasError
					hasError = True
					if isinstance(e, print_color.CustomExpection):
						return
					print '[error]: ', e.message
					print traceback.format_exc()
					print "[error at %s]" % exportDesc
					# print "[error at line: %d, key:%s]\n" % (i + 1, keys[j])

			# 写入行
			lineDatasList = []
			for i in xrange(len(datasList[0])):
				lineDatas = []
				for j in xrange(len(datasList)):
					lineDatas.append(datasList[j][i])
				lineDatasList.append(lineDatas)

			for datas in lineDatasList:
				line = getLineStr(datas)
				if line:
					text += line
			text += "\t};\n"

		text += "}\n"

		writeToFile(to_path + "/" + tabName, sheet_name, text)

		print "finished: " + exportDesc

if __name__ == '__main__':
	try:
		is_all = 1 #int(sys.argv[1])
		tab_filter.TabFilter_Enable = (is_all != 1)  # 1代表全部导表
	except BaseException,e:
		print '[error]: ', e.message
		print traceback.format_exc()
	
	need_export_tab = []
	if os.path.exists(from_path):
		need_export_tab = getAllFiles(from_path)  # 增量导表

	export(need_export_tab)

	if not hasError:
		tab_filter.Close()
	else:
		print_color.PrintRedText(
			'[error] 导表出现错误，请查看上方信息！！！'.encode('gb2312')
		)





