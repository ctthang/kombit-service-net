using System;
using System.IO;
using System.ServiceModel.Channels;
using System.Xml.Linq;

namespace Kombit.Samples.Consumer
{
    /// <summary>
    ///     A message modifier is responsible for modifying a message. This specific modifier will save a message at the first
    ///     time it is called. From the second time, it will return the
    ///     previously saved message if replayPreviousMessage is true
    /// </summary>
    internal class ReplayMessageModifier
    {
        private readonly bool replayPreviousMessage;
        private XElement previousMessage;

        /// <summary>
        ///     Creates an instance of this class!
        /// </summary>
        /// <param name="replayPreviousMessage">Specifies if this modifier should replay a previously saved message</param>
        public ReplayMessageModifier(bool replayPreviousMessage)
        {
            this.replayPreviousMessage = replayPreviousMessage;
        }

        /// <summary>
        ///     Saves a message if this is called the first time.
        ///     Return a previously saved message if replayPreviousMessage is true from the second call.
        /// </summary>
        /// <param name="encodedMessage"></param>
        /// <param name="bufferManager"></param>
        /// <param name="messageOffset"></param>
        /// <returns>Message in form of a byte array</returns>
        public ArraySegment<byte> Modify(ArraySegment<byte> encodedMessage, BufferManager bufferManager,
            int messageOffset)
        {
            XElement messageToUse;
            if (replayPreviousMessage && previousMessage != null)
            {
                messageToUse = previousMessage;
            }
            else
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(encodedMessage.Array, encodedMessage.Offset, encodedMessage.Count);

                    stream.Position = 0;
                    XElement currentMessage = XElement.Load(stream, LoadOptions.PreserveWhitespace);

                    messageToUse = currentMessage;
                    previousMessage = messageToUse;
                }
            }

            using (MemoryStream stream = new MemoryStream())
            {
                messageToUse.Save(stream, SaveOptions.DisableFormatting);

                byte[] messageBytes = stream.GetBuffer();
                int messageLength = (int) stream.Position;
                int totalLength = messageLength + messageOffset;
                byte[] totalBytes = bufferManager.TakeBuffer(totalLength);
                Array.Copy(messageBytes, 0, totalBytes, messageOffset, messageLength);
                ArraySegment<byte> byteArray = new ArraySegment<byte>(totalBytes, messageOffset, messageLength);
                return byteArray;
            }
        }
    }
}