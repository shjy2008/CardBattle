# -*- coding:utf8 -*-
import os
import hashlib
import platform
TabFilter_Log = True
TabFilter_Enable = True


class TabFilter(object):
    def __init__(self):
        self.file_md5_dict = None
        self.force_file_md5_dict = None
        self.log_file = None
        self.InitFileMD5Dict()
        self.InitForceFileMD5Dict()
        self.InitLogFile()

    def InitLogFile(self):
        if TabFilter_Log:
            self.log_file = open('TabFilterLog.txt', 'w')

    def InitForceFileMD5Dict(self):
        self.force_file_md5_dict = {}

    def InitFileMD5Dict(self):
        temp = open('TabMD5', 'a+')
        if temp:
            self.file_md5_dict = {}
            for line in temp:
                file_name, file_md5, file_size = line.rstrip().split(',')  # 存的是每一个文件对应的md5,size
                self.file_md5_dict[file_name] = {
                    'md5': file_md5,
                    'size': long(file_size),
                    'visited': False,  # 在导表时会遍历一次这个dict,遍历到的就把这个值置为True,保证删除表格也能处理到
                }
            temp.close()
        else:
            pass
        if self.file_md5_dict is None:
            raise Exception('Failed to Init TabFilter')

    def WriteFileMD5Dict(self):
        temp = open('TabMD5', 'w')
        for file_name, data in self.file_md5_dict.items():
            if not data.get('visited', None):  # 没遍历到的表就不要写进去了,因为估计是被删除了
                self.Log('Delete file={}'.format(file_name))
                continue
            temp.writelines(str(file_name)+','+str(data['md5'])+','+str(data['size'])+'\n')
        temp.close()

    def WriteForceFileMD5Dict(self):
        if self.force_file_md5_dict is None:
            return
        temp = open('TabMD5', 'w')
        for file_name, data in self.force_file_md5_dict.items():
            temp.writelines(str(file_name) + ',' + str(data['md5']) + ',' + str(data['size']) + '\n')
        temp.close()

    # export_all的时候用的,用于把所有xlsx的md5写入当前文件.
    def RecordForceFileMD5Dict(self, file_path):
        if file_path is not None:
            # 计算本地文件md5
            # try:
            tmp = open(file_path, 'rb')
            cur_md5 = hashlib.md5(tmp.read()).hexdigest()
            cur_size = os.path.getsize(file_path)
            tmp.close()
            # except IOError, ioe:
            #     pass
            # except BaseException, e:
            #     exit(e)

            if platform.system() == 'Windows':
                file_name = file_path.decode('gb2312').encode('utf-8')
            else:
                file_name = file_path
            self.force_file_md5_dict[file_name] = {
                'md5': cur_md5,
                'size': cur_size,
            }

    # 检查文件的md5看是否相同,相同则进一步比较大小
    def CheckNeedExportTab(self, file_path):
        if not TabFilter_Enable:
            self.RecordForceFileMD5Dict(file_path)
            return True
        if file_path is not None:
            # 计算本地文件md5
            try:
                tmp = open(file_path, 'rb')
                cur_md5 = hashlib.md5(tmp.read()).hexdigest()
                cur_size = os.path.getsize(file_path)
                tmp.close()
            except IOError, ioe:
                return True
                # if ioe.errno == 13:
                #     # .encode('gb2312')
                #     tab_name = file_path.split('/')[-1].split('.')[0]
                #     exit('请关闭 ({}) 文件再进行导表'.encode('gb2312').format(tab_name))
                # else:
                #     exit(ioe)
            except BaseException, e:
                exit(e)

            if platform.system() == 'Windows':
                file_name = file_path.decode('gb2312').encode('utf-8')
            else:
                file_name = file_path
            file_data = self.file_md5_dict.get(file_name, None)

            if file_data is None:  # 表示这个表格是新增的,需要计算md5,size并写入文件的
                self.file_md5_dict[file_name] = {
                    'md5': cur_md5,
                    'size': cur_size,
                    'visited': True,
                }
                self.Log('Add file={}'.format(file_name))
                return True
            else:
                self.file_md5_dict[file_name]['visited'] = True
                if self.file_md5_dict[file_name]['md5'] != cur_md5:
                    self.file_md5_dict[file_name]['md5'] = cur_md5
                    self.file_md5_dict[file_name]['size'] = cur_size
                    self.Log('Update file={}'.format(file_name))
                    return True
                elif self.file_md5_dict[file_name]['size'] != cur_size:
                    self.file_md5_dict[file_name]['size'] = cur_size
                    self.Log('Update file={}'.format(file_name))
                    return True

        return False

    def Close(self):
        self.WriteFileMD5Dict()
        if self.log_file:
            self.log_file.close()
        if not TabFilter_Enable:
            self.WriteForceFileMD5Dict()

    def Log(self, info):
        if TabFilter_Log:
            self.log_file.writelines(str(info)+'\n')


__TabFilterIns = TabFilter()
CheckNeedExportTab = __TabFilterIns.CheckNeedExportTab
Close = __TabFilterIns.Close
file_md5_dict = __TabFilterIns.file_md5_dict

