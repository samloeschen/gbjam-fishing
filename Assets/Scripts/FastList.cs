using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

/// <summary>
/// A faster replacement for List&lt;T&gt;.
/// </summary>
/// <typeparam name="T">The type of item being held in the array.</typeparam>

[System.Serializable]
public class FastList<T> {
	protected T[] array;
	public int count;

	public int capacity {
		get { return array.Length; }
	}

	/// <summary>
	/// Create a new FastList with an initial capacity.
	/// </summary>
	/// <param name="capacity"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FastList(int capacity = 10) {
		array = new T[capacity];
		count = 0;
	}

	/// <summary>
	/// Converts an array into a FastList.  This does not
	/// make a new copy of the array - the new FastList
	/// will use the existing array internally.
	/// </summary>
	/// /// <param name="fromArray"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FastList(T[] fromArray) {
		array = fromArray;
		count = array.Length;
	}

	/// <summary>
	/// Adds an item to the list. May need to allocate a larger
	/// array if there is not enough space.
	/// </summary>
	/// <param name="item"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(T item, bool allowExpand = true) {
		if (count==array.Length && allowExpand) {
			DoubleCapacity();
		}
		array[count] = item;
		count++;
	}

	// TODO get rid of if checks, precalculate length
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddRange (T[] arr, bool allowExpand = true) {
		for (int i = 0; i < arr.Length; i++) {
			if (count == array.Length && !allowExpand) {
				return;
			}
			array[count] = arr[i];
			count++;
		}
	}

	// TODO get rid of if checks, precalculate length
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddRange(List<T> list, bool allowExpand = true) {
		for (int i = 0; i < list.Count; i++) {
			if (count == array.Length && !allowExpand) {
				return;
			}
			array[count] = list[i];
			count++;
		}
	}

	/// <summary>
	/// Removes an item from the list and returns it, shifting each later
	/// item back by one space.  This is slower for longer lists -
	/// for faster deletion, consider using RemoveFast().
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Remove(int index) {
		T output = array[index];
		for (int i=index+1; i<array.Length;i++) {
			array[i-1] = array[i];
		}
		count -= 1;

		return output;
	}


	public void Insert (T element, int index) {
		if (count == array.Length) DoubleCapacity();
		count++;
		for (int i = count; i > index; i--) {
			array[i] = array[i - 1];
		}
		array[index] = element;
	}

	public void InsertFast (T element, int index) {
		if (count == array.Length) DoubleCapacity();
		array[count] = array[index];
		array[index] = element;
		count++;
	}

	/// <summary>
	/// Removes an item from the list and returns it,
	/// swapping the previous final-item into its slot.
	/// Faster than Remove(), but does not preserve your array's order.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T RemoveFast(int index) {
		T output = array[index];
		array[index] = array[count-1];
		count--;

		return output;
	}

	/// <summary>
	/// Removes the last item from the array and returns it.
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Pop() {
		count--;
		return array[count];
	}

	/// <summary>
	/// Clears the list.  Does not delete contents!
	/// References in "unused" slots will remain.
	/// To also set all references to null, use HardClearLength()
	/// or HardClear().
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear() {
		count = 0;
	}

	/// <summary>
	/// Clears the list and removes the references that
	/// it contained (up to the previous "length" value).
	/// To clear all references in the list, including those
	/// outside of the active range, use HardClear().
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void HardClearLength() {
		for (int i=0; i<count; i++) {
			array[i] = default;
		}
		count = 0;
	}

	/// <summary>
	/// Clears the list and removes all references that
	/// the array contained, even if they were outside
	/// of the active range.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void HardClear() {
		for (int i=0; i<array.Length; i++) {
			array[i] = default;
		}
		count = 0;
	}

	public T this[int i] {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get {
			return array[i];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set {
			array[i] = value;
		}
	}

	/// <summary>
	/// Get a reference to the FastList's internal array.
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T[] GetArray() {
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void DoubleCapacity() {
		int newLength = count*2;
		if (array.Length<4) {
			newLength=8;
		}
		T[] newArray = new T[newLength];
		array.CopyTo(newArray,0);
		array = newArray;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Shuffle () {
		int j;
		T tmp;
		for(int i = count - 1; i > 0; i--){
			j = Mathf.FloorToInt(UnityEngine.Random.value * (i + 1));
            tmp = array[i];
            array[i] = array[j];
            array[j] = tmp;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T First () {
		return this[0];
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Last () {
		return this[count - 1];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T GetRandomWithSwapback () {
		var idx = UnityEngine.Random.Range(0, count - 1);
		var result = this[idx];
		this[idx] = this[count - 1];
		this[count - 1] = result;
		return result;
	}
}
