namespace NServiceBus.PowerShell
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Messaging;
    using System.Xml;
    using System.Xml.Serialization;

    [Cmdlet(VerbsCommon.Get, "NServiceBusMSMQMessage")]
    public class GetMessage : PSCmdlet
    {
        [Parameter(HelpMessage = "The name of the private queue to search", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string QueueName { get; set; }

        protected override void ProcessRecord()
        {
            var queueAddress = string.Format("FormatName:DIRECT=OS:{0}\\private$\\{1}", Environment.MachineName,QueueName);

            
            var queue = new MessageQueue(queueAddress);
            var messageReadPropertyFilter = new MessagePropertyFilter {Id = true, Extension = true, ArrivedTime = true};

            queue.MessageReadPropertyFilter = messageReadPropertyFilter;

            foreach (var message in queue.GetAllMessages())
            {
                var o = new
                {
                    message.Id,
                    Headers = ParseHeaders(message),
                    message.ArrivedTime
                };
                WriteObject(o, true);
            }
        }

        IEnumerable<HeaderInfo> ParseHeaders(Message message)
        {

            IEnumerable<HeaderInfo> result = new List<HeaderInfo>();
            
            if(message.Extension.Length > 0)
            {
                using (var stream = new MemoryStream(message.Extension))
                using (var reader = XmlReader.Create(stream, new XmlReaderSettings { CheckCharacters = false }))

                {
                    result = headerSerializer.Deserialize(reader) as IEnumerable<HeaderInfo>;
                }
            }

            return result;
        }

        private static readonly XmlSerializer headerSerializer = new XmlSerializer(typeof(List<HeaderInfo>));
    }
}