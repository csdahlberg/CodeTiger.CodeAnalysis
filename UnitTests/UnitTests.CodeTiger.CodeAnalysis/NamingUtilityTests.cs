using CodeTiger.CodeAnalysis;
using Xunit;

namespace UnitTests.CodeTiger.CodeAnalysis
{
    public class NamingUtilityTests
    {
        public class IsProbablyPascalCased_String_Boolean
        {
            [Fact]
            public void ReturnsNullWhenNameIsNull()
            {
                Assert.Null(NamingUtility.IsProbablyPascalCased(null));
            }

            [Fact]
            public void ReturnsNullWhenNameIsEmpty()
            {
                Assert.Null(NamingUtility.IsProbablyPascalCased(""));
            }

            [Fact]
            public void ReturnsNullWhenNameIsASingleUppercaseLetter()
            {
                Assert.Null(NamingUtility.IsProbablyPascalCased("A"));
            }

            [Theory]
            [InlineData("Thing")]
            [InlineData("ThingTests")]
            [InlineData("Thing1")]
            [InlineData("Thing1Tests")]
            public void ReturnsTrueForPascalTypeNames(string name)
            {
                Assert.True(NamingUtility.IsProbablyPascalCased(name, true));
            }

            [Theory]
            [InlineData("t")]
            [InlineData("thing")]
            [InlineData("aThing")]
            [InlineData("1Thing")]
            [InlineData(" Thing")]
            [InlineData("`Thing")]
            public void ReturnsFalseWhenFirstCharacterIsNotAnUppercaseLetter(string name)
            {
                Assert.False(NamingUtility.IsProbablyPascalCased(name));
            }

            [Theory]
            [InlineData("Thing`1")]
            [InlineData("Thing_1")]
            [InlineData("Thing`1Tests")]
            [InlineData("Thing_1Tests")]
            public void ReturnsFalseForGenericTypeNameWhenGenericTypeArityIsNotAllowed(string name)
            {
                Assert.False(NamingUtility.IsProbablyPascalCased(name, false));
            }

            [Theory]
            [InlineData("Thing`A")]
            [InlineData("Thing`a1")]
            [InlineData("Thing_a")]
            [InlineData("Thing_A1")]
            public void ReturnsFalseForGenericTypeNameWhenNumberDoesNotFollowSeparator(string name)
            {
                Assert.False(NamingUtility.IsProbablyPascalCased(name, true));
            }

            [Theory]
            [InlineData("Thing`1")]
            [InlineData("Thing_1")]
            [InlineData("Thing`1Tests")]
            [InlineData("Thing_1Tests")]
            public void ReturnsTrueForGenericTypeNameWhenGenericTypeArityIsAllowed(string name)
            {
                Assert.True(NamingUtility.IsProbablyPascalCased(name, true));
            }
        }
    }
}
