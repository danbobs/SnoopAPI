using AutoFixture;
using AutoFixture.AutoMoq;

namespace Snoop.API.UnitTests
{
    public class TestBase
    {
        public TestBase()
        {
            this.Fixture = new Fixture();
            this.Fixture.Customize(new AutoMoqCustomization() { ConfigureMembers = true }); ;
            this.Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            this.Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public IFixture Fixture { get; set; }
    }
}
