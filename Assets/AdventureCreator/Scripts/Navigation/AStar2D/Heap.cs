using System;

namespace AC.AStar2D
{

	public class Heap<T> where T : IHeapItem<T>
	{

		#region Variables

		private readonly T[] items;
		private int currentItemCount;

		#endregion


		#region Constructors

		public Heap (int maxSize)
		{
			items = new T[maxSize];
		}

		#endregion


		#region PublicFunctions

		public void Add (T item)
		{
			item.HeapIndex = currentItemCount;
			items[currentItemCount] = item;
			SortUp (item);
			currentItemCount ++;
		}


		public T RemoveFirst ()
		{
			T firstItem = items[0];
			currentItemCount --;

			items[0] = items[currentItemCount];
			items[0].HeapIndex = 0;
			SortDown (items[0]);
			return firstItem;
		}


		public void UpdateItem (T item)
		{
			SortUp (item);
		}


		public bool Contains (T item)
		{
			return Equals (items[item.HeapIndex], item);
		}

		#endregion


		#region PrivateFunctions

		private void SortUp (T item)
		{
			int parentIndex = (item.HeapIndex - 1) /2;

			while (true)
			{
				T parentItem = items[parentIndex];
				if (item.CompareTo (parentItem) > 0)
				{
					Swap (item, parentItem);
				}
				else
				{
					break;
				}

				parentIndex = (item.HeapIndex - 1) / 2;
			}
		}


		private void SortDown (T item)
		{
			while (true)
			{
				int childIndexLeft = (item.HeapIndex * 2) + 1;
				int childIndexRight = childIndexLeft + 1;
				int swapIndex = 0;

				if (childIndexLeft < currentItemCount)
				{
					swapIndex = childIndexLeft;

					if (childIndexRight < currentItemCount)
					{
						if (items[childIndexLeft].CompareTo (items[childIndexRight]) < 0)
						{
							swapIndex = childIndexRight;
						}
					}

					if (item.CompareTo (items[swapIndex]) < 0)
					{
						Swap (item, items[swapIndex]);
					}
					else
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
		}


		private void Swap (T itemA, T itemB)
		{
			items[itemA.HeapIndex] = itemB;
			items[itemB.HeapIndex] = itemA;

			int itemAIndex = itemA.HeapIndex;
			itemA.HeapIndex = itemB.HeapIndex;
			itemB.HeapIndex = itemAIndex;
		}

		#endregion


		#region GetSet

		public int Count { get { return currentItemCount; } }

		#endregion

	}


	public interface IHeapItem<T> : IComparable<T>
	{

		int HeapIndex { get; set; }

	}

}