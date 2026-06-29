using System;
using System.Linq;
using System.Data.Entity;
using DA;

namespace TestDB {
    class Program {
        static void Main() {
            using (var db = new MyEntities()) {
                var count = db.TB_TRANSLATIONS.Count(x => x.TABLE_NAME == ""UI_LABEL"");
                Console.WriteLine($""UI_LABEL Count: {count}"");
            }
        }
    }
}
