using System.Xml;
using System;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Text;

namespace SaveSystem
{
    public static class EasySave
    {
        private delegate void SaveAction(string key, object value);
        private delegate object LoadAction(string key);
        private delegate bool ExistsAction(string key);
        private delegate void DeleteAction(string key);

        #region SAVE
        public static void Save<T>(string key, T value)
        {
            SaveAction saveAction = null;

            #region COMMON TYPES
            if (value is int)
                saveAction = SetInt;
            else if (value is float)
                saveAction = SetFloat;
            else if (value is string)
                saveAction = SetString;

            else if (value is bool)
                saveAction = SetBool;
            else if (value is double)
                saveAction = SetDouble;
            else if (value is long)
                saveAction = SetLong;
            else if (value is byte)
                saveAction = SetByte;
            else if (value is short)
                saveAction = SetShort;
            else if (value is char)
                saveAction = SetChar;
            else if (value is DateTime)
                saveAction = SetDateTime;
            #endregion

            #region UNSIGNED TYPES
            else if (value is uint)
                saveAction = SetUnsignedInt;
            else if (value is ulong)
                saveAction = SetUnsignedLong;
            else if (value is ushort)
                saveAction = SetUnsignedShort;
            #endregion

            #region OBJECT
            else
                saveAction = SetObject<T>;
            #endregion

            saveAction.Invoke(key, value);
        }

        #region SAVING
        #region COMMON TYPES
        private static void SetInt(string key, object value)
        {
            PlayerPrefs.SetInt(key + "int", (int)value);
        }
        private static void SetFloat(string key, object value)
        {
            PlayerPrefs.SetFloat(key + "float", (float)value);
        }
        private static void SetString(string key, object value)
        {
            PlayerPrefs.SetString(key + "string", (string)value);
        }

        private static void SetBool(string key, object value)
        {
            PlayerPrefs.SetString(key + "bool", (bool)value == true ? "1" : "0");
        }
        private static void SetDouble(string key, object value)
        {
            SetLong(key + "double", BitConverter.DoubleToInt64Bits((double)value));
        }
        private static void SetLong(string key, object value)
        {
            byte[] bytes = BitConverter.GetBytes((long)value);
            for (byte i = 0; i < 8; i++)
            {
                SetByte(key + "long" + i, bytes[i]);
            }
        }
        private static void SetByte(string key, object value)
        {
            PlayerPrefs.SetString(key + "byte", ((byte)value).ToString());
        }
        private static void SetShort(string key, object value)
        {
            PlayerPrefs.SetInt(key + "short", (short)value);
        }
        private static void SetChar(string key, object value)
        {
            PlayerPrefs.SetString(key + "char", ((char)value).ToString());
        }
        private static void SetDateTime(string key, object value)
        {
            PlayerPrefs.SetString(key + "DateTime", ((DateTime)value).ToString());
        }
        #endregion

        #region UNSIGNED TYPES
        private static void SetUnsignedInt(string key, object value)
        {
            PlayerPrefs.SetInt(key + "uint", (int)(uint)value);
        }
        private static void SetUnsignedShort(string key, object value)
        {
            PlayerPrefs.SetInt(key + "ushort", (ushort)value);
        }
        private static void SetUnsignedLong(string key, object value)
        {
            SetLong(key + "ulong", (long)(ulong)value);
        }
        #endregion

