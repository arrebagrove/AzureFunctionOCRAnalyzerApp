using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.Vision.Contract;
using Microsoft.ProjectOxford.Vision;
using System.Linq;
using System.Collections.Generic;

namespace AnalyzerApp
{
    public class Cupom
    {
        public string NomeGatewayPagamento { get; set; }
        public string Via { get; set; }
        public string TipoPagamento { get; set; }
        public string Arquivo { get; set; }
        public string Valor { get; set; }
    }

    public static class OCRAnalyzer
    {
        [FunctionName("BlobTriggerCSharp")]        
        public static Cupom Run([BlobTrigger("cupom/{name}", Connection = "AzureStorageConnection")]Stream myBlob, string name, TraceWriter log)
        {
            var client = new VisionServiceClient("abc468301e5e43948d08aa5a7ae96db7",
                "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr?");

            OcrResults result = client.RecognizeTextAsync(myBlob).Result;

            var words = (from r in result.Regions
                         from l in r.Lines
                         from w in l.Words
                         select w.Text).ToList();

            switch (words[0])
            {
                case "stone":
                    return MontarDadosCupomStone(words, name);
                case "getnee":
                    return MontarDadosCupomGenet(words, name);
                default:
                    return new Cupom
                    {
                        NomeGatewayPagamento = "Desconhecido",
                        Via = "Desconhecida",
                        TipoPagamento = "Desconhecido",
                        Valor = "Desconhecido",
                        Arquivo = name
                    };
            }
        }

        public static Cupom MontarDadosCupomStone(List<string> dadosCupom, string nomeArquivo)
        {
            return new Cupom
            {
                NomeGatewayPagamento = dadosCupom[0],
                Via = dadosCupom[5],
                TipoPagamento = dadosCupom[8],
                Valor = dadosCupom[30],
                Arquivo = nomeArquivo
            };
        }

        public static Cupom MontarDadosCupomGenet(List<string> dadosCupom, string nomeArquivo)
        {
            return new Cupom
            {
                NomeGatewayPagamento = dadosCupom[0],
                Via = dadosCupom[2],
                TipoPagamento = dadosCupom[25],
                Valor = dadosCupom[27],
                Arquivo = nomeArquivo
            };
        }
    }
}