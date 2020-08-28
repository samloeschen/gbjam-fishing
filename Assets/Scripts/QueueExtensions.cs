using System.Collections.Generic;
public static class QueueExtensions {
    public static bool TryDeque<T> (this Queue<T> queue, out T value) {
        value = default(T);
		if (queue.Count > 0) {
			value = queue.Dequeue();
			return true;
		}
		return false;
	}
}
