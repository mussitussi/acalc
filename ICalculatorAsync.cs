
public interface ICalculatorAsync
{
    Task<CalcResult> Calculate(CalcInput calcInput);
}
