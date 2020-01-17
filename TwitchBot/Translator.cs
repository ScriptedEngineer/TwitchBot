using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public static class Translator
    {
        static HttpClient Web = new HttpClient();
        static Dictionary<string, string> Langs = new Dictionary<string, string>
        {
            {"азербайджанский","az"},
            {"малаялам","ml"},
            {"албанский","sq"},
            {"мальтийский","mt"},
            {"амхарский","am"},
            {"македонский","mk"},
            {"английский","en"},
            {"маори","mi"},
            {"арабский","ar"},
            {"маратхи","mr"},
            {"армянский","hy"},
            {"марийский","mhr"},
            {"африкаанс","af"},
            {"монгольский","mn"},
            {"баскский","eu"},
            {"немецкий","de"},
            {"башкирский","ba"},
            {"непальский","ne"},
            {"белорусский","be"},
            {"норвежский","no"},
            {"бенгальский","bn"},
            {"панджаби","pa"},
            {"бирманский","my"},
            {"папьяменто","pap"},
            {"болгарский","bg"},
            {"персидский","fa"},
            {"боснийский","bs"},
            {"польский","pl"},
            {"валлийский","cy"},
            {"португальский","pt"},
            {"венгерский","hu"},
            {"румынский","ro"},
            {"вьетнамский","vi"},
            {"русский","ru"},
            {"гаитянский","ht"},
            {"себуанский","ceb"},
            {"галисийский","gl"},
            {"сербский","sr"},
            {"голландский","nl"},
            {"сингальский","si"},
            {"горномарийский","mrj"},
            {"словацкий","sk"},
            {"греческий","el"},
            {"словенский","sl"},
            {"грузинский","ka"},
            {"суахили","sw"},
            {"гуджарати","gu"},
            {"сунданский","su"},
            {"датский","da"},
            {"таджикский","tg"},
            {"иврит","he"},
            {"тайский","th"},
            {"идиш","yi"},
            {"тагальский","tl"},
            {"индонезийский","id"},
            {"тамильский","ta"},
            {"ирландский","ga"},
            {"татарский","tt"},
            {"итальянский","it"},
            {"телугу","te"},
            {"исландский","is"},
            {"турецкий","tr"},
            {"испанский","es"},
            {"удмуртский","udm"},
            {"казахский","kk"},
            {"узбекский","uz"},
            {"каннада","kn"},
            {"украинский","uk"},
            {"каталанский","ca"},
            {"урду","ur"},
            {"киргизский","ky"},
            {"финский","fi"},
            {"китайский","zh"},
            {"французский","fr"},
            {"корейский","ko"},
            {"хинди","hi"},
            {"коса","xh"},
            {"хорватский","hr"},
            {"кхмерский","km"},
            {"чешский","cs"},
            {"лаосский","lo"},
            {"шведский","sv"},
            {"латынь","la"},
            {"шотландский","gd"},
            {"латышский","lv"},
            {"эстонский","et"},
            {"литовский","lt"},
            {"эсперанто","eo"},
            {"люксембургский","lb"},
            {"яванский","jv"},
            {"малагасийский","mg"},
            {"японский","ja"},
            {"малайский","ms"},
            {"afrikaans","af"},
            {"amharic","am"},
            {"arabic","ar"},
            {"azerbaijani","az"},
            {"bashkir","ba"},
            {"belarusian","be"},
            {"bulgarian","bg"},
            {"bengali","bn"},
            {"bosnian","bs"},
            {"catalan","ca"},
            {"cebuano","ceb"},
            {"czech","cs"},
            {"welsh","cy"},
            {"danish","da"},
            {"german","de"},
            {"greek","el"},
            {"english","en"},
            {"esperanto","eo"},
            {"spanish","es"},
            {"estonian","et"},
            {"basque","eu"},
            {"persian","fa"},
            {"finnish","fi"},
            {"french","fr"},
            {"irish","ga"},
            {"scottish gaelic","gd"},
            {"galician","gl"},
            {"gujarati","gu"},
            {"hebrew","he"},
            {"hindi","hi"},
            {"croatian","hr"},
            {"haitian","ht"},
            {"hungarian","hu"},
            {"armenian","hy"},
            {"indonesian","id"},
            {"icelandic","is"},
            {"italian","it"},
            {"japanese","ja"},
            {"javanese","jv"},
            {"georgian","ka"},
            {"kazakh","kk"},
            {"khmer","km"},
            {"kannada","kn"},
            {"korean","ko"},
            {"kyrgyz","ky"},
            {"latin","la"},
            {"luxembourgish","lb"},
            {"lao","lo"},
            {"lithuanian","lt"},
            {"latvian","lv"},
            {"malagasy","mg"},
            {"mari","mhr"},
            {"maori","mi"},
            {"macedonian","mk"},
            {"malayalam","ml"},
            {"mongolian","mn"},
            {"marathi","mr"},
            {"hill mari","mrj"},
            {"malay","ms"},
            {"maltese","mt"},
            {"burmese","my"},
            {"nepali","ne"},
            {"dutch","nl"},
            {"norwegian","no"},
            {"punjabi","pa"},
            {"papiamento","pap"},
            {"polish","pl"},
            {"portuguese","pt"},
            {"romanian","ro"},
            {"russian","ru"},
            {"sinhalese","si"},
            {"slovak","sk"},
            {"slovenian","sl"},
            {"albanian","sq"},
            {"serbian","sr"},
            {"sundanese","su"},
            {"swedish","sv"},
            {"swahili","sw"},
            {"tamil","ta"},
            {"telugu","te"},
            {"tajik","tg"},
            {"thai","th"},
            {"tagalog","tl"},
            {"turkish","tr"},
            {"tatar","tt"},
            {"udmurt","udm"},
            {"ukrainian","uk"},
            {"urdu","ur"},
            {"uzbek","uz"},
            {"vietnamese","vi"},
            {"xhosa","xh"},
            {"yiddish","yi"},
            {"chinese","zh"},
        };
        public static string Translate(string input, string lang)
        {
            if (!Langs.ContainsKey(lang))
                return "Язык, на который вы хотите перевести, не существует в моем словаре.";
            string FiLaTg = GetLangTag(input);
            if(FiLaTg.Length > 4 || string.IsNullOrEmpty(FiLaTg))
                return "Мне не удалось определить язык на котором это написано.";
            return TranslateApi(input, FiLaTg + "-" + Langs[lang]);
        }
        public static string TranslateLT(string input, string lang)
        {
            string FiLaTg = GetLangTag(input);
            if (FiLaTg.Length > 4 || string.IsNullOrEmpty(FiLaTg))
                FiLaTg = "en";
            return TranslateApi(input, FiLaTg + "-" + lang);
        }
        public static string TranslateLV(string input, string lang)
        {
            return TranslateApi(input, lang);
        }
        private static string GetLangTag(string text)
        {
            var values = new Dictionary<string, string>
                {
                { "key", "trnsl.1.1.20171030T220430Z.b3cc6fed3034b5ab.89c978d22c3e764d4ad5080f8e947c90df16da6b" },
                { "text", text }
                };

            var content = new FormUrlEncodedContent(values);

            string lang = Web.PostAsync("https://translate.yandex.net/api/v1.5/tr.json/detect", content).Result.Content.ReadAsStringAsync().Result;
            return lang.Replace("{\"code\":200,\"lang\":\"", "").Replace("\"}","");
        }
        private static string TranslateApi(string text,string langVetcor)
        {
            var values = new Dictionary<string, string>
                {
                { "key", "trnsl.1.1.20171030T220430Z.b3cc6fed3034b5ab.89c978d22c3e764d4ad5080f8e947c90df16da6b" },
                { "text", text },
                { "lang",langVetcor}
                };

            var content = new FormUrlEncodedContent(values);

            string lang = Web.PostAsync("https://translate.yandex.net/api/v1.5/tr.json/translate", content).Result.Content.ReadAsStringAsync().Result;
            return lang.Split(new string[] { "\"text\":[\"" },StringSplitOptions.RemoveEmptyEntries).Last().Replace("\"]}","").Replace("\\r", "").Replace("\\n", "");
        }
        public static string TranslateYa(string input, string lang)
        {
            //https://translate.yandex.net/api/v1/tr.json/translate?id=46833bde.5dd17b27.ab1bb21d-2-0&srv=tr-text&lang=ru-emj&reason=auto&text=%D0%BF%D1%80%D0%B8%D0%B2%D0%B5%D1%82
            var values = new Dictionary<string, string>
                {
                { "text", input },
                { "options", "4" }
                };

            var content = new FormUrlEncodedContent(values);

            string lange = Web.PostAsync("https://translate.yandex.net/api/v1/tr.json/translate?id=46833bde.5dd17b27.ab1bb21d-2-0&srv=tr-text&lang="+ lang + "&reason=auto", content).Result.Content.ReadAsStringAsync().Result;
            return lange.Split(new string[] { "\"text\":[\"" }, StringSplitOptions.RemoveEmptyEntries).Last().Replace("\"]}", "").Replace("\\r", "").Replace("\\n", "");

        }
    }
}
