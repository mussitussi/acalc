Console.WriteLine("Hello, World!");

var c = new ACalculator(new O2P(), new P2O());

var i1 = CalcInput.Create(new Pair(CalcType.Price, 100.0));

var references = new List<CalcResult> { CalcResult.Price(102.0), CalcResult.OAS(0.1) };

var i2 = new CalcInput(new Pair(CalcType.OAS, 1.11), references);
var i3 = new CalcInput(new Pair(CalcType.DeltaOAS, 0.11), references);
var i4 = new CalcInput(new Pair(CalcType.Price, 124.11), references);


foreach (var run in Enumerable.Range(1, 2))
{
    Console.WriteLine($"Run {run}");
    foreach (var i in new[] { i1, i2, i3, i4 })
    {
        Console.WriteLine($"{DateTime.Now.TimeOfDay}: {i}");
        await foreach (var r in c.Calculate(i))
            Console.WriteLine($"{DateTime.Now.TimeOfDay}: {r}");
    }
}