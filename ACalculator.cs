public class ACalculator : ICalculatorAsyncEnumerable
{
    private readonly O2P _o2p;
    private readonly P2O _p2o;

    public ACalculator(O2P o2p, P2O p2o)
    {
        _o2p = o2p;
        _p2o = p2o;
    }

    public async IAsyncEnumerable<CalcResult> Calculate(CalcInput calcInput)
    {
        if (calcInput.Pair.Type == CalcType.Price)
            await foreach (var calcResult in _FromPrice(calcInput))
                yield return calcResult;
        else if (calcInput.Pair.Type == CalcType.OAS)
            await foreach (var calcResult in _FromOAS(calcInput))
                yield return calcResult;
        else if (calcInput.Pair.Type == CalcType.DeltaPrice)
            await foreach (var calcResult in _FromDeltaPrice(calcInput))
                yield return calcResult;
        else if (calcInput.Pair.Type == CalcType.DeltaOAS)
            await foreach (var calcResult in _FromDeltaOAS(calcInput))
                yield return calcResult;
    }

    private async IAsyncEnumerable<CalcResult> _FromPrice(CalcInput calcInput)
    {
        // *** p -> p0 -> dp ***
        // *** p -> o -> o0 -> do

        // p
        var p = CalcResult.Price(calcInput.Pair.Value);
        yield return p;

        // p0
        var p0 = _RefPrice(calcInput.References);

        // dp
        if (p0 is not null)
            yield return CalcResult.DeltaPrice(p.Pair.Value - p0.Pair.Value);

        // o
        var o = await _p2o.Calculate(calcInput).ConfigureAwait(false);
        if (o is null)
            yield break;

        yield return o;

        // o0
        var o0 = _RefOAS(calcInput.References);

        // do
        if (o0 is not null)
            yield return CalcResult.DeltaOAS(o.Pair.Value - o0.Pair.Value);
    }

    private async IAsyncEnumerable<CalcResult> _FromOAS(CalcInput calcInput)
    {
        // *** o -> o0 -> do ***
        // *** o -> p -> po -> dp ***

        // o 
        var o = new CalcResult(calcInput.Pair);
        yield return o;

        // o0
        var o0 = _RefOAS(calcInput.References);

        // do
        if (o0 is not null)
            yield return CalcResult.DeltaOAS(o.Pair.Value - o0.Pair.Value);

        // p
        var p = await _o2p.Calculate(calcInput).ConfigureAwait(false);
        if (p is null)
            yield break;

        yield return p;

        // p0 
        var p0 = _RefPrice(calcInput.References);

        // dp
        if (p0 is not null)
            yield return CalcResult.DeltaPrice(p.Pair.Value - p0.Pair.Value);
    }

    private async IAsyncEnumerable<CalcResult> _FromDeltaPrice(CalcInput calcInput)
    {
        // *** dp -> p0 -> p -> o -> o0 -> do *** 

        // dp
        var dp = new CalcResult(calcInput.Pair);
        yield return dp;

        // p0
        var p0 = _RefPrice(calcInput.References);
        if (p0 is null)
            yield break;

        // p
        var p = CalcResult.Price(p0.Pair.Value + dp.Pair.Value);
        yield return p;

        // o
        var o = await _p2o.Calculate(CalcInput.Create(p.Pair)).ConfigureAwait(false);
        if (o is null)
            yield break;

        yield return o;

        // o0
        var o0 = _RefOAS(calcInput.References);

        // do
        if (o0 is not null)
            yield return CalcResult.DeltaOAS(o.Pair.Value - o0.Pair.Value);
    }

    private async IAsyncEnumerable<CalcResult> _FromDeltaOAS(CalcInput calcInput)
    {
        // *** do -> o0 -> o -> p -> p0 -> dp  ***

        // do
        var @do = CalcResult.DeltaOAS(calcInput.Pair.Value);
        yield return @do;

        // o0
        var o0 = _RefOAS(calcInput.References);
        if (o0 is null)
            yield break;

        // o 
        var o = CalcResult.OAS(o0.Pair.Value + @do.Pair.Value);
        yield return o;

        // p
        var p = await _o2p.Calculate(CalcInput.Create(o.Pair)).ConfigureAwait(false);
        if (p is null)
            yield break;

        yield return p;

        // p0
        var p0 = _RefPrice(calcInput.References);

        // dp
        if (p0 is not null)
            yield return CalcResult.DeltaPrice(p.Pair.Value - p0.Pair.Value);
    }

    private CalcResult? _RefPrice(IEnumerable<CalcResult> references) =>
        references.FirstOrDefault(x => x.Pair.Type == CalcType.Price);

    private CalcResult? _RefOAS(IEnumerable<CalcResult> references) =>
        references.FirstOrDefault(x => x.Pair.Type == CalcType.OAS);
}