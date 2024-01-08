using ChannelFactory;
using MessageChannel;
using MessageConstruction;
using MessageConstruction.IdentifiedMessage;
using MessageDispatcher;
using MessageTransformation.CanonicalDataModel.Message.DecisionMessage;
using Receiver;
using Match;

using ConnectorIdentifier = System.Int32;
using ChannelIdentifier = System.Int32;
using MessageIdentifier = System.Int32;
using MessageConstruction.CorrelationIdentifier;
using ComputerPlayer;

namespace Match;
public class Program
{
    public static void Main()
    {
        var identifiedChannelDictionary = new IdentifiedChannelDictionary<IIdentifiedChannel<ChannelIdentifier>, ChannelIdentifier>();

        var connectionTracker = new ConnectionTracker<ConnectorIdentifier>(new Dictionary<ConnectorIdentifier, IConnectionConfirmation>());

        var playerLimitationAllower = new LimitedConnectionRequestMessageAllower<IIdentifiedSenderConnectionRequestMessage,ConnectorIdentifier>(2, connectionTracker);

        var messageChannelSourceTracker = new MessageChannelSourceTracker<ChannelIdentifier,MessageIdentifier>(new Dictionary<ChannelIdentifier, MessageIdentifier>());

        var identifiedDecisionMessageChannelFactory = new TempChannelFactory<IIdentifiedChannel<ChannelIdentifier>>((IReceiver receiver) =>
            {
                var generateRandom = new Func<ChannelIdentifier>(() =>
                {
                    return (identifiedChannelDictionary.Count + 1);
                });

                ChannelIdentifier channelIdentifier = generateRandom();
                var newIdentifiedChannel = new FeatureIntegrableIdentifiedChannel<ChannelIdentifier>(
                    new MessageTrackingFeature<IIdentifiedChannel<ChannelIdentifier>,ChannelIdentifier,IUsedMessage,MessageIdentifier>(messageChannelSourceTracker),
                    new PointToPointChannel(receiver),
                    channelIdentifier);
                identifiedChannelDictionary.Add(channelIdentifier, newIdentifiedChannel);
                return newIdentifiedChannel;
            });

        var channelCorrelationTracker = new ChannelCorrelationTracker<ChannelIdentifier>(new Dictionary<ChannelIdentifier, ChannelIdentifier>());

        var messageReplyingPerformer = new MessageReplyingPerformer<IIdentifiedChannel<ChannelIdentifier>, ChannelIdentifier, MessageIdentifier, IUsedMessage>(messageChannelSourceTracker, channelCorrelationTracker, identifiedChannelDictionary);

        var decisionMessageDispatcher = new Communicator<IDecisionMessage>(messageReplyingPerformer);

        var messageSendingChannelReceiverFactory = new TempReceiverFactory<IReceiver>(() => decisionMessageDispatcher);
        var messageReceivedChannelReceiverFactory = new TempReceiverFactory<TempMessageReceivedNotifier>(() => new TempMessageReceivedNotifier()); 
        
        var acceptedPlayerConnectionRequestReplyingMessageFactory = new AcceptedPlayerConnectionRequestReplyingMessageFactory<ConnectorIdentifier, IReceiver,TempMessageReceivedNotifier, ChannelIdentifier, TempConnectionRequestMessage<ConnectorIdentifier>, AcceptedPlayerConnectionRequestReplyingMessage>(
            identifiedDecisionMessageChannelFactory,
            identifiedDecisionMessageChannelFactory,
            messageSendingChannelReceiverFactory,
            messageReceivedChannelReceiverFactory,
            channelCorrelationTracker,
            connectionTracker
            );

        var acceptedPlayerConnectionRequestReplier = new ConnectionRequesMessageReplier<TempConnectionRequestMessage<ConnectorIdentifier>>(acceptedPlayerConnectionRequestReplyingMessageFactory);

        //var acceptedViewerConnectionRequestReplier;
        //var acceptedArbiterConnectionRequestReplier;
        //var deniedConnectionRequestReplier;

        IAsyncPerformer connectionRequestMessagePerformer = new CompositeAsyncPerformer(new[]
        {
            acceptedPlayerConnectionRequestReplier
        });

        var organizer = new Organizer<IIdentifiedSenderConnectionRequestMessage>(connectionRequestMessagePerformer);

        var setupChannel = new PointToPointChannel(organizer);


        var computerPlayer = new Player(setupChannel);
        //IPlayer humanPlayer = new HumanPlayer();

        //computerPlayer.Send(new ConnectionRequest()).To(matchSetupMessageSystem.SetupChannel);
        //humanPlayer.Send(new ConnectionRequest()).To(matchSetupMessageSystem.SetupChannel);

        //if (continueToPlay == true)
        //{
        //    return;
        //}
    }

