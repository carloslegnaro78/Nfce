using System;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Linq;

namespace Nfce
{
    class Program
    {
        static void Main(string[] args)
        {                

            //  Entrada: Link / Dados do QRCode, Parametro Tipo

            //  Saída Json com  Dados da NFCE

            string html = string.Empty;

            string strQRCode;

            Console.WriteLine("Informe o QRCode");

            strQRCode = Console.ReadLine();            

            string url = "https://www.nfce.fazenda.sp.gov.br/NFCeConsultaPublica/Paginas/ConsultaQRCode.aspx?p=" + strQRCode;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.UserAgent = "C# console client";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(@html);

            var nodesDiv = doc.DocumentNode.Descendants("div").Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "txtCenter").ToList();

            int countDiv = 0;

            foreach (var node in nodesDiv)
            {
                string value = node.InnerText;

                string[] lines = value.Split(new string[] { "\r\n", "\t\t\t\t\t\t\t\t" }, StringSplitOptions.None);

                string[] cabecalho = new string[2];

                if (countDiv == 0)
                {

                    cabecalho[0] = "nome";
                    cabecalho[1] = lines[1].Trim();

                    var json = JsonConvert.SerializeObject(cabecalho);

                    Console.WriteLine(json);
                }
                countDiv++;

            }

            foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
            {

                foreach (HtmlNode row in table.SelectNodes("tr"))
                {
                    int count = 0;

                    foreach (HtmlNode cell in row.SelectNodes("th|td"))
                    {

                        string value = cell.InnerText;

                        string[] lines = value.Split(new string[] { "\r\n", "\t\t\t\t\t\t\t\t\t\t", "\t\t\t\t\t\t\t\t\t", "          " }, StringSplitOptions.None);

                        if (count == 0)
                        {

                            string[] item = new string[8];

                            if (lines[2] != null)
                            {
                                item[0] = "Descricao";
                                item[1] = lines[2];
                            }

                            if (lines[6] != null)
                                item[2] = lines[6].Replace("(", "");

                            if (lines[8] != null)
                                item[3] = lines[8];

                            if (lines[22] != null)
                            {
                                item[4] = "Unidade";
                                item[5] = lines[22].Substring(6, 2).ToUpper();
                            }

                            if (lines[30] != null)
                            {
                                item[6] = "vlUnit";
                                item[7] = lines[30];
                            }

                            var json = JsonConvert.SerializeObject(item);

                            Console.WriteLine(json);

                        }
                        count++;
                    }
                }
            }

            Console.Read();
        }
    }
}
