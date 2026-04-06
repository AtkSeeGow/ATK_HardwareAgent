namespace HardwareAgent.Domain
{
    /// <summary>
    /// 資料封包
    /// </summary>
    public class DataEnvelope<T>
    {
        /// <summary>
        /// 來源資料類型
        /// </summary>
        public EndpointType Source { get; set; }

        /// <summary>
        /// 目的資料類型
        /// </summary>
        public EndpointType Destination { get; set; }

        /// <summary>
        /// 業務資料
        /// </summary>
        public T Payload { get; set; }

        /// <summary>
        /// 資料類型
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// 補充資訊
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
