namespace DA
{
    using System;
    using System.Collections.Generic;
    
    public partial class TB_USER_DEVICE
    {
        public decimal ID { get; set; }
        public decimal IDUSER { get; set; }
        public Nullable<decimal> MANV { get; set; }
        public string INSTALLATION_ID { get; set; }
        public string EXPO_PUSH_TOKEN { get; set; }
        public string PLATFORM { get; set; }
        public string PROJECT_ID { get; set; }
        public string ENVIRONMENT { get; set; }
        public string LANGUAGE { get; set; }
        public string APP_VERSION { get; set; }
        public string PERMISSION_STATUS { get; set; }
        public bool PUSH_ENABLED { get; set; }
        public bool IS_ACTIVE { get; set; }
        public string DEVICE_NAME { get; set; }
        public System.DateTime CREATED_AT { get; set; }
        public System.DateTime UPDATED_AT { get; set; }
        public System.DateTime LAST_SEEN_AT { get; set; }
    }
}
