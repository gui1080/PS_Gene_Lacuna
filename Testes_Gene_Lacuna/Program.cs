// Guilherme Braga
// https://www.linkedin.com/in/gui8600k/
// Cópia no Github -> https://github.com/gui1080/Testes_Gene_Lacuna
// Julho/Agosto 2022

using System;
using System.Text;
using RestSharp;
using System.Linq;
using System.Threading.Channels;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program
{
    // corpo do POST com meu usuário e senha
    public class post_autenticador
    {
        public string username { get; set; }

        public string password { get; set; }

    }

    public class post_resposta_check_gene
    {
        public bool isActivated { get; set; }

    }

    public class post_resposta_encode
    {
        public string strandEncoded { get; set; }

    }

    public class post_resposta_decode
    {
        public string strand { get; set; }

    }

    static void Main()
    {

        // realizando meu login, pegando a token de acesso
        // antes disso, a criação do meu usuário e outros testes foram feitos
        // no Imsomnia, para debug. 
        //------------------------------------------------------------

        //base url = https://gene.lacuna.cc/

        Console.WriteLine("Começando chamadas API! Pegando Acess Key com meu usuário e senha.");

        string autenticador_url = "https://gene.lacuna.cc/api/users/login";

        var autenticador_client = new RestClient(autenticador_url);

        var autenticador_request = new RestRequest();

        // observação -> vale a pena constatar que deixar usuário e senha assim abertos em código é uma prática terrível
        // que jamais deve ser reproduzida em aplicação real
        var autenticador_body = new post_autenticador { username= "gbragap", password= "alanturing123" };

        autenticador_request.AddJsonBody(autenticador_body);

        var autenticador_resposta = autenticador_client.Post(autenticador_request);

        string token = "erro";

        if (autenticador_resposta != null)
        {
            dynamic resposta_parsed = JsonConvert.DeserializeObject(autenticador_resposta.Content);

            token = resposta_parsed.accessToken;

            Console.Write("\n");
            Console.WriteLine(token);
            Console.Write("\n");
        }

        // pedindo o job
        //------------------------------------------------------------

        Console.WriteLine("Requisitando trabalho!");

        string job_request_url = "https://gene.lacuna.cc/api/dna/jobs";

        var job_client = new RestClient(job_request_url);

        var job_request = new RestRequest();

        job_request.AddHeader("Authorization", token);

        var job_resposta = job_client.Get(job_request);

        dynamic job_resposta_parsed = JsonConvert.DeserializeObject(job_resposta.Content);

        string result_requisicao = job_resposta_parsed.code;

        if (result_requisicao.Equals("Success"))
        {
            Console.Write("\n");
            Console.WriteLine("A resposta da requisição demonstra sucesso!");
            Console.Write("\n");
        }
        else
        {
            Console.Write("\n");
            Console.WriteLine("A resposta da requisição demonstra erro!");
            Console.Write("\n");
        }

        // separando os dados importantes para os pedidos

        dynamic conteudo_job = job_resposta_parsed.job;

        string job_id = conteudo_job.id;

        string job_type = conteudo_job.type;

        Console.WriteLine("------------------------------------------------------------");

        Console.Write("\n");
        Console.WriteLine("Job recuperado");
        Console.Write("\n");

        Console.WriteLine(job_type);

        Console.WriteLine("------------------------------------------------------------");

        // DECODE
        //------------------------------------------------------------

        if (job_type.Equals("DecodeStrand")) {


            Console.WriteLine("Realizando DecodeStrand");

            // pegar da resposta
            // strandEncoded

            string input_base64;
            string feedback_bin;

            input_base64 = conteudo_job.strandEncoded;

            Console.WriteLine("Input");
            Console.WriteLine(input_base64);

            feedback_bin = Decode(input_base64);

            Console.WriteLine("Output");
            Console.WriteLine(feedback_bin);

            Console.WriteLine("------------------------------------------------------------");

            // responder HTTP
            // ...

            //
            // URL Parameters -> job id
            // header -> auth token
            // body -> strand

            // criar umas variáveis que eu possa usar em todas as respostas
            // url = base + /api/dna/jobs/{job_id}/gene'
            // já fiz classes para o body das respostas

            Console.WriteLine("Começando chamadas API! Retornando resposta gerada.");

            string resposta_url = "https://gene.lacuna.cc/api/dna/jobs/" + job_id + "/decode";

            Console.WriteLine(resposta_url);

            var resposta_client = new RestClient(resposta_url);

            var resposta_request = new RestRequest();

            resposta_request.AddHeader("Authorization", token);

            var post_resposta_decode_body = new post_resposta_decode { strand = "0b" + feedback_bin };

            resposta_request.AddJsonBody(post_resposta_decode_body);

            var resposta_post = resposta_client.Post(resposta_request);

            if(resposta_post != null){

                dynamic resposta_post_parsed = JsonConvert.DeserializeObject(resposta_post.Content);

                string result_resposta_post = resposta_post_parsed.code;
                string mensagem_resposta_post = resposta_post_parsed.message;


                Console.WriteLine("Resposta!");
                Console.WriteLine(result_resposta_post);
                Console.WriteLine(mensagem_resposta_post);
            }

        }

        // ENCODE
        //------------------------------------------------------------

        if (job_type.Equals("EncodeStrand"))
        {

            Console.WriteLine("Realizando EncodeStrand");

            // pegar da resposta
            // strand

            string input_string;
            string feedback_bits;

            input_string = conteudo_job.strand;
            
            Console.WriteLine("Input");
            Console.WriteLine(input_string);

            feedback_bits = Encode(input_string);

            Console.WriteLine("Output");
            Console.WriteLine(feedback_bits);

            Console.WriteLine("------------------------------------------------------------");

            // responder HTTP
            // ...

            //
            // URL Parameters -> job id
            // header -> auth token
            // body -> strandEncoded


            Console.WriteLine("Começando chamadas API! Retornando resposta gerada.");

            string resposta_url = "https://gene.lacuna.cc/api/dna/jobs/" + job_id + "/encode";

            Console.WriteLine(resposta_url);

            var resposta_client = new RestClient(resposta_url);

            var resposta_request = new RestRequest();

            resposta_request.AddHeader("Authorization", token);

            var post_resposta_encode_body = new post_resposta_encode { strandEncoded = feedback_bits };

            resposta_request.AddJsonBody(post_resposta_encode_body);

            // request failed with status bad request!!!
            var resposta_post = resposta_client.Post(resposta_request);

            if (resposta_post != null)
            {

                dynamic resposta_post_parsed = JsonConvert.DeserializeObject(resposta_post.Content);

                string result_resposta_post = resposta_post_parsed.code;
                string mensagem_resposta_post = resposta_post_parsed.message;

                Console.WriteLine("Resposta!");
                Console.WriteLine(result_resposta_post);
                Console.WriteLine(mensagem_resposta_post);


            }

        }

        // CHECK GENE
        //------------------------------------------------------------

        if (job_type.Equals("CheckGene"))
        {

            Console.WriteLine("Realizando CheckGene");

            // pegar da resposta
            // strandEncoded
            // geneEncoded

            string dna_strand;
            string gene;
            string dna_strand_bin;
            string gene_bin;

            // vou passar tudo pra binário pra ficar mais preciso
            // encontrei erros comparando as strings em base de 64
            // algo que seria mais otimizado seria agrupar de 2 em 2 bits com nomenclatura "ATCG",
            // o resultado seria comparação de strings menores. Vou comparar bit com bit para fazer d reúso de código
            dna_strand = conteudo_job.strandEncoded;
            gene = conteudo_job.geneEncoded; 

            gene_bin = Decode(gene);
            dna_strand_bin = Decode(dna_strand);


            // acho que tem que dar decode antes, tá faltando isso

            Console.WriteLine("DNA Strand");
            Console.WriteLine(dna_strand_bin);

            Console.WriteLine("Gene");
            Console.WriteLine(gene_bin);

            bool resultado;

            resultado = CheckActivatedGene(gene_bin, dna_strand_bin);

            Console.WriteLine("Output");
            Console.WriteLine(resultado);

            // responder HTTP
            // ...

            //
            // URL Parameters -> job id
            // header -> auth token
            // body -> is bool

            Console.WriteLine("Começando chamadas API! Retornando resposta gerada.");

            string resposta_url = "https://gene.lacuna.cc/api/dna/jobs/" + job_id + "/gene";

            Console.WriteLine(resposta_url);

            var resposta_client = new RestClient(resposta_url);

            var resposta_request = new RestRequest();

            resposta_request.AddHeader("Authorization", token);

            var post_resposta_decode_body = new post_resposta_check_gene { isActivated = resultado };

            resposta_request.AddJsonBody(post_resposta_decode_body);

            var resposta_post = resposta_client.Post(resposta_request);

            if (resposta_post != null)
            {

                dynamic resposta_post_parsed = JsonConvert.DeserializeObject(resposta_post.Content);

                string result_resposta_post = resposta_post_parsed.code;
                string mensagem_resposta_post = resposta_post_parsed.message;


                Console.WriteLine("Resposta!");
                Console.WriteLine(result_resposta_post);
                Console.WriteLine(mensagem_resposta_post);
            }

        }

        Console.WriteLine("Fim de execução!");

    }

    // falta debugar
    public static string Decode(string input_string)
    {
        // passa de base64 para string de bytes
        byte[] data = Convert.FromBase64String(input_string);
        string stringByte_parcial = BitConverter.ToString(data);

        string stringByte = stringByte_parcial.Replace("-", "");

        string final = ""; 
        string result_parcial; 

        // passa para numero (string base 2)
        for (int i = 0; i < data.Length; i++)
        {

            var v = data[i];
            
            result_parcial = Convert.ToString(v, 2);
         
            // faz o processo ao longo da string toda, dando append numa var de resultado
            int result_parcial_size = 0;
            foreach (char ch in result_parcial)
            {
                result_parcial_size++;
            }

            
            while(result_parcial_size < 8)
            {
                result_parcial = "0" + result_parcial;
                result_parcial_size++;
            }

            final = result_parcial + final;

        }

        
        int j = 0;
        result_parcial = "";
        string result_append;
        var result_string = new System.Text.StringBuilder();

        foreach (char ch in final)
        {
            if(j == 0)
            {
                j = 1;
                result_parcial = "" + ch; 
            }
            else
            {
                j = 0; 
                result_parcial = ch + result_parcial;

                if (result_parcial == "00")
                {
                    result_append = "A";
                    result_string.Insert(0, result_append);
                }
                if (result_parcial == "01")
                {
                    result_append = "C";
                    result_string.Insert(0, result_append);
                }
                if (result_parcial == "11")
                {
                    result_append = "T";
                    result_string.Insert(0, result_append);
                }
                if (result_parcial == "10")
                {
                    result_append = "G";
                    result_string.Insert(0, result_append);
                }

            }

        }

        System.String final_string = result_string.ToString();

        return final_string;
    }

    // passar para base 64 (array de bytes pra entrar na função)
    public static string Encode(string input_letras)
    {
        var result_binario = new System.Text.StringBuilder();
        string result_append;

        // quebra a string em letras
        // compara uma a uma e dá append no equivalente numa variável de resultado
        foreach (char ch in input_letras)
        {
            // A: 0b00      C: 0b01
            // T: 0b11      G: 0b10
            if(ch == 'A')
            {
                result_append = "00";
                result_binario.Insert(0, result_append);
            }
            if (ch == 'C')
            {
                result_append = "01";
                result_binario.Insert(0, result_append);
            }
            if (ch == 'T')
            {
                result_append = "11";
                result_binario.Insert(0, result_append);
            }
            if (ch == 'G')
            {
                result_append = "10";
                result_binario.Insert(0, result_append);
            }
        }

        System.String final = result_binario.ToString();

        string parcial = "";
        var list = new List<byte>();
        byte byte_parcial = 0; 

        foreach (char ch in final)
        {
            if(parcial.Length == 8)
            {
                // em "parcial" estão os 8 bits a virarem uma entrada no array
                byte_parcial = Convert.ToByte(parcial, 2);
                list.Insert(0, byte_parcial);
                parcial = "";
               
            }
            else
            {
                // pega mais uma posição e incrementa k
                parcial = ch + parcial;

            }
        }

        var array_bytes = list.ToArray();
        string final_base64 = Convert.ToBase64String(array_bytes, 0, array_bytes.Length);

        return final_base64;
    }

    public static bool CheckActivatedGene(string gene, string dna_strand)
    {
     
        System.String current_gene;

        // checa o tamanho das variáveis

        int half_gene = 0;
        int slice_beginning = 0;
        int slice_end = 0;

        var result = true; 

        int gene_size = 0;
        foreach (char ch in gene)
        {
            gene_size++;
        }

        int dna_strand_size = 0;
        foreach (char ch in dna_strand)
        {
            dna_strand_size++;
        }

        // arredondado pra cima, pra sempre compararmos mais de 50%!
        // se tem mais de 50%, tem pelo menos 50%
        // só vai ser comparado pedaços de strings desse tamanho
        half_gene = gene_size / 2 + (gene_size % 2 > 0 ? 1 : 0);

        // cortar a string de 0 até o meio, e ir aumentando
        slice_end = half_gene;

        while (slice_end < dna_strand_size)
        {
            current_gene = gene.Slice(slice_beginning, slice_end);
            
            // checa se string maior contem a menor
            if (dna_strand.Contains(current_gene) == true)
            {

                result = true;
                
                break;
            }
            // se não contem, pega proxima string de tamanho definido!
            else
            {
                slice_beginning++;
                slice_end++;
                if (slice_end == gene_size)
                {
                    result = false;
                    break;
                }
            }
        }

        Console.Write("\n\nResultado encontrado dentro da função!\n\n");
        Console.Write(result);
        Console.Write("\n\n");
        return result;
    }
}

public static class Extensions
{
    // fonte: TheDeveloperBlog.com
    // retorna string "cortada" enter índice x e y
    public static string Slice(this string source, int start, int end)
    {
        if (end < 0) 
        {
            end = source.Length + end;
        }
        int len = end - start;               
        return source.Substring(start, len); 
    }
}
