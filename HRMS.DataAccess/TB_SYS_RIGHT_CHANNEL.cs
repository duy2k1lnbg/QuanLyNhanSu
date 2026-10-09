namespace DA
{
    using System;
    using System.Collections.Generic;

    public partial class TB_SYS_RIGHT_CHANNEL
    {
        public decimal IDUSER { get; set; }
        public string CLIENT_TYPE { get; set; }
        public string FUNCTION_CODE { get; set; }
        public decimal CAN_VIEW { get; set; }
        public decimal CAN_ADD { get; set; }
        public decimal CAN_EDIT { get; set; }
        public decimal CAN_DELETE { get; set; }
        public decimal CAN_PRINT { get; set; }
        public Nullable<System.DateTime> CREATED_AT { get; set; }
        public Nullable<System.DateTime> UPDATED_AT { get; set; }
    }
}
