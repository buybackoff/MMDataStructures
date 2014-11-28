using NUnit.Framework;

namespace MMDataStructures.Test
{
    /// <summary>
    /// Summary description for ByteArrayTest
    /// </summary>
    [TestFixture]
    public class ByteArrayTest
    {
        [Test]
        public void When_passing_in_two_equal_arrays_verify_they_are_the_same_unsafe()
        {
            byte[] arr1 = new byte[] {1, 2, 3, 4, 5};
            byte[] arr2 = new byte[] {1, 2, 3, 4, 5};
            Assert.IsTrue(ByteArrayCompare.UnSafeEquals(arr1, arr2));
        }

        [Test]
        public void When_passing_in_two_equal_arrays_verify_they_are_the_same_safe()
        {
            byte[] arr1 = new byte[] { 1, 2, 3, 4, 5 };
            byte[] arr2 = new byte[] { 1, 2, 3, 4, 5 };
            Assert.IsTrue(ByteArrayCompare.Equals(arr1, arr2));
        }

        [Test]
        public void When_passing_in_two_different_arrays_verify_they_are_differen_unsafe()
        {
            byte[] arr1 = new byte[] { 1, 2, 3, 4, 5 };
            byte[] arr2 = new byte[] { 1, 2, 3, 4, 6 };
            Assert.IsFalse(ByteArrayCompare.UnSafeEquals(arr1, arr2));
        }

        [Test]
        public void When_passing_in_two_different_arrays_verify_they_are_different_safe()
        {
            byte[] arr1 = new byte[] { 1, 2, 3, 4, 5 };
            byte[] arr2 = new byte[] { 1, 2, 3, 4, 6 };
            Assert.IsFalse(ByteArrayCompare.Equals(arr1, arr2));
        }
    }
}


