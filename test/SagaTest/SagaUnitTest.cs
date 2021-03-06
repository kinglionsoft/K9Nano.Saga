using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using K9Nano.Saga;
using Xunit;

namespace SagaTest
{
    public class SagaUnitTest
    {
        [Fact]
        public async Task RunToEnd()
        {
            var context = await SagaHelper
                 .StartsWith(() => Task.FromResult(new SagaContext()))
                 .Then(ctx =>
                 {
                     ctx.SetState("key", new List<int> { 1 });

                     return Task.CompletedTask;
                 }, "step1")
                 .Then(ctx =>
                 {
                     ctx.GetState<List<int>>("key").Add(2);

                     return Task.CompletedTask;
                 }, "step2")
                 .Then(ctx =>
                 {
                     ctx.GetState<List<int>>("key").Add(3);

                     return Task.CompletedTask;
                 })
                 .Build()
                 .ExecuteAsync()
                 ;

            context.ExecutedSteps.Should().BeEquivalentTo(new[] { "step1", "step2", "step-2" });

            var value = context.GetState<List<int>>("key");

            value.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        [Fact]
        public async Task RunToFailed()
        {
            var context = await SagaHelper
                    .StartsWith(() => Task.FromResult(new SagaContext()))
                    .Then(ctx =>
                    {
                        ctx.SetState("key", new List<int> { 1 });

                        return Task.CompletedTask;
                    }, "step1")
                    .Then(ctx =>
                    {
                        ctx.GetState<List<int>>("key").Add(2);
                        ctx.Success = false;
                        return Task.CompletedTask;
                    }, "step2")
                    .Then(ctx =>
                    {
                        ctx.GetState<List<int>>("key").Add(3);

                        return Task.CompletedTask;
                    }, "step3")
                    .Build()
                    .ExecuteAsync()
                ;

            var value = context.GetState<List<int>>("key");

            value.Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public async Task RunToError()
        {
            var context = await SagaHelper
                    .StartsWith(() => Task.FromResult(new SagaContext()))
                    .Then(ctx =>
                    {
                        ctx.SetState("key", new List<int> { 1 });

                        return Task.CompletedTask;
                    }, "step1")
                    .Then(ctx =>
                    {
                        ctx.GetState<List<int>>("key").Add(2);
                        throw new Exception("test");
                    }, "step2")
                    .Then(ctx =>
                    {
                        ctx.GetState<List<int>>("key").Add(3);

                        return Task.CompletedTask;
                    }, "step3")
                    .Build()
                    .ExecuteAsync()
                ;

            var value = context.GetState<List<int>>("key");

            value.Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public async Task Compensate()
        {
            var context = await SagaHelper
                    .StartsWith(() => Task.FromResult(new SagaContext()))
                    .Then(ctx =>
                    {
                        ctx.SetState("key", new List<int> { 1 });

                        return Task.CompletedTask;
                    }, "step1")
                        .CompensateWith(ctx =>
                        {
                            ctx.GetState<List<int>>("key").RemoveAt(0);
                            return Task.CompletedTask;
                        })

                    .Then(ctx =>
                    {
                        // do nothing 
                        return Task.CompletedTask;
                    }, "step-nothing")

                    .Then(ctx =>
                    {
                        ctx.GetState<List<int>>("key").Add(2);
                        throw new Exception("test");
                    }, "step2")
                        .CompensateWith(ctx =>
                        {
                            ctx.GetState<List<int>>("key").RemoveAt(1);
                            return Task.CompletedTask;
                        })

                    .Then(ctx =>
                    {
                        ctx.GetState<List<int>>("key").Add(3);

                        return Task.CompletedTask;
                    }, "step3")
                    .Build()
                    .ExecuteAsync()
                ;

            context.Success.Should().BeFalse();

            context.Error.InnerExceptions[0].Should().BeOfType<SagaException>();

            var sagaEx = (context.Error.InnerExceptions[0] as SagaException);
            sagaEx.StepName.Should().Be("step2");
            sagaEx.Message.Should().Be("test");

            context.ExecutedSteps.Should().BeEquivalentTo(new[] { "step1", "step-nothing", "step2", "step2", "step1" });

            var value = context.GetState<List<int>>("key");
            value.Should().BeEquivalentTo(new int[0]);
        }

        [Fact]
        public async Task CompensateError()
        {
            var context = new SagaContext();
            try
            {
                await SagaHelper
                    .StartsWith(() => Task.FromResult(context))
                    .Then(ctx =>
                    {
                        ctx.SetState("key", new List<int> { 1 });

                        return Task.CompletedTask;
                    }, "step1")
                        .CompensateWith(ctx =>
                        {
                            ctx.GetState<List<int>>("key").RemoveAt(0);
                            return Task.CompletedTask;
                        })

                    .Then(ctx =>
                    {
                        // do nothing 
                        return Task.CompletedTask;
                    }, "step-nothing")
                         .CompensateWith(ctx =>
                        {
                            throw new Exception("step-nothing");
                        }, true)

                    .Then(ctx =>
                    {
                        ctx.GetState<List<int>>("key").Add(2);
                        throw new Exception("test");
                    }, "step2")
                        .CompensateWith(ctx =>
                        {
                            ctx.GetState<List<int>>("key").RemoveAt(1);
                            return Task.CompletedTask;
                        })

                    .Then(ctx =>
                    {
                        ctx.GetState<List<int>>("key").Add(3);

                        return Task.CompletedTask;
                    }, "step3")
                    .Build()
                    .ExecuteAsync()
                ;
            }
            catch (SagaException ex)
            {
                ex.Message.Should().Be("step-nothing");
            }

            context.Success.Should().BeFalse();

            context.Error.InnerExceptions[0].Should().BeOfType<SagaException>();

            var sagaEx = (context.Error.InnerExceptions[0] as SagaException);
            sagaEx.StepName.Should().Be("step2");
            sagaEx.Message.Should().Be("test");

            context.ExecutedSteps.Should()
                .BeEquivalentTo(new[] { "step1", "step-nothing", "step2", "step2", "step-nothing" });

            var value = context.GetState<List<int>>("key");
            value.Should().BeEquivalentTo(new int[] { 1 });
        }
    }
}
