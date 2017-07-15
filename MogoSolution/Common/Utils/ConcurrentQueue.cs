using System;
using System.Collections.Generic;

namespace Mogo.Util {

sealed public class ConcurrentQueue<T> {

	private readonly Queue<T> _datas = new Queue<T>();
	private readonly Object _lockObj = new Object();

	public void Enqueue(T obj) {
		lock (_lockObj) {
			_datas.Enqueue(obj);
		}
	}

	public T Dequeue() {
		lock (_lockObj) {
			var obj = _datas.Dequeue();
			return obj;
		}
	}

	public bool TryDequeue(out T obj) {
		lock (_lockObj) {
			if (_datas.Count > 0) {
				obj = _datas.Dequeue();
				return true;
			}
			else {
				obj = default(T);
				return false;
			}
		}
	}

	public T TryDequeue() {
		T obj;
		return TryDequeue(out obj) ? obj : default(T);
	}

	public bool IsEmpty() {
		lock (_lockObj) {
			var value = _datas.Count == 0;
			return value;
		}
	}

	public void Clear() {
		lock (_lockObj) {
			_datas.Clear();
		}
	}

}

}