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

            //  string url = "https://www.nfce.fazenda.sp.gov.br/NFCeConsultaPublica/Paginas/ConsultaQRCode.aspx?p=35200961412110046651650230000122431881439089|2|1|1|9680b3ada1e70a1af2a24897c28732ea30e88a2f";

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

                string[] cabecalho = new string[10];

                if (countDiv == 0)
                {
                    cabecalho[0] = "NOME";
                    cabecalho[1] = lines[1].Trim();
                    cabecalho[2] = lines[4].Trim().Replace(":", "");
                    cabecalho[3] = lines[6].Trim().Replace(".", "").Replace("/", "").Replace("-", "");
                    cabecalho[4] = "ENDERECO";
                    cabecalho[5] = lines[7].Trim().ToUpper();
                    cabecalho[6] = lines[9].Replace("\t\t\t\t\t\t\t", "");
                    cabecalho[7] = lines[13].Replace("\t\t\t\t\t\t\t", "").ToUpper();
                    cabecalho[8] = lines[15].Replace("\t\t\t\t\t\t\t", "");
                    cabecalho[9] = lines[17].Replace("\t\t\t\t\t\t\t", "");

                    var json1 = JsonConvert.SerializeObject(cabecalho);

                    Console.WriteLine(json1);
                }
                countDiv++;
            }

            int size = 1;

            foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
            {
                
                foreach (HtmlNode row in table.SelectNodes("tr"))
                {
                    ++size;                  
                }
            }
            size--;

            string[,] item = new string[size, 8];
            int sizetr = 0;

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
                            if (lines[2] != null)
                            {
                                item[sizetr, 0] = "DESCRICAO";
                                item[sizetr, 1] = lines[2].ToUpper();
                            }

                            if (lines[6] != null)
                                item[sizetr, 2] = lines[6].Replace("(", "").Replace("ó", "o").ToUpper();

                            if (lines[8] != null)
                                item[sizetr, 3] = lines[8];

                            if (lines[22] != null)
                            {
                                item[sizetr, 4] = "UNIDADE";
                                item[sizetr, 5] = lines[22].Substring(6, 2).ToUpper();
                            }

                            if (lines[30] != null)
                            {
                                item[sizetr, 6] = "VLUNIT";
                                item[sizetr, 7] = lines[30].Replace(",", ".");
                            }
                            sizetr++;
                        }
                        count++;                        
                    }
                }
            }

            var conteudo = JsonConvert.SerializeObject(item);

            Console.WriteLine(conteudo);

            Console.Read();
 
        }
    }
}
