# K9Nano.Saga

A lightweight component for building SAGA pattern.

## Features

* Retry
* Compensate if error happened on reversing order.

## Get Started

* Quick usage

```c#
var context = await SagaHelper
    .StartsWith(() => new ValueTask<SagaContext>(new SagaContext()))
    .Then(ctx =>
    {
        ctx.SetState("key", new List<int> { 1 });

        return new ValueTask();
    }, "step1")
        .CompensateWith(ctx =>
        {
            ctx.GetState<List<int>>("key").RemoveAt(0);
            return new ValueTask();
        })

    .Then(ctx =>
    {
        // do nothing 
        return new ValueTask();
    }, "step-nothing")

    .Then(ctx =>
    {
        ctx.GetState<List<int>>("key").Add(2);
        throw new Exception("test");
    }, "step2")
        .CompensateWith(ctx =>
        {
            ctx.GetState<List<int>>("key").RemoveAt(1);
            return new ValueTask();
        })

    .Then(ctx =>
    {
        ctx.GetState<List<int>>("key").Add(3);

        return new ValueTask();
    }, "step3")
    .Build()
    .ExecuteAsync()
```

* Custom ISagaContext

``` C#
public async Task RunToEnd()
{
    var context = await SagaHelper
            .StartsWith(() => new ValueTask<CustomContext>(new CustomContext()))
            .Then(ctx =>
            {
                ctx.Value += "1";

                return new ValueTask();
            }, "step1")
            .Then(ctx =>
            {
                ctx.Value += "2";

                return new ValueTask();
            }, "step2")
            .Then(ctx =>
            {
                ctx.Value += "3";

                return new ValueTask();
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

```

* More samples in [test\SagaTest]([.\test\SagaTest)