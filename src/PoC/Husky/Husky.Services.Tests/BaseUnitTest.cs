using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;

namespace Husky.Services.Tests
{
    public class BaseUnitTest<T> where T: class
    {
        protected IFixture Fixture { get; private set; } = null!;

        protected T Sut { get; private set; } = null!;

        [SetUp]
        public void Setup()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            Sut = Fixture.Create<T>();
        }
    }
}