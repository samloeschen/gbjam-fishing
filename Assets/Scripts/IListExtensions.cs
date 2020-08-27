using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IListExtensions {

	public static void StringLikeFunction(this List<char> list) {

	}

	public static void Shuffle<T>(this IList<T> list){
		int n = list.Count;
		while (n > 1) {
			int i = (int)(UnityEngine.Random.value * n);
			n--;
			var t = list[n];
			list[n] = list[i];
			list[i] = t;
		}
	}
	public static void Swap<T>(this IList<T> list, int indexA, int indexB) {
		T tmp = list[indexA];
		list[indexA] = list[indexB];
		list[indexB] = tmp;
	}
    public static void ShuffleRange<T>(this IList<T> ts, int from, int to){
        int count = to > ts.Count ? ts.Count : to;
        int first = from > to ? to : from;
        int last = count;
        for(var i = first; i < last; ++i){
            var r = UnityEngine.Random.Range(i, count);
			count--;
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
	public static T GetRandomElement<T>(this List<T> list) {
        return list[Mathf.FloorToInt(UnityEngine.Random.value * list.Count)];
    }

	public static T GetRandomWithSwapback<T>(this List<T> list) {
		var idx = Mathf.FloorToInt(UnityEngine.Random.value * (list.Count - 1));
		var result = list[idx];
		list[idx] = list[list.Count - 1];
		list[list.Count - 1] = result;
		return result;
	}
}
