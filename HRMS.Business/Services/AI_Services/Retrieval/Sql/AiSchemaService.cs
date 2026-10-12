using System;
using Bu.Services.AI_Services.Retrieval.Sql;

namespace Bu.Services.AI_Services
{
    /// <summary>
    /// Facade cung cấp tài liệu schema cho AI pipeline.
    /// Toàn bộ định nghĩa schema đã được typed hóa tập trung tại AiSchemaCatalog.
    /// </summary>
    public class AiSchemaService
    {
        public string GetFullSchema()
        {
            return AiSchemaCatalog.GenerateFullSchemaDocumentation();
        }
    }
}