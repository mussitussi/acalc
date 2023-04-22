
public interface ICalculatorAsyncEnumerable
{
    IAsyncEnumerable<CalcResult> Calculate(CalcInput calcInput);
}
