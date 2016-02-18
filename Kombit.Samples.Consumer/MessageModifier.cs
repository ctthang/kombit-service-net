using System;
using System.IO;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xml.Linq;

namespace Kombit.Samples.Consumer
{
    /// <summary>
    ///     Provides modifier methods which can modify message before sending to STS which is used for some special tests.
    /// </summary>
    internal static class MessageModifier
    {
        /// <summary>
        ///     This modifier adds an atribute to the Body element of a message which effectively invalidates message's signature.
        /// </summary>
        /// <param name="encodedMessage">The untouched message</param>
        /// <param name="bufferManager">Buffer manager object</param>
        /// <param name="messageOffset">Offset of the message</param>
        /// <returns></returns>
        public static ArraySegment<byte> TamperMessage(ArraySegment<byte> encodedMessage, BufferManager bufferManager,
            int messageOffset)
        {
            XElement xmlMessage;
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(encodedMessage.Array, encodedMessage.Offset, encodedMessage.Count);

                stream.Position = 0;
                xmlMessage = XElement.Load(stream, LoadOptions.PreserveWhitespace);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                XNamespace ns = "http://www.w3.org/2003/05/soap-envelope";

                XElement bodyElement = xmlMessage.Element(ns + "Body");
                bodyElement.SetAttributeValue("Tampered", "true");
                xmlMessage.Save(stream, SaveOptions.DisableFormatting);
                byte[] messageBytes = stream.GetBuffer();
                int messageLength = (int) stream.Position;
                int totalLength = messageLength + messageOffset;
                byte[] totalBytes = bufferManager.TakeBuffer(totalLength);
                Array.Copy(messageBytes, 0, totalBytes, messageOffset, messageLength);
                ArraySegment<byte> byteArray = new ArraySegment<byte>(totalBytes, messageOffset, messageLength);
                return byteArray;
            }
        }

        /// <summary>
        ///     This modifier suspends a running thread for 10 seconds which effectively makes the message expire.
        /// </summary>
        /// <param name="encodedMessage">The untouched message</param>
        /// <param name="bufferManager">Buffer manager object</param>
        /// <param name="messageOffset">Offset of the message</param>
        /// <returns></returns>
        public static ArraySegment<byte> DelaySendingForValidTimeStampTestCase(ArraySegment<byte> encodedMessage, BufferManager bufferManager,
            int messageOffset)
        {
            Thread.Sleep(10000); // sleep 10 seconds

            return encodedMessage;
        }

        /// <summary>
        ///     This modifier suspends a running thread for 5 mins + 2 seconds which effectively makes the message expire.
        /// </summary>
        /// <param name="encodedMessage">The untouched message</param>
        /// <param name="bufferManager">Buffer manager object</param>
        /// <param name="messageOffset">Offset of the message</param>
        /// <returns></returns>
        public static ArraySegment<byte> DelaySendingForExpireTestCase(ArraySegment<byte> encodedMessage, BufferManager bufferManager,
            int messageOffset)
        {
            Thread.Sleep(302000); // sleep 5 mins + 2 seconds

            return encodedMessage;
        }
    }
}