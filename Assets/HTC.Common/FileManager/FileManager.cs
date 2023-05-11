using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace com.HTC.Common
{
    public static class FileManager 
    {
        // Application.persistentDataPath + "/Game.weeklyhow";
        public static void SaveData<T>(T saveObject, string path)
        {
            IFormatter formatter = new BinaryFormatter();

            var ss = new SurrogateSelector();
            addSurrogates(ss);
            formatter.SurrogateSelector = ss;

            if(!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            FileStream stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, saveObject);
            stream.Close();

            Debug.Log("Save file to: " + path);
        }

        public static T LoadData<T>(string path) where T: class
        {
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                var ss = new SurrogateSelector();

                addSurrogates(ss);

                formatter.SurrogateSelector = ss;

                FileStream stream = new FileStream(path, FileMode.Open);
                T data = formatter.Deserialize(stream) as T;
                stream.Close();
                return data;
            }
            else
            {
                Debug.LogError("Error: Saved file not found in " + path);
                return null;
            }
        }

        private static void addSurrogates(SurrogateSelector selector)
        {
            selector.AddSurrogate(typeof(Vector3Int),
               new StreamingContext(StreamingContextStates.All),
               new Vector3IntSurrogate());

            selector.AddSurrogate(typeof(Vector2Int),
            new StreamingContext(StreamingContextStates.All),
            new Vector2IntSurrogate());

            selector.AddSurrogate(typeof(Vector3),
            new StreamingContext(StreamingContextStates.All),
            new Vector3Surrogate());

            selector.AddSurrogate(typeof(Quaternion),
            new StreamingContext(StreamingContextStates.All),
            new QuternionSurrogate());
        }
    }

}
