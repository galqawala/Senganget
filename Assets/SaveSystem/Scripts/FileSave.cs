using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

namespace SaveSystem
{
    /// <summary>
    /// Helps you saving and loading data to or from files.
    /// Check out "Example.cs" to see how to use this class.
    /// </summary>
    public class FileSave
    {
        public FileFormat fileFormat;

        private delegate void SaveAction<T>(string filePath, T toSave);
        private delegate T LoadAction<T>(string filePath);

        #region CONSTRUCTOR
        public FileSave(FileFormat fileFormat)
        {
            this.fileFormat = fileFormat;
        }
        #endregion

        #region WRITE
        public void WriteToFile<T>(string filePath, T toSave)
        {
            SaveAction<T> saveAction = null;
            switch (fileFormat)
            {
                case FileFormat.Binary:
                    saveAction = WriteBinary;
                    break;
                case FileFormat.Xml:
                    saveAction = WriteXml;
                    break;
                case FileFormat.Json:
                    saveAction = WriteJson;
                    break;
            }
            saveAction.Invoke(filePath, toSave);
        }

        #region BINARY
        private void WriteBinary<T>(string filePath, T toSave)
        {
            try
            {
                using (Stream stream = File.Open(filePath, FileMode.Create))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(stream, toSave);
                }
            }
            catch (Exception exc)
            {
                Debug.LogError("Error while saving: Read ERRORS-file.\n" + exc);
            }
        }
        #endregion

        #region XML
        private void WriteXml<T>(string filePath, T toSave)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (TextWriter writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, toSave);
                }
            }
            catch (Exception exc)
            {
                Debug.LogError("Error while saving: Read ERRORS-file.\n" + exc);
            }
        }
        #endregion

        #region JSON
        private void WriteJson<T>(string filePath, T toSave)
        {
            try
            {
                string jsonString = JsonUtility.ToJson(toSave);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception exc)
            {
                Debug.LogError("Error while saving: Read ERRORS-file.\n" + exc);
            }
        }
        #endregion
        #endregion

        #region READ
        public T ReadFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return default(T);

            return ReadFile<T>(filePath);
        }
        public T ReadFromFile<T>(string filePath, T defaultValue)
        {
            if (!File.Exists(filePath))
                return defaultValue;

            return ReadFile<T>(filePath);
        }

        #region READ FILE
        private T ReadFile<T>(string filePath)
        {
            LoadAction<T> loadAction = null;
            switch (fileFormat)
            {
                case FileFormat.Binary:
                    loadAction = ReadBinary<T>;
                    break;
                case FileFormat.Xml:
                    loadAction = ReadXml<T>;
                    break;
                case FileFormat.Json:
                    loadAction = ReadJson<T>;
                    break;
            }
            return loadAction.Invoke(filePath);
        }
        #endregion

        #region BINARY
        private T ReadBinary<T>(string filePath)
        {
            try
            {
                using (Stream stream = File.Open(filePath, FileMode.Open))
                {
                    var binaryFormatter = new BinaryFormatter();
                    return (T)binaryFormatter.Deserialize(stream);
                }
            }
            catch (Exception exc)
            {
                Debug.LogError("Error while loading: Read ERRORS-file.\n" + exc);
                return default(T);
            }
        }
        #endregion

        #region XML
        private T ReadXml<T>(string filePath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (TextReader reader = new StreamReader(filePath))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception exc)
            {
                Debug.LogError("Error while loading: Read ERRORS-file.\n" + exc);
                return default(T);
            }
        }
        #endregion

        #region JSON
        private T ReadJson<T>(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                return JsonUtility.FromJson<T>(jsonString);
            }
            catch (Exception exc)
            {
                Debug.LogError("Error while loading: Read ERRORS-file.\n" + exc);
                return default(T);
            }
        }
        #endregion
        #endregion
    }
}