        #region OBJECT
        private static void SetObject<T>(string key, object toSerialize)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
                MemoryStream memStrm = new MemoryStream();
                UTF8Encoding utf8e = new UTF8Encoding();
                XmlTextWriter xmlSink = new XmlTextWriter(memStrm, utf8e);
                xmlSerializer.Serialize(xmlSink, toSerialize);
                byte[] utf8EncodedData = memStrm.ToArray();
                PlayerPrefs.SetString(key + toSerialize.GetType().Name, utf8e.GetString(utf8EncodedData));
            }
            catch (Exception exc)
            {
                Debug.LogError("Error while saving: Read ERRORS-file.\n" + exc);
            }
        }
        #endregion
        #endregion
        #endregion

        #region LOAD
        public static T Load<T>(string key)
        {
            if (!HasKey<T>(key))
                return default(T);
            return LoadValues<T>(key);
        }
        public static T Load<T>(string key, T defaultValue)
        {
            if (!HasKey<T>(key))
                return defaultValue;
            return LoadValues<T>(key);
        }

        #region LOAD VALUES
        private static T LoadValues<T>(string key)
        {
            LoadAction loadAction;
            Type type = typeof(T);

            #region COMMON TYPES
            if (type == typeof(int))
                loadAction = GetInt;
            else if (type == typeof(float))
                loadAction = GetFloat;
            else if (type == typeof(string))
                loadAction = GetString;

            else if (type == typeof(bool))
                loadAction = GetBool;
            else if (type == typeof(double))
                loadAction = GetDouble;
            else if (type == typeof(long))
                loadAction = GetLong;
            else if (type == typeof(byte))
                loadAction = GetByte;
            else if (type == typeof(short))
                loadAction = GetShort;
            else if (type == typeof(char))
                loadAction = GetChar;
            else if (type == typeof(DateTime))
                loadAction = GetDateTime;
            #endregion

            #region UNSIGNED TYPES
            else if (type == typeof(uint))
                loadAction = GetUnsignedInt;
            else if (type == typeof(ulong))
                loadAction = GetUnsignedLong;
            else if (type == typeof(ushort))
                loadAction = GetUnsignedShort;
            #endregion

            #region OBJECT
            //Object
            else
                loadAction = GetObject<T>;
            #endregion

            object loaded = loadAction.Invoke(key);
            return (T)loaded;
        }
        #endregion

        #region LOADING
        #region COMMON TYPES
        private static object GetInt(string key)
        {
            return PlayerPrefs.GetInt(key + "int");
        }
        private static object GetFloat(string key)
        {
            return PlayerPrefs.GetFloat(key + "float");
        }
        private static object GetString(string key)
        {
            return PlayerPrefs.GetString(key + "string");
        }

        private static object GetBool(string key)
        {
            return PlayerPrefs.GetString(key + "bool") == "1";
        }
        private static object GetDouble(string key)
        {
            long longVal = (long)GetLong(key + "double");
            return BitConverter.ToDouble(BitConverter.GetBytes(longVal), 0);
        }
        private static object GetLong(string key)
        {
            byte[] bytes = new byte[8];
            for (byte i = 0; i < 8; i++)
            {
                bytes[i] = (byte)GetByte(key + "long" + i);
            }

            return BitConverter.ToInt64(bytes, 0);
        }
        private static object GetByte(string key)
        {
            return byte.Parse(PlayerPrefs.GetString(key + "byte"));
        }
        private static object GetShort(string key)
        {
            return (short)PlayerPrefs.GetInt(key + "short");
        }
        private static object GetChar(string key)
        {
            return PlayerPrefs.GetString(key + "char")[0];
        }
        private static object GetDateTime(string key)
        {
            return DateTime.Parse(PlayerPrefs.GetString(key + "DateTime"));
        }
        #endregion

        #region UNSIGNED TYPES
        private static object GetUnsignedInt(string key)
        {
            return (uint)PlayerPrefs.GetInt(key + "uint");
        }
        private static object GetUnsignedShort(string key)
        {
            return (ushort)PlayerPrefs.GetInt(key + "ushort");
        }
        private static object GetUnsignedLong(string key)
        {
            return (ulong)(long)GetLong(key + "ulong");
        }
        #endregion

        #region OBJECT
        private static object GetObject<T>(string key)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

                using (TextReader textReader = new StringReader(PlayerPrefs.GetString(key + typeof(T).Name)))
                {
                    return (T)xmlSerializer.Deserialize(textReader);
                }
            }
            catch (Exception exc)
            {
                Debug.LogError("Error while loading: Read ERRORS-file.\n" + exc);
                return default(T);
            }
        }
        #endregion
        #endregion
        #endregion

        #region HAS KEY
        public static bool HasKey<T>(string key)
        {
            ExistsAction existsAction;
            Type type = typeof(T);

            #region COMMON TYPES
            if (type == typeof(int))
                existsAction = ExistsInt;
            else if (type == typeof(float))
                existsAction = ExistsFloat;
            else if (type == typeof(string))
                existsAction = ExistsString;

            else if (type == typeof(bool))
                existsAction = ExistsBool;
            else if (type == typeof(double))
                existsAction = ExistsDouble;
            else if (type == typeof(long))
                existsAction = ExistsLong;
            else if (type == typeof(byte))
                existsAction = ExistsByte;
            else if (type == typeof(short))
                existsAction = ExistsShort;
            else if (type == typeof(char))
                existsAction = ExistsChar;
            else if (type == typeof(DateTime))
                existsAction = ExistsDateTime;
            #endregion

            #region UNSIGNED TYPES
            else if (type == typeof(uint))
                existsAction = ExistsUnsignedInt;
            else if (type == typeof(ulong))
                existsAction = ExistsUnsignedLong;
            else if (type == typeof(ushort))
                existsAction = ExistsUnsignedShort;
            #endregion

            //Object
            else
                existsAction = ExistsObject<T>;

            return existsAction.Invoke(key);
        }

        #region IS KEY EXISTING
        #region COMMON TYPES
        private static bool ExistsInt(string key)
        {
            return PlayerPrefs.HasKey(key + "int");
        }
        private static bool ExistsFloat(string key)
        {
            return PlayerPrefs.HasKey(key + "float");
        }
        private static bool ExistsString(string key)
        {
            return PlayerPrefs.HasKey(key + "string");
        }

        private static bool ExistsBool(string key)
        {
            return PlayerPrefs.HasKey(key + "bool");
        }
        private static bool ExistsDouble(string key)
        {
            return ExistsLong(key + "double");
        }
        private static bool ExistsLong(string key)
        {
            for (byte i = 0; i < 8; i++)
            {
                if (!ExistsByte(key + "long" + i))
                    return false;
            }
            return true;
        }
        private static bool ExistsByte(string key)
        {
            return PlayerPrefs.HasKey(key + "byte");
        }
        private static bool ExistsShort(string key)
        {
            return PlayerPrefs.HasKey(key + "short");
        }
        private static bool ExistsChar(string key)
        {
            return PlayerPrefs.HasKey(key + "char");
        }
        private static bool ExistsDateTime(string key)
        {
            return PlayerPrefs.HasKey(key + "DateTime");
        }
        #endregion

        #region UNSIGNED TYPES
        private static bool ExistsUnsignedInt(string key)
        {
            return PlayerPrefs.HasKey(key + "uint");
        }
        private static bool ExistsUnsignedShort(string key)
        {
            return PlayerPrefs.HasKey(key + "ushort");
        }
        private static bool ExistsUnsignedLong(string key)
        {
            return ExistsLong(key + "ulong");
        }
        #endregion

        #region OBJECT
        private static bool ExistsObject<T>(string key)
        {
            return PlayerPrefs.HasKey(key + typeof(T).Name);
        }
        #endregion
        #endregion
        #endregion

        #region DELETE
        public static void Delete<T>(string key)
        {
            DeleteAction deleteAction;
            Type type = typeof(T);

            #region COMMON TYPES
            if (type == typeof(int))
                deleteAction = DeleteInt;
            else if (type == typeof(float))
                deleteAction = DeleteFloat;
            else if (type == typeof(string))
                deleteAction = DeleteString;

            else if (type == typeof(bool))
                deleteAction = DeleteBool;
            else if (type == typeof(double))
                deleteAction = DeleteDouble;
            else if (type == typeof(long))
                deleteAction = DeleteLong;
            else if (type == typeof(byte))
                deleteAction = DeleteByte;
            else if (type == typeof(short))
                deleteAction = DeleteShort;
            else if (type == typeof(char))
                deleteAction = DeleteChar;
            else if (type == typeof(DateTime))
                deleteAction = DeleteDateTime;
            #endregion

            #region UNSIGNED TYPES
            else if (type == typeof(uint))
                deleteAction = DeleteUnsignedInt;
            else if (type == typeof(ulong))
                deleteAction = DeleteUnsignedLong;
            else if (type == typeof(ushort))
                deleteAction = DeleteUnsignedShort;
            #endregion

            //Object
            else
                deleteAction = DeleteObject<T>;

            deleteAction.Invoke(key);
        }

        #region DELETING KEY
        #region COMMON TYPES
        private static void DeleteInt(string key)
        {
            PlayerPrefs.DeleteKey(key + "int");
        }
        private static void DeleteFloat(string key)
        {
            PlayerPrefs.DeleteKey(key + "float");
        }
        private static void DeleteString(string key)
        {
            PlayerPrefs.DeleteKey(key + "string");
        }

        private static void DeleteBool(string key)
        {
            PlayerPrefs.DeleteKey(key + "bool");
        }
        private static void DeleteDouble(string key)
        {
            DeleteLong(key + "double");
        }
        private static void DeleteLong(string key)
        {
            for (byte i = 0; i < 8; i++)
            {
                DeleteByte(key + "long" + i);
            }
        }
        private static void DeleteByte(string key)
        {
            PlayerPrefs.DeleteKey(key + "byte");
        }
        private static void DeleteShort(string key)
        {
            PlayerPrefs.DeleteKey(key + "short");
        }
        private static void DeleteChar(string key)
        {
            PlayerPrefs.DeleteKey(key + "char");
        }
        private static void DeleteDateTime(string key)
        {
            PlayerPrefs.DeleteKey(key + "DateTime");
        }
        #endregion

        #region UNSIGNED TYPES
        private static void DeleteUnsignedInt(string key)
        {
            PlayerPrefs.DeleteKey(key + "uint");
        }
        private static void DeleteUnsignedShort(string key)
        {
            PlayerPrefs.DeleteKey(key + "ushort");
        }
        private static void DeleteUnsignedLong(string key)
        {
            DeleteLong(key + "ulong");
        }
        #endregion

        #region OBJECT
        private static void DeleteObject<T>(string key)
        {
            PlayerPrefs.DeleteKey(key + typeof(T).Name);
        }
        #endregion
        #endregion
        #endregion
    }
}