    private class TempConnectionRequestMessage<TSenderIdentifier> : ISenderIdentifiedMessage<TSenderIdentifier>, IConnectionRequestMessage
        where TSenderIdentifier : IEquatable<TSenderIdentifier>
    {
        public TSenderIdentifier SenderIdentfier => throw new NotImplementedException();

        public IChannel ReplyingConnectionRequestMessageChanel => throw new NotImplementedException();
    }

    private class FeatureIntegrableIdentifiedChannel<TChannelIdentifier> : FeatureIntegrableChannel, IIdentifiedChannel<TChannelIdentifier>
        where TChannelIdentifier : IEquatable<TChannelIdentifier>
    {
        public FeatureIntegrableIdentifiedChannel(IMessageSendingFeature channelFeature, IChannel adaptedChannel, TChannelIdentifier channelIdentifier) : base(channelFeature, adaptedChannel)
        {
            ChannelIdentifier = channelIdentifier;
        }

        public TChannelIdentifier ChannelIdentifier { get; private set; }
    }

    private class TempMessageReceivedNotifier : IMessageReceivingNotifier, IReceiver
    {
        public event EventHandler<IMessage>? OnReceivingMessage;

        public void Receive(IMessage message)
        {
            OnReceivingMessage?.Invoke(this, message);
        }
    }

    private interface IIdentifiedSenderConnectionRequestMessage : IConnectionRequestMessage, ISenderIdentifiedMessage<ConnectorIdentifier>
    {

    }

    private class TempChannelFactory<TChannel> : IChannelFactory<TChannel>
        where TChannel : IChannel
    {
        public TempChannelFactory(Func<IReceiver, TChannel> createChannelFunction)
        {
            _CreateChannelFunction = createChannelFunction;
        }

        protected Func<IReceiver, TChannel> _CreateChannelFunction { get; private set; }

        public TChannel Create(IReceiver receiver)
        {
            return _CreateChannelFunction(receiver);
        }
    }

    private class TempReceiverFactory<TReceiver> : IReceiverFactory<TReceiver>
        where TReceiver : IReceiver
    {
        public TempReceiverFactory(Func<TReceiver> createReceiverFunction)
        {
            _CreateReceiverFunction = createReceiverFunction;
        }

        protected Func<TReceiver> _CreateReceiverFunction { get; private set; }

        public TReceiver Create()
        {
            return _CreateReceiverFunction();
        }
    }

    private interface IUsedMessage : IIdentifiedMessage<MessageIdentifier>, IDecisionMessage, IIdentifiedCorrelationMessage<MessageIdentifier>
    {

    }
}


//using System;
//using System.Threading.Tasks;
//using System.Threading;
//using System.Collections.Generic;
//using System.Linq;

//public class Program
//{
//    public static Task<Task<T>>[] Interleaved<T>(IEnumerable<Task<T>> tasks)
//    {
//        var inputTasks = tasks.ToList();

//        var taskCompletionSourceBuckets = new TaskCompletionSource<Task<T>>[inputTasks.Count];
//        var results = new Task<Task<T>>[taskCompletionSourceBuckets.Length];
//        for (int i = 0; i < taskCompletionSourceBuckets.Length; i++)
//        {
//            taskCompletionSourceBuckets[i] = new TaskCompletionSource<Task<T>>();
//            results[i] = taskCompletionSourceBuckets[i].Task;
//        }

//        int nextTaskIndex = -1;
//        Action<Task<T>> continuation = (Task<T> completedTask) =>
//        {
//            Interlocked.Increment(ref nextTaskIndex);
//            var bucket = taskCompletionSourceBuckets[nextTaskIndex];
//            bucket.TrySetResult(completedTask);
//        };

//        foreach (var inputTask in inputTasks)
//            inputTask.ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

//        return results;
//    }

//    public async static Task Run()
//    {
//        var tasks = new[] {
//            Task.Delay(0000).ContinueWith(task => 0),
//            Task.Delay(3000).ContinueWith(task => 3),
//            Task.Delay(1000).ContinueWith(task => 1),
//            Task.Delay(2000).ContinueWith(task => 2),
//            Task.Delay(5000).ContinueWith(task => 5),
//            Task.Delay(4000).ContinueWith(task => 4),
//        };

//        var test = Interleaved<int>(tasks);
//        foreach (var bucket in test)
//        {
//            var t = await bucket;
//            int result = await t;
//            Console.WriteLine("{0}: {1}", DateTime.Now, result);
//        }
//    }

//    public static void Main()
//    {

//        Run().Wait();
//    }
//}