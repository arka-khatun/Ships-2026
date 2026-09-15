using GA.Collections;
using Xunit;

public class LinkedListTests
{
	[Fact]
	public void TestBasicAdd()
	{
		GA.Collections.LinkedList<int> list = new GA.Collections.LinkedList<int>();
		list.Add(1);
		list.Add(6);
		list.Add(-1);

		Assert.Equal(3, list.Count);
	}
	
	[Fact]
	public void TestNewListIsEmpty()
	{
		GA.Collections.LinkedList<int> list = new GA.Collections.LinkedList<int>();
 
		Assert.Equal(0, list.Count);
		Assert.Empty(list);
	}
	
	
	[Fact]
	public void TestReturnsCorrectSavedValue()
	{
		GA.Collections.LinkedList<int> list = new GA.Collections.LinkedList<int>();
		list.Add(77);
		list.Add(66);
		list.Add(55);

		Assert.Equal(55, list.ElementAt(2));
	}
	
	[Fact]
	public void TestRemoveReturnsTrueAndDecreasesCount()
	{
		GA.Collections.LinkedList<int> list = new GA.Collections.LinkedList<int>();
		list.Add(6);
		list.Add(7);
		list.Add(8);
 
		bool removed = list.Remove(6);
 
		Assert.True(removed);
		Assert.Equal(2, list.Count);
	}
	
	[Fact]
	public void TestRemoveNonExistingReturnsFalseAndDoesNotDecreasesCount()
	{
		GA.Collections.LinkedList<int> list = new GA.Collections.LinkedList<int>();
		list.Add(6);
		list.Add(7);
 
		bool removed = list.Remove(666);
 
		Assert.False(removed);
		Assert.Equal(2, list.Count);
	}
	
	[Fact]
	public void TestClearRemovesAll()
	{
		GA.Collections.LinkedList<int> list = new GA.Collections.LinkedList<int>();
		list.Add(6);
		list.Add(7);
 
		list.Clear();
 
		Assert.Equal(0, list.Count);
		Assert.Empty(list);
		Assert.False(list.Contains(1));
	}
}