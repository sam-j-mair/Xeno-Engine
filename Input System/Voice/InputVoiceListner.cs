using Microsoft.Xna.Framework;
using System.Speech.Recognition;

/// <summary>
/// NOTE: THIS IS INCOMPLETE. 
/// </summary>
namespace XenoEngine.Systems
{
//     class InputVoiceListner : InputListner<>
//     {
//         //Load Dialog asset.
//         private readonly List<String> m_aCommands;
// 
//         private SpeechRecognitionEngine m_recognitionEngine;
// 
//         public InputVoiceListner() : base()
//         {
//             m_aCommands = new List<String>();
//             m_aCommands.Add("Start");
//             m_aCommands.Add("Move");
// 
//             m_recognitionEngine = new SpeechRecognitionEngine(new CultureInfo("en-US"));
//             m_recognitionEngine.SetInputToDefaultAudioDevice();
//             
//             CommandSegment intro = new CommandSegment("Intro", new KeyValue("Move", "Move"), new KeyValue("Attack", "Attack"), new KeyValue("Select", "Select"));
//             CommandSegment intro1 = new CommandSegment("intro1", new KeyValue("Target", "Target"), new KeyValue("Unit", "Unit"));
//             CommandSegment numberSegment = new CommandSegment("number",
//                 new KeyValue("One", 1),
//                 new KeyValue("Two", 2),
//                 new KeyValue("Three", 3),
//                 new KeyValue("Four", 4),
//                 new KeyValue("Five", 5),
//                 new KeyValue("Six", 6));
// 
//             CommandObject command1 = new CommandObject(intro, intro1, numberSegment);
//             CommandObject command2 = new CommandObject(intro, intro1);
// 
//             GrammarGenerator generator = new GrammarGenerator(command1, command2);
// 
//             Grammar grammar = generator.GenerateGrammarObject();
// 
//             grammar.SpeechRecognized += new System.EventHandler<SpeechRecognizedEventArgs>(InputRecievedHandlerTEST);
// 
//             m_recognitionEngine.LoadGrammar(grammar);
//             m_recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
//         }
// 
//         public void InputRecievedHandlerTEST(object sender, SpeechRecognizedEventArgs args)
//         {
//             RecognitionResult rslt = args.Result;
// 
//             Console.WriteLine(rslt.Semantics["Intro"].Value);
//             Console.WriteLine(rslt.Semantics["intro1"].Value);
// 
//             if (rslt.Semantics.ContainsKey("number"))
//                 Console.WriteLine(args.Result.Semantics["number"].Value.ToString());
//         }
// 
//         public void InputRecievedHandler(object sender, SpeechRecognizedEventArgs args)
//         {
//         }
// 
//         public void ProcessInput(object task)
//         {
//             Task<List<int>> taskType = task as Task<List<int>>;
//             String szCommands = "";
//             Regex regEx;
//             SpeechRecognizedEventArgs eventArgs = taskType.UserData as SpeechRecognizedEventArgs;
//             List<int> commandList = new List<int>();
// 
//             foreach (RecognizedWordUnit word in eventArgs.Result.Words)
//             {
//                 //This can be done safely as this is a read only variable
//                 //once created it doesn't change.
//                 foreach (String szCommand in m_aCommands)
//                 {
//                     if(szCommand == word.Text)
//                     {
//                         commandList.Add(szCommand.GetHashCode());
//                     }
//                 }
//             }
// 
//             taskType.ReturnData = commandList;
//         }
// 
//         public void TaskCompleted(object returnData)
//         {
//             List<int> returnList = returnData as List<int>;
// 
//             foreach (int hashCommand in returnList)
//             {
//                 foreach (ActionBinding<String> binding in ActionMap.Bindings)
//                 {
//                     if(binding.Key.GetHashCode() == hashCommand)
//                     {
//                         FireEvent(binding.Event);
//                     }
//                 }
//             }
// 
//         }
//     }

//     class IdleState : State<>
//     {
//         public override void OnEnter(StateMachine stateMachine)
//         {
//         }
// 
//         public override void OnUpdate(StateMachine stateMachine, GameTime gameTime)
//         {
//         }
// 
//         public override void OnExit(StateMachine stateMachine)
//         {
//         }
//     }
// 
//     class ListeningState : State
//     {
//         public override void OnEnter(StateMachine stateMachine)
//         {
//         }
// 
//         public override void OnUpdate(StateMachine stateMachine, GameTime gameTime)
//         {
//         }
// 
//         public override void OnExit(StateMachine stateMachine)
//         {
//         }
//     }


//         private void ReadInBuffer()
//         {
//             //m_szInputBuffer = SpeechSDK.GetSpeechToString.
//         }
// 
//         public void StartRead()
//         {
//             
//         }

    }

