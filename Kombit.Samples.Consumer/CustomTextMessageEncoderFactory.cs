using System;
using System.ServiceModel.Channels;

namespace Kombit.Samples.Consumer
{
    /// <summary>
    ///     Factory that provides instances of MessageEncoder.
    /// </summary>
    public class CustomTextMessageEncoderFactory : MessageEncoderFactory
    {
        private readonly MessageEncoder messageEncoder;
        private readonly MessageEncoderFactory wrappedMessageEncoderFactory;

        /// <summary>
        ///     Instantiates a new instance of the CustomTextMessageEncoderFactory class
        /// </summary>
        /// <param name="wrappedMessageEncoderFactory">A built-in .NET message encoder.</param>
        /// <param name="messageModifier">
        ///     A modifier function which receives a SOAP message as a byte array, performs some kind of
        ///     modification on it and returns the modified one
        /// </param>
        internal CustomTextMessageEncoderFactory(MessageEncoderFactory wrappedMessageEncoderFactory,
            Func<ArraySegment<byte>, BufferManager, int, ArraySegment<byte>> messageModifier)
        {
            this.wrappedMessageEncoderFactory = wrappedMessageEncoderFactory;
            messageEncoder = new TamperBodyTextMessageEncoder(this, this.wrappedMessageEncoderFactory.Encoder,
                messageModifier);
        }

        /// <summary>
        ///     Get encoder object
        /// </summary>
        public override MessageEncoder Encoder
        {
            get { return messageEncoder; }
        }

        /// <summary>
        ///     Get message version
        /// </summary>
        public override MessageVersion MessageVersion
        {
            get { return wrappedMessageEncoderFactory.MessageVersion; }
        }
    }
}