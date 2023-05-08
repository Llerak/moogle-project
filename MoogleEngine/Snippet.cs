using System.Diagnostics;
namespace MoogleEngine;
public class Snippet
{
    public Snippet(Query Consulta, Dictionary<string, float> DocumentosPuntuados)
    {
        Stopwatch time = new Stopwatch();
        time.Start();

        this.DocumentosSnippet = new Dictionary<string,string>();
        this.SubArrayDocumento(Consulta, DocumentosPuntuados);
        
        time.Stop();
        Console.WriteLine("Snnipet: " + time.Elapsed.TotalMilliseconds / 1000);
    }

    public Dictionary<string,string> DocumentosSnippet{get; private set;}
    private void SubArrayDocumento(Query Consulta, Dictionary<string, float> DocumentosPuntuados)
    {
        this.DocumentosSnippet = ProcesarDocumento(Consulta, DocumentosPuntuados);    
    }

    private static Dictionary<string, string> ProcesarDocumento(Query Consulta, Dictionary<string, float> DocumentosPuntuados)
    {
        Dictionary<string, string> Snippet = new Dictionary<string, string>();
        
        foreach (string Documento in DocumentosPuntuados.Keys)
        {
            Snippet.Add(Documento, SnippetDocumento(DividirDocumento(Documento), Consulta));
        }
        return Snippet;
    }

    private static Dictionary<string, Dictionary<string,float>> DividirDocumento(string Documento)
    {
        Dictionary<string, Dictionary<string,float>> DocumentoDividido = new Dictionary<string, Dictionary<string,float>>();        
        
        
        string[] DocumentoArray = File.ReadAllText(Documento).Split(' ',StringSplitOptions.RemoveEmptyEntries);
        
        int Contador = 0;
        string Parte ="";

        for (int i = 0; i < DocumentoArray.Length; i++)
        {
            Parte += DocumentoArray[i] + " ";
            Contador++;
            if(Contador == 74)
            {
                Contador = 0;
                Dictionary<string,float> ParteDocumento = new Dictionary<string, float>();
                
                DocumentoDividido.Add(Parte, ParteDocumento);                
                Parte = "";
            }
        }
        return DocumentoDividido;
    }

    private static string SnippetDocumento(Dictionary<string, Dictionary<string,float>> ListaDocumento, Query Consulta)
    {
        Dictionary<string, float> UniversoDocumento = new Dictionary<string, float>();

        foreach (string Documento in ListaDocumento.Keys)
        {            
            
            string[] PalabrasDoc =  ProcessedDocuments.Proceso(Documento);

            //palabras del documento con su respectivo peso
            Dictionary<string, float> PalabrasDocTF_IDF = new Dictionary<string, float>();

            foreach (string Palabra in PalabrasDoc)
            {
                if (!UniversoDocumento.ContainsKey(Palabra))
                    UniversoDocumento.Add(Palabra, 0);

                if (!PalabrasDocTF_IDF.ContainsKey(Palabra))
                {
                    PalabrasDocTF_IDF.Add(Palabra,1);
                    UniversoDocumento[Palabra]++;
                }
                else 
                    PalabrasDocTF_IDF[Palabra]++;
            }

            // Computar el la frecuencia del termino TF
            foreach (string Palabra in PalabrasDocTF_IDF.Keys)
                PalabrasDocTF_IDF[Palabra] = PalabrasDocTF_IDF[Palabra] / PalabrasDoc.Length;
            
            ListaDocumento[Documento] = PalabrasDocTF_IDF;
        }   

        foreach (string Palabra in UniversoDocumento.Keys)
            UniversoDocumento[Palabra] = (float)( Math.Log10( ListaDocumento.Count/ UniversoDocumento[Palabra] ) );

        foreach (var Documento in ListaDocumento.Values)
        {
            foreach (string Palabra in Documento.Keys)
                Documento[Palabra] = Documento[Palabra] * UniversoDocumento[Palabra];
        }
        
        return PuntuacionDocumentos(Consulta, UniversoDocumento, ListaDocumento);
    }

