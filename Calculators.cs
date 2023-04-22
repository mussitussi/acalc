public enum CalcType
{
    Price,
    OAS,
    DeltaOAS,
    DeltaPrice
}

public record Pair(CalcType Type, double Value);

public record CalcResult(Pair Pair)
{
    public static CalcResult Price(double value) => new CalcResult(new Pair(CalcType.Price, value));
    public static CalcResult DeltaPrice(double value) => new CalcResult(new Pair(CalcType.DeltaPrice, value));
    public static CalcResult OAS(double value) => new CalcResult(new Pair(CalcType.OAS, value));
    public static CalcResult DeltaOAS(double value) => new CalcResult(new Pair(CalcType.DeltaOAS, value));
}

public record CalcInput(Pair Pair, IEnumerable<CalcResult> References)
{
    public static CalcInput Create(Pair pair) => new CalcInput(pair, Enumerable.Empty<CalcResult>());
    public override string ToString() => $"CalcInput({Pair}, [{string.Join(",", this.References)}])";
}


public class P2O : ICalculatorAsync
{
    public async Task<CalcResult> Calculate(CalcInput calcInput)
    {
        if (calcInput.Pair.Type != CalcType.Price)
            return null!;
        else
        {
            await Task.Delay(3500);
            return new CalcResult(new Pair(CalcType.OAS, calcInput.Pair.Value - 100.0));
        }
    }
}


public class O2P : ICalculatorAsync
{
    public async Task<CalcResult> Calculate(CalcInput calcInput)
    {
        if (calcInput.Pair.Type != CalcType.OAS)
            return null!;
        else
        {
            await Task.Delay(3500);
            return new CalcResult(new Pair(CalcType.Price, calcInput.Pair.Value + 100.0));
        }
    }
}
