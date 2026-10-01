using System;
using System.Linq;
using NUnit.Framework;
class PureRunner {
 static int Main() {
  int pass=0,fail=0;
  foreach(var type in new[]{typeof(HRMS.Tests.TimeSegmentationEngineTests),typeof(HRMS.Tests.AttendanceRegressionTests)}) foreach(var m in type.GetMethods().Where(x=>x.IsDefined(typeof(TestAttribute),false))) {
   var f=Activator.CreateInstance(type);if(f is HRMS.Tests.TimeSegmentationEngineTests) ((HRMS.Tests.TimeSegmentationEngineTests)f).Setup();
   try{m.Invoke(f,null);Console.WriteLine("PASS "+m.Name);pass++;}
   catch(Exception e){Console.WriteLine("FAIL "+m.Name+": "+(e.InnerException??e).Message);fail++;}
  }
  Console.WriteLine("RESULT "+pass+" passed, "+fail+" failed");return fail==0?0:1;
 }
}