    private static string PuntuacionDocumentos(Query Consulta,Dictionary<string, float> UniversoPalabras, Dictionary<string, Dictionary<string, float>> MatrizDocumentos)
    {
        Dictionary<string, float> DocumentosPuntuados = new Dictionary<string, float>();
        
        Dictionary<string, float> ConsultaTFIDF = Consulta.TF_IDFConsulta;
        
        if(ConsultaTFIDF.Count == 0)
            return "";

        float SumatoriaC = CosineSimilarity.SumatoriaDiccionarioCuadrado(ConsultaTFIDF);
        float PromedioScoreFaltante = 0;
        
        foreach (string Documento in MatrizDocumentos.Keys)
        {
            float Score = 0;
            
            float SumatoriaCD = 0;
        
            foreach (string Palabra in ConsultaTFIDF.Keys)
            {
                if(MatrizDocumentos[Documento].ContainsKey(Palabra))
                    SumatoriaCD += MatrizDocumentos[Documento][Palabra] * ConsultaTFIDF[Palabra];
            }
       
            if(SumatoriaCD == 0)
                continue;  
            
            float SumatoriaD = CosineSimilarity.SumatoriaDiccionarioCuadrado(MatrizDocumentos[Documento]);
       
            Score = (SumatoriaCD/((float)(Math.Sqrt(SumatoriaC))*(float)(Math.Sqrt(SumatoriaD))));

            if(Score != 0)
            {    
                PromedioScoreFaltante += 1 - Score;
                DocumentosPuntuados.Add(Documento, Score);
            }
        } 

        PromedioScoreFaltante = PromedioScoreFaltante/(float)(DocumentosPuntuados.Count);

        DocumentosPuntuados = ActualizarPuntuacionCercania(DocumentosPuntuados, Consulta.PalabrasCercanas, PromedioScoreFaltante, MatrizDocumentos);

        DocumentosPuntuados = CosineSimilarity.OrdenarDiccionario(DocumentosPuntuados);
                
        return DocumentosPuntuados.Keys.ElementAt(0);
    }    

    private static Dictionary<string, float> ActualizarPuntuacionCercania(Dictionary<string, float> DocumentosPuntuados, List<string[]> PalabrasCercanas, float PromedioScoreFaltante, Dictionary<string, Dictionary<string, float>> MatrizDocumentos)
    {
        foreach (string Documento in DocumentosPuntuados.Keys)
        {
            float PromedioCercania = 0;
        
            foreach (string[] ParesCercanos in PalabrasCercanas)
            {
                string Palabra1 = ParesCercanos[0];
                string Palabra2 = ParesCercanos[1];
        
                if(MatrizDocumentos[Documento].ContainsKey(Palabra1) && MatrizDocumentos[Documento].ContainsKey(Palabra2))
                {
                    PromedioCercania += CercaniaPalabras(Documento, Palabra1, Palabra2);
                }
            }

            if(PromedioCercania != 0)
                PromedioCercania = PromedioCercania / (float)(PalabrasCercanas.Count);
                
            DocumentosPuntuados[Documento] += PromedioCercania*PromedioScoreFaltante;
        }

        return DocumentosPuntuados;
    }

    private static float CercaniaPalabras(string Documento, string Palabra1, string Palabra2)
    {

        string[] PalabrasDocumento = ProcessedDocuments.Proceso(Documento);            

        float Posicion1 = -1;
        float Posicion2  = -1;
        float DistanciaTemporal = 0;
        float Distancia = 0;

        for (int j = 0; j < PalabrasDocumento.Length; j++)
        {   
            if(PalabrasDocumento[j] == Palabra1)
            {
                Posicion1 = j+1;
                
                if (Posicion1 != -1 && Posicion2 != -1)
                {
                    DistanciaTemporal = (Posicion1 - Posicion2)/(float)PalabrasDocumento.Length;   
                }
            }
            else if(PalabrasDocumento[j] == Palabra2)
            {
                Posicion2 = PalabrasDocumento.Length - j;
                    
                if (Posicion1 != -1 && Posicion2 != -1)
                {
                    DistanciaTemporal = (Posicion1 + Posicion2)/(float)PalabrasDocumento.Length;
                }                   
            }    
                    
            if(DistanciaTemporal == 1)
            {    Distancia = 1;
                break;
            }
            else if(Distancia < DistanciaTemporal)
            {
                Distancia = DistanciaTemporal;
            }
                
        }
        return Distancia;
       
    }
}