using System;
using NUnit.Framework;

namespace MMDataStructures.Test
{
    /// <summary>
    /// Contains assertion types that are not provided with the standard MSTest assertions.
    /// </summary>
    public static class MyAssert
    {
        /// <summary>
        /// Checks to make sure that the input delegate throws a exception of type exceptionType.
        /// </summary>
        /// <typeparam name="exceptionType">The type of exception expected.</typeparam>
        /// <param name="blockToExecute">The block of code to execute to generate the exception.</param>
        public static void ThrowsException<exceptionType>(Action blockToExecute) where exceptionType : System.Exception
        {
            try
            {
                blockToExecute();
            }
            catch (exceptionType)
            {
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected exception of type " + typeof (exceptionType) + " but type of " + ex.GetType() +
                            " was thrown instead.");
            }

            Assert.Fail("Expected exception of type " + typeof (exceptionType) + " but no exception was thrown.");
        }
    }
}