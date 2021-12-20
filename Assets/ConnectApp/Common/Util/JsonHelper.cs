using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ConnectApp.Common.Util {
    public static class JsonHelper {
        public static T[] FromJson<T>(string json) {
            var wrapper = JsonUtility.FromJson<Wrapper<T>>(json: json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array) {
            var wrapper = new Wrapper<T> {
                Items = array
            };
            return JsonUtility.ToJson(obj: wrapper);
        }

        public static string ToJson<T>(List<T> list) {
            var wrapper = new Wrapper<T> {
                Items = list.ToArray()
            };
            return JsonUtility.ToJson(obj: wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint) {
            var wrapper = new Wrapper<T> {
                Items = array
            };
            return JsonUtility.ToJson(obj: wrapper, prettyPrint: prettyPrint);
        }

        public static Dictionary<string, object> ToDictionary(object json) {
            if (json is Dictionary<string, object> dictionary) {
                return dictionary;
            }

            if (json is string jsonString) {
                var jsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(value: jsonString);
                return jsonDictionary;
            }

            return new Dictionary<string, object>();
        }

        [Serializable]
        class Wrapper<T> {
            public T[] Items;
        }
    }
}