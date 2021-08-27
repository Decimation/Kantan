using System.Collections;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Global

namespace Kantan.Collections
{
	public class FixedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private readonly Dictionary<TKey, TValue> m_values;

		private readonly ISet<TKey> m_whitelist;

		public FixedDictionary(ISet<TKey> keys)
		{
			m_whitelist = keys;
			m_values    = new Dictionary<TKey, TValue>();

			foreach (TKey key in keys) {
				Add(key, default);
			}
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <inheritdoc />
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => m_values.GetEnumerator();

		/// <inheritdoc />
		public void Add(KeyValuePair<TKey, TValue> item) 
			=> ((IDictionary<TKey, TValue>) m_values).Add(item);

		/// <inheritdoc />
		public void Clear() => m_values.Clear();

		/// <inheritdoc />
		public bool Contains(KeyValuePair<TKey, TValue> item) 
			=> ((IDictionary<TKey, TValue>) m_values).Contains(item);

		/// <inheritdoc />
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
			((IDictionary<TKey, TValue>) m_values).CopyTo(array, arrayIndex);

		/// <inheritdoc />
		public bool Remove(KeyValuePair<TKey, TValue> item)
			=> ((IDictionary<TKey, TValue>) m_values).Remove(item);

		/// <inheritdoc />
		public int Count => m_values.Count;

		/// <inheritdoc />
		public bool IsReadOnly => false;

		/// <inheritdoc />
		public void Add(TKey key, TValue value)
		{
			if (m_whitelist.Contains(key)) {
				m_values.Add(key, value);
			}
		}

		/// <inheritdoc />
		public bool ContainsKey(TKey key) => m_values.ContainsKey(key);

		/// <inheritdoc />
		public bool Remove(TKey key) => m_values.Remove(key);

		/// <inheritdoc />
		public bool TryGetValue(TKey key, out TValue value) => m_values.TryGetValue(key, out value);

		/// <inheritdoc />
		public TValue this[TKey key]
		{
			get => m_values[key];
			set => m_values[key] = value;
		}

		/// <inheritdoc />
		public ICollection<TKey> Keys => m_values.Keys;

		/// <inheritdoc />
		public ICollection<TValue> Values => m_values.Values;
	}
}