using System;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace Nfce
{
    class Program
    {
        public class Estabelecimento
        {
            public string Nome { get; set; }
            public string Logradouro { get; set; }
            public string Bairro { get; set; }
            public string Cidade { get; set; }
            public string Uf { get; set; }
            public string Cnpj { get; set; }

            public Estabelecimento(string nome, string logradouro, string bairro, string cidade, string uf, string cnpj)
            {
                Nome = nome;
                Logradouro = logradouro;
                Bairro = bairro;
                Cidade = cidade;
                Uf = uf;
                Cnpj = cnpj;
            }
        }

        public class Item
        {

            public string Cod { get; set; }

            public string Descricao { get; set; }

            public string Unidade { get; set; }

            public string Vlunit { get; set; }

            public Item(string cod, string descricao, string unidade, string vlunit)
            {
                this.Cod = cod;
                this.Descricao = descricao;
                this.Unidade = unidade;
                this.Vlunit = vlunit;
            }

        }

        static void Main(string[] args)
        {

            //  Entrada: Link / Dados do QRCode, Parametro Tipo

            //  Saída Json com  Dados da NFCE

            string html = string.Empty;

            string strQRCode;

            Console.WriteLine("Informe o QRCode");

            strQRCode = Console.ReadLine();

            //string url = "https://www.nfce.fazenda.sp.gov.br/NFCeConsultaPublica/Paginas/ConsultaQRCode.aspx?p=35200961412110046651650230000122431881439089|2|1|1|9680b3ada1e70a1af2a24897c28732ea30e88a2f";

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

                if (countDiv == 0)
                {
                    Estabelecimento estabelecimento = new Estabelecimento(lines[1].Trim(), lines[7].Trim().ToUpper(), lines[13].Replace("\t\t\t\t\t\t\t", "").ToUpper(), lines[15].Replace("\t\t\t\t\t\t\t", "").ToUpper(), lines[17].Replace("\t\t\t\t\t\t\t", "").ToUpper(), lines[6].Trim().Replace(".", "").Replace("/", "").Replace("-", ""));

                    var cabecalho = JsonConvert.SerializeObject(estabelecimento);

                    Console.WriteLine(cabecalho);
                }
                countDiv++;
            }

            List<Item> Itens = new List<Item>();

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
                            Itens.Add(new Item(lines[8], lines[2].ToUpper(), lines[22].Substring(6, 2).ToUpper(), lines[30].Replace(",", ".")));
                        }
                        count++;
                    }
                }
            }

            var conteudo = JsonConvert.SerializeObject(Itens);

            Console.WriteLine(conteudo);

        }
    }
}
