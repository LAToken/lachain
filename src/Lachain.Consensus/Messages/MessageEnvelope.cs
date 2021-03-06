using Lachain.Proto;

namespace Lachain.Consensus.Messages
{
    public class MessageEnvelope
    {
        public ConsensusMessage? ExternalMessage { get; }

        public int ValidatorIndex { get; }
        public IInternalMessage? InternalMessage { get; }

        public MessageEnvelope(ConsensusMessage msg, int validatorIndex)
        {
            ExternalMessage = msg;
            InternalMessage = null;
            ValidatorIndex = validatorIndex;
        }

        public MessageEnvelope(IInternalMessage msg, int validatorIndex)
        {
            InternalMessage = msg;
            ExternalMessage = null;
            ValidatorIndex = validatorIndex;
        }

        public bool External => !(ExternalMessage is null);

        public string TypeString()
        {
            if (External) return ExternalMessage!.PayloadCase.ToString();
            return InternalMessage!.GetType().GetGenericTypeDefinition().Name.Contains("Request")
                ? "ProtocolRequest"
                : "ProtocolResponse";
        }
    }
}