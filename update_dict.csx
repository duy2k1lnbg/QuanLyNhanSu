
using System;
using System.IO;

string path = @"D:\QL_NS\QuanLyNhanSu\QLyNSu\Functions\TranslationManager.cs";
string content = File.ReadAllText(path);

string injection = @"                    _dictionary[""HRM SYSTEM""] = new TranslationValues { English = ""HRM SYSTEM"", Japanese = ""HRMシステム"", Chinese = ""人力资源管理系统"", Korean = ""HRM 시스템"" };
                    _dictionary[""Enterprise Management Platform""] = new TranslationValues { English = ""Enterprise Management Platform"", Japanese = ""企業管理プラットフォーム"", Chinese = ""企业管理平台"", Korean = ""기업 관리 플랫폼"" };
                    _dictionary[""Phiên bản v3.5.0""] = new TranslationValues { English = ""Version v3.5.0"", Japanese = ""バージョン v3.5.0"", Chinese = ""版本 v3.5.0"", Korean = ""버전 v3.5.0"" };
                    _dictionary[""⯌   Local AI Assistant Integrated\r\n⯌   Oracle 19c Enterprise DB\r\n⯌    High-Security Architecture""] = new TranslationValues { English = ""⯌   Local AI Assistant Integrated\r\n⯌   Oracle 19c Enterprise DB\r\n⯌    High-Security Architecture"", Japanese = ""⯌   ローカルAIアシスタント統合\r\n⯌   Oracle 19c エンタープライズDB\r\n⯌    高セキュリティアーキテクチャ"", Chinese = ""⯌   集成原生AI助手\r\n⯌   Oracle 19c 企业级数据库\r\n⯌    高安全架构"", Korean = ""⯌   로컬 AI 어시스턴트 통합\r\n⯌   Oracle 19c 엔터프라이즈 DB\r\n⯌    고보안 아키텍처"" };
                    _dictionary[""Ngôn Ngữ Hệ Thống:""] = new TranslationValues { English = ""System Language:"", Japanese = ""システム言語:"", Chinese = ""系统语言:"", Korean = ""시스템 언어:"" };
";

content = content.Replace("_dictionary = newDict;", "_dictionary = newDict;\r\n\r\n" + injection);
File.WriteAllText(path, content, System.Text.Encoding.UTF8);
Console.WriteLine("Done");

