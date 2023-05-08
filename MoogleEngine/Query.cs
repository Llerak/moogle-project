using System.Diagnostics;
namespace MoogleEngine;
public class Query
{
    //Constructor
    public Query(string Consulta, Dictionary<string,float> UniversoDePalabras)
    {
        Stopwatch time = new Stopwatch();
        time.Start();

        this.TF_IDFConsulta = new Dictionary<string,float>();
        this.PalabrasDeseadas = new List<string>();
        this.PalabrasNoDeseadas = new List<string>();
        this.PalabrasCercanas = new List<string[]>();
        this.ProcesamientoConsulta(Consulta,UniversoDePalabras);
        this.Sugerencia = SugerenciaConsulta(Consulta, UniversoDePalabras);
        
        time.Stop();
        Console.WriteLine("Consulta: " + time.Elapsed.TotalMilliseconds / 1000);
    }

    //                                        ATRIBUTOS
    public Dictionary<string,float> TF_IDFConsulta{get; private set;}
    public List<string> PalabrasDeseadas{get; private set;}
    public List<string> PalabrasNoDeseadas{get; private set;}
    public List<string[]> PalabrasCercanas{get; private set;} 
    public string Sugerencia{get; private set;}

    private void ProcesamientoConsulta(string Consulta, Dictionary<string,float> UniversoDePalabras)
    {
        // OBTENER EL VECTOR CONSULTA
        string[] PalabrasConsulta = RevisarConsulta(Consulta);
        this.TF_IDFConsulta = ComputarTF_IDF(PalabrasConsulta, UniversoDePalabras, Consulta);

        //
        string[] ConsultaSinProcesar = Consulta.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        this.PalabrasDeseadas = BorrarDuplicadosLista(OperdoresDeAparicion(ConsultaSinProcesar, true));
        this.PalabrasNoDeseadas = BorrarDuplicadosLista(OperdoresDeAparicion(ConsultaSinProcesar, false));
        this.PalabrasCercanas = PalabrasCercas(Consulta);
        
    }
    
    private static string RevisarPalabra(string palabraConsulta, Dictionary<string,float> UniversoDePalabras)
    {
        float DistanciaTemporal = float.MaxValue;
        string PalabraTemporal = "";
        
        // si la palabra existe entonces devolvermela sin mas
        if(UniversoDePalabras.ContainsKey(palabraConsulta.ToLower()))
            return palabraConsulta.ToLower();

        foreach(string Palabra in UniversoDePalabras.Keys)
        {
            float DistanciaDeLevenshteinTemporal = DistanciaDeLevenshtein(palabraConsulta.ToLower(),Palabra);
                
            if(DistanciaTemporal > DistanciaDeLevenshteinTemporal)
            {
                DistanciaTemporal = DistanciaDeLevenshteinTemporal;
                PalabraTemporal = Palabra;
            }        
        }
        return PalabraTemporal;
    }

