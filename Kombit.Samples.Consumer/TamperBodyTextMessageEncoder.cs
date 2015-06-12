using System;
using System.IO;
using System.ServiceModel.Channels;

namespace Kombit.Samples.Consumer
{
    /// <summary>
    ///     A message encoder which wraps a real encoder and that the same time provides an extensible point to modify a
    ///     message before it is sent over the wire
    /// </summary>
    public class TamperBodyTextMessageEncoder : MessageEncoder
    {
        private readonly CustomTextMessageEncoderFactory factory;
        private readonly Func<ArraySegment<byte>, BufferManager, int, ArraySegment<byte>> messageModifier;
        private readonly MessageEncoder wrappedMessageEncoder;

        public TamperBodyTextMessageEncoder(CustomTextMessageEncoderFactory factory, MessageEncoder encoder,
            Func<ArraySegment<byte>, BufferManager, int, ArraySegment<byte>> messageModifier)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            if (encoder == null)
                throw new ArgumentNullException("encoder");
            if (messageModifier == null)
                throw new ArgumentNullException("messageModifier");

            this.factory = factory;
            wrappedMessageEncoder = encoder;
            this.messageModifier = messageModifier;
        }

        /// <summary>
        ///     Message's content type
        /// </summary>
        public override string ContentType
        {
            get { return wrappedMessageEncoder.ContentType; }
        }

        /// <summary>
        ///     Message's media type
        /// </summary>
        public override string MediaType
        {
            get { return wrappedMessageEncoder.MediaType; }
        }

        /// <summary>
        ///     Message's version
        /// </summary>
        public override MessageVersion MessageVersion
        {
            get { return factory.MessageVersion; }
        }

        /// <summary>
        ///     Reads a message from a byte array. Simply call the wrapped encoder
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bufferManager"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            return wrappedMessageEncoder.ReadMessage(buffer, bufferManager, contentType);
        }

        /// <summary>
        ///     Reads a message from a stream. Simply call the wrapped encoder
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="maxSizeOfHeaders"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            return wrappedMessageEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
        }

        /// <summary>
        ///     Writes a message to a byte array. Firstly, call the wrapped encoder. Secondly, apply modification before returning
        ///     it
        /// </summary>
        /// <param name="message"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="bufferManager"></param>
        /// <param name="messageOffset"></param>
        /// <returns></returns>
        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager,
            int messageOffset)
        {
            ArraySegment<byte> encodedMessage = wrappedMessageEncoder.WriteMessage(message, maxMessageSize,
                bufferManager, messageOffset);

            return messageModifier(encodedMessage, bufferManager, messageOffset);
        }

        /// <summary>
        ///     Writes a message to a stream. Simply call the wrapped encoder
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stream"></param>
        public override void WriteMessage(Message message, Stream stream)
        {
            wrappedMessageEncoder.WriteMessage(message, stream);
        }
    }
}