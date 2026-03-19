
using Assets.Scripts.data;
using Assets.Scripts.utility;
using Assets.Scripts.utility.CommonInterface;
using Assets.Scripts.utility.Singleton;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.managers.archivemgr
{
    public class ArchiveManager : TSingleton<ArchiveManager>, IManager, IOnAllManagerInit, IBeforeAllManagerDestroy
    {
        private ArchiveManager() { }

        private Dictionary<string, ArchiveData> archiveDataDict;

        private const string relativeFolderPath = "/Saves/";
        private const string archiveSuffix = ".sav";
        private const string encryptKey = "CardGameXORKey";

        private string curArchiveDataName;

        public void Init()
        {

        }


        public void OnAllManagerInit()
        {
            ReadAllArchiveDataFromFiles();

            // TODO: to read the archiveDataName from PlayerPrefs
            curArchiveDataName = "default";

            // if no .sav files, then create one
            if (archiveDataDict.Count <= 0)
            {
                CreateDefaultArchiveData("default");
            }
        }

        public void BeforeAllManagerDestroy()
        {

        }

        public void Update()
        {

        }

        public void Destroy()
        {
            
        }

        private List<string> GetAllArchiveFilePaths()
        {
            var result = new List<string>();
            string archiveFolder = GetArchiveFolderPath();
            Debug.LogFormat("archiveFolder: {0}", archiveFolder);
            if(!Directory.Exists(archiveFolder))
            {
                Debug.Log("not found, create a new folder.");
                Directory.CreateDirectory(archiveFolder);
            }
            else
            {
                Debug.Log("found!");
            }

            var allFiles = Directory.GetFiles(archiveFolder);
            foreach (var file in allFiles)
            {
                if(file.Contains(archiveSuffix))
                {
                    Debug.LogFormat("file: {0}", file);
                    result.Add(file);
                }
            }

            return result;
        }

        private ArchiveData ReadFromFile(string file)
        {
            var bytes = File.ReadAllBytes(file);
            var decryptStr = Utils.DecryptToString(bytes, encryptKey);
            // [Note] when we deserialize the obejct below, we need to specify the 'Replace' in settings,
            // otherwise it will duplicate the data.
            var result = JsonConvert.DeserializeObject<ArchiveData>(decryptStr, new JsonSerializerSettings() { ObjectCreationHandling = ObjectCreationHandling.Replace });
            return result;
        }

        private void SaveToFile(string file, ArchiveData archiveData)
        {
            string jsonStr = JsonConvert.SerializeObject(archiveData);
            //Debug.LogFormat("jsonStr: {0}", jsonStr);

            var bytes = Utils.EncryptToBytes(jsonStr, encryptKey);
            if (Directory.Exists(file))
            {
                Debug.LogFormat("Warning, trying to overwrite the file: {0}", file);
                using (var fs = File.OpenWrite(file))
                {
                    fs.Write(bytes);
                }
            }
            else
            {
                using (var fs = File.Create(file))
                {
                    fs.Write(bytes);
                }
            }
        }

        private void ReadAllArchiveDataFromFiles()
        {
            archiveDataDict = new Dictionary<string, ArchiveData>();
            var archivePathList = GetAllArchiveFilePaths();
            foreach(var archivePath in archivePathList)
            {
                var archiveData = ReadFromFile(archivePath);
                archiveDataDict.Add(archiveData.GetName(), archiveData);
            }
        }

        public void SaveAllArchiveDataToFile()
        {
            Debug.Log("SaveAllArchiveDataToFile");
            string archiveFolder = GetArchiveFolderPath();
            foreach (var data in archiveDataDict.Values)
            {
                string archiveDataFilePath = string.Format("{0}{1}{2}", archiveFolder, data.GetName(), archiveSuffix);
                SaveToFile(archiveDataFilePath, data);
            }
        }

        public string GetArchiveFolderPath()
        {
            return Application.persistentDataPath + relativeFolderPath;
        }

        // create a new .sav file.
        public ArchiveData CreateDefaultArchiveData(string name)
        {
            if(archiveDataDict.ContainsKey(name))
            {
                Debug.LogError(string.Format("Already exist {0}.sav", name));
                return null;
            }

            var data = new ArchiveData(name);
            data.InitByDefault();
            archiveDataDict.Add(name, data);
            SaveAllArchiveDataToFile();
            return data;
        }

        public ArchiveData GetCurrentArchiveData()
        {
            return archiveDataDict[curArchiveDataName];
        }

        public void SaveCurrentArchiveDataToFile()
        {
            Debug.Log("SaveDefaultArchiveDataToFile");
            var curArchiveData = GetCurrentArchiveData();
            string archiveFolder = GetArchiveFolderPath();
            string archiveDataFilePath = string.Format("{0}{1}{2}", archiveFolder, curArchiveData.GetName(), archiveSuffix);
            SaveToFile(archiveDataFilePath, curArchiveData);
        }
    }
}
