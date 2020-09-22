using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using K9Nano.Saga;
using Xunit;

namespace SagaTest
{
    public class CustomContextTest
    {
        [Fact]
        public async Task RunToEnd()
        {
            var context = await SagaHelper
                    .StartsWith(() => Task.FromResult(new CustomContext()))
                    .Then(ctx =>
                    {
                        ctx.Value += "1";

                        return Task.CompletedTask;
                    }, "step1")
                    .Then(ctx =>
                    {
                        ctx.Value += "2";

                        return Task.CompletedTask;
                    }, "step2")
                    .Then(ctx =>
                    {
                        ctx.Value += "3";

                        return Task.CompletedTask;
                    })
                    .Build()
                    .ExecuteAsync()
                ;

            context.ExecutedSteps.Should().BeEquivalentTo(new[] { "step1", "step2", "step-2" });
            
            context.Value.Should().Be("123");
        }

        private class CustomContext : SagaContextBase
        {
            public string Value { get; set; } = string.Empty;
        }
    }
}