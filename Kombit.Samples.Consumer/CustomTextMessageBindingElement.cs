using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;

namespace Kombit.Samples.Consumer
{
    /// <summary>
    ///     A custom MessageBindingElement that wraps the TextMessageEncodingBindingElement and provides an extensible point to
    ///     create
    ///     custom MessageEncoderFactory
    /// </summary>
    public sealed class CustomTextMessageBindingElement : MessageEncodingBindingElement, IWsdlExportExtension
    {
        private readonly Func<MessageEncoderFactory, MessageEncoderFactory> messageEncoderFactoryFunc;
        private readonly TextMessageEncodingBindingElement wrappedMessageEncodingBindingElement;

        /// <summary>
        ///     Instantiates a new instance of the CustomTextMessageBindingElement class
        /// </summary>
        /// <param name="messageEncoderFactoryFunc"></param>
        public CustomTextMessageBindingElement(
            Func<MessageEncoderFactory, MessageEncoderFactory> messageEncoderFactoryFunc)
        {
            if (messageEncoderFactoryFunc == null)
                throw new ArgumentNullException("messageEncoderFactoryFunc");

            this.messageEncoderFactoryFunc = messageEncoderFactoryFunc;
            wrappedMessageEncodingBindingElement = new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8);
        }

        /// <summary>
        ///     Message version
        /// </summary>
        public override MessageVersion MessageVersion
        {
            get { return wrappedMessageEncodingBindingElement.MessageVersion; }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                wrappedMessageEncodingBindingElement.MessageVersion = value;
            }
        }

        #region IMessageEncodingBindingElement Members

        /// <summary>
        ///     Creates a MessageEncoderFactory. Usually it is a custom factory which wraps a built-in factory
        /// </summary>
        /// <returns></returns>
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            //return new CustomTextMessageEncoderFactory(wrappedMessageEncodingBindingElement.CreateMessageEncoderFactory());
            return messageEncoderFactoryFunc(wrappedMessageEncodingBindingElement.CreateMessageEncoderFactory());
        }

        #endregion

        /// <summary>
        ///     Clone the binding element
        /// </summary>
        /// <returns></returns>
        public override BindingElement Clone()
        {
            return new CustomTextMessageBindingElement(messageEncoderFactoryFunc);
        }

        /// <summary>
        ///     Build channel factory to communicate with sts
        /// </summary>
        /// <typeparam name="TChannel"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        /// <summary>
        ///     To specify whethere it is able to build channel factory
        /// </summary>
        /// <typeparam name="TChannel"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        /// <summary>
        ///     Build channel listener
        /// </summary>
        /// <typeparam name="TChannel"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        /// <summary>
        ///     Specify whethere it is able to build channel listener
        /// </summary>
        /// <typeparam name="TChannel"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        /// <summary>
        ///     Get property of a binding context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public override T GetProperty<T>(BindingContext context)
        {
            return wrappedMessageEncodingBindingElement.GetProperty<T>(context);
        }

        #region IWsdlExportExtension Members

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            // The MessageEncodingBindingElement is responsible for ensuring that the WSDL has the correct
            // SOAP version. We can delegate to the WCF implementation of TextMessageEncodingBindingElement for this.
            TextMessageEncodingBindingElement mebe = new TextMessageEncodingBindingElement();
            ((IWsdlExportExtension) mebe).ExportEndpoint(exporter, context);
        }

        #endregion
    }
}