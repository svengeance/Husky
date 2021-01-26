using NUnit.Framework;

namespace Husky.Internal.Shared.Tests.PartialVersionTests
{
    public class PartialVersionTests
    {
        [TestCase("20.04", 20, 4, null)]
        [TestCase("8", 8, null, null)]
        [TestCase("0.10.2", 0, 10, 2)]
        [TestCase("", null, null, null)]
        [Category("UnitTest")]
        public void Partial_version_substitutes_null_values_for_missing_inputs_from_string_constructor(string input, int? expectedMajor, int? expectedMinor, int? expectedPatch)
        {
            // Arrange
            // Act
            var partial = new PartialVersion(input);

            // Assert
            Assert.AreEqual(expectedMajor, partial.Major);
            Assert.AreEqual(expectedMinor, partial.Minor);
            Assert.AreEqual(expectedPatch, partial.Patch);
        }

        [TestCase(20, 04, null, 20, 4, null)]
        [TestCase(8, null, null, 8, null, null)]
        [TestCase(0, 10, 2, 0, 10, 2)]
        [TestCase(null, null, null, null, null, null)]
        [Category("UnitTest")]
        public void Partial_version_substitutes_null_values_for_missing_inputs_from_version_constructor(int? major, int? minor, int? patch, int? expectedMajor, int? expectedMinor, int? expectedPatch)
        {
            // Arrange
            // Act
            var partial = new PartialVersion(major, minor, patch);

            // Assert
            Assert.AreEqual(expectedMajor, partial.Major);
            Assert.AreEqual(expectedMinor, partial.Minor);
            Assert.AreEqual(expectedPatch, partial.Patch);
        }

        [TestCase("20.04", 20, 4, 0)]
        [TestCase("8", 8, 0, 0)]
        [TestCase("0.10.2", 0, 10, 2)]
        [TestCase("", 0, 0, 0)]
        [Category("UnitTest")]
        public void Partial_version_to_zero_version_substitutes_zeroes_for_nulls(string input, int? expectedMajor, int? expectedMinor, int? expectedPatch)
        {
            // Arrange
            var partial = new PartialVersion(input);

            // Act
            var zeroVersion = partial.ToZeroVersion();
            
            // Assert
            Assert.AreEqual(expectedMajor, zeroVersion.Major);
            Assert.AreEqual(expectedMinor, zeroVersion.Minor);
            Assert.AreEqual(expectedPatch, zeroVersion.Patch);
        }
    }
}