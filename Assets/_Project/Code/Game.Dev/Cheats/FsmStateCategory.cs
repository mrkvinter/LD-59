// using Game.Core.FinalStateMachine;
// using UnityEngine;
//
// namespace Code.Cheats
// {
//     public class FsmStateCategory : ICheatCategory
//     {
//         public void Draw()
//         {
//             for (var i = 0; i < Fsm.AllFsm.Count; i++)
//             {
//                 GUILayout.Label($"[{i}] {Fsm.AllFsm[i].Name} :");
//                 GUILayout.Label($"\tCurrent State : {Fsm.AllFsm[i].CurrentState?.GetType().Name ?? "null"}");
//                 GUILayout.Label($"\tIs Changing State : {Fsm.AllFsm[i].IsChangingState}");
//             }
//         }
//     }
// }