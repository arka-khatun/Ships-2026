using System.Collections;
using System.Collections.Generic;

namespace GA.Collections
{
	public class LinkedList<T> : ICollection<T>
	{
		protected class Node
		{
			public T Value { get; set; }
			public Node Next { get; set; }
			public Node Previous { get; set; }

			public Node() : this(default(T))
			{
			}

			public Node(T value, Node previous = null, Node next = null)
			{
				Value = value;
				Next = next;
				Previous = previous;
			}
		}

		/// <summary>
		/// The head of the linked list. When the list is empty, this will be null.
		/// </summary>
		protected Node Head { get; set; } = null;
		
		/// <summary>
		/// The tail of the linked list. When the list is empty, this will be null.
		/// Reference to the tail allows Add() add new items straight to the end of the list.
		/// </summary>
		protected Node Tail { get; set; } = null;

		public int Count { get; private set; } = 0;

		public virtual bool IsReadOnly => false;

		public void Add(T item)
		{
			if (IsReadOnly)
			{
				throw new System.NotSupportedException("The collection is read-only.");
			}

			Node node = new Node(item, previous: Tail);

			if (Head == null)
			{
				Head = node;
			}
			else
			{
				Tail.Next = node;
			}

			Tail = node;
			Count++;
		}

		public void Clear()
		{
			if (IsReadOnly)
			{
				throw new System.NotSupportedException("The collection is read-only.");
			}

			Head = null;
			Tail = null;
			Count = 0;
		}

		public bool Contains(T item)
		{
			Node current = Head;
			while (current != null)
			{
				if (EqualityComparer<T>.Default.Equals(current.Value, item))
				{
					return true;
				}

				current = current.Next;
			}

			return false;
		}

		public virtual void CopyTo(T[] array, int arrayIndex)
		{
			throw new System.NotImplementedException("Not nesessary for this example :D");
		}

		public IEnumerator<T> GetEnumerator()
		{
			Node current = Head;
			while (current != null)
			{
				yield return current.Value;
				current = current.Next;
			}
		}

		public bool Remove(T item)
		{
			if (IsReadOnly)
			{
				throw new System.NotSupportedException("This collection is read-only");
			}

			Node current = Head;

			while (current != null)
			{
				if (EqualityComparer<T>.Default.Equals(current.Value, item))
				{
					RemoveNode(current);
					return true;
				}

				current = current.Next;
			}

			return false;
		}

		/// <summary>
		/// Remove a node and unlink it from the list. Fix both previous and next.
		/// </summary>
		private void RemoveNode(Node node)
		{
			if (node.Previous != null)
				{
					node.Previous.Next = node.Next;
				}
			else
			{
				Head = node.Next;
			}
			
			if (node.Next != null)
				{
					node.Next.Previous = node.Previous;
				}
			else
				{
				Tail = node.Previous;
				}
			
			Count--;
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

	}
}