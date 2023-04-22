// using System.Threading.Tasks.Dataflow;

// public abstract class Agent<TMsg>
// {
//     public readonly ActionBlock<TMsg> _actionBlock;
//     public Agent() => _actionBlock = new ActionBlock<TMsg>(_Process);
//     protected abstract Task _Process(TMsg msg);
//     public void Post(TMsg msg) => _actionBlock.Post(msg);
// }

// public record CalcMsg(Guid Id, CalcInput? Input, CalcResult? Result, string? Finished);


// public class ClientAgent : Agent<CalcMsg>
// {
//     private readonly Agent<CalcMsg> _calcAgent;
//     private Dictionary<Guid, CalcInput> _idToInput = new();
//     private Dictionary<Guid, List<CalcResult>> _idToResult = new();

//     public ClientAgent(Agent<CalcMsg> calcAgent)
//     {
//         _calcAgent = calcAgent;
//     }

//     protected override Task _Process(CalcMsg msg)
//     {
//         if (msg.Input is not null)
//         {
//             _idToInput[msg.Id] = msg.Input;
//             _calcAgent.Post(msg);
//         }
//         else if (msg.Result is not null)
//         {
//             if (_idToResult.TryGetValue(msg.Id, out var lst))
//             {
//                 lst.Add(msg.Result);
//             }
//             else
//             {
//                 lst = new List<CalcResult> { msg.Result };
//                 _idToResult[msg.Id] = lst;
//             }
//         }
//         else if (msg.Finished is not null)
//         {
//             Console.WriteLine($"{msg.Id}: " + string.Join(",", _idToResult[msg.Id]));
//         }

//         return Task.CompletedTask;
//     }
// }


// public class CalcAgent : Agent<CalcMsg>
// {
//     private readonly List<CalcResult> _results = new();

//     private Agent<(Guid Id, string Isin, double Price)> _p2o;
//     private Agent<(Guid Id, string Isin, double Oas)> _o2p;
//     private Agent<CalcMsg> _clientAgent;

//     public CalcAgent()
//     {
//         _p2o = new P2OAgent(this);
//         _o2p = new O2PAgent(this);
//         _clientAgent = new ClientAgent(this);
//     }

//     protected override async Task _Process(CalcMsg msg)
//     {
//         if (msg.Input is not null)
//         {
//             var input = msg.Input;
//             if (input.Pair.Type == CalcType.Price)
//             {
//                 // price
//                 var p = new CalcResult(input.Pair);
//                 _clientAgent.Post(new CalcMsg(msg.Id, null, p, null));

//                 // delta-price?
//                 var p0 = input.References.FirstOrDefault(x => x.Pair.Type == CalcType.Price);
//                 if (p0 is not null)
//                 {
//                     var dp = new CalcResult(new Pair(CalcType.DeltaPrice, p.Pair.Value - p0.Pair.Value));
//                     _clientAgent.Post(new CalcMsg(msg.Id, null, dp, null));
//                 }

//                 _p2o.Post((msg.Id, "dk", p.Pair.Value));
//             }
//         }
//         else if (msg.Result is not null)
//         {
//             var (t, v)= msg.Result.Pair;
//             if (t == CalcType.OAS)
//             {
//                 // delta-price?
//                 var o0 = msg.References.FirstOrDefault(x => x.Pair.Type == CalcType.Price);

//             }
//         }
//     }

//     public class O2PAgent : Agent<(Guid Id, string Isin, double OAS)>
//     {
//         private readonly Agent<CalcMsg> _calcAgent;

//         public O2PAgent(Agent<CalcMsg> calcAgent)
//         {
//             _calcAgent = calcAgent;
//         }

//         protected override async Task _Process((Guid Id, string Isin, double OAS) msg)
//         {
//             await Task.Delay(3000);
//             var r = new CalcResult(new Pair(CalcType.Price, 100 + msg.OAS));
//             _calcAgent.Post(new CalcMsg(msg.Id, null, r, null));
//         }
//     }
// }

// public class P2OAgent : Agent<(Guid Id, string Isin, double Price)>
// {
//     private readonly Agent<CalcMsg> _calcAgent;

//     public P2OAgent(Agent<CalcMsg> calcAgent)
//     {
//         _calcAgent = calcAgent;
//     }

//     protected override async Task _Process((Guid Id, string Isin, double Price) msg)
//     {
//         await Task.Delay(3000);
//         var r = new CalcResult(new Pair(CalcType.OAS, msg.Price - 100.0));
//         _calcAgent.Post(new CalcMsg(msg.Id, null, r, null));
//     }
// }