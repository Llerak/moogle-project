namespace MoogleEngine;
using System.Diagnostics;

public class CosineSimilarity
{
    public CosineSimilarity(Query Consulta, Dictionary<string, float> Universo, Dictionary<string, Dictionary<string, float>> MatrizDocumento)
    {
        Stopwatch time = new Stopwatch();
        time.Start();

        this.DocumentosPuntuados = new Dictionary<string, float>();
        this.ProcesarConsultaDocumento(Consulta, Universo, MatrizDocumento);

        time.Stop();
        Console.WriteLine("Calculo de puntuacion: " + time.Elapsed.TotalMilliseconds / 1000);

    }

    public Dictionary<string, float> DocumentosPuntuados {get; private set;}

    private void ProcesarConsultaDocumento(Query Consulta, Dictionary<string, float> Universo, Dictionary<string, Dictionary<string, float>> MatrizDocumento)
    {

        this.DocumentosPuntuados = PuntuacionDocumentos(Consulta, Universo, MatrizDocumento);
    }

    private static Dictionary<string, float> PuntuacionDocumentos(Query Consulta,Dictionary<string, float> UniversoPalabras, Dictionary<string, Dictionary<string, float>> MatrizDocumentos)
    {
        Dictionary<string, float> DocumentosPuntuados = new Dictionary<string, float>();
        
        Dictionary<string, float> ConsultaTFIDF = Consulta.TF_IDFConsulta;
        
        if(ConsultaTFIDF.Count == 0)
            return DocumentosPuntuados;

        float SumatoriaC = SumatoriaDiccionarioCuadrado(ConsultaTFIDF);
        float PromedioScoreFaltante = 0;
    
       foreach (string Documento in MatrizDocumentos.Keys)
       {
            float Score = 0;
            
            float SumatoriaCD = 0;

            bool PaseDeseada = ComprobarPaseDeseada(MatrizDocumentos[Documento], Consulta.PalabrasDeseadas);
            bool PaseNoDeseada = ComprobarPaseNoDeseada(MatrizDocumentos[Documento], Consulta.PalabrasNoDeseadas);
            
            if(PaseDeseada == false || PaseNoDeseada == false)
                continue;
        
            foreach (string Palabra in ConsultaTFIDF.Keys)
            {
                if(MatrizDocumentos[Documento].ContainsKey(Palabra))
                    SumatoriaCD += MatrizDocumentos[Documento][Palabra] * ConsultaTFIDF[Palabra];
            }
       
            if(SumatoriaCD == 0)
                continue;  
            
            float SumatoriaD = SumatoriaDiccionarioCuadrado(MatrizDocumentos[Documento]);
       
            Score = (SumatoriaCD/((float)(Math.Sqrt(SumatoriaC))*(float)(Math.Sqrt(SumatoriaD))));

            if(Score != 0)
            {
                PromedioScoreFaltante += 1 - Score;
                DocumentosPuntuados.Add(Documento, Score);
            }
        } 

        PromedioScoreFaltante = PromedioScoreFaltante/(float)(DocumentosPuntuados.Count);
        
        DocumentosPuntuados = ActualizarPuntuacionCercania(DocumentosPuntuados, Consulta.PalabrasCercanas, PromedioScoreFaltante, MatrizDocumentos);        

        DocumentosPuntuados = OrdenarDiccionario(DocumentosPuntuados);
        
        return DocumentosPuntuados;
    } 

    public static float SumatoriaDiccionarioCuadrado(Dictionary<string, float> Diccionario)
    {
        float Sumatoria = 0;
        
        foreach (float Valor in Diccionario.Values)
        {
            Sumatoria += (float)(Math.Pow(Valor,2));
        }

        return Sumatoria;
    }
    private static bool ComprobarPaseNoDeseada(Dictionary<string, float> Documento, List<string> PalabrasNoDeseadas)
    {
        bool Pase = true;

        foreach (string Palabra in PalabrasNoDeseadas)
        {
            if(Documento.ContainsKey(Palabra))
            {    Pase = false;
                break;    
            }
        }
        
        return Pase;
    }

    private static bool ComprobarPaseDeseada(Dictionary<string, float> Documento, List<string> PalabrasDeseadas)
    {
        bool Pase = true;
        int Contador = PalabrasDeseadas.Count; 
    
        foreach (string Palabra in PalabrasDeseadas)
        {
            if(Documento.ContainsKey(Palabra))
            {
                Contador--;
            }
        }
        
        if(Contador != 0)
            Pase = false;
        
        return Pase;
    }

    //Calcular la cercania de palabras en un documento
    private static float CercaniaPalabras(string Documento, string Palabra1, string Palabra2)
    {

        string[] PalabrasDocumento = ProcessedDocuments.ObtenerPalabras(Documento, true);            

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

    public static Dictionary<string, float> OrdenarDiccionario(Dictionary<string, float> DocumentosDesordenados)
    {
        Dictionary<string, float> DocumentosOrdenados = new Dictionary<string, float>();
        int Contador = 0;
        foreach (string Documento in DocumentosDesordenados.Keys)
        {
            float Score = DocumentosDesordenados[Documento];
            string DocumentoTitulo = Documento;
            
            foreach (string Documento1 in DocumentosDesordenados.Keys)
            {
                
                if(Score < DocumentosDesordenados[Documento1])
                {
                    Score = DocumentosDesordenados[Documento1];
                    DocumentoTitulo = Documento1;
                }    
            }
            
            DocumentosOrdenados.Add(DocumentoTitulo, Score);
            DocumentosDesordenados.Remove(DocumentoTitulo);
            Contador++;
            if(Contador == 9)
                break;
        }
        
        return DocumentosOrdenados;
    }
}