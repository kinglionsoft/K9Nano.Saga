using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using K9Nano.Saga;
using Xunit;
using Xunit.Abstractions;

namespace SagaTest
{
    public class RetryTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RetryTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }


        [Fact]
        public async Task RetryAndSuccess()
        {
            var context = await SagaHelper
                    .StartsWith(() => new ValueTask<SagaContext>(new SagaContext()))
                    .Then(ctx =>
                    {
                        ctx.SetState("key", new List<int> { 1 });

                        return new ValueTask();
                    }, "step1")
                    .Then(ctx =>
                    {
                        _testOutputHelper.WriteLine("step2  running");
                        ctx.GetState<List<int>>("key").Add(2);

                        return new ValueTask();
                    }, "step2")
                        .Retry(3)
                    .Then(ctx =>
                    {
                        ctx.GetState<List<int>>("key").Add(3);

                        return new ValueTask();
                    })
                    .Build()
                    .ExecuteAsync()
                ;

            context.ExecutedSteps.Should().BeEquivalentTo(new[] { "step1", "step2", "step-2" });

            var value = context.GetState<List<int>>("key");

            value.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        [Fact]
        public async Task RetryAndError()
        {
            int count = 0;

            var context = await SagaHelper
                    .StartsWith(() => new ValueTask<SagaContext>(new SagaContext()))
                    .Then(ctx =>
                    {
                        ctx.SetState("key", new List<int> { 1 });

                        return new ValueTask();
                    }, "step1")
                    .Then(ctx =>
                    {
                        _testOutputHelper.WriteLine("step2  running");
                        count++;
                        if (count < 3)
                        {
                            throw new Exception("test");
                        }
                        ctx.GetState<List<int>>("key").Add(2);
                        _testOutputHelper.WriteLine("step2 running success");
                        return new ValueTask();
                    }, "step2")
                    .Retry(3)
                    .Then(ctx =>
                    {
                        ctx.GetState<List<int>>("key").Add(3);

                        return new ValueTask();
                    })
                    .Build()
                    .ExecuteAsync()
                ;

            context.ExecutedSteps.Should().BeEquivalentTo(new[] { "step1", "step2", "step-2" });

            var value = context.GetState<List<int>>("key");

            value.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        [Fact]
        public async Task RetryAndCompensate()
        {
            var context = await SagaHelper
                    .StartsWith(() => new ValueTask<SagaContext>(new SagaContext()))
                    .Then(ctx =>
                    {
                        ctx.SetState("key", new List<int> { 1 });

                        return new ValueTask();
                    }, "step1")
                    .Then(ctx =>
                    {
                        _testOutputHelper.WriteLine("step2 running");
                        ctx.GetState<List<int>>("key").Add(2);
                        throw new Exception("test");

                    }, "step2")
                        .Retry(3)
                        .CompensateWith(ctx =>
                        {
                            // !!! BE CAREFUL
                            // ctx.GetState<List<int>>("key").Add(2);  has been called 4 times
                            ctx.GetState<List<int>>("key").RemoveRange(1, 4);
                            return new ValueTask();
                        })
                    .Then(ctx =>
                    {
                        ctx.GetState<List<int>>("key").Add(3);

                        return new ValueTask();
                    })
                    .Build()
                    .ExecuteAsync()
                ;

            context.ExecutedSteps.Should().BeEquivalentTo(new[] { "step1", "step2", "step2" });

            var value = context.GetState<List<int>>("key");

            value.Should().BeEquivalentTo(new[] { 1 });
        }
    }
}