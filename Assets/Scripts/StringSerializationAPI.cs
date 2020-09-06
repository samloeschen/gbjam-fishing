using System;
using FullSerializer;
using UnityEngine;
public static class StringSerializationAPI {
    private static readonly fsSerializer _serializer = new fsSerializer();

    public static bool TrySerialize<T>(T value, out string result) {
        fsData data;
        try {
            _serializer.TrySerialize(typeof(T), value, out data);
            result = fsJsonPrinter.CompressedJson(data);
            return true;
        } catch {
            Debug.Log("Serialization failed!");
            result = "";
            return false;
        }
    }

    public static bool TryDeserialize<T>(string serializedState, out T result) {
        fsData data;
        object obj = null;
        result = default(T);
        try {
            Debug.Log(serializedState);
            data = fsJsonParser.Parse(serializedState);
            _serializer.TryDeserialize(data, typeof(T), ref obj);
            result = (T)obj;
            return true;
        } catch {
            Debug.Log("Deserialization failed!");
            return false;
        }
    }
}