using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.Model
{
	public class FixedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		public FixedDictionary() { }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public void Clear()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public int Count => throw new NotImplementedException();

		/// <inheritdoc />
		public bool IsReadOnly => throw new NotImplementedException();

		/// <inheritdoc />
		public void Add(TKey key, TValue value)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public bool ContainsKey(TKey key)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public bool Remove(TKey key)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public bool TryGetValue(TKey key, out TValue value)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public TValue this[TKey key]
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <inheritdoc />
		public ICollection<TKey> Keys => throw new NotImplementedException();

		/// <inheritdoc />
		public ICollection<TValue> Values => throw new NotImplementedException();
	}
}