    private static float DistanciaDeLevenshtein(string s, string t)
    {
        float porcentaje = 0;
        
        int costo = 0;
        int m = s.Length;
        int n = t.Length;
        int[,] d = new int[m + 1, n + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= m; d[i, 0] = i++) ;
        for (int j = 0; j <= n; d[0, j] = j++) ;
            
        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {       
                costo = (s[i - 1] == t[j - 1]) ? 0 : 1;  
                d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1,  
                                    d[i, j - 1] + 1),                              
                                    d[i - 1, j - 1] + costo);                   
            }
        }

        if (s.Length > t.Length)
        porcentaje = (float)d[m, n] / (float)(s.Length);
        else
        porcentaje = (float)d[m, n] / (float)(t.Length); 
            
        return d[m, n]; 
    }
     
    private static Dictionary<string, float> ComputarTF_IDF(string[] PalabrasConsulta, Dictionary<string, float> UniversoDePalabras,string Consulta)
    {
        Dictionary<string, float> Vector = new Dictionary<string, float>();
        float MayorPeso = 0;
        
        foreach (string PalabraConsulta in PalabrasConsulta)
        {
            if(!UniversoDePalabras.ContainsKey(PalabraConsulta))
                continue;

            if(!Vector.ContainsKey(PalabraConsulta))
                    Vector.Add(PalabraConsulta,0);
            else continue;

            int Contador = 0 ;
            foreach (string PalabraConsulta1 in PalabrasConsulta)
            {
                if (PalabraConsulta == PalabraConsulta1) Contador++;
            }

            float PesoTemporal = (Contador / (float)PalabrasConsulta.Length) * UniversoDePalabras[ PalabraConsulta ];

            Vector[PalabraConsulta] = PesoTemporal;

            if(PesoTemporal > MayorPeso)
                MayorPeso = PesoTemporal;
        }

        
        foreach(string Palabra in Vector.Keys)
        {
            if(PalabrasImportantes(Consulta).ContainsKey(Palabra))
                Vector[Palabra] += (float)(PalabrasImportantes(Consulta)[Palabra]) * MayorPeso; 
        }

        return Vector;
    }

    private static List<string> OperdoresDeAparicion(string[] Consulta, bool Deseada)
    {
        List<string> ListaDePalabras = new List<string>();

        foreach (string Palabra in Consulta)
        {
            string PalabraProcesada = string.Join("",ProcessedDocuments.ObtenerPalabras(Palabra));

            if(string.Join("",ProcessedDocuments.ObtenerPalabras(Palabra)) == "")
                continue;

            if(Palabra[0] == '!' && Deseada == false && !ListaDePalabras.Contains(Palabra))
            {
                ListaDePalabras.Add(Palabra.Substring(1,Palabra.Length-1));
            }
            else if(Palabra[0] == '^' && Deseada == true && !ListaDePalabras.Contains(Palabra))
            {
                ListaDePalabras.Add(Palabra.Substring(1,Palabra.Length-1));
            }            
        }
    
        return ListaDePalabras;
    }
        
    private static List<string[]> PalabrasCercas(string Consulta)
    {
        string[] ParesPalabras = RevisarConsulta(Consulta);
        List<string[]> PalabrasCercanas = new List<string[]>();


        for (int i = 1; i < ParesPalabras.Length - 1; i++)
        {
            string Palabra1 = string.Join("",ProcessedDocuments.ObtenerPalabras(ParesPalabras[i-1])).ToLower();
            string Palabra2 = string.Join("",ProcessedDocuments.ObtenerPalabras(ParesPalabras[i+1]));
            
            if(Palabra1 != "" && Palabra2 != "" && ParesPalabras[i] == "~")
            {
                string[] ParejaPalabras = {ParesPalabras[i-1], ParesPalabras[i+1]};
                PalabrasCercanas.Add(ParejaPalabras);
            }
        }

        return PalabrasCercanasRevisadas(PalabrasCercanas);
    
    }

    private static List<string[]> PalabrasCercanasRevisadas(List<string[]> PalabrasCercanas)
    {
        for (int i = 0; i < PalabrasCercanas.Count; i++)
        {
            string Palabra1 = PalabrasCercanas[i][0];
            string Palabra2 = PalabrasCercanas[i][1];

            if(Palabra1 == Palabra2)
            {
                PalabrasCercanas.RemoveAt(i);
                i--;
                continue;
            }
            

            if(PalabrasCercanas.Count == 1)
                break;
            
            for (int k = i+1; k < PalabrasCercanas.Count; k++)
            {
                string Palabra21 = PalabrasCercanas[k][0];
                string Palabra22 = PalabrasCercanas[k][1];

                if(Palabra1 == Palabra21 && Palabra2 == Palabra22)
                    PalabrasCercanas.RemoveAt(k);
                
                if(Palabra1 == Palabra22 && Palabra2 == Palabra21)
                    PalabrasCercanas.RemoveAt(k);
            }
        }
    
        return PalabrasCercanas;
    }

    private static Dictionary<string, int> PalabrasImportantes(string Consulta)
    {
        string[] ConsultaArray = Consulta.Split(' ',StringSplitOptions.RemoveEmptyEntries);

        Dictionary<string, int> PalabraImportancia = new Dictionary<string, int>();

        foreach(string Palabra in ConsultaArray)
        {
            string termino = string.Join("",ProcessedDocuments.ObtenerPalabras(Palabra));
            int Contador = 0;

            for (int i = 0; i < Palabra.Length; i++)
            {    
                if(Palabra[i] == '*' && termino != "")
                    Contador++;     
                else if(Palabra[i] != '*' && termino != "" && Contador != 0)
                {
                    string PalabraImportante = Palabra.Substring(i, Palabra.Length-i);
                        
                    if(!PalabraImportancia.ContainsKey(PalabraImportante))
                        PalabraImportancia.Add(PalabraImportante, Contador);
                    else
                        PalabraImportancia[PalabraImportante] += Contador;
                    
                    break;
                }    
            }
        }
    return PalabraImportancia;
    }
  
    private static string[] RevisarConsulta(string Consulta)
    {
        string[] ConsultaProcesada = Consulta.ToLower().Split(' ',StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < ConsultaProcesada.Length; i++)
        {
            string Palabra = ConsultaProcesada[i];
            if(Palabra[0] =='!')
            {
                ConsultaProcesada[i] = Palabra.Substring(1,Palabra.Length-1);
            }
            else if(Palabra[0] =='^')
            {
                ConsultaProcesada[i] = Palabra.Substring(1,Palabra.Length-1);
            }
            else if(Palabra[0] =='*')
            {
                for (int k = 0; k < Palabra.Length; k++)
                {
                    if(Palabra[k] != '*')
                    {
                        ConsultaProcesada[i] = Palabra.Substring(k, Palabra.Length - k );
                        break;
                    }
                }
            }
        }

        return ConsultaProcesada;
    }

    private static List<string> BorrarDuplicadosLista(List<string> ListaPalabras)
    {
        return new HashSet<string>(ListaPalabras).ToList();
    }

    private static string SugerenciaConsulta(string Consulta, Dictionary<string, float> UniversoPalabras)
    {
        string Sugerencia = "";
        string[] SugerenciaArray = Consulta.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < SugerenciaArray.Length; i++)
        {
            string Palabra = SugerenciaArray[i];    
            string PalabraSinSignos = string.Join("",ProcessedDocuments.Proceso(Palabra));
            
            if(PalabraSinSignos == "" && Palabra != "~")
                continue;
        
            if(Palabra[0] == '!')
            {
                Palabra = "!" + RevisarPalabra(Palabra,UniversoPalabras);
                Sugerencia += Palabra + " ";
            }
            else if(Palabra[0] == '^')
            {
                Palabra = "^" + RevisarPalabra(Palabra,UniversoPalabras);
                Sugerencia += Palabra + " ";
            }
            else if(Palabra[0] == '*')
            {
                string OperadorImportancia = "";
                
                for (int j = 0; j < Palabra.Length; j++)
                {
                    if(Palabra[j] == '*')
                    {
                        OperadorImportancia += "*";
                    }
                    else
                    {
                        Palabra = OperadorImportancia + RevisarPalabra(Palabra.Substring(j, Palabra.Length-j), UniversoPalabras);
                        break;
                    }
                }
                Sugerencia += Palabra + " ";
            }
            else if(Palabra == "~" && i != 0 && i != SugerenciaArray.Length - 1)
            {
                Sugerencia += Palabra + " ";
            }
            else if(Palabra != "~")
            {
                Palabra = RevisarPalabra(Palabra, UniversoPalabras);
                Sugerencia += Palabra + " ";
            }
        
        if(i == SugerenciaArray.Length-1)
        {
            Sugerencia = Sugerencia.Substring(0, Sugerencia.Length-1);
        }
        
        
        }       

        return Sugerencia;
    }
}