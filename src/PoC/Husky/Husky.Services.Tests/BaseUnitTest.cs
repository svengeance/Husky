using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;

namespace Husky.Services.Tests
{
    public class BaseUnitTest<T> where T: class
    {
        protected IFixture _fixture { get; private set; } = null!;

        protected T _sut { get; private set; } = null!;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            _sut = _fixture.Create<T>();
        }
    }
}