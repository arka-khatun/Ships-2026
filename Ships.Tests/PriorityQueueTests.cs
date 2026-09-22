using System;
using System.Collections.Generic;
using System.Linq;
using GA.Collections;
using Xunit;

public class PriorityQueueTests
{
    // BASIC TESTS

    [Fact]
    public void NewQueue_IsEmpty()
    {
        var queue = new PriorityQueue<int>();

        Assert.Equal(0, queue.Count);
    }
    // Working as intended

    [Fact]
    public void Enqueue_IncreasesCount()
    {
        var queue = new PriorityQueue<int>();
        queue.Enqueue(5);
        queue.Enqueue(3);

        Assert.Equal(2, queue.Count);
    }
    // Working as intended

    [Fact]
    public void Peek_ReturnsSmallestItem_WithoutRemovingIt()
    {
        var queue = new PriorityQueue<int>();
        queue.Enqueue(5);
        queue.Enqueue(1);
        queue.Enqueue(3);

        Assert.Equal(1, queue.Peek());
        Assert.Equal(3, queue.Count);
    }
    // Working as intended

    [Fact]
    public void Dequeue_ReturnsAndRemovesSmallestItem()
    {
        var queue = new PriorityQueue<int>();
        queue.Enqueue(5);
        queue.Enqueue(1);
        queue.Enqueue(3);

        int result = queue.Dequeue();

        Assert.Equal(1, result);
        Assert.Equal(2, queue.Count);
    }
    // Working as intended

    [Fact]
    public void Peek_OnEmptyQueue_ThrowsInvalidOperationException()
    {
        var queue = new PriorityQueue<int>();

        Assert.Throws<InvalidOperationException>(() => queue.Peek());
    }
    // Working as intended

    [Fact]
    public void Dequeue_OnEmptyQueue_ThrowsInvalidOperationException()
    {
        var queue = new PriorityQueue<int>();

        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
    }
    // Working as intended

    [Fact]
    public void Contains_ReturnsTrue_ForEnqueuedItem()
    {
        var queue = new PriorityQueue<int>();
        queue.Enqueue(7);
        queue.Enqueue(2);

        Assert.True(queue.Contains(7));
    }
    // Working as intended

    [Fact]
    public void Contains_ReturnsFalse_ForMissingItem()
    {
        var queue = new PriorityQueue<int>();
        queue.Enqueue(7);

        Assert.False(queue.Contains(99));
    }
    // Working as intended

    [Fact]
    public void Clear_EmptiesQueueAndAllowsReuse()
    {
        var queue = new PriorityQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);

        queue.Clear();

        Assert.Equal(0, queue.Count);
        Assert.Throws<InvalidOperationException>(() => queue.Peek());
        
        queue.Enqueue(4);
        queue.Enqueue(1);
        Assert.Equal(1, queue.Peek());
    }
    // Working as intended

    
    // CORRECT ORDERING TESTS
    // Testing if the priority queue will correct the order when needed, with
    // cases where the order is ascending (already correct), where the order is descending
    // (whole list needs to be corrected), random list, and list with duplicates, only one node
    // and empty input.

    [Theory]
    [InlineData(new int[] { 1, 2, 3, 4, 5 })]
    [InlineData(new int[] { 5, 4, 3, 2, 1 })]
    [InlineData(new int[] { 5, 3, 8, 1, 9, 2 })]
    [InlineData(new int[] { 3, 3, 3, 1, 1, 2 })]
    [InlineData(new int[] { -5, 0, 5, -100, 100 })]
    [InlineData(new int[] { 42 })]
    [InlineData(new int[] { })]
    public void Dequeue_AlwaysReturnsItemsInAscendingOrder(int[] values)
    {
        var queue = new PriorityQueue<int>();
        foreach (var v in values)
        {
            queue.Enqueue(v);
        }

        int[] expected = values.OrderBy(v => v).ToArray();
        var actual = new List<int>();
        while (queue.Count > 0)
        {
            actual.Add(queue.Dequeue());
        }

        Assert.Equal(expected, actual);
    }
    // Working as intended

    
    // HEAP CONSISTENCE TESTS

    [Fact]
    public void IsConsistent_IsTrueForEmptyQueue()
    {
        var queue = new PriorityQueue<int>();

        Assert.True(queue.IsConsistent());
    }
    // Working as intended

    [Fact]
    public void IsConsistent_HoldsAfterEveryEnqueueAndDequeue()
    {
        var queue = new PriorityQueue<int>();
        int[] values = { 9, 1, 7, 3, 5, 2, 8, 4, 6, 0 };

        foreach (var v in values)
        {
            queue.Enqueue(v);
            Assert.True(queue.IsConsistent());
        }

        while (queue.Count > 0)
        {
            queue.Dequeue();
            Assert.True(queue.IsConsistent());
        }
    }
    // Working as intended

    
    // FUZZZ TEST
    // Trying to break the system with random operation sequences.
    // The test runs a long randomly generated Enqueue/Dequeue sequence
    // and compares it to a regular List<int> with Min().
    // Bug could be found as a failed IsConsistent() test or as a wrong Dequeue value.
    [Fact]
    public void Fuzz_RandomEnqueueDequeueSequence_MatchesReferenceOrderingAndStaysConsistent()
    {
        var random = new Random(12345);
        var queue = new PriorityQueue<int>();
        var reference = new List<int>();

        for (int i = 0; i < 500; i++)
        {
            if (reference.Count > 0 && random.NextDouble() < 0.45)
            {
                int expected = reference.Min();
                reference.Remove(expected);

                int actual = queue.Dequeue();

                Assert.Equal(expected, actual);
            }
            else
            {
                int value = random.Next(-100, 100);
                reference.Add(value);
                queue.Enqueue(value);
            }

            Assert.True(queue.IsConsistent());
        }

        // Empties the rest and compares to the remaining list order.
        List<int> expectedRemainder = reference.OrderBy(v => v).ToList();
        var actualRemainder = new List<int>();
        while (queue.Count > 0)
        {
            actualRemainder.Add(queue.Dequeue());
        }

        Assert.Equal(expectedRemainder, actualRemainder);
    }
    // Working as intended
    

    // FINDINGS - Does not work!

    // FINDING 1: PriorityQueue<T> does not assert for a null value.
    // When T is a reference type (e.g. string), null.CompareTo() throws a NullReferenceException
    // when a null node is compared to a some other node.
    // I.e. when the target of Enqueue call is not the first or only node of the list.
    [Fact]
    public void Enqueue_NullItem_ThrowsNullReferenceException_KnownLimitation()
    {
        var queue = new PriorityQueue<string>();
        queue.Enqueue("b");

        Assert.Throws<NullReferenceException>(() => queue.Enqueue(null));
    }

    // FINDING 2: As a result of the Finding 1, the list is left in a corrupted state.
    // _data.Add() runs as the first line of Enqueue, and the error remains
    // as it appears later in the code. With lack of any rollback feature, the Count
    // increases permanently, while the heap order is not restored.
    [Fact]
    public void Enqueue_NullItem_LeavesQueueInCorruptedState_KnownLimitation()
    {
        var queue = new PriorityQueue<string>();
        queue.Enqueue("b");

        Assert.Throws<NullReferenceException>(() => queue.Enqueue(null));

        Assert.Equal(2, queue.Count);
        Assert.False(queue.IsConsistent());
    }
}