namespace DA
{
    using System;
    using System.Collections.Generic;

    public partial class TB_PUSH_OUTBOX
    {
        public decimal ID { get; set; }
        public decimal NOTIFICATION_ID { get; set; }
        public decimal DEVICE_ID { get; set; }
        public decimal IDUSER { get; set; }
        public string INSTALLATION_ID { get; set; }
        public string EXPO_PUSH_TOKEN { get; set; }
        public string TITLE { get; set; }
        public string BODY { get; set; }
        public string DATA_JSON { get; set; }
        public string STATUS { get; set; }
        public decimal RETRY_COUNT { get; set; }
        public decimal MAX_RETRIES { get; set; }
        public System.DateTime NEXT_RETRY_AT { get; set; }
        public Nullable<System.DateTime> LOCKED_UNTIL { get; set; }
        public string LOCKED_BY { get; set; }
        public string TICKET_ID { get; set; }
        public string TICKET_STATUS { get; set; }
        public string TICKET_ERROR { get; set; }
        public string RECEIPT_STATUS { get; set; }
        public string RECEIPT_ERROR { get; set; }
        public Nullable<System.DateTime> RECEIPT_CHECKED_AT { get; set; }
        public string LAST_ERROR { get; set; }
        public string IDEMPOTENCY_KEY { get; set; }
        public System.DateTime CREATED_AT { get; set; }
        public Nullable<System.DateTime> SENT_AT { get; set; }
    }
}
